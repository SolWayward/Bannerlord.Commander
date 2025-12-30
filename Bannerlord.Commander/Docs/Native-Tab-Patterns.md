# Native Bannerlord Tab and Selection Patterns

## Overview
This document explains how Bannerlord's native UI implements tab selection (Clan Screen) and item selection with gold borders (Character Screen Skills).

## Key Insight
Native Bannerlord does NOT use IsVisible bindings on child widgets for selection state. Instead:
- **ButtonWidget** with `IsSelected` attribute bound to ViewModel
- **Multi-layer brushes** with state-based styling (Default, Hovered, Selected, Pressed)
- **AlphaFactor** to show/hide layers based on state

---

## Clan Screen Tab Implementation

**Source File:** `Modules/SandBox/GUI/Prefabs/Clan/ClanScreen.xml` (Lines 209-235)

### Widget Structure:
```xml
<ButtonWidget DoNotPassEventsToChildren="true" 
              WidthSizePolicy="Fixed" 
              HeightSizePolicy="Fixed" 
              SuggestedWidth="!Header.Tab.Center.Width.Scaled" 
              SuggestedHeight="!Header.Tab.Center.Height.Scaled" 
              Brush="Header.Tab.Center" 
              Command.Click="SetSelectedCategory" 
              CommandParameter.Click="1" 
              IsSelected="@IsPartiesSelected" 
              UpdateChildrenStates="true">
    <Children>
        <TextWidget WidthSizePolicy="StretchToParent" 
                    HeightSizePolicy="StretchToParent" 
                    Brush="Clan.TabControl.Text" 
                    Text="@PartiesText" />
    </Children>
</ButtonWidget>
```

### Key Attributes:
- `IsSelected="@IsPartiesSelected"` - Binds to ViewModel boolean property
- `UpdateChildrenStates="true"` - Propagates state to children
- `Brush="Header.Tab.Center"` - Multi-layer brush with states
- `CommandParameter.Click="1"` - Passes tab index to command

---

## Tab Brush Definition

**Source File:** `Modules/Native/GUI/Brushes/Standard.xml` (Line 342)

### Header.Tab.Center Brush:
```xml
<Brush Name="Header.Tab.Center">
  <Layers>
    <BrushLayer Name="Default" Sprite="StdAssets\page_button_center" />
    <BrushLayer Name="Selected" Sprite="StdAssets\page_button_center_selected" />
  </Layers>
  <Styles>
    <Style Name="Default">
      <BrushLayer Name="Default" AlphaFactor="1" ColorFactor="1" />
      <BrushLayer Name="Selected" AlphaFactor="0" />
    </Style>
    <Style Name="Hovered">
      <BrushLayer Name="Default" AlphaFactor="1" ColorFactor="1.1" />
      <BrushLayer Name="Selected" AlphaFactor="0" />
    </Style>
    <Style Name="Selected">
      <BrushLayer Name="Default" AlphaFactor="0" />
      <BrushLayer Name="Selected" AlphaFactor="1" ColorFactor="2" Color="#FFC349FF" />
    </Style>
    <Style Name="Pressed">
      <BrushLayer Name="Default" AlphaFactor="1" ColorFactor="0.8" />
      <BrushLayer Name="Selected" AlphaFactor="0" />
    </Style>
  </Styles>
  <SoundProperties>
    <EventSounds>
      <EventSound EventName="Click" Audio="tab" />
    </EventSounds>
  </SoundProperties>
</Brush>
```

### How It Works:
1. Two layers defined: "Default" and "Selected"
2. **Default Style**: Default layer visible (AlphaFactor=1), Selected layer hidden (AlphaFactor=0)
3. **Selected Style**: Default layer hidden (AlphaFactor=0), Selected layer visible with gold color
4. Color="#FFC349FF" is the gold color used
5. ColorFactor="2" amplifies brightness

---

## Character Screen Skill Selection

**Source File:** `Modules/SandBox/GUI/Prefabs/CharacterDeveloper/SkillGridItem.xml` (Line 25)

### Widget Structure:
```xml
<SkillGridItemButtonWidget WidthSizePolicy="Fixed" 
                           HeightSizePolicy="Fixed" 
                           CanLearnBrush="CharacterDeveloper.SkillButtonBackground" 
                           CannotLearnBrush="CharacterDeveloper.SkillButtonBackground.CannotLearn" 
                           Command.Click="ExecuteInspect" 
                           IsSelected="@IsInspected" 
                           Command.AlternateClick="ExecuteShowSkillConcept">
```

### Skill Button Brush:
**Source File:** `Modules/SandBox/GUI/Brushes/CharacterDeveloper.xml` (Line 267)

