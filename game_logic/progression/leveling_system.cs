using System;

namespace GameLogic.Progression
{
    /// <summary>
    /// Interface for entities that can level up
    /// </summary>
    public interface ILevelable
    {
        int Level { get; }
        int MaxLevel { get; }
        bool CanLevelUp();
        bool LevelUp();
    }

    /// <summary>
    /// Interface for XP-based leveling (Player, Companions, Abilities)
    /// </summary>
    public interface IExperienceBased : ILevelable
    {
        int Experience { get; }
        int ExperienceToNextLevel { get; }
        void GainExperience(int amount);
    }

    /// <summary>
    /// Leveling system utility class
    /// Provides helper methods for calculating XP curves and level requirements
    /// </summary>
    public static class LevelingSystem
    {
        #region XP Calculation Methods

        /// <summary>
        /// Calculate XP required for a specific level using exponential curve
        /// Used by Player and Companions
        /// </summary>
        public static int CalculateXPForLevel(int level, int baseXP = 100, double scalingFactor = 1.15)
        {
            if (level <= 1) return 0;

            // Exponential formula: baseXP * (scalingFactor ^ (level - 1))
            return (int)(baseXP * Math.Pow(scalingFactor, level - 1));
        }

        /// <summary>
        /// Calculate total XP needed to reach a specific level from level 1
        /// </summary>
        public static int CalculateTotalXPForLevel(int targetLevel, int baseXP = 100, double scalingFactor = 1.15)
        {
            int totalXP = 0;
            for (int level = 2; level <= targetLevel; level++)
            {
                totalXP += CalculateXPForLevel(level, baseXP, scalingFactor);
            }
            return totalXP;
        }

        /// <summary>
        /// Calculate what level a given amount of total XP corresponds to
        /// </summary>
        public static int CalculateLevelFromTotalXP(int totalXP, int baseXP = 100, double scalingFactor = 1.15, int maxLevel = 100)
        {
            int level = 1;
            int xpAccumulated = 0;

            while (level < maxLevel)
            {
                int xpForNextLevel = CalculateXPForLevel(level + 1, baseXP, scalingFactor);
                if (xpAccumulated + xpForNextLevel > totalXP)
                {
                    break;
                }
                xpAccumulated += xpForNextLevel;
                level++;
            }

            return level;
        }

        /// <summary>
        /// Get XP progress percentage towards next level
        /// </summary>
        public static float GetXPProgress(int currentXP, int xpForNextLevel)
        {
            if (xpForNextLevel <= 0) return 1.0f;
            return Math.Clamp((float)currentXP / xpForNextLevel, 0f, 1f);
        }

        #endregion

        #region Ability Leveling Methods

        /// <summary>
        /// Calculate XP required to level up an ability
        /// Abilities level faster than characters (1-20 range typically)
        /// </summary>
        public static int CalculateAbilityXPForLevel(int level, int baseXP = 50, double scalingFactor = 1.2)
        {
            if (level <= 1) return 0;
            return (int)(baseXP * Math.Pow(scalingFactor, level - 1));
        }

        /// <summary>
        /// Get ability rank name based on level
        /// </summary>
        public static string GetAbilityRankName(int level)
        {
            return level switch
            {
                <= 3 => "Novice",
                <= 6 => "Apprentice",
                <= 10 => "Adept",
                <= 15 => "Expert",
                <= 19 => "Master",
                >= 20 => "Legendary",
                _ => "Unknown"
            };
        }

        #endregion

        #region Item Enhancement Methods

        /// <summary>
        /// Calculate cost to upgrade an item to the next level
        /// Cost increases significantly with level
        /// </summary>
        public static int CalculateItemUpgradeCost(int currentLevel, int baseGoldCost = 100)
        {
            // Cost formula: baseGoldCost * (currentLevel^1.5)
            // Level 1->2: 100 gold
            // Level 10->11: ~316 gold
            // Level 50->51: ~3,535 gold
            // Level 99->100: ~9,949 gold
            return (int)(baseGoldCost * Math.Pow(currentLevel, 1.5));
        }

        /// <summary>
        /// Calculate material cost to upgrade an item
        /// Higher level items need more materials
        /// </summary>
        public static int CalculateItemMaterialCost(int currentLevel)
        {
            // Materials needed increases every 10 levels
            return 1 + (currentLevel / 10);
        }

        /// <summary>
        /// Check if an item is at a milestone level (10, 25, 50, 75, 100)
        /// Milestone levels could unlock special bonuses
        /// </summary>
        public static bool IsMilestoneLevel(int level)
        {
            return level == 10 || level == 25 || level == 50 || level == 75 || level == 100;
        }

        #endregion

        #region Stat Scaling Methods

        /// <summary>
        /// Calculate stat scaling multiplier based on level
        /// Used by items to scale their stats
        /// </summary>
        public static float GetStatScalingMultiplier(int level, int maxLevel, float scalingRate = 0.01f)
        {
            // Returns 1.0 at level 1, increases by scalingRate per level
            // Default: 1% increase per level
            return 1.0f + ((level - 1) * scalingRate);
        }

