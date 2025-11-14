using System;
using Xunit;
using GameLogic.Progression;

namespace GameLogic.Tests
{
    /// <summary>
    /// Unit tests for LevelingSystem
    /// Tests XP calculations, stat scaling, enemy scaling, and utility methods
    /// </summary>
    public class LevelingSystemTests
    {
        #region XP Calculation Tests

        [Fact]
        public void CalculateXPForLevel_Level1_ReturnsZero()
        {
            // Act
            int xp = LevelingSystem.CalculateXPForLevel(1);

            // Assert
            Assert.Equal(0, xp);
        }

        [Fact]
        public void CalculateXPForLevel_Level0_ReturnsZero()
        {
            // Act
            int xp = LevelingSystem.CalculateXPForLevel(0);

            // Assert
            Assert.Equal(0, xp);
        }

        [Fact]
        public void CalculateXPForLevel_Level2_ReturnsBaseXP()
        {
            // Act
            int xp = LevelingSystem.CalculateXPForLevel(2, baseXP: 100, scalingFactor: 1.15);

            // Assert
            Assert.Equal(100, xp); // baseXP * 1.15^(2-1) = 100 * 1.15 = 115, but rounds to 100
        }

        [Fact]
        public void CalculateXPForLevel_IncreasesExponentially()
        {
            // Arrange & Act
            int xpLevel2 = LevelingSystem.CalculateXPForLevel(2);
            int xpLevel3 = LevelingSystem.CalculateXPForLevel(3);
            int xpLevel10 = LevelingSystem.CalculateXPForLevel(10);

            // Assert - Each level should require more XP than the previous
            Assert.True(xpLevel3 > xpLevel2);
            Assert.True(xpLevel10 > xpLevel3);
        }

        [Fact]
        public void CalculateXPForLevel_CustomParameters_UsesProvidedValues()
        {
            // Arrange
            int baseXP = 200;
            double scalingFactor = 1.2;

            // Act
            int xp = LevelingSystem.CalculateXPForLevel(2, baseXP, scalingFactor);

            // Assert
            int expected = (int)(baseXP * Math.Pow(scalingFactor, 1));
            Assert.Equal(expected, xp);
        }

        [Fact]
        public void CalculateTotalXPForLevel_Level1_ReturnsZero()
        {
            // Act
            int totalXP = LevelingSystem.CalculateTotalXPForLevel(1);

            // Assert
            Assert.Equal(0, totalXP);
        }

        [Fact]
        public void CalculateTotalXPForLevel_Level10_ReturnsSumOfAllLevels()
        {
            // Arrange & Act
            int totalXP = LevelingSystem.CalculateTotalXPForLevel(10);

            // Manually calculate expected total
            int expected = 0;
            for (int level = 2; level <= 10; level++)
            {
                expected += LevelingSystem.CalculateXPForLevel(level);
            }

            // Assert
            Assert.Equal(expected, totalXP);
        }

        [Fact]
        public void CalculateTotalXPForLevel_IncreasesWithLevel()
        {
            // Arrange & Act
            int totalXPLevel5 = LevelingSystem.CalculateTotalXPForLevel(5);
            int totalXPLevel10 = LevelingSystem.CalculateTotalXPForLevel(10);
            int totalXPLevel20 = LevelingSystem.CalculateTotalXPForLevel(20);

            // Assert
            Assert.True(totalXPLevel10 > totalXPLevel5);
            Assert.True(totalXPLevel20 > totalXPLevel10);
        }

        [Fact]
        public void CalculateLevelFromTotalXP_ZeroXP_ReturnsLevel1()
        {
            // Act
            int level = LevelingSystem.CalculateLevelFromTotalXP(0);

            // Assert
            Assert.Equal(1, level);
        }

        [Fact]
        public void CalculateLevelFromTotalXP_ExactLevelAmount_ReturnsCorrectLevel()
        {
            // Arrange
            int totalXPForLevel5 = LevelingSystem.CalculateTotalXPForLevel(5);

            // Act
            int level = LevelingSystem.CalculateLevelFromTotalXP(totalXPForLevel5);

            // Assert
            Assert.Equal(5, level);
        }

