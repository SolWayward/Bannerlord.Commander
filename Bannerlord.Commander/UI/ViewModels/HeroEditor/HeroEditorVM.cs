using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;

namespace Bannerlord.Commander.UI.ViewModels.HeroEditor
{
    /// <summary>
    /// Main ViewModel for the Hero Editor Panel.
    /// Coordinates all sub-ViewModels and responds to hero selection changes.
    /// Equipment slots use native SPItemVM for full compatibility with native widgets.
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
        private HeroEquipmentVM _heroEquipment;
        private HeroCharacterVM _heroCharacter;
        private ClanSelectionPopupVM _clanSelectionPopup;
        private bool _isVisible;
        private string _selectedHeroStringId;

        // Individual equipment slots using native SPItemVM for proper widget compatibility
        private SPItemVM _headSlot;
        private SPItemVM _capeSlot;
        private SPItemVM _bodySlot;
        private SPItemVM _glovesSlot;
        private SPItemVM _legSlot;
        private SPItemVM _horseSlot;
        private SPItemVM _horseHarnessSlot;
        private SPItemVM _weapon0Slot;
        private SPItemVM _weapon1Slot;
        private SPItemVM _weapon2Slot;
        private SPItemVM _weapon3Slot;
        private SPItemVM _bannerSlot;

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
            HeroEquipment = new();
            HeroCharacter = new();
            _clanSelectionPopup = new();
            IsVisible = false;
            SelectedHeroStringId = "";

            // Initialize equipment slots - following native SPInventoryVM pattern
            HeadSlot = new();
            CapeSlot = new();
            BodySlot = new();
            GlovesSlot = new();
            LegSlot = new();
            HorseSlot = new();
            HorseHarnessSlot = new();
            Weapon0Slot = new();
            Weapon1Slot = new();
            Weapon2Slot = new();
            Weapon3Slot = new();
            BannerSlot = new();

            // Subscribe to equipment loadout changes
            HeroEquipment.PropertyChanged += OnEquipmentPropertyChanged;
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
                HeroEquipment?.RefreshForHero(_hero);
                HeroCharacter?.RefreshForHero(_hero);
                
                // Refresh equipment slots directly - following native pattern
                RefreshEquipmentSlots();
                
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
            HeroEquipment?.Clear();
            HeroCharacter?.Clear();
            
