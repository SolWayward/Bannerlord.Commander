using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.Commander.UI.ViewModels.HeroEditor
{
    /// <summary>
    /// ViewModel for hero equipment display.
    /// Uses individual slot properties following native SPInventoryVM pattern.
    /// </summary>
    public class HeroEquipmentVM : ViewModel
    {
        #region Private Fields

        private Hero _hero;
        private int _selectedLoadoutIndex;
        private string _loadoutName;
        
        // Individual slot ViewModels - following native pattern from SPInventoryVM
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

        #endregion

        #region Constructor

        public HeroEquipmentVM()
        {
            _selectedLoadoutIndex = 0;
            LoadoutName = "Battle";
            
            // Initialize all slots with empty state - following native pattern
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
            
            if (_hero != null)
            {
                RefreshEquipment();
            }
            else
            {
                Clear();
            }
        }

        /// <summary>
        /// Clears all data from the ViewModel.
        /// </summary>
        public void Clear()
        {
            _hero = null;
            _selectedLoadoutIndex = 0;
            LoadoutName = "Battle";
            
            // Reset all slots to empty state
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
        }

        public override void OnFinalize()
        {
            base.OnFinalize();
            
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
            RefreshEquipment();
        }

        #endregion

        #region DataSource Properties - Individual Armor Slots

        /// <summary>
        /// Gets the head/helmet equipment slot.
        /// </summary>
        [DataSourceProperty]
        public EquipmentSlotVM HeadSlot
        {
            get => _headSlot;
            private set
            {
                if (_headSlot != value)
                {
                    _headSlot = value;
                    OnPropertyChangedWithValue(value, nameof(HeadSlot));
                }
            }
        }

        /// <summary>
        /// Gets the cape/cloak equipment slot.
        /// </summary>
        [DataSourceProperty]
        public EquipmentSlotVM CapeSlot
        {
            get => _capeSlot;
            private set
            {
                if (_capeSlot != value)
                {
                    _capeSlot = value;
                    OnPropertyChangedWithValue(value, nameof(CapeSlot));
                }
            }
        }

        /// <summary>
        /// Gets the body armor equipment slot.
        /// </summary>
        [DataSourceProperty]
        public EquipmentSlotVM BodySlot
        {
            get => _bodySlot;
            private set
            {
                if (_bodySlot != value)
                {
                    _bodySlot = value;
                    OnPropertyChangedWithValue(value, nameof(BodySlot));
                }
            }
        }

        /// <summary>
        /// Gets the gloves equipment slot.
        /// </summary>
        [DataSourceProperty]
        public EquipmentSlotVM GlovesSlot
        {
            get => _glovesSlot;
            private set
            {
                if (_glovesSlot != value)
                {
                    _glovesSlot = value;
                    OnPropertyChangedWithValue(value, nameof(GlovesSlot));
                }
            }
        }

        /// <summary>
        /// Gets the leg armor/boots equipment slot.
        /// </summary>
        [DataSourceProperty]
        public EquipmentSlotVM LegSlot
        {
            get => _legSlot;
            private set
            {
                if (_legSlot != value)
                {
                    _legSlot = value;
                    OnPropertyChangedWithValue(value, nameof(LegSlot));
                }
            }
        }

        /// <summary>
        /// Gets the horse/mount equipment slot.
        /// </summary>
        [DataSourceProperty]
        public EquipmentSlotVM HorseSlot
        {
            get => _horseSlot;
            private set
            {
                if (_horseSlot != value)
                {
                    _horseSlot = value;
                    OnPropertyChangedWithValue(value, nameof(HorseSlot));
                }
            }
        }

        /// <summary>
        /// Gets the horse harness/armor equipment slot.
        /// </summary>
        [DataSourceProperty]
        public EquipmentSlotVM HorseHarnessSlot
        {
            get => _horseHarnessSlot;
            private set
            {
                if (_horseHarnessSlot != value)
                {
                    _horseHarnessSlot = value;
                    OnPropertyChangedWithValue(value, nameof(HorseHarnessSlot));
                }
            }
        }

        #endregion

        #region DataSource Properties - Individual Weapon Slots

        /// <summary>
        /// Gets the first weapon slot.
        /// </summary>
        [DataSourceProperty]
        public EquipmentSlotVM Weapon0Slot
        {
            get => _weapon0Slot;
            private set
            {
                if (_weapon0Slot != value)
                {
                    _weapon0Slot = value;
                    OnPropertyChangedWithValue(value, nameof(Weapon0Slot));
                }
            }
        }

        /// <summary>
        /// Gets the second weapon slot.
        /// </summary>
        [DataSourceProperty]
        public EquipmentSlotVM Weapon1Slot
        {
            get => _weapon1Slot;
            private set
            {
                if (_weapon1Slot != value)
                {
                    _weapon1Slot = value;
                    OnPropertyChangedWithValue(value, nameof(Weapon1Slot));
                }
            }
        }

        /// <summary>
        /// Gets the third weapon slot.
        /// </summary>
        [DataSourceProperty]
        public EquipmentSlotVM Weapon2Slot
        {
            get => _weapon2Slot;
            private set
            {
                if (_weapon2Slot != value)
                {
                    _weapon2Slot = value;
                    OnPropertyChangedWithValue(value, nameof(Weapon2Slot));
                }
            }
        }

        /// <summary>
        /// Gets the fourth weapon slot.
        /// </summary>
        [DataSourceProperty]
        public EquipmentSlotVM Weapon3Slot
        {
            get => _weapon3Slot;
            private set
            {
                if (_weapon3Slot != value)
                {
                    _weapon3Slot = value;
                    OnPropertyChangedWithValue(value, nameof(Weapon3Slot));
                }
            }
        }

        #endregion

        #region DataSource Properties - General

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

        #endregion

        #region Private Methods

        /// <summary>
        /// Refreshes the equipment display based on the selected loadout.
        /// Following native SPInventoryVM.UpdateCharacterEquipment pattern.
        /// </summary>
        private void RefreshEquipment()
        {
            if (_hero == null)
            {
                Clear();
                return;
            }
            
            // Get the appropriate equipment based on selected loadout
            Equipment equipment = _selectedLoadoutIndex == 0 ?
                _hero.BattleEquipment : _hero.CivilianEquipment;
            
            if (equipment == null)
            {
                Clear();
                return;
            }
            
            // Update each armor slot following native InitializeCharacterEquipmentSlot pattern
            HeadSlot.RefreshWith(EquipmentIndex.Head, equipment[EquipmentIndex.Head]);
            CapeSlot.RefreshWith(EquipmentIndex.Cape, equipment[EquipmentIndex.Cape]);
            BodySlot.RefreshWith(EquipmentIndex.Body, equipment[EquipmentIndex.Body]);
            GlovesSlot.RefreshWith(EquipmentIndex.Gloves, equipment[EquipmentIndex.Gloves]);
            LegSlot.RefreshWith(EquipmentIndex.Leg, equipment[EquipmentIndex.Leg]);
            HorseSlot.RefreshWith(EquipmentIndex.Horse, equipment[EquipmentIndex.Horse]);
            HorseHarnessSlot.RefreshWith(EquipmentIndex.HorseHarness, equipment[EquipmentIndex.HorseHarness]);
            
            // Update each weapon slot
            Weapon0Slot.RefreshWith(EquipmentIndex.Weapon0, equipment[EquipmentIndex.Weapon0]);
            Weapon1Slot.RefreshWith(EquipmentIndex.Weapon1, equipment[EquipmentIndex.Weapon1]);
            Weapon2Slot.RefreshWith(EquipmentIndex.Weapon2, equipment[EquipmentIndex.Weapon2]);
            Weapon3Slot.RefreshWith(EquipmentIndex.Weapon3, equipment[EquipmentIndex.Weapon3]);
        }

        #endregion
    }
}
