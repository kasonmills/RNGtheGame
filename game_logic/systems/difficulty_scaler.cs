using System;
using GameLogic.Progression;

namespace GameLogic.Systems
{
    // Difficulty uses the one list the player picks from: DifficultyLevel (game_settings.cs) -
    // Normal / Hard / Difficult / Unfair (merged 2026-11-10; this file had its own
    // Easy/Normal/Hard/Nightmare list, same multipliers in the same order). Hard is the 1.0x baseline.

    /// <summary>
    /// Difficulty scaler that adjusts enemy stats based on party strength and game difficulty
    /// Scales to the ENTIRE PARTY (player + companions) for balanced combat
    /// Encourages players to keep all companions leveled up
    /// </summary>
    public class DifficultyScaler
    {
        // Base game difficulty multipliers - enemy health and damage are split (2026-11-23)
        private readonly DifficultyLevel _gameDifficulty;
        private readonly float _healthMultiplier;
        private readonly float _damageMultiplier;

        /// <summary>
        /// Enemy accuracy is never raised above this by the difficulty (one already above it
        /// keeps its own number).
        /// </summary>
        public const int MaxEnemyAccuracy = 95;

        // Scaling configuration
        private const float HEALTH_SCALING_PER_LEVEL = 0.10f;      // +10% HP per level
        private const float DAMAGE_SCALING_PER_LEVEL = 0.08f;      // +8% damage per level
        private const float XP_SCALING_PER_LEVEL = 0.12f;          // +12% XP per level
        private const float GOLD_SCALING_PER_LEVEL = 0.10f;        // +10% gold per level

        public DifficultyScaler(DifficultyLevel gameDifficulty)
        {
            _gameDifficulty = gameDifficulty;
            _healthMultiplier = GetEnemyHealthMultiplier(gameDifficulty);
            _damageMultiplier = GetEnemyDamageMultiplier(gameDifficulty);
        }

        #region Party Power Calculation

