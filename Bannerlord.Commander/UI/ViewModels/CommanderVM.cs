using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bannerlord.Commander.UI.Enums;
using Bannerlord.Commander.UI.Services;
using Bannerlord.GameMaster.Heroes;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;

namespace Bannerlord.Commander.UI.ViewModels
{
    /// <summary>
    /// ViewModel for the Commander main screen.
    /// Provides data binding for the UI elements.
    /// </summary>
    public class CommanderVM : ViewModel, IHeroSelectionHandler
    {
        #region Constants

        /// <summary>
        /// Number of heroes to add per frame during incremental loading.
        /// Kept low for smooth background loading without UI freeze.
        /// </summary>
        private const int HeroesPerFrame = 20;

        /// <summary>
        /// Number of items to add per frame during sort (faster since VMs already exist).
        /// </summary>
        private const int SortItemsPerFrame = 50;

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
        /// Used to trigger background filtering after user stops typing.
        /// </summary>
        private const int FilterDebounceMs = 200;

        /// <summary>
        /// Number of items to show INSTANTLY after filter completes.
        /// This ensures the user sees results immediately without any lag.
        /// </summary>
        private const int InstantResultCount = 40;

        /// <summary>
        /// Number of items to add per frame during incremental filter population.
        /// Only runs when user is idle (not typing) to ensure smooth typing experience.
        /// </summary>
        private const int StreamBatchSize = 25;

        /// <summary>
        /// Time in ms to wait after last input before streaming more results.
        /// While user is typing, we pause streaming to ensure 60fps textbox experience.
        /// </summary>
        private const int TypingDebounceMs = 200;

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

        // Heroes collection
        private MBBindingList<HeroItemVM> _heroes;
        private List<HeroItemVM> _allHeroes; // Unfiltered master list
        private HeroItemVM _selectedHero;

        // Loading state
        private bool _isLoading;
        private bool _isSorting;
        private bool _isFiltering;
        private bool _needsHeroLoad;
        private bool _hasLoadedOnce;
        private string _loadingStatusText;

        // Incremental loading state - used for both initial load and filter results
        private List<HeroItemVM> _pendingHeroVMs;
        private int _pendingHeroIndex;
        private int _totalHeroCount;

        // Incremental filter population state
        private bool _incrementalFilterActive;
        private List<HeroItemVM> _pendingFilteredHeroes;
        private int _pendingFilterIndex;

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

        // Filter state - per mode storage
        private string _filterText = "";
        private readonly Dictionary<CommanderMode, string> _filterTextByMode = new Dictionary<CommanderMode, string>();
        private string _pendingFilterText;
        private bool _filterPending;
        private DateTime _lastFilterChange = DateTime.MinValue;

        // Background filter state - filtering runs on background thread to avoid UI stutter
        private CancellationTokenSource _filterCts;
        private volatile bool _backgroundFilterComplete;
        private List<HeroItemVM> _backgroundFilterResult;
        private readonly object _filterResultLock = new object();

        // Search versioning - increments on every filter change to invalidate old searches
        private int _currentSearchVersion = 0;

        // Last input time - used for debouncing during incremental population
        private DateTime _lastInputTime = DateTime.MinValue;

        // Per-mode hero list cache to avoid reloading when switching tabs
        private readonly Dictionary<CommanderMode, List<HeroItemVM>> _heroListByMode = new Dictionary<CommanderMode, List<HeroItemVM>>();
        private readonly Dictionary<CommanderMode, List<HeroItemVM>> _filteredHeroListByMode = new Dictionary<CommanderMode, List<HeroItemVM>>();

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
            Heroes = new MBBindingList<HeroItemVM>();

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
        /// Called each frame to handle deferred operations and incremental loading/sorting
        /// </summary>
        public void OnTick()
        {
            ProcessDeferredHeroLoad();
            ProcessDeferredQuery();
            ProcessIncrementalHeroOperation();
            ProcessDeferredFilter();
            ProcessBackgroundFilterResult();
            ProcessIncrementalFilterPopulation();
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
                ResetLoadingState();
                StartHeroLoading();
            }
            // TODO: Add refresh for other modes as implemented
        }

