using System;
using GameLogic.Entities;
using GameLogic.Systems;

namespace GameLogic.Abilities.PlayerAbilities
{
    /// <summary>
    /// Healing Ability - Restores health to the user
    /// Scalable: Level 1 = 10-20 HP, Level 100 = 100-150 HP
    /// Uses RNG to determine exact healing amount within range
    /// </summary>
    public class HealingAbility : Ability
    {
        public HealingAbility()
        {
            Name = "Heal";
            Description = "Restore health with a burst of healing energy";
            TargetType = AbilityTarget.Self;
            Rarity = AbilityRarity.Common;

            Cooldown = 4;  // 4 turn cooldown
        }

        /// <summary>
        /// Execute the ability - heals the user
        /// </summary>
        public override void Execute(Entity user, Entity target, RNGManager rng)
        {
            Console.WriteLine($"\n{user.Name} uses {Name}!");

            // Get scaled healing range based on ability level
            int minHeal = GetScaledValueInt(10, 100);   // 10 HP at lv1, 100 HP at lv100
            int maxHeal = GetScaledValueInt(20, 150);   // 20 HP at lv1, 150 HP at lv100

            // RNG determines actual healing within range
            int healAmount = rng.Roll(minHeal, maxHeal);

            // Store health before healing
            int healthBefore = user.Health;

            // Apply healing
            user.Heal(healAmount);

            // Calculate actual healing (in case max HP limit)
            int actualHealing = user.Health - healthBefore;

            Console.WriteLine($"Restored {actualHealing} HP! ({user.Health}/{user.MaxHealth})");

            if (actualHealing < healAmount)
            {
                Console.WriteLine($"(Healed to full health!)");
            }

            // Set cooldown (uses level-based reduction)
            CurrentCooldown = GetEffectiveCooldown();

            // Gain scaled combat experience
            GainCombatExperience();
        }

        /// <summary>
        /// Get the current healing range (for display/info)
        /// </summary>
        public (int min, int max) GetHealingRange()
        {
            int minHeal = GetScaledValueInt(10, 100);
            int maxHeal = GetScaledValueInt(20, 150);
            return (minHeal, maxHeal);
        }

        /// <summary>
        /// Get info including current healing range
        /// </summary>
        public override string GetInfo()
        {
            var (min, max) = GetHealingRange();
            string cooldownInfo = CurrentCooldown > 0 ? $" (Cooldown: {CurrentCooldown})" : "";

            return $"{Name} (Lv.{Level}/{MaxLevel}){cooldownInfo}\n" +
                   $"{Description}\n" +
                   $"Heals: {min}-{max} HP";
        }

        /// <summary>
        /// Called when ability levels up
        /// </summary>
        protected override void OnLevelUp()
        {
            var (min, max) = GetHealingRange();
            Console.WriteLine($"Healing power increased to {min}-{max} HP!");
        }
    }
}