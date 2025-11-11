using GameLogic.Entities;
using GameLogic.Systems;

namespace GameLogic.Abilities.PlayerAbilities
{
    /// <summary>
    /// Critical Strike - Increases critical hit chance
    /// Scalable: Level 1 = +1% crit chance, Level 100 = +50% crit chance
    /// Applies a temporary buff effect
    /// </summary>
    public class CriticalStrike : Ability
    {
        public CriticalStrike()
        {
            Name = "Critical Strike";
            Description = "Sharpen your focus, increasing critical hit chance";
            TargetType = AbilityTarget.Self;
            Rarity = AbilityRarity.Uncommon;

            ManaCost = 0;  // Free to use
            Cooldown = 4;  // 4 turn cooldown after use
        }

        /// <summary>
        /// Execute the ability - applies CriticalStrikeEffect to the user
        /// </summary>
        public override void Execute(Entity user, Entity target, RNGManager rng)
        {
            Console.WriteLine($"\n{user.Name} uses {Name}!");

            // Get scaled crit chance percentage based on ability level
            double critChanceBonus = GetScaledValue(1.0, 50.0);  // 1% to 50%

            // Create and apply the effect
            var effect = new CriticalStrikeEffect(
                duration: 3,  // Lasts 3 turns
                potency: (int)critChanceBonus  // Store as integer percentage
            );

            user.AddEffect(effect);

            Console.WriteLine($"Critical hit chance increased by {critChanceBonus:F1}% for 3 turns!");

            // Set cooldown
            CurrentCooldown = Cooldown;

            // Gain experience for using the ability
            GainExperience(10);
        }

        /// <summary>
        /// Get the current critical chance bonus (for external calculations)
        /// </summary>
        public double GetCriticalChance()
        {
            return GetScaledValue(1.0, 50.0);
        }

        /// <summary>
        /// Get info including current crit chance
        /// </summary>
        public override string GetInfo()
        {
            double critChance = GetScaledValue(1.0, 50.0);
            string cooldownInfo = CurrentCooldown > 0 ? $" (Cooldown: {CurrentCooldown})" : "";

            return $"{Name} (Lv.{Level}/{MaxLevel}){cooldownInfo}\n" +
                   $"{Description}\n" +
                   $"Current Bonus: +{critChance:F1}% crit chance for 3 turns";
        }

        /// <summary>
        /// Called when ability levels up
        /// </summary>
        protected override void OnLevelUp()
        {
            Console.WriteLine($"Critical strike chance increased to +{GetScaledValue(1.0, 50.0):F1}%!");
        }
    }

    /// <summary>
    /// Status effect applied by Critical Strike ability
    /// </summary>
    public class CriticalStrikeEffect : AbilityEffect
    {
        public CriticalStrikeEffect(int duration, int potency) : base(duration, potency)
        {
            Name = "Critical Strike";
            Description = $"Critical hit chance increased by {potency}%";
            Type = EffectType.Buff;
            CanStack = false;  // Cannot stack multiple crit buffs
        }

        /// <summary>
        /// Apply effect each turn (this is passive, no per-turn action)
        /// The crit bonus is applied during damage calculation
        /// </summary>
        public override void ApplyEffect(Entity target, RNGManager rng)
        {
            // This effect is passive - the bonus is checked during damage calculation
            // No per-turn action needed
            Console.WriteLine($"  {Name}: +{Potency}% crit chance ({Duration} turns left)");
        }

        /// <summary>
        /// Get the critical chance bonus from this effect
        /// </summary>
        public int GetCritChanceBonus()
        {
            return Potency;
        }

        public override void OnApplied(Entity target)
        {
            Console.WriteLine($"⚡ {target.Name}'s strikes are deadly precise!");
        }

        public override void OnExpired(Entity target)
        {
            Console.WriteLine($"⚡ {target.Name}'s critical strike focus faded.");
        }
    }
}