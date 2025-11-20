using GameLogic.Entities;
using GameLogic.Systems;

namespace GameLogic.Abilities.PlayerAbilities
{
    /// <summary>
    /// Defense Boost - Reduces incoming damage
    /// Scalable: Level 1 = -1% damage taken, Level 100 = -60% damage taken
    /// Applies a temporary defensive buff effect
    /// </summary>
    public class DefenseBoost : Ability
    {
        public DefenseBoost()
        {
            Name = "Defense Boost";
            Description = "Harden your defenses, reducing incoming damage";
            TargetType = AbilityTarget.Self;
            Rarity = AbilityRarity.Common;

            Cooldown = 5;  // 5 turn cooldown after use
        }

        /// <summary>
        /// Execute the ability - applies DefenseBoostEffect to the user
        /// </summary>
        public override void Execute(Entity user, Entity target, RNGManager rng)
        {
            Console.WriteLine($"\n{user.Name} uses {Name}!");

            // Get scaled defense percentage based on ability level
            double defensePercentage = GetScaledValue(1.0, 60.0);  // 1% to 60%

            // Create and apply the effect
            var effect = new DefenseBoostEffect(
                duration: 3,  // Lasts 3 turns
                potency: (int)defensePercentage  // Store as integer percentage
            );

            user.AddEffect(effect);

            Console.WriteLine($"Damage taken reduced by {defensePercentage:F1}% for 3 turns!");

            // Set cooldown (uses level-based reduction)
            CurrentCooldown = GetEffectiveCooldown();

            // Gain scaled combat experience
            GainCombatExperience();
        }

        /// <summary>
        /// Get the current defense reduction percentage (for external calculations)
        /// </summary>
        public double GetDefenseReduction()
        {
            return GetScaledValue(1.0, 60.0);
        }

        /// <summary>
        /// Get the damage multiplier (for damage calculations)
        /// At 1%: multiplier = 0.99 (take 99% damage)
        /// At 60%: multiplier = 0.40 (take 40% damage)
        /// </summary>
        public double GetDamageMultiplier()
        {
            double reduction = GetScaledValue(1.0, 60.0);
            return 1.0 - (reduction / 100.0);  // e.g., 0.99 to 0.40
        }

        /// <summary>
        /// Get info including current defense amount
        /// </summary>
        public override string GetInfo()
        {
            double defensePercentage = GetScaledValue(1.0, 60.0);
            string cooldownInfo = CurrentCooldown > 0 ? $" (Cooldown: {CurrentCooldown})" : "";

            return $"{Name} (Lv.{Level}/{MaxLevel}){cooldownInfo}\n" +
                   $"{Description}\n" +
                   $"Current Reduction: -{defensePercentage:F1}% damage taken for 3 turns";
        }

        /// <summary>
        /// Called when ability levels up
        /// </summary>
        protected override void OnLevelUp()
        {
            Console.WriteLine($"Defense reduction increased to -{GetScaledValue(1.0, 60.0):F1}%!");
        }
    }

    /// <summary>
    /// Status effect applied by Defense Boost ability
    /// </summary>
    public class DefenseBoostEffect : AbilityEffect
    {
        public DefenseBoostEffect(int duration, int potency) : base(duration, potency)
        {
            Name = "Defense Boost";
            Description = $"Damage taken reduced by {potency}%";
            Type = EffectType.Buff;
            CanStack = false;  // Cannot stack multiple defense boosts
        }

        /// <summary>
        /// Apply effect each turn (this is passive, no per-turn action)
        /// The damage reduction is applied during damage calculation
        /// </summary>
        public override void ApplyEffect(Entity target, RNGManager rng)
        {
            // This effect is passive - the reduction is checked during damage calculation
            // No per-turn action needed
            Console.WriteLine($"  {Name}: -{Potency}% damage taken ({Duration} turns left)");
        }

        /// <summary>
        /// Get the damage multiplier for this effect
        /// If potency is 20%, multiplier is 0.80 (take 80% damage)
        /// </summary>
        public double GetDamageMultiplier()
        {
            return 1.0 - (Potency / 100.0);
        }

        public override void OnApplied(Entity target)
        {
            Console.WriteLine($"🛡 {target.Name} is fortified with defensive energy!");
        }

        public override void OnExpired(Entity target)
        {
            Console.WriteLine($"🛡 {target.Name}'s defense boost faded.");
        }
    }
}