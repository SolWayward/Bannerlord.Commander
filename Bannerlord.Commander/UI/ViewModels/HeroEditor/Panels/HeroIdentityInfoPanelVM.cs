using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.Commander.UI.ViewModels.HeroEditor.Panels
{
    /// <summary>
    /// ViewModel for the hero identity information panel.
    /// Consolidates gender, age, birth/death dates, and culture information.
    /// Eliminates nested DataSource requirement by including CultureName directly.
    /// </summary>
    public class HeroIdentityInfoPanelVM : ViewModel
    {
        #region Private Fields

        private Hero _hero;
        private string _gender;
        private string _ageText;
        private string _birthDateText;
        private string _deathDateText;
        private string _cultureName;
        private string _aliveStatus;
        private string _aliveStatusColor;

        #endregion

        #region Constructor

        public HeroIdentityInfoPanelVM()
        {
            ClearFields();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes the ViewModel with data from the specified hero.
        /// </summary>
        /// <param name="hero">The hero to display identity info for</param>
        public void RefreshForHero(Hero hero)
        {
            _hero = hero;

            if (_hero != null)
            {
                Gender = _hero.IsFemale ? "Female" : "Male";
                AgeText = ((int)_hero.Age).ToString();
                BirthDateText = GetBirthDateDisplay(_hero);

                // Show death date if one is set, regardless of whether hero is currently dead
                string deathDisplay = GetDeathDateDisplay(_hero);
                DeathDateText = deathDisplay != "-" ? deathDisplay : "-";

                // Culture information
                CultureName = _hero.Culture?.Name?.ToString() ?? "Unknown";

                // Alive/Dead status
                if (_hero.IsAlive)
                {
                    AliveStatus = "Alive";
                    AliveStatusColor = "#90EE90FF"; // Light green pastel
                }
                else
                {
                    AliveStatus = "Dead";
                    AliveStatusColor = "#FFB3B3FF"; // Light red pastel
                }
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
            ClearFields();
        }

        /// <summary>
        /// Clears all text/value fields.
        /// </summary>
        private void ClearFields()
        {
            Gender = "";
            AgeText = "";
            BirthDateText = "";
            DeathDateText = "";
            CultureName = "Unknown";
            AliveStatus = "";
            AliveStatusColor = "#FFFFFFFF";
        }

        #endregion

        #region DataSource Properties

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
        /// Gets the hero's culture name display text.
        /// </summary>
        [DataSourceProperty]
        public string CultureName
        {
            get => _cultureName;
            private set => SetProperty(ref _cultureName, value, nameof(CultureName));
        }

        /// <summary>
        /// Gets the hero's alive/dead status text.
        /// </summary>
        [DataSourceProperty]
        public string AliveStatus
        {
            get => _aliveStatus;
            private set => SetProperty(ref _aliveStatus, value, nameof(AliveStatus));
        }

        /// <summary>
        /// Gets the color for the alive/dead status text.
        /// </summary>
        [DataSourceProperty]
        public string AliveStatusColor
        {
            get => _aliveStatusColor;
            private set => SetProperty(ref _aliveStatusColor, value, nameof(AliveStatusColor));
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

        #endregion
    }
}
