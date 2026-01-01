using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.Commander.UI.ViewModels
{
    /// <summary>
    /// ViewModel for individual hero items in the list.
    /// Wraps TaleWorlds.CampaignSystem.Hero for UI data binding.
    /// </summary>
    public class HeroItemVM : ViewModel
    {
        private readonly Hero _hero;
        private readonly IHeroSelectionHandler _selectionHandler;
        
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
        private bool _isFiltered;

        /// <summary>
        /// Creates a new HeroItemVM wrapping the specified Hero
        /// </summary>
        /// <param name="hero">The Hero to wrap</param>
        /// <param name="selectionHandler">Handler for selection events</param>
        public HeroItemVM(Hero hero, IHeroSelectionHandler selectionHandler)
        {
            _hero = hero;
            _selectionHandler = selectionHandler;
            
            InitializeFromHero(hero);
        }

        /// <summary>
        /// Initializes all display properties from the Hero object
        /// </summary>
        private void InitializeFromHero(Hero hero)
        {
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
            _selectionHandler?.SelectHero(this);
        }

        #region DataSource Properties

        [DataSourceProperty]
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value, nameof(Name));
        }

        [DataSourceProperty]
        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value, nameof(Id));
        }

        [DataSourceProperty]
        public string Clan
        {
            get => _clan;
            set => SetProperty(ref _clan, value, nameof(Clan));
        }

        [DataSourceProperty]
        public string Kingdom
        {
            get => _kingdom;
            set => SetProperty(ref _kingdom, value, nameof(Kingdom));
        }

        [DataSourceProperty]
        public string Culture
        {
            get => _culture;
            set => SetProperty(ref _culture, value, nameof(Culture));
        }

        [DataSourceProperty]
        public int Level
        {
            get => _level;
            set
            {
                if (SetProperty(ref _level, value, nameof(Level)))
                {
                    OnPropertyChangedWithValue(value.ToString(), nameof(LevelText));
                }
            }
        }

        [DataSourceProperty]
        public string LevelText => _level.ToString();

        [DataSourceProperty]
        public string Gender
        {
            get => _gender;
            set => SetProperty(ref _gender, value, nameof(Gender));
        }

        [DataSourceProperty]
        public int Age
        {
            get => _age;
            set
            {
                if (SetProperty(ref _age, value, nameof(Age)))
                {
                    OnPropertyChangedWithValue(value.ToString(), nameof(AgeText));
                }
            }
        }

        [DataSourceProperty]
        public string AgeText => _age.ToString();

        [DataSourceProperty]
        public string HeroType
        {
            get => _heroType;
            set => SetProperty(ref _heroType, value, nameof(HeroType));
        }

        [DataSourceProperty]
        public bool IsAlive
        {
            get => _isAlive;
            set => SetProperty(ref _isAlive, value, nameof(IsAlive));
        }

        [DataSourceProperty]
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value, nameof(IsSelected));
        }

        /// <summary>
        /// When true, this hero should be hidden from the list (filtered out).
        /// Bound to IsHidden in XML for instant filtering without list rebuild.
        /// </summary>
        [DataSourceProperty]
        public bool IsFiltered
        {
            get => _isFiltered;
            set => SetProperty(ref _isFiltered, value, nameof(IsFiltered));
        }

        #endregion

        #region Helper Methods

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
        /// Helper method to reduce boilerplate in int property setters
        /// </summary>
        private bool SetProperty(ref int field, int value, string propertyName)
        {
            if (field == value)
                return false;

            field = value;
            OnPropertyChanged(propertyName);
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

        #endregion
    }
}
