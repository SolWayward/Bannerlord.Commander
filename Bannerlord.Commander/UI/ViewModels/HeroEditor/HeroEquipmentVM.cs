using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets.Inventory;

namespace Bannerlord.Commander.UI.ViewModels.HeroEditor
{
    /// <summary>
    /// ViewModel for hero equipment loadout management.
    /// Equipment slots are now managed at HeroEditorVM level following native SPInventoryVM pattern.
    /// This VM only handles loadout selection (Battle vs Civilian).
    /// </summary>
    public class HeroEquipmentVM : ViewModel
    {
        #region Private Fields

        private Hero _hero;
        private int _selectedLoadoutIndex;
        private string _loadoutName;

        #endregion

        #region Constructor

        public HeroEquipmentVM()
        {
            _selectedLoadoutIndex = 0;
            LoadoutName = "Battle";
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes the ViewModel with data from the specified hero.
        /// </summary>
        /// <param name="hero">The hero to display equipment for</param>
        public void RefreshForHero(Hero hero)
        {
            _hero = hero;
            Debug.Print("[HeroEquipment] RefreshForHero(): Set hero to " + (_hero != null ? _hero.Name.ToString() : "null"));
        }

        /// <summary>
        /// Clears all data from the ViewModel.
        /// </summary>
        public void Clear()
        {
            _hero = null;
            _selectedLoadoutIndex = 0;
            LoadoutName = "Battle";
        }

        #endregion

        #region Execute Methods

        /// <summary>
        /// Toggles between Battle and Civilian loadouts.
        /// </summary>
        public void ExecuteToggleLoadout()
        {
            _selectedLoadoutIndex = _selectedLoadoutIndex == 0 ? 1 : 0;
            LoadoutName = _selectedLoadoutIndex == 0 ? "Battle" : "Civilian";
            // Notify that loadout changed so parent can refresh equipment slots
            OnPropertyChanged(nameof(SelectedLoadoutIndex));
        }

        #endregion

        #region DataSource Properties

        /// <summary>
        /// Gets the current loadout name (Battle or Civilian).
        /// </summary>
        [DataSourceProperty]
        public string LoadoutName
        {
            get => _loadoutName;
            private set
            {
                if (_loadoutName != value)
                {
                    _loadoutName = value;
                    OnPropertyChangedWithValue(value, nameof(LoadoutName));
                }
            }
        }

        /// <summary>
        /// Gets the selected loadout index (0 = Battle, 1 = Civilian).
        /// Exposed for HeroEditorVM to access when refreshing equipment slots.
        /// </summary>
        public int SelectedLoadoutIndex => _selectedLoadoutIndex;

        #endregion
    }
}
