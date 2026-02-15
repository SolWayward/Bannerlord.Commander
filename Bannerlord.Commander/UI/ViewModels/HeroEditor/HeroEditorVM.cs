using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using Bannerlord.Commander.UI.ViewModels.HeroEditor.Panels;
using Bannerlord.Commander.UI.ViewModels.HeroMode;


namespace Bannerlord.Commander.UI.ViewModels.HeroEditor
{
    /// <summary>
    /// Main ViewModel for the Hero Editor Panel.
    /// Coordinates all sub-ViewModels and responds to hero selection changes.
    /// Equipment slots use custom EquipmentSlotVM for standalone equipment display without InventoryLogic dependency.
    /// </summary>
    public class HeroEditorVM : ViewModel, IHeroSelectionHandler
    {
        #region Private Fields

        private Hero _hero;
        private HeroListItemVM _selectedListItem;

        // New Panel VMs - 1:1 match with XML panels
        private HeroPortraitPanelVM _heroPortraitPanel;
        private HeroNamePanelVM _heroNamePanel;
        private HeroActionButtonsPanelVM _heroActionButtonsPanel;
        private ClanBannerPanelVM _clanBannerPanel;
        private HeroIdentityInfoPanelVM _heroIdentityInfoPanel;
        private HeroClanPanelVM _heroClanPanel;
        private HeroKingdomPanelVM _heroKingdomPanel;

        // Existing VMs that remain unchanged
        private HeroPartyVM _heroParty;
        private HeroSkillsVM _heroSkills;
        private HeroInventoryVM _heroInventory;
        private HeroCharacterVM _heroCharacter;

        private bool _isVisible;
        private string _selectedHeroStringId;

        #endregion

        #region Constructor

