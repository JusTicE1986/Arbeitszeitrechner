using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arbeitszeitrechner.Model;

namespace Arbeitszeitrechner.ViewModel
{
    public class ArbeitszeitViewModel
    {
        public bool _verteilungAktiv = false;
        public ObservableCollection<ArbeitszeitWoche> Arbeitswochen { get; set; }
        //public ObservableCollection<ArbeitszeitTag> Arbeitszeiten { get; set; }

        public ArbeitszeitViewModel()
        {
            GeneriereWochenweiseEintraege();
            VerteileRestzeitAufWochentage();
            Debug.WriteLine($"[Konstruktor] ArbeitszeitViewModel erstellt. Anzahl Wochen: {Arbeitswochen?.Count ?? 0}");

        }

        private void GeneriereWochenweiseEintraege()
        {
            int jahr = DateTime.Now.Year;
            var feiertage = FeiertagsBerechnung.GetFeiertage(jahr);

            var alleTage = Enumerable.Range(0, DateTime.IsLeapYear(jahr) ? 366 : 365)
                .Select(i => new DateTime(jahr, 1, 1).AddDays(i))
                .Select(datum => new ArbeitszeitTag(datum, Arbeitszeitmodell.Regelarbeitszeit)
                {
                    //Datum = datum,
                    //Arbeitszeitmodell = Arbeitszeitmodell.Regelarbeitszeit,
                    //StartZeit = (datum.DayOfWeek == DayOfWeek.Saturday || datum.DayOfWeek == DayOfWeek.Sunday) ? TimeSpan.Zero : new TimeSpan(6, 0, 0),
                    //EndZeit = (datum.DayOfWeek == DayOfWeek.Saturday || datum.DayOfWeek == DayOfWeek.Sunday)? TimeSpan.Zero : new TimeSpan(14,30,0),

                    IsFeiertag = feiertage.ContainsKey(datum),
                    FeiertagsName = feiertage.ContainsKey(datum) ? feiertage[datum] : string.Empty

                })

                .ToList();

            Arbeitswochen = new ObservableCollection<ArbeitszeitWoche>
                (
                alleTage
                .GroupBy(tag => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                    tag.Datum,
                    CalendarWeekRule.FirstFourDayWeek,
                    DayOfWeek.Monday))
                .Select(gruppe => new ArbeitszeitWoche
                {
                    Kalenderwoche = gruppe.Key,
                    Arbeitstage = gruppe.ToList()
                })
                );
        }

        public void VerteileRestzeitAufWochentage()
        {
            Debug.WriteLine($"✅ VerteileRestzeitAufWochentage() wurde aufgerufen. Anzahl Wochen: {Arbeitswochen?.Count ?? 0}");
            if (_verteilungAktiv)
                Debug.WriteLine("⛔ VerteileRestzeitAufWochentage() wurde blockiert (Schutz-Flag aktiv)");
            return;  // ➤ Schutz vor Endlosschleife
            _verteilungAktiv = true;

            foreach (var woche in Arbeitswochen)
            {
                var wocheMitAenderungen = woche.Arbeitstage
                    .FirstOrDefault(tag => tag.GeaenderteKalenderwoche == woche.Kalenderwoche);

                if (wocheMitAenderungen == null)
                    continue; // ➤ Keine geänderten Zeiten in dieser Woche → Keine Verteilung nötig

                for (int i = 0; i < woche.Arbeitstage.Count; i++)
                {
                    var tag = woche.Arbeitstage[i];
                    Debug.WriteLine($"Tag: {tag.Datum.ToShortDateString()} | Differenzzeit: {tag.Differenzzeit}");
                    

                    // ➤ Differenzzeit berechnen (Tatsächliche Arbeitszeit - Geplante Arbeitszeit)
                    tag.Differenzzeit = tag.TatsaechlicheArbeitszeit - (tag.GeplanteArbeitszeit - tag.Pause);

                    if (tag.Differenzzeit == TimeSpan.Zero)
                        continue; // ➤ Keine Abweichung = Keine Verteilung erforderlich

                    // ➤ Nachfolgende Arbeitstage der selben Woche inklusive Freitag ermitteln
                    var restlicheArbeitstage = woche.Arbeitstage
                        .Skip(i + 1)
                        .ToList(); // ➤ Freitag wird hier mit einbezogen

                    if (!restlicheArbeitstage.Any())
                        continue; // ➤ Keine verbleibenden Arbeitstage mehr

                    // ➤ Differenzzeit gleichmäßig aufteilen
                    TimeSpan aufteilung = TimeSpan.FromMinutes(tag.Differenzzeit.TotalMinutes / restlicheArbeitstage.Count);

                    // ➤ Verteilung auf nachfolgende Tage
                    foreach (var folgetag in restlicheArbeitstage)
                    {
                        folgetag.SetzeEndzeitManuell(folgetag.EndZeit + aufteilung);
                    }

                    // ➤ Differenzzeit wird auf 0 gesetzt, da sie verteilt wurde
                    tag.Differenzzeit = TimeSpan.Zero;
                }
            }
        }
    }
}
