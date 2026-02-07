using Bannerlord.Commander.UI.States;
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
    /// Handles mod initialization and Shift+C hotkey to open the Commander screen.
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
        /// Called every frame - handles Shift+C key press to open the Commander screen
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

            if ((Input.IsKeyDown(InputKey.LeftShift) || Input.IsKeyDown(InputKey.RightShift))
                && Input.IsKeyPressed(InputKey.C))
            {
                OpenCommanderScreen();
            }
        }

        private bool IsGameActive()
        {
            return Game.Current?.GameStateManager != null;
        }

        private void CheckIfScreenClosed()
        {
            if (!(Game.Current.GameStateManager.ActiveState is CommanderState))
            {
                _isScreenOpen = false;
            }
        }

        private void OpenCommanderScreen()
        {
            Game.Current.GameStateManager.PushState(
                Game.Current.GameStateManager.CreateState<CommanderState>());
            _isScreenOpen = true;
        }
    }
}
