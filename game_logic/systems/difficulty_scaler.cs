using System;
using GameLogic.Progression;

namespace GameLogic.Systems
{
    /// <summary>
    /// Base game difficulty setting chosen at character creation
    /// Affects all enemies, loot, and rewards
    /// </summary>
    public enum GameDifficulty
    {
        Easy,       // 0.75x difficulty
        Normal,     // 1.0x difficulty (baseline)
        Hard,       // 1.5x difficulty
        Nightmare   // 2.0x difficulty
    }

    /// <summary>
    /// Difficulty scaler that adjusts enemy stats based on party strength and game difficulty
    /// Scales to the ENTIRE PARTY (player + companions) for balanced combat
    /// Encourages players to keep all companions leveled up
    /// </summary>
    public class DifficultyScaler
    {
        // Base game difficulty multiplier
        private readonly GameDifficulty _gameDifficulty;
        private readonly float _difficultyMultiplier;

        // Scaling configuration
        private const float HEALTH_SCALING_PER_LEVEL = 0.10f;      // +10% HP per level
        private const float DAMAGE_SCALING_PER_LEVEL = 0.08f;      // +8% damage per level
        private const float XP_SCALING_PER_LEVEL = 0.12f;          // +12% XP per level
        private const float GOLD_SCALING_PER_LEVEL = 0.10f;        // +10% gold per level

        public DifficultyScaler(GameDifficulty gameDifficulty)
        {
            _gameDifficulty = gameDifficulty;
            _difficultyMultiplier = GetDifficultyMultiplier(gameDifficulty);
        }

        #region Party Power Calculation

        /// <summary>
        /// Calculate the effective "party level" for scaling enemies
        /// Takes into account player level + all active companions
        /// </summary>
        public int CalculatePartyLevel(int playerLevel, params int[] companionLevels)
        {
            if (companionLevels == null || companionLevels.Length == 0)
            {
                // Solo player - just use player level
                return playerLevel;
            }

            // Calculate weighted average with player counting more
            // Player weight: 50%, Companions: 50% total split among them
            float playerWeight = 0.5f;
            float companionWeight = 0.5f / companionLevels.Length;

            float totalLevel = playerLevel * playerWeight;
            foreach (int companionLevel in companionLevels)
            {
                totalLevel += companionLevel * companionWeight;
            }

            return (int)Math.Ceiling(totalLevel);
        }

        /// <summary>
        /// Calculate party level with a penalty for under-leveled companions
        /// This makes having low-level companions a disadvantage
        /// </summary>
        public int CalculatePartyLevelWithPenalty(int playerLevel, params int[] companionLevels)
        {
            if (companionLevels == null || companionLevels.Length == 0)
            {
                return playerLevel;
            }

            int partyLevel = CalculatePartyLevel(playerLevel, companionLevels);

            // Check for under-leveled companions (more than 5 levels below player)
            int underLeveledCount = 0;
            foreach (int companionLevel in companionLevels)
            {
                if (playerLevel - companionLevel > 5)
                {
                    underLeveledCount++;
                }
            }

            // Apply penalty: -1 level per under-leveled companion
            partyLevel -= underLeveledCount;

            // Don't go below player level though
            return Math.Max(partyLevel, playerLevel);
        }

        /// <summary>
        /// Calculate party level with bonus for well-balanced party
        /// Rewards keeping companions at similar levels
        /// </summary>
        public int CalculatePartyLevelBalanced(int playerLevel, params int[] companionLevels)
        {
            if (companionLevels == null || companionLevels.Length == 0)
            {
                return playerLevel;
            }

            int partyLevel = CalculatePartyLevel(playerLevel, companionLevels);

            // Calculate level variance
            int minLevel = companionLevels.Min();
            int maxLevel = companionLevels.Max();
            int levelSpread = maxLevel - minLevel;

            // Bonus for well-balanced party (all within 3 levels)
            if (levelSpread <= 3 && Math.Abs(playerLevel - minLevel) <= 3)
            {
                partyLevel += 1; // Small bonus for balanced party
            }

            return partyLevel;
        }

        #endregion

