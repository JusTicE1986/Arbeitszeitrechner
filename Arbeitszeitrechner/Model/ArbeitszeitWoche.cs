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


        public TimeSpan WochenArbeitsZeit => TimeSpan.FromTicks(
            Arbeitstage
            .Where(tag => tag.StartZeit != TimeSpan.Zero && tag.EndZeit != TimeSpan.Zero)
            .Sum(tag => tag.TatsaechlicheArbeitszeit.Ticks));
    }
}
