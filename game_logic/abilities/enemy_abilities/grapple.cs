using GameLogic.Entities;
using GameLogic.Systems;

namespace GameLogic.Abilities.EnemyAbilities
{
    /// <summary>
    /// Grappled status effect - reduces the target's effective speed for its duration.
    /// No recurring damage; the initial hit is dealt by the attack that applies this
    /// effect (see CombatManager.ProcessAttack's AppliesEffect handling). Speed reduction
    /// itself is read directly off this effect's Potency by
    /// CombatManager.GetEntityEffectiveSpeed, independent of the per-round SpeedModifier
    /// system (which would otherwise overwrite a multi-round slow every round).
    /// </summary>
    public class GrappledEffect : AbilityEffect
    {
        public GrappledEffect(int duration, int speedReduction)
            : base(duration, speedReduction) // Potency = speedReduction
        {
            Name = "Grappled";
            Description = "Speed reduced while held in a crushing grip.";
            Type = EffectType.CrowdControl;
            CanStack = false;
        }

        public override void ApplyEffect(Entity target, RNGManager rng)
        {
            // No recurring damage - purely a speed debuff for its duration.
        }
    }
}
