using System;
using Bannerlord.GameMaster.Heroes;
using Bannerlord.GameMaster.Information;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.Commander.UI.ViewModels.HeroEditor.Panels
{
    /// <summary>
    /// ViewModel for the hero name input panel.
    /// Handles hero name editing with BLGM API integration.
    /// </summary>
    public class HeroNamePanelVM : ViewModel
    {
        #region Private Fields

        private Hero _hero;
        private string _heroName;
        private string _originalName;

        #endregion

        #region Constructor

        public HeroNamePanelVM()
        {
            Clear();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes the ViewModel with data from the specified hero.
        /// </summary>
        /// <param name="hero">The hero to display name for</param>
        public void RefreshForHero(Hero hero)
        {
            _hero = hero;

            if (_hero != null)
            {
                HeroName = _hero.Name?.ToString() ?? "";
                _originalName = HeroName;
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
            HeroName = "";
            _originalName = "";
        }

        #endregion

        #region Execute Methods

        /// <summary>
        /// Saves the hero's name using BLGM API.
        /// Called when the user finishes editing the name field.
        /// </summary>
        public void ExecuteSaveName()
        {
            if (_hero == null || string.IsNullOrWhiteSpace(HeroName))
                return;

            // Only save if the name changed
            if (HeroName != _originalName)
            {
                try
                {
                    // Use BLGM API to set the hero's name
                    _hero.SetStringName(HeroName);
                    _originalName = HeroName;

                    InfoMessage.Success($"Hero name changed to: {HeroName}");
                }
                catch (Exception ex)
                {
                    InfoMessage.Error($"Failed to change hero name: {ex.Message}");

                    // Revert to original name on failure
                    HeroName = _originalName;
                }
            }
        }

        #endregion

        #region DataSource Properties

        /// <summary>
        /// Gets or sets the hero's name (editable).
        /// </summary>
        [DataSourceProperty]
        public string HeroName
        {
            get => _heroName;
            set => SetProperty(ref _heroName, value, nameof(HeroName));
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
