using System;
using System.Linq;
using Xunit;
using GameLogic.Systems;

namespace GameLogic.Tests
{
    /// <summary>
    /// Unit tests for DifficultyScaler
    /// Tests party level calculations, enemy scaling, boss/elite scaling, and utility methods
    /// </summary>
    public class DifficultyScalerTests
    {
        #region Party Level Calculation Tests

        [Fact]
        public void CalculatePartyLevel_SoloPlayer_ReturnsPlayerLevel()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            int partyLevel = scaler.CalculatePartyLevel(10);

            // Assert
            Assert.Equal(10, partyLevel);
        }

        [Fact]
        public void CalculatePartyLevel_PlayerWithOneCompanion_ReturnsWeightedAverage()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            int partyLevel = scaler.CalculatePartyLevel(playerLevel: 10, companionLevels: 8);

            // Assert - Player 50%, Companion 50%: (10 * 0.5) + (8 * 0.5) = 9 (ceiling)
            Assert.Equal(9, partyLevel);
        }

        [Fact]
        public void CalculatePartyLevel_PlayerWithTwoCompanions_ReturnsWeightedAverage()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            int partyLevel = scaler.CalculatePartyLevel(playerLevel: 10, companionLevels: new[] { 8, 6 });

            // Assert - Player 50%, Each companion 25%: (10 * 0.5) + (8 * 0.25) + (6 * 0.25) = 8.5 = 9 (ceiling)
            Assert.Equal(9, partyLevel);
        }

        [Fact]
        public void CalculatePartyLevel_PlayerWithThreeCompanions_ReturnsWeightedAverage()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            int partyLevel = scaler.CalculatePartyLevel(playerLevel: 12, companionLevels: new[] { 10, 10, 10 });

            // Assert - Player 50%, Each companion ~16.67%: (12 * 0.5) + (10 * 0.5) = 11 (ceiling)
            Assert.Equal(11, partyLevel);
        }

        [Fact]
        public void CalculatePartyLevel_AllSameLevel_ReturnsPlayerLevel()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            int partyLevel = scaler.CalculatePartyLevel(playerLevel: 15, companionLevels: new[] { 15, 15, 15 });

            // Assert
            Assert.Equal(15, partyLevel);
        }

        [Fact]
        public void CalculatePartyLevel_NullCompanions_ReturnsPlayerLevel()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            int partyLevel = scaler.CalculatePartyLevel(20, null);

            // Assert
            Assert.Equal(20, partyLevel);
        }

        [Fact]
        public void CalculatePartyLevelWithPenalty_NoUnderLeveledCompanions_ReturnsNormalPartyLevel()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            int partyLevel = scaler.CalculatePartyLevelWithPenalty(playerLevel: 10, companionLevels: new[] { 9, 8 });

            // Assert - No penalty since companions are within 5 levels
            int expectedLevel = scaler.CalculatePartyLevel(10, 9, 8);
            Assert.Equal(expectedLevel, partyLevel);
        }

        [Fact]
        public void CalculatePartyLevelWithPenalty_OneUnderLeveledCompanion_AppliesPenalty()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            int partyLevel = scaler.CalculatePartyLevelWithPenalty(playerLevel: 20, companionLevels: new[] { 10 });

            // Assert - Companion is 10 levels below (>5), so -1 penalty
            int normalLevel = scaler.CalculatePartyLevel(20, 10);
            Assert.Equal(normalLevel - 1, partyLevel);
        }

        [Fact]
        public void CalculatePartyLevelWithPenalty_MultipleUnderLeveledCompanions_AppliesMultiplePenalties()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            int partyLevel = scaler.CalculatePartyLevelWithPenalty(playerLevel: 20, companionLevels: new[] { 10, 12 });

            // Assert - Both companions are >5 levels below, so -2 penalty
            int normalLevel = scaler.CalculatePartyLevel(20, 10, 12);
            Assert.Equal(normalLevel - 2, partyLevel);
        }

        [Fact]
        public void CalculatePartyLevelWithPenalty_NeverGoesBelowPlayerLevel()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act - Many under-leveled companions
            int partyLevel = scaler.CalculatePartyLevelWithPenalty(playerLevel: 50, companionLevels: new[] { 1, 1, 1, 1, 1 });

            // Assert - Should never go below player level
            Assert.True(partyLevel >= 50);
        }

        [Fact]
        public void CalculatePartyLevelBalanced_WellBalancedParty_ReceivesBonus()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act - All companions within 3 levels of each other and player
            int partyLevel = scaler.CalculatePartyLevelBalanced(playerLevel: 10, companionLevels: new[] { 9, 10, 8 });

            // Assert - Should get +1 bonus
            int normalLevel = scaler.CalculatePartyLevel(10, 9, 10, 8);
            Assert.Equal(normalLevel + 1, partyLevel);
        }

        [Fact]
        public void CalculatePartyLevelBalanced_UnbalancedParty_NoBonus()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act - Large level spread
            int partyLevel = scaler.CalculatePartyLevelBalanced(playerLevel: 10, companionLevels: new[] { 15, 5 });

            // Assert - No bonus due to 10-level spread
            int normalLevel = scaler.CalculatePartyLevel(10, 15, 5);
            Assert.Equal(normalLevel, partyLevel);
        }

        #endregion

        #region Enemy Scaling Tests

        [Fact]
        public void ScaleEnemyHealth_Level1Normal_ReturnsBaseHealth()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            int health = scaler.ScaleEnemyHealth(baseHealth: 100, partyLevel: 1);

            // Assert - At level 1, should be base health
            Assert.Equal(100, health);
        }

        [Fact]
        public void ScaleEnemyHealth_IncreasesWithLevel()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            int health1 = scaler.ScaleEnemyHealth(100, partyLevel: 1);
            int health10 = scaler.ScaleEnemyHealth(100, partyLevel: 10);
            int health50 = scaler.ScaleEnemyHealth(100, partyLevel: 50);

            // Assert
            Assert.True(health10 > health1);
            Assert.True(health50 > health10);
        }

        [Fact]
        public void ScaleEnemyHealth_EasyDifficulty_ReducesHealth()
        {
            // Arrange
            var easyScaler = new DifficultyScaler(GameDifficulty.Easy);
            var normalScaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            int easyHealth = easyScaler.ScaleEnemyHealth(100, partyLevel: 10);
            int normalHealth = normalScaler.ScaleEnemyHealth(100, partyLevel: 10);

            // Assert
            Assert.True(easyHealth < normalHealth);
        }

        [Fact]
        public void ScaleEnemyHealth_HardDifficulty_IncreasesHealth()
        {
            // Arrange
            var hardScaler = new DifficultyScaler(GameDifficulty.Hard);
            var normalScaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            int hardHealth = hardScaler.ScaleEnemyHealth(100, partyLevel: 10);
            int normalHealth = normalScaler.ScaleEnemyHealth(100, partyLevel: 10);

            // Assert
            Assert.True(hardHealth > normalHealth);
        }

        [Fact]
        public void ScaleEnemyDamage_Level1Normal_ReturnsBaseDamage()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            int damage = scaler.ScaleEnemyDamage(baseDamage: 10, partyLevel: 1);

            // Assert
            Assert.Equal(10, damage);
        }

        [Fact]
        public void ScaleEnemyDamage_IncreasesWithLevel()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            int damage1 = scaler.ScaleEnemyDamage(10, partyLevel: 1);
            int damage10 = scaler.ScaleEnemyDamage(10, partyLevel: 10);
            int damage50 = scaler.ScaleEnemyDamage(10, partyLevel: 50);

            // Assert
            Assert.True(damage10 > damage1);
            Assert.True(damage50 > damage10);
        }

        [Fact]
        public void ScaleEnemyXP_Level1Normal_ReturnsBaseXP()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            int xp = scaler.ScaleEnemyXP(baseXP: 50, partyLevel: 1);

            // Assert
            Assert.Equal(50, xp);
        }

        [Fact]
        public void ScaleEnemyXP_IncreasesWithLevel()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            int xp1 = scaler.ScaleEnemyXP(50, partyLevel: 1);
            int xp10 = scaler.ScaleEnemyXP(50, partyLevel: 10);
            int xp50 = scaler.ScaleEnemyXP(50, partyLevel: 50);

            // Assert
            Assert.True(xp10 > xp1);
            Assert.True(xp50 > xp10);
        }

        [Fact]
        public void ScaleEnemyXP_HardDifficulty_GivesBonus()
        {
            // Arrange
            var hardScaler = new DifficultyScaler(GameDifficulty.Hard);
            var normalScaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            int hardXP = hardScaler.ScaleEnemyXP(50, partyLevel: 10);
            int normalXP = normalScaler.ScaleEnemyXP(50, partyLevel: 10);

            // Assert - Hard should give +25% XP
            Assert.True(hardXP > normalXP);
        }

        [Fact]
        public void ScaleEnemyXP_EasyDifficulty_ReducesXP()
        {
            // Arrange
            var easyScaler = new DifficultyScaler(GameDifficulty.Easy);
            var normalScaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            int easyXP = easyScaler.ScaleEnemyXP(50, partyLevel: 10);
            int normalXP = normalScaler.ScaleEnemyXP(50, partyLevel: 10);

            // Assert - Easy should give 80% XP
            Assert.True(easyXP < normalXP);
        }

        [Fact]
        public void ScaleEnemyGold_ReturnsPositiveValue()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);
            var rng = new Random(42);

            // Act
            int gold = scaler.ScaleEnemyGold(baseGold: 20, partyLevel: 5, rng: rng);

            // Assert
            Assert.True(gold > 0);
        }

        [Fact]
        public void ScaleEnemyGold_HasVariance()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);
            var rng = new Random(42);
            var goldResults = new System.Collections.Generic.HashSet<int>();

            // Act - Generate 20 gold rewards
            for (int i = 0; i < 20; i++)
            {
                int gold = scaler.ScaleEnemyGold(20, partyLevel: 5, rng: rng);
                goldResults.Add(gold);
            }

            // Assert - Should have variance
            Assert.True(goldResults.Count > 1);
        }

        [Fact]
        public void ScaleEnemyGold_IncreasesWithLevel()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);
            var rng = new Random(42);

            // Act - Average over multiple rolls
            int avgGold1 = 0;
            int avgGold50 = 0;
            for (int i = 0; i < 100; i++)
            {
                avgGold1 += scaler.ScaleEnemyGold(20, partyLevel: 1, rng: rng);
                avgGold50 += scaler.ScaleEnemyGold(20, partyLevel: 50, rng: rng);
            }
            avgGold1 /= 100;
            avgGold50 /= 100;

            // Assert
            Assert.True(avgGold50 > avgGold1);
        }

        [Fact]
        public void ScaleEnemyGold_NightmareDifficulty_GivesBonus()
        {
            // Arrange
            var nightmareScaler = new DifficultyScaler(GameDifficulty.Nightmare);
            var normalScaler = new DifficultyScaler(GameDifficulty.Normal);
            var rng = new Random(42);

            // Act - Average over multiple rolls
            int nightmareGold = 0;
            int normalGold = 0;
            for (int i = 0; i < 100; i++)
            {
                nightmareGold += nightmareScaler.ScaleEnemyGold(20, partyLevel: 10, rng: rng);
                normalGold += normalScaler.ScaleEnemyGold(20, partyLevel: 10, rng: rng);
            }

            // Assert
            Assert.True(nightmareGold > normalGold);
        }

        #endregion

        #region Boss Scaling Tests

        [Fact]
        public void ScaleBossHealth_IsThreeTimesRegular()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            int regularHealth = scaler.ScaleEnemyHealth(100, partyLevel: 10);
            int bossHealth = scaler.ScaleBossHealth(100, partyLevel: 10);

            // Assert
            Assert.Equal(regularHealth * 3, bossHealth);
        }

        [Fact]
        public void ScaleBossDamage_Is1Point5TimesRegular()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            int regularDamage = scaler.ScaleEnemyDamage(10, partyLevel: 10);
            int bossDamage = scaler.ScaleBossDamage(10, partyLevel: 10);

            // Assert
            Assert.Equal((int)(regularDamage * 1.5f), bossDamage);
        }

        [Fact]
        public void ScaleBossXP_IsFiveTimesRegular()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            int regularXP = scaler.ScaleEnemyXP(50, partyLevel: 10);
            int bossXP = scaler.ScaleBossXP(50, partyLevel: 10);

            // Assert
            Assert.Equal(regularXP * 5, bossXP);
        }

        [Fact]
        public void ScaleBossGold_IsMultipleTimesRegular()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);
            var rng = new Random(42);

            // Act - Average over multiple rolls
            int avgRegularGold = 0;
            int avgBossGold = 0;
            for (int i = 0; i < 100; i++)
            {
                avgRegularGold += scaler.ScaleEnemyGold(20, partyLevel: 10, rng: rng);
                avgBossGold += scaler.ScaleBossGold(20, partyLevel: 10, rng: rng);
            }

            // Assert - Boss gold should be significantly more (3-5x)
            Assert.True(avgBossGold > avgRegularGold * 2.5);
        }

        #endregion

        #region Elite Scaling Tests

        [Fact]
        public void ScaleEliteHealth_Is1Point75TimesRegular()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            int regularHealth = scaler.ScaleEnemyHealth(100, partyLevel: 10);
            int eliteHealth = scaler.ScaleEliteHealth(100, partyLevel: 10);

            // Assert
            Assert.Equal((int)(regularHealth * 1.75f), eliteHealth);
        }

        [Fact]
        public void ScaleEliteDamage_Is1Point25TimesRegular()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            int regularDamage = scaler.ScaleEnemyDamage(10, partyLevel: 10);
            int eliteDamage = scaler.ScaleEliteDamage(10, partyLevel: 10);

            // Assert
            Assert.Equal((int)(regularDamage * 1.25f), eliteDamage);
        }

        [Fact]
        public void ScaleEliteXP_IsTwoTimesRegular()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            int regularXP = scaler.ScaleEnemyXP(50, partyLevel: 10);
            int eliteXP = scaler.ScaleEliteXP(50, partyLevel: 10);

            // Assert
            Assert.Equal(regularXP * 2, eliteXP);
        }

        [Fact]
        public void ScaleEliteGold_IsBetweenRegularAndBoss()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);
            var rng = new Random(42);

            // Act - Average over multiple rolls
            int avgRegularGold = 0;
            int avgEliteGold = 0;
            for (int i = 0; i < 100; i++)
            {
                avgRegularGold += scaler.ScaleEnemyGold(20, partyLevel: 10, rng: rng);
                avgEliteGold += scaler.ScaleEliteGold(20, partyLevel: 10, rng: rng);
            }

            // Assert - Elite should be roughly 1.5-2x regular
            Assert.True(avgEliteGold > avgRegularGold);
            Assert.True(avgEliteGold < avgRegularGold * 2.5);
        }

        #endregion

        #region Loot Quality Tests

        [Fact]
        public void GetScaledItemLevel_ReturnsValueNearPartyLevel()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);
            var rng = new Random(42);

            // Act
            int itemLevel = scaler.GetScaledItemLevel(partyLevel: 20, rng: rng, variance: 3);

            // Assert - Should be within ±3 of party level
            Assert.InRange(itemLevel, 17, 23);
        }

        [Fact]
        public void GetScaledItemLevel_ClampsToValidRange()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);
            var rng = new Random(42);

            // Act
            int lowLevel = scaler.GetScaledItemLevel(partyLevel: 1, rng: rng, variance: 10);
            int highLevel = scaler.GetScaledItemLevel(partyLevel: 100, rng: rng, variance: 10);

            // Assert - Should clamp to 1-100
            Assert.InRange(lowLevel, 1, 100);
            Assert.InRange(highLevel, 1, 100);
        }

        [Fact]
        public void GetLootQualityModifier_Normal_ReturnsOne()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            float modifier = scaler.GetLootQualityModifier();

            // Assert
            Assert.Equal(1.0f, modifier);
        }

        [Fact]
        public void GetLootQualityModifier_Easy_ReducesQuality()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Easy);

            // Act
            float modifier = scaler.GetLootQualityModifier();

            // Assert
            Assert.True(modifier < 1.0f);
        }

        [Fact]
        public void GetLootQualityModifier_Hard_IncreasesQuality()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Hard);

            // Act
            float modifier = scaler.GetLootQualityModifier();

            // Assert
            Assert.True(modifier > 1.0f);
        }

        [Fact]
        public void GetLootQualityModifier_Nightmare_MaximumQuality()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Nightmare);

            // Act
            float modifier = scaler.GetLootQualityModifier();

            // Assert
            Assert.Equal(1.6f, modifier);
        }

        #endregion

        #region Utility Method Tests

        [Fact]
        public void GetGameDifficulty_ReturnsSetDifficulty()
        {
            // Arrange
            var easyScaler = new DifficultyScaler(GameDifficulty.Easy);
            var hardScaler = new DifficultyScaler(GameDifficulty.Hard);

            // Act & Assert
            Assert.Equal(GameDifficulty.Easy, easyScaler.GetGameDifficulty());
            Assert.Equal(GameDifficulty.Hard, hardScaler.GetGameDifficulty());
        }

        [Fact]
        public void GetDifficultyMultiplier_ReturnsCorrectMultipliers()
        {
            // Arrange
            var easyScaler = new DifficultyScaler(GameDifficulty.Easy);
            var normalScaler = new DifficultyScaler(GameDifficulty.Normal);
            var hardScaler = new DifficultyScaler(GameDifficulty.Hard);
            var nightmareScaler = new DifficultyScaler(GameDifficulty.Nightmare);

            // Act & Assert
            Assert.Equal(0.75f, easyScaler.GetDifficultyMultiplier());
            Assert.Equal(1.0f, normalScaler.GetDifficultyMultiplier());
            Assert.Equal(1.5f, hardScaler.GetDifficultyMultiplier());
            Assert.Equal(2.0f, nightmareScaler.GetDifficultyMultiplier());
        }

        [Fact]
        public void GetDifficultyDescription_ReturnsDescriptiveText()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Hard);

            // Act
            string description = scaler.GetDifficultyDescription();

            // Assert
            Assert.Contains("Hard", description);
            Assert.Contains("150%", description);
        }

        [Fact]
        public void GetEffectiveEnemyLevel_RegularEnemy_ReturnsPartyLevel()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            int level = scaler.GetEffectiveEnemyLevel(baseLevel: 1, partyLevel: 20);

            // Assert
            Assert.Equal(20, level);
        }

        [Fact]
        public void GetEffectiveEnemyLevel_EliteEnemy_AddsTwo()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            int level = scaler.GetEffectiveEnemyLevel(baseLevel: 1, partyLevel: 20, isElite: true);

            // Assert
            Assert.Equal(22, level);
        }

        [Fact]
        public void GetEffectiveEnemyLevel_Boss_AddsFive()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            int level = scaler.GetEffectiveEnemyLevel(baseLevel: 1, partyLevel: 20, isBoss: true);

            // Assert
            Assert.Equal(25, level);
        }

        [Fact]
        public void GetEffectiveEnemyLevel_CapsAt100()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            int level = scaler.GetEffectiveEnemyLevel(baseLevel: 1, partyLevel: 99, isBoss: true);

            // Assert - 99 + 5 = 104, but should cap at 100
            Assert.Equal(100, level);
        }

        [Fact]
        public void GetDangerRating_RegularEnemy_ReturnsBasicRating()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            string rating = scaler.GetDangerRating();

            // Assert
            Assert.Equal("Normal", rating);
        }

        [Fact]
        public void GetDangerRating_EliteEnemy_ContainsEliteTag()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Hard);

            // Act
            string rating = scaler.GetDangerRating(isElite: true);

            // Assert
            Assert.Contains("[Elite]", rating);
        }

        [Fact]
        public void GetDangerRating_Boss_ContainsBossTag()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Nightmare);

            // Act
            string rating = scaler.GetDangerRating(isBoss: true);

            // Assert
            Assert.Contains("[BOSS]", rating);
        }

        #endregion

        #region Party Composition Tests

        [Fact]
        public void GetPartyCompositionInfo_SoloPlayer_ReturnsSoloMessage()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            string info = scaler.GetPartyCompositionInfo(playerLevel: 10);

            // Assert
            Assert.Contains("Solo", info);
            Assert.Contains("Level 10", info);
        }

        [Fact]
        public void GetPartyCompositionInfo_BalancedParty_ShowsBalanced()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            string info = scaler.GetPartyCompositionInfo(playerLevel: 10, companionLevels: new[] { 9, 10, 8 });

            // Assert
            Assert.Contains("BALANCED", info);
        }

        [Fact]
        public void GetPartyCompositionInfo_UnderLeveledCompanions_ShowsWeakParty()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            string info = scaler.GetPartyCompositionInfo(playerLevel: 20, companionLevels: new[] { 10, 12 });

            // Assert
            Assert.Contains("WEAK PARTY", info);
        }

        [Fact]
        public void GetRecommendedCompanionLevel_ReturnsPlayerLevelMinus3()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            int recommended = scaler.GetRecommendedCompanionLevel(playerLevel: 20);

            // Assert
            Assert.Equal(17, recommended);
        }

        [Fact]
        public void GetRecommendedCompanionLevel_MinimumLevel1()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            int recommended = scaler.GetRecommendedCompanionLevel(playerLevel: 1);

            // Assert
            Assert.Equal(1, recommended);
        }

        [Fact]
        public void IsCompanionUnderLeveled_WithinRange_ReturnsFalse()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            bool underLeveled = scaler.IsCompanionUnderLeveled(companionLevel: 15, playerLevel: 20);

            // Assert
            Assert.False(underLeveled);
        }

        [Fact]
        public void IsCompanionUnderLeveled_MoreThan5Below_ReturnsTrue()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            bool underLeveled = scaler.IsCompanionUnderLeveled(companionLevel: 10, playerLevel: 20);

            // Assert
            Assert.True(underLeveled);
        }

        [Fact]
        public void IsCompanionUnderLeveled_Exactly5Below_ReturnsFalse()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            bool underLeveled = scaler.IsCompanionUnderLeveled(companionLevel: 15, playerLevel: 20);

            // Assert
            Assert.False(underLeveled);
        }

        [Fact]
        public void IsCompanionUnderLeveled_Exactly6Below_ReturnsTrue()
        {
            // Arrange
            var scaler = new DifficultyScaler(GameDifficulty.Normal);

            // Act
            bool underLeveled = scaler.IsCompanionUnderLeveled(companionLevel: 14, playerLevel: 20);

            // Assert
            Assert.True(underLeveled);
        }

        #endregion
    }
}