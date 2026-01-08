using Bannerlord.Commander.UI.Screens;
using TaleWorlds.Core;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.GauntletUI.PrefabSystem;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ScreenSystem;

namespace Bannerlord.Commander
{
    /// <summary>
    /// Main entry point for the Bannerlord.Commander mod.
    /// Handles mod initialization and F10 hotkey to open the Commander screen.
    /// </summary>
    public class SubModule : MBSubModuleBase
    {
        private bool _isScreenOpen;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            InformationManager.DisplayMessage(
                new InformationMessage("Bannerlord.Commander loaded! Press F10 to open Commander screen.")
            );
        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();
        }

        /// <summary>
        /// Called every frame - handles F10 key press to open the Commander screen
        /// </summary>
        protected override void OnApplicationTick(float dt)
        {
            base.OnApplicationTick(dt);

            if (!IsGameActive())
                return;

            if (_isScreenOpen)
            {
                CheckIfScreenClosed();
                return;
            }

            if (Input.IsKeyPressed(InputKey.F10))
            {
                OpenCommanderScreen();
            }
        }

        private bool IsGameActive()
        {
            return Game.Current != null;
        }

        private void CheckIfScreenClosed()
        {
            if (!(ScreenManager.TopScreen is CommanderGauntletScreen))
            {
                _isScreenOpen = false;
            }
        }

        private void OpenCommanderScreen()
        {
            var screen = new CommanderGauntletScreen();
            ScreenManager.PushScreen(screen);
            _isScreenOpen = true;
        }
    }
}