        /// <summary>
        /// Selects a hero and deselects all others
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
            }
        }

        public override void OnFinalize()
        {
            // Cancel any pending background filter operation
            CancelPendingFilter();

            base.OnFinalize();
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
        public bool IsSorting => _isSorting;

        [DataSourceProperty]
        public bool IsBusy => _isLoading || _isSorting || _isFiltering;

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

                    // Record time for debounce logic during incremental population
                    _lastInputTime = DateTime.UtcNow;

                    // Increment search version - invalidates any old searches currently running
                    _currentSearchVersion++;

                    // IMMEDIATE STOP: Halt any list streaming from previous searches
                    // This keeps the UI responsive for typing - no stutters from old results streaming in
                    _incrementalFilterActive = false;
                    _pendingFilteredHeroes = null;

                    // Schedule deferred filter with debounce
                    _pendingFilterText = _filterText;
                    _filterPending = true;
                    _lastFilterChange = DateTime.UtcNow;
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
            // Don't reload - just restore from cache if available
            if (_hasLoadedOnce && _heroListByMode.TryGetValue(CommanderMode.Heroes, out var cachedAllHeroes))
            {
                _allHeroes = cachedAllHeroes;

                // Restore filtered list if available, otherwise apply current filter
                if (_filteredHeroListByMode.TryGetValue(CommanderMode.Heroes, out var cachedFiltered) && cachedFiltered != null)
                {
                    // Populate list BEFORE assigning to property to avoid multiple UI notifications
                    var newList = new MBBindingList<HeroItemVM>();
                    foreach (var hero in cachedFiltered)
                    {
                        newList.Add(hero);
                    }
                    Heroes = newList;  // Single UI update
                    UpdateHeroCountStatus();
                }
                else if (!string.IsNullOrEmpty(_filterText))
                {
                    StartBackgroundFilter(_filterText);
                }
                else
                {
                    // No filter - show all heroes from cache
                    // Populate list BEFORE assigning to property to avoid multiple UI notifications
                    var newList = new MBBindingList<HeroItemVM>();
                    foreach (var hero in cachedAllHeroes)
                    {
                        newList.Add(hero);
                    }
                    Heroes = newList;  // Single UI update
                    UpdateHeroCountStatus();
                }
            }
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
            if (_isLoading || _isSorting)
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
            _totalHeroCount = _pendingHeroVMs.Count;

            HeroSorter.Sort(_pendingHeroVMs, _currentSortColumn, _sortAscending);

            _pendingHeroIndex = 0;
            UpdateLoadingStatus();
        }

        /// <summary>
        /// Processes a batch of heroes each frame for incremental loading or sorting.
        /// </summary>
        private void ProcessIncrementalHeroOperation()
        {
            if (!_isLoading && !_isSorting)
                return;

            if (_pendingHeroVMs == null)
                return;

            int itemsPerFrame = _isSorting ? SortItemsPerFrame : HeroesPerFrame;
            int endIndex = Math.Min(_pendingHeroIndex + itemsPerFrame, _pendingHeroVMs.Count);

            for (int i = _pendingHeroIndex; i < endIndex; i++)
            {
                Heroes.Add(_pendingHeroVMs[i]);
            }

            _pendingHeroIndex = endIndex;

            string operation = _isSorting ? "Sorting" : "Loading";
            LoadingStatusText = $"{operation}... {_pendingHeroIndex}/{_pendingHeroVMs.Count}";

            if (_pendingHeroIndex >= _pendingHeroVMs.Count)
            {
                CompleteHeroOperation();
            }
        }

        private void CompleteHeroOperation()
        {
            bool wasLoading = _isLoading;

            _isLoading = false;
            _isSorting = false;
            _isFiltering = false;

            // Store unfiltered master list when loading completes
            if (wasLoading)
            {
                _allHeroes = Heroes.ToList();
                // Cache the hero list for this mode
                _heroListByMode[_selectedMode] = _allHeroes;
                // Clear any cached filtered list since we just loaded fresh
                _filteredHeroListByMode.Remove(_selectedMode);
            }

            _pendingHeroVMs = null;
            _pendingHeroIndex = 0;

            // Show total count when operation completes
            UpdateHeroCountStatus();

            OnPropertyChanged(nameof(IsBusy));

            if (wasLoading)
            {
                _hasLoadedOnce = true;

                // Apply any existing filter after loading completes
                if (!string.IsNullOrEmpty(_filterText))
                {
                    StartBackgroundFilter(_filterText);
                }
            }
        }

        /// <summary>
        /// Updates the status text to show current hero count with filter info if applicable.
        /// </summary>
        private void UpdateHeroCountStatus()
        {
            if (_allHeroes != null && !string.IsNullOrEmpty(_filterText))
            {
                LoadingStatusText = $"{Heroes.Count} / {_allHeroes.Count} Heroes";
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

        private void ResetLoadingState()
        {
            _isLoading = false;
            _isSorting = false;
        }

        #endregion

        #region Private Methods - Sorting

        /// <summary>
        /// Starts incremental sorting - prepares sorted list then adds incrementally.
        /// </summary>
        private void StartIncrementalSort()
        {
            if (_isLoading || _isSorting)
                return;

            if (Heroes == null || Heroes.Count == 0)
                return;

            _isSorting = true;
            LoadingStatusText = "Sorting...";
            OnPropertyChanged(nameof(IsBusy));

            _pendingHeroVMs = Heroes.ToList();
            HeroSorter.Sort(_pendingHeroVMs, _currentSortColumn, _sortAscending);
            _pendingHeroIndex = 0;

            Heroes = new MBBindingList<HeroItemVM>();
        }

        /// <summary>
        /// Handles sorting by a column. If already sorting by this column, toggles direction.
        /// </summary>
        private void ExecuteSortByColumn(HeroSortColumn column)
        {
            if (_isLoading || _isSorting)
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
            StartIncrementalSort();
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
        /// Cancels any pending background filter operation.
        /// </summary>
        private void CancelPendingFilter()
        {
            if (_filterCts != null)
            {
                _filterCts.Cancel();
                _filterCts.Dispose();
                _filterCts = null;
            }
        }

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
            StartBackgroundFilter(_pendingFilterText);
        }

        /// <summary>
        /// Checks if a background filter has completed and applies the results on the main thread.
        /// </summary>
        private void ProcessBackgroundFilterResult()
        {
            if (!_backgroundFilterComplete)
                return;

            List<HeroItemVM> result;
            lock (_filterResultLock)
            {
                if (!_backgroundFilterComplete || _backgroundFilterResult == null)
                    return;

                result = _backgroundFilterResult;
                _backgroundFilterResult = null;
                _backgroundFilterComplete = false;
            }

            // Apply results on main thread
            ApplyFilterResults(result);
        }

        /// <summary>
        /// Starts filtering on a background thread to avoid UI stutter.
        /// The filtering and sorting happen off the main thread, then results are applied on the main thread.
        /// Uses search versioning to cancel outdated searches automatically.
        /// </summary>
        private void StartBackgroundFilter(string filter)
        {
            // Don't start new filter if busy with other operations
            if (_isLoading || _isSorting)
                return;

            if (_allHeroes == null || _allHeroes.Count == 0)
                return;

            // Cancel any previous filter operation
            CancelPendingFilter();

            _isFiltering = true;
            OnPropertyChanged(nameof(IsBusy));

            // Capture state for the background thread (thread-safe snapshot)
            int searchVersion = _currentSearchVersion;
            var heroesSnapshot = _allHeroes.ToList();
            var sortColumn = _currentSortColumn;
            var sortAscending = _sortAscending;

            // Create new cancellation token
            _filterCts = new CancellationTokenSource();
            var token = _filterCts.Token;

            // Run filter on background thread
            Task.Run(() =>
            {
                try
                {
                    // Fast fail: If user typed again already, don't bother starting
                    if (searchVersion != _currentSearchVersion)
                        return;

                    List<HeroItemVM> filtered;

                    if (string.IsNullOrWhiteSpace(filter))
                    {
                        // No filter - show all heroes
                        filtered = heroesSnapshot.ToList();
                    }
                    else
                    {
                        // Filter by name (case-insensitive)
                        string lowerFilter = filter.ToLowerInvariant();
                        filtered = heroesSnapshot
                            .Where(h => h.Name != null && h.Name.ToLowerInvariant().Contains(lowerFilter))
                            .ToList();
                    }

                    // Check for cancellation or version mismatch before sorting
                    if (token.IsCancellationRequested || searchVersion != _currentSearchVersion)
                        return;

                    // Sort the filtered list
                    HeroSorter.Sort(filtered, sortColumn, sortAscending);

                    // Check for cancellation or version mismatch before delivering result
                    if (token.IsCancellationRequested || searchVersion != _currentSearchVersion)
                        return;

                    // Store result for main thread to pick up ONLY if version still matches
                    lock (_filterResultLock)
                    {
                        if (searchVersion == _currentSearchVersion)
                        {
                            _backgroundFilterResult = filtered;
                            _backgroundFilterComplete = true;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Filter was cancelled, ignore
                }
                catch (Exception)
                {
                    // Log error if needed, but don't crash
                    lock (_filterResultLock)
                    {
                        _backgroundFilterComplete = false;
                        _backgroundFilterResult = null;
                    }
                }
            }, token);
        }

        /// <summary>
        /// Applies filter results to the UI using the "Smooth Filtering" pattern.
        /// Shows the first batch (InstantResultCount) immediately, then streams the rest
        /// incrementally when the user is idle (not typing).
        /// </summary>
        private void ApplyFilterResults(List<HeroItemVM> filtered)
        {
            // Clear selection if selected hero is no longer visible
            if (_selectedHero != null && !filtered.Contains(_selectedHero))
            {
                _selectedHero.IsSelected = false;
                _selectedHero = null;
            }

            // Cache the filtered result for this mode
            _filteredHeroListByMode[_selectedMode] = filtered;

            // THE SMOOTH PART: Create a new list with ONLY the first batch (e.g., 40 items)
            // This happens instantly on the main thread, so the user sees results immediately.
            var newList = new MBBindingList<HeroItemVM>();
            int countToAdd = Math.Min(filtered.Count, InstantResultCount);
            for (int i = 0; i < countToAdd; i++)
            {
                newList.Add(filtered[i]);
            }

            // Update the UI with the first batch
            Heroes = newList;

            // Queue the rest for "streaming" if there are more items
            if (filtered.Count > InstantResultCount)
            {
                _pendingFilteredHeroes = filtered;
                _pendingFilterIndex = InstantResultCount; // Start where we left off
                _incrementalFilterActive = true;
                // _isFiltering stays true until incremental population completes
            }
            else
            {
                // All items fit in first batch - done
                _incrementalFilterActive = false;
                _pendingFilteredHeroes = null;
                _isFiltering = false;
                OnPropertyChanged(nameof(IsBusy));
            }

            // Show status with total count
            LoadingStatusText = $"{filtered.Count} Heroes found";
        }

        /// <summary>
        /// Processes incremental filter result population.
        /// Adds a batch of filtered heroes each frame, but ONLY when user is idle.
        /// This ensures the textbox remains 60fps smooth while typing.
        /// </summary>
        private void ProcessIncrementalFilterPopulation()
        {
            if (!_incrementalFilterActive || _pendingFilteredHeroes == null)
                return;

            // DEBOUNCE: If user typed within the last TypingDebounceMs, wait.
            // This ensures the textbox cursor remains 60fps smooth while typing.
            double msSinceLastInput = (DateTime.UtcNow - _lastInputTime).TotalMilliseconds;
            if (msSinceLastInput < TypingDebounceMs)
            {
                return;
            }

            // User is idle - stream in a small batch
            int endIndex = Math.Min(_pendingFilterIndex + StreamBatchSize, _pendingFilteredHeroes.Count);

            for (int i = _pendingFilterIndex; i < endIndex; i++)
            {
                Heroes.Add(_pendingFilteredHeroes[i]);
            }

            _pendingFilterIndex = endIndex;

            // Check if complete
            if (_pendingFilterIndex >= _pendingFilteredHeroes.Count)
            {
                _incrementalFilterActive = false;
                _pendingFilteredHeroes = null;
                _pendingFilterIndex = 0;

                _isFiltering = false;
                OnPropertyChanged(nameof(IsBusy));
                UpdateHeroCountStatus();
            }
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