        public HeroEditorVM()
        {
            // Initialize new Panel ViewModels
            HeroPortraitPanel = new();
            HeroNamePanel = new();
            HeroNamePanel.SetOnNameChanged(OnHeroNameChanged);
            HeroActionButtonsPanel = new();
            HeroActionButtonsPanel.SetOnEditorClosed(OnExternalEditorClosed);
            ClanBannerPanel = new();
            ClanBannerPanel.SetOnEditorClosed(OnExternalEditorClosed);
            HeroIdentityInfoPanel = new();
            HeroClanPanel = new();
            HeroKingdomPanel = new();

            // Initialize existing ViewModels
            HeroParty = new();
            HeroSkills = new();
            HeroInventory = new();
            HeroCharacter = new();

            IsVisible = false;
            SelectedHeroStringId = "";

            // Subscribe to HeroInventory property changes to forward equipment slot notifications
            HeroInventory.PropertyChanged += OnInventoryPropertyChanged;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes all sub-ViewModels with data from the specified hero.
        /// </summary>
        /// <param name="hero">The hero to display in the editor</param>
        public void RefreshForHero(Hero hero)
        {
            _hero = hero;

            if (_hero != null)
            {
                SelectedHeroStringId = _hero.StringId ?? "";

                // Refresh new Panel ViewModels
                HeroPortraitPanel?.RefreshForHero(_hero);
                HeroNamePanel?.RefreshForHero(_hero);
                HeroActionButtonsPanel?.RefreshForHero(_hero);
                ClanBannerPanel?.RefreshForHero(_hero);
                HeroIdentityInfoPanel?.RefreshForHero(_hero);
                HeroClanPanel?.RefreshForHero(_hero);
                HeroKingdomPanel?.RefreshForHero(_hero);

                // Refresh existing ViewModels
                HeroParty?.RefreshForHero(_hero);
                HeroSkills?.RefreshForHero(_hero);
                HeroInventory?.RefreshForHero(_hero);
                HeroCharacter?.RefreshForHero(_hero);

                IsVisible = true;
            }
            else
            {
                Clear();
            }
        }

        /// <summary>
        /// Clears all data and hides the editor panel.
        /// </summary>
        public void Clear()
        {
            _hero = null;
            IsVisible = false;
            SelectedHeroStringId = "";

            // Clear new Panel ViewModels
            HeroPortraitPanel?.Clear();
            HeroNamePanel?.Clear();
            HeroActionButtonsPanel?.Clear();
            ClanBannerPanel?.Clear();
            HeroIdentityInfoPanel?.Clear();
            HeroClanPanel?.Clear();
            HeroKingdomPanel?.Clear();

            // Clear existing ViewModels
            HeroParty?.Clear();
            HeroSkills?.Clear();
            HeroInventory?.Clear();
            HeroCharacter?.Clear();
        }

        /// <summary>
        /// Implementation of IHeroSelectionHandler - called when a hero is selected.
        /// </summary>
        public void SelectHero(HeroListItemVM hero)
        {
            _selectedListItem = hero;
            if (hero != null)
            {
                RefreshForHero(hero.Hero);
            }
            else
            {
                Clear();
            }
        }

        public override void OnFinalize()
        {
            base.OnFinalize();

            // Unsubscribe from HeroInventory property changes
            if (HeroInventory != null)
            {
                HeroInventory.PropertyChanged -= OnInventoryPropertyChanged;
            }

            // Finalize new Panel ViewModels
            HeroPortraitPanel?.OnFinalize();
            HeroNamePanel?.OnFinalize();
            HeroActionButtonsPanel?.OnFinalize();
            ClanBannerPanel?.OnFinalize();
            HeroIdentityInfoPanel?.OnFinalize();
            HeroClanPanel?.OnFinalize();
            HeroKingdomPanel?.OnFinalize();

            // Finalize existing ViewModels
            HeroParty?.OnFinalize();
            HeroSkills?.OnFinalize();
            HeroInventory?.OnFinalize();
            HeroCharacter?.OnFinalize();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Callback invoked when the hero name changes in the name panel.
        /// Updates the hero list item so the left panel reflects the new name.
        /// </summary>
        private void OnHeroNameChanged(string newName)
        {
            if (_selectedListItem != null)
            {
                _selectedListItem.Name = newName;
            }
        }

        /// <summary>
        /// Callback invoked when an external BLGM editor (appearance, inventory, banner) closes.
        /// Refreshes all panels to reflect any changes made in those editors.
        /// </summary>
        private void OnExternalEditorClosed()
        {
            if (_hero != null)
            {
                RefreshForHero(_hero);
            }
        }

        #endregion

        #region Private Event Handlers

        /// <summary>
        /// Forwards property change notifications from HeroInventory equipment slots to this ViewModel.
        /// This allows direct binding to equipment slots in Gauntlet XML without nested path navigation.
        /// </summary>
        private void OnInventoryPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Forward equipment slot property changes
            switch (e.PropertyName)
            {
                case nameof(HeroInventoryVM.HeadSlot):
                    OnPropertyChangedWithValue(HeroInventory.HeadSlot, nameof(HeadSlot));
                    break;
                case nameof(HeroInventoryVM.CapeSlot):
                    OnPropertyChangedWithValue(HeroInventory.CapeSlot, nameof(CapeSlot));
                    break;
                case nameof(HeroInventoryVM.BodySlot):
                    OnPropertyChangedWithValue(HeroInventory.BodySlot, nameof(BodySlot));
                    break;
                case nameof(HeroInventoryVM.GlovesSlot):
                    OnPropertyChangedWithValue(HeroInventory.GlovesSlot, nameof(GlovesSlot));
                    break;
                case nameof(HeroInventoryVM.LegSlot):
                    OnPropertyChangedWithValue(HeroInventory.LegSlot, nameof(LegSlot));
                    break;
                case nameof(HeroInventoryVM.HorseSlot):
                    OnPropertyChangedWithValue(HeroInventory.HorseSlot, nameof(HorseSlot));
                    break;
                case nameof(HeroInventoryVM.HorseHarnessSlot):
                    OnPropertyChangedWithValue(HeroInventory.HorseHarnessSlot, nameof(HorseHarnessSlot));
                    break;
                case nameof(HeroInventoryVM.Weapon0Slot):
                    OnPropertyChangedWithValue(HeroInventory.Weapon0Slot, nameof(Weapon0Slot));
                    break;
                case nameof(HeroInventoryVM.Weapon1Slot):
                    OnPropertyChangedWithValue(HeroInventory.Weapon1Slot, nameof(Weapon1Slot));
                    break;
                case nameof(HeroInventoryVM.Weapon2Slot):
                    OnPropertyChangedWithValue(HeroInventory.Weapon2Slot, nameof(Weapon2Slot));
                    break;
                case nameof(HeroInventoryVM.Weapon3Slot):
                    OnPropertyChangedWithValue(HeroInventory.Weapon3Slot, nameof(Weapon3Slot));
                    break;
                case nameof(HeroInventoryVM.BannerSlot):
                    OnPropertyChangedWithValue(HeroInventory.BannerSlot, nameof(BannerSlot));
                    break;
                case nameof(HeroInventoryVM.SelectedLoadoutIndex):
                    // Sync the 3D character model equipment with the inventory loadout selection
                    HeroCharacter?.SetUseCivilianEquipment(HeroInventory.SelectedLoadoutIndex == 1);
                    break;
            }
        }

        #endregion

        #region DataSource Properties

        /// <summary>
        /// Gets the currently selected hero.
        /// </summary>
        public Hero Hero => _hero;

        /// <summary>
        /// Gets or sets whether the editor panel is visible.
        /// </summary>
        [DataSourceProperty]
        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value, nameof(IsVisible));
        }

