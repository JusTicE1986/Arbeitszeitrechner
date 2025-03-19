using Arbeitszeitrechner.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arbeitszeitrechner.Services
{

    public interface IWochenService
    {
        ObservableCollection<ArbeitszeitWoche> GeneriereWochenweiseEintraege(int jahr, Dictionary<DateTime, string> feiertage);
        void VerteileRestzeitAufWochentage(ObservableCollection<ArbeitszeitWoche> arbeitswochen);
        void DynamischeRestzeitVerteilung(ArbeitszeitWoche woche);
    }
    public class WochenService : IWochenService
    {
        private bool _wochenGeneriert = false; 

        public void DynamischeRestzeitVerteilung(ArbeitszeitWoche woche)
        {
            // Identifiziere den Tag mit der Differenzzeit
            var tagMitAbweichung = woche.Arbeitstage.FirstOrDefault(tag => tag.Differenzzeit != TimeSpan.Zero);

            if (tagMitAbweichung == null) return; // ➤ Nichts zu verteilen

            // Ermittele verbleibende Arbeitstage (ausgenommen Wochenende und Feiertage)
            var verbleibendeArbeitstage = woche.Arbeitstage
                .Where(tag => tag.Datum > tagMitAbweichung.Datum && !tag.IstWochenende && !tag.IsFeiertag)
                .ToList();

            if (!verbleibendeArbeitstage.Any()) return;

            // ➤ Gleichmäßige Verteilung der Differenzzeit
            TimeSpan aufteilung = TimeSpan.FromMinutes(tagMitAbweichung.Differenzzeit.TotalMinutes / verbleibendeArbeitstage.Count);

            foreach (var folgetag in verbleibendeArbeitstage)
            {
                folgetag.SetzeEndzeitManuell(folgetag.EndZeit - aufteilung);
                folgetag.WurdeVerteilt = true;

                folgetag.BerechneArbeitszeiten();
            }

            // Nach Verteilung zurücksetzen
            
            tagMitAbweichung.WurdeVerteilt = true;
            tagMitAbweichung.BerechneArbeitszeiten();
        }

        public ObservableCollection<ArbeitszeitWoche> GeneriereWochenweiseEintraege(int jahr, Dictionary<DateTime, string> feiertage)
        {
            if (_wochenGeneriert) return new ObservableCollection<ArbeitszeitWoche>();  // ➤ Doppelte Initialisierung verhindern
            _wochenGeneriert = true;
            var alleTage = Enumerable.Range(0, DateTime.IsLeapYear(jahr) ? 366 : 365)
                .Select(i => new DateTime(jahr, 1, 1).AddDays(i))
                .Select(datum => new ArbeitszeitTag(datum, Arbeitszeitmodell.Regelarbeitszeit)
                {
                    IsFeiertag = feiertage.ContainsKey(datum),
                    FeiertagsName = feiertage.ContainsKey(datum) ? feiertage[datum] : string.Empty
                })
                .ToList();

            return new ObservableCollection<ArbeitszeitWoche>(
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

        public void VerteileRestzeitAufWochentage(ObservableCollection<ArbeitszeitWoche> arbeitswochen)
        {
            foreach (var woche in arbeitswochen)
            {
                var wocheMitAenderungen = woche.Arbeitstage
                    .FirstOrDefault(tag => tag.Differenzzeit != TimeSpan.Zero && !tag.WurdeVerteilt);
                if(wocheMitAenderungen == null)
                {
                    continue;
                }

                for (int i = 0; i < woche.Arbeitstage.Count; i++)
                {
                    var tag = woche.Arbeitstage[i];
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
                        folgetag.WurdeVerteilt = true;
                        folgetag.BerechneArbeitszeiten();
                    }

                    tag.WurdeVerteilt = true;
                    tag.BerechneArbeitszeiten();
                }
            }        }
    }
}
