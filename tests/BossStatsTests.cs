using System.Linq;
using Xunit;
using GameLogic.Systems;
using GameLogic.Entities.Enemies.Bosses;

namespace GameLogic.Tests
{
    /// <summary>
    /// Bosses grow from their hand-set level-1 stats (BossStatSheet, approved 2026-11-10) to the
    /// level they are fought at, through DifficultyScaler's boss growth (health x3, damage x1.5).
    /// Expected numbers match docs/boss_stats_draft.md ("What that becomes in the fight").
    /// </summary>
    public class BossStatsTests
    {
        private static BossEnemy MakeBoss(string bossId, int level)
        {
            return new BossEnemy(bossId, "Test Boss", "the Tester", "A boss for tests.", level);
        }

        #region Growth formulas

        [Theory]
        [InlineData(70, 1, 210)]     // Level 1: just the x3
        [InlineData(70, 2, 231)]     // Skarn: 70 x 1.1 x 3
        [InlineData(60, 8, 306)]     // Monk: 60 x 1.7 x 3
        [InlineData(125, 45, 2025)]  // Final boss: 125 x 5.4 x 3
        public void BossHealthAtLevel_GrowsTenPercentPerLevel_TimesThree(int level1, int level, int expected)
        {
            Assert.Equal(expected, DifficultyScaler.BossHealthAtLevel(level1, level));
        }

        [Theory]
        [InlineData(9, 2, 15)]       // Skarn min: 9 x 1.08 x 1.5 = 14.58 -> 15
        [InlineData(17, 2, 28)]      // Skarn max: 27.54 -> 28
        [InlineData(11, 26, 50)]     // Slime Dragon min
        [InlineData(19, 26, 86)]     // Slime Dragon max
        public void BossDamageAtLevel_GrowsEightPercentPerLevel_TimesOnePointFive(int level1, int level, int expected)
        {
            Assert.Equal(expected, DifficultyScaler.BossDamageAtLevel(level1, level));
        }

        [Theory]
        [InlineData(3, 0.3, 26, 11)]   // 3 + 7.5 = 10.5 -> 11 (halves round up)
        [InlineData(12, 0.0, 2, 12)]   // No growth
        [InlineData(14, 0.3, 45, 27)]  // 14 + 13.2 = 27.2 -> 27
        public void BossStatAtLevel_AddsGrowthPerLevel(int level1, double growth, int level, int expected)
        {
            Assert.Equal(expected, DifficultyScaler.BossStatAtLevel(level1, growth, level));
        }

        #endregion

        #region The sheet

        [Fact]
        public void Sheet_HasAllNineBosses_WithSaneNumbers()
        {
            var ids = BossStatSheet.AllIds.ToList();

            // 9 bosses, the Cyclops Brothers as two rows
            Assert.Equal(9, ids.Count);
            foreach (var id in ids)
            {
                var stats = BossStatSheet.Get(id);
                Assert.True(stats.Health > 0, id);
                Assert.True(stats.MinDamage > 0 && stats.MinDamage <= stats.MaxDamage, id);
                Assert.InRange(stats.Accuracy, 1, 100);
                Assert.InRange(stats.CritChance, 0, 100);
            }
        }

        [Fact]
        public void Skarn_InTheBossList_UsesHisSheetRow()
        {
            var skarn = BossDefinitions.GetBoss(BossStatSheet.Skarn);

            Assert.NotNull(skarn);
            Assert.True(skarn.HasFixedLevel);
            Assert.Equal(2, skarn.Level);
            Assert.Equal(231, skarn.MaxHealth);
            Assert.Equal(15, skarn.MinDamage);
            Assert.Equal(28, skarn.MaxDamage);
            Assert.Equal(1, skarn.Defense);
            Assert.Equal(12, skarn.Speed);
            Assert.Equal(72, skarn.Accuracy);
            Assert.Equal(5, skarn.CritChance);
        }

        [Fact]
        public void Captain_KeepsHisCritNumbers_AtAnyLevel()
        {
            var captain = MakeBoss(BossStatSheet.Captain, 32);

            Assert.Equal(55, captain.CritChance);
            Assert.Equal(2.5, captain.CritMultiplier);
            Assert.Equal(861, captain.MaxHealth);
            Assert.Equal(24, captain.Speed);
        }

        [Fact]
        public void InitializeStats_AtANewLevel_RebuildsFromTheSheet()
        {
            var monk = MakeBoss(BossStatSheet.LibrarianMonk, 1);
            Assert.Equal(180, monk.MaxHealth); // 60 x 3

            monk.InitializeStats(8);

            Assert.Equal(8, monk.Level);
            Assert.Equal(306, monk.MaxHealth);
            Assert.Equal(306, monk.Health);
            Assert.Equal(15, monk.MinDamage);
            Assert.Equal(26, monk.MaxDamage);
            Assert.Equal(4, monk.Defense);
            Assert.Equal(9, monk.Speed);
        }

        [Fact]
        public void BossWithNoSheetRow_UsesTheFallbackFormula()
        {
            var boss = MakeBoss("not_on_the_sheet", 10);

            Assert.Equal(100 + 10 * 30, boss.MaxHealth);
            Assert.Equal(10 + 10 * 3, boss.MinDamage);
        }

        #endregion

        #region Difficulty on top, once

        [Theory]
        [InlineData(DifficultyLevel.Normal, 174)]    // 231 x 0.75 = 173.25 -> 174
        [InlineData(DifficultyLevel.Hard, 231)]
        [InlineData(DifficultyLevel.Difficult, 347)] // 346.5 -> 347
        [InlineData(DifficultyLevel.Unfair, 462)]
        public void Difficulty_IsAppliedOnTopOfTheSheetStats(DifficultyLevel difficulty, int expectedHealth)
        {
            var skarn = MakeBoss(BossStatSheet.Skarn, 2);
            var scaler = new DifficultyScaler(difficulty);

            scaler.ApplyDifficultyToEnemy(skarn);

            Assert.Equal(expectedHealth, skarn.MaxHealth);
        }

        [Fact]
        public void Difficulty_DoesNotStack_WhenTheBossIsRebuiltForARetry()
        {
            var skarn = MakeBoss(BossStatSheet.Skarn, 2);
            var scaler = new DifficultyScaler(DifficultyLevel.Unfair);

            // First attempt
            scaler.ApplyDifficultyToEnemy(skarn);
            scaler.ApplyDifficultyToEnemy(skarn); // A second call does nothing
            Assert.Equal(462, skarn.MaxHealth);

            // Retry: CombatManager rebuilds the boss, then applies the difficulty again
            skarn.InitializeStats(2);
            scaler.ApplyDifficultyToEnemy(skarn);
            Assert.Equal(462, skarn.MaxHealth);
            Assert.Equal(30, skarn.MinDamage);
            Assert.Equal(56, skarn.MaxDamage);
        }

        #endregion
    }
}
