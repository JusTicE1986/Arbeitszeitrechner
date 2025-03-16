using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arbeitszeitrechner.Model
{
    public class Mitarbeiter
    {
        public string Name { get; set; }
        public Arbeitszeitmodell Zeitmodell { get; set; }
        public List<Arbeitszeit> Arbeitszeiten { get; set; }
    }

    public enum Arbeitszeitmodell
    {
        VierzigStunden,
        Regelarbeitszeit,
        VerkürzteVollzeit
    }
}
