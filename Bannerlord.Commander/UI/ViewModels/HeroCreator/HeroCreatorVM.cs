using System;
using System.Collections.Generic;
using Bannerlord.Commander.UI.Enums;
using Bannerlord.GameMaster.Characters;
using Bannerlord.GameMaster.Clans;
using Bannerlord.GameMaster.Cultures;
using Bannerlord.GameMaster.Heroes;
using Bannerlord.GameMaster.Information;
using Bannerlord.GameMaster.Settlements;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Bannerlord.Commander.UI.ViewModels.HeroCreator
{
    /// <summary>
    /// Main ViewModel for the Hero Creator overlay panel.
    /// Manages all state, filter logic, and hero creation logic.
    /// </summary>
    public class HeroCreatorVM : ViewModel
    {
        #region Constants

        private const int MaxFilteredClans = 15;

        #endregion

        #region Private Fields

        private List<HeroCreatorClanItemVM> _allClans;

        private string _heroName;
        private string _levelText;
        private bool _isMaleSelected;
        private bool _isFemaleSelected;
        private bool _createParty;
        private bool _isCreatePartyEnabled;
        private float _appearanceRandomization;
        private string _clanFilterText;

        private MBBindingList<HeroCreatorClanItemVM> _filteredClans;
        private MBBindingList<HeroCreatorTypeItemVM> _heroTypes;
        private MBBindingList<HeroCreatorCultureItemVM> _cultures;

        #endregion

        #region Events

        /// <summary>
        /// Raised when the panel should be closed (Cancel or after successful Create).
        /// </summary>
        public event Action OnCloseRequested;

        /// <summary>
        /// Raised when a hero is successfully created.
        /// </summary>
        public event Action<Hero> OnHeroCreated;

        #endregion

        #region Constructor

        public HeroCreatorVM()
        {
            _filteredClans = new MBBindingList<HeroCreatorClanItemVM>();
            _heroTypes = new MBBindingList<HeroCreatorTypeItemVM>();
            _cultures = new MBBindingList<HeroCreatorCultureItemVM>();

            // Defaults
            HeroName = "";
            LevelText = "";
            IsMaleSelected = true;
            IsFemaleSelected = false;
            CreateParty = false;
            IsCreatePartyEnabled = true;
            AppearanceRandomization = 0.5f;
            _clanFilterText = "";

            InitializeClans();
            InitializeTypes();
            InitializeCultures();
        }

        #endregion

        #region DataSource Properties

        [DataSourceProperty]
        public string HeroName
        {
            get => _heroName;
            set
            {
                if (_heroName != value)
                {
                    _heroName = value;
                    OnPropertyChangedWithValue(value, nameof(HeroName));
                }
            }
        }

        [DataSourceProperty]
        public string LevelText
        {
            get => _levelText;
            set
            {
                if (_levelText != value)
                {
                    _levelText = value;
                    OnPropertyChangedWithValue(value, nameof(LevelText));
                }
            }
        }

        [DataSourceProperty]
        public bool IsMaleSelected
        {
            get => _isMaleSelected;
            set
            {
                if (_isMaleSelected != value)
                {
                    _isMaleSelected = value;
                    OnPropertyChanged(nameof(IsMaleSelected));
                }
            }
        }

        [DataSourceProperty]
        public bool IsFemaleSelected
        {
            get => _isFemaleSelected;
            set
            {
                if (_isFemaleSelected != value)
                {
                    _isFemaleSelected = value;
                    OnPropertyChanged(nameof(IsFemaleSelected));
                }
            }
        }

        [DataSourceProperty]
        public bool CreateParty
        {
            get => _createParty;
            set
            {
                if (_createParty != value)
                {
                    _createParty = value;
                    OnPropertyChanged(nameof(CreateParty));
                }
            }
        }

        [DataSourceProperty]
        public bool IsCreatePartyEnabled
        {
            get => _isCreatePartyEnabled;
            set
            {
                if (_isCreatePartyEnabled != value)
                {
                    _isCreatePartyEnabled = value;
                    OnPropertyChanged(nameof(IsCreatePartyEnabled));
                }
            }
        }

        [DataSourceProperty]
        public float AppearanceRandomization
        {
            get => _appearanceRandomization;
            set
            {
                if (Math.Abs(_appearanceRandomization - value) > 0.001f)
                {
                    _appearanceRandomization = value;
                    OnPropertyChangedWithValue(value, nameof(AppearanceRandomization));
                }
            }
        }

        [DataSourceProperty]
        public string ClanFilterText
        {
            get => _clanFilterText;
            set
            {
                if (_clanFilterText != value)
                {
                    _clanFilterText = value;
                    OnPropertyChangedWithValue(value, nameof(ClanFilterText));
                    ApplyClanFilter();
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<HeroCreatorClanItemVM> FilteredClans
        {
            get => _filteredClans;
            set
            {
                if (_filteredClans != value)
                {
                    _filteredClans = value;
                    OnPropertyChangedWithValue(value, nameof(FilteredClans));
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<HeroCreatorTypeItemVM> HeroTypes
        {
            get => _heroTypes;
            set
            {
                if (_heroTypes != value)
                {
                    _heroTypes = value;
                    OnPropertyChangedWithValue(value, nameof(HeroTypes));
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<HeroCreatorCultureItemVM> Cultures
        {
            get => _cultures;
            set
            {
                if (_cultures != value)
                {
                    _cultures = value;
                    OnPropertyChangedWithValue(value, nameof(Cultures));
                }
            }
        }

        #endregion

        #region Execute Methods

        public void ExecuteSelectMale()
        {
            IsMaleSelected = true;
            IsFemaleSelected = false;
        }

        public void ExecuteSelectFemale()
        {
            IsMaleSelected = false;
            IsFemaleSelected = true;
        }

        public void ExecuteCancel()
        {
            OnCloseRequested?.Invoke();
        }

        /// <summary>
        /// Validates inputs and creates a hero using BLGM APIs.
        /// </summary>
        public void ExecuteCreate()
        {
            // Resolve selected type
            HeroCreatorTypeItemVM selectedType = GetSelectedType();
            if (selectedType == null)
            {
                InfoMessage.Warning("Please select a hero type");
                return;
            }

            // Check for unimplemented types
            if (selectedType.TypeEnum == HeroCreatorType.MinorLord)
            {
                InfoMessage.Warning("Minor Lord creation is not yet implemented - awaiting BLGM support");
                return;
            }

            if (selectedType.TypeEnum == HeroCreatorType.Notable)
            {
                InfoMessage.Warning("Notable creation is not yet implemented - awaiting BLGM support");
                return;
            }

            // Resolve selected culture
            CultureObject culture = GetSelectedCulture();
            CultureFlags cultureFlags = (culture != null)
                ? CultureLookup.GetCultureFlag(culture)
                : CultureFlags.AllMainCultures;

            // Resolve gender
            GenderFlags genderFlags = IsMaleSelected ? GenderFlags.Male : GenderFlags.Female;

            // Resolve name (empty = auto-generate)
            CultureObject resolvedCulture = culture ?? CultureLookup.RandomMainCulture();
            string name = string.IsNullOrWhiteSpace(HeroName)
                ? CultureLookup.GetUniqueRandomHeroName(resolvedCulture, genderFlags == GenderFlags.Female)
                : HeroName.Trim();

            // Resolve clan
            Clan selectedClan = GetSelectedClan();
            float randomFactor = AppearanceRandomization;

            switch (selectedType.TypeEnum)
            {
                case HeroCreatorType.Lord:
                    CreateLord(name, cultureFlags, genderFlags, selectedClan, randomFactor);
                    break;

                case HeroCreatorType.Wanderer:
                    CreateWanderer(name, cultureFlags, genderFlags, selectedClan, randomFactor);
                    break;
            }
        }

        #endregion

        #region Selection Handlers

        /// <summary>
        /// Called by HeroCreatorClanItemVM when a clan is selected.
        /// Handles mutual exclusion across all clan items.
        /// </summary>
        public void SelectClan(HeroCreatorClanItemVM selected)
        {
            for (int i = 0; i < _allClans.Count; i++)
            {
                _allClans[i].IsSelected = (_allClans[i] == selected);
            }
        }

        /// <summary>
        /// Called by HeroCreatorTypeItemVM when a type is selected.
        /// Handles mutual exclusion and enables/disables Create Party checkbox.
        /// </summary>
        public void SelectType(HeroCreatorTypeItemVM selected)
        {
            for (int i = 0; i < HeroTypes.Count; i++)
            {
                HeroTypes[i].IsSelected = (HeroTypes[i] == selected);
            }

            // Create Party checkbox is only enabled for Lord type
            IsCreatePartyEnabled = (selected.TypeEnum == HeroCreatorType.Lord);
            if (!IsCreatePartyEnabled)
            {
                CreateParty = false;
            }
        }

        /// <summary>
        /// Called by HeroCreatorCultureItemVM when a culture is selected.
        /// Handles mutual exclusion across all culture items.
        /// </summary>
        public void SelectCulture(HeroCreatorCultureItemVM selected)
        {
            for (int i = 0; i < Cultures.Count; i++)
            {
                Cultures[i].IsSelected = (Cultures[i] == selected);
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the clan list with Random, Player Clan, and all other clans sorted by name.
        /// </summary>
        private void InitializeClans()
        {
            _allClans = new List<HeroCreatorClanItemVM>();

            // "Random" pseudo-item first
            HeroCreatorClanItemVM randomItem = new(null, "Random", "", this, isSpecial: true);
            _allClans.Add(randomItem);

            // Player clan second
            if (Clan.PlayerClan != null)
            {
                HeroCreatorClanItemVM playerItem = new(
                    Clan.PlayerClan,
                    Clan.PlayerClan.Name?.ToString() ?? "Player Clan",
                    Clan.PlayerClan.Kingdom?.Name?.ToString() ?? "No Kingdom",
                    this,
                    isSpecial: true);
                _allClans.Add(playerItem);
            }

            // All other clans sorted by name, using ClanQueries
            MBReadOnlyList<Clan> allClans = ClanQueries.QueryClans("", sortBy: "name");
            for (int i = 0; i < allClans.Count; i++)
            {
                Clan clan = allClans[i];
                if (clan == Clan.PlayerClan) continue;

                string kingdomName = clan.Kingdom?.Name?.ToString() ?? "No Kingdom";
                HeroCreatorClanItemVM item = new(clan, clan.Name?.ToString() ?? "Unknown", kingdomName, this);
                _allClans.Add(item);
            }

            // Select Random by default
            randomItem.IsSelected = true;

            ApplyClanFilter();
        }

        /// <summary>
        /// Initializes the fixed 4-item type list.
        /// Lord and Wanderer are enabled; MinorLord and Notable are disabled (awaiting BLGM).
        /// </summary>
        private void InitializeTypes()
        {
            HeroCreatorTypeItemVM lordItem = new(HeroCreatorType.Lord, "Lord", this, isEnabled: true);
            HeroCreatorTypeItemVM minorLordItem = new(HeroCreatorType.MinorLord, "Minor Lord", this, isEnabled: false);
            HeroCreatorTypeItemVM wandererItem = new(HeroCreatorType.Wanderer, "Wanderer", this, isEnabled: true);
            HeroCreatorTypeItemVM notableItem = new(HeroCreatorType.Notable, "Notable", this, isEnabled: false);

            // Default to Lord
            lordItem.IsSelected = true;

            HeroTypes.Add(lordItem);
            HeroTypes.Add(minorLordItem);
            HeroTypes.Add(wandererItem);
            HeroTypes.Add(notableItem);
        }

        /// <summary>
        /// Initializes the culture list with Random + all main cultures.
        /// </summary>
        private void InitializeCultures()
        {
            HeroCreatorCultureItemVM randomItem = new(null, "Random", this);
            Cultures.Add(randomItem);

            List<CultureObject> mainCultures = CultureLookup.MainCultures;
            for (int i = 0; i < mainCultures.Count; i++)
            {
                CultureObject culture = mainCultures[i];
                HeroCreatorCultureItemVM item = new(culture, culture.Name?.ToString() ?? culture.StringId, this);
                Cultures.Add(item);
            }

            // Default to Random
            randomItem.IsSelected = true;
        }

        #endregion

        #region Clan Filtering

        /// <summary>
        /// Filters the clan list based on ClanFilterText. Matches against clan name OR kingdom name.
        /// Always shows max 12 results. Random and Player Clan are always at top when unfiltered.
        /// </summary>
        private void ApplyClanFilter()
        {
            if (_allClans == null) return;

            FilteredClans.Clear();
            int count = 0;
            string filter = ClanFilterText?.ToLowerInvariant() ?? "";

            for (int i = 0; i < _allClans.Count; i++)
            {
                if (count >= MaxFilteredClans) break;

                HeroCreatorClanItemVM clan = _allClans[i];

                if (string.IsNullOrEmpty(filter)
                    || clan.ClanName.ToLowerInvariant().Contains(filter)
                    || clan.KingdomName.ToLowerInvariant().Contains(filter))
                {
                    FilteredClans.Add(clan);
                    count++;
                }
            }
        }

        #endregion

        #region Hero Creation

        /// <summary>
        /// Creates a Lord hero and invokes creation events.
        /// </summary>
        private void CreateLord(string name, CultureFlags cultureFlags, GenderFlags genderFlags,
            Clan selectedClan, float randomFactor)
        {
            Clan lordClan = selectedClan ?? GetRandomMainCultureClan();
            if (lordClan == null)
            {
                InfoMessage.Error("Could not find a suitable clan for Lord creation");
                return;
            }

            Hero hero = HeroGenerator.CreateLord(
                name, cultureFlags, genderFlags, lordClan,
                withParty: CreateParty,
                randomFactor: randomFactor);

            InfoMessage.Success($"Created Lord: {hero.Name}");
            OnHeroCreated?.Invoke(hero);
            OnCloseRequested?.Invoke();
        }

        /// <summary>
        /// Creates a Wanderer hero. If no clan is specified, places at a random town.
        /// If a clan is specified, creates as a companion added to that clan.
        /// </summary>
        private void CreateWanderer(string name, CultureFlags cultureFlags, GenderFlags genderFlags,
            Clan selectedClan, float randomFactor)
        {
            if (selectedClan == null)
            {
                // No clan specified - create as wanderer in random town
                Settlement randomTown = SettlementManager.GetRandomTown();
                if (randomTown == null)
                {
                    InfoMessage.Error("Could not find a town for Wanderer placement");
                    return;
                }

                Hero wanderer = HeroGenerator.CreateWanderer(
                    name, cultureFlags, genderFlags,
                    randomTown, randomFactor);

                InfoMessage.Success($"Created Wanderer: {wanderer.Name} at {randomTown.Name}");
                OnHeroCreated?.Invoke(wanderer);
            }

            else
            {
                // Clan specified - create as companion and add to clan
                List<Hero> companions = HeroGenerator.CreateCompanions(
                    1, cultureFlags, genderFlags, randomFactor);

                if (companions.Count > 0)
                {
                    Hero companion = companions[0];
                    companion.SetName(new TextObject(name), new TextObject(name));
                    AddCompanionAction.Apply(selectedClan, companion);
                    InfoMessage.Success($"Created Companion: {companion.Name} in {selectedClan.Name}");
                    OnHeroCreated?.Invoke(companion);
                }

                else
                {
                    InfoMessage.Error("Failed to create companion");
                    return;
                }
            }

            OnCloseRequested?.Invoke();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets the currently selected type item, or null if none.
        /// </summary>
        private HeroCreatorTypeItemVM GetSelectedType()
        {
            for (int i = 0; i < HeroTypes.Count; i++)
            {
                if (HeroTypes[i].IsSelected) return HeroTypes[i];
            }

            return null;
        }

        /// <summary>
        /// Gets the currently selected culture object, or null for Random.
        /// </summary>
        private CultureObject GetSelectedCulture()
        {
            for (int i = 0; i < Cultures.Count; i++)
            {
                if (Cultures[i].IsSelected) return Cultures[i].Culture;
            }

            return null;
        }

        /// <summary>
        /// Gets the currently selected clan, or null for Random.
        /// </summary>
        private Clan GetSelectedClan()
        {
            for (int i = 0; i < _allClans.Count; i++)
            {
                if (_allClans[i].IsSelected) return _allClans[i].Clan;
            }

            return null;
        }

        /// <summary>
        /// Gets a random clan that belongs to a main culture.
        /// </summary>
        private Clan GetRandomMainCultureClan()
        {
            MBReadOnlyList<Clan> clans = ClanQueries.QueryClans("");
            List<Clan> mainCultureClans = new();

            for (int i = 0; i < clans.Count; i++)
            {
                Clan clan = clans[i];
                if (clan?.Culture != null && clan.Culture.IsMainCulture)
                {
                    mainCultureClans.Add(clan);
                }
            }

            if (mainCultureClans.Count == 0) return null;

            int randomIndex = MBRandom.RandomInt(mainCultureClans.Count);
            return mainCultureClans[randomIndex];
        }

        /// <summary>
        /// Parses level text to int. Returns -1 if empty/invalid (tells BLGM to randomize).
        /// Clamps to 1-62 matching BLGM's InitializeSkillsForLevel range.
        /// </summary>
        private int ParseLevel()
        {
            if (string.IsNullOrWhiteSpace(LevelText)) return -1;

            if (int.TryParse(LevelText, out int level))
            {
                return MBMath.ClampInt(level, 1, 62);
            }

            return -1;
        }

        #endregion

        #region Cleanup

        public override void OnFinalize()
        {
            base.OnFinalize();

            OnCloseRequested = null;
            OnHeroCreated = null;

            _allClans?.Clear();
            _allClans = null;

            FilteredClans?.Clear();
            HeroTypes?.Clear();
            Cultures?.Clear();
        }

        #endregion
    }
}
