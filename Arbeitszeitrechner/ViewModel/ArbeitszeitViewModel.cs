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
        #region Eigenschaften
        public bool _verteilungAktiv = false;
        public ObservableCollection<ArbeitszeitWoche> Arbeitswochen { get; set; }
        #endregion

        #region Methoden
        private void GeneriereWochenweiseEintraege()
        {
            int jahr = DateTime.Now.Year;
            var feiertage = FeiertagsBerechnung.GetFeiertage(jahr);

            var alleTage = Enumerable.Range(0, DateTime.IsLeapYear(jahr) ? 366 : 365)
                .Select(i => new DateTime(jahr, 1, 1).AddDays(i))
                .Select(datum => new ArbeitszeitTag(datum, Arbeitszeitmodell.Regelarbeitszeit)
                {
                    IsFeiertag = feiertage.ContainsKey(datum),
                    FeiertagsName = feiertage.ContainsKey(datum) ? feiertage[datum] : string.Empty
                })
                .ToList();

            Arbeitswochen = new ObservableCollection<ArbeitszeitWoche>(
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
            if (_verteilungAktiv)
            {
                return;
            }

            _verteilungAktiv = true;

            foreach (var woche in Arbeitswochen)
            {
                var wocheMitAenderungen = woche.Arbeitstage
                    .FirstOrDefault(tag => tag.Differenzzeit != TimeSpan.Zero && !tag.WurdeVerteilt);

                if (wocheMitAenderungen == null)
                {
                    continue;
                }

                for (int i = 0; i < woche.Arbeitstage.Count; i++)
                {
                    var tag = woche.Arbeitstage[i];

                    // Neue Debug-Ausgabe zur Überprüfung der Differenzzeit

                    if (tag.Differenzzeit == TimeSpan.Zero || tag.WurdeVerteilt)
                    {
                        continue;
                    }

                    var restlicheArbeitstage = woche.Arbeitstage
                        .Skip(i + 1)
                        .Where(t => !t.IstWochenende && !t.WurdeVerteilt)
                        .ToList();

                    if (!restlicheArbeitstage.Any())
                    {
                        continue;
                    }

                    TimeSpan aufteilung = TimeSpan.FromMinutes(tag.Differenzzeit.TotalMinutes / restlicheArbeitstage.Count);


                    foreach (var folgetag in restlicheArbeitstage)
                    {
                        folgetag.SetzeEndzeitManuell(folgetag.EndZeit - aufteilung);
                        folgetag.WurdeVerteilt = true; // ➤ Markiere Tag als verteilt
                    }

                    tag.Differenzzeit = TimeSpan.Zero;  // ➤ Nach erfolgreicher Verteilung auf Null setzen
                    tag.WurdeVerteilt = true;           // ➤ Markiere auch diesen Tag als verteilt
                }
            }

            _verteilungAktiv = false;
        }


        #endregion

        #region Konstruktor
        public ArbeitszeitViewModel()
        {
            GeneriereWochenweiseEintraege();
        }
        #endregion
    }
}
