using GameLogic.Entities;
using GameLogic.Systems;

namespace GameLogic.Abilities.EnemyAbilities
{
    /// <summary>
    /// The Giant House Centipede's poison (boss #3). Built by GiantCentipedeMechanic on a bite.
    ///
    /// - Damage per turn comes from his level (PoisonAttack.GetPoisonStatsForLevel), times the
    ///   number of stacks (my reading of "poison stacks").
    /// - A new bite on someone already poisoned by him adds a stack, but only adds the shortest
    ///   duration of his difficulty's band to the time left - it doesn't go back to a full roll
    ///   (decided; his poison only for now - section 2 asks whether Burn and Bleed should
    ///   work the same way).
    /// - It is a different effect from an ordinary PoisonedEffect, so the two don't merge.
    /// </summary>
    public class CentipedePoisonEffect : PoisonedEffect
    {
        /// <summary>Turns a restack adds (the difficulty band's minimum).</summary>
        public int RestackTurns { get; }

        public CentipedePoisonEffect(int duration, int minDamage, int maxDamage, int restackTurns)
            : base(duration, minDamage, maxDamage)
        {
            RestackTurns = restackTurns;
        }

        public override void ApplyEffect(Entity target, RNGManager rng)
        {
            int damage = rng.Roll(MinDamage, MaxDamage) * StackCount;
            target.TakeDamage(damage);

            GameLog.Write($"  💀 {Name}: {target.Name} takes {damage} poison damage! ({Duration} turns left)");
        }

        /// <summary>
        /// A restack: one more stack, and RestackTurns more time - not a full refresh.
        /// </summary>
        public override void OnReapplied(AbilityEffect incoming, Entity target)
        {
            AddStack();
            Duration += RestackTurns;
            GameLog.Write($"☠ The poison deepens on {target.Name}! (x{StackCount}, {Duration} turns left)");
        }

        public override string GetDisplayInfo()
        {
            string stackInfo = StackCount > 1 ? $" x{StackCount}" : "";
            return $"{Name}{stackInfo} ({MinDamage * StackCount}-{MaxDamage * StackCount} dmg, {Duration} turns)";
        }
    }
}
