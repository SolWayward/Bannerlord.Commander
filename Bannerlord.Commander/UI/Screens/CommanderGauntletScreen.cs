using Bannerlord.Commander.UI.ViewModels;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.ScreenSystem;

namespace Bannerlord.Commander.UI.Screens
{
    /// <summary>
    /// The main Commander screen - a full-screen Gauntlet UI for Commander functionality.
    /// Uses the standard Bannerlord screen pattern with GauntletLayer.
    /// </summary>
    public class CommanderGauntletScreen : ScreenBase
    {
        private GauntletLayer _gauntletLayer;
        private CommanderVM _viewModel;
        private bool _isClosing;
        private CampaignTimeControlMode _previousTimeControlMode;
        private bool _wasTimePaused;

        /// <summary>
        /// Common game hotkeys that need to be consumed to prevent them from
        /// triggering native screens while our Commander screen is open.
        /// </summary>
        private static readonly InputKey[] HotkeyKeysToConsume = new InputKey[]
        {
            InputKey.C,  // Character screen
            InputKey.I,  // Inventory
            InputKey.N,  // Encyclopedia/Notes
            InputKey.K,  // Kingdom
            InputKey.L,  // Clan
            InputKey.P,  // Party
            InputKey.Q,  // Quest log
            InputKey.B,  // Kingdom decisions/barter
            InputKey.T,  // Trade
            InputKey.E,  // Encyclopedia
            InputKey.O,  // Clan screen alt
            InputKey.H,  // Help
            InputKey.J,  // Journal
            InputKey.U,  // Units
            InputKey.Y,  // Yes/confirm in dialogs
            InputKey.F1, InputKey.F2, InputKey.F3, InputKey.F4,
            InputKey.F5, InputKey.F6, InputKey.F7, InputKey.F8,
            InputKey.F9, InputKey.F10, InputKey.F11, InputKey.F12,
        };

        /// <summary>
        /// Native screen type names that should be closed if they appear on top of our screen.
        /// This is the "Screen Guard" failsafe for when input blocking doesn't work.
        /// </summary>
        private static readonly string[] UnwantedScreenTypeNames = new string[]
        {
            "CharacterDeveloperScreen",  // Character (C)
            "InventoryScreen",           // Inventory (I)
            "ClanScreen",                // Clan (L)
            "GauntletQuestScreen",       // Journal (J)
            "KingdomManagementScreen",   // Kingdom (K)
            "PartyScreen",               // Party (P)
            "GauntletEncyclopediaScreen" // Encyclopedia (N/E)
        };

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _viewModel = new CommanderVM();
            _viewModel.OnCloseRequested += OnCloseRequested;

            // Create layer with very high local order to take priority over all other layers
            // Using a unique category ID to avoid conflicts
            _gauntletLayer = new GauntletLayer("CommanderScreenLayer", 10000, false);
            _gauntletLayer.LoadMovie("CommanderScreen", _viewModel);

            AddLayer(_gauntletLayer);

            // Set input restrictions to capture ALL keyboard/mouse input.
            // Note: This alone may not stop MapScreen from stealing hotkeys due to execution order.
            // The Screen Guard in OnFrameTick handles any screens that slip through.
            _gauntletLayer.InputRestrictions.SetInputRestrictions();
            _gauntletLayer.IsFocusLayer = true;
            ScreenManager.TrySetFocus(_gauntletLayer);

            PauseGameTime();
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            if (_gauntletLayer != null)
            {
                // Re-assert focus and input restrictions when screen activates
                _gauntletLayer.InputRestrictions.SetInputRestrictions();
                _gauntletLayer.IsFocusLayer = true;
                ScreenManager.TrySetFocus(_gauntletLayer);
            }

            _viewModel?.RefreshCurrentMode();
            PauseGameTime();
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
        }

        protected override void OnFinalize()
        {
            ResumeGameTime();

            if (_viewModel != null)
            {
                _viewModel.OnCloseRequested -= OnCloseRequested;
                _viewModel.OnFinalize();
                _viewModel = null;
            }

            _gauntletLayer = null;

            base.OnFinalize();
        }