            // Reset equipment slots to empty - using native SPItemVM empty constructor
            HeadSlot = new();
            CapeSlot = new();
            BodySlot = new();
            GlovesSlot = new();
            LegSlot = new();
            HorseSlot = new();
            HorseHarnessSlot = new();
            Weapon0Slot = new();
            Weapon1Slot = new();
            Weapon2Slot = new();
            Weapon3Slot = new();
            BannerSlot = new();
        }

        /// <summary>
        /// Implementation of IHeroSelectionHandler - called when a hero is selected.
        /// </summary>
        public void SelectHero(HeroItemVM hero)
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
            
            // Unsubscribe from events
            HeroEquipment.PropertyChanged -= OnEquipmentPropertyChanged;
            
            HeroInfo?.OnFinalize();
            HeroIdentity?.OnFinalize();
            HeroCultureClan?.OnFinalize();
            HeroParty?.OnFinalize();
            HeroSkills?.OnFinalize();
            HeroEquipment?.OnFinalize();
            HeroCharacter?.OnFinalize();
            
            // Finalize equipment slots
            HeadSlot?.OnFinalize();
            CapeSlot?.OnFinalize();
            BodySlot?.OnFinalize();
            GlovesSlot?.OnFinalize();
            LegSlot?.OnFinalize();
            HorseSlot?.OnFinalize();
            HorseHarnessSlot?.OnFinalize();
            Weapon0Slot?.OnFinalize();
            Weapon1Slot?.OnFinalize();
            Weapon2Slot?.OnFinalize();
            Weapon3Slot?.OnFinalize();
            BannerSlot?.OnFinalize();
        }

        #endregion

        #region Private Event Handlers

        /// <summary>
        /// Handles property changes in HeroEquipmentVM to refresh equipment slots when loadout changes.
        /// </summary>
        private void OnEquipmentPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Refresh equipment slots when loadout changes
            if (e.PropertyName == "SelectedLoadoutIndex")
            {
                RefreshEquipmentSlots();
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
        public HeroEquipmentVM HeroEquipment
        {
            get => _heroEquipment;
            private set => SetProperty(ref _heroEquipment, value, nameof(HeroEquipment));
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

        /// <summary>
        /// Gets the head/helmet equipment slot using native SPItemVM.
        /// Following native SPInventoryVM.CharacterHelmSlot pattern.
        /// </summary>
        [DataSourceProperty]
        public SPItemVM HeadSlot
        {
            get => _headSlot;
            private set => SetProperty(ref _headSlot, value, nameof(HeadSlot));
        }

        [DataSourceProperty]
        public SPItemVM CapeSlot
        {
            get => _capeSlot;
            private set => SetProperty(ref _capeSlot, value, nameof(CapeSlot));
        }

        [DataSourceProperty]
        public SPItemVM BodySlot
        {
            get => _bodySlot;
            private set => SetProperty(ref _bodySlot, value, nameof(BodySlot));
        }

        [DataSourceProperty]
        public SPItemVM GlovesSlot
        {
            get => _glovesSlot;
            private set => SetProperty(ref _glovesSlot, value, nameof(GlovesSlot));
        }

        [DataSourceProperty]
        public SPItemVM LegSlot
        {
            get => _legSlot;
            private set => SetProperty(ref _legSlot, value, nameof(LegSlot));
        }

        [DataSourceProperty]
        public SPItemVM HorseSlot
        {
            get => _horseSlot;
            private set => SetProperty(ref _horseSlot, value, nameof(HorseSlot));
        }

        [DataSourceProperty]
        public SPItemVM HorseHarnessSlot
        {
            get => _horseHarnessSlot;
            private set => SetProperty(ref _horseHarnessSlot, value, nameof(HorseHarnessSlot));
        }

        [DataSourceProperty]
        public SPItemVM Weapon0Slot
        {
            get => _weapon0Slot;
            private set => SetProperty(ref _weapon0Slot, value, nameof(Weapon0Slot));
        }

        [DataSourceProperty]
        public SPItemVM Weapon1Slot
        {
            get => _weapon1Slot;
            private set => SetProperty(ref _weapon1Slot, value, nameof(Weapon1Slot));
        }

        [DataSourceProperty]
        public SPItemVM Weapon2Slot
        {
            get => _weapon2Slot;
            private set => SetProperty(ref _weapon2Slot, value, nameof(Weapon2Slot));
        }

        [DataSourceProperty]
        public SPItemVM Weapon3Slot
        {
            get => _weapon3Slot;
            private set => SetProperty(ref _weapon3Slot, value, nameof(Weapon3Slot));
        }

        [DataSourceProperty]
        public SPItemVM BannerSlot
        {
            get => _bannerSlot;
            private set => SetProperty(ref _bannerSlot, value, nameof(BannerSlot));
        }

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

        /// <summary>
        /// Refreshes equipment slots with data from current equipment loadout.
        /// Creates new SPItemVM instances for each slot using native pattern.
        /// </summary>
        private void RefreshEquipmentSlots()
        {
            if (_hero == null)
            {
                return;
            }

            // Get the appropriate equipment based on selected loadout from HeroEquipment
            Equipment equipment = HeroEquipment.SelectedLoadoutIndex == 0 ?
                _hero.BattleEquipment : _hero.CivilianEquipment;

            if (equipment == null)
            {
                return;
            }

            // Update each slot - creating new SPItemVM instances for proper native compatibility
            HeadSlot = CreateEquipmentSlotVM(equipment[EquipmentIndex.Head], EquipmentIndex.Head);
            CapeSlot = CreateEquipmentSlotVM(equipment[EquipmentIndex.Cape], EquipmentIndex.Cape);
            BodySlot = CreateEquipmentSlotVM(equipment[EquipmentIndex.Body], EquipmentIndex.Body);
            GlovesSlot = CreateEquipmentSlotVM(equipment[EquipmentIndex.Gloves], EquipmentIndex.Gloves);
            LegSlot = CreateEquipmentSlotVM(equipment[EquipmentIndex.Leg], EquipmentIndex.Leg);
            HorseSlot = CreateEquipmentSlotVM(equipment[EquipmentIndex.Horse], EquipmentIndex.Horse);
            HorseHarnessSlot = CreateEquipmentSlotVM(equipment[EquipmentIndex.HorseHarness], EquipmentIndex.HorseHarness);

            // Update weapon slots
            Weapon0Slot = CreateEquipmentSlotVM(equipment[EquipmentIndex.Weapon0], EquipmentIndex.Weapon0);
            Weapon1Slot = CreateEquipmentSlotVM(equipment[EquipmentIndex.Weapon1], EquipmentIndex.Weapon1);
            Weapon2Slot = CreateEquipmentSlotVM(equipment[EquipmentIndex.Weapon2], EquipmentIndex.Weapon2);
            Weapon3Slot = CreateEquipmentSlotVM(equipment[EquipmentIndex.Weapon3], EquipmentIndex.Weapon3);

            // Update banner slot
            BannerSlot = CreateEquipmentSlotVM(equipment[EquipmentIndex.ExtraWeaponSlot], EquipmentIndex.ExtraWeaponSlot);
        }

        /// <summary>
        /// Creates an SPItemVM from an equipment element.
        /// Uses native pattern: empty constructor for empty slots, manual property setup for equipped items.
        /// </summary>
        /// <param name="element">Equipment element to create slot for</param>
        /// <param name="index">Equipment index for type identification</param>
        /// <returns>New SPItemVM instance configured for the slot</returns>
        private SPItemVM CreateEquipmentSlotVM(EquipmentElement element, EquipmentIndex index)
        {
            SPItemVM slot = new();
            
            if (!element.IsEmpty && element.Item != null)
            {
                // Populate slot with item data - following native SPItemVM initialization pattern
                // Native SPItemVM sets these properties (lines 28-29, 60-64 in decompiled code)
                slot.StringId = element.Item.StringId ?? "";
                slot.ImageIdentifier = new ItemImageIdentifierVM(element.Item, "");
                slot.ItemDescription = element.GetModifiedItemName()?.ToString() ?? element.Item.Name?.ToString() ?? "Unknown";
                slot.TypeName = GetEquipmentTypeName(index);
            }
            else
            {
                // Empty slot - following native SPItemVM empty constructor pattern (lines 26-31)
                // Native sets StringId = "", ImageIdentifier = new ItemImageIdentifierVM(null, "")
                slot.StringId = "";
                slot.ImageIdentifier = new ItemImageIdentifierVM(null, "");
                slot.ItemDescription = "";
                slot.TypeName = GetEquipmentTypeName(index);
            }

            return slot;
        }

        /// <summary>
        /// Gets a display name for the equipment slot type.
        /// </summary>
        private static string GetEquipmentTypeName(EquipmentIndex index)
        {
            return index switch
            {
                EquipmentIndex.Head => "Head",
                EquipmentIndex.Cape => "Cape",
                EquipmentIndex.Body => "Body",
                EquipmentIndex.Gloves => "Gloves",
                EquipmentIndex.Leg => "Legs",
                EquipmentIndex.Horse => "Horse",
                EquipmentIndex.HorseHarness => "Harness",
                EquipmentIndex.Weapon0 => "Weapon 1",
                EquipmentIndex.Weapon1 => "Weapon 2",
                EquipmentIndex.Weapon2 => "Weapon 3",
                EquipmentIndex.Weapon3 => "Weapon 4",
                EquipmentIndex.ExtraWeaponSlot => "Banner",
                _ => ""
            };
        }

        #endregion
    }
}
