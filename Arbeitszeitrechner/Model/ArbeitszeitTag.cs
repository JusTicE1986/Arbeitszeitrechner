using Arbeitszeitrechner;
using Arbeitszeitrechner.Model;
using Arbeitszeitrechner.ViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Diagnostics;
using System.Globalization;

public partial class ArbeitszeitTag : ObservableObject
{
    public Arbeitszeitmodell Arbeitszeitmodell { get; set; }
    public DateTime Datum { get; set; }

    private TimeSpan _startZeit;
    public TimeSpan StartZeit
    {
        get => _startZeit;
        set
        {
            if (SetProperty(ref _startZeit, value))  // ➤ SetProperty sorgt für PropertyChanged
            {
                if (!_endZeitManuellGesetzt)
                {
                    EndZeit = _startZeit + BerechneStandardArbeitszeit();
                }

                BerechneArbeitszeiten();

                App.Current.Dispatcher.Invoke(() =>
                {
                    (App.Current.MainWindow.DataContext as ArbeitszeitViewModel)?.VerteileRestzeitAufWochentage();
                });
                Debug.WriteLine($"Startzeit geändert auf {StartZeit} um {DateTime.Now:HH:mm:ss.fff}");

                GeaenderteKalenderwoche = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                Datum,
                CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday);
            }
        }
    }

    public TimeSpan Differenzzeit { get; set; } = TimeSpan.Zero;

    public TimeSpan GeplanteArbeitszeit => BerechneStandardArbeitszeit();

    private bool _endZeitManuellGesetzt = false;

    private TimeSpan _endZeit;
    public TimeSpan EndZeit
    {
        get => _endZeit;
        set
        {
            if (SetProperty(ref _endZeit, value))  // ➤ SetProperty sorgt für PropertyChanged
            {
                _endZeitManuellGesetzt = true;
                BerechneArbeitszeiten();

                App.Current.Dispatcher.Invoke(() =>
                {
                    (App.Current.MainWindow.DataContext as ArbeitszeitViewModel)?.VerteileRestzeitAufWochentage();
                });
                Debug.WriteLine($"Startzeit geändert auf {StartZeit} um {DateTime.Now:HH:mm:ss.fff}");

                GeaenderteKalenderwoche = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                Datum,
                CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday);
            }
        }
    }

    public void SetzeEndzeitManuell(TimeSpan neueEndzeit)
    {
        _endZeitManuellGesetzt = true;
        EndZeit = neueEndzeit;
    }

    [ObservableProperty]
    private TimeSpan _gesamtArbeitsZeit;

    [ObservableProperty]
    private TimeSpan _pause;

    [ObservableProperty]
    private TimeSpan _tatsaechlicheArbeitszeit;

    public int GeaenderteKalenderwoche { get; set; } = -1;


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
        else if (GesamtArbeitsZeit <= TimeSpan.FromHours(9))
        {
            Pause = TimeSpan.FromMinutes(30);
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

    }

    public bool IsFeiertag { get; set; }
    public string FeiertagsName { get; set; } = string.Empty;

    public bool IstWochenende => Datum.DayOfWeek == DayOfWeek.Saturday || Datum.DayOfWeek == DayOfWeek.Sunday;

    public ArbeitszeitTag(DateTime datum, Arbeitszeitmodell arbeitszeitmodell)
    {
        Datum = datum;
        Arbeitszeitmodell = arbeitszeitmodell;

        StartZeit = IstWochenende || IsFeiertag
            ? TimeSpan.Zero
            : new TimeSpan(8, 0, 0);

        EndZeit = IstWochenende || IsFeiertag ? TimeSpan.Zero : StartZeit + BerechneStandardArbeitszeit();
        BerechneArbeitszeiten();  // Initialberechnung der Arbeitszeiten
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
}