```xml
<Brush Name="CharacterDeveloper.SkillButtonBackground">
  <Layers>
    <BrushLayer Name="Default" Sprite="CharacterDeveloper\SkillBackgrounds\skill_card" 
                ExtendLeft="18" ExtendTop="18" ExtendRight="17" ExtendBottom="18" 
                IsHidden="true" OverlayMethod="CoverWithTexture" 
                OverlaySprite="stone_texture_overlay" />
    <BrushLayer Name="Selected" Sprite="CharacterDeveloper\SkillBackgrounds\skill_card_selection" 
                ExtendLeft="19" ExtendTop="18" ExtendRight="18" ExtendBottom="18" 
                IsHidden="true" OverlayMethod="CoverWithTexture" 
                OverlaySprite="stone_texture_overlay" />
  </Layers>
  <Styles>
    <Style Name="Default">
      <BrushLayer Name="Default" AlphaFactor="0.45" IsHidden="false" />
    </Style>
    <Style Name="Hovered">
      <BrushLayer Name="Default" AlphaFactor="0.8" IsHidden="false" />
    </Style>
    <Style Name="Selected">
      <BrushLayer Name="Selected" ColorFactor="1" IsHidden="false" />
    </Style>
    <Style Name="Disabled">
      <BrushLayer Name="Default" ColorFactor="0.8" AlphaFactor="0.8" IsHidden="false" />
    </Style>
  </Styles>
</Brush>
```

---

## Implementation Pattern Summary

### Required Components:

1. **ButtonWidget Attributes:**
   - `IsSelected="@PropertyName"` - Boolean binding to ViewModel
   - `UpdateChildrenStates="true"` - Propagate state changes
   - `DoNotPassEventsToChildren="true"` - Required for click handling
   - `Brush="BrushName"` - Multi-layer brush

2. **Brush Structure:**
   - Define two BrushLayers: "Default" and "Selected"
   - Default layer: Normal appearance (gray/dark)
   - Selected layer: Active appearance (gold)

3. **Brush Styles:**
   - `Default`: Show Default layer, hide Selected layer
   - `Hovered`: Brighten Default layer (ColorFactor > 1)
   - `Selected`: Hide Default layer, show Selected layer with gold color
   - `Pressed`: Darken Default layer (ColorFactor < 1)

4. **Layer Visibility Control:**
   - `AlphaFactor="0"` to hide layer
   - `AlphaFactor="1"` to show layer
   - Alternative: `IsHidden="true/false"`

5. **Gold Color:**
   - Color="#FFC349FF" - Native Bannerlord gold
   - ColorFactor="2" or similar to amplify

---

## Why IsVisible Bindings Don't Work

The IsVisible attribute on child widgets does NOT integrate with ButtonWidget's state system. When you bind IsVisible to a ViewModel property:
- The widget shows/hides based on property value
- BUT it doesn't respond to ButtonWidget's Default/Hovered/Selected/Pressed states

Native pattern uses ButtonWidget's built-in state management:
- ButtonWidget receives click and state changes
- IsSelected property determines "Selected" state
- Brush automatically swaps to Selected Style
- No child widget visibility manipulation needed

---

## Native Brushes Available

Can use native brushes directly:
- `Header.Tab.Left` - Left tab position
- `Header.Tab.Center` - Center tab position  
- `Header.Tab.Right` - Right tab position
- `Clan.TabControl.Text` - Tab text styling

---

## Example Implementation

### Custom Tab Brush (using BlankWhiteSquare_9):
```xml
<Brush Name="Commander.Tab">
  <Layers>
    <BrushLayer Name="Default" Sprite="BlankWhiteSquare_9" />
    <BrushLayer Name="Selected" Sprite="BlankWhiteSquare_9" />
  </Layers>
  <Styles>
    <Style Name="Default">
      <BrushLayer Name="Default" AlphaFactor="1" Color="#3A3A3AFF" />
      <BrushLayer Name="Selected" AlphaFactor="0" />
    </Style>
    <Style Name="Hovered">
      <BrushLayer Name="Default" AlphaFactor="1" ColorFactor="1.3" Color="#3A3A3AFF" />
      <BrushLayer Name="Selected" AlphaFactor="0" />
    </Style>
    <Style Name="Selected">
      <BrushLayer Name="Default" AlphaFactor="0" />
      <BrushLayer Name="Selected" AlphaFactor="1" Color="#FFC349FF" />
    </Style>
    <Style Name="Pressed">
      <BrushLayer Name="Default" AlphaFactor="1" ColorFactor="0.8" Color="#3A3A3AFF" />
      <BrushLayer Name="Selected" AlphaFactor="0" />
    </Style>
  </Styles>
</Brush>
```

### Tab Widget:
```xml
<ButtonWidget DoNotPassEventsToChildren="true"
              WidthSizePolicy="Fixed" HeightSizePolicy="Fixed"
              SuggestedWidth="150" SuggestedHeight="40"
              Brush="Commander.Tab"
              Command.Click="ExecuteSelectKingdoms"
              IsSelected="@IsKingdomsSelected"
              UpdateChildrenStates="true">
  <Children>
    <TextWidget WidthSizePolicy="StretchToParent"
                HeightSizePolicy="StretchToParent"
                Brush="Clan.TabControl.Text"
                Text="Kingdoms" />
  </Children>
</ButtonWidget>
```
