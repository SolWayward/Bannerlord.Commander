using Bannerlord.Commander.UI;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ScreenSystem;

namespace Bannerlord.Commander
{
    public class SubModule : MBSubModuleBase
    {
        private bool _isScreenOpen;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            
            InformationManager.DisplayMessage(
                new InformationMessage("Bannerlord.Commander loaded! Press F10 to open test screen.")
            );
        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();
        }

        /// <summary>
        /// Called every frame - handles F10 key press to open the test screen
        /// </summary>
        protected override void OnApplicationTick(float dt)
        {
            base.OnApplicationTick(dt);

            // Only handle input if a game is active
            if (Game.Current == null)
                return;

            // Don't open another screen if one is already open
            if (_isScreenOpen)
            {
                // Check if screen was closed
                if (!(ScreenManager.TopScreen is CommanderTestGauntletScreen))
                {
                    _isScreenOpen = false;
                }
                return;
            }

            // F10 opens the test screen
            if (Input.IsKeyPressed(InputKey.F10))
            {
                OpenTestScreen();
            }
        }

        private void OpenTestScreen()
        {
            var screen = new CommanderTestGauntletScreen();
            ScreenManager.PushScreen(screen);
            _isScreenOpen = true;
        }
    }
}
