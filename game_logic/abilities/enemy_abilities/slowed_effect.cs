using GameLogic.Entities;
using GameLogic.Systems;

namespace GameLogic.Abilities.EnemyAbilities
{
    /// <summary>
    /// Slowed - speed is lowered for a few turns (a slime's sticky hit, decided 2026-11-23:
    /// -2 speed for 2 turns). Like Grappled and Pinned, the speed loss is read off Potency by
    /// CombatManager.GetEntityEffectiveSpeed, so the per-round SpeedModifier doesn't wipe it.
    /// Landing it again refreshes it rather than stacking.
    /// </summary>
    public class SlowedEffect : AbilityEffect
    {
        public SlowedEffect(int duration, int speedReduction)
            : base(duration, speedReduction) // Potency = speedReduction
        {
            Name = "Slowed";
            Description = "Speed reduced - stuck in something sticky.";
            Type = EffectType.CrowdControl;
            CanStack = false;
        }

        public override void ApplyEffect(Entity target, RNGManager rng)
        {
            // No recurring damage - purely a speed debuff for its duration.
        }

        public override void OnApplied(Entity target)
        {
            GameLog.Write($"{target.Name} is slowed!");
        }

        public override void OnExpired(Entity target)
        {
            GameLog.Write($"{target.Name} is no longer slowed.");
        }
    }
}
