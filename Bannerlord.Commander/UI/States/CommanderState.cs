using TaleWorlds.Core;

namespace Bannerlord.Commander.UI.States
{
    /// <summary>
    /// GameState for the Commander screen overlay.
    /// Follows the native pattern used by ClanState, KingdomState, QuestsState, etc.
    /// Pushing this state makes it the ActiveState, which stops MapState.OnMapModeTick
    /// from running SandBoxViewVisualManager.OnTick (prevents NavalDLC crash).
    /// </summary>
    public class CommanderState : GameState
    {
        public override bool IsMenuState => true;

        public CommanderState() { }
    }
}
