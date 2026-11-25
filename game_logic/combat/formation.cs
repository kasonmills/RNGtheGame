using System.Collections.Generic;
using System.Linq;
using GameLogic.Entities;
using GameLogic.Entities.Enemies;
using GameLogic.Entities.Enemies.Bosses;
using GameLogic.Entities.NPCs;
using GameLogic.Entities.NPCs.Companions;
using GameLogic.Items;

namespace GameLogic.Combat
{
    public enum FormationRow
    {
        Front,
        Back
    }

    /// <summary>
    /// The party's formation (rules decided 2026-11-23 - see done.md, "Positions"): 4 slots,
    /// 2 front (slots 0 and 1) and 2 back (slots 2 and 3). The formation is set in the party
    /// menu (PartyManager.SetFormationSlot), saved, and every fight starts with it
    /// (CombatManager copies it - a Reposition mid-fight doesn't change the saved one).
    /// - Enemies pick front-row members twice as often as back-row ones (their random picks
    ///   only - a pattern pick ignores rows).
    /// - Every attack on the back row (either side) gets -10 hit chance.
    /// - A melee attack from the back row does -30% damage (spear -10%). Ranged attacks have
    ///   no penalty in either row.
    /// - Spells, heals, items and abilities ignore rows. Bosses ignore rows.
    /// - A row with nobody standing in front of it counts as the front row.
    /// </summary>
    public static class Formation
    {
        public const int SlotCount = 4;
        public const int FrontSlotCount = 2;

        /// <summary>No slot yet - placed with FirstFreeSlot when it's needed.</summary>
        public const int NoSlot = -1;

        /// <summary>Hit chance taken off every attack on the back row.</summary>
        public const int BackRowHitPenalty = 10;

        /// <summary>A melee attack from the back row does 70% damage.</summary>
        public const double BackRowMeleeDamageMultiplier = 0.7;

        /// <summary>A spear from the back row does 90% damage - its reach.</summary>
        public const double BackRowSpearDamageMultiplier = 0.9;

        /// <summary>Enemies' random picks: a front-row member counts twice, a back-row one once.</summary>
        public const int FrontRowTargetWeight = 2;
        public const int BackRowTargetWeight = 1;

        public static bool IsValidSlot(int slot) => slot >= 0 && slot < SlotCount;

        /// <summary>The row a slot is in. No slot counts as the back row.</summary>
        public static FormationRow RowOf(int slot) => slot >= 0 && slot < FrontSlotCount ? FormationRow.Front : FormationRow.Back;

        /// <summary>Bow, crossbow, staff and wand are ranged; everything else is melee.</summary>
        public static bool IsRangedWeapon(WeaponType type)
        {
            return type == WeaponType.Bow || type == WeaponType.Crossbow
                || type == WeaponType.Staff || type == WeaponType.Wand;
        }

        /// <summary>
        /// The weapon a combatant fights with, or null: unarmed, cat-form Chance (gear does
        /// nothing in cat form), or an enemy.
        /// </summary>
        public static Weapon GetWeapon(Entity combatant)
        {
            switch (combatant)
            {
                case Entities.Player.Player player:
                    return player.EquippedWeapon;
                case CompanionChance chance when chance.Form == ChanceForm.Cat:
                    return null;
                case NPCBase npc:
                    return npc.EquippedWeapon;
                default:
                    return null;
            }
        }

        /// <summary>
        /// True when this combatant's plain attack is ranged. Party members: by their weapon
        /// (unarmed and cat-form Chance are melee). Enemies: a back-row enemy (archers, imps)
        /// fights at range, the rest are melee.
        /// </summary>
        public static bool AttacksAtRange(Entity combatant)
        {
            if (combatant is EnemyBase enemy)
            {
                return enemy.IsBackRow;
            }

            var weapon = GetWeapon(combatant);
            return weapon != null && IsRangedWeapon(weapon.Type);
        }

        /// <summary>
        /// The damage multiplier for this combatant's plain attack from the back row:
        /// 1 for ranged, 0.9 for a spear, 0.7 for any other melee.
        /// </summary>
        public static double BackRowDamageMultiplier(Entity attacker)
        {
            if (AttacksAtRange(attacker)) return 1.0;

            var weapon = GetWeapon(attacker);
            return weapon != null && weapon.Type == WeaponType.Spear
                ? BackRowSpearDamageMultiplier
                : BackRowMeleeDamageMultiplier;
        }

        /// <summary>
        /// The first empty slot for a new member: ranged users try the back row first (slots
        /// 2, 3, then 0, 1), everyone else goes in order (0, 1, 2, 3). NoSlot if all are taken.
        /// </summary>
        public static int FirstFreeSlot(IEnumerable<int> takenSlots, bool ranged)
        {
            var taken = new HashSet<int>(takenSlots ?? Enumerable.Empty<int>());
            var order = ranged ? new[] { 2, 3, 0, 1 } : new[] { 0, 1, 2, 3 };
            foreach (int slot in order)
            {
                if (!taken.Contains(slot)) return slot;
            }
            return NoSlot;
        }

        /// <summary>
        /// Give every member a slot of their own: valid slots that nobody else holds are kept
        /// (first come, in list order), and anyone without one gets the first free slot
        /// (FirstFreeSlot). Returns each member's slot - the members themselves aren't changed.
        /// </summary>
        public static Dictionary<Entity, int> Arrange(IEnumerable<Entity> members, System.Func<Entity, int> currentSlot)
        {
            var slots = new Dictionary<Entity, int>();
            var list = members.Where(m => m != null).Distinct().ToList();

            foreach (var member in list)
            {
                int slot = currentSlot(member);
                if (IsValidSlot(slot) && !slots.ContainsValue(slot))
                {
                    slots[member] = slot;
                }
            }

            foreach (var member in list)
            {
                if (slots.ContainsKey(member)) continue;
                slots[member] = FirstFreeSlot(slots.Values, AttacksAtRange(member));
            }

            return slots;
        }

        /// <summary>Bosses ignore rows: never in the back row, and their attacks ignore it.</summary>
        public static bool IgnoresRows(Entity combatant) => combatant is BossEnemy;
    }
}
