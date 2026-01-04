using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.Commander.UI.ViewModels.HeroEditor
{
    /// <summary>
    /// ViewModel for hero equipment loadout management.
    /// Equipment slots are now managed at HeroEditorVM level following native SPInventoryVM pattern.
    /// This VM only handles loadout selection (Battle vs Civilian).
    /// </summary>
    public class HeroInventoryVM : ViewModel
    {
        #region Private Fields

        private Hero _hero;
        private int _selectedLoadoutIndex;
        private string _loadoutName;

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

        public HeroInventoryVM()
        {
            _selectedLoadoutIndex = 0;
            LoadoutName = "Battle";

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

            PropertyChanged += OnLoadoutPropertyChanged;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes the ViewModel with data from the specified hero.
        /// </summary>
        /// <param name="hero">The hero to display equipment for</param>
        public void RefreshForHero(Hero hero)
        {
            _hero = hero;
            RefreshEquipmentSlots();
        }

        /// <summary>
        /// Clears all data from the ViewModel.
        /// </summary>
        public void Clear()
        {
            _hero = null;
            _selectedLoadoutIndex = 0;
            LoadoutName = "Battle";

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

        public override void OnFinalize()
        {
            base.OnFinalize();

            // Unsubscribe from events
            PropertyChanged -= OnLoadoutPropertyChanged;

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

        #region Execute Methods

        /// <summary>
        /// Toggles between Battle and Civilian loadouts.
        /// </summary>
        public void ExecuteToggleLoadout()
        {
            _selectedLoadoutIndex = _selectedLoadoutIndex == 0 ? 1 : 0;
            LoadoutName = _selectedLoadoutIndex == 0 ? "Battle" : "Civilian";
            // Notify that loadout changed so parent can refresh equipment slots
            OnPropertyChanged(nameof(SelectedLoadoutIndex));
        }

        #endregion

        #region DataSource Properties

        /// <summary>
        /// Gets the current loadout name (Battle or Civilian).
        /// </summary>
        [DataSourceProperty]
        public string LoadoutName
        {
            get => _loadoutName;
            private set
            {
                if (_loadoutName != value)
                {
                    _loadoutName = value;
                    OnPropertyChangedWithValue(value, nameof(LoadoutName));
                }
            }
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

        /// <summary>
        /// Gets the selected loadout index (0 = Battle, 1 = Civilian).
        /// Exposed for HeroEditorVM to access when refreshing equipment slots.
        /// </summary>
        public int SelectedLoadoutIndex => _selectedLoadoutIndex;

        #endregion

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

        private void OnLoadoutPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedLoadoutIndex))
            {
                RefreshEquipmentSlots();
            }
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
            Equipment equipment = SelectedLoadoutIndex == 0 ?
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
    }
}
