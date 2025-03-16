using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arbeitszeitrechner.Model
{
    public class FeiertagsBerechnung
    {
        public static Dictionary<DateTime, string> GetFeiertage(int jahr)
        {
            var feiertage = new Dictionary<DateTime, string>
        {
            { new DateTime(jahr, 1, 1), "Neujahr" },
            { new DateTime(jahr, 5, 1), "Tag der Arbeit" },
            { new DateTime(jahr, 10, 3), "Tag der Deutschen Einheit" },
            { new DateTime(jahr, 12, 25), "1. Weihnachtstag" },
            { new DateTime(jahr, 12, 26), "2. Weihnachtstag" }
        };

            // ➤ Bewegliche Feiertage berechnen
            var ostersonntag = BerechneOstersonntag(jahr);
            feiertage.Add(ostersonntag, "Ostersonntag");
            feiertage.Add(ostersonntag.AddDays(-2), "Karfreitag");
            feiertage.Add(ostersonntag.AddDays(1), "Ostermontag");
            feiertage.Add(ostersonntag.AddDays(39), "Christi Himmelfahrt");
            feiertage.Add(ostersonntag.AddDays(49), "Pfingstsonntag");
            feiertage.Add(ostersonntag.AddDays(50), "Pfingstmontag");
            feiertage.Add(ostersonntag.AddDays(60), "Fronleichnam");

            return feiertage;
        }

        // ➤ Algorithmus zur Berechnung des Ostersonntags (Gauss'sche Osterformel)
        private static DateTime BerechneOstersonntag(int jahr)
        {
            int a = jahr % 19;
            int b = jahr / 100;
            int c = jahr % 100;
            int d = b / 4;
            int e = b % 4;
            int f = (b + 8) / 25;
            int g = (b - f + 1) / 3;
            int h = (19 * a + b - d - g + 15) % 30;
            int i = c / 4;
            int k = c % 4;
            int l = (32 + 2 * e + 2 * i - h - k) % 7;
            int m = (a + 11 * h + 22 * l) / 451;
            int monat = (h + l - 7 * m + 114) / 31;
            int tag = ((h + l - 7 * m + 114) % 31) + 1;

            return new DateTime(jahr, monat, tag);
        }
    }

}
