using TaleWorlds.Library;
using Bannerlord.Commander.UI.ViewModels.HeroEditor;

namespace Bannerlord.Commander.UI.ViewModels.HeroMode
{
    /// <summary>
    /// Coordinator ViewModel for the Heroes mode.
    /// Owns and coordinates HeroListVM and HeroEditorVM, managing communication between them.
    /// </summary>
    public class HeroModeVM : ViewModel
    {
        #region Private Fields

        private HeroListVM _heroListVM;
        private HeroEditorVM _heroEditorVM;
        private bool _isVisible;

        #endregion

        #region Constructor

        public HeroModeVM()
        {
            // Create the hero list view model
            _heroListVM = new HeroListVM();

            // Create the hero editor view model
            _heroEditorVM = new HeroEditorVM();

            // Initialize visibility
            IsVisible = false;

            // Wire up hero selection coordination
            // HeroListVM implements IHeroSelectionHandler and receives hero selection events from CommanderHeroVM widgets
            // When a hero is selected, HeroListVM.SelectHero is called
            // HeroesModeVM then coordinates by updating HeroEditorVM with the selected hero
            SubscribeToHeroSelected();
        }

        #endregion

        #region Public Methods - Lifecycle

        /// <summary>
        /// Called each frame to handle deferred operations and incremental loading.
        /// Delegates to HeroListVM's tick handler.
        /// </summary>
        public void OnTick()
        {
            // Delegate tick processing to HeroListVM
            // Initial loading is triggered by _needsHeroLoad flag set in HeroListVM constructor
            _heroListVM?.OnTick();
        }

        /// <summary>
        /// Refreshes data for the Heroes mode.
        /// Called when the mode is activated or reopened to ensure fresh data.
        /// </summary>
        public void RefreshCurrentMode()
        {
            // Clear and reload heroes
            _heroListVM?.RefreshCurrentMode();
        }

        public override void OnFinalize()
        {
            // Unsubscribe from hero selection coordination
            if (_heroListVM != null)
            {
                _heroListVM.HeroSelected -= OnHeroSelectedFromList;
            }

            base.OnFinalize();
            _heroListVM?.OnFinalize();
            _heroEditorVM?.OnFinalize();
        }

        #endregion

        #region DataSource Properties

        /// <summary>
        /// Gets the hero list view model.
        /// </summary>
        [DataSourceProperty]
        public HeroListVM HeroList => _heroListVM;

        /// <summary>
        /// Gets the hero editor view model.
        /// </summary>
        [DataSourceProperty]
        public HeroEditorVM HeroEditor => _heroEditorVM;

        /// <summary>
        /// Gets or sets whether the heroes mode is currently visible.
        /// </summary>
        [DataSourceProperty]
        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value, nameof(IsVisible));
        }

        #endregion

        #region Private Methods - Coordination

        /// <summary>
        /// Sets up coordination between HeroListVM and HeroEditorVM.
        /// When a hero is selected in the list, the editor is updated with that hero.
        /// </summary>
        private void SubscribeToHeroSelected()
        {
            // Subscribe to hero selection events from the list
            // When a hero is selected, update the editor
            _heroListVM.HeroSelected += OnHeroSelectedFromList;
        }

        /// <summary>
        /// Called when a hero is selected in HeroListVM.
        /// Updates HeroEditorVM to display the selected hero.
        /// </summary>
        private void OnHeroSelectedFromList(HeroListItemVM hero)
        {
            if (_heroEditorVM == null)
            {
                return;
            }

            _heroEditorVM.SelectHero(hero);
        }

        #endregion

        #region Helper Methods

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
