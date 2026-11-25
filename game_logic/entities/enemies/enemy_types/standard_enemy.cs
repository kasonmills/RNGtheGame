using System;
using GameLogic.Abilities;
using GameLogic.Systems;

namespace GameLogic.Entities.Enemies.EnemyTypes
{
    /// <summary>
    /// One regular enemy's numbers and behaviour, as multipliers on the standard enemy
    /// (enemy_roster.md, reviewed 2026-11-24). All numbers are placeholders, to retune after
    /// playtesting. The profiles themselves are in EnemyRoster.
    /// </summary>
    public class EnemyProfile
    {
        public string Name { get; init; }
        public string Family { get; init; }
        public string Description { get; init; }
        public EnemyType Type { get; init; } = EnemyType.Common;

        // Multipliers on the standard enemy
        public double Health { get; init; } = 1.0;
        public double Damage { get; init; } = 1.0;
        public double Defense { get; init; } = 1.0;
        public int SpeedBonus { get; init; }

        // Flat numbers
        public int Accuracy { get; init; } = StandardEnemy.StandardAccuracy;
        public int CritChance { get; init; } = StandardEnemy.StandardCritChance;

        /// <summary>Multiplier on the standard XP (and gold) for its level.</summary>
        public double XPWeight { get; init; } = 1.0;

        /// <summary>How likely it is to notice and charge the player in the overworld (0-100).</summary>
        public int Aggression { get; init; } = 50;

        // Behaviour
        public EnemyBehavior Behavior { get; init; } = EnemyBehavior.Balanced;
        public EnemyTargetPattern Targeting { get; init; } = EnemyTargetPattern.Random;
        public double FleeBelowHealth { get; init; }       // 0 = never flees
        public int FleeAttemptChance { get; init; } = 30;   // 100 = always tries below the line
        public bool BackRow { get; init; }

        /// <summary>
        /// Its hits' status effect: the chance (1-100) a hit carries it, and the effect built for
        /// its level. 0 / null = plain hits.
        /// </summary>
        public int OnHitEffectChance { get; init; }
        public Func<int, AbilityEffect> OnHitEffect { get; init; }

        /// <summary>
        /// Enemies of the same group pay less XP and gold the more of them are beaten in one
        /// fight (EnemyBase.DiminishingRewardsGroup). Null = always full rewards.
        /// </summary>
        public string RewardsGroup { get; init; }
    }

    /// <summary>
    /// A regular enemy built from the standard enemy and its EnemyProfile (decided 2026-11-23,
    /// numbers reviewed 2026-11-24):
    /// - Health 20 + 6 per level, damage (2 + level/2) to (5 + level), defense level/4,
    ///   speed 10 + level/4, 75% accuracy, 5% crit - each times the profile's multiplier.
    /// - Health and damage round to the nearest whole number (at least 1), defense rounds down.
    /// - The difficulty goes on top when a fight starts (DifficultyScaler.ApplyDifficultyToEnemy).
    /// </summary>
    public abstract class StandardEnemy : EnemyBase
    {
        public const int StandardAccuracy = 75;
        public const int StandardCritChance = 5;

        /// <summary>
        /// Base XP and gold per level for a weight-1 enemy - placeholders until the XP rework
        /// (to-do section 1) sets the real base reward.
        /// </summary>
        public const int StandardXPPerLevel = 50;
        public const int StandardGoldPerLevel = 12;

        public EnemyProfile Profile { get; }

        protected StandardEnemy(EnemyProfile profile, int level)
        {
            Profile = profile;
            Name = profile.Name;
            Family = profile.Family;
            Type = profile.Type;
            Behavior = profile.Behavior;
            Targeting = profile.Targeting;
            FleeBelowHealth = profile.FleeBelowHealth;
            FleeAttemptChance = profile.FleeAttemptChance;
            IsBackRow = profile.BackRow;
            DiminishingRewardsGroup = profile.RewardsGroup;

            InitializeStats(level);
        }

        // ===== The standard enemy =====

        public static int StandardHealth(int level) => 20 + 6 * level;
        public static int StandardMinDamage(int level) => 2 + level / 2;
        public static int StandardMaxDamage(int level) => 5 + level;
        public static int StandardDefense(int level) => level / 4;
        public static int StandardSpeed(int level) => 10 + level / 4;

        public override void InitializeStats(int level)
        {
            Level = Math.Max(1, level);
            DifficultyApplied = false;
            SpawnedOnDefeat = false;

            MaxHealth = Math.Max(1, Scale(StandardHealth(Level), Profile.Health));
            Health = MaxHealth;

            MinDamage = Math.Max(1, Scale(StandardMinDamage(Level), Profile.Damage));
            MaxDamage = Math.Max(MinDamage, Scale(StandardMaxDamage(Level), Profile.Damage));

            Defense = (int)Math.Floor(StandardDefense(Level) * Profile.Defense + 1e-9);
            Speed = Math.Max(1, StandardSpeed(Level) + Profile.SpeedBonus);

            Accuracy = Profile.Accuracy;
            CritChance = Profile.CritChance;
            AggressionLevel = Profile.Aggression;

            XPValue = Math.Max(1, Scale(StandardXPPerLevel * Level, Profile.XPWeight));
            GoldValue = Math.Max(0, Scale(StandardGoldPerLevel * Level, Profile.XPWeight));
        }

        /// <summary>A standard number times a multiplier, rounded to the nearest whole number.</summary>
        private static int Scale(int value, double multiplier)
        {
            return (int)Math.Round(Math.Round(value * multiplier, 6));
        }

        public override AbilityEffect RollOnHitEffect(RNGManager rng)
        {
            if (Profile.OnHitEffect == null || Profile.OnHitEffectChance <= 0) return null;
            return rng.Roll(1, 100) <= Profile.OnHitEffectChance ? Profile.OnHitEffect(Level) : null;
        }

        public override string GetDescription()
        {
            return string.IsNullOrEmpty(Profile.Description)
                ? $"{Name} (Level {Level})"
                : $"{Name} - {Profile.Description} (Level {Level})";
        }
    }
}
