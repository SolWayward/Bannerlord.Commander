using System;
using System.Collections.Generic;
using Bannerlord.GameMaster.Heroes;
using Bannerlord.Commander.UI.Enums;
using Bannerlord.Commander.UI.Services;
using Bannerlord.Commander.UI.ViewModels.Base;
using TaleWorlds.Library;
using TaleWorlds.CampaignSystem;

namespace Bannerlord.Commander.UI.ViewModels.HeroMode
{
    /// <summary>
    /// ViewModel for managing the hero list with sorting, filtering, and selection.
    /// Encapsulates all hero list management logic from CommanderVM.
    /// 
    /// Uses the IsFiltered property pattern (like native Inventory):
    /// - Heroes are never removed from the list
    /// - IsFiltered toggles visibility via IsHidden binding in XML
    /// - This avoids expensive list rebuilds and keeps UI smooth
    /// </summary>
    public class HeroListVM : CommanderListVMBase<HeroListItemVM>, IHeroSelectionHandler
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
        /// 15ms works well with native IsFiltered pattern for responsive filtering.
        /// </summary>
        private const int FilterDebounceMs = 15;

        #endregion

        #region Private Fields - Hero Collection

        private MBBindingList<HeroListItemVM> _heroes;
        private HeroListItemVM _selectedHero;

        #endregion

        #region Private Fields - Loading State

        private bool _isLoading;
        private bool _needsHeroLoad;
        private bool _hasLoadedOnce;
        private string _loadingStatusText;

        /// <summary>
        /// List of pre-created and pre-sorted HeroListItemVMs to be incrementally added to the UI.
        /// VMs are created and sorted all at once (fast), then added incrementally for smooth display.
        /// </summary>
        private List<HeroListItemVM> _pendingHeroVMs;
        private int _pendingHeroIndex;

        /// <summary>
        /// Parameters for deferred hero loading
        /// </summary>
        private string _pendingLoadQuery = "";
        private HeroTypes _pendingLoadHeroTypes = HeroTypes.None;
        private bool _pendingLoadMatchAll = true;
        private bool _pendingQueryExecution;

        #endregion

        #region Private Fields - Sorting

        private HeroSortColumn _currentSortColumn = HeroSortColumn.Name;
        private bool _sortAscending = true;

        private string _nameSortIndicatorText;
        private string _genderSortIndicatorText;
        private string _ageSortIndicatorText;
        private string _clanSortIndicatorText;
        private string _kingdomSortIndicatorText;
        private string _cultureSortIndicatorText;
        private string _typeSortIndicatorText;
        private string _levelSortIndicatorText;

        #endregion

        #region Private Fields - Filtering

        private string _filterText = "";
        private string _pendingFilterText;
        private bool _filterPending;
        private DateTime _lastFilterChange = DateTime.MinValue;
        private int _visibleHeroCount;

        #endregion

        #region Base Class Overrides

        protected override MBBindingList<HeroListItemVM> GetCurrentList() => Heroes;
        protected override HeroListItemVM GetSelectedItem() => _selectedHero;
        protected override void SelectItem(HeroListItemVM item) => SelectHero(item);
        protected override bool IsItemVisible(HeroListItemVM item) => item != null && !item.IsFiltered;

        #endregion

        #region Events

        /// <summary>
        /// Event to notify when filter text changes (for input restriction management)
        /// </summary>
        public event Action OnFilterTextChanged;

        /// <summary>
        /// Fired when a hero is selected (implements hero-editor coordination)
        /// </summary>
        public event Action<HeroListItemVM> HeroSelected;

        #endregion

        #region Constructor

        public HeroListVM()
        {
            Heroes = new MBBindingList<HeroListItemVM>();

            // Initialize sort indicator texts
            UpdateSortIndicatorTexts();

            // Flag that heroes need to be loaded, but defer until bindings are ready
            _needsHeroLoad = true;
            _hasLoadedOnce = false;
        }

        #endregion

        #region Public Methods - Lifecycle

        /// <summary>
        /// Called each frame to handle deferred operations and incremental loading.
        /// Processes all pending operations: hero load, query, incremental loading, filter.
        /// </summary>
        public void OnTick()
        {
            ProcessDeferredHeroLoad();
            ProcessDeferredQuery();
            ProcessIncrementalHeroLoading();
            ProcessDeferredFilter();
        }

        /// <summary>
        /// Refresh the hero list by reloading heroes when the mode becomes visible
        /// </summary>
        public void RefreshCurrentMode()
        {
            _needsHeroLoad = true;
            _hasLoadedOnce = false;
        }

        #endregion

        #region Public Methods - Selection (IHeroSelectionHandler)

        /// <summary>
        /// Selects a hero and deselects all others.
        /// Fires HeroSelected event to notify subscribers (e.g., HeroesModeVM).
        /// </summary>
        public void SelectHero(HeroListItemVM hero)
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

