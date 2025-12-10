using GameLogic.Entities;
using GameLogic.Systems;

namespace GameLogic.Abilities
{
    /// <summary>
    /// Rallying Cry - Passive ability that increases companion attack damage
    /// Level 1: +5% companion attack damage
    /// Level 100: +90% companion attack damage
    /// Linear scaling between levels
    /// Does NOT affect player damage, only companions
    /// </summary>
    public class RallyingCryAbility : Ability
    {
        public RallyingCryAbility()
        {
            Name = "Rallying Cry";
            Description = "Your inspiring presence and battle cries embolden your companions, increasing their attack damage.";
            IsPassive = true;
            Rarity = AbilityRarity.Rare;

            // Passive abilities don't have cooldown or targeting
            Cooldown = 0;
            TargetType = AbilityTarget.AllAllies;
        }

        /// <summary>
        /// Get the companion attack damage bonus percentage based on level
        /// Scales from 5% to 90% linearly
        /// </summary>
        public override int GetPassiveBonusValue()
        {
            // Linear scaling: 5% at level 1, 90% at level 100
            return GetScaledValueInt(5, 90);
        }

        /// <summary>
        /// Get the actual damage multiplier (as a decimal)
        /// E.g., 25% bonus returns 1.25
        /// </summary>
        public double GetDamageMultiplier()
        {
            int bonusPercent = GetPassiveBonusValue();
            return 1.0 + (bonusPercent / 100.0);
        }

        /// <summary>
        /// Called when Rallying Cry levels up - notify about damage increase
        /// </summary>
        protected override void OnLevelUp()
        {
            int newBonus = GetPassiveBonusValue();

            // Notify every 10 levels about progress
            if (Level % 10 == 0)
            {
                Console.WriteLine($"Your rallying cry grows stronger! Companion attack bonus is now {newBonus}%!");
            }
        }

        /// <summary>
        /// Get detailed info about Rallying Cry ability
        /// </summary>
        public override string GetInfo()
        {
            int bonus = GetPassiveBonusValue();
            int nextMilestone = ((Level / 10) + 1) * 10;
            if (nextMilestone > 100) nextMilestone = 100;

            string info = $"{Name} (Lv.{Level}/{MaxLevel}) [PASSIVE]\n" +
                         $"{Description}\n\n" +
                         $"Current Companion Attack Bonus: +{bonus}%\n" +
                         $"Affects: All companions in your party\n" +
                         $"Does NOT affect: Player damage";

            if (Level < 100)
            {
                int nextBonus = GetScaledValueInt(5, 90); // Calculate what it would be at next milestone
                // Calculate bonus at next milestone
                Ability tempAbility = new RallyingCryAbility();
                tempAbility.Level = nextMilestone;
                int milestoneBonus = tempAbility.GetPassiveBonusValue();

                info += $"\n\nNext Milestone: Level {nextMilestone}\n" +
                       $"  Companion attack bonus will be {milestoneBonus}%";
            }
            else
            {
                info += $"\n\nMax level reached! Companion attack bonus at maximum ({bonus}%).";
            }

            info += $"\n\nXP: {Experience}/{ExperienceToNextLevel}";

            return info;
        }

        /// <summary>
        /// Passive abilities can't be executed in combat
        /// </summary>
        public override void Execute(Entity user, Entity target, RNGManager rng)
        {
            Console.WriteLine($"{Name} is a passive ability providing a permanent +{GetPassiveBonusValue()}% attack bonus to all companions.");
            Console.WriteLine("It is always active and cannot be used in combat.");
        }
    }
}