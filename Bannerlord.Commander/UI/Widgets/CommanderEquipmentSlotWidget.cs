using TaleWorlds.Core;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets;

namespace Bannerlord.Commander.UI.Widgets
{
    /// <summary>
    /// Custom equipment slot widget that extends ButtonWidget for native hover and click support.<br />
    /// Key features:<br />
    /// - Extends ButtonWidget for native Command.Click and hover support<br />
    /// - Handles tooltips directly in OnHoverBegin/OnHoverEnd via InformationManager<br />
    /// - Manages background visual state (Default/Selected)
    /// </summary>
    public class CommanderEquipmentSlotWidget : ButtonWidget
    {
        private Widget _background;
        private ImageIdentifierWidget _imageIdentifier;
        private bool _isSelected;
        private bool _lastSelectedState;
        private EquipmentElement _equipmentElement;

        public CommanderEquipmentSlotWidget(UIContext context) : base(context)
        {
        }

        protected override void OnLateUpdate(float dt)
        {
            base.OnLateUpdate(dt);

            // Update background state when selection changes
            // Native uses OnUpdate, but OnLateUpdate is fine for non-critical UI updates
            if (_background != null && _lastSelectedState != _isSelected)
            {
                _background.SetState(_isSelected ? "Selected" : "Default");
                _lastSelectedState = _isSelected;
            }
        }

        /// <summary>
        /// Called when the mouse begins hovering over this widget.
        /// Shows item tooltip directly using InformationManager.
        /// Follows native InventoryItemButtonWidget pattern.
        /// </summary>
        protected override void OnHoverBegin()
        {
            base.OnHoverBegin();

            // Show tooltip if we have a valid item
            if (!_equipmentElement.IsEmpty && _equipmentElement.Item != null)
            {
                InformationManager.ShowTooltip(typeof(ItemObject), new object[] { _equipmentElement });
            }
        }

        /// <summary>
        /// Called when the mouse stops hovering over this widget.
        /// Hides any active tooltip.
        /// Follows native InventoryItemButtonWidget pattern.
        /// </summary>
        protected override void OnHoverEnd()
        {
            base.OnHoverEnd();
            InformationManager.HideTooltip();
        }

        /// <summary>
        /// PropertyChanged event handler for ImageIdentifier.
        /// Hides the widget when no item image is present.
        /// Follows native InventoryEquippedItemSlotWidget pattern.
        /// </summary>
        private void ImageIdentifierOnPropertyChanged(PropertyOwnerObject owner, string propertyName, object value)
        {
            if (propertyName == "ImageId")
            {
                base.IsHidden = string.IsNullOrEmpty((string)value);
            }
        }

        /// <summary>
        /// Reference to the background Widget that displays slot background sprite.
        /// Set via Background="..\BackgroundId" in XML.
        /// Changed to Widget (not BrushWidget) to match native pattern.
        /// </summary>
        [Editor(false)]
        public Widget Background
        {
            get => _background;
            set
            {
                if (_background != value)
                {
                    _background = value;
                    // CRITICAL: AddState registers "Selected" state with the widget
                    // so that SetState("Selected") can activate it later.
                    // This is required even though the Brush defines the "Selected" style.
                    _background.AddState("Selected");
                    OnPropertyChanged<Widget>(value, nameof(Background));
                }
            }
        }

        /// <summary>
        /// Reference to the ImageIdentifierWidget child that displays the item image.
        /// Subscribes to PropertyChanged to detect when ImageId changes.
        /// Set via ImageIdentifier="ImageIdentifier" in XML.
        /// </summary>
        [Editor(false)]
        public ImageIdentifierWidget ImageIdentifier
        {
            get => _imageIdentifier;
            set
            {
                if (_imageIdentifier != value)
                {
                    // Unsubscribe from old widget
                    if (_imageIdentifier != null)
                    {
                        _imageIdentifier.PropertyChanged -= ImageIdentifierOnPropertyChanged;
                    }

                    _imageIdentifier = value;

                    // Subscribe to new widget
                    if (_imageIdentifier != null)
                    {
                        _imageIdentifier.PropertyChanged += ImageIdentifierOnPropertyChanged;
                    }

                    OnPropertyChanged<ImageIdentifierWidget>(value, nameof(ImageIdentifier));
                }
            }
        }

        /// <summary>
        /// Whether this slot is currently selected.
        /// Controls background widget state (Default vs Selected).
        /// Bound to ViewModel's IsSelected property.
        /// </summary>
        [Editor(false)]
        public new bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(value, nameof(IsSelected));
                }
            }
        }

        /// <summary>
        /// The equipment element in this slot.
        /// Used directly by the widget to show tooltips on hover.
        /// Bound from ViewModel's EquipmentElement property via XML binding.
        /// </summary>
        [Editor(false)]
        public EquipmentElement EquipmentElement
        {
            get => _equipmentElement;
            set
            {
                if (!_equipmentElement.Equals(value))
                {
                    _equipmentElement = value;
                    // Note: Cannot use OnPropertyChanged<T> for struct types (CS0452)
                    // This property is used for tooltip display, not UI binding
                }
            }
        }
    }
}
