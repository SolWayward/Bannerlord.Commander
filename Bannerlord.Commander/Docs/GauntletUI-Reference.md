# Bannerlord Gauntlet UI Reference

## File Structure (Critical)

```
YourMod/
  _Module/
    GUI/
      Prefabs/           <-- Movie XML files go HERE (required location)
        MyScreen.xml
      Brushes/           <-- Custom brush definitions (optional)
        MyBrushes.xml
```

**PITFALL:** XML files in `GUI/` root are NOT loaded. Must be in `GUI/Prefabs/`.

---

## Minimal Working Screen (C#)

```csharp
public class MyScreen : ScreenBase
{
    private GauntletLayer _layer;
    private MyViewModel _vm;

    protected override void OnInitialize()
    {
        base.OnInitialize();
        _vm = new MyViewModel();
        
        // Parameters: categoryId, localOrder, shouldClear
        _layer = new GauntletLayer("GauntletLayer", 100, false);
        _layer.LoadMovie("MyScreen", _vm);  // Loads GUI/Prefabs/MyScreen.xml
        
        AddLayer(_layer);
        _layer.InputRestrictions.SetInputRestrictions();
        _layer.IsFocusLayer = true;
        ScreenManager.TrySetFocus(_layer);
    }

    protected override void OnFinalize()
    {
        _vm?.OnFinalize();
        _vm = null;
        _layer = null;  // DO NOT manually release movie or remove layer
        base.OnFinalize();
    }

    protected override void OnFrameTick(float dt)
    {
        base.OnFrameTick(dt);
        if (Input.IsKeyPressed(InputKey.Escape))
            ScreenManager.PopScreen();
    }
}
```

**PITFALL:** Do NOT call `_layer.ReleaseMovie()` or `RemoveLayer()` in OnFinalize - causes NullReferenceException. Base class handles cleanup.

---

## Minimal Working XML

```xml
<Prefab>
  <Window>
    <Widget WidthSizePolicy="Fixed" HeightSizePolicy="Fixed" 
            SuggestedWidth="400" SuggestedHeight="300"
            HorizontalAlignment="Center" VerticalAlignment="Center"
            Sprite="BlankWhiteSquare_9" Color="#2a2a2aFF">
      <Children>
        <RichTextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed"
                        SuggestedHeight="40" VerticalAlignment="Top"
                        Brush="TownManagement.Description.Title.Text"
                        Text="@TitleProperty" />
                        
        <ButtonWidget WidthSizePolicy="Fixed" HeightSizePolicy="Fixed"
                      SuggestedWidth="100" SuggestedHeight="40"
                      VerticalAlignment="Bottom" HorizontalAlignment="Center"
                      DoNotPassEventsToChildren="true"
                      Command.Click="ExecuteMyMethod">
          <Children>
            <Widget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent"
                    Sprite="BlankWhiteSquare_9" Color="#555555FF" />
            <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent"
                        Brush="Clan.TabControl.Text" Text="Click Me" />
          </Children>
        </ButtonWidget>
      </Children>
    </Widget>
  </Window>
</Prefab>
```

**PITFALLS:**
- Child widgets MUST be wrapped in `<Children>` tags
- TextWidget/RichTextWidget MUST have a `Brush` attribute - no brush = no text rendered
- Without `DoNotPassEventsToChildren="true"` on buttons, clicks may not register

---

## ViewModel (C#)

```csharp
public class MyViewModel : ViewModel
{
    private string _titleProperty;

    [DataSourceProperty]
    public string TitleProperty
    {
        get => _titleProperty;
        set { _titleProperty = value; OnPropertyChangedWithValue(value, nameof(TitleProperty)); }
    }

    public void ExecuteMyMethod()  // Bound via Command.Click="ExecuteMyMethod"
    {
        // Handle click
    }
}
```

---

## Opening the Screen

```csharp
ScreenManager.PushScreen(new MyScreen());
```