        /// <summary>
        /// Calculate the effective "party level" for scaling enemies: a plain average of the
        /// player and the companions passed in (pass only the fighting party), rounded up.
        /// Decided 2026-11-10: a plain average, not player-weighted - it balances better across
        /// more party set-ups.
        /// </summary>
        public int CalculatePartyLevel(int playerLevel, params int[] companionLevels)
        {
            if (companionLevels == null || companionLevels.Length == 0)
            {
                // Solo player - just use player level
                return playerLevel;
            }

            float totalLevel = playerLevel + companionLevels.Sum();
            return (int)Math.Ceiling(totalLevel / (companionLevels.Length + 1));

            // Old player-weighted version (player 50%, companions share the other 50%) - not
            // used in the MVP, kept in case it is wanted later:
            // float playerWeight = 0.5f;
            // float companionWeight = 0.5f / companionLevels.Length;
            // float weightedLevel = playerLevel * playerWeight;
            // foreach (int companionLevel in companionLevels)
            // {
            //     weightedLevel += companionLevel * companionWeight;
            // }
            // return (int)Math.Ceiling(weightedLevel);
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

            // The penalty exists precisely because under-leveled companions already pull
            // partyLevel below playerLevel - flooring at playerLevel would cancel it out
            // every time it's meant to apply. Only floor at 1, a real level has to be positive.
            return Math.Max(partyLevel, 1);
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

        #region The numbers per difficulty (decided 2026-11-23 - Hard is the 1x baseline)

        /// <summary>
        /// Enemy health: Normal 75%, Hard 100%, Difficult 175%, Unfair 275%.
        /// </summary>
        public static float GetEnemyHealthMultiplier(DifficultyLevel difficulty)
        {
            return difficulty switch
            {
                DifficultyLevel.Normal => 0.75f,
                DifficultyLevel.Hard => 1.0f,
                DifficultyLevel.Difficult => 1.75f,
                DifficultyLevel.Unfair => 2.75f,
                _ => 1.0f
            };
        }

        /// <summary>
        /// Enemy damage: Normal 75%, Hard 100%, Difficult 150%, Unfair 225%.
        /// </summary>
        public static float GetEnemyDamageMultiplier(DifficultyLevel difficulty)
        {
            return difficulty switch
            {
                DifficultyLevel.Normal => 0.75f,
                DifficultyLevel.Hard => 1.0f,
                DifficultyLevel.Difficult => 1.5f,
                DifficultyLevel.Unfair => 2.25f,
                _ => 1.0f
            };
        }

        /// <summary>
        /// Enemy accuracy (percentage points): Normal +0, Hard +5, Difficult +10, Unfair +15,
        /// never raising it above 95% (MaxEnemyAccuracy).
        /// </summary>
        public static int GetEnemyAccuracyBonus(DifficultyLevel difficulty)
        {
            return difficulty switch
            {
                DifficultyLevel.Hard => 5,
                DifficultyLevel.Difficult => 10,
                DifficultyLevel.Unfair => 15,
                _ => 0
            };
        }

        /// <summary>
        /// Enemy crit chance (percentage points): Normal and Hard +0, Difficult +5, Unfair +10.
        /// </summary>
        public static int GetEnemyCritBonus(DifficultyLevel difficulty)
        {
            return difficulty switch
            {
                DifficultyLevel.Difficult => 5,
                DifficultyLevel.Unfair => 10,
                _ => 0
            };
        }

        /// <summary>
        /// Enemy speed: Normal and Hard +0, Difficult +2, Unfair +4.
        /// </summary>
        public static int GetEnemySpeedBonus(DifficultyLevel difficulty)
        {
            return difficulty switch
            {
                DifficultyLevel.Difficult => 2,
                DifficultyLevel.Unfair => 4,
                _ => 0
            };
        }

        /// <summary>
        /// Enemy defense: Normal and Hard 100%, Difficult 120%, Unfair 140%.
        /// </summary>
        public static float GetEnemyDefenseMultiplier(DifficultyLevel difficulty)
        {
            return difficulty switch
            {
                DifficultyLevel.Difficult => 1.2f,
                DifficultyLevel.Unfair => 1.4f,
                _ => 1.0f
            };
        }

        /// <summary>
        /// The difficulty the tutorial fight (Skarn) is played at - capped at Hard (decided
        /// 2026-11-23): Normal stays Normal, everything else plays as Hard.
        /// </summary>
        public static DifficultyLevel GetTutorialDifficulty(DifficultyLevel difficulty)
        {
            return difficulty == DifficultyLevel.Normal ? DifficultyLevel.Normal : DifficultyLevel.Hard;
        }

        #endregion

        #region Applying difficulty in play

        /// <summary>
        /// Make an enemy as strong as this difficulty says, on top of the stats its level gave
        /// it: health x GetEnemyHealthMultiplier, damage x GetEnemyDamageMultiplier, defense x
        /// GetEnemyDefenseMultiplier (all rounded up), plus the accuracy (capped at 95%), crit and
        /// speed bonuses. Done once per enemy (EnemyBase.DifficultyApplied), so a boss that is
        /// fought again after a loss doesn't grow every attempt. Called by CombatManager when a
        /// fight starts and when an enemy joins one (bosses and summons too).
        /// A boss mechanic that sets its own number at fight start (the Captain's crit chance)
        /// runs after this, so its own number wins.
        /// </summary>
        public void ApplyDifficultyToEnemy(Entities.Enemies.EnemyBase enemy)
        {
            if (enemy == null || enemy.DifficultyApplied) return;

            bool atFullHealth = enemy.Health >= enemy.MaxHealth;

            enemy.MaxHealth = ScaleForDifficulty(enemy.MaxHealth);
            enemy.Health = atFullHealth
                ? enemy.MaxHealth
                : Math.Min(enemy.MaxHealth, ScaleForDifficulty(enemy.Health));
            enemy.MinDamage = ScaleDamageForDifficulty(enemy.MinDamage);
            enemy.MaxDamage = ScaleDamageForDifficulty(enemy.MaxDamage);

            enemy.Defense = ScaleUp(enemy.Defense, GetEnemyDefenseMultiplier(_gameDifficulty));
            enemy.Speed += GetEnemySpeedBonus(_gameDifficulty);
            enemy.CritChance = Math.Min(100, enemy.CritChance + GetEnemyCritBonus(_gameDifficulty));

            int accuracyBonus = GetEnemyAccuracyBonus(_gameDifficulty);
            if (accuracyBonus > 0 && enemy.Accuracy < MaxEnemyAccuracy)
            {
                enemy.Accuracy = Math.Min(MaxEnemyAccuracy, enemy.Accuracy + accuracyBonus);
            }

            enemy.DifficultyApplied = true;
        }

        /// <summary>
        /// A health number multiplied by the difficulty's enemy health multiplier, rounded up,
        /// never below 1 (for a number above 0). Also used for the Librarian-Monk's shield.
        /// </summary>
        public int ScaleForDifficulty(int value)
        {
            return ScaleUp(value, _healthMultiplier);
        }

        /// <summary>
        /// A damage number multiplied by the difficulty's enemy damage multiplier, rounded up,
        /// never below 1 (for a number above 0).
        /// </summary>
        public int ScaleDamageForDifficulty(int value)
        {
            return ScaleUp(value, _damageMultiplier);
        }

        private static int ScaleUp(int value, float multiplier)
        {
            if (value <= 0) return value;
            // Rounded to 6 places first so floating-point noise (1.2f is 1.2000000476...) doesn't
            // round a whole number up
            return Math.Max(1, (int)Math.Ceiling(Math.Round(value * (double)(decimal)multiplier, 6)));
        }

        /// <summary>
        /// The XP multiplier for this difficulty (Normal 80%, Hard 100%, Difficult 130%, Unfair
        /// 160%). The numbers live in LevelingSystem.GetDifficultyMultiplier - one place for them.
        /// </summary>
        public float GetXPMultiplier()
        {
            return LevelingSystem.GetDifficultyMultiplier(_gameDifficulty);
        }

        /// <summary>
        /// The gold multiplier for this difficulty (Normal and Hard 100%, Difficult 125%,
        /// Unfair 150% - decided 2026-11-23).
        /// </summary>
        public float GetGoldMultiplier()
        {
            return _gameDifficulty switch
            {
                DifficultyLevel.Normal => 1.0f,
                DifficultyLevel.Hard => 1.0f,
                DifficultyLevel.Difficult => 1.25f,  // +25% gold
                DifficultyLevel.Unfair => 1.5f,      // +50% gold
                _ => 1.0f
            };
        }

        #endregion

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
            float scaledHealth = baseHealth * levelMultiplier * _healthMultiplier;

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
            float scaledDamage = baseDamage * levelMultiplier * _damageMultiplier;

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
            float scaledXP = baseXP * levelMultiplier * GetXPMultiplier();

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
            float averageGold = baseGold * levelMultiplier * GetGoldMultiplier();

            // Add variance: ±25%
            float minGold = averageGold * 0.75f;
            float maxGold = averageGold * 1.25f;

            return rng.Next((int)minGold, (int)maxGold + 1);
        }

