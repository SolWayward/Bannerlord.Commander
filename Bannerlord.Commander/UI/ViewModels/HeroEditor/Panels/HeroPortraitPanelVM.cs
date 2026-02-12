using Bannerlord.Commander.Settings;
using Bannerlord.GameMaster.Characters;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;

namespace Bannerlord.Commander.UI.ViewModels.HeroEditor.Panels
{
    /// <summary>
    /// ViewModel for the hero portrait panel.
    /// Displays the hero's character portrait image.
    /// </summary>
    public class HeroPortraitPanelVM : ViewModel
    {
        #region Private Fields

        private Hero _hero;
        private CharacterImageIdentifierVM _portraitImage;

        #endregion

        #region Constructor

        public HeroPortraitPanelVM()
        {
            _portraitImage = null;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes the ViewModel with data from the specified hero.
        /// </summary>
        /// <param name="hero">The hero to display portrait for</param>
        public void RefreshForHero(Hero hero)
        {
            _hero = hero;

            if (_hero != null)
            {
                // Finalize previous portrait if it exists before creating new one
                _portraitImage?.OnFinalize();

                // Create CharacterPortrait
                CharacterCode characterCode = CharacterHelpers.BuildCharacterCode(_hero.CharacterObject, true, SettingsManager.HeroSettings.ShowHiddenInfo);
                PortraitImage = new CharacterImageIdentifierVM(characterCode);
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

            // Finalize and set to null - don't create empty VM which causes silhouette
            _portraitImage?.OnFinalize();
            _portraitImage = null;
            OnPropertyChanged(nameof(PortraitImage));
        }

        public override void OnFinalize()
        {
            base.OnFinalize();
            _portraitImage?.OnFinalize();
        }

        #endregion

        #region DataSource Properties

        /// <summary>
        /// Gets the character portrait image identifier for display.
        /// </summary>
        [DataSourceProperty]
        public CharacterImageIdentifierVM PortraitImage
        {
            get => _portraitImage;
            private set
            {
                if (_portraitImage != value)
                {
                    _portraitImage = value;
                    OnPropertyChangedWithValue(value, nameof(PortraitImage));
                }
            }
        }

        #endregion
    }
}