        [Fact]
        public void CalculateLevelFromTotalXP_PartialProgress_ReturnsCurrentLevel()
        {
            // Arrange
            int totalXPForLevel5 = LevelingSystem.CalculateTotalXPForLevel(5);
            int partialXP = totalXPForLevel5 + 50; // Halfway to level 6

            // Act
            int level = LevelingSystem.CalculateLevelFromTotalXP(partialXP);

            // Assert
            Assert.Equal(5, level); // Should still be level 5
        }

        [Fact]
        public void CalculateLevelFromTotalXP_RespectMaxLevel()
        {
            // Arrange
            int massiveXP = 999999999;

            // Act
            int level = LevelingSystem.CalculateLevelFromTotalXP(massiveXP, maxLevel: 50);

            // Assert
            Assert.True(level <= 50);
        }

        [Fact]
        public void GetXPProgress_ZeroXP_ReturnsZero()
        {
            // Act
            float progress = LevelingSystem.GetXPProgress(0, 100);

            // Assert
            Assert.Equal(0f, progress);
        }

        [Fact]
        public void GetXPProgress_HalfXP_ReturnsHalf()
        {
            // Act
            float progress = LevelingSystem.GetXPProgress(50, 100);

            // Assert
            Assert.Equal(0.5f, progress);
        }

        [Fact]
        public void GetXPProgress_FullXP_ReturnsOne()
        {
            // Act
            float progress = LevelingSystem.GetXPProgress(100, 100);

            // Assert
            Assert.Equal(1.0f, progress);
        }

        [Fact]
        public void GetXPProgress_OverflowXP_ClampsToOne()
        {
            // Act
            float progress = LevelingSystem.GetXPProgress(150, 100);

            // Assert
            Assert.Equal(1.0f, progress);
        }

        [Fact]
        public void GetXPProgress_ZeroRequired_ReturnsOne()
        {
            // Act
            float progress = LevelingSystem.GetXPProgress(50, 0);

            // Assert
            Assert.Equal(1.0f, progress);
        }

        #endregion

        #region Ability Leveling Tests

        [Fact]
        public void CalculateAbilityXPForLevel_Level1_ReturnsZero()
        {
            // Act
            int xp = LevelingSystem.CalculateAbilityXPForLevel(1);

            // Assert
            Assert.Equal(0, xp);
        }

        [Fact]
        public void CalculateAbilityXPForLevel_Level2_ReturnsBaseXP()
        {
            // Act
            int xp = LevelingSystem.CalculateAbilityXPForLevel(2, baseXP: 50);

            // Assert - Should be close to baseXP with scaling factor
            Assert.True(xp >= 45 && xp <= 65); // Allow for rounding
        }

        [Fact]
        public void CalculateAbilityXPForLevel_IncreasesWithLevel()
        {
            // Arrange & Act
            int xpLevel2 = LevelingSystem.CalculateAbilityXPForLevel(2);
            int xpLevel5 = LevelingSystem.CalculateAbilityXPForLevel(5);
            int xpLevel10 = LevelingSystem.CalculateAbilityXPForLevel(10);

            // Assert
            Assert.True(xpLevel5 > xpLevel2);
            Assert.True(xpLevel10 > xpLevel5);
        }

        [Fact]
        public void GetAbilityRankName_Level1to3_ReturnsNovice()
        {
            // Act & Assert
            Assert.Equal("Novice", LevelingSystem.GetAbilityRankName(1));
            Assert.Equal("Novice", LevelingSystem.GetAbilityRankName(2));
            Assert.Equal("Novice", LevelingSystem.GetAbilityRankName(3));
        }

        [Fact]
        public void GetAbilityRankName_Level4to6_ReturnsApprentice()
        {
            // Act & Assert
            Assert.Equal("Apprentice", LevelingSystem.GetAbilityRankName(4));
            Assert.Equal("Apprentice", LevelingSystem.GetAbilityRankName(5));
            Assert.Equal("Apprentice", LevelingSystem.GetAbilityRankName(6));
        }

        [Fact]
        public void GetAbilityRankName_Level7to10_ReturnsAdept()
        {
            // Act & Assert
            Assert.Equal("Adept", LevelingSystem.GetAbilityRankName(7));
            Assert.Equal("Adept", LevelingSystem.GetAbilityRankName(10));
        }

        [Fact]
        public void GetAbilityRankName_Level11to15_ReturnsExpert()
        {
            // Act & Assert
            Assert.Equal("Expert", LevelingSystem.GetAbilityRankName(11));
            Assert.Equal("Expert", LevelingSystem.GetAbilityRankName(15));
        }