        /// <summary>
        /// Get the difficulty multiplier for the game difficulty
        /// </summary>
        private float GetDifficultyMultiplier(GameDifficulty difficulty)
        {
            return difficulty switch
            {
                GameDifficulty.Easy => 0.75f,
                GameDifficulty.Normal => 1.0f,
                GameDifficulty.Hard => 1.5f,
                GameDifficulty.Nightmare => 2.0f,
                _ => 1.0f
            };
        }

        #region Enemy Stat Scaling

        /// <summary>
        /// Scale enemy health based on party level and difficulty
        /// Use CalculatePartyLevel() to get the effective party level first
        /// </summary>
        public int ScaleEnemyHealth(int baseHealth, int partyLevel)
        {
            // Calculate level scaling: baseHealth * (1 + (level - 1) * scaling)
            // Example: 50 HP at level 1, 55 HP at level 2, 115 HP at level 10
            float levelMultiplier = 1.0f + ((partyLevel - 1) * HEALTH_SCALING_PER_LEVEL);

            // Apply difficulty multiplier
            float scaledHealth = baseHealth * levelMultiplier * _difficultyMultiplier;

            return (int)Math.Ceiling(scaledHealth);
        }

        /// <summary>
        /// Scale enemy damage based on party level and difficulty
        /// Use CalculatePartyLevel() to get the effective party level first
        /// </summary>
        public int ScaleEnemyDamage(int baseDamage, int partyLevel)
        {
            // Damage scales slower than health for balance
            float levelMultiplier = 1.0f + ((partyLevel - 1) * DAMAGE_SCALING_PER_LEVEL);

            // Apply difficulty multiplier
            float scaledDamage = baseDamage * levelMultiplier * _difficultyMultiplier;

            return (int)Math.Ceiling(scaledDamage);
        }

        /// <summary>
        /// Scale enemy XP reward based on party level and difficulty
        /// Higher difficulty = more XP as reward
        /// Use CalculatePartyLevel() to get the effective party level first
        /// </summary>
        public int ScaleEnemyXP(int baseXP, int partyLevel)
        {
            // XP scales with level
            float levelMultiplier = 1.0f + ((partyLevel - 1) * XP_SCALING_PER_LEVEL);

            // Difficulty bonus: harder difficulties give more XP
            float difficultyBonus = _gameDifficulty switch
            {
                GameDifficulty.Easy => 0.8f,       // Less XP on easy
                GameDifficulty.Normal => 1.0f,
                GameDifficulty.Hard => 1.25f,      // +25% XP on hard
                GameDifficulty.Nightmare => 1.5f,  // +50% XP on nightmare
                _ => 1.0f
            };

            float scaledXP = baseXP * levelMultiplier * difficultyBonus;

            return (int)Math.Ceiling(scaledXP);
        }

        /// <summary>
        /// Scale enemy gold drop based on party level and difficulty
        /// Use CalculatePartyLevel() to get the effective party level first
        /// </summary>
        public int ScaleEnemyGold(int baseGold, int partyLevel, Random rng = null)
        {
            rng ??= new Random();

            // Gold scales with level
            float levelMultiplier = 1.0f + ((partyLevel - 1) * GOLD_SCALING_PER_LEVEL);

            // Apply difficulty (harder = more gold)
            float difficultyBonus = _gameDifficulty switch
            {
                GameDifficulty.Easy => 1.0f,
                GameDifficulty.Normal => 1.0f,
                GameDifficulty.Hard => 1.2f,       // +20% gold
                GameDifficulty.Nightmare => 1.4f,  // +40% gold
                _ => 1.0f
            };

            float averageGold = baseGold * levelMultiplier * difficultyBonus;

            // Add variance: ±25%
            float minGold = averageGold * 0.75f;
            float maxGold = averageGold * 1.25f;

            return rng.Next((int)minGold, (int)maxGold + 1);
        }

        #endregion

        #region Boss Scaling

        /// <summary>
        /// Scale boss health (bosses get more health than regular enemies)
        /// Use CalculatePartyLevel() to get the effective party level first
        /// </summary>
        public int ScaleBossHealth(int baseHealth, int partyLevel)
        {
            // Bosses have 3x more health than regular enemies
            int regularHealth = ScaleEnemyHealth(baseHealth, partyLevel);
            return regularHealth * 3;
        }

