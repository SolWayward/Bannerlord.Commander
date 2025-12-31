using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Library;
using Bannerlord.GameMaster.Heroes;
using TaleWorlds.CampaignSystem;

namespace Bannerlord.Commander.UI
{
    /// <summary>
    /// Enumeration of available Commander modes
    /// </summary>
    public enum CommanderMode
    {
        Kingdoms,
        Clans,
        Heroes,
        Settlements,
        Troops,
        Items,
        Characters
    }

    /// <summary>
    /// Enumeration of sortable columns in Heroes view
    /// </summary>
    public enum HeroSortColumn
    {
        Name,
        Gender,
        Age,
        Clan,
        Kingdom,
        Culture,
        Type,
        Level
    }

    /// <summary>
    /// ViewModel for the Commander main screen.
    /// Provides data binding for the UI elements.
    /// </summary>
    public class CommanderVM : ViewModel
    {
        // Number of heroes to add per frame during incremental loading
        // Kept low for smooth background loading without UI freeze
        private const int HeroesPerFrame = 20;
        
        // Number of items to add per frame during sort (faster since VMs already exist)
        // Reduced from 50 to 20 to avoid UI stalls during sorting
        private const int SortItemsPerFrame = 20;
        
        private string _titleText;
        private CommanderMode _selectedMode;
        
        // Individual selection state backing fields
        private bool _isKingdomsSelected;
        private bool _isClansSelected;
        private bool _isHeroesSelected;
        private bool _isSettlementsSelected;
        private bool _isTroopsSelected;
        private bool _isItemsSelected;
        private bool _isCharactersSelected;
        private string _selectedModeName;
        
        // Heroes collection for data binding
        private MBBindingList<HeroItemVM> _heroes;
        private HeroItemVM _selectedHero;
        private bool _isLoading;
        private bool _isSorting;
        private bool _needsHeroLoad;
        
        // Incremental loading/sorting state - uses pre-sorted HeroItemVMs
        private List<HeroItemVM> _pendingHeroVMs;
        private int _pendingHeroIndex;
        private string _loadingStatusText;
        
        // Deferred loading parameters
        private string _pendingLoadQuery = "";
        private HeroTypes _pendingLoadHeroTypes = HeroTypes.None;
        private bool _pendingLoadMatchAll = true;
        
        // Track if this is the first time opening (for initial load)
        private bool _hasLoadedOnce;
        
        // Sorting state
        private HeroSortColumn _currentSortColumn = HeroSortColumn.Name;
        private bool _sortAscending = true;

        // Event to notify the screen that close was requested
        public event System.Action OnCloseRequested;

        public CommanderVM()
        {
            // Get version from assembly, fallback to hardcoded if not available
            var version = GetVersionString();
            TitleText = $"COMMANDER {version}";
            
            // Initialize empty heroes collection
            Heroes = new MBBindingList<HeroItemVM>();
            
            // Default to Heroes mode selected
            SelectMode(CommanderMode.Heroes);
            
            // Flag that heroes need to be loaded, but defer until bindings are ready
            _needsHeroLoad = true;
            _hasLoadedOnce = false;
        }
        
        /// <summary>
        /// Called each frame to handle deferred operations and incremental loading/sorting
        /// </summary>
        public void OnTick()
        {
            // Start loading heroes on first tick if needed - by this time all bindings are ready
            if (_needsHeroLoad && _selectedMode == CommanderMode.Heroes)
            {
                _needsHeroLoad = false;
                StartHeroLoading();
            }
            
            // Process incremental hero loading or sorting
            ProcessIncrementalHeroOperation();
        }

        private string GetVersionString()
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
            
            // Fallback to hardcoded version
            return "v1.3.13.1";
        }

        /// <summary>
        /// Central method to handle mode selection and update all related properties
        /// </summary>
        private void SelectMode(CommanderMode mode)
        {
            _selectedMode = mode;
            
            // Update all selection states
            IsKingdomsSelected = mode == CommanderMode.Kingdoms;
            IsClansSelected = mode == CommanderMode.Clans;
            IsHeroesSelected = mode == CommanderMode.Heroes;
            IsSettlementsSelected = mode == CommanderMode.Settlements;
            IsTroopsSelected = mode == CommanderMode.Troops;
            IsItemsSelected = mode == CommanderMode.Items;
            IsCharactersSelected = mode == CommanderMode.Characters;
            
            // Update the selected mode name
            SelectedModeName = mode.ToString();
        }

