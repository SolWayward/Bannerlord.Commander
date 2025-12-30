using System.Reflection;
using System.Collections.Generic;
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
    /// ViewModel for the Commander main screen.
    /// Provides data binding for the UI elements.
    /// </summary>
    public class CommanderVM : ViewModel
    {
        // Number of heroes to add per frame during incremental loading
        private const int HeroesPerFrame = 50;
        
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
        private bool _needsHeroLoad;
        
        // Incremental loading state
        private List<Hero> _pendingHeroes;
        private int _pendingHeroIndex;
        private string _loadingStatusText;

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
        }
        
        /// <summary>
        /// Called each frame to handle deferred operations and incremental loading
        /// </summary>
        public void OnTick()
        {
            // Start loading heroes on first tick if needed - by this time all bindings are ready
            if (_needsHeroLoad && _selectedMode == CommanderMode.Heroes)
            {
                _needsHeroLoad = false;
                StartHeroLoading();
            }
            
            // Process incremental hero loading
            ProcessIncrementalHeroLoading();
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
        /// Starts the incremental hero loading process
        /// </summary>
        private void StartHeroLoading(string query = "", HeroTypes heroTypes = HeroTypes.None, bool matchAll = true)
        {
            if (_isLoading)
                return;

            _isLoading = true;
            _selectedHero = null;
            
            // Clear the existing list
            Heroes.Clear();

            // Query all alive heroes using BLGM - this is fast, just creates the list
            _pendingHeroes = HeroQueries.QueryHeroes(query, heroTypes, matchAll, includeDead: false);
            _pendingHeroIndex = 0;
            
            // Update loading status
            UpdateLoadingStatus();
        }
        
        /// <summary>
        /// Processes a batch of heroes each frame for incremental loading
        /// </summary>
        private void ProcessIncrementalHeroLoading()
        {
            if (!_isLoading || _pendingHeroes == null)
                return;
            
            // Calculate how many heroes to process this frame
            int endIndex = System.Math.Min(_pendingHeroIndex + HeroesPerFrame, _pendingHeroes.Count);
            
            // Add heroes for this frame's batch
            for (int i = _pendingHeroIndex; i < endIndex; i++)
            {
                Heroes.Add(new HeroItemVM(_pendingHeroes[i], this));
            }
            
            _pendingHeroIndex = endIndex;
            
            // Update loading status
            UpdateLoadingStatus();
            
            // Check if loading is complete
            if (_pendingHeroIndex >= _pendingHeroes.Count)
            {
                _isLoading = false;
                _pendingHeroes = null;
                _pendingHeroIndex = 0;
                LoadingStatusText = "";
            }
        }
        
        /// <summary>
        /// Updates the loading status text
        /// </summary>
        private void UpdateLoadingStatus()
        {
            if (_pendingHeroes != null && _pendingHeroes.Count > 0)
            {
                LoadingStatusText = $"Loading... {_pendingHeroIndex}/{_pendingHeroes.Count}";
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
            // Start incremental hero loading when Heroes mode is selected
            StartHeroLoading();
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
            IsAlive = hero.IsAlive;
            IsSelected = false;
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
