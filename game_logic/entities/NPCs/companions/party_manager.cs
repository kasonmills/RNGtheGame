using System.Collections.Generic;
using System.Linq;
using GameLogic.Combat;

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

            // A recruit's max health follows the party's difficulty (the player's multiplier),
            // before the catch-up levels so those gains are scaled too
            companion.ApplyMaxHealthMultiplier(player.MaxHealthMultiplier);

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
            companion.FormationSlot = Formation.NoSlot; // The bench has no slots
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

            // The one coming in takes the slot of the one going out
            int slot = fighting?.FormationSlot ?? Formation.NoSlot;
            if (!(MoveToBench(player, fighting) && MoveToFighting(player, benched)))
            {
                return false;
            }
            if (Formation.IsValidSlot(slot))
            {
                SetFormationSlot(player, benched, slot);
            }
            return true;
        }

        // ----- The formation (decided 2026-11-23 - see GameLogic.Combat.Formation) -----

        /// <summary>The player and the fighting companions - everyone with a formation slot.</summary>
        public List<Entity> GetFormationMembers(Player.Player player)
        {
            var members = new List<Entity>();
            if (player != null) members.Add(player);
            members.AddRange(FightingCompanions);
            return members;
        }

        /// <summary>
        /// Give everyone in the formation a slot of their own: kept if valid and not shared,
        /// otherwise the first free one (ranged users at the back first). Call after loading
        /// (old saves have no slots) - recruiting, benching and swapping keep it tidy themselves.
        /// </summary>
        public void FixFormation(Player.Player player)
        {
            var slots = Formation.Arrange(GetFormationMembers(player), m => m.FormationSlot);
            foreach (var pair in slots)
            {
                pair.Key.FormationSlot = pair.Value;
            }
        }

        /// <summary>
        /// Put the player or a fighting companion in a slot (0-1 front, 2-3 back) - from the
        /// party menu. Whoever was in that slot swaps to the mover's old slot. False (nothing
        /// changed) for a slot that doesn't exist or someone not in the formation.
        /// TODO-GODOT: call this from the party menu's formation view.
        /// </summary>
        public bool SetFormationSlot(Player.Player player, Entity member, int slot)
        {
            var members = GetFormationMembers(player);
            if (member == null || !members.Contains(member) || !Formation.IsValidSlot(slot))
            {
                return false;
            }

            FixFormation(player);
            var occupant = members.FirstOrDefault(m => m != member && m.FormationSlot == slot);
            if (occupant != null)
            {
                occupant.FormationSlot = member.FormationSlot;
            }
            member.FormationSlot = slot;
            return true;
        }

        /// <summary>Who is in a slot, or null if it's empty.</summary>
        public Entity GetMemberInSlot(Player.Player player, int slot)
        {
            return GetFormationMembers(player).FirstOrDefault(m => m.FormationSlot == slot);
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

        private void PutInFightingParty(Player.Player player, Entity companion)
        {
            if (companion is NPCBase npcCompanion)
            {
                npcCompanion.JoinParty();
            }

            if (companion is CompanionBase companionBase)
            {
                companionBase.ApplyPassiveBonus(player);
            }

            // A newcomer fills the first empty slot, ranged users at the back first
            companion.FormationSlot = Formation.NoSlot;
            FixFormation(player);
        }
    }
}
