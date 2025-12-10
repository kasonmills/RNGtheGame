using GameLogic.Entities;
using GameLogic.Systems;

namespace GameLogic.Abilities
{
    /// <summary>
    /// Swift Tactics - Passive ability that increases companion speed
    /// Level 1: +2% speed for all companions
    /// Level 100: +50% speed for all companions
    /// Percentage-based scaling - benefits all companion types proportionally
    /// Does NOT affect player speed, only companions
    /// </summary>
    public class SwiftTacticsAbility : Ability
    {
        public SwiftTacticsAbility()
        {
            Name = "Swift Tactics";
            Description = "Your tactical expertise and battlefield coordination allow your companions to move and react faster in combat.";
            IsPassive = true;
            Rarity = AbilityRarity.Epic;

            // Passive abilities don't have cooldown or targeting
            Cooldown = 0;
            TargetType = AbilityTarget.AllAllies;
        }

        /// <summary>
        /// Get the companion speed bonus percentage based on level
        /// Scales from 2% to 50% linearly
        /// </summary>
        public override int GetPassiveBonusValue()
        {
            // Linear scaling: 2% at level 1, 50% at level 100
            return GetScaledValueInt(2, 50);
        }

        /// <summary>
        /// Get the actual speed multiplier (as a decimal)
        /// E.g., 25% bonus returns 1.25
        /// </summary>
        public double GetSpeedMultiplier()
        {
            int bonusPercent = GetPassiveBonusValue();
            return 1.0 + (bonusPercent / 100.0);
        }

        /// <summary>
        /// Calculate the speed bonus for a companion with given base speed
        /// Returns the bonus amount (not the total)
        /// </summary>
        public int GetSpeedBonus(int baseSpeed)
        {
            double multiplier = GetSpeedMultiplier();
            int boostedSpeed = (int)(baseSpeed * multiplier);
            return boostedSpeed - baseSpeed;
        }

        /// <summary>
        /// Called when Swift Tactics levels up - notify about speed increase
        /// </summary>
        protected override void OnLevelUp()
        {
            int newBonus = GetPassiveBonusValue();

            // Notify every 10 levels about progress
            if (Level % 10 == 0)
            {
                Console.WriteLine($"Your tactical prowess improves! Companion speed bonus is now {newBonus}%!");
            }
        }

        /// <summary>
        /// Get detailed info about Swift Tactics ability
        /// </summary>
        public override string GetInfo()
        {
            int bonus = GetPassiveBonusValue();
            int nextMilestone = ((Level / 10) + 1) * 10;
            if (nextMilestone > 100) nextMilestone = 100;

            string info = $"{Name} (Lv.{Level}/{MaxLevel}) [PASSIVE]\n" +
                         $"{Description}\n\n" +
                         $"Current Companion Speed Bonus: +{bonus}%\n" +
                         $"Affects: All companions in your party\n" +
                         $"Does NOT affect: Player speed\n" +
                         $"Effect: Companions act earlier in turn order";

            if (Level < 100)
            {
                // Calculate bonus at next milestone
                Ability tempAbility = new SwiftTacticsAbility();
                tempAbility.Level = nextMilestone;
                int milestoneBonus = tempAbility.GetPassiveBonusValue();

                info += $"\n\nNext Milestone: Level {nextMilestone}\n" +
                       $"  Companion speed bonus will be {milestoneBonus}%";
            }
            else
            {
                info += $"\n\nMax level reached! Companion speed bonus at maximum ({bonus}%).";
            }

            // Show example speed values
            info += $"\n\nExample Speed Values (at {bonus}% bonus):";
            info += $"\n  Warrior (base 23) → {(int)(23 * GetSpeedMultiplier())}";
            info += $"\n  Healer (base 27) → {(int)(27 * GetSpeedMultiplier())}";
            info += $"\n  Ranger (base 38) → {(int)(38 * GetSpeedMultiplier())}";
            info += $"\n  Rogue (base 40) → {(int)(40 * GetSpeedMultiplier())}";

            info += $"\n\nXP: {Experience}/{ExperienceToNextLevel}";

            return info;
        }

        /// <summary>
        /// Passive abilities can't be executed in combat
        /// </summary>
        public override void Execute(Entity user, Entity target, RNGManager rng)
        {
            Console.WriteLine($"{Name} is a passive ability providing a permanent +{GetPassiveBonusValue()}% speed bonus to all companions.");
            Console.WriteLine("It is always active and cannot be used in combat.");
        }
    }
}