using GameLogic.Entities;
using GameLogic.Systems;

namespace GameLogic.Abilities
{
    /// <summary>
    /// Executioner - Passive ability that modifies critical hit mechanics
    /// Level 1: -10% crit chance, 1.1x crit multiplier (drawback)
    /// Level 100: +10% crit chance, 3.0x crit multiplier (powerful)
    /// Linear scaling between levels - transitions from drawback to advantage
    /// Affects PLAYER ONLY, not companions
    /// </summary>
    public class ExecutionerAbility : Ability
    {
        public ExecutionerAbility()
        {
            Name = "Executioner";
            Description = "You've mastered the art of devastating critical strikes. Your critical hits deal massive damage, though perfecting this technique initially reduces your crit frequency.";
            IsPassive = true;
            Rarity = AbilityRarity.Legendary;

            // Passive abilities don't have cooldown or targeting
            Cooldown = 0;
            TargetType = AbilityTarget.Self;
        }

        /// <summary>
        /// Get the critical hit chance modifier based on level
        /// Scales from -10% to +10% linearly
        /// Negative at low levels (drawback), positive at high levels (bonus)
        /// </summary>
        public int GetCritChanceModifier()
        {
            // Linear scaling: -10% at level 1, +10% at level 100
            return GetScaledValueInt(-10, 10);
        }

        /// <summary>
        /// Get the critical hit damage multiplier based on level
        /// Scales from 1.1x to 3.0x linearly
        /// Default game crit is 1.5x, this replaces it
        /// </summary>
        public double GetCritMultiplier()
        {
            // Linear scaling: 1.1x at level 1, 3.0x at level 100
            double minMultiplier = 1.1;
            double maxMultiplier = 3.0;
            return GetScaledValue(minMultiplier, maxMultiplier);
        }

        /// <summary>
        /// Get the passive bonus value for display purposes
        /// Returns the crit chance modifier percentage
        /// </summary>
        public override int GetPassiveBonusValue()
        {
            return GetCritChanceModifier();
        }

        /// <summary>
        /// Called when Executioner levels up - notify about improvements
        /// </summary>
        protected override void OnLevelUp()
        {
            int critChanceMod = GetCritChanceModifier();
            double critMultiplier = GetCritMultiplier();

            // Notify every 10 levels about progress
            if (Level % 10 == 0)
            {
                string chanceText = critChanceMod >= 0 ? $"+{critChanceMod}%" : $"{critChanceMod}%";
                Console.WriteLine($"Your execution technique improves! Crit chance: {chanceText}, Crit multiplier: {critMultiplier:F2}x!");
            }

            // Special message when transitioning from penalty to bonus (around level 51)
            if (Level == 51)
            {
                Console.WriteLine("You've mastered the basics! Your critical hit chance is no longer penalized!");
            }
        }

        /// <summary>
        /// Get detailed info about Executioner ability
        /// </summary>
        public override string GetInfo()
        {
            int critChanceMod = GetCritChanceModifier();
            double critMultiplier = GetCritMultiplier();
            int nextMilestone = ((Level / 10) + 1) * 10;
            if (nextMilestone > 100) nextMilestone = 100;

            string chanceText = critChanceMod >= 0 ? $"+{critChanceMod}%" : $"{critChanceMod}%";
            string chanceDescription = critChanceMod >= 0 ? "Bonus" : "Penalty";

            string info = $"{Name} (Lv.{Level}/{MaxLevel}) [PASSIVE]\n" +
                         $"{Description}\n\n" +
                         $"Current Critical Hit Chance: {chanceText} ({chanceDescription})\n" +
                         $"Current Critical Multiplier: {critMultiplier:F2}x (Default: 1.5x)\n" +
                         $"Affects: Player only\n" +
                         $"Does NOT affect: Companions";

            if (Level < 100)
            {
                // Calculate values at next milestone
                Ability tempAbility = new ExecutionerAbility();
                tempAbility.Level = nextMilestone;
                int milestoneCritMod = ((ExecutionerAbility)tempAbility).GetCritChanceModifier();
                double milestoneCritMult = ((ExecutionerAbility)tempAbility).GetCritMultiplier();
                string milestoneChanceText = milestoneCritMod >= 0 ? $"+{milestoneCritMod}%" : $"{milestoneCritMod}%";

                info += $"\n\nNext Milestone: Level {nextMilestone}\n" +
                       $"  Crit chance: {milestoneChanceText}\n" +
                       $"  Crit multiplier: {milestoneCritMult:F2}x";
            }
            else
            {
                info += $"\n\nMax level reached! Critical strikes at maximum power!";
            }

            // Show example damage values
            info += $"\n\nExample Damage (base 50):";
            info += $"\n  Normal hit: 50 damage";
            info += $"\n  Critical hit: {(int)(50 * critMultiplier)} damage ({critMultiplier:F2}x)";
            info += $"\n  (Default crit would be: 75 damage [1.5x])";

            info += $"\n\nXP: {Experience}/{ExperienceToNextLevel}";

            return info;
        }

        /// <summary>
        /// Passive abilities can't be executed in combat
        /// </summary>
        public override void Execute(Entity user, Entity target, RNGManager rng)
        {
            int critChanceMod = GetCritChanceModifier();
            double critMultiplier = GetCritMultiplier();
            string chanceText = critChanceMod >= 0 ? $"+{critChanceMod}%" : $"{critChanceMod}%";

            Console.WriteLine($"{Name} is a passive ability modifying critical hit mechanics.");
            Console.WriteLine($"Current effects: {chanceText} crit chance, {critMultiplier:F2}x crit multiplier");
            Console.WriteLine("It is always active and cannot be used in combat.");
        }
    }
}