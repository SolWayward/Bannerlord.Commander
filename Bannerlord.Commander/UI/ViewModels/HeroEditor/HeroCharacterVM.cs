using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.Commander.UI.ViewModels.HeroEditor
{
    /// <summary>
    /// ViewModel for displaying the 3D character model using CharacterTableauWidget.
    /// Provides all properties needed to render the character in the UI.
    /// </summary>
    public class HeroCharacterVM : ViewModel
    {
        #region Private Fields

        private Hero _hero;
        private string _bodyProperties;
        private string _equipmentCode;
        private bool _isFemale;
        private int _stanceIndex;
        private string _bannerCodeText;
        private string _charStringId;
        private string _mountCreationKey;
        private uint _armorColor1;
        private uint _armorColor2;
        private int _race;
        private bool _useCivilianEquipment;

        #endregion

        #region Constructor

        public HeroCharacterVM()
        {
            _bodyProperties = "";
            _equipmentCode = "";
            _isFemale = false;
            _stanceIndex = 0;
            _bannerCodeText = "";
            _charStringId = "";
            _mountCreationKey = "";
            _armorColor1 = 0;
            _armorColor2 = 0;
            _race = 0;
            _useCivilianEquipment = false;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes the ViewModel with data from the specified hero.
        /// </summary>
        /// <param name="hero">The hero to display</param>
        public void RefreshForHero(Hero hero)
        {
            _hero = hero;
            
            if (_hero != null)
            {
                RefreshCharacterData();
            }
            else
            {
                Clear();
            }
        }

        /// <summary>
        /// Clears all data from the ViewModel.
        /// </summary>
        public void Clear()
        {
            _hero = null;
            BodyProperties = "";
            EquipmentCode = "";
            IsFemale = false;
            StanceIndex = 0;
            BannerCodeText = "";
            CharStringId = "";
            MountCreationKey = "";
            ArmorColor1 = 0;
            ArmorColor2 = 0;
            Race = 0;
        }

        /// <summary>
        /// Toggles between battle and civilian equipment.
        /// </summary>
        public void ExecuteToggleEquipment()
        {
            _useCivilianEquipment = !_useCivilianEquipment;
            RefreshCharacterData();
        }

        /// <summary>
        /// Sets the civilian equipment mode and refreshes the character display.
        /// Called by HeroEditorVM to sync with HeroInventoryVM loadout changes.
        /// </summary>
        /// <param name="useCivilian">True for civilian equipment, false for battle equipment</param>
        public void SetUseCivilianEquipment(bool useCivilian)
        {
            if (_useCivilianEquipment != useCivilian)
            {
                _useCivilianEquipment = useCivilian;
                RefreshCharacterData();
            }
        }

        #endregion

        #region DataSource Properties

        /// <summary>
        /// Gets the body properties string for CharacterTableauWidget.
        /// </summary>
        [DataSourceProperty]
        public string BodyProperties
        {
            get => _bodyProperties;
            private set
            {
                if (_bodyProperties != value)
                {
                    _bodyProperties = value;
                    OnPropertyChangedWithValue(value, nameof(BodyProperties));
                }
            }
        }

        /// <summary>
        /// Gets the equipment code string for CharacterTableauWidget.
        /// </summary>
        [DataSourceProperty]
        public string EquipmentCode
        {
            get => _equipmentCode;
            private set
            {
                if (_equipmentCode != value)
                {
                    _equipmentCode = value;
                    OnPropertyChangedWithValue(value, nameof(EquipmentCode));
                }
            }
        }

        /// <summary>
        /// Gets whether the character is female.
        /// </summary>
        [DataSourceProperty]
        public bool IsFemale
        {
            get => _isFemale;
            private set
            {
                if (_isFemale != value)
                {
                    _isFemale = value;
                    OnPropertyChanged(nameof(IsFemale));
                }
            }
        }

        /// <summary>
        /// Gets the stance index for the character pose.
        /// </summary>
        [DataSourceProperty]
        public int StanceIndex
        {
            get => _stanceIndex;
            private set
            {
                if (_stanceIndex != value)
                {
                    _stanceIndex = value;
                    OnPropertyChanged(nameof(StanceIndex));
                }
            }
        }

        /// <summary>
        /// Gets the banner code text for the clan banner display.
        /// </summary>
        [DataSourceProperty]
        public string BannerCodeText
        {
            get => _bannerCodeText;
            private set
            {
                if (_bannerCodeText != value)
                {
                    _bannerCodeText = value;
                    OnPropertyChangedWithValue(value, nameof(BannerCodeText));
                }
            }
        }

        /// <summary>
        /// Gets the character string ID.
        /// </summary>
        [DataSourceProperty]
        public string CharStringId
        {
            get => _charStringId;
            private set
            {
                if (_charStringId != value)
                {
                    _charStringId = value;
                    OnPropertyChangedWithValue(value, nameof(CharStringId));
                }
            }
        }

        /// <summary>
        /// Gets the mount creation key for horse display.
        /// </summary>
        [DataSourceProperty]
        public string MountCreationKey
        {
            get => _mountCreationKey;
            private set
            {
                if (_mountCreationKey != value)
                {
                    _mountCreationKey = value;
                    OnPropertyChangedWithValue(value, nameof(MountCreationKey));
                }
            }
        }

        /// <summary>
        /// Gets the primary armor color.
        /// </summary>
        [DataSourceProperty]
        public uint ArmorColor1
        {
            get => _armorColor1;
            private set
            {
                if (_armorColor1 != value)
                {
                    _armorColor1 = value;
                    OnPropertyChanged(nameof(ArmorColor1));
                }
            }
        }

        /// <summary>
        /// Gets the secondary armor color.
        /// </summary>
        [DataSourceProperty]
        public uint ArmorColor2
        {
            get => _armorColor2;
            private set
            {
                if (_armorColor2 != value)
                {
                    _armorColor2 = value;
                    OnPropertyChanged(nameof(ArmorColor2));
                }
            }
        }

        /// <summary>
        /// Gets the race ID for the character.
        /// </summary>
        [DataSourceProperty]
        public int Race
        {
            get => _race;
            private set
            {
                if (_race != value)
                {
                    _race = value;
                    OnPropertyChanged(nameof(Race));
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Refreshes all character display data.
        /// </summary>
        private void RefreshCharacterData()
        {
            if (_hero == null)
                return;
            
            // Get body properties
            TaleWorlds.Core.BodyProperties bodyProps = _hero.BodyProperties;
            BodyProperties = bodyProps.ToString();
            
            // Get equipment code based on current mode
            Equipment equipment = _useCivilianEquipment ? _hero.CivilianEquipment : _hero.BattleEquipment;
            EquipmentCode = equipment?.CalculateEquipmentCode() ?? "";
            
            // Set basic properties
            IsFemale = _hero.IsFemale;
            CharStringId = _hero.CharacterObject?.StringId ?? "";
            Race = _hero.CharacterObject?.Race ?? 0;
            StanceIndex = 0; // Standing pose
            
            // Get faction colors (MapFaction returns Kingdom when clan is in one, otherwise the Clan itself)
            if (_hero.MapFaction != null)
            {
                ArmorColor1 = _hero.MapFaction.Color;
                ArmorColor2 = _hero.MapFaction.Color2;
            }
            else if (_hero.CharacterObject?.Culture != null)
            {
                // Fallback to culture colors if no faction
                ArmorColor1 = _hero.CharacterObject.Culture.Color;
                ArmorColor2 = _hero.CharacterObject.Culture.Color2;
            }
            else
            {
                ArmorColor1 = 0;
                ArmorColor2 = 0;
            }
            
            // Banner still comes from the clan (not the kingdom)
            BannerCodeText = _hero.ClanBanner?.Serialize() ?? "";
            
            // Get mount creation key if hero has a horse
            // Note: MountCreationKey retrieval may require different approach based on game version
            MountCreationKey = "";
            if (equipment != null)
            {
                EquipmentElement horseElement = equipment[EquipmentIndex.Horse];
                if (!horseElement.IsEmpty && horseElement.Item?.HorseComponent != null)
                {
                    // For now we'll leave mount key empty - proper implementation may need BLGM support
                    MountCreationKey = "";
                }
            }
        }

        #endregion
    }
}
