using System;
using Bannerlord.GameMaster.Cultures;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.Commander.UI.ViewModels.HeroEditor
{
    /// <summary>
    /// ViewModel for hero culture and clan information.
    /// Phase 1: Displays culture as read-only text.
    /// Phase 2: Will add culture dropdown and clan selection popup.
    /// </summary>
    public class HeroCultureClanVM : ViewModel
    {
        #region Private Fields

        private Hero _hero;
        private ClanSelectionPopupVM _clanSelectionPopup;
        private string _cultureName;
        private string _clanName;
        private string _kingdomName;
        private bool _hasClan;

        #endregion

        #region Constructor

        public HeroCultureClanVM()
        {
            Clear();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes the ViewModel with data from the specified hero.
        /// </summary>
        /// <param name="hero">The hero to display information for</param>
        /// <param name="clanSelectionPopup">The clan selection popup ViewModel</param>
        public void RefreshForHero(Hero hero, ClanSelectionPopupVM clanSelectionPopup = null)
        {
            _hero = hero;
            _clanSelectionPopup = clanSelectionPopup;
            
            if (_hero != null)
            {
                RefreshCultureInfo();
                RefreshClanInfo();
            }
            else
            {
                Clear();
            }
        }

        /// <summary>
        /// Clears all data from the ViewModel.
        /// </summary>
        public void Clear()
        {
            _hero = null;
            CultureName = "Unknown";
            ClanName = "No Clan";
            KingdomName = "No Kingdom";
            HasClan = false;
        }

        #endregion

        #region Execute Methods

        /// <summary>
        /// Opens the clan selection popup.
        /// Placeholder for Phase 2 implementation.
        /// </summary>
        public void ExecuteOpenClanSelection()
        {
            // TODO: Phase 2 - Implement clan selection popup and culture dropdown
            InformationManager.DisplayMessage(
                new InformationMessage("Clan & Culture selection - Coming in Phase 2", 
                TaleWorlds.Library.Color.FromUint(4282569842u)));
        }

        #endregion

        #region DataSource Properties

        /// <summary>
        /// Gets the culture name display text.
        /// </summary>
        [DataSourceProperty]
        public string CultureName
        {
            get => _cultureName;
            set => SetProperty(ref _cultureName, value, nameof(CultureName));
        }

        /// <summary>
        /// Gets the clan name display text.
        /// </summary>
        [DataSourceProperty]
        public string ClanName
        {
            get => _clanName;
            set => SetProperty(ref _clanName, value, nameof(ClanName));
        }

        /// <summary>
        /// Gets the kingdom name display text.
        /// </summary>
        [DataSourceProperty]
        public string KingdomName
        {
            get => _kingdomName;
            set => SetProperty(ref _kingdomName, value, nameof(KingdomName));
        }

        /// <summary>
        /// Gets whether the hero has a clan.
        /// </summary>
        [DataSourceProperty]
        public bool HasClan
        {
            get => _hasClan;
            set => SetProperty(ref _hasClan, value, nameof(HasClan));
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Refreshes the culture information display.
        /// </summary>
        private void RefreshCultureInfo()
        {
            if (_hero == null)
            {
                CultureName = "Unknown";
                return;
            }
            
            CultureName = _hero.Culture?.Name?.ToString() ?? "Unknown";
        }

        /// <summary>
        /// Refreshes the clan and kingdom information display.
        /// </summary>
        private void RefreshClanInfo()
        {
            if (_hero == null)
            {
                Clear();
                return;
            }
            
            // Clan information
            if (_hero.Clan != null)
            {
                HasClan = true;
                ClanName = _hero.Clan.Name?.ToString() ?? "Unknown Clan";
                
                // Kingdom information
                if (_hero.Clan.Kingdom != null)
                {
                    KingdomName = _hero.Clan.Kingdom.Name?.ToString() ?? "Unknown Kingdom";
                }
                else
                {
                    KingdomName = "No Kingdom";
                }
            }
            else
            {
                HasClan = false;
                ClanName = "No Clan";
                KingdomName = "No Kingdom";
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Helper method to reduce boilerplate in string property setters.
        /// </summary>
        private bool SetProperty(ref string field, string value, string propertyName)
        {
            if (field == value)
                return false;

            field = value;
            OnPropertyChangedWithValue(value, propertyName);
            return true;
        }

        /// <summary>
        /// Helper method to reduce boilerplate in bool property setters.
        /// </summary>
        private bool SetProperty(ref bool field, bool value, string propertyName)
        {
            if (field == value)
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion
    }
}
