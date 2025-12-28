using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.ScreenSystem;

namespace Bannerlord.Commander.UI
{
    /// <summary>
    /// A dedicated GauntletScreen for the test UI.
    /// Uses the standard Bannerlord screen pattern with GauntletLayer.
    /// </summary>
    public class CommanderTestGauntletScreen : ScreenBase
    {
        private GauntletLayer _gauntletLayer;
        private CommanderTestVM _viewModel;
        private bool _isClosing;

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _viewModel = new CommanderTestVM();
            _viewModel.OnCloseRequested += OnCloseRequested;
            
            // Create the layer (categoryId, localOrder, shouldClear)
            _gauntletLayer = new GauntletLayer("GauntletLayer", 100, false);
            
            // Load the movie - must be in GUI/Prefabs folder
            _gauntletLayer.LoadMovie("CommanderTestScreen", _viewModel);
            
            // Add and focus the layer
            AddLayer(_gauntletLayer);
            _gauntletLayer.InputRestrictions.SetInputRestrictions();
            _gauntletLayer.IsFocusLayer = true;
            ScreenManager.TrySetFocus(_gauntletLayer);
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            
            if (_gauntletLayer != null)
            {
                ScreenManager.TrySetFocus(_gauntletLayer);
            }
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
        }

        protected override void OnFinalize()
        {
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
    }
}
