using System.Reflection;
using TaleWorlds.Library;

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

        // Event to notify the screen that close was requested
        public event System.Action OnCloseRequested;

        public CommanderVM()
        {
            // Get version from assembly, fallback to hardcoded if not available
            var version = GetVersionString();
            TitleText = $"COMMANDER {version}";
            
            // Default to Kingdoms mode selected
            SelectMode(CommanderMode.Kingdoms);
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
}
