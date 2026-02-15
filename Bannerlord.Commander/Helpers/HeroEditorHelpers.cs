using TaleWorlds.CampaignSystem;

namespace Bannerlord.Commander.Helpers
{
    /// <summary>
    /// Utility methods for HeroEditor
    /// </summary>
    public static class HeroEditorHelpers
    {
        /// MARK: ResolveHeroDescript
        /// <summary>
        /// Resolve a custom rank description to fill in the gaps missing from the native rank descriptions
        /// </summary>
        public static string ResolveCustomHeroDescription(Hero hero)
        {
            string value = "";
            // Resolve custom description
            if (hero != null)
            {
                // Current Settlement
                string currentSettlement = "Unknown";
                if (hero.CurrentSettlement != null)
                    currentSettlement = hero.CurrentSettlement.GetName().ToString();

                // Leader
                if (hero.IsClanLeader)
                    value = $"Leader of {hero.Clan.GetName()}";

                // Gang Leader
                else if (hero.IsGangLeader)
                    value = $"Gang Leader of {currentSettlement}";

                // Bandit
                else if (hero.Culture != null && hero.Culture.IsBandit)
                    value = $"Bandit of {hero.Culture.GetName()}";

                // Preacher
                else if (hero.IsPreacher)
                {
                    if (hero.Clan != null)
                        value = $"Preacher of {hero.Clan.Name}";
                    else
                        value = $"Preacher at {hero.CurrentSettlement}";
                }

                // Rebel
                else if (hero.IsRebel)
                {
                    if (hero.Clan != null)
                        value = $"Rebel of {hero.Clan.Name}";
                    else
                        value = $"Rebel at {hero.CurrentSettlement}";
                }

                // Merchant
                else if (hero.IsMerchant)
                    value = $"Merchant of {currentSettlement}";

                // Artisan
                else if (hero.IsArtisan)
                    value = $"Artisan of {currentSettlement}";

                // Headman
                else if (hero.IsHeadman)
                    value = $"Headman of {currentSettlement}";

                // Notable
                else if (hero.IsRuralNotable)
                    value = $"Rural Notable of {currentSettlement}";
                else if (hero.IsUrbanNotable)
                    value = $"Urban Notable of {currentSettlement}";
                else if (hero.IsNotable)
                    value = $"Notable of {currentSettlement}";

                // Wanderer
                else if (hero.IsWanderer)
                {
                    if (hero.Clan == null)
                        value = $"Bannerless Wanderer at {currentSettlement}";
                    else
                        value = $"Wanderer pledged to {hero.Clan.GetName()}";
                }

                // Minor Lord
                else if (hero.IsMinorFactionHero)
                {
                    if (hero.Clan != null)
                    {
                        if (hero.Clan.IsUnderMercenaryService && hero.Clan.Kingdom != null)
                            value = $"Mercenary of {hero.Clan.Kingdom.GetName()}";
                        else
                            value = $"Lesser Noble of {hero.Clan.GetName()}";
                    }

                    else
                        value = $"Lesser Bannerless Noble";
                }

                else if (hero.IsLord)
                {
                    if (hero.Clan != null)
                        value = $"Noble of {hero.Clan.GetName()}";

                    else
                        value = "Bannerless Noble";
                }

                else
                    value = "Unknown rank";
            }

            return value;
        }
    }
}