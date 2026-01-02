using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bannerlord.Commander.UI.Enums;
using Bannerlord.Commander.UI.Services;
using Bannerlord.Commander.UI.ViewModels.HeroEditor;
using Bannerlord.GameMaster.Heroes;
using TaleWorlds.Library;

namespace Bannerlord.Commander.UI.ViewModels
{
    /// <summary>
    /// ViewModel for the Commander main screen.
    /// Provides data binding for the UI elements.
    /// 
    /// Filtering uses the IsFiltered property pattern (like native Inventory):
    /// - Heroes are never removed from the list
    /// - IsFiltered toggles visibility via IsHidden binding in XML
    /// - This avoids expensive list rebuilds and keeps UI smooth
    /// </summary>
    public class CommanderVM : ViewModel, IHeroSelectionHandler
    {
        #region Constants

        /// <summary>
        /// Number of heroes to add per frame during incremental loading.
        /// Kept low for smooth background loading without UI freeze.
        /// </summary>
        private const int HeroesPerFrame = 50;

        /// <summary>
        /// Sort indicator for ascending sort.
        /// </summary>
        private const string SortAscIndicator = " ^";

        /// <summary>
        /// Sort indicator for descending sort.
        /// </summary>
        private const string SortDescIndicator = " v";

        /// <summary>
        /// Minimum delay between filter operations in milliseconds.
        /// Used as debounce to avoid filtering on every keystroke.
        /// </summary>
        private const int FilterDebounceMs = 15; // Not realy needed with using native IsFiltered pattern but keeping for future safety (reduced from 150 to 15ms)

        #endregion

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

        // Heroes collection - single list, filtering done via IsFiltered property
        private MBBindingList<HeroItemVM> _heroes;
        private HeroItemVM _selectedHero;

        // Hero Editor
        private HeroEditorVM _heroEditor;

        // Loading state
        private bool _isLoading;
        private bool _needsHeroLoad;
        private bool _hasLoadedOnce;
        private string _loadingStatusText;

        // Incremental loading state
        private List<HeroItemVM> _pendingHeroVMs;
        private int _pendingHeroIndex;

        // Deferred loading parameters
        private string _pendingLoadQuery = "";
        private HeroTypes _pendingLoadHeroTypes = HeroTypes.None;
        private bool _pendingLoadMatchAll = true;
        private bool _pendingQueryExecution;

        // Sorting state
        private HeroSortColumn _currentSortColumn = HeroSortColumn.Name;
        private bool _sortAscending = true;

        // Sort indicator text fields
        private string _nameSortIndicatorText;
        private string _genderSortIndicatorText;
        private string _ageSortIndicatorText;
        private string _clanSortIndicatorText;
        private string _kingdomSortIndicatorText;
        private string _cultureSortIndicatorText;
        private string _typeSortIndicatorText;
        private string _levelSortIndicatorText;

        // Filter state - uses IsFiltered property on each HeroItemVM
        private string _filterText = "";
        private readonly Dictionary<CommanderMode, string> _filterTextByMode = new Dictionary<CommanderMode, string>();
        private string _pendingFilterText;
        private bool _filterPending;
        private DateTime _lastFilterChange = DateTime.MinValue;

        // Visible hero count for status display
        private int _visibleHeroCount;

        #endregion

        #region Events

        /// <summary>
        /// Event to notify the screen that close was requested
        /// </summary>
        public event Action OnCloseRequested;

        /// <summary>
        /// Event to notify the screen when filter text changes (for input restriction management)
        /// </summary>
        public event Action OnFilterTextChanged;

        #endregion

        #region Constructor