        /// <summary>
        /// Scale boss damage (bosses hit harder)
        /// Use CalculatePartyLevel() to get the effective party level first
        /// </summary>
        public int ScaleBossDamage(int baseDamage, int partyLevel)
        {
            // Bosses deal 1.5x damage
            int regularDamage = ScaleEnemyDamage(baseDamage, partyLevel);
            return (int)(regularDamage * 1.5f);
        }

        /// <summary>
        /// Scale boss XP reward (bosses give much more XP)
        /// Use CalculatePartyLevel() to get the effective party level first
        /// </summary>
        public int ScaleBossXP(int baseXP, int partyLevel)
        {
            // Bosses give 5x XP
            int regularXP = ScaleEnemyXP(baseXP, partyLevel);
            return regularXP * 5;
        }

        /// <summary>
        /// Scale boss gold reward (bosses drop more gold)
        /// Use CalculatePartyLevel() to get the effective party level first
        /// </summary>
        public int ScaleBossGold(int baseGold, int partyLevel, Random rng = null)
        {
            // Bosses drop 3-5x gold
            rng ??= new Random();
            int regularGold = ScaleEnemyGold(baseGold, partyLevel, rng);
            int multiplier = rng.Next(3, 6); // 3x to 5x
            return regularGold * multiplier;
        }

        #endregion

        #region Elite/Special Enemy Scaling

        /// <summary>
        /// Scale elite enemy health (stronger than normal, weaker than boss)
        /// Use CalculatePartyLevel() to get the effective party level first
        /// </summary>
        public int ScaleEliteHealth(int baseHealth, int partyLevel)
        {
            // Elites have 1.75x health
            int regularHealth = ScaleEnemyHealth(baseHealth, partyLevel);
            return (int)(regularHealth * 1.75f);
        }

        /// <summary>
        /// Scale elite enemy damage
        /// Use CalculatePartyLevel() to get the effective party level first
        /// </summary>
        public int ScaleEliteDamage(int baseDamage, int partyLevel)
        {
            // Elites deal 1.25x damage
            int regularDamage = ScaleEnemyDamage(baseDamage, partyLevel);
            return (int)(regularDamage * 1.25f);
        }

        /// <summary>
        /// Scale elite enemy XP reward
        /// Use CalculatePartyLevel() to get the effective party level first
        /// </summary>
        public int ScaleEliteXP(int baseXP, int partyLevel)
        {
            // Elites give 2x XP
            int regularXP = ScaleEnemyXP(baseXP, partyLevel);
            return regularXP * 2;
        }

        /// <summary>
        /// Scale elite enemy gold reward
        /// Use CalculatePartyLevel() to get the effective party level first
        /// </summary>
        public int ScaleEliteGold(int baseGold, int partyLevel, Random rng = null)
        {
            // Elites drop 1.5-2x gold
            rng ??= new Random();
            int regularGold = ScaleEnemyGold(baseGold, partyLevel, rng);
            float multiplier = 1.5f + ((float)rng.NextDouble() * 0.5f); // 1.5x to 2.0x
            return (int)(regularGold * multiplier);
        }

        #endregion

        #region Loot Quality Scaling

        /// <summary>
        /// Get item level based on party level (with variance)
        /// Use CalculatePartyLevel() to get the effective party level first
        /// </summary>
        public int GetScaledItemLevel(int partyLevel, Random rng = null, int variance = 3)
        {
            rng ??= new Random();

            // Item drops at party level ±variance
            int itemLevel = partyLevel + rng.Next(-variance, variance + 1);

            // Clamp to valid range (1-100)
            return Math.Clamp(itemLevel, 1, 100);
        }

