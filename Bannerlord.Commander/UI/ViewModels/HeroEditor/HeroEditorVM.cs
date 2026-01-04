using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.Commander.UI.ViewModels.HeroEditor
{
    /// <summary>
    /// Main ViewModel for the Hero Editor Panel.
    /// Coordinates all sub-ViewModels and responds to hero selection changes.
    /// Equipment slots use custom EquipmentSlotVM for standalone equipment display without InventoryLogic dependency.
    /// </summary>
    public class HeroEditorVM : ViewModel, IHeroSelectionHandler
    {
        #region Private Fields

        private Hero _hero;
        private HeroInfoVM _heroInfo;
        private HeroIdentityVM _heroIdentity;
        private HeroCultureClanVM _heroCultureClan;
        private HeroPartyVM _heroParty;
        private HeroSkillsVM _heroSkills;
        private HeroInventoryVM _heroInventory;
        private HeroCharacterVM _heroCharacter;
        private ClanSelectionPopupVM _clanSelectionPopup;
        private bool _isVisible;
        private string _selectedHeroStringId;

        #endregion

        #region Constructor

        public HeroEditorVM()
        {
            // Initialize sub-ViewModels
            HeroInfo = new();
            HeroIdentity = new();
            HeroCultureClan = new();
            HeroParty = new();
            HeroSkills = new();
            HeroInventory = new();
            HeroCharacter = new();
            _clanSelectionPopup = new();
            IsVisible = false;
            SelectedHeroStringId = "";

            // Subscribe to HeroInventory property changes to forward equipment slot notifications
            HeroInventory.PropertyChanged += OnInventoryPropertyChanged;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes all sub-ViewModels with data from the specified hero.
        /// </summary>
        /// <param name="hero">The hero to display in the editor</param>
        public void RefreshForHero(Hero hero)
        {
            _hero = hero;

            if (_hero != null)
            {
                SelectedHeroStringId = _hero.StringId ?? "";
                HeroInfo?.RefreshForHero(_hero);
                HeroIdentity?.RefreshForHero(_hero);
                HeroCultureClan?.RefreshForHero(_hero, _clanSelectionPopup);
                HeroParty?.RefreshForHero(_hero);
                HeroSkills?.RefreshForHero(_hero);
                HeroInventory?.RefreshForHero(_hero);
                HeroCharacter?.RefreshForHero(_hero);

                IsVisible = true;
            }
            else
            {
                Clear();
            }
        }

        /// <summary>
        /// Clears all data and hides the editor panel.
        /// </summary>
        public void Clear()
        {
            _hero = null;
            IsVisible = false;
            SelectedHeroStringId = "";

            HeroInfo?.Clear();
            HeroIdentity?.Clear();
            HeroCultureClan?.Clear();
            HeroParty?.Clear();
            HeroSkills?.Clear();
            HeroInventory?.Clear();
            HeroCharacter?.Clear();
        }

        /// <summary>
        /// Implementation of IHeroSelectionHandler - called when a hero is selected.
        /// </summary>
        public void SelectHero(CommanderHeroVM hero)
        {
            if (hero != null)
            {
                RefreshForHero(hero.Hero);
            }
            else
            {
                Clear();
            }
        }

        public override void OnFinalize()
        {
            base.OnFinalize();

            // Unsubscribe from HeroInventory property changes
            if (HeroInventory != null)
            {
                HeroInventory.PropertyChanged -= OnInventoryPropertyChanged;
            }

            HeroInfo?.OnFinalize();
            HeroIdentity?.OnFinalize();
            HeroCultureClan?.OnFinalize();
            HeroParty?.OnFinalize();
            HeroSkills?.OnFinalize();
            HeroInventory?.OnFinalize();
            HeroCharacter?.OnFinalize();
        }

        #endregion

        #region Private Event Handlers

        /// <summary>
        /// Forwards property change notifications from HeroInventory equipment slots to this ViewModel.
        /// This allows direct binding to equipment slots in Gauntlet XML without nested path navigation.
        /// </summary>
        private void OnInventoryPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Forward equipment slot property changes
            switch (e.PropertyName)
            {
                case nameof(HeroInventoryVM.HeadSlot):
                    OnPropertyChangedWithValue(HeroInventory.HeadSlot, nameof(HeadSlot));
                    break;
                case nameof(HeroInventoryVM.CapeSlot):
                    OnPropertyChangedWithValue(HeroInventory.CapeSlot, nameof(CapeSlot));
                    break;
                case nameof(HeroInventoryVM.BodySlot):
                    OnPropertyChangedWithValue(HeroInventory.BodySlot, nameof(BodySlot));
                    break;
                case nameof(HeroInventoryVM.GlovesSlot):
                    OnPropertyChangedWithValue(HeroInventory.GlovesSlot, nameof(GlovesSlot));
                    break;
                case nameof(HeroInventoryVM.LegSlot):
                    OnPropertyChangedWithValue(HeroInventory.LegSlot, nameof(LegSlot));
                    break;
                case nameof(HeroInventoryVM.HorseSlot):
                    OnPropertyChangedWithValue(HeroInventory.HorseSlot, nameof(HorseSlot));
                    break;
                case nameof(HeroInventoryVM.HorseHarnessSlot):
                    OnPropertyChangedWithValue(HeroInventory.HorseHarnessSlot, nameof(HorseHarnessSlot));
                    break;
                case nameof(HeroInventoryVM.Weapon0Slot):
                    OnPropertyChangedWithValue(HeroInventory.Weapon0Slot, nameof(Weapon0Slot));
                    break;
                case nameof(HeroInventoryVM.Weapon1Slot):
                    OnPropertyChangedWithValue(HeroInventory.Weapon1Slot, nameof(Weapon1Slot));
                    break;
                case nameof(HeroInventoryVM.Weapon2Slot):
                    OnPropertyChangedWithValue(HeroInventory.Weapon2Slot, nameof(Weapon2Slot));
                    break;
                case nameof(HeroInventoryVM.Weapon3Slot):
                    OnPropertyChangedWithValue(HeroInventory.Weapon3Slot, nameof(Weapon3Slot));
                    break;
                case nameof(HeroInventoryVM.BannerSlot):
                    OnPropertyChangedWithValue(HeroInventory.BannerSlot, nameof(BannerSlot));
                    break;
            }
        }

        #endregion

        #region DataSource Properties

        /// <summary>
        /// Gets the currently selected hero.
        /// </summary>
        public Hero Hero => _hero;

        /// <summary>
        /// Gets or sets whether the editor panel is visible.
        /// </summary>
        [DataSourceProperty]
        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value, nameof(IsVisible));
        }

        /// <summary>
        /// Gets the selected hero's StringId for display.
        /// </summary>
        [DataSourceProperty]
        public string SelectedHeroStringId
        {
            get => _selectedHeroStringId;
            set => SetProperty(ref _selectedHeroStringId, value, nameof(SelectedHeroStringId));
        }

        /// <summary>
        /// Gets the hero info sub-ViewModel (portrait and read-only data).
        /// </summary>
        [DataSourceProperty]
        public HeroInfoVM HeroInfo
        {
            get => _heroInfo;
            private set => SetProperty(ref _heroInfo, value, nameof(HeroInfo));
        }

        /// <summary>
        /// Gets the hero identity sub-ViewModel (editable name/title).
        /// </summary>
        [DataSourceProperty]
        public HeroIdentityVM HeroIdentity
        {
            get => _heroIdentity;
            private set => SetProperty(ref _heroIdentity, value, nameof(HeroIdentity));
        }

        /// <summary>
        /// Gets the hero culture/clan sub-ViewModel.
        /// </summary>
        [DataSourceProperty]
        public HeroCultureClanVM HeroCultureClan
        {
            get => _heroCultureClan;
            private set => SetProperty(ref _heroCultureClan, value, nameof(HeroCultureClan));
        }

        /// <summary>
        /// Gets the hero party sub-ViewModel.
        /// </summary>
        [DataSourceProperty]
        public HeroPartyVM HeroParty
        {
            get => _heroParty;
            private set => SetProperty(ref _heroParty, value, nameof(HeroParty));
        }

        /// <summary>
        /// Gets the hero skills sub-ViewModel.
        /// </summary>
        [DataSourceProperty]
        public HeroSkillsVM HeroSkills
        {
            get => _heroSkills;
            private set => SetProperty(ref _heroSkills, value, nameof(HeroSkills));
        }

        /// <summary>
        /// Gets the hero equipment sub-ViewModel.
        /// </summary>
        [DataSourceProperty]
        public HeroInventoryVM HeroInventory
        {
            get => _heroInventory;
            private set => SetProperty(ref _heroInventory, value, nameof(HeroInventory));
        }

        /// <summary>
        /// Gets the hero character sub-ViewModel (for 3D model display).
        /// </summary>
        [DataSourceProperty]
        public HeroCharacterVM HeroCharacter
        {
            get => _heroCharacter;
            private set => SetProperty(ref _heroCharacter, value, nameof(HeroCharacter));
        }

        /// <summary>
        /// Gets the clan selection popup ViewModel.
        /// </summary>
        [DataSourceProperty]
        public ClanSelectionPopupVM ClanSelectionPopup
        {
            get => _clanSelectionPopup;
            private set => SetProperty(ref _clanSelectionPopup, value, nameof(ClanSelectionPopup));
        }

        // Pass-through properties for equipment slots to support Gauntlet's flat binding requirements
        // These forward to HeroInventoryVM's slot properties

        /// <summary>
        /// Pass-through property for head/helmet equipment slot from HeroInventory.
        /// </summary>
        [DataSourceProperty]
        public EquipmentSlotVM HeadSlot => HeroInventory?.HeadSlot;

        /// <summary>
        /// Pass-through property for cape equipment slot from HeroInventory.
        /// </summary>
        [DataSourceProperty]
        public EquipmentSlotVM CapeSlot => HeroInventory?.CapeSlot;

        /// <summary>
        /// Pass-through property for body/armor equipment slot from HeroInventory.
        /// </summary>
        [DataSourceProperty]
        public EquipmentSlotVM BodySlot => HeroInventory?.BodySlot;

        /// <summary>
        /// Pass-through property for gloves equipment slot from HeroInventory.
        /// </summary>
        [DataSourceProperty]
        public EquipmentSlotVM GlovesSlot => HeroInventory?.GlovesSlot;

        /// <summary>
        /// Pass-through property for leg/boots equipment slot from HeroInventory.
        /// </summary>
        [DataSourceProperty]
        public EquipmentSlotVM LegSlot => HeroInventory?.LegSlot;

        /// <summary>
        /// Pass-through property for horse equipment slot from HeroInventory.
        /// </summary>
        [DataSourceProperty]
        public EquipmentSlotVM HorseSlot => HeroInventory?.HorseSlot;

        /// <summary>
        /// Pass-through property for horse harness equipment slot from HeroInventory.
        /// </summary>
        [DataSourceProperty]
        public EquipmentSlotVM HorseHarnessSlot => HeroInventory?.HorseHarnessSlot;

        /// <summary>
        /// Pass-through property for weapon slot 0 from HeroInventory.
        /// </summary>
        [DataSourceProperty]
        public EquipmentSlotVM Weapon0Slot => HeroInventory?.Weapon0Slot;

        /// <summary>
        /// Pass-through property for weapon slot 1 from HeroInventory.
        /// </summary>
        [DataSourceProperty]
        public EquipmentSlotVM Weapon1Slot => HeroInventory?.Weapon1Slot;

        /// <summary>
        /// Pass-through property for weapon slot 2 from HeroInventory.
        /// </summary>
        [DataSourceProperty]
        public EquipmentSlotVM Weapon2Slot => HeroInventory?.Weapon2Slot;

        /// <summary>
        /// Pass-through property for weapon slot 3 from HeroInventory.
        /// </summary>
        [DataSourceProperty]
        public EquipmentSlotVM Weapon3Slot => HeroInventory?.Weapon3Slot;

        /// <summary>
        /// Pass-through property for banner equipment slot from HeroInventory.
        /// </summary>
        [DataSourceProperty]
        public EquipmentSlotVM BannerSlot => HeroInventory?.BannerSlot;

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

        /// <summary>
        /// Helper method to reduce boilerplate in string property setters.
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
        /// Helper method to reduce boilerplate in ViewModel property setters.
        /// </summary>
        private bool SetProperty<T>(ref T field, T value, string propertyName) where T : ViewModel
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
