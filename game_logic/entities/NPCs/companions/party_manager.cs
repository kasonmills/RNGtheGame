using System.Collections.Generic;

namespace GameLogic.Entities.NPCs.Companions
{
    /// <summary>
    /// Owns the active companion roster and party-capacity rules.
    /// </summary>
    public class PartyManager
    {
        public List<Entity> ActiveCompanions { get; } = new List<Entity>();

        /// <summary>
        /// Get the maximum party size based on the player's Leadership ability
        /// Base size is 4 (player + 3 companions)
        /// Leadership passive ability can increase this up to 8 at level 100
        /// </summary>
        public int GetMaxPartySize(Player.Player player)
        {
            const int basePartySize = 4;

            if (player == null || player.SelectedAbility == null)
            {
                return basePartySize;
            }

            // Check if player has Leadership passive ability
            if (player.SelectedAbility is Abilities.LeadershipAbility leadership)
            {
                int bonus = leadership.GetPassiveBonusValue();
                return basePartySize + bonus;
            }

            return basePartySize;
        }

        /// <summary>
        /// Check if party has room for another companion
        /// </summary>
        public bool CanAddCompanion(Player.Player player)
        {
            // ActiveCompanions doesn't include the player, so max is (GetMaxPartySize() - 1)
            int maxCompanions = GetMaxPartySize(player) - 1;
            return ActiveCompanions.Count < maxCompanions;
        }
    }
}