        public CommanderVM()
        {
            TitleText = $"COMMANDER {GetVersionString()}";
            Heroes = new MBBindingList<HeroItemVM>();

            // Initialize Hero Editor ViewModel
            HeroEditor = new HeroEditorVM();

            // Initialize sort indicator texts
            UpdateSortIndicatorTexts();

            // Default to Heroes mode selected
            SelectMode(CommanderMode.Heroes);

            // Flag that heroes need to be loaded, but defer until bindings are ready
            _needsHeroLoad = true;
            _hasLoadedOnce = false;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Called each frame to handle deferred operations and incremental loading
        /// </summary>
        public void OnTick()
        {
            ProcessDeferredHeroLoad();
            ProcessDeferredQuery();
            ProcessIncrementalHeroLoading();
            ProcessDeferredFilter();
        }

        /// <summary>
        /// Refreshes data for the current mode.
        /// Called when the menu is reopened to ensure fresh data.
        /// </summary>
        public void RefreshCurrentMode()
        {
            if (!_hasLoadedOnce)
                return;

            if (_selectedMode == CommanderMode.Heroes)
            {
                _isLoading = false;
                StartHeroLoading();
            }
            // TODO: Add refresh for other modes as implemented
        }

        /// <summary>
        /// Selects a hero and deselects all others, then updates HeroEditor
        /// </summary>
        public void SelectHero(HeroItemVM hero)
        {
            if (_selectedHero == hero)
                return;

            if (_selectedHero != null)
            {
                _selectedHero.IsSelected = false;
            }

            _selectedHero = hero;
            if (_selectedHero != null)
            {
                _selectedHero.IsSelected = true;
                HeroEditor?.RefreshForHero(_selectedHero.Hero);
            }
            else
            {
                HeroEditor?.Clear();
            }
            
            OnPropertyChanged(nameof(IsHeroSelected));
        }

        public override void OnFinalize()
        {
            base.OnFinalize();
            HeroEditor?.OnFinalize();
            OnCloseRequested = null;
        }

        /// <summary>
        /// Gets the HeroEditor ViewModel for the right panel
        /// </summary>
        [DataSourceProperty]
        public HeroEditorVM HeroEditor
        {
            get => _heroEditor;
            private set
            {
                if (_heroEditor != value)
                {
                    _heroEditor = value;
                    OnPropertyChangedWithValue(value, nameof(HeroEditor));
                }
            }
        }

        /// <summary>
        /// Gets whether a hero is currently selected (for editor panel visibility)
        /// </summary>
        [DataSourceProperty]
        public bool IsHeroSelected => _selectedHero != null && HeroEditor?.IsVisible == true;

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
        public MBBindingList<HeroItemVM> Heroes
        {
            get => _heroes;
            set => SetProperty(ref _heroes, value, nameof(Heroes));
        }

        [DataSourceProperty]
        public string LoadingStatusText
        {
            get => _loadingStatusText;
            set => SetProperty(ref _loadingStatusText, value, nameof(LoadingStatusText));
        }

        [DataSourceProperty]
        public bool IsLoading => _isLoading;

        [DataSourceProperty]
        public bool IsBusy => _isLoading;

        [DataSourceProperty]
        public string NameSortIndicatorText
        {
            get => _nameSortIndicatorText;
            set => SetProperty(ref _nameSortIndicatorText, value, nameof(NameSortIndicatorText));
        }

        [DataSourceProperty]
        public string GenderSortIndicatorText
        {
            get => _genderSortIndicatorText;
            set => SetProperty(ref _genderSortIndicatorText, value, nameof(GenderSortIndicatorText));
        }

        [DataSourceProperty]
        public string AgeSortIndicatorText
        {
            get => _ageSortIndicatorText;
            set => SetProperty(ref _ageSortIndicatorText, value, nameof(AgeSortIndicatorText));
        }

        [DataSourceProperty]
        public string ClanSortIndicatorText
        {
            get => _clanSortIndicatorText;
            set => SetProperty(ref _clanSortIndicatorText, value, nameof(ClanSortIndicatorText));
        }

        [DataSourceProperty]
        public string KingdomSortIndicatorText
        {
            get => _kingdomSortIndicatorText;
            set => SetProperty(ref _kingdomSortIndicatorText, value, nameof(KingdomSortIndicatorText));
        }

        [DataSourceProperty]
        public string CultureSortIndicatorText
        {
            get => _cultureSortIndicatorText;
            set => SetProperty(ref _cultureSortIndicatorText, value, nameof(CultureSortIndicatorText));
        }

        [DataSourceProperty]
        public string TypeSortIndicatorText
        {
            get => _typeSortIndicatorText;
            set => SetProperty(ref _typeSortIndicatorText, value, nameof(TypeSortIndicatorText));
        }

        [DataSourceProperty]
        public string LevelSortIndicatorText
        {
            get => _levelSortIndicatorText;
            set => SetProperty(ref _levelSortIndicatorText, value, nameof(LevelSortIndicatorText));
        }

        [DataSourceProperty]
        public string FilterText
        {
            get => _filterText;
            set
            {
                if (_filterText != value)
                {
                    _filterText = value ?? "";
                    OnPropertyChangedWithValue(value, nameof(FilterText));

                    // Store for current mode
                    _filterTextByMode[_selectedMode] = _filterText;

                    // Schedule deferred filter with debounce
                    _pendingFilterText = _filterText;
                    _filterPending = true;
                    _lastFilterChange = DateTime.UtcNow;

                    // Notify screen that filter text changed (for input restriction management)
                    OnFilterTextChanged?.Invoke();
                }
            }
        }

        #endregion

        #region Execute Methods (Button Click Handlers)

        public void ExecuteSelectKingdoms() => SelectMode(CommanderMode.Kingdoms);
        public void ExecuteSelectClans() => SelectMode(CommanderMode.Clans);

        public void ExecuteSelectHeroes()
        {
            SelectMode(CommanderMode.Heroes);
        }

        public void ExecuteSelectSettlements() => SelectMode(CommanderMode.Settlements);
        public void ExecuteSelectTroops() => SelectMode(CommanderMode.Troops);
        public void ExecuteSelectItems() => SelectMode(CommanderMode.Items);
        public void ExecuteSelectCharacters() => SelectMode(CommanderMode.Characters);

        public void ExecuteClose() => OnCloseRequested?.Invoke();

        public void ExecuteSortByName() => ExecuteSortByColumn(HeroSortColumn.Name);
        public void ExecuteSortByGender() => ExecuteSortByColumn(HeroSortColumn.Gender);
        public void ExecuteSortByAge() => ExecuteSortByColumn(HeroSortColumn.Age);
        public void ExecuteSortByClan() => ExecuteSortByColumn(HeroSortColumn.Clan);
        public void ExecuteSortByKingdom() => ExecuteSortByColumn(HeroSortColumn.Kingdom);
        public void ExecuteSortByCulture() => ExecuteSortByColumn(HeroSortColumn.Culture);
        public void ExecuteSortByType() => ExecuteSortByColumn(HeroSortColumn.Type);
        public void ExecuteSortByLevel() => ExecuteSortByColumn(HeroSortColumn.Level);

        #endregion

        #region Private Methods - Mode Selection

        /// <summary>
        /// Central method to handle mode selection and update all related properties
        /// </summary>
        private void SelectMode(CommanderMode mode)
        {
            // Save current filter text for the previous mode (if any)
            if (_selectedMode != mode)
            {
                _filterTextByMode[_selectedMode] = _filterText;
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

            // Restore filter text for the new mode (or empty if not set)
            if (_filterTextByMode.TryGetValue(mode, out string savedFilter))
            {
                _filterText = savedFilter;
            }
            else
            {
                _filterText = "";
            }
            OnPropertyChangedWithValue(_filterText, nameof(FilterText));
        }

        #endregion

        #region Private Methods - Hero Loading

        private void ProcessDeferredHeroLoad()
        {
            if (_needsHeroLoad && _selectedMode == CommanderMode.Heroes)
            {
                _needsHeroLoad = false;
                StartHeroLoading();
            }
        }

        private void ProcessDeferredQuery()
        {
            if (_pendingQueryExecution)
            {
                _pendingQueryExecution = false;
                ExecuteDeferredQuery();
            }
        }

        /// <summary>
        /// Starts the incremental hero loading process using deferred tick-based loading.
        /// </summary>
        private void StartHeroLoading(string query = "", HeroTypes heroTypes = HeroTypes.None, bool matchAll = true)
        {
            if (_isLoading)
                return;

            _pendingLoadQuery = query;
            _pendingLoadHeroTypes = heroTypes;
            _pendingLoadMatchAll = matchAll;

            Heroes = new MBBindingList<HeroItemVM>();
            _isLoading = true;
            _selectedHero = null;
            LoadingStatusText = "Loading...";

            _pendingQueryExecution = true;
        }

        /// <summary>
        /// Executes the deferred hero query. Called on next tick after StartHeroLoading.
        /// </summary>
        private void ExecuteDeferredQuery()
        {
            var rawHeroes = HeroQueries.QueryHeroes(
                _pendingLoadQuery,
                _pendingLoadHeroTypes,
                _pendingLoadMatchAll,
                includeDead: false);

            _pendingHeroVMs = rawHeroes.Select(h => new HeroItemVM(h, this)).ToList();

            // Pre-sort the heroes before adding to list
            HeroSorter.Sort(_pendingHeroVMs, _currentSortColumn, _sortAscending);

            _pendingHeroIndex = 0;
            UpdateLoadingStatus();
        }

        /// <summary>
        /// Processes a batch of heroes each frame for incremental loading.
        /// </summary>
        private void ProcessIncrementalHeroLoading()
        {
            if (!_isLoading || _pendingHeroVMs == null)
                return;

            int endIndex = Math.Min(_pendingHeroIndex + HeroesPerFrame, _pendingHeroVMs.Count);

            for (int i = _pendingHeroIndex; i < endIndex; i++)
            {
                Heroes.Add(_pendingHeroVMs[i]);
            }

            _pendingHeroIndex = endIndex;
            LoadingStatusText = $"Loading... {_pendingHeroIndex}/{_pendingHeroVMs.Count}";

            if (_pendingHeroIndex >= _pendingHeroVMs.Count)
            {
                CompleteHeroLoading();
            }
        }

        private void CompleteHeroLoading()
        {
            _isLoading = false;
            _pendingHeroVMs = null;
            _pendingHeroIndex = 0;
            _hasLoadedOnce = true;

            // Apply initial filter if there's existing filter text
            if (!string.IsNullOrEmpty(_filterText))
            {
                ApplyFilter(_filterText);
            }
            else
            {
                _visibleHeroCount = Heroes.Count;
            }

            UpdateHeroCountStatus();
            OnPropertyChanged(nameof(IsBusy));
        }

        /// <summary>
        /// Updates the status text to show current hero count with filter info if applicable.
        /// </summary>
        private void UpdateHeroCountStatus()
        {
            if (!string.IsNullOrEmpty(_filterText))
            {
                LoadingStatusText = $"{_visibleHeroCount} / {Heroes.Count} Heroes";
            }
            else
            {
                LoadingStatusText = $"{Heroes.Count} Heroes";
            }
        }

        private void UpdateLoadingStatus()
        {
            if (_pendingHeroVMs != null && _pendingHeroVMs.Count > 0)
            {
                LoadingStatusText = $"Loading... {_pendingHeroIndex}/{_pendingHeroVMs.Count}";
            }
            else
            {
                LoadingStatusText = "";
            }
        }

        #endregion

        #region Private Methods - Sorting

        /// <summary>
        /// Handles sorting by a column. If already sorting by this column, toggles direction.
        /// Uses in-place MBBindingList.Sort() for instant performance (native inventory pattern).
        /// </summary>
        private void ExecuteSortByColumn(HeroSortColumn column)
        {
            if (_isLoading)
                return;

            if (_currentSortColumn == column)
            {
                _sortAscending = !_sortAscending;
            }
            else
            {
                _currentSortColumn = column;
                _sortAscending = true;
            }

            UpdateSortIndicatorTexts();
            ApplySortToList();
        }

        /// <summary>
        /// Applies the current sort to the Heroes list.
        /// Uses in-place sorting via MBBindingList.Sort(IComparer) for instant performance.
        /// This matches the native inventory pattern - no list rebuild, single notification.
        /// </summary>
        private void ApplySortToList()
        {
            if (Heroes == null || Heroes.Count == 0)
                return;

            // Get comparer for the current column and configure sort direction
            var comparer = HeroSorter.GetComparer(_currentSortColumn);
            comparer.SetSortMode(_sortAscending);

            // Sort in-place - no list rebuild, single UI notification
            // This is the native inventory pattern for responsive sorting
            Heroes.Sort(comparer);

            // Filter state (IsFiltered on each VM) is preserved through in-place sort
            UpdateHeroCountStatus();
        }

        /// <summary>
        /// Updates all column header texts with appropriate sort indicators.
        /// </summary>
        private void UpdateSortIndicatorTexts()
        {
            string GetIndicator(HeroSortColumn column)
            {
                if (_currentSortColumn != column)
                    return "";
                return _sortAscending ? SortAscIndicator : SortDescIndicator;
            }

            NameSortIndicatorText = "Name" + GetIndicator(HeroSortColumn.Name);
            GenderSortIndicatorText = "Gender" + GetIndicator(HeroSortColumn.Gender);
            AgeSortIndicatorText = "Age" + GetIndicator(HeroSortColumn.Age);
            ClanSortIndicatorText = "Clan" + GetIndicator(HeroSortColumn.Clan);
            KingdomSortIndicatorText = "Kingdom" + GetIndicator(HeroSortColumn.Kingdom);
            CultureSortIndicatorText = "Culture" + GetIndicator(HeroSortColumn.Culture);
            TypeSortIndicatorText = "Type" + GetIndicator(HeroSortColumn.Type);
            LevelSortIndicatorText = "Level" + GetIndicator(HeroSortColumn.Level);
        }

        #endregion

        #region Private Methods - Filtering

        /// <summary>
        /// Processes deferred filter operations with debounce.
        /// </summary>
        private void ProcessDeferredFilter()
        {
            if (!_filterPending)
                return;

            // Wait for debounce period
            if ((DateTime.UtcNow - _lastFilterChange).TotalMilliseconds < FilterDebounceMs)
                return;

            _filterPending = false;
            ApplyFilter(_pendingFilterText);
        }

        /// <summary>
        /// Applies filter to all heroes by setting their IsFiltered property.
        /// This is the native Inventory pattern - no list rebuilding, just boolean flips.
        /// The UI hides items where IsFiltered = true via IsHidden binding.
        /// </summary>
        private void ApplyFilter(string filter)
        {
            if (Heroes == null || Heroes.Count == 0)
                return;

            // Empty filter - show all
            if (string.IsNullOrWhiteSpace(filter))
            {
                _visibleHeroCount = 0;
                foreach (var hero in Heroes)
                {
                    hero.IsFiltered = false;
                    _visibleHeroCount++;
                }
            }
            else
            {
                // Filter by name (case-insensitive)
                string lowerFilter = filter.ToLowerInvariant();
                _visibleHeroCount = 0;

                foreach (var hero in Heroes)
                {
                    bool matches = hero.Name != null &&
                                   hero.Name.ToLowerInvariant().Contains(lowerFilter);
                    hero.IsFiltered = !matches;

                    if (matches)
                    {
                        _visibleHeroCount++;
                    }
                }
            }

            // Clear selection if selected hero is now filtered
            if (_selectedHero != null && _selectedHero.IsFiltered)
            {
                _selectedHero.IsSelected = false;
                _selectedHero = null;
            }

            UpdateHeroCountStatus();
        }

        #endregion

        #region Private Methods - Utilities

        private static string GetVersionString()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
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
        /// Helper method for MBBindingList property setters
        /// </summary>
        private bool SetProperty(ref MBBindingList<HeroItemVM> field, MBBindingList<HeroItemVM> value, string propertyName)
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
