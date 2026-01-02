using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;

namespace Bannerlord.Commander.UI.ViewModels.HeroEditor
{
    /// <summary>
    /// ViewModel for hero equipment display.
    /// Shows equipment slots with item icons.
    /// </summary>
    public class HeroEquipmentVM : ViewModel
    {
        #region Private Fields

        private Hero _hero;
        private MBBindingList<EquipmentSlotVM> _armorSlots;
        private MBBindingList<EquipmentSlotVM> _weaponSlots;
        private int _selectedLoadoutIndex;
        private string _loadoutName;

        #endregion

        #region Constructor

        public HeroEquipmentVM()
        {
            ArmorSlots = new();
            WeaponSlots = new();
            _selectedLoadoutIndex = 0;
            LoadoutName = "Battle";
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
            ArmorSlots.Clear();
            WeaponSlots.Clear();
            _selectedLoadoutIndex = 0;
            LoadoutName = "Battle";
        }

        public override void OnFinalize()
        {
            base.OnFinalize();
            
            if (ArmorSlots != null)
            {
                foreach (EquipmentSlotVM slot in ArmorSlots)
                {
                    slot?.OnFinalize();
                }
            }
            
            if (WeaponSlots != null)
            {
                foreach (EquipmentSlotVM slot in WeaponSlots)
                {
                    slot?.OnFinalize();
                }
            }
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

        #region DataSource Properties

        /// <summary>
        /// Gets the armor equipment slots (Head, Cape, Body, Gloves, Legs, Horse, Harness).
        /// </summary>
        [DataSourceProperty]
        public MBBindingList<EquipmentSlotVM> ArmorSlots
        {
            get => _armorSlots;
            private set
            {
                if (_armorSlots != value)
                {
                    _armorSlots = value;
                    OnPropertyChangedWithValue(value, nameof(ArmorSlots));
                }
            }
        }

        /// <summary>
        /// Gets the weapon equipment slots (Weapon 1-4).
        /// </summary>
        [DataSourceProperty]
        public MBBindingList<EquipmentSlotVM> WeaponSlots
        {
            get => _weaponSlots;
            private set
            {
                if (_weaponSlots != value)
                {
                    _weaponSlots = value;
                    OnPropertyChangedWithValue(value, nameof(WeaponSlots));
                }
            }
        }

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
        /// </summary>
        private void RefreshEquipment()
        {
            ArmorSlots.Clear();
            WeaponSlots.Clear();
            
            if (_hero == null)
                return;
            
            // Get the appropriate equipment based on selected loadout
            Equipment equipment = _selectedLoadoutIndex == 0 ?
                _hero.BattleEquipment : _hero.CivilianEquipment;
            
            if (equipment == null)
                return;
            
            // Add armor slots (displayed on the left side)
            ArmorSlots.Add(new(EquipmentIndex.Head, equipment[EquipmentIndex.Head]));
            ArmorSlots.Add(new(EquipmentIndex.Cape, equipment[EquipmentIndex.Cape]));
            ArmorSlots.Add(new(EquipmentIndex.Body, equipment[EquipmentIndex.Body]));
            ArmorSlots.Add(new(EquipmentIndex.Gloves, equipment[EquipmentIndex.Gloves]));
            ArmorSlots.Add(new(EquipmentIndex.Leg, equipment[EquipmentIndex.Leg]));
            ArmorSlots.Add(new(EquipmentIndex.Horse, equipment[EquipmentIndex.Horse]));
            ArmorSlots.Add(new(EquipmentIndex.HorseHarness, equipment[EquipmentIndex.HorseHarness]));
            
            // Add weapon slots (displayed on the right side)
            WeaponSlots.Add(new(EquipmentIndex.Weapon0, equipment[EquipmentIndex.Weapon0]));
            WeaponSlots.Add(new(EquipmentIndex.Weapon1, equipment[EquipmentIndex.Weapon1]));
            WeaponSlots.Add(new(EquipmentIndex.Weapon2, equipment[EquipmentIndex.Weapon2]));
            WeaponSlots.Add(new(EquipmentIndex.Weapon3, equipment[EquipmentIndex.Weapon3]));
        }

        #endregion
    }

    /// <summary>
    /// ViewModel representing a single equipment slot with image identifier.
    /// </summary>
    public class EquipmentSlotVM : ViewModel
    {
        #region Private Fields

        private EquipmentIndex _slotIndex;
        private string _slotName;
        private string _itemName;
        private bool _hasItem;
        private ItemImageIdentifierVM _imageIdentifier;
        private int _itemModifier;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new equipment slot ViewModel.
        /// </summary>
        /// <param name="index">The equipment slot index</param>
        /// <param name="element">The equipment element in this slot</param>
        public EquipmentSlotVM(EquipmentIndex index, EquipmentElement element)
        {
            _slotIndex = index;
            SlotName = GetSlotName(index);
            
            if (element.Item != null)
            {
                HasItem = true;
                ItemName = element.Item.Name?.ToString() ?? "Unknown";
                ItemModifier = element.ItemModifier != null ? 1 : 0;
                
                // Create ItemImageIdentifierVM for item display
                ImageIdentifier = new ItemImageIdentifierVM(element.Item, "");
            }
            else
            {
                HasItem = false;
                ItemName = "Empty";
                ItemModifier = 0;
                ImageIdentifier = new ItemImageIdentifierVM(null, "");
            }
        }

        #endregion

        #region DataSource Properties

        /// <summary>
        /// Gets the slot name.
        /// </summary>
        [DataSourceProperty]
        public string SlotName
        {
            get => _slotName;
            private set
            {
                if (_slotName != value)
                {
                    _slotName = value;
                    OnPropertyChangedWithValue(value, nameof(SlotName));
                }
            }
        }

        /// <summary>
        /// Gets the item name.
        /// </summary>
        [DataSourceProperty]
        public string ItemName
        {
            get => _itemName;
            private set
            {
                if (_itemName != value)
                {
                    _itemName = value;
                    OnPropertyChangedWithValue(value, nameof(ItemName));
                }
            }
        }

        /// <summary>
        /// Gets whether this slot has an item.
        /// </summary>
        [DataSourceProperty]
        public bool HasItem
        {
            get => _hasItem;
            private set
            {
                if (_hasItem != value)
                {
                    _hasItem = value;
                    OnPropertyChanged(nameof(HasItem));
                }
            }
        }

        /// <summary>
        /// Gets the image identifier for displaying the item icon.
        /// </summary>
        [DataSourceProperty]
        public ItemImageIdentifierVM ImageIdentifier
        {
            get => _imageIdentifier;
            private set
            {
                if (_imageIdentifier != value)
                {
                    _imageIdentifier = value;
                    OnPropertyChangedWithValue(value, nameof(ImageIdentifier));
                }
            }
        }

        /// <summary>
        /// Gets item modifier tier (0 = none, 1+ = has modifier).
        /// </summary>
        [DataSourceProperty]
        public int ItemModifier
        {
            get => _itemModifier;
            private set
            {
                if (_itemModifier != value)
                {
                    _itemModifier = value;
                    OnPropertyChanged(nameof(ItemModifier));
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets a display name for the equipment slot.
        /// </summary>
        private string GetSlotName(EquipmentIndex index)
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
                EquipmentIndex.NumEquipmentSetSlots => "Banner",
                _ => "Unknown"
            };
        }

        #endregion

        public override void OnFinalize()
        {
            base.OnFinalize();
            ImageIdentifier?.OnFinalize();
        }
    }
}
