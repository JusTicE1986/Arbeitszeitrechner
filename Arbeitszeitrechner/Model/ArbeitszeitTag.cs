using Arbeitszeitrechner;
using Arbeitszeitrechner.Model;
using Arbeitszeitrechner.Services;
using Arbeitszeitrechner.ViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Diagnostics;

public partial class ArbeitszeitTag : ObservableObject
{
    #region Eigenschaften
    public Arbeitszeitmodell Arbeitszeitmodell { get; set; }
    public DateTime Datum { get; set; }
    private readonly ArbeitszeitBerechnungsService _berechnungsService = new ArbeitszeitBerechnungsService();

    [ObservableProperty]
    private TimeSpan _startZeit;
    partial void OnStartZeitChanged(TimeSpan oldValue, TimeSpan newValue)
    {
        BerechneArbeitszeiten();
        App.Current.Dispatcher.Invoke(() =>
        {
            (App.Current.MainWindow.DataContext as ArbeitszeitViewModel)
                ?.VerteileRestzeitAutomatisch();
        });

    }

    [ObservableProperty]
    private TimeSpan _endZeit;
    partial void OnEndZeitChanged(TimeSpan oldValue, TimeSpan newValue)
    {
        BerechneArbeitszeiten();
        App.Current.Dispatcher.Invoke(() =>
        {
            (App.Current.MainWindow.DataContext as ArbeitszeitViewModel)
                ?.VerteileRestzeitAutomatisch();
        });

    }
    [ObservableProperty]
    private TimeSpan _gesamtArbeitsZeit;

    [ObservableProperty]
    private TimeSpan _pause;

    [ObservableProperty]
    private TimeSpan _tatsaechlicheArbeitszeit;
    private bool _berechnungBereitsDurchgeführt;

    public TimeSpan Differenzzeit { get; private set; } = TimeSpan.Zero;
    public TimeSpan GeplanteArbeitszeit => _berechnungsService.BestimmeStandardArbeitszeit(Arbeitszeitmodell);

    public bool IsFeiertag { get; set; }
    public string FeiertagsName { get; set; } = string.Empty;
    public bool IstWochenende => Datum.DayOfWeek == DayOfWeek.Saturday || Datum.DayOfWeek == DayOfWeek.Sunday;
    public bool WurdeVerteilt { get; set; } = false;

    #endregion

    #region Methoden
    public void SetzeEndzeitManuell(TimeSpan neueEndzeit)
    {
        if (_endZeit == neueEndzeit) return;
        _endZeit = neueEndzeit;
        BerechneArbeitszeiten();
    }

    public void BerechneArbeitszeiten()
    {
        if (_berechnungBereitsDurchgeführt) return;
        _berechnungBereitsDurchgeführt = true;

        GesamtArbeitsZeit = EndZeit > StartZeit ? EndZeit - StartZeit : TimeSpan.Zero;

        if (GesamtArbeitsZeit <= TimeSpan.FromHours(6))
        {
            Pause = TimeSpan.Zero;
        }
        else if (GesamtArbeitsZeit <= TimeSpan.FromHours(6.5))
        {
            Pause = GesamtArbeitsZeit - TimeSpan.FromHours(6);
        }
        else if (GesamtArbeitsZeit <= TimeSpan.FromHours(9.25))
        {
            Pause = TimeSpan.FromMinutes(30);
        }
        else
        {
            Pause = TimeSpan.FromMinutes(45);
        }

        TatsaechlicheArbeitszeit = GesamtArbeitsZeit - Pause;

        Debug.WriteLine($"🟠 BerechneArbeitszeiten() → Tatsächliche Arbeitszeit: {TatsaechlicheArbeitszeit}");
        Debug.WriteLine($"🟠 BerechneArbeitszeiten() → Differenzzeit: {Differenzzeit}");
        _berechnungBereitsDurchgeführt = false;
        OnPropertyChanged(nameof(EndZeit));
    }

    #endregion

    #region Konstruktor
    public ArbeitszeitTag(DateTime datum, Arbeitszeitmodell arbeitszeitmodell)
    {
        Datum = datum;
        Arbeitszeitmodell = arbeitszeitmodell;

        // Korrekte Initialisierung sicherstellen
        if (!IstWochenende && !IsFeiertag)
        {
            StartZeit = new TimeSpan(8, 0, 0);   // Standard-Startzeit
            EndZeit = StartZeit + _berechnungsService.BestimmeStandardArbeitszeit(Arbeitszeitmodell.Regelarbeitszeit);
        }
        else
        {
            StartZeit = TimeSpan.Zero;
            EndZeit = TimeSpan.Zero;
        }

        BerechneArbeitszeiten();
    }
    #endregion
}
