using GameLogic.Entities;
using GameLogic.Systems;

namespace GameLogic.Abilities
{
    /// <summary>
    /// Precision Training - Passive ability that increases accuracy for all allies
    /// Level 1: +1% accuracy for all allies (including player)
    /// Level 100: +30% accuracy for all allies (including player)
    /// Linear scaling between levels
    /// Affects both player and companions
    /// </summary>
    public class PrecisionTrainingAbility : Ability
    {
        public PrecisionTrainingAbility()
        {
            Name = "Precision Training";
            Description = "Your expertise in combat techniques improves the accuracy of you and your allies, making attacks more likely to hit.";
            IsPassive = true;
            Rarity = AbilityRarity.Epic;

            // Passive abilities don't have cooldown or targeting
            Cooldown = 0;
            TargetType = AbilityTarget.AllAllies;
        }

        /// <summary>
        /// Get the accuracy bonus percentage based on level
        /// Scales from 1% to 30% linearly
        /// </summary>
        public override int GetPassiveBonusValue()
        {
            // Linear scaling: 1% at level 1, 30% at level 100
            return GetScaledValueInt(1, 30);
        }

        /// <summary>
        /// Get the accuracy bonus as a decimal for hit chance calculations
        /// E.g., 10% accuracy returns 0.10 to add to hit chance
        /// </summary>
        public double GetAccuracyBonus()
        {
            int bonusPercent = GetPassiveBonusValue();
            return bonusPercent / 100.0;
        }

        /// <summary>
        /// Called when Precision Training levels up - notify about accuracy increase
        /// </summary>
        protected override void OnLevelUp()
        {
            int newBonus = GetPassiveBonusValue();

            // Notify every 5 levels about progress (since gains are smaller)
            if (Level % 5 == 0)
            {
                Console.WriteLine($"Your precision training improves! Ally accuracy bonus is now {newBonus}%!");
            }
        }

        /// <summary>
        /// Get detailed info about Precision Training ability
        /// </summary>
        public override string GetInfo()
        {
            int bonus = GetPassiveBonusValue();
            int nextMilestone = ((Level / 5) + 1) * 5;
            if (nextMilestone > 100) nextMilestone = 100;

            string info = $"{Name} (Lv.{Level}/{MaxLevel}) [PASSIVE]\n" +
                         $"{Description}\n\n" +
                         $"Current Accuracy Bonus: +{bonus}%\n" +
                         $"Affects: You and all companions in your party\n" +
                         $"Effect: Increases hit chance for all attacks";

            if (Level < 100)
            {
                // Calculate bonus at next milestone
                Ability tempAbility = new PrecisionTrainingAbility();
                tempAbility.Level = nextMilestone;
                int milestoneBonus = tempAbility.GetPassiveBonusValue();

                info += $"\n\nNext Milestone: Level {nextMilestone}\n" +
                       $"  Accuracy bonus will be {milestoneBonus}%";
            }
            else
            {
                info += $"\n\nMax level reached! Accuracy bonus at maximum ({bonus}%).";
            }

            info += $"\n\nXP: {Experience}/{ExperienceToNextLevel}";

            return info;
        }

        /// <summary>
        /// Passive abilities can't be executed in combat
        /// </summary>
        public override void Execute(Entity user, Entity target, RNGManager rng)
        {
            Console.WriteLine($"{Name} is a passive ability providing a permanent +{GetPassiveBonusValue()}% accuracy bonus to all allies.");
            Console.WriteLine("It is always active and cannot be used in combat.");
        }
    }
}