        #region DataSource Properties

        [DataSourceProperty]
        public string TitleText
        {
            get => _titleText;
            set
            {
                if (_titleText != value)
                {
                    _titleText = value;
                    OnPropertyChangedWithValue(value, nameof(TitleText));
                }
            }
        }

        [DataSourceProperty]
        public string SelectedModeName
        {
            get => _selectedModeName;
            set
            {
                if (_selectedModeName != value)
                {
                    _selectedModeName = value;
                    OnPropertyChangedWithValue(value, nameof(SelectedModeName));
                }
            }
        }

        [DataSourceProperty]
        public bool IsKingdomsSelected
        {
            get => _isKingdomsSelected;
            set
            {
                if (_isKingdomsSelected != value)
                {
                    _isKingdomsSelected = value;
                    OnPropertyChangedWithValue(value, nameof(IsKingdomsSelected));
                }
            }
        }

        [DataSourceProperty]
        public bool IsClansSelected
        {
            get => _isClansSelected;
            set
            {
                if (_isClansSelected != value)
                {
                    _isClansSelected = value;
                    OnPropertyChangedWithValue(value, nameof(IsClansSelected));
                }
            }
        }

        [DataSourceProperty]
        public bool IsHeroesSelected
        {
            get => _isHeroesSelected;
            set
            {
                if (_isHeroesSelected != value)
                {
                    _isHeroesSelected = value;
                    OnPropertyChangedWithValue(value, nameof(IsHeroesSelected));
                }
            }
        }

        [DataSourceProperty]
        public bool IsSettlementsSelected
        {
            get => _isSettlementsSelected;
            set
            {
                if (_isSettlementsSelected != value)
                {
                    _isSettlementsSelected = value;
                    OnPropertyChangedWithValue(value, nameof(IsSettlementsSelected));
                }
            }
        }

        [DataSourceProperty]
        public bool IsTroopsSelected
        {
            get => _isTroopsSelected;
            set
            {
                if (_isTroopsSelected != value)
                {
                    _isTroopsSelected = value;
                    OnPropertyChangedWithValue(value, nameof(IsTroopsSelected));
                }
            }
        }

        [DataSourceProperty]
        public bool IsItemsSelected
        {
            get => _isItemsSelected;
            set
            {
                if (_isItemsSelected != value)
                {
                    _isItemsSelected = value;
                    OnPropertyChangedWithValue(value, nameof(IsItemsSelected));
                }
            }
        }