            OnPropertyChanged(nameof(IsHeroSelected));
            HeroSelected?.Invoke(hero);
        }

        #endregion

        #region Public Methods - Sorting

        public void ExecuteSortByName() => ExecuteSortByColumn(HeroSortColumn.Name);
        public void ExecuteSortByGender() => ExecuteSortByColumn(HeroSortColumn.Gender);
        public void ExecuteSortByAge() => ExecuteSortByColumn(HeroSortColumn.Age);
        public void ExecuteSortByClan() => ExecuteSortByColumn(HeroSortColumn.Clan);
        public void ExecuteSortByKingdom() => ExecuteSortByColumn(HeroSortColumn.Kingdom);
        public void ExecuteSortByCulture() => ExecuteSortByColumn(HeroSortColumn.Culture);
        public void ExecuteSortByType() => ExecuteSortByColumn(HeroSortColumn.Type);
        public void ExecuteSortByLevel() => ExecuteSortByColumn(HeroSortColumn.Level);

        #endregion

        #region DataSource Properties - Hero Collection

        [DataSourceProperty]
        public MBBindingList<HeroListItemVM> Heroes
        {
            get => _heroes;
            set => SetProperty(ref _heroes, value, nameof(Heroes));
        }

        [DataSourceProperty]
        public bool IsHeroSelected => _selectedHero != null;

        #endregion

        #region DataSource Properties - Loading

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

        #endregion

        #region DataSource Properties - Sorting

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

        #endregion

        #region DataSource Properties - Filtering

        /// <summary>
        /// Gets the count of visible heroes (after filtering)
        /// </summary>
        [DataSourceProperty]
        public int VisibleHeroCount => _visibleHeroCount;

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

                    // Schedule deferred filter with debounce
                    _pendingFilterText = _filterText;
                    _filterPending = true;
                    _lastFilterChange = DateTime.UtcNow;

                    // Notify that filter text changed (for input restriction management)
                    OnFilterTextChanged?.Invoke();
                }
            }
        }

        #endregion

        #region Private Methods - Hero Loading

        /// <summary>
        /// Processes the deferred hero load flag.
        /// Initiates hero loading when _needsHeroLoad is true.
        /// </summary>
        private void ProcessDeferredHeroLoad()
        {
            if (!_needsHeroLoad)
                return;

            _needsHeroLoad = false;
            StartHeroLoading();
        }

        /// <summary>
        /// Processes the deferred query execution.
        /// Executes the actual hero query when _pendingQueryExecution is true.
        /// </summary>
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

            Heroes = new MBBindingList<HeroListItemVM>();
            _isLoading = true;
            _selectedHero = null;
            LoadingStatusText = "Loading...";

            _pendingQueryExecution = true;
        }

        /// <summary>
        /// Executes the deferred hero query.
        /// Called on next tick after StartHeroLoading to avoid blocking.
        /// Creates ALL ViewModels at once and sorts them BEFORE incremental display.
        /// This ensures heroes appear in correct order from the start with no visible resort.
        /// </summary>
        private void ExecuteDeferredQuery()
        {
            // Get raw heroes (this is the slow part - done once)
            List<Hero> rawHeroes = HeroQueries.QueryHeroes(
                _pendingLoadQuery,
                _pendingLoadHeroTypes,
                _pendingLoadMatchAll,
                includeDead: false);

            // Create ALL VMs at once (this is fast - the query is the slow part)
            _pendingHeroVMs = new List<HeroListItemVM>(rawHeroes.Count);
            foreach (Hero hero in rawHeroes)
            {
                _pendingHeroVMs.Add(new HeroListItemVM(hero, this));
            }

            // Sort BEFORE adding to UI - ensures correct order from start, no visible resort
            HeroSorter.Sort(_pendingHeroVMs, _currentSortColumn, _sortAscending);

            _pendingHeroIndex = 0;
            UpdateLoadingStatus();
        }

        /// <summary>
        /// Processes a batch of pre-sorted VMs each frame for incremental display.
        /// VMs are already created and sorted - this just adds them to the UI list incrementally.
        /// This ensures smooth, progressive loading with heroes appearing in correct order.
        /// </summary>
        private void ProcessIncrementalHeroLoading()
        {
            if (!_isLoading || _pendingHeroVMs == null)
                return;

            int endIndex = Math.Min(_pendingHeroIndex + HeroesPerFrame, _pendingHeroVMs.Count);

            // Add pre-sorted VMs to UI list
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

        /// <summary>
        /// Finalizes hero loading and applies initial filter if needed.
        /// Note: NO sort here - VMs were pre-sorted before incremental display.
        /// </summary>
        private void CompleteHeroLoading()
        {
            _isLoading = false;
            _pendingHeroVMs = null;
            _pendingHeroIndex = 0;
            _hasLoadedOnce = true;

            // DO NOT sort here - already sorted before incremental load
            // This eliminates the jarring visible reorder after loading completes

            // Apply initial filter if there's existing filter text
            if (!string.IsNullOrEmpty(_filterText))
            {
                ApplyFilter(_filterText);
            }
            else
            {
                _visibleHeroCount = Heroes.Count;
                OnPropertyChanged(nameof(VisibleHeroCount));
            }

            UpdateHeroCountStatus();
            OnPropertyChanged(nameof(IsBusy));
        }

        /// <summary>
        /// Updates the loading status text to show progress.
        /// </summary>
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
            HeroComparer comparer = HeroSorter.GetComparer(_currentSortColumn);
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
        /// Waits for FilterDebounceMs after last filter change before applying.
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
                foreach (HeroListItemVM hero in Heroes)
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

                foreach (HeroListItemVM hero in Heroes)
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

            OnPropertyChanged(nameof(VisibleHeroCount));
            UpdateHeroCountStatus();
        }

        #endregion

        #region Private Methods - Utilities

        /// <summary>
        /// Helper method to reduce boilerplate in string property setters.
        /// Uses OnPropertyChanged to ensure PropertyChanged event is raised for subscribers.
        /// </summary>
        private bool SetProperty(ref string field, string value, string propertyName)
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
        private bool SetProperty(ref MBBindingList<HeroListItemVM> field, MBBindingList<HeroListItemVM> value, string propertyName)
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