        #endregion

        #region Boss Scaling

        // The boss multipliers on top of a boss's hand-set level-1 stats (decided 2026-11-10)
        public const double BossHealthMultiplier = 3.0;
        public const double BossDamageMultiplier = 1.5;

        /// <summary>
        /// A boss's health at a level, from its level-1 health: x(1 + 10% per level above 1) x3.
        /// No difficulty here - CombatManager applies it once, afterwards (ApplyDifficultyToEnemy).
        /// </summary>
        public static int BossHealthAtLevel(int level1Health, int level)
        {
            return GrowForBoss(level1Health, level, HEALTH_SCALING_PER_LEVEL, BossHealthMultiplier);
        }

        /// <summary>
        /// A boss's damage (min or max) at a level, from its level-1 damage:
        /// x(1 + 8% per level above 1) x1.5. No difficulty here, as above.
        /// </summary>
        public static int BossDamageAtLevel(int level1Damage, int level)
        {
            return GrowForBoss(level1Damage, level, DAMAGE_SCALING_PER_LEVEL, BossDamageMultiplier);
        }

        /// <summary>
        /// A flat-growth boss stat (defense, speed) at a level: the level-1 value + growth per
        /// level above 1, rounded to the nearest whole number.
        /// </summary>
        public static int BossStatAtLevel(int level1Value, double growthPerLevel, int level)
        {
            int levelsAbove = Math.Max(0, level - 1);
            return (int)Math.Round(level1Value + growthPerLevel * levelsAbove, MidpointRounding.AwayFromZero);
        }

        private static int GrowForBoss(int level1Value, int level, float perLevel, double bossMultiplier)
        {
            if (level1Value <= 0) return level1Value;
            int levelsAbove = Math.Max(0, level - 1);
            double grown = level1Value * (1.0 + levelsAbove * (double)(decimal)perLevel) * bossMultiplier;
            // Rounded to 6 places first so floating-point noise (77.00000001) doesn't round up a whole point
            return Math.Max(1, (int)Math.Ceiling(Math.Round(grown, 6)));
        }

