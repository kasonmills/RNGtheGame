using System.Collections.Generic;
using System.Linq;

namespace GameLogic.Entities.NPCs.Companions
{
    /// <summary>
    /// Owns the companion roster and party-capacity rules. Every recruited companion is on
    /// the roster; each one is either in the fighting party (InParty - fights and takes a
    /// party slot) or benched (recruited but sitting out). Benched companions still get
    /// half of each fight's XP (decided 2026-11-02).
    /// </summary>
    public class PartyManager
    {
        /// <summary>
        /// Everyone recruited, fighting or benched. This is the list to pass to
        /// CombatManager.StartCombat - it picks out the fighters itself.
        /// </summary>
        public List<Entity> RecruitedCompanions { get; } = new List<Entity>();

        /// <summary>
        /// Companions in the fighting party (appear in combat, max 3)
        /// </summary>
        public List<Entity> FightingCompanions => RecruitedCompanions.Where(IsFighting).ToList();

        /// <summary>
        /// Companions who are recruited but sitting out
        /// </summary>
        public List<Entity> BenchedCompanions => RecruitedCompanions.Where(c => !IsFighting(c)).ToList();

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
        /// Check if the fighting party has room for another companion
        /// </summary>
        public bool CanAddCompanion(Player.Player player)
        {
            // FightingCompanions doesn't include the player, so max is (GetMaxPartySize() - 1)
            int maxCompanions = GetMaxPartySize(player) - 1;
            return FightingCompanions.Count < maxCompanions;
        }

        /// <summary>
        /// Recruit a companion onto the roster. A new recruit is raised to the player's
        /// level minus 1 if they start below it (decided 2026-11-02), joins the fighting
        /// party if there is a free slot and is benched otherwise. Returns false only if
        /// the companion is null or already recruited.
        /// </summary>
        public bool RecruitCompanion(Player.Player player, Entity companion)
        {
            if (companion == null || RecruitedCompanions.Contains(companion))
            {
                return false;
            }

            if (companion is CompanionBase newRecruit)
            {
                newRecruit.RaiseToLevel(player.Level - 1);
            }

            RecruitedCompanions.Add(companion);

            if (CanAddCompanion(player))
            {
                PutInFightingParty(player, companion);
            }
            else if (companion is NPCBase benched)
            {
                benched.LeaveParty();
            }

            if (companion is CompanionBase companionBase)
            {
                // Items a companion brings into the party bag (e.g. Boon's starting bolts) - once only
                if (!companionBase.StartingItemsGiven)
                {
                    foreach (var item in companionBase.GetStartingPartyItems())
                    {
                        player.Inventory.AddItem(item);
                    }
                    companionBase.StartingItemsGiven = true;
                }
            }

            return true;
        }

        /// <summary>
        /// Move a fighting companion to the bench. Their passive bonus stops applying.
        /// TODO-GODOT: call this from the party menu.
        /// </summary>
        public bool MoveToBench(Player.Player player, Entity companion)
        {
            if (!RecruitedCompanions.Contains(companion) || !IsFighting(companion))
            {
                return false;
            }

            if (companion is CompanionBase companionBase)
            {
                companionBase.RemovePassiveBonus(player);
            }

            ((NPCBase)companion).LeaveParty();
            return true;
        }

        /// <summary>
        /// Move a benched companion into the fighting party. Fails if the party is full -
        /// use SwapCompanions() then.
        /// TODO-GODOT: call this from the party menu.
        /// </summary>
        public bool MoveToFighting(Player.Player player, Entity companion)
        {
            if (!RecruitedCompanions.Contains(companion) || IsFighting(companion) || !CanAddCompanion(player))
            {
                return false;
            }

            PutInFightingParty(player, companion);
            return true;
        }

        /// <summary>
        /// Bench one fighting companion and put a benched one in their place.
        /// TODO-GODOT: call this from the party menu.
        /// </summary>
        public bool SwapCompanions(Player.Player player, Entity fighting, Entity benched)
        {
            if (!RecruitedCompanions.Contains(benched) || IsFighting(benched))
            {
                return false;
            }

            return MoveToBench(player, fighting) && MoveToFighting(player, benched);
        }

        /// <summary>
        /// Replace the roster with companions rebuilt from a save. Their fighting/benched
        /// marks and passive state come from the save, so no bonus is applied again here.
        /// </summary>
        public void RestoreRoster(IEnumerable<CompanionBase> companions)
        {
            RecruitedCompanions.Clear();
            RecruitedCompanions.AddRange(companions);
        }

        private static bool IsFighting(Entity companion)
        {
            return companion is NPCBase npc && npc.InParty;
        }

        private static void PutInFightingParty(Player.Player player, Entity companion)
        {
            if (companion is NPCBase npcCompanion)
            {
                npcCompanion.JoinParty();
            }

            if (companion is CompanionBase companionBase)
            {
                companionBase.ApplyPassiveBonus(player);
            }
        }
    }
}
