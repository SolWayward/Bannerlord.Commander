using System.Collections.Generic;
using Bannerlord.Commander.UI.ViewModels;
using Bannerlord.GameMaster.Information;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace Bannerlord.Commander.UI.Screens
{
    public class CommanderGauntletScreen : ScreenBase
    {
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

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _viewModel = new CommanderVM();
            _viewModel.OnCloseRequested += OnCloseRequested;

            _gauntletLayer = new GauntletLayer("GauntletLayer", 1000, false);
            _gauntletLayer.LoadMovie("CommanderScreen", _viewModel);

            AddLayer(_gauntletLayer);

            // Load required sprite categories
            LoadSprites();

            // Enable all Input (Game seems to ignore this but just in case)
            _gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            _gauntletLayer.IsFocusLayer = true;
            ScreenManager.TrySetFocus(_gauntletLayer);

            // Temporarily unbind hotkeys that interfere with text input
            UnbindMapHotKeys();

            PauseGameTime();
        }

        protected override void OnFinalize()
        {
            ResumeGameTime();

            // Restore hotkeys on screen close
            RestoreMapHotKeys();

            if (_viewModel != null)
            {
                _viewModel.OnCloseRequested -= OnCloseRequested;
                _viewModel.OnFinalize();
                _viewModel = null;
            }

            _gauntletLayer = null;

            base.OnFinalize();
        }

        /// <summary>
        /// Load required sprite categories
        /// </summary>
        void LoadSprites()
        {
            SpriteCategory inventoryCategory = UIResourceManager.SpriteData.SpriteCategories["ui_inventory"];
            inventoryCategory.Load(UIResourceManager.ResourceContext, UIResourceManager.ResourceDepot);
        }

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
            foreach (var kvp in _originalKeyboardKeys)
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

        private void OnCloseRequested() { CloseScreen(); }

        private void CloseScreen()
        {
            if (_isClosing) return;
            _isClosing = true;
            ScreenManager.PopScreen();
        }

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

        protected override void OnActivate()
        {
            base.OnActivate();
            if (_gauntletLayer != null)
            {
                _gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
                _gauntletLayer.IsFocusLayer = true;
                ScreenManager.TrySetFocus(_gauntletLayer);
            }

            // Re-apply unbinds.
            // Since UnbindMapHotKeys checks ContainsKey, it won't accidentally save "Invalid".
            // This ensures keys stay unbound even if the game reloads them on focus change.
            UnbindMapHotKeys();

            _viewModel?.RefreshCurrentMode();
            PauseGameTime();
        }
    }
}