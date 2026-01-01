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
    /// 
    /// Hotkey blocking is handled automatically by the EditableTextWidget in the XML.
    /// When the text input has focus, the engine blocks game hotkeys from triggering.
    /// This is the same pattern used by the native Inventory search bar.
    /// </summary>
    public class CommanderGauntletScreen : ScreenBase
    {
        private GauntletLayer _gauntletLayer;
        private CommanderVM _viewModel;
        private bool _isClosing;
        private CampaignTimeControlMode _previousTimeControlMode;
        private bool _wasTimePaused;

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _viewModel = new CommanderVM();
            _viewModel.OnCloseRequested += OnCloseRequested;

            // Create Gauntlet layer with standard priority
            _gauntletLayer = new GauntletLayer("GauntletLayer", 1000, false);
            _gauntletLayer.LoadMovie("CommanderScreen", _viewModel);

            AddLayer(_gauntletLayer);

            // Set input restrictions to capture keyboard/mouse input
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

            // Handle frame tick for ViewModel (deferred operations, incremental loading)
            _viewModel?.OnTick();

            // Handle escape to close the screen
            if (_gauntletLayer != null && _gauntletLayer.Input.IsKeyPressed(InputKey.Escape))
            {
                CloseScreen();
            }
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
