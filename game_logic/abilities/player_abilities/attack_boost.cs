using System;
using GameLogic.Entities;
using GameLogic.Systems;

namespace GameLogic.Abilities.PlayerAbilities
{
    /// <summary>
    /// Attack Boost - Increases player's attack damage
    /// Scalable: Level 1 = +1% damage, Level 100 = +70% damage
    /// Applies a temporary buff effect
    /// </summary>
    public class AttackBoost : Ability
    {
        public AttackBoost()
        {
            Name = "Attack Boost";
            Description = "Temporarily increases your attack damage";
            TargetType = AbilityTarget.Self;
            Rarity = AbilityRarity.Common;

            Cooldown = 5;  // 5 turn cooldown after use
        }

        /// <summary>
        /// Execute the ability - applies AttackBoostEffect to the user
        /// </summary>
        public override void Execute(Entity user, Entity target, RNGManager rng)
        {
            Console.WriteLine($"\n{user.Name} uses {Name}!");

            // Get scaled boost percentage based on ability level
            double boostPercentage = GetScaledValue(1.0, 70.0);  // 1% to 70%

            // Create and apply the effect
            var effect = new AttackBoostEffect(
                duration: 3,  // Lasts 3 turns
                potency: (int)boostPercentage  // Store as integer percentage
            );

            user.AddEffect(effect);

            Console.WriteLine($"Attack damage increased by {boostPercentage:F1}% for 3 turns!");

            // Set cooldown (uses level-based reduction)
            CurrentCooldown = GetEffectiveCooldown();

            // Gain scaled combat experience
            GainCombatExperience();
        }

        /// <summary>
        /// Get the current boost multiplier (for external calculations)
        /// </summary>
        public double GetAttackMultiplier()
        {
            double percentage = GetScaledValue(1.0, 70.0);
            return 1.0 + (percentage / 100.0);  // e.g., 1.01 to 1.70
        }

        /// <summary>
        /// Get info including current boost amount
        /// </summary>
        public override string GetInfo()
        {
            double boostPercentage = GetScaledValue(1.0, 70.0);
            string cooldownInfo = CurrentCooldown > 0 ? $" (Cooldown: {CurrentCooldown})" : "";

            return $"{Name} (Lv.{Level}/{MaxLevel}){cooldownInfo}\n" +
                   $"{Description}\n" +
                   $"Current Boost: +{boostPercentage:F1}% damage for 3 turns";
        }

        /// <summary>
        /// Called when ability levels up
        /// </summary>
        protected override void OnLevelUp()
        {
            Console.WriteLine($"Attack boost increased to +{GetScaledValue(1.0, 70.0):F1}%!");
        }
    }

    /// <summary>
    /// Status effect applied by Attack Boost ability
    /// </summary>
    public class AttackBoostEffect : AbilityEffect
    {
        public AttackBoostEffect(int duration, int potency) : base(duration, potency)
        {
            Name = "Attack Boost";
            Description = $"Attack damage increased by {potency}%";
            Type = EffectType.Buff;
            CanStack = false;  // Cannot stack multiple attack boosts
        }

        /// <summary>
        /// Apply effect each turn (this is passive, no per-turn action)
        /// The damage boost is applied during damage calculation
        /// </summary>
        public override void ApplyEffect(Entity target, RNGManager rng)
        {
            // This effect is passive - the boost is checked during damage calculation
            // No per-turn action needed
            Console.WriteLine($"  {Name}: +{Potency}% damage ({Duration} turns left)");
        }

        /// <summary>
        /// Get the damage multiplier for this effect
        /// </summary>
        public double GetDamageMultiplier()
        {
            return 1.0 + (Potency / 100.0);
        }

        public override void OnApplied(Entity target)
        {
            Console.WriteLine($"⚔ {target.Name}'s attacks are empowered!");
        }

        public override void OnExpired(Entity target)
        {
            Console.WriteLine($"⚔ {target.Name}'s attack boost faded.");
        }
    }
}