---

## Known Working Brushes (from ImprovedGarrisons mod)

### Text Brushes
| Brush Name | Description |
|------------|-------------|
| `TownManagement.Description.Title.Text` | Gold/tan title text |
| `TownManagement.Description.Value.Text` | Value/status text |
| `Clan.TabControl.Text` | Standard UI text (tab buttons) |
| `Recruitment.Popup.Title.Text` | Popup/panel title style |
| `SPOptions.OptionName.Text` | Options menu text |
| `SPOptions.Description.Title.Text` | Options description title |
| `SPOptions.Description.Text` | Options description body |
| `SPOptions.GameKeysGroup.Title.Text` | Section group title |
| `SPOptions.Slider.Value.Text` | Slider value display |
| `SPOptions.Dropdown.Center.Text` | Dropdown center text |
| `EncounterTextBrush` | Encounter/notification text |

### Button/Control Brushes
| Brush Name | Description |
|------------|-------------|
| `ButtonBrush2` | Standard game button |
| `ButtonBrush4` | Alternative button style |
| `GameMenu.Extend.Button` | Expandable side panel button |
| `GameMenu.Extend.Button.Arrow` | Arrow icon for expand button |
| `Header.Tab.Center` | Tab button (center style) |
| `Header.Tab.Left` | Tab button (left end) |
| `Header.Tab.Right` | Tab button (right end) |
| `SPOptions.Checkbox.Empty.Button` | Empty checkbox |
| `SPOptions.Checkbox.Full.Button` | Filled checkbox |
| `SPOptions.Slider.Handle` | Slider handle |
| `SPOptions.GameKey.Button.Canvas` | Game key button background |
| `SPOptions.GameKey.Button.Frame` | Game key button frame |
| `ButtonRightArrowBrush1` | Right arrow button |
| `ButtonLeftArrowBrush1` | Left arrow button |

### Frame/Panel Brushes  
| Brush Name | Description |
|------------|-------------|
| `Frame1Brush` | Standard panel frame |
| `InnerFrameShadow1Brush` | Panel with inner shadow |
| `TroopSelection.Card` | Card-style panel |
| `Encyclopedia.SubPage.Element` | Encyclopedia element style |
| `SPOptions.CollapserLine` | Collapsible section line |
| `EscapeMenu.Background` | Escape menu background |
| `FaceGen.Scrollbar.Handle` | Scrollbar handle |

---

## Known Working Sprites (from ImprovedGarrisons mod)

### Basic/Utility
| Sprite Name | Description |
|-------------|-------------|
| `BlankWhiteSquare_9` | Solid color rectangle (use with Color attr) |

### Decorative
| Sprite Name | Description |
|-------------|-------------|
| `StdAssets\tabbar_popup` | Decorative header banner |
| `StdAssets\game_menu_hinges` | Decorative hinges |
| `StdAssets\Popup\scrollable_field_gradient` | Gradient for scrollable areas |

### Dividers
| Sprite Name | Description |
|-------------|-------------|
| `SPGeneral\TownManagement\title_divider` | Horizontal divider line |
| `SPGeneral\TownManagement\vertical_divider` | Vertical divider |

### Slider Components
| Sprite Name | Description |
|-------------|-------------|
| `SPGeneral\SPOptions\standart_slider_canvas` | Slider background |
| `SPGeneral\SPOptions\standart_slider_fill` | Slider fill bar |
| `SPGeneral\SPOptions\standart_slider_frame` | Slider frame |
| `SPGeneral\SPOptions\checkbox_full` | Filled checkbox sprite |

### Scrollbar
| Sprite Name | Description |
|-------------|-------------|
| `MPLobby\CustomServer\lobby_slider_bed` | Scrollbar track |

---

## Widget Reference

