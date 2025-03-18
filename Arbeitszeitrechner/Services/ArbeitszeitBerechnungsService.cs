using Arbeitszeitrechner.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arbeitszeitrechner.Services
{
    public class ArbeitszeitBerechnungsService
    {
        public TimeSpan BerechneGesamtArbeitszeit(TimeSpan startZeit, TimeSpan endZeit)
        {
            return endZeit > startZeit ? endZeit - startZeit : TimeSpan.Zero;
        }

        public TimeSpan BerechnePause(TimeSpan gesamtArbeitszeit)
        {
            if (gesamtArbeitszeit <= TimeSpan.FromHours(6))
                return TimeSpan.Zero;
            else if (gesamtArbeitszeit <= TimeSpan.FromHours(6.5))
                return gesamtArbeitszeit - TimeSpan.FromHours(6);
            else if (gesamtArbeitszeit <= TimeSpan.FromHours(9.25))
                return TimeSpan.FromMinutes(30);
            else
                return TimeSpan.FromMinutes(45);
        }

        public TimeSpan BerechneDifferenzzeit(TimeSpan tatsArbeitszeit, TimeSpan geplanteArbeitszeit, TimeSpan pause)
        {
            return tatsArbeitszeit - (geplanteArbeitszeit - pause);
        }

        public TimeSpan BestimmeStandardArbeitszeit(Arbeitszeitmodell arbeitszeitmodell)
            {
            return arbeitszeitmodell switch
            {
                Arbeitszeitmodell.VierzigStunden => new TimeSpan(8, 30, 0),
                Arbeitszeitmodell.VerkürzteVollzeit => new TimeSpan(7, 30, 0),
                Arbeitszeitmodell.Regelarbeitszeit => new TimeSpan(8, 6, 0)
            };
        }
    }
}
