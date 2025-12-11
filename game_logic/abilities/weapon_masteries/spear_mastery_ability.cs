using GameLogic.Entities;
using GameLogic.Systems;

namespace GameLogic.Abilities.WeaponMasteries
{
    /// <summary>
    /// Spear Mastery - Passive ability that provides bonuses when wielding a spear
    /// Level 1: +1% accuracy, +1% attack damage, +1% crit chance
    /// Level 100: +40% accuracy, +70% attack damage, +25% crit chance
    /// Linear scaling between levels
    /// Only active when wielding a Spear weapon
    /// </summary>
    public class SpearMasteryAbility : Ability
    {
        public SpearMasteryAbility()
        {
            Name = "Spear Mastery";
            Description = "Your extensive training with spears grants you enhanced accuracy, damage, and critical strike potential when wielding a spear.";
            IsPassive = true;
            Rarity = AbilityRarity.Rare;

            // Passive abilities don't have cooldown or targeting
            Cooldown = 0;
            TargetType = AbilityTarget.Self;
        }

        /// <summary>
        /// Get the accuracy bonus percentage based on level
        /// Scales from 1% to 40% linearly
        /// </summary>
        public int GetAccuracyBonus()
        {
            return GetScaledValueInt(1, 40);
        }

        /// <summary>
        /// Get the attack damage bonus percentage based on level
        /// Scales from 1% to 70% linearly
        /// </summary>
        public int GetAttackDamageBonus()
        {
            return GetScaledValueInt(1, 70);
        }

        /// <summary>
        /// Get the critical hit chance bonus percentage based on level
        /// Scales from 1% to 25% linearly
        /// </summary>
        public int GetCritChanceBonus()
        {
            return GetScaledValueInt(1, 25);
        }

        /// <summary>
        /// Get the accuracy multiplier (for applying to base accuracy)
        /// E.g., 10% returns 0.10
        /// </summary>
        public double GetAccuracyMultiplier()
        {
            return GetAccuracyBonus() / 100.0;
        }

        /// <summary>
        /// Get the damage multiplier (for applying to base damage)
        /// E.g., 50% returns 1.50
        /// </summary>
        public double GetDamageMultiplier()
        {
            int bonusPercent = GetAttackDamageBonus();
            return 1.0 + (bonusPercent / 100.0);
        }

        /// <summary>
        /// Check if player is wielding a spear (required for bonuses to apply)
        /// </summary>
        public bool IsWieldingCorrectWeapon(Entity entity)
        {
            if (entity is Entities.Player.Player player)
            {
                return player.EquippedWeapon != null &&
                       player.EquippedWeapon.Type == Items.WeaponType.Spear;
            }
            return false;
        }

        /// <summary>
        /// Called when Spear Mastery levels up - notify about improvement
        /// </summary>
        protected override void OnLevelUp()
        {
            int accuracyBonus = GetAccuracyBonus();
            int damageBonus = GetAttackDamageBonus();
            int critBonus = GetCritChanceBonus();

            // Notify every 10 levels about progress
            if (Level % 10 == 0)
            {
                Console.WriteLine($"Your spear mastery improves! Now: +{accuracyBonus}% accuracy, +{damageBonus}% damage, +{critBonus}% crit chance!");
            }
        }

        /// <summary>
        /// Get detailed info about Spear Mastery ability
        /// </summary>
        public override string GetInfo()
        {
            int accuracyBonus = GetAccuracyBonus();
            int damageBonus = GetAttackDamageBonus();
            int critBonus = GetCritChanceBonus();

            string info = $"{Name} (Lv.{Level}/{MaxLevel}) [PASSIVE]\\n" +
                         $"{Description}\\n\\n" +
                         $"Current Bonuses (when wielding a Spear):\\n" +
                         $"  Accuracy: +{accuracyBonus}%\\n" +
                         $"  Attack Damage: +{damageBonus}%\\n" +
                         $"  Critical Hit Chance: +{critBonus}%\\n";

            if (Level < 100)
            {
                int nextMilestone = ((Level / 10) + 1) * 10;
                if (nextMilestone > 100) nextMilestone = 100;

                // Calculate bonuses at next milestone
                Ability tempAbility = new SpearMasteryAbility();
                tempAbility.Level = nextMilestone;
                int milestoneAccuracy = ((SpearMasteryAbility)tempAbility).GetAccuracyBonus();
                int milestoneDamage = ((SpearMasteryAbility)tempAbility).GetAttackDamageBonus();
                int milestoneCrit = ((SpearMasteryAbility)tempAbility).GetCritChanceBonus();

                info += $"\\nNext Milestone: Level {nextMilestone}\\n" +
                       $"  Accuracy: +{milestoneAccuracy}%\\n" +
                       $"  Damage: +{milestoneDamage}%\\n" +
                       $"  Crit Chance: +{milestoneCrit}%";
            }
            else
            {
                info += $"\\nMax level reached! All bonuses at maximum.";
            }

            info += $"\\n\\nXP: {Experience}/{ExperienceToNextLevel}";

            return info;
        }

        /// <summary>
        /// Passive abilities can't be executed in combat
        /// </summary>
        public override void Execute(Entity user, Entity target, RNGManager rng)
        {
            int accuracyBonus = GetAccuracyBonus();
            int damageBonus = GetAttackDamageBonus();
            int critBonus = GetCritChanceBonus();

            Console.WriteLine($"{Name} is a passive ability providing bonuses when wielding a spear.");
            Console.WriteLine($"Current bonuses: +{accuracyBonus}% accuracy, +{damageBonus}% damage, +{critBonus}% crit chance");
            Console.WriteLine("It is always active and cannot be manually used.");
        }
    }
}