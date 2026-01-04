# Equipment Slot Click Crash - Final Solution

## Problem Summary

Equipment slots were crashing when clicked because `InventoryEquippedItemSlotWidget` (which extends `ButtonWidget`) fires `ExecuteSelectItem()` on the DataSource, expecting an `SPItemVM` with a `ProcessItemSelect` delegate. Since `EquipmentSlotVM` doesn't have this delegate, clicking caused a null reference exception.

Additionally, the native widget requires an `InventoryScreenWidget` ancestor for background rendering, which didn't exist in our hierarchy.

## Solution Implemented

Created a custom `CommanderEquipmentSlotWidget` that:
1. Extends `Widget` (NOT `ButtonWidget`) - eliminates the automatic click handling that was causing crashes
2. Manages background BrushWidget state directly via `OnLateUpdate()`
3. Preserves `ImageIdentifierWidget` as child - maintains async item texture loading with spinner
4. Supports click handling via `Command.Click` binding on parent Widget container

## Files Modified

### 1. New Widget: `Bannerlord.Commander/UI/Widgets/CommanderEquipmentSlotWidget.cs`
- Custom widget extending `Widget` (not ButtonWidget)
- Has `Background`, `ImageIdentifier`, and `IsSelected` properties
- `OnLateUpdate()` manages background state (Default/Selected) without requiring InventoryScreenWidget
- Auto-hides widget when no image is present (like native widget)

### 2. Updated ViewModel: `Bannerlord.Commander/UI/ViewModels/HeroEditor/EquipmentSlotVM.cs`
- Added `IsSelected` property with DataSource binding
- Added `ExecuteSelectSlot()` command handler that toggles selection
- Updated `Reset()` to clear selection state
- Ready for future equipment management features

### 3. Updated XML: `Bannerlord.Commander/_Module/GUI/Prefabs/HeroEditor/HeroEditorPanel.xml`
- Replaced all 12 `InventoryEquippedItemSlotWidget` instances with `CommanderEquipmentSlotWidget`
- Added `Command.Click="ExecuteSelectSlot"` to all slot container widgets
- Removed problematic properties (`TargetEquipmentIndex`, `ItemType`) that were part of native inventory system

## What Works Now

1. **No crash on click** - Clicking slots toggles selection instead of crashing
2. **Background rendering** - Equipment slot backgrounds render correctly with native brushes
3. **Selection highlighting** - Selected state applies highlight sprites via brush system
4. **Async item loading** - ImageIdentifierWidget preserves async texture loading with spinner
5. **Future ready** - Click handler in place for future equipment management features

## Architecture Benefits

| Aspect | Native Widget | Custom Solution |
|--------|--------------|----------------|
| Click behavior | Crashes (wrong VM type) | Toggles selection (safe) |
| Background rendering | Requires InventoryScreenWidget | Works independently |
| Async loading | Works | Preserved |
| Complexity | Deep inheritance chain | Simple Widget subclass |
| Future extensibility | Limited | Easily enhanced |

## Future Enhancements

The `ExecuteSelectSlot()` command is currently a simple toggle. To implement full equipment management:

1. In `HeroEquipmentVM`, subscribe to slot selection changes
2. When a slot is selected, open an equipment picker UI
3. Use BLGM equipment management APIs to change equipment
4. Refresh the slot ViewModel with new equipment

Example enhancement (future):
```csharp
// In ExecuteSelectSlot():
public void ExecuteSelectSlot()
{
    IsSelected = !IsSelected;
    if (IsSelected)
    {
        // Notify parent VM to open equipment picker for this slot
        _parentVM?.OpenEquipmentPickerForSlot(_slotIndex);
    }
}
```

## Technical Notes

- Schema warnings about `CommanderEquipmentSlotWidget` are expected (incomplete custom schema)
- Native inventory brushes (InventoryHelmetSlot, etc.) work perfectly with custom widget
- Portrait frame sprite (portrait_cart) renders correctly
- All 12 slots use consistent pattern for maintainability

## Testing

To verify the fix:
1. Launch game and open Hero Editor
2. Click any equipment slot - should toggle selection highlighting
3. Verify backgrounds render for all slots
4. Verify item images load with spinner animation
5. Verify portrait frames appear around items
6. Verify no crashes occur when clicking slots
