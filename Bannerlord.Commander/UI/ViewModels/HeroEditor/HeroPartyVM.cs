using System;
using Bannerlord.GameMaster.Heroes;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
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
        private bool _canCreateParty;
        private bool _canLeaveParty;
        private string _createLeaveButtonText;

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
            CanCreateParty = false;
            CanLeaveParty = false;
            CreateLeaveButtonText = "Create Party";
        }

        #endregion

        #region Execute Methods

        /// <summary>
        /// Disbands the hero's party.
        /// Only available if the hero is the party leader.
        /// NOTE: Party disbanding is not yet implemented in BLGM.
        /// TODO: Request BLGM implementation for DisbandParty or RemoveParty extension method.
        /// </summary>
        public void ExecuteDisbandParty()
        {
            if (_hero == null || !CanDisbandParty)
                return;
            
            // Party disbanding not yet supported - BLGM needs to implement this feature
            InformationManager.DisplayMessage(
                new InformationMessage("Party disbanding not yet supported - requires BLGM extension",
                TaleWorlds.Library.Color.FromUint(4291559424u)));
        }

        /// <summary>
        /// Creates a new party for the hero or leaves the current party.
        /// Uses BLGM API's GetHomeOrAlternativeSettlement() for party creation.
        /// </summary>
        public void ExecuteCreateOrLeaveParty()
        {
            if (_hero == null)
                return;
            
            try
            {
                if (!IsInParty)
                {
                    // Create a new party using BLGM API
                    var settlement = _hero.GetHomeOrAlternativeSettlement();
                    if (settlement != null)
                    {
                        _hero.CreateParty(settlement);
                        
                        InformationManager.DisplayMessage(
                            new InformationMessage($"Party created for {_hero.Name} at {settlement.Name}", 
                            TaleWorlds.Library.Color.FromUint(4282569842u)));
                    }
                    else
                    {
                        InformationManager.DisplayMessage(
                            new InformationMessage("Could not find a suitable settlement for party creation", 
                            TaleWorlds.Library.Color.FromUint(4291559424u)));
                    }
                }
                else if (!IsPartyLeader)
                {
                    // Leave the current party
                    var party = _hero.PartyBelongedTo;
                    if (party != null && party.MemberRoster != null)
                    {
                        party.MemberRoster.RemoveTroop(_hero.CharacterObject, 1);
                        
                        InformationManager.DisplayMessage(
                            new InformationMessage($"{_hero.Name} left the party", 
                            TaleWorlds.Library.Color.FromUint(4282569842u)));
                    }
                }
                
                RefreshPartyInfo();
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(
                    new InformationMessage($"Failed to create/leave party: {ex.Message}", 
                    TaleWorlds.Library.Color.FromUint(4291559424u)));
            }
        }

        /// <summary>
        /// Opens the party editor.
        /// Placeholder for future implementation.
        /// </summary>
        public void ExecuteEditParty()
        {
            // TODO: Future implementation - party editor
            InformationManager.DisplayMessage(
                new InformationMessage("Party editor - Coming in a future update", 
                TaleWorlds.Library.Color.FromUint(4282569842u)));
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
        /// Gets whether the Create Party button should be enabled.
        /// </summary>
        [DataSourceProperty]
        public bool CanCreateParty
        {
            get => _canCreateParty;
            set => SetProperty(ref _canCreateParty, value, nameof(CanCreateParty));
        }

        /// <summary>
        /// Gets whether the Leave Party button should be enabled.
        /// </summary>
        [DataSourceProperty]
        public bool CanLeaveParty
        {
            get => _canLeaveParty;
            set => SetProperty(ref _canLeaveParty, value, nameof(CanLeaveParty));
        }

        /// <summary>
        /// Gets the text for the Create/Leave Party button.
        /// </summary>
        [DataSourceProperty]
        public string CreateLeaveButtonText
        {
            get => _createLeaveButtonText;
            set => SetProperty(ref _createLeaveButtonText, value, nameof(CreateLeaveButtonText));
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
                    PartyStatusText = $"In Party ({PartySize} troops)";
                }
            }
            else
            {
                PartyStatusText = "Not in Party";
            }
            
            // Update button states
            CanDisbandParty = IsPartyLeader;
            CanCreateParty = !IsInParty;
            CanLeaveParty = IsInParty && !IsPartyLeader;
            
            // Update Create/Leave button text
            if (IsInParty && !IsPartyLeader)
            {
                CreateLeaveButtonText = "Leave Party";
            }
            else
            {
                CreateLeaveButtonText = "Create Party";
            }
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
        /// Helper method to reduce boilerplate in int property setters.
        /// </summary>
        private bool SetProperty(ref int field, int value, string propertyName)
        {
            if (field == value)
                return false;

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
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion
    }
}