| Widget | Use Case |
|--------|----------|
| `Widget` | Container, backgrounds |
| `TextWidget` | Simple text |
| `RichTextWidget` | Styled text, supports brush overrides |
| `ButtonWidget` | Clickable button |
| `ListPanel` | Vertical/horizontal list layout |
| `GridWidget` | Grid layout |
| `ScrollablePanel` | Scrollable content area |
| `ScrollbarWidget` | Scrollbar control |
| `ImageWidget` | Display images/icons |
| `BrushWidget` | Apply brush as background |
| `HintWidget` | Tooltip trigger |
| `SliderWidget` | Slider control |
| `TabControl` | Tab container |
| `TabToggleWidget` | Tab button |

---

## Common Attributes

| Attribute | Values |
|-----------|--------|
| `WidthSizePolicy` | `Fixed`, `StretchToParent`, `CoverChildren` |
| `HeightSizePolicy` | `Fixed`, `StretchToParent`, `CoverChildren` |
| `HorizontalAlignment` | `Left`, `Center`, `Right` |
| `VerticalAlignment` | `Top`, `Center`, `Bottom` |
| `Margin[Top/Bottom/Left/Right]` | Pixels |
| `Position[X/Y]Offset` | Pixel offset from alignment |
| `Color` | `#RRGGBBAA` hex |
| `AlphaFactor` | 0.0 - 1.0 transparency |
| `IsVisible` | `true`/`false` or `@BoolProperty` |
| `IsEnabled` | `true`/`false` |
| `DoNotAcceptEvents` | Ignore mouse events |
| `DoNotPassEventsToChildren` | Block event propagation |
| `UpdateChildrenStates` | Update child visual states |
| `ClipContents` | Clip overflow |

### Brush Overrides (add to any brush reference)
```xml
Brush.FontSize="24"
Brush.GlobalColor="#FF0000FF"
Brush.AlphaFactor="0.5"
Brush.TextHorizontalAlignment="Left"
```

---

## Layout Methods

For `ListPanel`:
```xml
LayoutImp.LayoutMethod="VerticalBottomToTop"
LayoutImp.LayoutMethod="HorizontalLeftToRight"
```

For `GridWidget`:
```xml
LayoutImp="GridLayout" ColumnCount="2" DefaultCellHeight="30" DefaultCellWidth="80"
```

---

## Data Binding

- `@PropertyName` - Binds to ViewModel property
- `{PropertyName}` - DataSource reference for nested ViewModels
- `Command.Click="MethodName"` - Calls ViewModel method on click
- `Command.AlternateClick="MethodName"` - Right-click handler
- `Command.HoverBegin="MethodName"` - Mouse enter
- `Command.HoverEnd="MethodName"` - Mouse leave
- `IsVisible="@BoolProperty"` - Visibility binding
- `IsSelected="@BoolProperty"` - Selection state binding

### ItemTemplate for Lists
```xml
<ListPanel DataSource="{MyCollection}">
  <ItemTemplate>
    <TextWidget Text="@ItemProperty" />
  </ItemTemplate>
</ListPanel>
```

---

## Standard Prefabs (from game)

These are reusable prefabs you can include:
```xml
<Standard.Background />
<Standard.TopPanel Parameter.Title="@Title" />
<Standard.DialogCloseButtons Parameter.CancelButtonAction="ExecuteCancel" 
                              Parameter.DoneButtonAction="ExecuteDone" />
<Standard.DropdownWithHorizontalControl Parameter.SelectorDataSource="{Selector}" />
<Standard.VerticalScrollbar />
```

---

## Quick Checklist

- [ ] XML is in `GUI/Prefabs/` folder
- [ ] Child widgets wrapped in `<Children>` tags  
- [ ] Text widgets have `Brush` attribute
- [ ] Buttons have `DoNotPassEventsToChildren="true"`
- [ ] OnFinalize does NOT manually release movie/layer
- [ ] ViewModel properties have `[DataSourceProperty]` attribute
- [ ] ViewModel methods for commands are public
