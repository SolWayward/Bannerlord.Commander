using System;
using Bannerlord.GameMaster.Heroes;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Bannerlord.Commander.UI.ViewModels.HeroEditor
{
    /// <summary>
    /// ViewModel for editable hero identity information.
    /// Handles hero name and title editing with BLGM API integration.
    /// </summary>
    public class HeroIdentityVM : ViewModel
    {
        #region Private Fields

        private Hero _hero;
        private string _heroName;
        private string _heroTitle;
        private string _originalName;
        private string _originalTitle;

        #endregion

        #region Constructor

        public HeroIdentityVM()
        {
            Clear();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes the ViewModel with data from the specified hero.
        /// </summary>
        /// <param name="hero">The hero to display information for</param>
        public void RefreshForHero(Hero hero)
        {
            _hero = hero;
            
            if (_hero != null)
            {
                HeroName = _hero.Name?.ToString() ?? "";
                _originalName = HeroName;
                
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
            HeroName = "";
            HeroTitle = "";
            _originalName = "";
            _originalTitle = "";
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
                    
                    InformationManager.DisplayMessage(
                        new InformationMessage($"Hero name changed to: {HeroName}", 
                        TaleWorlds.Library.Color.FromUint(4282569842u)));
                }
                catch (Exception ex)
                {
                    InformationManager.DisplayMessage(
                        new InformationMessage($"Failed to change hero name: {ex.Message}", 
                        TaleWorlds.Library.Color.FromUint(4291559424u)));
                    
                    // Revert to original name on failure
                    HeroName = _originalName;
                }
            }
        }

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
            InformationManager.DisplayMessage(
                new InformationMessage("Title editing not yet supported - requires BLGM extension",
                TaleWorlds.Library.Color.FromUint(4291559424u)));
            
            // Revert to original title
            HeroTitle = _originalTitle;
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