        /// <summary>
        /// Calculate bonus stats gained at a specific level
        /// Used for character stat increases on level up
        /// </summary>
        public static int CalculateStatIncreasePerLevel(int level, int baseIncrease = 5)
        {
            // Stats increase more at higher levels
            // Example: baseIncrease=5 means +5 at level 1, +5.05 at level 10, +5.5 at level 100
            return baseIncrease + (level / 20);
        }

        #endregion

        #region Enemy Scaling Methods

        /// <summary>
        /// Calculate enemy stats based on player level or area level
        /// Enemies scale differently than player characters
        /// </summary>
        public static int CalculateEnemyHealthForLevel(int level, int baseHealth = 50)
        {
            // Enemies gain more health per level than players
            // Formula: baseHealth * (1.1 ^ level)
            return (int)(baseHealth * Math.Pow(1.1, level));
        }

        /// <summary>
        /// Calculate enemy damage for a given level
        /// </summary>
        public static int CalculateEnemyDamageForLevel(int level, int baseDamage = 5)
        {
            // Damage scales slower than health
            // Formula: baseDamage * (1.08 ^ level)
            return (int)(baseDamage * Math.Pow(1.08, level));
        }

        /// <summary>
        /// Calculate XP reward for defeating an enemy
        /// </summary>
        public static int CalculateEnemyXPReward(int enemyLevel, int baseXP = 20)
        {
            // XP reward scales with enemy level
            // Formula: baseXP * (1.15 ^ level)
            return (int)(baseXP * Math.Pow(1.15, enemyLevel));
        }

        /// <summary>
        /// Calculate gold reward for defeating an enemy
        /// </summary>
        public static int CalculateEnemyGoldReward(int enemyLevel, int baseGold = 10, Random rng = null)
        {
            rng ??= new Random();

            // Gold reward with randomness
            int averageGold = (int)(baseGold * Math.Pow(1.12, enemyLevel));

            // Add variance: ±30%
            int minGold = (int)(averageGold * 0.7);
            int maxGold = (int)(averageGold * 1.3);

            return rng.Next(minGold, maxGold + 1);
        }

        #endregion

        #region Level Difference Calculations

        /// <summary>
        /// Calculate XP modifier based on level difference
        /// Fighting higher level enemies gives more XP, lower level gives less
        /// </summary>
        public static float CalculateXPModifierForLevelDifference(int playerLevel, int enemyLevel)
        {
            int levelDiff = enemyLevel - playerLevel;

            if (levelDiff >= 5)
            {
                // Fighting much stronger enemies: +50% XP
                return 1.5f;
            }
            else if (levelDiff >= 3)
            {
                // Fighting stronger enemies: +25% XP
                return 1.25f;
            }
            else if (levelDiff >= -2)
            {
                // Similar level: Normal XP
                return 1.0f;
            }
            else if (levelDiff >= -5)
            {
                // Fighting weaker enemies: -50% XP
                return 0.5f;
            }
            else
            {
                // Fighting much weaker enemies: -80% XP
                return 0.2f;
            }
        }

        /// <summary>
        /// Check if an enemy is too low level to give XP
        /// </summary>
        public static bool IsEnemyTooLowLevel(int playerLevel, int enemyLevel, int levelThreshold = 10)
        {
            return (playerLevel - enemyLevel) > levelThreshold;
        }

        #endregion

        #region Difficulty Scaling

        /// <summary>
        /// Calculate difficulty multiplier for different game modes
        /// </summary>
        public static float GetDifficultyMultiplier(DifficultyLevel difficulty)
        {
            return difficulty switch
            {
                DifficultyLevel.Easy => 0.75f,
                DifficultyLevel.Normal => 1.0f,
                DifficultyLevel.Hard => 1.5f,
                DifficultyLevel.Nightmare => 2.0f,
                _ => 1.0f
            };
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Format XP for display (e.g., "1,234 / 5,000")
        /// </summary>
        public static string FormatXPDisplay(int currentXP, int requiredXP)
        {
            return $"{currentXP:N0} / {requiredXP:N0}";
        }

        /// <summary>
        /// Get a visual progress bar for XP or level progress
        /// </summary>
        public static string GetProgressBar(float progress, int barLength = 20)
        {
            progress = Math.Clamp(progress, 0f, 1f);
            int filledLength = (int)(barLength * progress);
            int emptyLength = barLength - filledLength;

            string filled = new string('█', filledLength);
            string empty = new string('░', emptyLength);

            return $"[{filled}{empty}] {progress * 100:F0}%";
        }

        #endregion
    }

    /// <summary>
    /// Difficulty levels for the game
    /// </summary>
    public enum DifficultyLevel
    {
        Easy,
        Normal,
        Hard,
        Nightmare
    }
}