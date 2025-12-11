using GameLogic.Entities;
using GameLogic.Systems;

namespace GameLogic.Abilities
{
    /// <summary>
    /// Iron Will - Passive ability that gives chance to cleanse negative status effects
    /// Level 1: 1% chance to cleanse at end of each round
    /// Level 100: 25% chance to cleanse at end of each round
    /// Linear scaling between levels
    /// Affects PLAYER ONLY, not companions
    /// Removes: Bleeding, Poison, Burns, Debuffs, Crowd Control
    /// </summary>
    public class IronWillAbility : Ability
    {
        public IronWillAbility()
        {
            Name = "Iron Will";
            Description = "Your indomitable willpower allows you to shrug off debilitating effects. At the end of each combat round, you have a chance to cleanse all negative status effects.";
            IsPassive = true;
            Rarity = AbilityRarity.Epic;

            // Passive abilities don't have cooldown or targeting
            Cooldown = 0;
            TargetType = AbilityTarget.Self;
        }

        /// <summary>
        /// Get the cleanse chance percentage based on level
        /// Scales from 1% to 25% linearly
        /// </summary>
        public override int GetPassiveBonusValue()
        {
            // Linear scaling: 1% at level 1, 25% at level 100
            return GetScaledValueInt(1, 25);
        }

        /// <summary>
        /// Get the cleanse chance as a decimal for probability checks
        /// E.g., 10% returns 0.10
        /// </summary>
        public double GetCleanseChance()
        {
            int chancePercent = GetPassiveBonusValue();
            return chancePercent / 100.0;
        }

        /// <summary>
        /// Attempt to cleanse negative effects from the player
        /// Returns true if cleanse was triggered, false otherwise
        /// </summary>
        public bool TryCleanseEffects(Entity player, RNGManager rng)
        {
            int cleanseChance = GetPassiveBonusValue();
            int roll = rng.Roll(1, 100);

            if (roll <= cleanseChance)
            {
                // Cleanse succeeded!
                player.RemoveNegativeEffects();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Called when Iron Will levels up - notify about improvement
        /// </summary>
        protected override void OnLevelUp()
        {
            int newChance = GetPassiveBonusValue();

            // Notify every 5 levels about progress
            if (Level % 5 == 0)
            {
                Console.WriteLine($"Your willpower strengthens! Cleanse chance is now {newChance}%!");
            }
        }

        /// <summary>
        /// Get detailed info about Iron Will ability
        /// </summary>
        public override string GetInfo()
        {
            int cleanseChance = GetPassiveBonusValue();
            int nextMilestone = ((Level / 5) + 1) * 5;
            if (nextMilestone > 100) nextMilestone = 100;

            string info = $"{Name} (Lv.{Level}/{MaxLevel}) [PASSIVE]\n" +
                         $"{Description}\n\n" +
                         $"Current Cleanse Chance: {cleanseChance}% (per round)\n" +
                         $"Triggers: At the end of each combat round\n" +
                         $"Affects: Player only\n" +
                         $"Removes: Bleeding, Poison, Burns, Debuffs, Crowd Control";

            if (Level < 100)
            {
                // Calculate chance at next milestone
                Ability tempAbility = new IronWillAbility();
                tempAbility.Level = nextMilestone;
                int milestoneChance = tempAbility.GetPassiveBonusValue();

                info += $"\n\nNext Milestone: Level {nextMilestone}\n" +
                       $"  Cleanse chance will be {milestoneChance}%";
            }
            else
            {
                info += $"\n\nMax level reached! Cleanse chance at maximum ({cleanseChance}%).";
            }

            // Show probability breakdown
            info += $"\n\nProbability of Cleansing:";
            info += $"\n  Within 1 round: {cleanseChance}%";

            // Calculate probability of cleansing within multiple rounds
            double failChance = 1.0 - (cleanseChance / 100.0);
            double within3Rounds = (1.0 - Math.Pow(failChance, 3)) * 100.0;
            double within5Rounds = (1.0 - Math.Pow(failChance, 5)) * 100.0;

            info += $"\n  Within 3 rounds: {within3Rounds:F1}%";
            info += $"\n  Within 5 rounds: {within5Rounds:F1}%";

            info += $"\n\nXP: {Experience}/{ExperienceToNextLevel}";

            return info;
        }

        /// <summary>
        /// Passive abilities can't be executed in combat
        /// </summary>
        public override void Execute(Entity user, Entity target, RNGManager rng)
        {
            Console.WriteLine($"{Name} is a passive ability providing a {GetPassiveBonusValue()}% chance to cleanse negative effects each round.");
            Console.WriteLine("It is always active and cannot be manually used.");
        }
    }
}