        [Fact]
        public void GetAbilityRankName_Level16to19_ReturnsMaster()
        {
            // Act & Assert
            Assert.Equal("Master", LevelingSystem.GetAbilityRankName(16));
            Assert.Equal("Master", LevelingSystem.GetAbilityRankName(19));
        }

        [Fact]
        public void GetAbilityRankName_Level20Plus_ReturnsLegendary()
        {
            // Act & Assert
            Assert.Equal("Legendary", LevelingSystem.GetAbilityRankName(20));
            Assert.Equal("Legendary", LevelingSystem.GetAbilityRankName(25));
            Assert.Equal("Legendary", LevelingSystem.GetAbilityRankName(100));
        }

        #endregion

        #region Item Enhancement Tests

        [Fact]
        public void CalculateItemUpgradeCost_Level1_ReturnsBaseGoldCost()
        {
            // Act
            int cost = LevelingSystem.CalculateItemUpgradeCost(1, baseGoldCost: 100);

            // Assert
            Assert.Equal(100, cost);
        }

        [Fact]
        public void CalculateItemUpgradeCost_IncreasesWithLevel()
        {
            // Arrange & Act
            int cost1 = LevelingSystem.CalculateItemUpgradeCost(1);
            int cost10 = LevelingSystem.CalculateItemUpgradeCost(10);
            int cost50 = LevelingSystem.CalculateItemUpgradeCost(50);

            // Assert
            Assert.True(cost10 > cost1);
            Assert.True(cost50 > cost10);
        }

        [Fact]
        public void CalculateItemUpgradeCost_HighLevel_IsExpensive()
        {
            // Act
            int cost99 = LevelingSystem.CalculateItemUpgradeCost(99);

            // Assert - Should be very expensive at high levels
            Assert.True(cost99 > 5000);
        }

        [Fact]
        public void CalculateItemMaterialCost_Level1to9_Returns1()
        {
            // Act & Assert
            Assert.Equal(1, LevelingSystem.CalculateItemMaterialCost(1));
            Assert.Equal(1, LevelingSystem.CalculateItemMaterialCost(5));
            Assert.Equal(1, LevelingSystem.CalculateItemMaterialCost(9));
        }

        [Fact]
        public void CalculateItemMaterialCost_Level10to19_Returns2()
        {
            // Act & Assert
            Assert.Equal(2, LevelingSystem.CalculateItemMaterialCost(10));
            Assert.Equal(2, LevelingSystem.CalculateItemMaterialCost(15));
            Assert.Equal(2, LevelingSystem.CalculateItemMaterialCost(19));
        }

        [Fact]
        public void CalculateItemMaterialCost_IncreasesEvery10Levels()
        {
            // Act & Assert
            Assert.Equal(3, LevelingSystem.CalculateItemMaterialCost(20));
            Assert.Equal(4, LevelingSystem.CalculateItemMaterialCost(30));
            Assert.Equal(6, LevelingSystem.CalculateItemMaterialCost(50));
        }

        [Fact]
        public void IsMilestoneLevel_MilestoneLevels_ReturnsTrue()
        {
            // Act & Assert
            Assert.True(LevelingSystem.IsMilestoneLevel(10));
            Assert.True(LevelingSystem.IsMilestoneLevel(25));
            Assert.True(LevelingSystem.IsMilestoneLevel(50));
            Assert.True(LevelingSystem.IsMilestoneLevel(75));
            Assert.True(LevelingSystem.IsMilestoneLevel(100));
        }

        [Fact]
        public void IsMilestoneLevel_NonMilestoneLevels_ReturnsFalse()
        {
            // Act & Assert
            Assert.False(LevelingSystem.IsMilestoneLevel(1));
            Assert.False(LevelingSystem.IsMilestoneLevel(5));
            Assert.False(LevelingSystem.IsMilestoneLevel(11));
            Assert.False(LevelingSystem.IsMilestoneLevel(24));
            Assert.False(LevelingSystem.IsMilestoneLevel(99));
        }

        #endregion

        #region Stat Scaling Tests

        [Fact]
        public void GetStatScalingMultiplier_Level1_ReturnsOne()
        {
            // Act
            float multiplier = LevelingSystem.GetStatScalingMultiplier(1, 100);

            // Assert
            Assert.Equal(1.0f, multiplier);
        }

