using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;

namespace Bannerlord.Commander.UI.ViewModels.HeroEditor.Panels
{
    /// <summary>
    /// ViewModel for the clan banner panel.
    /// Displays the hero's clan banner image.
    /// </summary>
    public class ClanBannerPanelVM : ViewModel
    {
        #region Private Fields

        private Hero _hero;
        private BannerImageIdentifierVM _clanBanner;

        #endregion

        #region Constructor

        public ClanBannerPanelVM()
        {
            _clanBanner = null;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes the ViewModel with data from the specified hero.
        /// </summary>
        /// <param name="hero">The hero to display clan banner for</param>
        public void RefreshForHero(Hero hero)
        {
            _hero = hero;

            if (_hero != null)
            {
                // Update clan banner
                _clanBanner?.OnFinalize();
                ClanBanner = new BannerImageIdentifierVM(_hero.ClanBanner, true);
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

            // Finalize and set to null
            _clanBanner?.OnFinalize();
            _clanBanner = null;
            OnPropertyChanged(nameof(ClanBanner));
        }

        public override void OnFinalize()
        {
            base.OnFinalize();
            _clanBanner?.OnFinalize();
        }

        #endregion

        #region DataSource Properties

        /// <summary>
        /// Gets the clan banner image identifier for display.
        /// </summary>
        [DataSourceProperty]
        public BannerImageIdentifierVM ClanBanner
        {
            get => _clanBanner;
            private set
            {
                if (_clanBanner != value)
                {
                    _clanBanner = value;
                    OnPropertyChangedWithValue(value, nameof(ClanBanner));
                }
            }
        }

        #endregion
    }
}
