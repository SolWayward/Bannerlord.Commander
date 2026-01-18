using System;
using System.Collections.Generic;
using System.Reflection;
using Bannerlord.Commander.UI.Enums;
using Bannerlord.Commander.UI.ViewModels.HeroMode;
using TaleWorlds.Library;

namespace Bannerlord.Commander.UI.ViewModels
{
    /// <summary>
    /// ViewModel for the Commander main screen.
    /// Lightweight mode coordinator that delegates to mode-specific ViewModels.
    /// </summary>
    public class CommanderVM : ViewModel
    {
        #region Private Fields

        private string _titleText;
        private CommanderMode _selectedMode;
        private string _selectedModeName;

        // Mode selection state
        private bool _isKingdomsSelected;
        private bool _isClansSelected;
        private bool _isHeroesSelected;
        private bool _isSettlementsSelected;
        private bool _isTroopsSelected;
        private bool _isItemsSelected;
        private bool _isCharactersSelected;

        // Mode ViewModels
        private HeroModeVM _heroesMode;

        // Filter persistence across mode switches
        private readonly Dictionary<CommanderMode, string> _filterTextByMode = new Dictionary<CommanderMode, string>();

        // Filter text for all modes - bound to UI filter box
        private string _filterText = "";

        #endregion

        #region Events

        /// <summary>
        /// Event to notify the screen that close was requested
        /// </summary>
        public event Action OnCloseRequested;

        #endregion

        #region Constructor

