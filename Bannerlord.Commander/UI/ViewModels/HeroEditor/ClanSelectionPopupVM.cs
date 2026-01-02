using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.Commander.UI.ViewModels.HeroEditor
{
    /// <summary>
    /// ViewModel for the clan selection popup.
    /// Displays all game clans with filtering capability.
    /// NOTE: Banner display is simplified - full ImageIdentifierVM implementation pending.
    /// </summary>
    public class ClanSelectionPopupVM : ViewModel
    {
        #region Private Fields

        private Hero _currentHero;
        private bool _isVisible;
        private string _filterText;
        private MBBindingList<ClanItemVM> _clans;
        private List<ClanItemVM> _allClansUnfiltered;
        private Action _onClanSelected;

        #endregion

        #region Constructor

        public ClanSelectionPopupVM()
        {
            Clans = new MBBindingList<ClanItemVM>();
            _allClansUnfiltered = new List<ClanItemVM>();
            FilterText = "";
            IsVisible = false;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Shows the popup with all available clans.
        /// </summary>
        /// <param name="hero">The hero whose clan will be changed</param>
        /// <param name="onClanSelected">Callback when a clan is selected</param>
        public void ShowPopup(Hero hero, Action onClanSelected = null)
        {
            _currentHero = hero;
            _onClanSelected = onClanSelected;
            
            InitializeClans();
            IsVisible = true;
        }

        /// <summary>
        /// Hides the popup.
        /// </summary>
        public void Clear()
        {
            _currentHero = null;
            _onClanSelected = null;
            IsVisible = false;
            FilterText = "";
            Clans.Clear();
            _allClansUnfiltered.Clear();
        }

        public override void OnFinalize()
        {
            base.OnFinalize();
            
            if (Clans != null)
            {
                foreach (var clan in Clans)
                {
                    clan?.OnFinalize();
                }
            }
        }

        #endregion

        #region Execute Methods

        /// <summary>
        /// Closes the popup without making changes.
        /// </summary>
        public void ExecuteClose()
        {
            IsVisible = false;
        }

        /// <summary>
        /// Selects a clan and assigns it to the hero.
        /// </summary>
        /// <param name="clanItem">The clan item to select</param>
        public void ExecuteSelectClan(ClanItemVM clanItem)
        {
            if (_currentHero == null || clanItem?.Clan == null)
                return;
            
            try
            {
                // Change the hero's clan
                _currentHero.Clan = clanItem.Clan;
                
                InformationManager.DisplayMessage(
                    new InformationMessage($"{_currentHero.Name} moved to {clanItem.Clan.Name}", 
                    TaleWorlds.Library.Color.FromUint(4282569842u)));
                
                // Invoke callback to refresh the parent view
                _onClanSelected?.Invoke();
                
                // Close the popup
                IsVisible = false;
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(
                    new InformationMessage($"Failed to change clan: {ex.Message}", 
                    TaleWorlds.Library.Color.FromUint(4291559424u)));
            }
        }

        #endregion

        #region DataSource Properties

        /// <summary>
        /// Gets or sets whether the popup is visible.
        /// </summary>
        [DataSourceProperty]
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    OnPropertyChanged(nameof(IsVisible));
                }
            }
        }

        /// <summary>
        /// Gets or sets the filter text for searching clans.
        /// </summary>
        [DataSourceProperty]
        public string FilterText
        {
            get => _filterText;
            set
            {
                if (_filterText != value)
                {
                    _filterText = value;
                    OnPropertyChangedWithValue(value, nameof(FilterText));
                    ApplyFilter();
                }
            }
        }

        /// <summary>
        /// Gets the filtered list of clans.
        /// </summary>
        [DataSourceProperty]
        public MBBindingList<ClanItemVM> Clans
        {
            get => _clans;
            private set
            {
                if (_clans != value)
                {
                    _clans = value;
                    OnPropertyChangedWithValue(value, nameof(Clans));
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initializes the clan list with all available clans from the game.
        /// </summary>
        private void InitializeClans()
        {
            Clans.Clear();
            _allClansUnfiltered.Clear();
            
            // Get all clans from the game
            foreach (var clan in Clan.All)
            {
                if (clan != null)
                {
                    var clanVM = new ClanItemVM(clan, this);
                    _allClansUnfiltered.Add(clanVM);
                }
            }
            
            // Sort clans by name
            _allClansUnfiltered = _allClansUnfiltered.OrderBy(c => c.ClanName).ToList();
            
            // Apply initial filter (which will populate Clans list)
            ApplyFilter();
        }

        /// <summary>
        /// Applies the current filter text to the clan list.
        /// </summary>
        private void ApplyFilter()
        {
            Clans.Clear();
            
            if (string.IsNullOrWhiteSpace(FilterText))
            {
                // No filter - show all clans
                foreach (var clan in _allClansUnfiltered)
                {
                    Clans.Add(clan);
                }
            }
            else
            {
                // Filter by clan name or kingdom name
                string filter = FilterText.ToLowerInvariant();
                foreach (var clan in _allClansUnfiltered)
                {
                    if (clan.ClanName.ToLowerInvariant().Contains(filter) ||
                        clan.KingdomName.ToLowerInvariant().Contains(filter))
                    {
                        Clans.Add(clan);
                    }
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// ViewModel representing a single clan in the selection list.
    /// NOTE: Banner display is simplified - full ImageIdentifierVM implementation pending.
    /// </summary>
    public class ClanItemVM : ViewModel
    {
        #region Private Fields

        private readonly Clan _clan;
        private readonly ClanSelectionPopupVM _parent;
        private string _clanName;
        private string _kingdomName;
        private int _memberCount;
        private string _tierText;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new clan item ViewModel.
        /// </summary>
        /// <param name="clan">The clan object</param>
        /// <param name="parent">The parent popup ViewModel</param>
        public ClanItemVM(Clan clan, ClanSelectionPopupVM parent)
        {
            _clan = clan;
            _parent = parent;
            
            if (_clan != null)
            {
                ClanName = _clan.Name?.ToString() ?? "Unknown Clan";
                
                if (_clan.Kingdom != null)
                {
                    KingdomName = _clan.Kingdom.Name?.ToString() ?? "No Kingdom";
                }
                else
                {
                    KingdomName = "No Kingdom";
                }
                
                MemberCount = _clan.Heroes?.Count ?? 0;
                TierText = $"Tier {_clan.Tier}";
            }
        }

        #endregion

        #region Execute Methods

        /// <summary>
        /// Called when this clan item is clicked.
        /// </summary>
        public void ExecuteSelect()
        {
            _parent?.ExecuteSelectClan(this);
        }

        #endregion

        #region DataSource Properties

        /// <summary>
        /// Gets the clan object.
        /// </summary>
        public Clan Clan => _clan;

        /// <summary>
        /// Gets the clan name.
        /// </summary>
        [DataSourceProperty]
        public string ClanName
        {
            get => _clanName;
            private set
            {
                if (_clanName != value)
                {
                    _clanName = value;
                    OnPropertyChangedWithValue(value, nameof(ClanName));
                }
            }
        }

        /// <summary>
        /// Gets the kingdom name.
        /// </summary>
        [DataSourceProperty]
        public string KingdomName
        {
            get => _kingdomName;
            private set
            {
                if (_kingdomName != value)
                {
                    _kingdomName = value;
                    OnPropertyChangedWithValue(value, nameof(KingdomName));
                }
            }
        }

        /// <summary>
        /// Gets the member count.
        /// </summary>
        [DataSourceProperty]
        public int MemberCount
        {
            get => _memberCount;
            private set
            {
                if (_memberCount != value)
                {
                    _memberCount = value;
                    OnPropertyChanged(nameof(MemberCount));
                }
            }
        }

        /// <summary>
        /// Gets the tier text.
        /// </summary>
        [DataSourceProperty]
        public string TierText
        {
            get => _tierText;
            private set
            {
                if (_tierText != value)
                {
                    _tierText = value;
                    OnPropertyChangedWithValue(value, nameof(TierText));
                }
            }
        }

        #endregion
    }
}
