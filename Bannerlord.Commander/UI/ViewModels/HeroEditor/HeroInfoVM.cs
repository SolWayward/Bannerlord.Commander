using Bannerlord.Commander.Settings;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;

namespace Bannerlord.Commander.UI.ViewModels.HeroEditor
{
    /// <summary>
    /// ViewModel for read-only hero information display.
    /// Shows hero portrait and basic hero statistics.
    /// Creates CharacterImageIdentifierVM directly for portrait display using native pattern.
    /// </summary>
    public class HeroInfoVM : ViewModel
    {
        #region Private Fields

        private Hero _hero;
        private CharacterImageIdentifierVM _portraitImage;
        private BannerImageIdentifierVM _clanBanner;
        private string _gender;
        private string _ageText;
        private string _genderAgeText;
        private string _birthDateText;
        private string _deathDateText;
        private bool _isDead;

        #endregion

        #region Constructor

        public HeroInfoVM()
        {
            ClearFields();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes the ViewModel with data from the specified hero.
        /// </summary>
        /// <param name="hero">The hero to display information for</param>
        public void RefreshForHero(Hero hero)
        {
            _hero = hero;

            if (_hero != null)
            {
                // Finalize previous portrait if it exists before creating new one
                _portraitImage?.OnFinalize();

                // Create CharacterPortrait
                CharacterCode characterCode = Helpers.CharacterCodeHelpers.BuildCharacterCode(_hero.CharacterObject, true, SettingsManager.HeroSettings.ShowHiddenInfo);
                PortraitImage = new CharacterImageIdentifierVM(characterCode);

                // Update clan banner
                _clanBanner?.OnFinalize();
                ClanBanner = new BannerImageIdentifierVM(_hero.ClanBanner, true);

                Gender = _hero.IsFemale ? "Female" : "Male";
                AgeText = ((int)_hero.Age).ToString();
                GenderAgeText = $"Gender: {Gender}  Age: {AgeText}";
                BirthDateText = $"Born: {GetBirthDateDisplay(_hero)}";

                IsDead = !_hero.IsAlive;

                // Show death date if one is set, regardless of whether hero is currently dead
                string deathDisplay = GetDeathDateDisplay(_hero);
                DeathDateText = deathDisplay != "-" ? $"Death: {deathDisplay}" : "Death: -";
            }
            else
            {
                Clear();
            }
        }

        /// <summary>
        /// Clears all data from the ViewModel.
        /// Sets PortraitImage to null instead of creating empty VM to avoid silhouette.
        /// </summary>
        public void Clear()
        {
            _hero = null;

            // Finalize and set to null - don't create empty VM which causes silhouette
            _portraitImage?.OnFinalize();
            _portraitImage = null;
            OnPropertyChanged(nameof(PortraitImage));

            _clanBanner?.OnFinalize();
            _clanBanner = null;
            OnPropertyChanged(nameof(ClanBanner));

            ClearFields();
        }

        /// <summary>
        /// Clears all text/value fields without touching the image VMs.
        /// </summary>
        private void ClearFields()
        {
            Gender = "";
            AgeText = "";
            GenderAgeText = "";
            BirthDateText = "";
            DeathDateText = "";
            IsDead = false;
        }

        public override void OnFinalize()
        {
            base.OnFinalize();
            _portraitImage?.OnFinalize();
            ClanBanner?.OnFinalize();
        }

        #endregion

        #region DataSource Properties

        /// <summary>
        /// Gets the character portrait image identifier for display.
        /// Bind directly to this property in XML using DataSource="{PortraitImage}".
        /// This follows the native pattern used in ClanLordItemVM, GameMenuPartyItemVM, etc.
        /// </summary>
        [DataSourceProperty]
        public CharacterImageIdentifierVM PortraitImage
        {
            get => _portraitImage;
            private set
            {
                if (_portraitImage != value)
                {
                    _portraitImage = value;
                    OnPropertyChangedWithValue(value, nameof(PortraitImage));
                }
            }
        }

        /// <summary>
        /// Gets the clan banner image identifier for ImageIdentifierWidget display.
        /// </summary>
        [DataSourceProperty]
        public BannerImageIdentifierVM ClanBanner
        {
            get => _clanBanner;
            private set
            {
                if (_clanBanner != value)
                {
                    _clanBanner = value;
                    OnPropertyChangedWithValue(value, nameof(ClanBanner));
                }
            }
        }

        /// <summary>
        /// Gets the hero's gender display text.
        /// </summary>
        [DataSourceProperty]
        public string Gender
        {
            get => _gender;
            private set => SetProperty(ref _gender, value, nameof(Gender));
        }

        /// <summary>
        /// Gets the hero's age as text.
        /// </summary>
        [DataSourceProperty]
        public string AgeText
        {
            get => _ageText;
            private set => SetProperty(ref _ageText, value, nameof(AgeText));
        }

        /// <summary>
        /// Gets the combined gender and age text for compact display.
        /// </summary>
        [DataSourceProperty]
        public string GenderAgeText
        {
            get => _genderAgeText;
            private set => SetProperty(ref _genderAgeText, value, nameof(GenderAgeText));
        }

        /// <summary>
        /// Gets the hero's birth date display text.
        /// </summary>
        [DataSourceProperty]
        public string BirthDateText
        {
            get => _birthDateText;
            private set => SetProperty(ref _birthDateText, value, nameof(BirthDateText));
        }

        /// <summary>
        /// Gets the hero's death date display text.
        /// </summary>
        [DataSourceProperty]
        public string DeathDateText
        {
            get => _deathDateText;
            private set => SetProperty(ref _deathDateText, value, nameof(DeathDateText));
        }

        /// <summary>
        /// Gets whether the hero is dead.
        /// </summary>
        [DataSourceProperty]
        public bool IsDead
        {
            get => _isDead;
            private set => SetProperty(ref _isDead, value, nameof(IsDead));
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets a formatted birth date display string.
        /// </summary>
        private string GetBirthDateDisplay(Hero hero)
        {
            if (hero?.BirthDay == null)
                return "Unknown";

            try
            {
                CampaignTime birthDay = hero.BirthDay;
                int year = (int)birthDay.GetYear;
                int seasonIndex = (int)birthDay.GetSeasonOfYear;
                string seasonName = GetSeasonName(seasonIndex);
                int day = (int)birthDay.GetDayOfSeason + 1;

                return $"{seasonName} {day}, {year}";
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Gets a formatted death date display string.
        /// </summary>
        private string GetDeathDateDisplay(Hero hero)
        {
            if (hero?.DeathDay == null)
                return "-";

            try
            {
                CampaignTime deathDay = hero.DeathDay;
                int year = (int)deathDay.GetYear;
                int seasonIndex = (int)deathDay.GetSeasonOfYear;
                string seasonName = GetSeasonName(seasonIndex);
                int day = (int)deathDay.GetDayOfSeason + 1;

                return $"{seasonName} {day}, {year}";
            }
            catch
            {
                return "-";
            }
        }

        /// <summary>
        /// Gets the season name from season index.
        /// </summary>
        private string GetSeasonName(int seasonIndex)
        {
            return seasonIndex switch
            {
                0 => "Spring",
                1 => "Summer",
                2 => "Autumn",
                3 => "Winter",
                _ => "Unknown"
            };
        }

        #endregion

        #region Helper Methods

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

        #endregion
    }
}