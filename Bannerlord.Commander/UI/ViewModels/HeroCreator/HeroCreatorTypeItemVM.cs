using Bannerlord.Commander.UI.Enums;
using TaleWorlds.Library;

namespace Bannerlord.Commander.UI.ViewModels.HeroCreator
{
    /// <summary>
    /// ViewModel for an individual type item in the Hero Creator type list.
    /// </summary>
    public class HeroCreatorTypeItemVM : ViewModel
    {
        private readonly HeroCreatorVM _parent;
        private readonly HeroCreatorType _typeEnum;
        private string _typeName;
        private bool _isSelected;
        private bool _isEnabled;

        public HeroCreatorTypeItemVM(HeroCreatorType typeEnum, string typeName,
            HeroCreatorVM parent, bool isEnabled)
        {
            _typeEnum = typeEnum;
            _parent = parent;
            TypeName = typeName;
            IsEnabled = isEnabled;
        }

        /// <summary>
        /// The underlying type enum value.
        /// </summary>
        public HeroCreatorType TypeEnum => _typeEnum;

        public void ExecuteSelect()
        {
            if (!IsEnabled) return;
            _parent?.SelectType(this);
        }

        [DataSourceProperty]
        public string TypeName
        {
            get => _typeName;
            set
            {
                if (_typeName != value)
                {
                    _typeName = value;
                    OnPropertyChangedWithValue(value, nameof(TypeName));
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
        /// False for MinorLord and Notable (greyed out, awaiting BLGM support).
        /// </summary>
        [DataSourceProperty]
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    OnPropertyChanged(nameof(IsEnabled));
                }
            }
        }
    }
}