        // The methods below scale a single number by party level AND difficulty together. Boss
        // fights don't use them (bosses grow from their level-1 stats above, and the difficulty is
        // applied once by CombatManager); kept for other callers and tests.

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
                DifficultyLevel.Normal => 0.8f,       // 20% worse drops
                DifficultyLevel.Hard => 1.0f,
                DifficultyLevel.Difficult => 1.3f,   // 30% better drops
                DifficultyLevel.Unfair => 1.6f,      // 60% better drops
                _ => 1.0f
            };
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Get the current game difficulty
        /// </summary>
        public DifficultyLevel GetGameDifficulty()
        {
            return _gameDifficulty;
        }

        /// <summary>
        /// The enemy health multiplier for this difficulty
        /// </summary>
        public float GetHealthMultiplier()
        {
            return _healthMultiplier;
        }

        /// <summary>
        /// The enemy damage multiplier for this difficulty
        /// </summary>
        public float GetDamageMultiplier()
        {
            return _damageMultiplier;
        }

        /// <summary>
        /// Get a display-friendly difficulty description (short; the full text for the
        /// difficulty-select screen is GameStartup.GetDifficultyDescription).
        /// TODO: the flavour words are placeholders for the writing pass.
        /// </summary>
        public string GetDifficultyDescription()
        {
            int health = (int)Math.Round(_healthMultiplier * 100);
            int damage = (int)Math.Round(_damageMultiplier * 100);
            string flavour = _gameDifficulty switch
            {
                DifficultyLevel.Normal => "good for learning",
                DifficultyLevel.Hard => "the intended experience",
                DifficultyLevel.Difficult => "for experienced players",
                DifficultyLevel.Unfair => "brutal challenge",
                _ => ""
            };
            return $"{_gameDifficulty} ({health}% enemy health, {damage}% enemy damage, {flavour})";
        }

        /// <summary>
        /// Calculate the "effective level" of an enemy for display
        /// Accounts for enemy type (normal/elite/boss) and scaling
        /// Use CalculatePartyLevel() to get the effective party level first
        /// </summary>
        public int GetEffectiveEnemyLevel(int baseLevel, int partyLevel, bool isElite = false, bool isBoss = false)
        {
            // Bosses: the party's level + 5, capped at 45 (the real rule, used by CombatManager)
            if (isBoss)
            {
                return GetBossLevel(partyLevel);
            }

            // In level-scaling games, enemies are always at party level
            int effectiveLevel = partyLevel;

            if (isElite)
            {
                effectiveLevel += 2; // Elites are shown as +2 levels
            }

            return Math.Min(effectiveLevel, 100); // Cap at level 100
        }

        /// <summary>
        /// How many levels above the party a boss is (decided 2026-10-14).
        /// </summary>
        public const int BossLevelsAboveParty = 5;

        /// <summary>
        /// A boss's level for a party at this level (CalculatePartyLevel - the plain average of
        /// the fighting party): the party level + 5, between 1 and LevelCaps.MaxBossLevel (45).
        /// Bosses with HasFixedLevel (Skarn, the tutorial) don't use this.
        /// </summary>
        public int GetBossLevel(int partyLevel)
        {
            return Math.Clamp(partyLevel + BossLevelsAboveParty, 1, GameLogic.Items.LevelCaps.MaxBossLevel);
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
                    DifficultyLevel.Normal => "[BOSS] Challenging",
                    DifficultyLevel.Hard => "[BOSS] Dangerous",
                    DifficultyLevel.Difficult => "[BOSS] Deadly",
                    DifficultyLevel.Unfair => "[BOSS] Lethal",
                    _ => "[BOSS]"
                };
            }
            else if (isElite)
            {
                return _gameDifficulty switch
                {
                    DifficultyLevel.Normal => "[Elite] Moderate",
                    DifficultyLevel.Hard => "[Elite] Tough",
                    DifficultyLevel.Difficult => "[Elite] Dangerous",
                    DifficultyLevel.Unfair => "[Elite] Deadly",
                    _ => "[Elite]"
                };
            }
            else
            {
                return _gameDifficulty switch
                {
                    DifficultyLevel.Normal => "Manageable",
                    DifficultyLevel.Hard => "Fair Fight",
                    DifficultyLevel.Difficult => "Tough",
                    DifficultyLevel.Unfair => "Dangerous",
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