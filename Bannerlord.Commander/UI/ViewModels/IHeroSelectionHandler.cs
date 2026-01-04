namespace Bannerlord.Commander.UI.ViewModels
{
    /// <summary>
    /// Interface for handling hero selection in the UI.
    /// Implemented by ViewModels that manage hero lists.
    /// </summary>
    public interface IHeroSelectionHandler
    {
        /// <summary>
        /// Selects the specified hero and deselects others
        /// </summary>
        /// <param name="hero">The hero to select</param>
        void SelectHero(CommanderHeroVM hero);
    }
}
