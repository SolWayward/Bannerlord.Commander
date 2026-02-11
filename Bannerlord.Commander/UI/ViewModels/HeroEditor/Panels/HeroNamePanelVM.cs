using System;
using System.Globalization;
using Bannerlord.GameMaster.Heroes;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.Commander.UI.ViewModels.HeroEditor.Panels
{
    /// <summary>
    /// ViewModel for the combined hero title + name panel.
    /// Handles hero name editing with auto-save and displays the hero's title (read-only).
    /// </summary>
    public class HeroNamePanelVM : ViewModel
    {
        #region Private Fields

        private Hero _hero;
        private string _heroName;
        private string _originalName;
        private string _heroTitle;
        private bool _hasTitle;
        private bool _isRefreshing;
        private Action<string> _onNameChanged;

        #endregion

        #region Constructor

        public HeroNamePanelVM()
        {
            Clear();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the callback invoked when the hero name changes (for updating the hero list).
        /// </summary>
        public void SetOnNameChanged(Action<string> onNameChanged)
        {
            _onNameChanged = onNameChanged;
        }

        /// <summary>
        /// Refreshes the ViewModel with data from the specified hero.
        /// </summary>
        /// <param name="hero">The hero to display name/title for</param>
        public void RefreshForHero(Hero hero)
        {
            _hero = hero;

            if (_hero != null)
            {
                _isRefreshing = true;
                HeroName = _hero.Name?.ToString() ?? "";
                _originalName = HeroName;
                _isRefreshing = false;

                // Compute title word (e.g., "King", "Baron") - only for heroes in a kingdom faction
                if (_hero.MapFaction != null && _hero.MapFaction.IsKingdomFaction && _hero.MapFaction.Culture != null)
                {
                    HeroTitle = CleanTitle(HeroHelper.GetTitleInIndefiniteCase(_hero)?.ToString() ?? "");
                }

                else
                {
                    HeroTitle = "";
                }

                HasTitle = !string.IsNullOrEmpty(HeroTitle);
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
            _isRefreshing = true;
            HeroName = "";
            _originalName = "";
            HeroTitle = "";
            HasTitle = false;
            _isRefreshing = false;
        }

        #endregion

        #region DataSource Properties

        /// <summary>
        /// Gets or sets the hero's name (editable). Auto-saves to game object on user edit.
        /// </summary>
        [DataSourceProperty]
        public string HeroName
        {
            get => _heroName;
            set
            {
                if (SetProperty(ref _heroName, value, nameof(HeroName)))
                {
                    // Only persist to game object when user is editing (not during refresh)
                    if (!_isRefreshing && _hero != null && !string.IsNullOrWhiteSpace(value))
                    {
                        _hero.SetStringName(value);
                        _originalName = value;
                        _onNameChanged?.Invoke(value);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the hero's title (read-only display, e.g., "King", "Baron").
        /// </summary>
        [DataSourceProperty]
        public string HeroTitle
        {
            get => _heroTitle;
            private set => SetProperty(ref _heroTitle, value, nameof(HeroTitle));
        }

        /// <summary>
        /// Gets whether the hero has a title to display.
        /// Used for XML visibility binding.
        /// </summary>
        [DataSourceProperty]
        public bool HasTitle
        {
            get => _hasTitle;
            private set
            {
                if (_hasTitle != value)
                {
                    _hasTitle = value;
                    OnPropertyChanged(nameof(HasTitle));
                }
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Strips the indefinite article prefix ("a " or "an ") from the native title
        /// and capitalizes the first letter. The native GetTitleInIndefiniteCase returns
        /// titles like "a baron", "an archon" for non-rulers; rulers already lack the prefix.
        /// </summary>
        private static string CleanTitle(string rawTitle)
        {
            if (string.IsNullOrEmpty(rawTitle))
                return rawTitle;

            // Strip indefinite article prefix
            if (rawTitle.StartsWith("an ", StringComparison.OrdinalIgnoreCase))
            {
                rawTitle = rawTitle.Substring(3);
            }

            else if (rawTitle.StartsWith("a ", StringComparison.OrdinalIgnoreCase))
            {
                rawTitle = rawTitle.Substring(2);
            }

            // Capitalize first letter
            if (rawTitle.Length > 0)
            {
                rawTitle = char.ToUpper(rawTitle[0], CultureInfo.InvariantCulture) + rawTitle.Substring(1);
            }

            return rawTitle;
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

        #endregion
    }
}