        public CommanderVM()
        {
            TitleText = $"COMMANDER {GetVersionString()}";

            // Initialize mode ViewModels
            _heroesMode = new();

            // Subscribe to HeroListVM changes to update count text
            if (_heroesMode?.HeroList != null)
            {
                _heroesMode.HeroList.PropertyChanged += OnHeroListPropertyChanged;
            }

            // Default to Heroes mode selected
            SelectMode(CommanderMode.Heroes);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Called each frame to handle mode-specific operations
        /// </summary>
        public void OnTick()
        {
            if (HeroesMode != null && IsHeroesSelected)
            {
                HeroesMode.OnTick();
            }
        }

        /// <summary>
        /// Refreshes data for the current mode.
        /// Called when the menu is reopened to ensure fresh data.
        /// </summary>
        public void RefreshCurrentMode()
        {
            if (HeroesMode != null && IsHeroesSelected)
            {
                HeroesMode.RefreshCurrentMode();
            }
        }

        public override void OnFinalize()
        {
            base.OnFinalize();

            // Unsubscribe from HeroListVM events
            if (_heroesMode?.HeroList != null)
            {
                _heroesMode.HeroList.PropertyChanged -= OnHeroListPropertyChanged;
            }

            HeroesMode?.OnFinalize();
            OnCloseRequested = null;
        }

        #endregion

        #region DataSource Properties

        [DataSourceProperty]
        public string TitleText
        {
            get => _titleText;
            set => SetProperty(ref _titleText, value, nameof(TitleText));
        }

        [DataSourceProperty]
        public string SelectedModeName
        {
            get => _selectedModeName;
            set => SetProperty(ref _selectedModeName, value, nameof(SelectedModeName));
        }

        [DataSourceProperty]
        public bool IsKingdomsSelected
        {
            get => _isKingdomsSelected;
            set => SetProperty(ref _isKingdomsSelected, value, nameof(IsKingdomsSelected));
        }

        [DataSourceProperty]
        public bool IsClansSelected
        {
            get => _isClansSelected;
            set => SetProperty(ref _isClansSelected, value, nameof(IsClansSelected));
        }

        [DataSourceProperty]
        public bool IsHeroesSelected
        {
            get => _isHeroesSelected;
            set => SetProperty(ref _isHeroesSelected, value, nameof(IsHeroesSelected));
        }

        [DataSourceProperty]
        public bool IsSettlementsSelected
        {
            get => _isSettlementsSelected;
            set => SetProperty(ref _isSettlementsSelected, value, nameof(IsSettlementsSelected));
        }

        [DataSourceProperty]
        public bool IsTroopsSelected
        {
            get => _isTroopsSelected;
            set => SetProperty(ref _isTroopsSelected, value, nameof(IsTroopsSelected));
        }

        [DataSourceProperty]
        public bool IsItemsSelected
        {
            get => _isItemsSelected;
            set => SetProperty(ref _isItemsSelected, value, nameof(IsItemsSelected));
        }

        [DataSourceProperty]
        public bool IsCharactersSelected
        {
            get => _isCharactersSelected;
            set => SetProperty(ref _isCharactersSelected, value, nameof(IsCharactersSelected));
        }

        [DataSourceProperty]
        public HeroModeVM HeroesMode
        {
            get => _heroesMode;
            private set
            {
                if (_heroesMode != value)
                {
                    _heroesMode = value;
                    OnPropertyChangedWithValue(value, nameof(HeroesMode));
                }
            }
        }

        /// <summary>
        /// Filter text for all modes - bound to UI filter box
        /// </summary>
        [DataSourceProperty]
        public string FilterText
        {
            get => _filterText;
            set
            {
                if (_filterText != value)
                {
                    _filterText = value;
                    OnPropertyChanged(nameof(FilterText));
                    UpdateCurrentModeFilter();
                }
            }
        }

        /// <summary>
        /// Loading status text forwarded from current mode's list VM.
        /// Used by footer bar for count display (e.g., "1928 Heroes" or "Loading... 50/1928").
        /// </summary>
        [DataSourceProperty]
        public string LoadingStatusText => HeroesMode?.HeroList?.LoadingStatusText ?? "";

        #endregion

        #region Execute Methods (Button Click Handlers)

        public void ExecuteSelectKingdoms() => SelectMode(CommanderMode.Kingdoms);
        public void ExecuteSelectClans() => SelectMode(CommanderMode.Clans);
        public void ExecuteSelectHeroes() => SelectMode(CommanderMode.Heroes);
        public void ExecuteSelectSettlements() => SelectMode(CommanderMode.Settlements);
        public void ExecuteSelectTroops() => SelectMode(CommanderMode.Troops);
        public void ExecuteSelectItems() => SelectMode(CommanderMode.Items);
        public void ExecuteSelectCharacters() => SelectMode(CommanderMode.Characters);

        public void ExecuteClose() => OnCloseRequested?.Invoke();

        #endregion

        #region Private Methods - Filter Management

        /// <summary>
        /// Apply filter text to current active mode
        /// </summary>
        private void UpdateCurrentModeFilter()
        {
            if (IsHeroesSelected && HeroesMode?.HeroList != null)
            {
                HeroesMode.HeroList.FilterText = _filterText;
            }
            // Future: Add similar logic for other modes
        }

        #endregion

        #region Private Methods - Mode Selection

        /// <summary>
        /// Central method to handle mode selection and update all related properties.
        /// Manages mode visibility and filter persistence.
        /// </summary>
        private void SelectMode(CommanderMode mode)
        {
            if (_selectedMode != mode)
            {
                // Handle previous mode visibility
                if (_selectedMode == CommanderMode.Heroes && HeroesMode != null)
                {
                    HeroesMode.IsVisible = false;
                }
            }

            _selectedMode = mode;

            IsKingdomsSelected = mode == CommanderMode.Kingdoms;
            IsClansSelected = mode == CommanderMode.Clans;
            IsHeroesSelected = mode == CommanderMode.Heroes;
            IsSettlementsSelected = mode == CommanderMode.Settlements;
            IsTroopsSelected = mode == CommanderMode.Troops;
            IsItemsSelected = mode == CommanderMode.Items;
            IsCharactersSelected = mode == CommanderMode.Characters;

            SelectedModeName = mode.ToString();

            // Handle new mode visibility
            if (mode == CommanderMode.Heroes && HeroesMode != null)
            {
                HeroesMode.IsVisible = true;
            }
        }

        #endregion

        #region Private Methods - Utilities

        private static string GetVersionString()
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version;
                if (version != null)
                {
                    return $"v{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
                }
            }
            catch
            {
                // Ignore reflection errors
            }

            return "v1.3.13.1";
        }

        /// <summary>
        /// Helper method to reduce boilerplate in string property setters
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
        /// Helper method to reduce boilerplate in bool property setters
        /// </summary>
        private bool SetProperty(ref bool field, bool value, string propertyName)
        {
            if (field == value)
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Handles property changes from HeroListVM to update count text display.
        /// Forwards LoadingStatusText changes to the footer bar binding.
        /// </summary>
        private void OnHeroListPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(HeroListVM.LoadingStatusText))
            {
                OnPropertyChanged(nameof(LoadingStatusText));
            }
        }

        #endregion
    }
}
