using TaleWorlds.CampaignSystem;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.ScreenSystem;

namespace Bannerlord.Commander.UI
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

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _viewModel = new CommanderVM();
            _viewModel.OnCloseRequested += OnCloseRequested;
            
            // Create the layer (categoryId, localOrder, shouldClear)
            _gauntletLayer = new GauntletLayer("GauntletLayer", 100, false);
            
            // Load the movie - must be in GUI/Prefabs folder
            _gauntletLayer.LoadMovie("CommanderScreen", _viewModel);
            
            // Add and focus the layer
            AddLayer(_gauntletLayer);
            _gauntletLayer.InputRestrictions.SetInputRestrictions();
            _gauntletLayer.IsFocusLayer = true;
            ScreenManager.TrySetFocus(_gauntletLayer);
            
            // Pause game time when opening the Commander menu
            PauseGameTime();
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            
            if (_gauntletLayer != null)
            {
                ScreenManager.TrySetFocus(_gauntletLayer);
            }
            
            // Refresh hero list when menu is reopened to avoid stale state
            _viewModel?.RefreshCurrentMode();
            
            // Ensure game time is paused when reactivating
            PauseGameTime();
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
        }

        protected override void OnFinalize()
        {
            // Resume game time when closing the Commander menu
            ResumeGameTime();
            
            if (_viewModel != null)
            {
                _viewModel.OnCloseRequested -= OnCloseRequested;
                _viewModel.OnFinalize();
                _viewModel = null;
            }
            
            // DO NOT manually release movie or remove layer here
            // The base class ScreenBase.OnFinalize() handles layer finalization
            _gauntletLayer = null;
            
            base.OnFinalize();
        }

        protected override void OnFrameTick(float dt)
        {
            base.OnFrameTick(dt);
            
            if (_isClosing)
                return;
            
            // Allow ViewModel to handle deferred operations
            _viewModel?.OnTick();
            
            // Handle ESC to close
            if (Input.IsKeyPressed(InputKey.Escape) ||
                (_gauntletLayer != null && _gauntletLayer.Input.IsKeyPressed(InputKey.Escape)))
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
