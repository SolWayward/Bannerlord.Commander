using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.Commander.UI.ViewModels.HeroEditor.Panels
{
    /// <summary>
    /// ViewModel for the hero kingdom read-only panel.
    /// Displays the kingdom that the hero's clan belongs to.
    /// </summary>
    public class HeroKingdomPanelVM : ViewModel
    {
        #region Private Fields

        private Hero _hero;
        private string _kingdomName;

        #endregion

        #region Constructor

        public HeroKingdomPanelVM()
        {
            Clear();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes the ViewModel with data from the specified hero.
        /// </summary>
        /// <param name="hero">The hero to display kingdom for</param>
        public void RefreshForHero(Hero hero)
        {
            _hero = hero;

            if (_hero != null)
            {
                RefreshKingdomInfo();
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
            KingdomName = "No Kingdom";
        }

        #endregion

        #region DataSource Properties

        /// <summary>
        /// Gets the kingdom name display text.
        /// </summary>
        [DataSourceProperty]
        public string KingdomName
        {
            get => _kingdomName;
            private set => SetProperty(ref _kingdomName, value, nameof(KingdomName));
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Refreshes the kingdom information display.
        /// </summary>
        private void RefreshKingdomInfo()
        {
            if (_hero == null)
            {
                Clear();
                return;
            }

            // Kingdom information
            if (_hero.Clan?.Kingdom != null)
            {
                KingdomName = _hero.Clan.Kingdom.Name?.ToString() ?? "Unknown Kingdom";
            }
            else
            {
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

        #endregion
    }
}
