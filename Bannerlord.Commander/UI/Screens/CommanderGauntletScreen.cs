using System.Collections.Generic;
using Bannerlord.Commander.UI.States;
using Bannerlord.Commander.UI.ViewModels;
using Bannerlord.Commander.UI.ViewModels.HeroCreator;
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

        // Hero Creator overlay layer
        private GauntletLayer _heroCreatorLayer;
        private HeroCreatorVM _heroCreatorVM;

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
            if (Input.IsKeyPressed(InputKey.Escape))
            {
                // If Hero Creator is open, close it (consume Escape)
                if (_heroCreatorLayer != null)
                {
                    CloseHeroCreator();
                }

                else
                {
                    CloseScreen();
                }
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
                _viewModel.OnCreateHeroRequested += OpenHeroCreator;
            }

            // Always reload sprite categories — native screens (e.g. GauntletInventoryScreen)
            // may unload shared categories like "ui_inventory" in their OnFinalize().
            // SpriteCategory.Load() is safe to call when already loaded (no-op if textures are in GPU memory).
            LoadSprites();

            // MARK: Layer is kept alive across activations to avoid re-running LoadMovie()
            // on a fully-populated MBBindingList (~2000 items), which causes a ~2s freeze
            // as Gauntlet must instantiate all row widgets synchronously.
            // The layer is only created once; on reactivation we just restore focus.
            if (_gauntletLayer == null)
            {
                _gauntletLayer = new GauntletLayer("GauntletLayer", 1000, false);
                _gauntletLayer.LoadMovie("CommanderScreen", _viewModel);
                AddLayer(_gauntletLayer);
            }

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

            // MARK: Layer is kept alive — only lose focus so the editor state on top
            // can receive input. The widget tree and all bound VMs stay intact,
            // preserving scroll position, selection state, and avoiding the ~2s
            // LoadMovie() freeze on reactivation.
            if (_gauntletLayer != null)
            {
                _gauntletLayer.InputRestrictions.ResetInputRestrictions();
                _gauntletLayer.IsFocusLayer = false;
                ScreenManager.TryLoseFocus(_gauntletLayer);
            }
        }

        void IGameStateListener.OnFinalize()
        {
            // Clean up hero creator if still open
            if (_heroCreatorLayer != null)
            {
                CloseHeroCreator();
            }

            // Layer cleanup happens here — the only place where the layer is removed and destroyed.
            if (_gauntletLayer != null)
            {
                _gauntletLayer.InputRestrictions.ResetInputRestrictions();
                _gauntletLayer.IsFocusLayer = false;
                ScreenManager.TryLoseFocus(_gauntletLayer);
                RemoveLayer(_gauntletLayer);
                _gauntletLayer = null;
            }

            if (_viewModel != null)
            {
                _viewModel.OnCloseRequested -= OnCloseRequested;
                _viewModel.OnCreateHeroRequested -= OpenHeroCreator;
                _viewModel.OnFinalize();
                _viewModel = null;
            }
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

        #region Hero Creator Overlay

        /// <summary>
        /// Opens the Hero Creator as a second GauntletLayer overlaid on top of Commander.
        /// Commander remains visible but loses focus/input.
        /// </summary>
        private void OpenHeroCreator()
        {
            if (_heroCreatorLayer != null) return; // Already open

            _heroCreatorVM = new HeroCreatorVM();
            _heroCreatorVM.OnCloseRequested += CloseHeroCreator;
            _heroCreatorVM.OnHeroCreated += OnHeroCreated;

            _heroCreatorLayer = new GauntletLayer("HeroCreatorLayer", 1001, false);
            _heroCreatorLayer.LoadMovie("HeroCreatorScreen", _heroCreatorVM);
            AddLayer(_heroCreatorLayer);

            _heroCreatorLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            _heroCreatorLayer.IsFocusLayer = true;
            ScreenManager.TrySetFocus(_heroCreatorLayer);
        }

        /// <summary>
        /// Closes the Hero Creator overlay and restores focus to Commander.
        /// </summary>
        private void CloseHeroCreator()
        {
            if (_heroCreatorLayer == null) return;

            _heroCreatorLayer.InputRestrictions.ResetInputRestrictions();
            _heroCreatorLayer.IsFocusLayer = false;
            ScreenManager.TryLoseFocus(_heroCreatorLayer);
            RemoveLayer(_heroCreatorLayer);
            _heroCreatorLayer = null;

            // Restore focus to Commander layer
            _gauntletLayer.IsFocusLayer = true;
            ScreenManager.TrySetFocus(_gauntletLayer);

            _heroCreatorVM.OnCloseRequested -= CloseHeroCreator;
            _heroCreatorVM.OnHeroCreated -= OnHeroCreated;
            _heroCreatorVM.OnFinalize();
            _heroCreatorVM = null;
        }

        /// <summary>
        /// Called when a hero is successfully created. Refreshes the hero list and auto-selects the new hero.
        /// </summary>
        private void OnHeroCreated(Hero hero)
        {
            // Refresh the hero list to include the new hero
            _viewModel?.RefreshCurrentMode();
            // TODO: Auto-select the new hero in the list after refresh completes
            // This may require passing the hero's StringId to HeroListVM for deferred selection
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
