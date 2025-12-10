using GameLogic.Entities;
using GameLogic.Systems;

namespace GameLogic.Abilities
{
    /// <summary>
    /// Leadership - Passive ability that increases max party size
    /// Level 1: +0 (base party size is 4)
    /// Level 25: +1 (party size becomes 5)
    /// Level 50: +2 (party size becomes 6)
    /// Level 75: +3 (party size becomes 7)
    /// Level 100: +4 (party size becomes 8)
    /// </summary>
    public class LeadershipAbility : Ability
    {
        public LeadershipAbility()
        {
            Name = "Leadership";
            Description = "Your natural charisma and leadership allow you to inspire more companions to join your party.";
            IsPassive = true;
            Rarity = AbilityRarity.Rare;

            // Passive abilities don't have cooldown or targeting
            Cooldown = 0;
            TargetType = AbilityTarget.Self;
        }

        /// <summary>
        /// Get the party size bonus based on level
        /// Increases by 1 every 25 levels
        /// </summary>
        public override int GetPassiveBonusValue()
        {
            if (Level >= 100) return 4;
            if (Level >= 75) return 3;
            if (Level >= 50) return 2;
            if (Level >= 25) return 1;
            return 0;
        }

        /// <summary>
        /// Called when leadership levels up - notify about party size increase
        /// </summary>
        protected override void OnLevelUp()
        {
            int newBonus = GetPassiveBonusValue();
            int baseSize = 4;
            int totalSize = baseSize + newBonus;

            // Notify on milestone levels
            if (Level == 25 || Level == 50 || Level == 75 || Level == 100)
            {
                Console.WriteLine($"Your leadership has grown! Max party size increased to {totalSize}!");
            }
        }

        /// <summary>
        /// Get detailed info about leadership ability
        /// </summary>
        public override string GetInfo()
        {
            int bonus = GetPassiveBonusValue();
            int currentSize = 4 + bonus;
            int nextMilestone = 0;

            if (Level < 25) nextMilestone = 25;
            else if (Level < 50) nextMilestone = 50;
            else if (Level < 75) nextMilestone = 75;
            else if (Level < 100) nextMilestone = 100;

            string info = $"{Name} (Lv.{Level}/{MaxLevel}) [PASSIVE]\n" +
                         $"{Description}\n\n" +
                         $"Current Party Size: {currentSize}\n" +
                         $"Party Size Bonus: +{bonus}";

            if (nextMilestone > 0)
            {
                int nextBonus = (nextMilestone / 25);
                int nextSize = 4 + nextBonus;
                info += $"\n\nNext Milestone: Level {nextMilestone}\n" +
                       $"  Party Size will increase to {nextSize}";
            }
            else
            {
                info += $"\n\nMax level reached! Party size at maximum ({currentSize}).";
            }

            info += $"\n\nXP: {Experience}/{ExperienceToNextLevel}";

            return info;
        }

        /// <summary>
        /// Passive abilities can't be executed in combat
        /// </summary>
        public override void Execute(Entity user, Entity target, RNGManager rng)
        {
            Console.WriteLine($"{Name} is a passive ability providing a permanent +{GetPassiveBonusValue()} party size bonus.");
            Console.WriteLine("It is always active and cannot be used in combat.");
        }
    }
}