        /// <summary>
        /// Get luck modifier for loot rarity based on difficulty
        /// Harder difficulties have better drop rates
        /// </summary>
        public float GetLootQualityModifier()
        {
            return _gameDifficulty switch
            {
                GameDifficulty.Easy => 0.8f,       // 20% worse drops
                GameDifficulty.Normal => 1.0f,
                GameDifficulty.Hard => 1.3f,       // 30% better drops
                GameDifficulty.Nightmare => 1.6f,  // 60% better drops
                _ => 1.0f
            };
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Get the current game difficulty
        /// </summary>
        public GameDifficulty GetGameDifficulty()
        {
            return _gameDifficulty;
        }

        /// <summary>
        /// Get the difficulty multiplier
        /// </summary>
        public float GetDifficultyMultiplier()
        {
            return _difficultyMultiplier;
        }

        /// <summary>
        /// Get a display-friendly difficulty description
        /// </summary>
        public string GetDifficultyDescription()
        {
            return _gameDifficulty switch
            {
                GameDifficulty.Easy => "Easy (75% difficulty, good for learning)",
                GameDifficulty.Normal => "Normal (100% difficulty, balanced experience)",
                GameDifficulty.Hard => "Hard (150% difficulty, for experienced players)",
                GameDifficulty.Nightmare => "Nightmare (200% difficulty, brutal challenge)",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Calculate the "effective level" of an enemy for display
        /// Accounts for enemy type (normal/elite/boss) and scaling
        /// Use CalculatePartyLevel() to get the effective party level first
        /// </summary>
        public int GetEffectiveEnemyLevel(int baseLevel, int partyLevel, bool isElite = false, bool isBoss = false)
        {
            // In level-scaling games, enemies are always at party level
            int effectiveLevel = partyLevel;

            // Bosses/elites can be considered "higher level" for display purposes
            if (isBoss)
            {
                effectiveLevel += 5; // Bosses are shown as +5 levels
            }
            else if (isElite)
            {
                effectiveLevel += 2; // Elites are shown as +2 levels
            }

            return Math.Min(effectiveLevel, 100); // Cap at level 100
        }

        /// <summary>
        /// Get a danger rating string for UI based on difficulty and enemy type
        /// </summary>
        public string GetDangerRating(bool isElite = false, bool isBoss = false)
        {
            if (isBoss)
            {
                return _gameDifficulty switch
                {
                    GameDifficulty.Easy => "[BOSS] Challenging",
                    GameDifficulty.Normal => "[BOSS] Dangerous",
                    GameDifficulty.Hard => "[BOSS] Deadly",
                    GameDifficulty.Nightmare => "[BOSS] Lethal",
                    _ => "[BOSS]"
                };
            }
            else if (isElite)
            {
                return _gameDifficulty switch
                {
                    GameDifficulty.Easy => "[Elite] Moderate",
                    GameDifficulty.Normal => "[Elite] Tough",
                    GameDifficulty.Hard => "[Elite] Dangerous",
                    GameDifficulty.Nightmare => "[Elite] Deadly",
                    _ => "[Elite]"
                };
            }
            else
            {
                return _gameDifficulty switch
                {
                    GameDifficulty.Easy => "Easy",
                    GameDifficulty.Normal => "Normal",
                    GameDifficulty.Hard => "Tough",
                    GameDifficulty.Nightmare => "Dangerous",
                    _ => "Unknown"
                };
            }
        }

        #endregion

        #region Party Composition Analysis

        /// <summary>
        /// Get info about party composition for UI display
        /// </summary>
        public string GetPartyCompositionInfo(int playerLevel, params int[] companionLevels)
        {
            if (companionLevels == null || companionLevels.Length == 0)
            {
                return $"Solo (Level {playerLevel})";
            }

            int partyLevel = CalculatePartyLevel(playerLevel, companionLevels);
            int avgCompanionLevel = (int)companionLevels.Average();
            int underLeveledCount = companionLevels.Count(c => playerLevel - c > 5);

            string status = underLeveledCount > 0 ? "WEAK PARTY" : "BALANCED";

            return $"Party Level {partyLevel} ({status})\n" +
                   $"Player: Level {playerLevel}\n" +
                   $"Companions: {companionLevels.Length} active (Avg Level {avgCompanionLevel})";
        }

        /// <summary>
        /// Calculate recommended companion level for current player level
        /// </summary>
        public int GetRecommendedCompanionLevel(int playerLevel)
        {
            // Companions should ideally be within 3 levels of player
            return Math.Max(1, playerLevel - 3);
        }

        /// <summary>
        /// Check if a companion is under-leveled and needs training
        /// </summary>
        public bool IsCompanionUnderLeveled(int companionLevel, int playerLevel)
        {
            return (playerLevel - companionLevel) > 5;
        }

        #endregion
    }
}