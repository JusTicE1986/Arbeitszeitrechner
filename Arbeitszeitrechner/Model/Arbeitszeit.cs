using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arbeitszeitrechner.Model
{
    public class Arbeitszeit
    {
        public DateTime Startzeit { get; set; }
        public DateTime Endzeit { get; set; }
        public TimeSpan Pause { get; set; }
        public Abwesenheit Abwesenheit { get; set; }
        public DateTime Datum { get; set; }
        public TimeSpan Gesamtarbeitszeit { get; set; }
        public string Notizen { get; set; }
        public TimeSpan Überstunden { get; set; }

    }
    public enum Abwesenheit { 
        Krankheit, 
        Urlaub, 
        Feiertag, 
        Überstunden, 
        Sonstiges };
}
