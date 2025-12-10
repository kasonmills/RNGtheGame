using GameLogic.Entities;
using GameLogic.Systems;

namespace GameLogic.Abilities
{
    /// <summary>
    /// Evasion - Passive ability that gives a chance to completely avoid damage
    /// Level 1: 1% chance to evade
    /// Level 100: 60% chance to evade
    /// Scales linearly between levels
    /// Does NOT work when defending (defending already reduces damage)
    /// </summary>
    public class EvasionAbility : Ability
    {
        public EvasionAbility()
        {
            Name = "Evasion";
            Description = "Your reflexes and agility allow you to dodge incoming attacks, taking no damage. Does not work while defending.";
            IsPassive = true;
            Rarity = AbilityRarity.Epic;

            // Passive abilities don't have cooldown or targeting
            Cooldown = 0;
            TargetType = AbilityTarget.Self;
        }

        /// <summary>
        /// Get the evasion chance percentage based on level
        /// Scales linearly from 1% (level 1) to 60% (level 100)
        /// </summary>
        public override int GetPassiveBonusValue()
        {
            // Level 1: 1%
            // Level 100: 60%
            // Linear scaling: 1 + ((Level - 1) * 59 / 99)
            return GetScaledValueInt(1, 60);
        }

        /// <summary>
        /// Check if an attack should be evaded
        /// Returns true if the attack is evaded (no damage taken)
        /// </summary>
        public bool ShouldEvade(RNGManager rng, bool isDefending)
        {
            // Evasion doesn't work while defending
            if (isDefending)
            {
                return false;
            }

            int evasionChance = GetPassiveBonusValue();
            int roll = rng.Roll(1, 100);

            return roll <= evasionChance;
        }

        /// <summary>
        /// Called when evasion levels up
        /// </summary>
        protected override void OnLevelUp()
        {
            int newChance = GetPassiveBonusValue();

            // Notify at milestone levels
            if (Level % 10 == 0)
            {
                Console.WriteLine($"Your evasion has improved! Evasion chance is now {newChance}%!");
            }
        }

        /// <summary>
        /// Get detailed info about evasion ability
        /// </summary>
        public override string GetInfo()
        {
            int evasionChance = GetPassiveBonusValue();
            int nextLevelChance = Level < MaxLevel ? GetScaledValueInt(1, 60) + 1 : evasionChance;

            string info = $"{Name} (Lv.{Level}/{MaxLevel}) [PASSIVE]\n" +
                         $"{Description}\n\n" +
                         $"Current Evasion Chance: {evasionChance}%\n" +
                         $"When triggered: Take 0 damage instead of incoming damage\n" +
                         $"Restriction: Does not work while defending";

            if (Level < MaxLevel)
            {
                // Calculate approximate level for next 5% milestone
                int currentChance = evasionChance;
                int nextMilestone = ((currentChance / 5) + 1) * 5; // Round up to next multiple of 5
                if (nextMilestone > 60) nextMilestone = 60;

                // Calculate level needed for next milestone
                // Formula: Level = 1 + ((Chance - 1) * 99 / 59)
                int levelsNeeded = nextMilestone - currentChance;
                double levelsPerPercent = 99.0 / 59.0; // ~1.678 levels per 1%
                int approxLevelForMilestone = Level + (int)Math.Ceiling(levelsNeeded * levelsPerPercent);

                if (approxLevelForMilestone > MaxLevel) approxLevelForMilestone = MaxLevel;

                info += $"\n\nNext Milestone: {nextMilestone}% (around Level {approxLevelForMilestone})";
            }
            else
            {
                info += $"\n\nMax level reached! Evasion chance at maximum ({evasionChance}%).";
            }

            info += $"\n\nXP: {Experience}/{ExperienceToNextLevel}";

            return info;
        }

        /// <summary>
        /// Passive abilities can't be executed in combat
        /// </summary>
        public override void Execute(Entity user, Entity target, RNGManager rng)
        {
            Console.WriteLine($"{Name} is a passive ability providing a {GetPassiveBonusValue()}% chance to evade incoming damage.");
            Console.WriteLine("It is always active and cannot be used manually.");
            Console.WriteLine("Note: Evasion does not work while defending.");
        }
    }
}