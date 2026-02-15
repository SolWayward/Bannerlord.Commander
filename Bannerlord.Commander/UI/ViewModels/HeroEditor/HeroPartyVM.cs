using Bannerlord.GameMaster.Information;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.Commander.UI.ViewModels.HeroEditor
{
    /// <summary>
    /// ViewModel for hero party information and management.
    /// Displays party status and provides buttons for party operations.
    /// </summary>
    public class HeroPartyVM : ViewModel
    {
        #region Private Fields

        private Hero _hero;
        private bool _isInParty;
        private bool _isPartyLeader;
        private int _partySize;
        private string _partyStatusText;
        private bool _canDisbandParty;
        private string _editPartyButtonText;

        #endregion

        #region Constructor

        public HeroPartyVM()
        {
            Clear();
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
                RefreshPartyInfo();
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
            IsInParty = false;
            IsPartyLeader = false;
            PartySize = 0;
            PartyStatusText = "Not in Party";
            CanDisbandParty = false;
            EditPartyButtonText = "Create Party";
        }

        #endregion

        #region Execute Methods

        /// <summary>
        /// Disbands the hero's party.
        /// Only available if the hero is the party leader.
        /// </summary>
        public void ExecuteDisbandParty()
        {
            if (_hero == null || !CanDisbandParty)
            {
                return;
            }

            // TODO: Awaiting BLGM implementation for DisbandParty
            InfoMessage.Warning("Party disbanding not yet supported - requires BLGM extension");
        }

        /// <summary>
        /// Opens a party assignment dialog to add the hero to an existing party.
        /// Placeholder for future implementation.
        /// </summary>
        public void ExecuteAddHeroToParty()
        {
            if (_hero == null)
            {
                return;
            }

            // TODO: Future implementation - party assignment UI
            InfoMessage.Status("Assign Party - Coming in a future update");
        }

        /// <summary>
        /// When the hero is in a party, opens the party editor.
        /// When the hero is not in a party, creates a new party.
        /// </summary>
        public void ExecuteEditOrCreateParty()
        {
            if (_hero == null)
            {
                return;
            }

            if (IsInParty)
            {
                // TODO: Future implementation - party editor
                InfoMessage.Status("Party editor - Coming in a future update");
            }

            else
            {
                // TODO: Awaiting BLGM implementation for CreateParty
                InfoMessage.Status("Create Party - Coming in a future update");
            }
        }

        #endregion

        #region DataSource Properties

        /// <summary>
        /// Gets whether the hero is in a party.
        /// </summary>
        [DataSourceProperty]
        public bool IsInParty
        {
            get => _isInParty;
            set => SetProperty(ref _isInParty, value, nameof(IsInParty));
        }

        /// <summary>
        /// Gets whether the hero is the party leader.
        /// </summary>
        [DataSourceProperty]
        public bool IsPartyLeader
        {
            get => _isPartyLeader;
            set => SetProperty(ref _isPartyLeader, value, nameof(IsPartyLeader));
        }

        /// <summary>
        /// Gets the party size (total member count).
        /// </summary>
        [DataSourceProperty]
        public int PartySize
        {
            get => _partySize;
            set => SetProperty(ref _partySize, value, nameof(PartySize));
        }

        /// <summary>
        /// Gets the party status text for display.
        /// </summary>
        [DataSourceProperty]
        public string PartyStatusText
        {
            get => _partyStatusText;
            set => SetProperty(ref _partyStatusText, value, nameof(PartyStatusText));
        }

        /// <summary>
        /// Gets whether the Disband Party button should be enabled.
        /// </summary>
        [DataSourceProperty]
        public bool CanDisbandParty
        {
            get => _canDisbandParty;
            set => SetProperty(ref _canDisbandParty, value, nameof(CanDisbandParty));
        }

        /// <summary>
        /// Gets the text for the Edit/Create Party button.
        /// Shows "Edit Party" when hero is in a party, "Create Party" otherwise.
        /// </summary>
        [DataSourceProperty]
        public string EditPartyButtonText
        {
            get => _editPartyButtonText;
            set => SetProperty(ref _editPartyButtonText, value, nameof(EditPartyButtonText));
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Refreshes all party-related information and button states.
        /// </summary>
        private void RefreshPartyInfo()
        {
            if (_hero == null)
            {
                Clear();
                return;
            }

            var party = _hero.PartyBelongedTo;

            // Determine party membership
            IsInParty = party != null;
            IsPartyLeader = party?.LeaderHero == _hero;

            // Get party size
            if (party != null && party.MemberRoster != null)
            {
                PartySize = party.MemberRoster.TotalManCount;
            }

            else
            {
                PartySize = 0;
            }

            // Build status text
            if (IsInParty)
            {
                if (IsPartyLeader)
                {
                    PartyStatusText = $"Leading Party ({PartySize} troops)";
                }

                else
                {
                    string leaderText = "Leaderless";

                    if (_hero.PartyBelongedTo.LeaderHero != null)
                        leaderText = $"{_hero.PartyBelongedTo.LeaderHero.Name}'s";

                    PartyStatusText = $"In {leaderText} Party ({PartySize} troops)";
                }
            }

            else
            {
                PartyStatusText = "Not in Party";
            }

            // Update button states
            CanDisbandParty = IsPartyLeader;
            EditPartyButtonText = IsInParty ? "Edit Party" : "Create Party";
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Helper method to reduce boilerplate in string property setters.
        /// </summary>
        private bool SetProperty(ref string field, string value, string propertyName)
        {
            if (field == value)
            {
                return false;
            }

            field = value;
            OnPropertyChangedWithValue(value, propertyName);
            return true;
        }

        /// <summary>
        /// Helper method to reduce boilerplate in int property setters.
        /// </summary>
        private bool SetProperty(ref int field, int value, string propertyName)
        {
            if (field == value)
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Helper method to reduce boilerplate in bool property setters.
        /// </summary>
        private bool SetProperty(ref bool field, bool value, string propertyName)
        {
            if (field == value)
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion
    }
}
