using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;

namespace Bannerlord.Commander.UI.ViewModels.HeroEditor
{
    /// <summary>
    /// ViewModel representing a single equipment slot with image identifier.
    /// Follows native SPItemVM pattern - only creates ImageIdentifier when item exists.
    /// </summary>
    public class EquipmentSlotVM : ViewModel
    {
        #region Private Fields

        private EquipmentIndex _slotIndex;
        private EquipmentElement _equipmentElement;
        private string _slotName;
        private string _itemName;
        private bool _hasItem;
        private ItemImageIdentifierVM _imageIdentifier;
        private int _itemModifier;
        private string _emptySlotSprite;
        private bool _isSelected;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new empty equipment slot ViewModel.
        /// Following native SPItemVM default constructor pattern.
        /// </summary>
        public EquipmentSlotVM()
        {
            _slotIndex = EquipmentIndex.None;
            _slotName = "";
            _itemName = "";
            _hasItem = false;
            _imageIdentifier = null;  // NULL for empty slots - prevents crash
            _itemModifier = 0;
            _emptySlotSprite = "Inventory\\empty_head_slot";
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Command handler for slot selection.
        /// Toggles the IsSelected state for this slot.
        /// Future enhancement: Implement equipment management UI when slot is selected.
        /// </summary>
        public void ExecuteSelectSlot()
        {
            IsSelected = !IsSelected;
            // TODO: Future enhancement - open equipment selection/management UI
            // This will be implemented when equipment management features are added
        }

        /// <summary>
        /// Refreshes this slot with new equipment data.
        /// Following native SPItemVM.RefreshWith pattern.
        /// </summary>
        /// <param name="index">The equipment slot index</param>
        /// <param name="element">The equipment element in this slot</param>
        public void RefreshWith(EquipmentIndex index, EquipmentElement element)
        {
            _slotIndex = index;
            _equipmentElement = element;
            SlotName = GetSlotName(index);
            EmptySlotSprite = GetEmptySlotSprite(index);

            if (!element.IsEmpty && element.Item != null)
            {
                HasItem = true;
                ItemName = element.Item.Name?.ToString() ?? "Unknown";
                ItemModifier = element.ItemModifier != null ? 1 : 0;

                // Create ItemImageIdentifierVM ONLY when we have a valid item
                ImageIdentifier = new ItemImageIdentifierVM(element.Item, "");
            }
            else
            {
                HasItem = false;
                ItemName = "";
                ItemModifier = 0;

                // Set to null for empty slots - prevents ItemImageTextureProvider crash
                ImageIdentifier = null;
            }
        }

        /// <summary>
        /// Shows the native item tooltip with full item details (stats, armor values, etc.)
        /// Called by Command.HoverBegin in the Gauntlet XML.
        /// Uses the native InformationManager.ShowTooltip system for rich item tooltips.
        /// </summary>
        public void ExecuteShowItemTooltip()
        {
            // Show tooltip if we have a valid item
            // CRITICAL: Must use typeof(ItemObject), not typeof(EquipmentElement)
            // The tooltip system is registered for ItemObject, but expects EquipmentElement in args
            if (!_equipmentElement.IsEmpty && _equipmentElement.Item != null)
            {
                InformationManager.ShowTooltip(typeof(ItemObject), new object[] { _equipmentElement });
            }
        }

        /// <summary>
        /// Hides the item tooltip.
        /// Called by Command.HoverEnd in the Gauntlet XML.
        /// </summary>
        public void ExecuteHideItemTooltip()
        {
            InformationManager.HideTooltip();
        }

        /// <summary>
        /// Resets this slot to empty state.
        /// </summary>
        public void Reset()
        {
            _slotIndex = EquipmentIndex.None;
            SlotName = "";
            HasItem = false;
            ItemName = "";
            ItemModifier = 0;
            ImageIdentifier = null;
            EmptySlotSprite = "Inventory\\empty_head_slot";
            IsSelected = false;
        }

        public override void OnFinalize()
        {
            base.OnFinalize();
            ImageIdentifier?.OnFinalize();
            _imageIdentifier = null;
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
        /// Used to toggle visibility between item image and empty slot sprite.
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
                    OnPropertyChanged(nameof(IsNotHasItem));
                }
            }
        }

        /// <summary>
        /// Gets whether this slot does NOT have an item.
        /// Used for visibility binding on empty slot sprite.
        /// </summary>
        [DataSourceProperty]
        public bool IsNotHasItem => !_hasItem;

        /// <summary>
        /// Gets the image identifier for displaying the item icon.
        /// NULL when slot is empty - preventings crash.
        /// </summary>
        [DataSourceProperty]
        public ItemImageIdentifierVM ImageIdentifier
        {
            get => _imageIdentifier;
            private set
            {
                if (_imageIdentifier != value)
                {
                    _imageIdentifier?.OnFinalize();
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

        /// <summary>
        /// Gets the sprite to display when slot is empty.
        /// Different sprites for different slot types (head, body, weapon, etc.)
        /// </summary>
        [DataSourceProperty]
        public string EmptySlotSprite
        {
            get => _emptySlotSprite;
            private set
            {
                if (_emptySlotSprite != value)
                {
                    _emptySlotSprite = value;
                    OnPropertyChangedWithValue(value, nameof(EmptySlotSprite));
                }
            }
        }

        /// <summary>
        /// Gets or sets whether this equipment slot is currently selected.
        /// Used by CommanderEquipmentSlotWidget to control background state (Default vs Selected).
        /// Can be set externally for future slot selection/management features.
        /// </summary>
        [DataSourceProperty]
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        /// <summary>
        /// Gets the equipment element for this slot.
        /// Bound directly to CommanderEquipmentSlotWidget for tooltip display.
        /// Widget handles tooltips in OnHoverBegin/OnHoverEnd using this value.
        /// </summary>
        [DataSourceProperty]
        public EquipmentElement EquipmentElement => _equipmentElement;

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets a display name for the equipment slot.
        /// </summary>
        private static string GetSlotName(EquipmentIndex index)
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
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Gets the appropriate empty slot sprite for the equipment slot type.
        /// These are the native game sprites used in the inventory screen.
        /// </summary>
        private static string GetEmptySlotSprite(EquipmentIndex index)
        {
            return index switch
            {
                EquipmentIndex.Head => "Inventory\\empty_head_slot",
                EquipmentIndex.Cape => "Inventory\\empty_cape_slot",
                EquipmentIndex.Body => "Inventory\\empty_body_slot",
                EquipmentIndex.Gloves => "Inventory\\empty_glove_slot",
                EquipmentIndex.Leg => "Inventory\\empty_boot_slot",
                EquipmentIndex.Horse => "Inventory\\empty_horse_slot",
                EquipmentIndex.HorseHarness => "Inventory\\empty_saddle_slot",
                EquipmentIndex.Weapon0 => "Inventory\\empty_weapon_slot",
                EquipmentIndex.Weapon1 => "Inventory\\empty_weapon_slot",
                EquipmentIndex.Weapon2 => "Inventory\\empty_weapon_slot",
                EquipmentIndex.Weapon3 => "Inventory\\empty_weapon_slot",
                EquipmentIndex.ExtraWeaponSlot => "Inventory\\empty_banner_slot",
                _ => "Inventory\\empty_weapon_slot"
            };
        }

        #endregion
    }
}
