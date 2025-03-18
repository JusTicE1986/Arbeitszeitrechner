using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arbeitszeitrechner.Model
{
    public class ArbeitszeitWoche
    {
        public int Kalenderwoche { get; set; }
        public List<ArbeitszeitTag> Arbeitstage { get; set; } = new List<ArbeitszeitTag>();
        public string WochenArbeitsZeitAnzeige => $"{(int)WochenArbeitsZeit.TotalHours}:{WochenArbeitsZeit.Minutes:D2}";

        public TimeSpan TatsaechlicheWochenArbeitszeit => TimeSpan.FromTicks(
            Arbeitstage.Sum(tag => tag.TatsaechlicheArbeitszeit.Ticks));
        public string TatsaechlicheWochenArbeitszeitAnzeige =>
    $"{(int)TatsaechlicheWochenArbeitszeit.TotalHours}:{TatsaechlicheWochenArbeitszeit.Minutes:D2}";

        public TimeSpan Differenz => TatsaechlicheWochenArbeitszeit - WochenArbeitsZeit;
        public string DifferenzAnzeige =>
    $"{(int)Differenz.TotalHours}:{Differenz.Minutes:D2}";

        public bool IstImSoll => Differenz >= TimeSpan.Zero;


        //public bool IstGeoeffnet { get; set; } = false;
        public bool IstAktuelleWoche => Arbeitstage.Any(tag => tag.Datum.Date == DateTime.Today.Date);
        public bool EnthaeltFeiertag => Arbeitstage.Any(tag => tag.IsFeiertag);

        public TimeSpan WochenArbeitsZeit => TimeSpan.FromTicks(
            Arbeitstage
            .Where(tag => tag.StartZeit != TimeSpan.Zero && tag.EndZeit != TimeSpan.Zero)
            .Sum(tag => tag.TatsaechlicheArbeitszeit.Ticks));
    }
}
