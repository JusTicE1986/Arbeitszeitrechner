using Arbeitszeitrechner;
using Arbeitszeitrechner.Model;
using Arbeitszeitrechner.ViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using System.Globalization;

public partial class ArbeitszeitTag : ObservableObject
{
    #region Eigenschaften

    public Arbeitszeitmodell Arbeitszeitmodell { get; set; }
    public DateTime Datum { get; set; }

    private TimeSpan _startZeit;
    public TimeSpan StartZeit
    {
        get => _startZeit;
        set
        {
            if (_startZeit == value) return; // ➤ Schutz vor doppeltem Aufruf
            if (SetProperty(ref _startZeit, value))
            {
                if (!_endZeitManuellGesetzt)
                {
                    EndZeit = _startZeit + BerechneStandardArbeitszeit();
                }

                BerechneArbeitszeiten();

                // Nur wenn wirklich eine Differenzzeit vorliegt
                if (Differenzzeit != TimeSpan.Zero)
                {
                    Debug.WriteLine($"🟢 Startzeit geändert: {StartZeit}");
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        (App.Current.MainWindow.DataContext as ArbeitszeitViewModel)?.VerteileRestzeitAufWochentage();
                    });
                }
            }
        }
    }

    private TimeSpan _endZeit;
    public TimeSpan EndZeit
    {
        get => _endZeit;
        set
        {
            if (_endZeit == value) return; // ➤ Schutz vor doppeltem Aufruf
            if (SetProperty(ref _endZeit, value))
            {
                _endZeitManuellGesetzt = true;
                BerechneArbeitszeiten();

                // Nur bei echter Differenzzeit Verteilung starten
                if (Differenzzeit != TimeSpan.Zero)
                {
                    Debug.WriteLine($"🟢 Endzeit geändert: {EndZeit}");
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        (App.Current.MainWindow.DataContext as ArbeitszeitViewModel)?.VerteileRestzeitAufWochentage();
                    });
                }
            }
        }
    }

    public TimeSpan Differenzzeit { get; set; } = TimeSpan.Zero;
    public TimeSpan GeplanteArbeitszeit => BerechneStandardArbeitszeit();

    private bool _endZeitManuellGesetzt = false;

    [ObservableProperty]
    private TimeSpan _gesamtArbeitsZeit;
    [ObservableProperty]
    private TimeSpan _pause;
    [ObservableProperty]
    private TimeSpan _tatsaechlicheArbeitszeit;

    public int GeaenderteKalenderwoche { get; set; } = -1;
    public bool IsFeiertag { get; set; }
    public string FeiertagsName { get; set; } = string.Empty;
    public bool IstWochenende => Datum.DayOfWeek == DayOfWeek.Saturday || Datum.DayOfWeek == DayOfWeek.Sunday;
    public bool WurdeVerteilt { get; set; } = false;  // ➤ Neues Flag zur Verteilungssteuerung


    #endregion
    #region Methoden
    public void SetzeEndzeitManuell(TimeSpan neueEndzeit)
    {
        if (_endZeit == neueEndzeit) return;  // ➤ Schutz vor mehrfacher Ausführung
        _endZeitManuellGesetzt = true;
        EndZeit = neueEndzeit;
    }

    private void BerechneArbeitszeiten()
    {

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

        // ➤ Korrigierte Differenzzeitberechnung
        Differenzzeit = TatsaechlicheArbeitszeit - (GeplanteArbeitszeit - Pause);
        
    }

    private TimeSpan BerechneStandardArbeitszeit()
    {
        return Arbeitszeitmodell switch
        {
            Arbeitszeitmodell.VierzigStunden => new TimeSpan(8, 30, 0),
            Arbeitszeitmodell.VerkürzteVollzeit => new TimeSpan(7, 30, 0),
            _ => new TimeSpan(8, 6, 0)
        };
    }
    #endregion

    #region Konstruktor
    public ArbeitszeitTag(DateTime datum, Arbeitszeitmodell arbeitszeitmodell)
    {
        Datum = datum;
        Arbeitszeitmodell = arbeitszeitmodell;

        StartZeit = IstWochenende || IsFeiertag
            ? TimeSpan.Zero
            : new TimeSpan(8, 0, 0);

        EndZeit = IstWochenende || IsFeiertag
            ? TimeSpan.Zero
            : StartZeit + BerechneStandardArbeitszeit();

        BerechneArbeitszeiten();  // Initialberechnung der Arbeitszeiten
    }
    #endregion
}
