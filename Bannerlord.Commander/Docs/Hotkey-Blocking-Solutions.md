# Hotkey Blocking Solutions for Text Input in Bannerlord

> **IMPORTANT**: This report excludes Harmony-based solutions. Harmony is NOT an option for this project due to third-party dependency restrictions.

## Problem Statement

When typing in the filter text input field (`EditableTextWidget`), certain letters trigger Bannerlord's built-in hotkeys instead of being typed:

- Lowercase 'c' opens the Character screen instead of typing 'c'
- Lowercase 'i' opens the Inventory screen instead of typing 'i'
- Lowercase 'n' opens the Encyclopedia instead of typing 'n'
- Shift+C works because the game detects it as a different key combination

ESC correctly closes our menu instead of opening the game menu - this suggests the game checks for open menus before processing ESC.

### Root Cause Analysis

**From Gemini's analysis:**
> The Native MapScreen listens to the global GameKey system (via HotKeyManager), which runs parallel to (and often ignores) the Gauntlet UI input bubbling. Your UI consumes the character 'c', but the Global Input system still sees the physical key 'C' pressed and triggers the "Open Character Screen" action before your layer can stop it.

**Critical insight:** The `MapScreen` is notorious for ignoring `InputUsageMask` because it considers itself a "Game State" rather than a "UI Layer." The native MapScreen runs on a different thread priority/logic loop that cannot be beaten with standard UI bubbling.

**Key insight:** The current `ConsumeHotkeyInputs` method in [`CommanderGauntletScreen.cs`](../UI/Screens/CommanderGauntletScreen.cs) fails because it checks the Layer's input context. Even if you consume it there, the **Global input state** remains touched, and the MapScreen reads from the **Global state**, not the layer's local context.

---

## Current Implementation Status

In [`CommanderGauntletScreen.cs`](../UI/Screens/CommanderGauntletScreen.cs), the following approaches have been tried and **failed**:

- `SetInputRestrictions()` on the GauntletLayer
- `IsFocusLayer = true`
- `ScreenManager.TrySetFocus()`
- Manually consuming keys via `IsKeyPressed()`/`IsKeyReleased()` in `OnFrameTick()`
- Setting high layer order (10000)

**None work** because they operate at the Gauntlet layer level, while hotkeys are processed at the global `HotKeyManager` level.

---

## How to Decompile TaleWorlds Assemblies

To research how native screens (like Encyclopedia) handle text input, decompile the game's assemblies:

### Method 1: Using dnSpy (Recommended)

1. **Download dnSpy**
   - Get from: https://github.com/dnSpy/dnSpy/releases
   - Free, open-source .NET debugger and assembly editor

2. **Locate Bannerlord Assemblies**
   ```
   Steam\steamapps\common\Mount & Blade II Bannerlord\bin\Win64_Shipping_Client\
   ```
   
   Key assemblies:
   | Assembly | Contents |
   |----------|----------|
   | `TaleWorlds.CampaignSystem.dll` | Encyclopedia ViewModels, MapScreen |
   | `TaleWorlds.Library.dll` | Core ViewModel classes |
   | `TaleWorlds.GauntletUI.dll` | Widget system, EditableTextWidget |
   | `TaleWorlds.InputSystem.dll` | HotKeyManager, InputKey, categories |
   | `TaleWorlds.ScreenSystem.dll` | ScreenBase, ScreenManager |

3. **Navigate to Target Classes**
   - Launch dnSpy
   - File -> Open -> Select DLL
   - Expand assembly tree
   - Navigate to target namespace/class
   - Right-click -> "Edit Class (C#)" for decompiled source

### Method 2: Using ILSpy

- Download: https://github.com/icsharpcode/ILSpy/releases
- Similar workflow to dnSpy

### Method 3: Using JetBrains dotPeek

- Download: https://www.jetbrains.com/decompiler/
- Free from JetBrains

### Key Classes to Research

