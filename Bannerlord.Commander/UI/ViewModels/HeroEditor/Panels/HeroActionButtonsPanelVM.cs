using System;
using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Items.Inventory;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Library;

namespace Bannerlord.Commander.UI.ViewModels.HeroEditor.Panels
{
    /// <summary>
    /// ViewModel for the hero action buttons panel.
    /// Provides commands for Edit Appearance and Open Inventory actions.
    /// </summary>
    public class HeroActionButtonsPanelVM : ViewModel
    {
        private Hero _hero;
        private Action _onEditorClosed;

        public HeroActionButtonsPanelVM() { }

        public void SetOnEditorClosed(Action onEditorClosed)
        {
            _onEditorClosed = onEditorClosed;
        }

        public void RefreshForHero(Hero hero)
        {
            _hero = hero;
        }

        public void Clear()
        {
            _hero = null;
        }

        /// <summary>
        /// Opens the BLGM full face generator editor for the selected hero.
        /// </summary>
        public void ExecuteEditAppearance()
        {
            if (_hero == null)
            {
                return;
            }

            Bannerlord.GameMaster.Heroes.HeroEditor editor = new(_hero);
            BLGMResult result = editor.HeroAppearanceEditorUI.OpenFullEditor(onComplete: _onEditorClosed);
        }

        /// <summary>
        /// Opens the native inventory UI for the selected hero.
        /// If the hero is the player or in the player's party, opens single-hero discard mode.
        /// Otherwise, opens a two-sided view with the player on the right and selected hero on the left.
        /// </summary>
        public void ExecuteOpenInventory()
        {
            if (_hero == null)
            {
                return;
            }

            bool isPlayerOrInPlayerParty = _hero == Hero.MainHero
                || _hero.PartyBelongedTo == MobileParty.MainParty;

            if (isPlayerOrInPlayerParty)
            {
                InventoryManager inventoryManager = new(_hero);
                BLGMResult result = inventoryManager.OpenInventory(onComplete: _onEditorClosed);
            }

            else
            {
                InventoryManager inventoryManager = new(
                    rightHero: Hero.MainHero,
                    leftHero: _hero,
                    middleHero: _hero);
                BLGMResult result = inventoryManager.OpenInventory(onComplete: _onEditorClosed);
            }
        }
    }
}
