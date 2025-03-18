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
using Arbeitszeitrechner.Services;
using CommunityToolkit.Mvvm.Input;

namespace Arbeitszeitrechner.ViewModel
{
    public class ArbeitszeitViewModel
    {
        #region Eigenschaften
        private readonly IWochenService _wochenService;
        private readonly FeiertagsService _feiertagService;
        private readonly ArbeitszeitBerechnungsService _arbeitszeitBerechnungsService;

        public ObservableCollection<ArbeitszeitWoche> Arbeitswochen { get; set; }

        #endregion
        #region Methoden
        public void GeneriereWochenweiseEintraege()
        {
            int jahr = DateTime.Now.Year;
            var feiertage = _feiertagService.GetFeiertage(jahr);
            Arbeitswochen = _wochenService.GeneriereWochenweiseEintraege(jahr, feiertage);
        }

        public void VerteileRestzeitAufWochentage()
        {
            _wochenService.VerteileRestzeitAufWochentage(Arbeitswochen);
        }

        public void VerteileRestzeitAutomatisch()
        {
            foreach (var woche in Arbeitswochen)
            {
                _wochenService.DynamischeRestzeitVerteilung(woche);
            }
        }

        
        #endregion
        #region Konstruktor
        public ArbeitszeitViewModel(
            IWochenService wochenService,
            FeiertagsService feiertagsService,
            ArbeitszeitBerechnungsService arbeitszeitBerechnungsService)
        {
            _wochenService = wochenService ?? throw new ArgumentNullException(nameof(wochenService));
            _feiertagService = feiertagsService ?? throw new ArgumentNullException(nameof(feiertagsService));
            _arbeitszeitBerechnungsService = arbeitszeitBerechnungsService ?? throw new ArgumentNullException(nameof(arbeitszeitBerechnungsService));

            GeneriereWochenweiseEintraege();
        }

        

        #endregion

    }
}
