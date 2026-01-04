using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets;

namespace Bannerlord.Commander.UI.Widgets
{
    /// <summary>
    /// Custom equipment slot widget that renders backgrounds and manages selection state
    /// without the InventoryScreenWidget dependency that causes crashes.
    /// 
    /// Does NOT extend ButtonWidget - click handling is via Command.Click on parent Widget.
    /// Preserves async item loading by keeping ImageIdentifierWidget as child.
    /// </summary>
    public class CommanderEquipmentSlotWidget : Widget
    {
        private BrushWidget _background;
        private ImageIdentifierWidget _imageIdentifier;
        private bool _isSelected;
        private bool _lastSelectedState;
        private bool _isInitialized;

        public CommanderEquipmentSlotWidget(UIContext context) : base(context)
        {
        }

        protected override void OnLateUpdate(float dt)
        {
            base.OnLateUpdate(dt);

            // Initialize background state once after binding resolution
            if (_background != null && !_isInitialized)
            {
                _background.SetState("Default");
                _isInitialized = true;
            }

            // Update background state when selection changes
            if (_background != null && _lastSelectedState != _isSelected)
            {
                _background.SetState(_isSelected ? "Selected" : "Default");
                _lastSelectedState = _isSelected;
            }

            // Hide widget when no image is present (like native InventoryEquippedItemSlotWidget does)
            if (_imageIdentifier != null)
            {
                base.IsHidden = string.IsNullOrEmpty(_imageIdentifier.ImageId);
            }
        }

        /// <summary>
        /// Reference to the background BrushWidget that displays slot background sprite.
        /// Set via Background="..\BackgroundId" in XML.
        /// </summary>
        [Editor(false)]
        public BrushWidget Background
        {
            get => _background;
            set
            {
                if (_background != value)
                {
                    _background = value;
                    if (_background != null)
                    {
                        // Add Selected state to brush so SetState() can toggle it
                        _background.AddState("Selected");
                    }
                    OnPropertyChanged<BrushWidget>(value, nameof(Background));
                    _isInitialized = false; // Re-initialize with new background
                }
            }
        }

        /// <summary>
        /// Reference to the ImageIdentifierWidget child that displays the item image.
        /// Used to auto-hide the slot when no item is present.
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
                    _imageIdentifier = value;
                    OnPropertyChanged<ImageIdentifierWidget>(value, nameof(ImageIdentifier));
                }
            }
        }

        /// <summary>
        /// Whether this slot is currently selected.
        /// Controls background brush state (Default vs Selected).
        /// Bound to ViewModel's IsSelected property.
        /// </summary>
        [Editor(false)]
        public bool IsSelected
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
    }
}
