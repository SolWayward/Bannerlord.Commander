using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.Commander.UI.ViewModels.HeroEditor.Panels
{
    /// <summary>
    /// ViewModel for the hero clan read-only panel.
    /// Displays the hero's clan name and whether they have a clan.
    /// </summary>
    public class HeroClanPanelVM : ViewModel
    {
        #region Private Fields

        private Hero _hero;
        private string _clanName;
        private bool _hasClan;

        #endregion

        #region Constructor

        public HeroClanPanelVM()
        {
            Clear();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes the ViewModel with data from the specified hero.
        /// </summary>
        /// <param name="hero">The hero to display clan for</param>
        public void RefreshForHero(Hero hero)
        {
            _hero = hero;

            if (_hero != null)
            {
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
            ClanName = "No Clan";
            HasClan = false;
        }

        #endregion

        #region DataSource Properties

        /// <summary>
        /// Gets the clan name display text.
        /// </summary>
        [DataSourceProperty]
        public string ClanName
        {
            get => _clanName;
            private set => SetProperty(ref _clanName, value, nameof(ClanName));
        }

        /// <summary>
        /// Gets whether the hero has a clan.
        /// </summary>
        [DataSourceProperty]
        public bool HasClan
        {
            get => _hasClan;
            private set => SetProperty(ref _hasClan, value, nameof(HasClan));
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Refreshes the clan information display.
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
            }
            else
            {
                HasClan = false;
                ClanName = "No Clan";
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
