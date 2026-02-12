using System.Collections.Generic;
using Bannerlord.Commander.UI.States;
using Bannerlord.Commander.UI.ViewModels;
using Bannerlord.GameMaster.Information;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace Bannerlord.Commander.UI.Screens
{
    /// <summary>
    /// Gauntlet screen bound to <see cref="CommanderState"/> via the GameStateScreen attribute.
    /// Follows the native pattern used by GauntletClanScreen, GauntletQuestsScreen, etc.
    /// The engine creates this screen automatically when CommanderState is pushed.
    /// </summary>
    [GameStateScreen(typeof(CommanderState))]
    public class CommanderGauntletScreen : ScreenBase, IGameStateListener
    {
        private readonly CommanderState _commanderState;
        private GauntletLayer _gauntletLayer;
        private CommanderVM _viewModel;
        private bool _isClosing;
        private CampaignTimeControlMode _previousTimeControlMode;
        private bool _wasTimePaused;

        // Stores original hotkey bindings to restore on close
        private Dictionary<GameKey, InputKey> _originalKeyboardKeys = new();

        // List of categories that interfere with typing or overlay
        private readonly string[] _blockedHotKeyCategories = new[]
        {
            "GenericCampaignPanelsGameKeyCategory", // I, C, P, L, K, etc.
            "Generic",                              // WASD, tab, alt
            "MapHotKeyCategory",                    // Space, 1, 2, 3, Arrows, WASD
        };

        public CommanderGauntletScreen(CommanderState commanderState)
        {
            _commanderState = commanderState;
        }

        #region ScreenBase Overrides

        protected override void OnInitialize()
        {
            base.OnInitialize();
        }

        protected override void OnFinalize()
        {
            base.OnFinalize();
        }

        protected override void OnFrameTick(float dt)
        {
            base.OnFrameTick(dt);
            if (_isClosing) return;

            _viewModel?.OnTick();

            // Handle Escape manually
            // We use Input.IsKeyPressed (Global) because layer input might be restricted
            if (_gauntletLayer != null && Input.IsKeyPressed(InputKey.Escape))
            {
                CloseScreen();
            }
        }

        /// <summary>
        /// Called when the screen is re-activated (e.g. returning from a child state).
        /// Data refresh is handled by IGameStateListener.OnActivate() — no duplicate refresh here.
        /// </summary>
        protected override void OnActivate()
        {
            base.OnActivate();
        }

        #endregion

        #region IGameStateListener Implementation

        void IGameStateListener.OnInitialize()
        {
            // Empty body - matches native pattern (GauntletClanScreen, GauntletQuestsScreen)
        }

        void IGameStateListener.OnActivate()
        {
            base.OnActivate();

            // MARK: VM is kept alive across activations to preserve UI state (selected hero, mode, etc.)
            if (_viewModel == null)
            {
                _viewModel = new CommanderVM();
                _viewModel.OnCloseRequested += OnCloseRequested;
            }

            // Always reload sprite categories — native screens (e.g. GauntletInventoryScreen)
            // may unload shared categories like "ui_inventory" in their OnFinalize().
            // SpriteCategory.Load() is safe to call when already loaded (no-op if textures are in GPU memory).
            LoadSprites();

            // MARK: Always create a fresh GauntletLayer — native pattern (GauntletClanScreen).
            // A removed layer's UIContext becomes stale; reusing it causes NullRef in GauntletChatLogView.
            _gauntletLayer = new GauntletLayer("GauntletLayer", 1000, false);
            _gauntletLayer.LoadMovie("CommanderScreen", _viewModel);

            AddLayer(_gauntletLayer);

            _gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            _gauntletLayer.IsFocusLayer = true;
            ScreenManager.TrySetFocus(_gauntletLayer);

            UnbindMapHotKeys();
            PauseGameTime();
            _viewModel?.RefreshCurrentMode();
        }

        void IGameStateListener.OnDeactivate()
        {
            base.OnDeactivate();

            RestoreMapHotKeys();

            // Remove the layer — the old instance is orphaned for GC (native pattern).
            // A fresh layer is created on next OnActivate().
            if (_gauntletLayer != null)
            {
                _gauntletLayer.IsFocusLayer = false;
                ScreenManager.TryLoseFocus(_gauntletLayer);
                RemoveLayer(_gauntletLayer);
            }
        }

        void IGameStateListener.OnFinalize()
        {
            if (_viewModel != null)
            {
                _viewModel.OnCloseRequested -= OnCloseRequested;
                _viewModel.OnFinalize();
                _viewModel = null;
            }

            _gauntletLayer = null;
        }

        #endregion

        #region Screen Close

        private void OnCloseRequested() { CloseScreen(); }

        private void CloseScreen()
        {
            if (_isClosing) return;
            _isClosing = true;
            ResumeGameTime();
            Game.Current.GameStateManager.PopState(0);
        }

        #endregion

        #region Sprite Loading

        /// <summary>
        /// Load required sprite categories
        /// </summary>
        private void LoadSprites()
        {
            SpriteCategory inventoryCategory = UIResourceManager.SpriteData.SpriteCategories["ui_inventory"];
            inventoryCategory.Load(UIResourceManager.ResourceContext, UIResourceManager.ResourceDepot);
        }

        #endregion

        #region Hotkey Management

        /// <summary>
        /// Prevent hotkeys from interfering with text input by unbinding them temporarily.
        /// Safe to call multiple times (checks dictionary before saving).
        /// </summary>
        private void UnbindMapHotKeys()
        {
            foreach (string categoryId in _blockedHotKeyCategories)
            {
                try
                {
                    GameKeyContext category = HotKeyManager.GetCategory(categoryId);

                    if (category == null)
                        continue;

                    foreach (GameKey gameKey in category.RegisteredGameKeys)
                    {
                        if (gameKey == null || gameKey.KeyboardKey == null) continue;

                        // Save original hotkey binding if not already saved.
                        if (!_originalKeyboardKeys.ContainsKey(gameKey))
                        {
                            // Important: Don't save if it's already Invalid
                            if (gameKey.KeyboardKey.InputKey != InputKey.Invalid)
                            {
                                _originalKeyboardKeys[gameKey] = gameKey.KeyboardKey.InputKey;
                            }
                        }

                        // Temporarily unbind hotkey if it isn't already unbound
                        if (gameKey.KeyboardKey.InputKey != InputKey.Invalid)
                        {
                            gameKey.KeyboardKey.ChangeKey(InputKey.Invalid);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    // Log error but continue
                    InfoMessage.Warning($"[Commander] Error unbinding hotkeys for {categoryId}: {ex.Message}\nCurrent Game Version likely changed hotkey categories.\nText Input may cause menus to open, hold shift as typing as a work around");
                }
            }
        }

        /// <summary>
        /// Restores previously unbound hotkeys to their original bindings using direct object references.
        /// </summary>
        private void RestoreMapHotKeys()
        {
            foreach (KeyValuePair<GameKey, InputKey> kvp in _originalKeyboardKeys)
            {
                GameKey gameKey = kvp.Key;
                InputKey originalInput = kvp.Value;

                try
                {
                    // We simply use the reference we stored. No lookup needed.
                    if (gameKey != null && gameKey.KeyboardKey != null)
                    {
                        gameKey.KeyboardKey.ChangeKey(originalInput);
                    }
                }
                catch
                {
                    // Ignore errors, errors will already be logged in UnbindMapHotKeys
                }
            }

            _originalKeyboardKeys.Clear();
        }

        #endregion

        #region Game Time Control

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

        private void ResumeGameTime()
        {
            if (Campaign.Current != null && _wasTimePaused)
            {
                Campaign.Current.SetTimeControlModeLock(false);
                Campaign.Current.TimeControlMode = _previousTimeControlMode;
                _wasTimePaused = false;
            }
        }

        #endregion
    }
}