        /// <summary>
        /// Gets the selected hero's StringId for display.
        /// </summary>
        [DataSourceProperty]
        public string SelectedHeroStringId
        {
            get => _selectedHeroStringId;
            set => SetProperty(ref _selectedHeroStringId, value, nameof(SelectedHeroStringId));
        }

        // New Panel ViewModels - 1:1 match with XML panels

        /// <summary>
        /// Gets the hero portrait panel ViewModel.
        /// </summary>
        [DataSourceProperty]
        public HeroPortraitPanelVM HeroPortraitPanel
        {
            get => _heroPortraitPanel;
            private set => SetProperty(ref _heroPortraitPanel, value, nameof(HeroPortraitPanel));
        }

        /// <summary>
        /// Gets the hero name panel ViewModel.
        /// </summary>
        [DataSourceProperty]
        public HeroNamePanelVM HeroNamePanel
        {
            get => _heroNamePanel;
            private set => SetProperty(ref _heroNamePanel, value, nameof(HeroNamePanel));
        }

        /// <summary>
        /// Gets the hero action buttons panel ViewModel.
        /// </summary>
        [DataSourceProperty]
        public HeroActionButtonsPanelVM HeroActionButtonsPanel
        {
            get => _heroActionButtonsPanel;
            private set => SetProperty(ref _heroActionButtonsPanel, value, nameof(HeroActionButtonsPanel));
        }

        /// <summary>
        /// Gets the clan banner panel ViewModel.
        /// </summary>
        [DataSourceProperty]
        public ClanBannerPanelVM ClanBannerPanel
        {
            get => _clanBannerPanel;
            private set => SetProperty(ref _clanBannerPanel, value, nameof(ClanBannerPanel));
        }

        /// <summary>
        /// Gets the hero identity info panel ViewModel.
        /// </summary>
        [DataSourceProperty]
        public HeroIdentityInfoPanelVM HeroIdentityInfoPanel
        {
            get => _heroIdentityInfoPanel;
            private set => SetProperty(ref _heroIdentityInfoPanel, value, nameof(HeroIdentityInfoPanel));
        }

        /// <summary>
        /// Gets the hero clan panel ViewModel.
        /// </summary>
        [DataSourceProperty]
        public HeroClanPanelVM HeroClanPanel
        {
            get => _heroClanPanel;
            private set => SetProperty(ref _heroClanPanel, value, nameof(HeroClanPanel));
        }

        /// <summary>
        /// Gets the hero kingdom panel ViewModel.
        /// </summary>
        [DataSourceProperty]
        public HeroKingdomPanelVM HeroKingdomPanel
        {
            get => _heroKingdomPanel;
            private set => SetProperty(ref _heroKingdomPanel, value, nameof(HeroKingdomPanel));
        }

        // Existing ViewModels

        /// <summary>
        /// Gets the hero party sub-ViewModel.
        /// </summary>
        [DataSourceProperty]
        public HeroPartyVM HeroParty
        {
            get => _heroParty;
            private set => SetProperty(ref _heroParty, value, nameof(HeroParty));
        }

        /// <summary>
        /// Gets the hero skills sub-ViewModel.
        /// </summary>
        [DataSourceProperty]
        public HeroSkillsVM HeroSkills
        {
            get => _heroSkills;
            private set => SetProperty(ref _heroSkills, value, nameof(HeroSkills));
        }