        [DataSourceProperty]
        public bool IsCharactersSelected
        {
            get => _isCharactersSelected;
            set
            {
                if (_isCharactersSelected != value)
                {
                    _isCharactersSelected = value;
                    OnPropertyChangedWithValue(value, nameof(IsCharactersSelected));
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<HeroItemVM> Heroes
        {
            get => _heroes;
            set
            {
                if (_heroes != value)
                {
                    _heroes = value;
                    OnPropertyChangedWithValue(value, nameof(Heroes));
                }
            }
        }

        #endregion

        #region Hero Data Loading

        /// <summary>
        /// Starts the incremental hero loading process using deferred tick-based loading.
        /// Heroes are pre-sorted before incremental add so they appear in correct order as they load.
        /// </summary>
        private void StartHeroLoading(string query = "", HeroTypes heroTypes = HeroTypes.None, bool matchAll = true)
        {
            if (_isLoading || _isSorting)
                return;

            _isLoading = true;
            _selectedHero = null;
            
            // Store parameters for deferred loading
            _pendingLoadQuery = query;
            _pendingLoadHeroTypes = heroTypes;
            _pendingLoadMatchAll = matchAll;
            
            // Update loading status
            LoadingStatusText = "Preparing...";
            OnPropertyChanged(nameof(IsBusy));
            
            // Query all alive heroes using BLGM
            var rawHeroes = HeroQueries.QueryHeroes(_pendingLoadQuery, _pendingLoadHeroTypes, _pendingLoadMatchAll, includeDead: false);
            
            // Convert ALL to HeroItemVMs immediately (fast - just object creation)
            _pendingHeroVMs = rawHeroes.Select(h => new HeroItemVM(h, this)).ToList();
            
            // PRE-SORT the VMs before starting incremental add
            // This ensures items appear in sorted order as they load (fixes Issue 3)
            SortHeroList(_pendingHeroVMs);
            
            _pendingHeroIndex = 0;
            
            // Create fresh empty Heroes list - this resets scroll state (fixes Issue 1)
            Heroes = new MBBindingList<HeroItemVM>();
            
            UpdateLoadingStatus();
        }
        
        /// <summary>
        /// Starts incremental sorting - prepares sorted list then adds incrementally.
        /// This keeps UI responsive during sort operations (fixes Issue 2).
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
            
            // Extract existing VMs and sort them (fast - VMs already exist)
            _pendingHeroVMs = Heroes.ToList();
            SortHeroList(_pendingHeroVMs);
            _pendingHeroIndex = 0;
            
            // Create fresh empty Heroes list - this resets scroll state
            Heroes = new MBBindingList<HeroItemVM>();
        }
        
        /// <summary>
        /// Processes a batch of heroes each frame for incremental loading or sorting.
        /// Shared logic for both operations since both just add from _pendingHeroVMs.
        /// </summary>
        private void ProcessIncrementalHeroOperation()
        {
            if (!_isLoading && !_isSorting)
                return;
            
            if (_pendingHeroVMs == null)
                return;
            
            // Determine batch size based on operation type
            // Sorting can use larger batches since VMs already exist
            int itemsPerFrame = _isSorting ? SortItemsPerFrame : HeroesPerFrame;
            
            // Calculate how many heroes to process this frame
            int endIndex = System.Math.Min(_pendingHeroIndex + itemsPerFrame, _pendingHeroVMs.Count);
            
            // Add heroes for this frame's batch
            for (int i = _pendingHeroIndex; i < endIndex; i++)
            {
                Heroes.Add(_pendingHeroVMs[i]);
            }
            
            _pendingHeroIndex = endIndex;
            
            // Update status text
            if (_isSorting)
            {
                LoadingStatusText = $"Sorting... {_pendingHeroIndex}/{_pendingHeroVMs.Count}";
            }
            else
            {
                LoadingStatusText = $"Loading... {_pendingHeroIndex}/{_pendingHeroVMs.Count}";
            }
            
            // Check if operation is complete
            if (_pendingHeroIndex >= _pendingHeroVMs.Count)
            {
                CompleteHeroOperation();
            }
        }
        
        /// <summary>
        /// Completes the current loading or sorting operation
        /// </summary>
        private void CompleteHeroOperation()
        {
            bool wasLoading = _isLoading;
            
            _isLoading = false;
            _isSorting = false;
            _pendingHeroVMs = null;
            _pendingHeroIndex = 0;
            LoadingStatusText = "";
            OnPropertyChanged(nameof(IsBusy));
            
            if (wasLoading)
            {
                _hasLoadedOnce = true;
            }
        }
        
        /// <summary>
        /// Updates the loading status text
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
        /// Selects a hero and deselects all others
        /// </summary>
        public void SelectHero(HeroItemVM hero)
        {
            if (_selectedHero == hero)
                return;

            // Deselect previous
            if (_selectedHero != null)
            {
                _selectedHero.IsSelected = false;
            }

            // Select new
            _selectedHero = hero;
            if (_selectedHero != null)
            {
                _selectedHero.IsSelected = true;
            }
        }
        
        [DataSourceProperty]
        public string LoadingStatusText
        {
            get => _loadingStatusText;
            set
            {
                if (_loadingStatusText != value)
                {
                    _loadingStatusText = value;
                    OnPropertyChangedWithValue(value, nameof(LoadingStatusText));
                }
            }
        }
        
        [DataSourceProperty]
        public bool IsLoading
        {
            get => _isLoading;
        }
        
        [DataSourceProperty]
        public bool IsSorting
        {
            get => _isSorting;
        }
        
        [DataSourceProperty]
        public bool IsBusy
        {
            get => _isLoading || _isSorting;
        }

        #endregion

        #region Execute Methods (Button Click Handlers)

        /// <summary>
        /// Called when Kingdoms button is clicked
        /// </summary>
        public void ExecuteSelectKingdoms()
        {
            SelectMode(CommanderMode.Kingdoms);
        }

        /// <summary>
        /// Called when Clans button is clicked
        /// </summary>
        public void ExecuteSelectClans()
        {
            SelectMode(CommanderMode.Clans);
        }

        /// <summary>
        /// Called when Heroes button is clicked
        /// </summary>
        public void ExecuteSelectHeroes()
        {
            SelectMode(CommanderMode.Heroes);
            // Use deferred loading via tick system like initial load
            // This prevents freeze when switching tabs
            if (_hasLoadedOnce)
            {
                // Reset states to allow new load
                // Creating fresh list in StartHeroLoading fixes scrollbar issues (Issue 1)
                _isLoading = false;
                _isSorting = false;
                StartHeroLoading();
            }
        }

        /// <summary>
        /// Called when Settlements button is clicked
        /// </summary>
        public void ExecuteSelectSettlements()
        {
            SelectMode(CommanderMode.Settlements);
        }

        /// <summary>
        /// Called when Troops button is clicked
        /// </summary>
        public void ExecuteSelectTroops()
        {
            SelectMode(CommanderMode.Troops);
        }

        /// <summary>
        /// Called when Items button is clicked
        /// </summary>
        public void ExecuteSelectItems()
        {
            SelectMode(CommanderMode.Items);
        }

        /// <summary>
        /// Called when Characters button is clicked
        /// </summary>
        public void ExecuteSelectCharacters()
        {
            SelectMode(CommanderMode.Characters);
        }

        /// <summary>
        /// Called when Close button is clicked (bound in XML via Command.Click="ExecuteClose")
        /// </summary>
        public void ExecuteClose()
        {
            OnCloseRequested?.Invoke();
        }
        
        /// <summary>
        /// Refreshes data for the current mode.
        /// Called when the menu is reopened to ensure fresh data.
        /// </summary>
        public void RefreshCurrentMode()
        {
            // Only refresh if we've loaded at least once (not the initial load)
            // This prevents double loading on first open
            if (!_hasLoadedOnce)
                return;
                
            switch (_selectedMode)
            {
                case CommanderMode.Heroes:
                    // Force reload heroes to get fresh game state
                    _isLoading = false;
                    _isSorting = false;
                    StartHeroLoading();
                    break;
                // Add other modes as they are implemented
                case CommanderMode.Kingdoms:
                case CommanderMode.Clans:
                case CommanderMode.Settlements:
                case CommanderMode.Troops:
                case CommanderMode.Items:
                case CommanderMode.Characters:
                    // TODO: Implement refresh for other modes when they are added
                    break;
            }
        }

        #endregion

        #region Sorting Methods

        /// <summary>
        /// Sorts a list of HeroItemVM based on current sort column and direction
        /// </summary>
        private void SortHeroList(List<HeroItemVM> list)
        {
            switch (_currentSortColumn)
            {
                case HeroSortColumn.Name:
                    list.Sort((a, b) => _sortAscending
                        ? string.Compare(a.Name, b.Name, System.StringComparison.Ordinal)
                        : string.Compare(b.Name, a.Name, System.StringComparison.Ordinal));
                    break;
                
                case HeroSortColumn.Gender:
                    list.Sort((a, b) => _sortAscending
                        ? string.Compare(a.Gender, b.Gender, System.StringComparison.Ordinal)
                        : string.Compare(b.Gender, a.Gender, System.StringComparison.Ordinal));
                    break;
                
                case HeroSortColumn.Age:
                    list.Sort((a, b) => _sortAscending
                        ? a.Age.CompareTo(b.Age)
                        : b.Age.CompareTo(a.Age));
                    break;
                
                case HeroSortColumn.Clan:
                    list.Sort((a, b) => _sortAscending
                        ? string.Compare(a.Clan, b.Clan, System.StringComparison.Ordinal)
                        : string.Compare(b.Clan, a.Clan, System.StringComparison.Ordinal));
                    break;
                
                case HeroSortColumn.Kingdom:
                    list.Sort((a, b) => _sortAscending
                        ? string.Compare(a.Kingdom, b.Kingdom, System.StringComparison.Ordinal)
                        : string.Compare(b.Kingdom, a.Kingdom, System.StringComparison.Ordinal));
                    break;
                
                case HeroSortColumn.Culture:
                    list.Sort((a, b) => _sortAscending
                        ? string.Compare(a.Culture, b.Culture, System.StringComparison.Ordinal)
                        : string.Compare(b.Culture, a.Culture, System.StringComparison.Ordinal));
                    break;
                
                case HeroSortColumn.Type:
                    list.Sort((a, b) => _sortAscending
                        ? string.Compare(a.HeroType, b.HeroType, System.StringComparison.Ordinal)
                        : string.Compare(b.HeroType, a.HeroType, System.StringComparison.Ordinal));
                    break;
                
                case HeroSortColumn.Level:
                    list.Sort((a, b) => _sortAscending
                        ? a.Level.CompareTo(b.Level)
                        : b.Level.CompareTo(a.Level));
                    break;
            }
        }

        /// <summary>
        /// Handles sorting by a column. If already sorting by this column, toggles direction.
        /// Uses incremental sorting to keep UI responsive (fixes Issue 2).
        /// </summary>
        private void ExecuteSortByColumn(HeroSortColumn column)
        {
            // Don't allow sorting while loading or already sorting
            if (_isLoading || _isSorting)
                return;

            // If clicking the same column, toggle direction
            if (_currentSortColumn == column)
            {
                _sortAscending = !_sortAscending;
            }
            else
            {
                // New column, default to ascending
                _currentSortColumn = column;
                _sortAscending = true;
            }

            // Use incremental sorting instead of instant (fixes Issue 2)
            StartIncrementalSort();
        }

        public void ExecuteSortByName()
        {
            ExecuteSortByColumn(HeroSortColumn.Name);
        }

        public void ExecuteSortByGender()
        {
            ExecuteSortByColumn(HeroSortColumn.Gender);
        }

        public void ExecuteSortByAge()
        {
            ExecuteSortByColumn(HeroSortColumn.Age);
        }

        public void ExecuteSortByClan()
        {
            ExecuteSortByColumn(HeroSortColumn.Clan);
        }

        public void ExecuteSortByKingdom()
        {
            ExecuteSortByColumn(HeroSortColumn.Kingdom);
        }

        public void ExecuteSortByCulture()
        {
            ExecuteSortByColumn(HeroSortColumn.Culture);
        }

        public void ExecuteSortByType()
        {
            ExecuteSortByColumn(HeroSortColumn.Type);
        }

        public void ExecuteSortByLevel()
        {
            ExecuteSortByColumn(HeroSortColumn.Level);
        }

        #endregion

        public override void OnFinalize()
        {
            base.OnFinalize();
            OnCloseRequested = null;
        }
    }

    /// <summary>
    /// ViewModel for individual hero items in the list
    /// Wraps TaleWorlds.CampaignSystem.Hero for UI data binding
    /// </summary>
    public class HeroItemVM : ViewModel
    {
        private readonly Hero _hero;
        private readonly CommanderVM _parentVM;
        private string _name;
        private string _id;
        private string _clan;
        private string _kingdom;
        private string _culture;
        private int _level;
        private string _gender;
        private int _age;
        private string _heroType;
        private bool _isAlive;
        private bool _isSelected;

        public HeroItemVM(Hero hero, CommanderVM parentVM)
        {
            _hero = hero;
            _parentVM = parentVM;
            
            // Initialize display properties from Hero
            Name = hero.Name?.ToString() ?? "Unknown";
            Id = hero.StringId ?? "";
            Clan = hero.Clan?.Name?.ToString() ?? "None";
            Kingdom = hero.Clan?.Kingdom?.Name?.ToString() ?? "None";
            Culture = hero.Culture?.Name?.ToString() ?? "Unknown";
            Level = hero.Level;
            Gender = hero.IsFemale ? "Female" : "Male";
            Age = (int)hero.Age;
            HeroType = DetermineHeroType(hero);
            IsAlive = hero.IsAlive;
            IsSelected = false;
        }

        /// <summary>
        /// Determines the display type for a hero based on their properties
        /// </summary>
        private static string DetermineHeroType(Hero hero)
        {
            // Check child first - children are always children regardless of other status
            if (hero.IsChild) return "Child";
            
            // Check faction types - bandit faction heroes
            if (hero.Clan?.IsBanditFaction == true) return "Bandit";
            
            // Check minor faction - these are typically mercenary/minor faction lords
            if (hero.Clan?.IsMinorFaction == true) return "Minor Faction";
            
            // Standard hero types
            if (hero.IsLord) return "Lord";
            if (hero.IsWanderer) return "Wanderer";
            if (hero.IsNotable) return "Notable";
            
            // Fallback for any other hero type
            return "Other";
        }

        /// <summary>
        /// Gets the underlying Hero object
        /// </summary>
        public Hero Hero => _hero;

        /// <summary>
        /// Called when this hero row is clicked
        /// </summary>
        public void ExecuteSelect()
        {
            _parentVM?.SelectHero(this);
        }

        [DataSourceProperty]
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChangedWithValue(value, nameof(Name));
                }
            }
        }

        [DataSourceProperty]
        public string Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChangedWithValue(value, nameof(Id));
                }
            }
        }

        [DataSourceProperty]
        public string Clan
        {
            get => _clan;
            set
            {
                if (_clan != value)
                {
                    _clan = value;
                    OnPropertyChangedWithValue(value, nameof(Clan));
                }
            }
        }

        [DataSourceProperty]
        public string Kingdom
        {
            get => _kingdom;
            set
            {
                if (_kingdom != value)
                {
                    _kingdom = value;
                    OnPropertyChangedWithValue(value, nameof(Kingdom));
                }
            }
        }

        [DataSourceProperty]
        public string Culture
        {
            get => _culture;
            set
            {
                if (_culture != value)
                {
                    _culture = value;
                    OnPropertyChangedWithValue(value, nameof(Culture));
                }
            }
        }

        [DataSourceProperty]
        public int Level
        {
            get => _level;
            set
            {
                if (_level != value)
                {
                    _level = value;
                    OnPropertyChangedWithValue(value, nameof(Level));
                    OnPropertyChangedWithValue(value.ToString(), nameof(LevelText));
                }
            }
        }

        [DataSourceProperty]
        public string LevelText
        {
            get => _level.ToString();
        }

        [DataSourceProperty]
        public string Gender
        {
            get => _gender;
            set
            {
                if (_gender != value)
                {
                    _gender = value;
                    OnPropertyChangedWithValue(value, nameof(Gender));
                }
            }
        }

        [DataSourceProperty]
        public int Age
        {
            get => _age;
            set
            {
                if (_age != value)
                {
                    _age = value;
                    OnPropertyChangedWithValue(value, nameof(Age));
                    OnPropertyChangedWithValue(value.ToString(), nameof(AgeText));
                }
            }
        }

        [DataSourceProperty]
        public string AgeText
        {
            get => _age.ToString();
        }

        [DataSourceProperty]
        public string HeroType
        {
            get => _heroType;
            set
            {
                if (_heroType != value)
                {
                    _heroType = value;
                    OnPropertyChangedWithValue(value, nameof(HeroType));
                }
            }
        }

        [DataSourceProperty]
        public bool IsAlive
        {
            get => _isAlive;
            set
            {
                if (_isAlive != value)
                {
                    _isAlive = value;
                    OnPropertyChangedWithValue(value, nameof(IsAlive));
                }
            }
        }

        [DataSourceProperty]
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChangedWithValue(value, nameof(IsSelected));
                }
            }
        }
    }
}