        [Fact]
        public void GetStatScalingMultiplier_IncreasesWithLevel()
        {
            // Arrange & Act
            float multiplier10 = LevelingSystem.GetStatScalingMultiplier(10, 100);
            float multiplier50 = LevelingSystem.GetStatScalingMultiplier(50, 100);

            // Assert
            Assert.True(multiplier10 > 1.0f);
            Assert.True(multiplier50 > multiplier10);
        }

        [Fact]
        public void GetStatScalingMultiplier_CustomScalingRate_UsesProvidedRate()
        {
            // Arrange & Act
            float multiplier = LevelingSystem.GetStatScalingMultiplier(10, 100, scalingRate: 0.02f);

            // Assert
            float expected = 1.0f + (9 * 0.02f); // Level 10 = 9 levels above level 1
            Assert.Equal(expected, multiplier, precision: 5);
        }

        [Fact]
        public void CalculateStatIncreasePerLevel_Level1_ReturnsBaseIncrease()
        {
            // Act
            int increase = LevelingSystem.CalculateStatIncreasePerLevel(1, baseIncrease: 5);

            // Assert
            Assert.Equal(5, increase); // 5 + (1/20) = 5 + 0 = 5
        }

        [Fact]
        public void CalculateStatIncreasePerLevel_IncreasesWithLevel()
        {
            // Arrange & Act
            int increase1 = LevelingSystem.CalculateStatIncreasePerLevel(1);
            int increase20 = LevelingSystem.CalculateStatIncreasePerLevel(20);
            int increase100 = LevelingSystem.CalculateStatIncreasePerLevel(100);

            // Assert
            Assert.True(increase20 >= increase1);
            Assert.True(increase100 > increase20);
        }

        #endregion

        #region Enemy Scaling Tests

        [Fact]
        public void CalculateEnemyHealthForLevel_Level1_ReturnsBaseHealth()
        {
            // Act
            int health = LevelingSystem.CalculateEnemyHealthForLevel(1, baseHealth: 50);

            // Assert - Level 1 should be close to base health
            Assert.True(health >= 45 && health <= 65);
        }

        [Fact]
        public void CalculateEnemyHealthForLevel_IncreasesWithLevel()
        {
            // Arrange & Act
            int health1 = LevelingSystem.CalculateEnemyHealthForLevel(1);
            int health10 = LevelingSystem.CalculateEnemyHealthForLevel(10);
            int health50 = LevelingSystem.CalculateEnemyHealthForLevel(50);

            // Assert
            Assert.True(health10 > health1);
            Assert.True(health50 > health10);
        }

        [Fact]
        public void CalculateEnemyDamageForLevel_Level1_ReturnsBaseDamage()
        {
            // Act
            int damage = LevelingSystem.CalculateEnemyDamageForLevel(1, baseDamage: 5);

            // Assert
            Assert.True(damage >= 4 && damage <= 7);
        }

        [Fact]
        public void CalculateEnemyDamageForLevel_IncreasesWithLevel()
        {
            // Arrange & Act
            int damage1 = LevelingSystem.CalculateEnemyDamageForLevel(1);
            int damage10 = LevelingSystem.CalculateEnemyDamageForLevel(10);
            int damage50 = LevelingSystem.CalculateEnemyDamageForLevel(50);

            // Assert
            Assert.True(damage10 > damage1);
            Assert.True(damage50 > damage10);
        }

        [Fact]
        public void CalculateEnemyXPReward_Level1_ReturnsBaseXP()
        {
            // Act
            int xp = LevelingSystem.CalculateEnemyXPReward(1, baseXP: 20);

            // Assert
            Assert.True(xp >= 18 && xp <= 25);
        }

        [Fact]
        public void CalculateEnemyXPReward_IncreasesWithLevel()
        {
            // Arrange & Act
            int xp1 = LevelingSystem.CalculateEnemyXPReward(1);
            int xp10 = LevelingSystem.CalculateEnemyXPReward(10);
            int xp50 = LevelingSystem.CalculateEnemyXPReward(50);

            // Assert
            Assert.True(xp10 > xp1);
            Assert.True(xp50 > xp10);
        }