```
TaleWorlds.InputSystem/
  - HotKeyManager.cs          <- Find category management APIs
  - GameKeyContext.cs         <- Category object (no SetEnabled method!)
  - Input.cs                  <- Look for ClearKeys(), SetKey() methods

TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia/
  - EncyclopediaHomeVM.cs     <- Has working search bar
  - EncyclopediaSearchResultVM.cs

TaleWorlds.GauntletUI/
  - EditableTextWidget.cs     <- Input handling methods
  - GauntletLayer.cs

TaleWorlds.ScreenSystem/
  - ScreenBase.cs             <- Input-related overrides
  - ScreenManager.cs

SandBox.View.Map/
  - MapScreen.cs              <- HandleInput method
```

---

## Solution Comparison: Roo vs Gemini

| Aspect | Roo's Analysis | Gemini's Analysis |
|--------|---------------|-------------------|
| Root Cause | HotKeyManager operates at different level than Gauntlet layer | Global GameKey system runs before/parallel to UI layer |
| Primary Solution | Study Encyclopedia + HotKey Category API | `Input.ClearKeys()` timing hack |
| Failed Approach | - | `HotKeyManager.GetCategory().SetEnabled()` - **DOES NOT EXIST** |
| Fallback | Custom widget, TextInquiry modal | Aggressive InputRestrictions |
| Agreement | Both identify global vs layer-local input as the core issue |

---

## Solutions (Prioritized)

### Solution 1: Global Input Clear - "The Global Input Eraser" (HIGHEST PRIORITY)

**Source: Gemini**

Since the Game Input system reads from the global input buffer, we can scrub that buffer clean immediately after your UI processes it but before the MapScreen gets a chance to see it.

This relies on `OnFrameTick` order. Since your screen is the `TopScreen`, your tick usually runs before the background MapScreen.

```csharp
protected override void OnFrameTick(float dt)
{
    base.OnFrameTick(dt);

    // 1. Let your VM/UI handle the input (typing 'c' into the box)
    _viewModel?.OnTick();
    
    // 2. THE FIX: Wipe the global input buffer.
    // This removes the 'C' key press from the global state so the MapScreen
    // (which runs its logic after this) sees nothing.
    // We only do this if we are actually typing or if the screen is fully active.
    
    // Check if any key is pressed that might trigger a menu
    if (_gauntletLayer.Input.IsKeyDown(InputKey.C) || 
        _gauntletLayer.Input.IsKeyDown(InputKey.I) || 
        _gauntletLayer.Input.IsKeyDown(InputKey.N) ||
        _gauntletLayer.Input.IsKeyDown(InputKey.K) ||
        _gauntletLayer.Input.IsKeyDown(InputKey.L) ||
        _gauntletLayer.Input.IsKeyDown(InputKey.P) ||
        _gauntletLayer.Input.IsKeyDown(InputKey.Q) ||
        _gauntletLayer.Input.IsKeyDown(InputKey.B) ||
        _gauntletLayer.Input.IsKeyDown(InputKey.T) ||
        _gauntletLayer.Input.IsKeyDown(InputKey.E))
    {
        // Force the engine to forget these keys were pressed this frame
        TaleWorlds.InputSystem.Input.ClearKeys();
    }
}
```

**Why this works:** Your `OnFrameTick` runs before the background MapScreen's logic. By calling `Input.ClearKeys()`, you wipe the global input buffer clean. When MapScreen later checks if 'C' was pressed, the buffer is empty.

**Alternative:** If `ClearKeys()` doesn't exist in your game version, try `Input.SetKey(key, false)` for each hotkey.

**Research Required:** Verify `Input.ClearKeys()` exists via dnSpy in `TaleWorlds.InputSystem.Input`.

---

### Solution 2: Aggressive InputRestrictions

**Source: Gemini**

Ensure your InputRestrictions are explicitly set to forbid everything, not just use defaults.

```csharp
// In OnInitialize and OnActivate
_gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
```

**Why this might fail:** MapScreen is notorious for ignoring `InputUsageMask` because it considers itself a "Game State" rather than a "UI Layer." If this doesn't work, you must use Solution 1.

**Note:** Your current code calls `SetInputRestrictions()` without parameters. Try the explicit version with `InputUsageMask.All`.

---

### ~~Solution 3: Disable HotKey Category~~ (API DOES NOT EXIST)

**Source: Initial Gemini suggestion - CORRECTED**

> **WARNING:** `HotKeyManager.GetCategory("CampaignHotKeyCategory")` returns a `GameKeyContext` which does **NOT** have a `SetEnabled()` method or property. This solution does not work as originally described.

