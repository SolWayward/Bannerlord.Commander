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
        private HeroEquipmentVM _heroEquipment;
        private HeroCharacterVM _heroCharacter;
        private ClanSelectionPopupVM _clanSelectionPopup;
        private bool _isVisible;
        private string _selectedHeroStringId;

        // Individual equipment slots using custom EquipmentSlotVM for standalone equipment display
        private EquipmentSlotVM _headSlot;
        private EquipmentSlotVM _capeSlot;
        private EquipmentSlotVM _bodySlot;
        private EquipmentSlotVM _glovesSlot;
        private EquipmentSlotVM _legSlot;
        private EquipmentSlotVM _horseSlot;
        private EquipmentSlotVM _horseHarnessSlot;
        private EquipmentSlotVM _weapon0Slot;
        private EquipmentSlotVM _weapon1Slot;
        private EquipmentSlotVM _weapon2Slot;
        private EquipmentSlotVM _weapon3Slot;
        private EquipmentSlotVM _bannerSlot;

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

            // Initialize equipment slots with custom EquipmentSlotVM
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

            // Reset equipment slots to empty state
            HeadSlot.Reset();
            CapeSlot.Reset();
            BodySlot.Reset();
            GlovesSlot.Reset();
            LegSlot.Reset();
            HorseSlot.Reset();
            HorseHarnessSlot.Reset();
            Weapon0Slot.Reset();
            Weapon1Slot.Reset();
            Weapon2Slot.Reset();
            Weapon3Slot.Reset();
            BannerSlot.Reset();
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
        /// Gets the head/helmet equipment slot.
        /// </summary>
        [DataSourceProperty]
        public EquipmentSlotVM HeadSlot
        {
            get => _headSlot;
            private set => SetProperty(ref _headSlot, value, nameof(HeadSlot));
        }

        [DataSourceProperty]
        public EquipmentSlotVM CapeSlot
        {
            get => _capeSlot;
            private set => SetProperty(ref _capeSlot, value, nameof(CapeSlot));
        }

        [DataSourceProperty]
        public EquipmentSlotVM BodySlot
        {
            get => _bodySlot;
            private set => SetProperty(ref _bodySlot, value, nameof(BodySlot));
        }

        [DataSourceProperty]
        public EquipmentSlotVM GlovesSlot
        {
            get => _glovesSlot;
            private set => SetProperty(ref _glovesSlot, value, nameof(GlovesSlot));
        }

        [DataSourceProperty]
        public EquipmentSlotVM LegSlot
        {
            get => _legSlot;
            private set => SetProperty(ref _legSlot, value, nameof(LegSlot));
        }

        [DataSourceProperty]
        public EquipmentSlotVM HorseSlot
        {
            get => _horseSlot;
            private set => SetProperty(ref _horseSlot, value, nameof(HorseSlot));
        }

        [DataSourceProperty]
        public EquipmentSlotVM HorseHarnessSlot
        {
            get => _horseHarnessSlot;
            private set => SetProperty(ref _horseHarnessSlot, value, nameof(HorseHarnessSlot));
        }

        [DataSourceProperty]
        public EquipmentSlotVM Weapon0Slot
        {
            get => _weapon0Slot;
            private set => SetProperty(ref _weapon0Slot, value, nameof(Weapon0Slot));
        }

        [DataSourceProperty]
        public EquipmentSlotVM Weapon1Slot
        {
            get => _weapon1Slot;
            private set => SetProperty(ref _weapon1Slot, value, nameof(Weapon1Slot));
        }

        [DataSourceProperty]
        public EquipmentSlotVM Weapon2Slot
        {
            get => _weapon2Slot;
            private set => SetProperty(ref _weapon2Slot, value, nameof(Weapon2Slot));
        }

        [DataSourceProperty]
        public EquipmentSlotVM Weapon3Slot
        {
            get => _weapon3Slot;
            private set => SetProperty(ref _weapon3Slot, value, nameof(Weapon3Slot));
        }

        [DataSourceProperty]
        public EquipmentSlotVM BannerSlot
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
        /// Uses EquipmentSlotVM.RefreshWith() to update existing slot instances.
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

            // Update each slot using EquipmentSlotVM.RefreshWith()
            HeadSlot.RefreshWith(EquipmentIndex.Head, equipment[EquipmentIndex.Head]);
            CapeSlot.RefreshWith(EquipmentIndex.Cape, equipment[EquipmentIndex.Cape]);
            BodySlot.RefreshWith(EquipmentIndex.Body, equipment[EquipmentIndex.Body]);
            GlovesSlot.RefreshWith(EquipmentIndex.Gloves, equipment[EquipmentIndex.Gloves]);
            LegSlot.RefreshWith(EquipmentIndex.Leg, equipment[EquipmentIndex.Leg]);
            HorseSlot.RefreshWith(EquipmentIndex.Horse, equipment[EquipmentIndex.Horse]);
            HorseHarnessSlot.RefreshWith(EquipmentIndex.HorseHarness, equipment[EquipmentIndex.HorseHarness]);

            // Update weapon slots
            Weapon0Slot.RefreshWith(EquipmentIndex.Weapon0, equipment[EquipmentIndex.Weapon0]);
            Weapon1Slot.RefreshWith(EquipmentIndex.Weapon1, equipment[EquipmentIndex.Weapon1]);
            Weapon2Slot.RefreshWith(EquipmentIndex.Weapon2, equipment[EquipmentIndex.Weapon2]);
            Weapon3Slot.RefreshWith(EquipmentIndex.Weapon3, equipment[EquipmentIndex.Weapon3]);

            // Update banner slot
            BannerSlot.RefreshWith(EquipmentIndex.ExtraWeaponSlot, equipment[EquipmentIndex.ExtraWeaponSlot]);
        }

        #endregion
    }
}