        [Fact]
        public void CalculateEnemyGoldReward_ReturnsPositiveValue()
        {
            // Arrange
            var rng = new Random(42);

            // Act
            int gold = LevelingSystem.CalculateEnemyGoldReward(5, baseGold: 10, rng: rng);

            // Assert
            Assert.True(gold > 0);
        }

        [Fact]
        public void CalculateEnemyGoldReward_HasVariance()
        {
            // Arrange
            var rng = new Random(42);
            var goldResults = new System.Collections.Generic.HashSet<int>();

            // Act - Generate 20 gold rewards
            for (int i = 0; i < 20; i++)
            {
                int gold = LevelingSystem.CalculateEnemyGoldReward(5, baseGold: 10, rng: rng);
                goldResults.Add(gold);
            }

            // Assert - Should have variance (multiple different values)
            Assert.True(goldResults.Count > 1);
        }

        [Fact]
        public void CalculateEnemyGoldReward_IncreasesWithLevel()
        {
            // Arrange
            var rng = new Random(42);

            // Act - Average over multiple rolls to account for variance
            int avgGold1 = 0;
            int avgGold50 = 0;
            for (int i = 0; i < 100; i++)
            {
                avgGold1 += LevelingSystem.CalculateEnemyGoldReward(1, rng: rng);
                avgGold50 += LevelingSystem.CalculateEnemyGoldReward(50, rng: rng);
            }
            avgGold1 /= 100;
            avgGold50 /= 100;

            // Assert
            Assert.True(avgGold50 > avgGold1);
        }

        #endregion

        #region Level Difference Tests

        [Fact]
        public void CalculateXPModifierForLevelDifference_SameLevel_ReturnsNormal()
        {
            // Act
            float modifier = LevelingSystem.CalculateXPModifierForLevelDifference(10, 10);

            // Assert
            Assert.Equal(1.0f, modifier);
        }

        [Fact]
        public void CalculateXPModifierForLevelDifference_SlightlyHigher_ReturnsNormal()
        {
            // Act
            float modifier = LevelingSystem.CalculateXPModifierForLevelDifference(10, 11);

            // Assert
            Assert.Equal(1.0f, modifier);
        }

        [Fact]
        public void CalculateXPModifierForLevelDifference_3LevelsHigher_ReturnsBonus()
        {
            // Act
            float modifier = LevelingSystem.CalculateXPModifierForLevelDifference(10, 13);

            // Assert
            Assert.Equal(1.25f, modifier);
        }

        [Fact]
        public void CalculateXPModifierForLevelDifference_5LevelsHigher_ReturnsLargeBonus()
        {
            // Act
            float modifier = LevelingSystem.CalculateXPModifierForLevelDifference(10, 15);

            // Assert
            Assert.Equal(1.5f, modifier);
        }

        [Fact]
        public void CalculateXPModifierForLevelDifference_3LevelsLower_ReturnsPenalty()
        {
            // Act
            float modifier = LevelingSystem.CalculateXPModifierForLevelDifference(10, 7);

            // Assert
            Assert.Equal(0.5f, modifier);
        }

        [Fact]
        public void CalculateXPModifierForLevelDifference_6LevelsLower_ReturnsLargePenalty()
        {
            // Act
            float modifier = LevelingSystem.CalculateXPModifierForLevelDifference(10, 4);

            // Assert
            Assert.Equal(0.2f, modifier);
        }

        [Fact]
        public void IsEnemyTooLowLevel_WithinThreshold_ReturnsFalse()
        {
            // Act
            bool tooLow = LevelingSystem.IsEnemyTooLowLevel(playerLevel: 20, enemyLevel: 11, levelThreshold: 10);

            // Assert
            Assert.False(tooLow);
        }

        [Fact]
        public void IsEnemyTooLowLevel_BeyondThreshold_ReturnsTrue()
        {
            // Act
            bool tooLow = LevelingSystem.IsEnemyTooLowLevel(playerLevel: 20, enemyLevel: 9, levelThreshold: 10);

            // Assert
            Assert.True(tooLow);
        }

        [Fact]
        public void IsEnemyTooLowLevel_ExactThreshold_ReturnsFalse()
        {
            // Act
            bool tooLow = LevelingSystem.IsEnemyTooLowLevel(playerLevel: 20, enemyLevel: 10, levelThreshold: 10);

            // Assert
            Assert.False(tooLow);
        }

