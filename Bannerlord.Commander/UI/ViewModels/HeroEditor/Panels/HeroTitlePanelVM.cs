using Bannerlord.GameMaster.Information;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.Commander.UI.ViewModels.HeroEditor.Panels
{
    /// <summary>
    /// ViewModel for the hero title input panel.
    /// Handles hero title editing (currently read-only pending BLGM implementation).
    /// </summary>
    public class HeroTitlePanelVM : ViewModel
    {
        #region Private Fields

        private Hero _hero;
        private string _heroTitle;
        private string _originalTitle;

        #endregion

        #region Constructor

        public HeroTitlePanelVM()
        {
            Clear();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes the ViewModel with data from the specified hero.
        /// </summary>
        /// <param name="hero">The hero to display title for</param>
        public void RefreshForHero(Hero hero)
        {
            _hero = hero;

            if (_hero != null)
            {
                // Title is stored in FirstName property
                HeroTitle = _hero.FirstName?.ToString() ?? "";
                _originalTitle = HeroTitle;
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
            HeroTitle = "";
            _originalTitle = "";
        }

        #endregion

        #region Execute Methods

        /// <summary>
        /// Saves the hero's title.
        /// Called when the user finishes editing the title field.
        /// NOTE: Title editing is not yet implemented in BLGM - FirstName property is read-only.
        /// TODO: Request BLGM implementation for SetFirstName/SetTitle extension method.
        /// </summary>
        public void ExecuteSaveTitle()
        {
            if (_hero == null)
                return;

            // Title editing not yet supported - BLGM needs to implement this feature
            InfoMessage.Warning("Title editing not yet supported - requires BLGM extension");

            // Revert to original title
            HeroTitle = _originalTitle;
        }

        #endregion

        #region DataSource Properties

        /// <summary>
        /// Gets or sets the hero's title (editable).
        /// </summary>
        [DataSourceProperty]
        public string HeroTitle
        {
            get => _heroTitle;
            set => SetProperty(ref _heroTitle, value, nameof(HeroTitle));
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
