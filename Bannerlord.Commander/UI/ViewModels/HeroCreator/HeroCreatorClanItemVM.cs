using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.Commander.UI.ViewModels.HeroCreator
{
    /// <summary>
    /// ViewModel for an individual clan item in the Hero Creator clan list.
    /// </summary>
    public class HeroCreatorClanItemVM : ViewModel
    {
        private readonly HeroCreatorVM _parent;
        private readonly Clan _clan;
        private string _clanName;
        private string _kingdomName;
        private bool _isSelected;
        private bool _isSpecial;

        public HeroCreatorClanItemVM(Clan clan, string clanName, string kingdomName,
            HeroCreatorVM parent, bool isSpecial = false)
        {
            _clan = clan;
            _parent = parent;
            ClanName = clanName;
            KingdomName = kingdomName;
            IsSpecial = isSpecial;
        }

        /// <summary>
        /// The underlying Clan object. Null for "Random" pseudo-item.
        /// </summary>
        public Clan Clan => _clan;

        public void ExecuteSelect()
        {
            _parent?.SelectClan(this);
        }

        [DataSourceProperty]
        public string ClanName
        {
            get => _clanName;
            set
            {
                if (_clanName != value)
                {
                    _clanName = value;
                    OnPropertyChangedWithValue(value, nameof(ClanName));
                }
            }
        }

        [DataSourceProperty]
        public string KingdomName
        {
            get => _kingdomName;
            set
            {
                if (_kingdomName != value)
                {
                    _kingdomName = value;
                    OnPropertyChangedWithValue(value, nameof(KingdomName));
                }
            }
        }

        [DataSourceProperty]
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        /// <summary>
        /// True for "Random" and "Player Clan" items for visual distinction.
        /// </summary>
        [DataSourceProperty]
        public bool IsSpecial
        {
            get => _isSpecial;
            set
            {
                if (_isSpecial != value)
                {
                    _isSpecial = value;
                    OnPropertyChanged(nameof(IsSpecial));
                }
            }
        }
    }
}