        protected override void OnFrameTick(float dt)
        {
            base.OnFrameTick(dt);

            if (_isClosing)
                return;

            // STEP 1: Capture escape state FIRST before any key clearing
            bool escapePressed = _gauntletLayer != null && _gauntletLayer.Input.IsKeyPressed(InputKey.Escape);

            // STEP 2: Let your VM/UI handle the input (e.g., typing 'c' into the textbox)
            _viewModel?.OnTick();

            // STEP 3: THE SCREEN GUARD (The "No-Harmony" Safety Net)
            // If the MapScreen stole input and opened a menu on top of us, kill it instantly.
            // This runs BEFORE we clear keys so we can react to screens that slipped through.
            CloseUnwantedScreens();

            // STEP 4: Wipe the global input buffer to prevent MapScreen from seeing keys.
            // The MapScreen reads from the GLOBAL input state (via HotKeyManager), not the layer's local context.
            // By clearing all global input after our UI processes it, MapScreen sees nothing.
            ClearGlobalInputForHotkeys();

            // STEP 5: Handle escape after everything else
            if (escapePressed)
            {
                CloseScreen();
            }
        }

        /// <summary>
        /// Screen Guard: Checks if any unwanted native screens opened on top of our Commander screen.
        /// If they did (due to execution order issues where MapScreen runs before our input blocking),
        /// this immediately closes them and re-asserts our screen's dominance.
        /// </summary>
        private void CloseUnwantedScreens()
        {
            // Only act if we're no longer the top screen
            if (ScreenManager.TopScreen == this)
                return;

            string topScreenType = ScreenManager.TopScreen?.GetType().Name;
            if (string.IsNullOrEmpty(topScreenType))
                return;

            // Check if the top screen is one of the native menus we want to block
            bool isUnwanted = false;
            foreach (var unwantedType in UnwantedScreenTypeNames)
            {
                if (topScreenType == unwantedType)
                {
                    isUnwanted = true;
                    break;
                }
            }

            if (isUnwanted)
            {
                // Force close the unwanted screen immediately
                ScreenManager.PopScreen();

                // Re-assert our dominance over input
                if (_gauntletLayer != null)
                {
                    _gauntletLayer.InputRestrictions.SetInputRestrictions();
                    _gauntletLayer.IsFocusLayer = true;
                    ScreenManager.TrySetFocus(_gauntletLayer);
                }
            }
        }

        /// <summary>
        /// Aggressively clears the global input buffer to prevent native screens from opening.
        /// The native MapScreen checks for inputs using the global GameKey system (HotKeyManager),
        /// which runs in parallel to our Gauntlet UI layer. Even if our UI consumes the event,
        /// the global system still sees the physical key press.
        ///
        /// AGGRESSIVE FIX: Clear ALL keys on EVERY frame unconditionally.
        /// Our OnFrameTick runs before MapScreen's logic due to screen ordering.
        /// By clearing the global input buffer every frame, MapScreen sees nothing.
        /// This is more aggressive than checking for specific keys, but ensures
        /// no hotkey can slip through regardless of timing.
        /// </summary>
        private void ClearGlobalInputForHotkeys()
        {
            // Clear all global input unconditionally every frame
            TaleWorlds.InputSystem.Input.ClearKeys();
        }

        private void OnCloseRequested()
        {
            CloseScreen();
        }

        private void CloseScreen()
        {
            if (_isClosing)
                return;

            _isClosing = true;
            ScreenManager.PopScreen();
        }

        /// <summary>
        /// Pauses game time when the Commander menu is opened.
        /// This prevents game state changes while editing objects.
        /// </summary>
        private void PauseGameTime()
        {
            if (Campaign.Current != null && !_wasTimePaused)
            {
                _previousTimeControlMode = Campaign.Current.TimeControlMode;
                _wasTimePaused = true;
                Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;
                Campaign.Current.SetTimeControlModeLock(true);
            }
        }

        /// <summary>
        /// Resumes game time when the Commander menu is closed.
        /// Restores the previous time control mode.
        /// </summary>
        private void ResumeGameTime()
        {
            if (Campaign.Current != null && _wasTimePaused)
            {
                Campaign.Current.SetTimeControlModeLock(false);
                Campaign.Current.TimeControlMode = _previousTimeControlMode;
                _wasTimePaused = false;
            }
        }
    }
}