The initial suggestion was:
```csharp
// THIS DOES NOT WORK - SetEnabled does not exist on GameKeyContext
var category = HotKeyManager.GetCategory("CampaignHotKeyCategory");
category.SetEnabled(false); // <-- This method does not exist
```

**Status:** SKIP THIS SOLUTION - API does not exist.

**Research needed:** Examine `GameKeyContext` via dnSpy to see if there's an alternative way to disable a category. There may be:
- A different method to toggle categories
- A property to set
- A different class that manages category state

---

### Solution 4: Study Encyclopedia Search Implementation

**Source: Roo**

The Encyclopedia has a working search bar. TaleWorlds solved this exact problem.

**Steps:**
1. Decompile `TaleWorlds.CampaignSystem.dll`
2. Navigate to `TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia`
3. Examine `EncyclopediaHomeVM` - find the search text property
4. Look at the Encyclopedia screen class initialization
5. Check for:
   - Special ViewModel property attributes
   - Custom widget types
   - Screen initialization patterns
   - HotKey category disabling calls
   - How they handle input differently

---

### Solution 5: Native Text Inquiry Modal (Workaround)

**Source: Roo**

Use Bannerlord's built-in text input dialog that has proper hotkey blocking.

```csharp
private void ShowFilterDialog()
{
    InformationManager.ShowTextInquiry(new TextInquiryData(
        titleText: "Filter Heroes",
        text: "Enter filter text:",
        isAffirmativeOptionShown: true,
        isNegativeOptionShown: true,
        affirmativeText: "Apply",
        negativeText: "Cancel",
        affirmativeAction: (text) => {
            FilterText = text;
            RefreshHeroList();
        },
        negativeAction: null,
        shouldInputBeObfuscated: false,
        maxLength: 50
    ));
}
```

| Pros | Cons |
|------|------|
| Guaranteed to work | Not inline with UI |
| Native UI consistency | Extra click required |
| No complex input handling | Modal interrupts workflow |

---

### Solution 6: Custom EditableTextWidget Subclass

**Source: Roo**

Create a custom widget that overrides input handling.

```csharp
namespace Bannerlord.Commander.UI.Widgets
{
    using TaleWorlds.GauntletUI;
    
    public class HotkeyBlockingTextWidget : EditableTextWidget
    {
        private bool _isFocused = false;
        
        public HotkeyBlockingTextWidget(UIContext context) : base(context)
        {
        }
        
        protected override void OnGainFocus()
        {
            base.OnGainFocus();
            _isFocused = true;
            // Notify that text input is active
        }
        
        protected override void OnLoseFocus()
        {
            base.OnLoseFocus();
            _isFocused = false;
        }
        
        // Override to consume keyboard events
        // NOTE: Method name needs verification via dnSpy
        protected override bool OnKeyEvent(InputKey key)
        {
            if (_isFocused)
            {
                bool handled = base.OnKeyEvent(key);
                return true; // Block propagation to hotkey system
            }
            return base.OnKeyEvent(key);
        }
    }
}
```

**Register in SubModule.cs:**
```csharp
protected override void OnSubModuleLoad()
{
    base.OnSubModuleLoad();
    UIResourceManager.WidgetFactory.AddCustomType(
        "HotkeyBlockingTextWidget",
        typeof(HotkeyBlockingTextWidget)
    );
}
```

**Use in XML:**
```xml
<HotkeyBlockingTextWidget WidthSizePolicy="StretchToParent"
                          HeightSizePolicy="StretchToParent"
                          Text="@FilterText" />
```

**Research Required:** Verify actual method names in `EditableTextWidget` via dnSpy.

---

### Solution 7: Push Custom GameState

**Source: Roo**

Some `GameState` types may automatically disable game hotkeys.

```csharp
namespace Bannerlord.Commander.UI.States
{
    using TaleWorlds.Core;
    
    public class CommanderMenuState : GameState
    {
        public override bool IsMenuState => true; // May disable game hotkeys
        
        protected override void OnActivate()
        {
            base.OnActivate();
            // Initialize screen here
        }
    }
}

// Usage:
GameStateManager.Current.PushState(new CommanderMenuState());
```

**Research Required:** Check how `GameState.IsMenuState` affects input processing via dnSpy.