        /// <summary>
        /// Gets the hero equipment sub-ViewModel.
        /// </summary>
        [DataSourceProperty]
        public HeroInventoryVM HeroInventory
        {
            get => _heroInventory;
            private set => SetProperty(ref _heroInventory, value, nameof(HeroInventory));
        }

        /// <summary>
        /// Gets the hero character sub-ViewModel (for 3D model display).
        /// </summary>
        [DataSourceProperty]
        public HeroCharacterVM HeroCharacter
        {
            get => _heroCharacter;
            private set => SetProperty(ref _heroCharacter, value, nameof(HeroCharacter));
        }

        // Pass-through properties for equipment slots to support Gauntlet's flat binding requirements
        // These forward to HeroInventoryVM's slot properties

        /// <summary>
        /// Pass-through property for head/helmet equipment slot from HeroInventory.
        /// </summary>
        [DataSourceProperty]
        public EquipmentSlotVM HeadSlot => HeroInventory?.HeadSlot;

        /// <summary>
        /// Pass-through property for cape equipment slot from HeroInventory.
        /// </summary>
        [DataSourceProperty]
        public EquipmentSlotVM CapeSlot => HeroInventory?.CapeSlot;

        /// <summary>
        /// Pass-through property for body/armor equipment slot from HeroInventory.
        /// </summary>
        [DataSourceProperty]
        public EquipmentSlotVM BodySlot => HeroInventory?.BodySlot;

        /// <summary>
        /// Pass-through property for gloves equipment slot from HeroInventory.
        /// </summary>
        [DataSourceProperty]
        public EquipmentSlotVM GlovesSlot => HeroInventory?.GlovesSlot;

        /// <summary>
        /// Pass-through property for leg/boots equipment slot from HeroInventory.
        /// </summary>
        [DataSourceProperty]
        public EquipmentSlotVM LegSlot => HeroInventory?.LegSlot;

        /// <summary>
        /// Pass-through property for horse equipment slot from HeroInventory.
        /// </summary>
        [DataSourceProperty]
        public EquipmentSlotVM HorseSlot => HeroInventory?.HorseSlot;

        /// <summary>
        /// Pass-through property for horse harness equipment slot from HeroInventory.
        /// </summary>
        [DataSourceProperty]
        public EquipmentSlotVM HorseHarnessSlot => HeroInventory?.HorseHarnessSlot;

        /// <summary>
        /// Pass-through property for weapon slot 0 from HeroInventory.
        /// </summary>
        [DataSourceProperty]
        public EquipmentSlotVM Weapon0Slot => HeroInventory?.Weapon0Slot;

        /// <summary>
        /// Pass-through property for weapon slot 1 from HeroInventory.
        /// </summary>
        [DataSourceProperty]
        public EquipmentSlotVM Weapon1Slot => HeroInventory?.Weapon1Slot;

        /// <summary>
        /// Pass-through property for weapon slot 2 from HeroInventory.
        /// </summary>
        [DataSourceProperty]
        public EquipmentSlotVM Weapon2Slot => HeroInventory?.Weapon2Slot;

        /// <summary>
        /// Pass-through property for weapon slot 3 from HeroInventory.
        /// </summary>
        [DataSourceProperty]
        public EquipmentSlotVM Weapon3Slot => HeroInventory?.Weapon3Slot;

        /// <summary>
        /// Pass-through property for banner equipment slot from HeroInventory.
        /// </summary>
        [DataSourceProperty]
        public EquipmentSlotVM BannerSlot => HeroInventory?.BannerSlot;

        #endregion

        #region Helper Methods

        /// <summary>
        /// Helper method to reduce boilerplate in bool property setters.
        /// </summary>
        private bool SetProperty(ref bool field, bool value, string propertyName)
        {
            if (field == value)
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Helper method to reduce boilerplate in string property setters.
        /// </summary>
        private bool SetProperty(ref string field, string value, string propertyName)
        {
            if (field == value)
                return false;

            field = value;
            OnPropertyChangedWithValue(value, propertyName);
            return true;
        }

        /// <summary>
        /// Helper method to reduce boilerplate in ViewModel property setters.
        /// </summary>
        private bool SetProperty<T>(ref T field, T value, string propertyName) where T : ViewModel
        {
            if (field == value)
                return false;

            field = value;
            OnPropertyChangedWithValue(value, propertyName);
            return true;
        }

        #endregion
    }
}