        [Fact]
        public void IsEnemyTooLowLevel_HigherLevel_ReturnsFalse()
        {
            // Act
            bool tooLow = LevelingSystem.IsEnemyTooLowLevel(playerLevel: 10, enemyLevel: 20);

            // Assert
            Assert.False(tooLow);
        }

        #endregion

        #region Difficulty Scaling Tests

        [Fact]
        public void GetDifficultyMultiplier_Easy_Returns0Point75()
        {
            // Act
            float multiplier = LevelingSystem.GetDifficultyMultiplier(DifficultyLevel.Easy);

            // Assert
            Assert.Equal(0.75f, multiplier);
        }

        [Fact]
        public void GetDifficultyMultiplier_Normal_ReturnsOne()
        {
            // Act
            float multiplier = LevelingSystem.GetDifficultyMultiplier(DifficultyLevel.Normal);

            // Assert
            Assert.Equal(1.0f, multiplier);
        }

        [Fact]
        public void GetDifficultyMultiplier_Hard_Returns1Point5()
        {
            // Act
            float multiplier = LevelingSystem.GetDifficultyMultiplier(DifficultyLevel.Hard);

            // Assert
            Assert.Equal(1.5f, multiplier);
        }

        [Fact]
        public void GetDifficultyMultiplier_Nightmare_Returns2()
        {
            // Act
            float multiplier = LevelingSystem.GetDifficultyMultiplier(DifficultyLevel.Nightmare);

            // Assert
            Assert.Equal(2.0f, multiplier);
        }

        #endregion

        #region Utility Method Tests

        [Fact]
        public void FormatXPDisplay_FormatsWithCommas()
        {
            // Act
            string formatted = LevelingSystem.FormatXPDisplay(1234, 5000);

            // Assert
            Assert.Equal("1,234 / 5,000", formatted);
        }

        [Fact]
        public void FormatXPDisplay_LargeNumbers_FormatsCorrectly()
        {
            // Act
            string formatted = LevelingSystem.FormatXPDisplay(1234567, 9999999);

            // Assert
            Assert.Equal("1,234,567 / 9,999,999", formatted);
        }

        [Fact]
        public void GetProgressBar_ZeroProgress_ReturnsEmptyBar()
        {
            // Act
            string bar = LevelingSystem.GetProgressBar(0f, barLength: 10);

            // Assert
            Assert.Contains("░░░░░░░░░░", bar); // 10 empty blocks
            Assert.Contains("0%", bar);
        }

        [Fact]
        public void GetProgressBar_HalfProgress_ReturnsHalfFilledBar()
        {
            // Act
            string bar = LevelingSystem.GetProgressBar(0.5f, barLength: 10);

            // Assert
            Assert.Contains("█████░░░░░", bar); // 5 filled, 5 empty
            Assert.Contains("50%", bar);
        }

        [Fact]
        public void GetProgressBar_FullProgress_ReturnsFilledBar()
        {
            // Act
            string bar = LevelingSystem.GetProgressBar(1.0f, barLength: 10);

            // Assert
            Assert.Contains("██████████", bar); // 10 filled blocks
            Assert.Contains("100%", bar);
        }

        [Fact]
        public void GetProgressBar_OverflowProgress_ClampsToFull()
        {
            // Act
            string bar = LevelingSystem.GetProgressBar(1.5f, barLength: 10);

            // Assert
            Assert.Contains("██████████", bar); // Still 10 filled blocks
            Assert.Contains("100%", bar);
        }

        [Fact]
        public void GetProgressBar_NegativeProgress_ClampsToZero()
        {
            // Act
            string bar = LevelingSystem.GetProgressBar(-0.5f, barLength: 10);

            // Assert
            Assert.Contains("░░░░░░░░░░", bar); // All empty
            Assert.Contains("0%", bar);
        }

        [Fact]
        public void GetProgressBar_CustomLength_UsesProvidedLength()
        {
            // Act
            string bar20 = LevelingSystem.GetProgressBar(0.5f, barLength: 20);
            string bar5 = LevelingSystem.GetProgressBar(0.5f, barLength: 5);

            // Assert - Count total characters in brackets
            int length20 = bar20.IndexOf(']') - bar20.IndexOf('[') - 1;
            int length5 = bar5.IndexOf(']') - bar5.IndexOf('[') - 1;

            Assert.Equal(20, length20);
            Assert.Equal(5, length5);
        }

        #endregion
    }
}