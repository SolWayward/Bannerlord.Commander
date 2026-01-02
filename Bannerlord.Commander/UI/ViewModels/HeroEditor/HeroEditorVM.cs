using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.Commander.UI.ViewModels.HeroEditor
{
    /// <summary>
    /// Main ViewModel for the Hero Editor Panel.
    /// Coordinates all sub-ViewModels and responds to hero selection changes.
    /// </summary>
    public class HeroEditorVM : ViewModel, IHeroSelectionHandler
    {
        #region Private Fields

        private Hero _hero;
        private HeroInfoVM _heroInfo;
        private HeroIdentityVM _heroIdentity;
        private HeroCultureClanVM _heroCultureClan;
        private HeroPartyVM _heroParty;
        private HeroSkillsVM _heroSkills;
        private HeroEquipmentVM _heroEquipment;
        private HeroCharacterVM _heroCharacter;
        private ClanSelectionPopupVM _clanSelectionPopup;
        private bool _isVisible;
        private string _selectedHeroStringId;

        #endregion

        #region Constructor

        public HeroEditorVM()
        {
            // Initialize sub-ViewModels
            HeroInfo = new();
            HeroIdentity = new();
            HeroCultureClan = new();
            HeroParty = new();
            HeroSkills = new();
            HeroEquipment = new();
            HeroCharacter = new();
            _clanSelectionPopup = new();
            IsVisible = false;
            SelectedHeroStringId = "";
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
                HeroInfo?.RefreshForHero(_hero);
                HeroIdentity?.RefreshForHero(_hero);
                HeroCultureClan?.RefreshForHero(_hero, _clanSelectionPopup);
                HeroParty?.RefreshForHero(_hero);
                HeroSkills?.RefreshForHero(_hero);
                HeroEquipment?.RefreshForHero(_hero);
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
            
            HeroInfo?.Clear();
            HeroIdentity?.Clear();
            HeroCultureClan?.Clear();
            HeroParty?.Clear();
            HeroSkills?.Clear();
            HeroEquipment?.Clear();
            HeroCharacter?.Clear();
        }

        /// <summary>
        /// Implementation of IHeroSelectionHandler - called when a hero is selected.
        /// </summary>
        public void SelectHero(HeroItemVM hero)
        {
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
            
            HeroInfo?.OnFinalize();
            HeroIdentity?.OnFinalize();
            HeroCultureClan?.OnFinalize();
            HeroParty?.OnFinalize();
            HeroSkills?.OnFinalize();
            HeroEquipment?.OnFinalize();
            HeroCharacter?.OnFinalize();
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

        /// <summary>
        /// Gets the hero info sub-ViewModel (portrait and read-only data).
        /// </summary>
        [DataSourceProperty]
        public HeroInfoVM HeroInfo
        {
            get => _heroInfo;
            private set => SetProperty(ref _heroInfo, value, nameof(HeroInfo));
        }

        /// <summary>
        /// Gets the hero identity sub-ViewModel (editable name/title).
        /// </summary>
        [DataSourceProperty]
        public HeroIdentityVM HeroIdentity
        {
            get => _heroIdentity;
            private set => SetProperty(ref _heroIdentity, value, nameof(HeroIdentity));
        }

        /// <summary>
        /// Gets the hero culture/clan sub-ViewModel.
        /// </summary>
        [DataSourceProperty]
        public HeroCultureClanVM HeroCultureClan
        {
            get => _heroCultureClan;
            private set => SetProperty(ref _heroCultureClan, value, nameof(HeroCultureClan));
        }

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
        public HeroEquipmentVM HeroEquipment
        {
            get => _heroEquipment;
            private set => SetProperty(ref _heroEquipment, value, nameof(HeroEquipment));
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

        /// <summary>
        /// Gets the clan selection popup ViewModel.
        /// </summary>
        [DataSourceProperty]
        public ClanSelectionPopupVM ClanSelectionPopup
        {
            get => _clanSelectionPopup;
            private set => SetProperty(ref _clanSelectionPopup, value, nameof(ClanSelectionPopup));
        }

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