---

### Solution 8: Override Screen Input Methods

**Source: Roo**

Research `ScreenBase` for input-related overrides.

```csharp
public class CommanderGauntletScreen : ScreenBase
{
    // Research these potential overrides via dnSpy:
    public override bool IsInputEnabled => false;
    public override bool IsViewportInputEnabled => false;
    public override bool IsMouseVisible => true;
    public override bool IsInputCaptured => true;
    
    protected override bool HandleInput(InputContext input)
    {
        // Block hotkey processing
        return true;
    }
}
```

---

### Solution 9: Low-Level Windows API Hook (LAST RESORT)

**Source: Roo**

Use P/Invoke to intercept keyboard input at OS level. Only use if ALL other solutions fail.

```csharp
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Bannerlord.Commander.UI.Services
{
    public class KeyboardHook : IDisposable
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;
        
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
        
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        
        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        
        public static bool SuppressHotkeys { get; set; }
        
        public void Install()
        {
            _proc = HookCallback;
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                _hookID = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, 
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }
        
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN && SuppressHotkeys)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                
                // Block A-Z (65-90) when text input is active
                if (vkCode >= 65 && vkCode <= 90)
                {
                    return (IntPtr)1; // Block key from reaching game
                }
            }
            
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
        
        public void Dispose()
        {
            if (_hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookID);
            }
        }
    }
}
```

**Warning:** This is invasive and may have unintended consequences. Use only as absolute last resort.

---

## Recommended Investigation Order

```
1. Try Input.ClearKeys() in OnFrameTick (Solution 1)
   |
   +--[Works?]---> Done!
   |
   +--[Fails]---> 2. Try aggressive InputRestrictions with InputUsageMask.All (Solution 2)
                      |
                      +--[Works?]---> Done!
                      |
                      +--[Fails]---> 3. Decompile Encyclopedia, study their pattern (Solution 4)
                                        |
                                        +--[Found pattern]---> Implement it
                                        |
                                        +--[No pattern]---> 4. Try TextInquiry modal (workaround) (Solution 5)
                                                               |
                                                               +--[Acceptable UX?]---> Done!
                                                               |
                                                               +--[Need inline]---> 5. Custom widget (Solution 6)
                                                                                       |
                                                                                       +--[Fails]---> 6. GameState approach (Solution 7)
                                                                                                         |
                                                                                                         +--[Fails]---> 7. Windows hook (last resort) (Solution 9)
```

**Note:** Solution 3 (HotKey Category Disable via `SetEnabled`) is confirmed NON-FUNCTIONAL. Skip it.

---

## Testing Checklist

For each solution attempted:

- [ ] Lowercase letters (a-z) work in text input
- [ ] Uppercase letters (A-Z) work in text input  
- [ ] Numbers (0-9) work in text input
- [ ] Special characters work
- [ ] Character screen (C) does NOT open when typing 'c'
- [ ] Inventory screen (I) does NOT open when typing 'i'
- [ ] Encyclopedia (N) does NOT open when typing 'n'
- [ ] Other hotkeys (K, L, P, Q, B, T, E) do NOT trigger
- [ ] Function keys (F1-F12) do NOT trigger
- [ ] ESC still closes your menu correctly
- [ ] Input works when menu first opens
- [ ] Input works after clicking around UI
- [ ] Hotkeys are RE-ENABLED after closing the menu
- [ ] No performance impact
- [ ] No exceptions in logs

---

## Summary

**Primary solution: `Input.ClearKeys()`** - This is the most promising non-Harmony solution. It attacks the problem at the right level by wiping the global input buffer after your UI processes input but before MapScreen reads it.

**Failed approach:** `HotKeyManager.GetCategory("CampaignHotKeyCategory").SetEnabled(false)` does NOT work because `GameKeyContext` does not have a `SetEnabled` method.

The key insights are:
1. Consuming keys at the layer level does not work because MapScreen reads from the **global** input state
2. MapScreen ignores `InputUsageMask` because it considers itself a "Game State" rather than a "UI Layer"
3. The native MapScreen runs on a different priority/logic loop that cannot be beaten with standard UI bubbling

Start with Solution 1 (`Input.ClearKeys()`), verify the API exists via dnSpy, and work down the priority list if it fails.
