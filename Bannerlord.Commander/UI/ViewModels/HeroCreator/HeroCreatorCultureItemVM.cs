using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.Commander.UI.ViewModels.HeroCreator
{
    /// <summary>
    /// ViewModel for an individual culture item in the Hero Creator culture list.
    /// </summary>
    public class HeroCreatorCultureItemVM : ViewModel
    {
        private readonly HeroCreatorVM _parent;
        private readonly CultureObject _culture;
        private string _cultureName;
        private bool _isSelected;

        public HeroCreatorCultureItemVM(CultureObject culture, string cultureName, HeroCreatorVM parent)
        {
            _culture = culture;
            _parent = parent;
            CultureName = cultureName;
        }

        /// <summary>
        /// The underlying CultureObject. Null for "Random".
        /// </summary>
        public CultureObject Culture => _culture;

        public void ExecuteSelect()
        {
            _parent?.SelectCulture(this);
        }

        [DataSourceProperty]
        public string CultureName
        {
            get => _cultureName;
            set
            {
                if (_cultureName != value)
                {
                    _cultureName = value;
                    OnPropertyChangedWithValue(value, nameof(CultureName));
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
    }
}
