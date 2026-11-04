using System;
using Xunit;
using GameLogic.Progression;

namespace GameLogic.Tests
{
    /// <summary>
    /// Unit tests for LootGenerator's gold rolls.
    /// Gold used to be a CombatManager.CalculateGoldReward method (tested with reflection
    /// in CombatManagerTests.cs); it now comes entirely from the unified loot table system,
    /// so these tests target LootGenerator directly - no reflection needed, it's all public API.
    /// </summary>
    public class LootGeneratorTests
    {
        #region Test Helper Methods

        /// <summary>
        /// NothingDropChance is 0 here, so gold always drops - avoids flaky range
        /// assertions from the "nothing dropped" case other templates can hit.
        /// </summary>
        private LootTable CreateGuaranteedGoldTable()
        {
            return LootTableTemplates.CreateEliteEnemyLoot();
        }

        #endregion

        #region Gold Range Tests

        [Fact]
        public void GenerateLoot_Gold_ReturnsValueInExpectedRange()
        {
            // Arrange
            var generator = new LootGenerator(new Random(42));
            var table = CreateGuaranteedGoldTable(); // MinGold 20, MaxGold 50

            // Act
            var loot = generator.GenerateLoot(table, playerLevel: 10, areaLevel: 0);

            // Assert - LootGenerator scales as MinGold + level/2 to MaxGold + level
            int expectedMin = table.MinGold + (10 / 2);
            int expectedMax = table.MaxGold + 10;
            Assert.InRange(loot.Gold, expectedMin, expectedMax);
        }

        [Fact]
        public void GenerateLoot_Gold_ScalesWithPlayerLevel()
        {
            // Arrange
            var generator = new LootGenerator(new Random(42));
            var table = CreateGuaranteedGoldTable();

            // Act - average over many rolls, since any single roll's ranges can overlap
            const int samples = 200;
            long lowLevelTotal = 0;
            long highLevelTotal = 0;
            for (int i = 0; i < samples; i++)
            {
                lowLevelTotal += generator.GenerateLoot(table, playerLevel: 5, areaLevel: 0).Gold;
                highLevelTotal += generator.GenerateLoot(table, playerLevel: 40, areaLevel: 0).Gold; // 40 is the real level cap
            }

            // Assert - higher player level should give noticeably more gold on average
            double lowAverage = (double)lowLevelTotal / samples;
            double highAverage = (double)highLevelTotal / samples;
            Assert.True(highAverage > lowAverage);
        }

        [Fact]
        public void GenerateLoot_Gold_UsesHigherOfPlayerAndAreaLevel()
        {
            // Arrange
            var generator = new LootGenerator(new Random(42));
            var table = CreateGuaranteedGoldTable();

            // Act - a low player level in a high-level area should scale off the area
            var loot = generator.GenerateLoot(table, playerLevel: 1, areaLevel: 40);

            // Assert
            int expectedMin = table.MinGold + (40 / 2);
            int expectedMax = table.MaxGold + 40;
            Assert.InRange(loot.Gold, expectedMin, expectedMax);
        }

        [Fact]
        public void GenerateLoot_Gold_HasVariance()
        {
            // Arrange
            var generator = new LootGenerator(new Random(42));
            var table = CreateGuaranteedGoldTable();

            // Act - roll gold multiple times
            var goldResults = new System.Collections.Generic.HashSet<int>();
            for (int i = 0; i < 20; i++)
            {
                goldResults.Add(generator.GenerateLoot(table, playerLevel: 10, areaLevel: 0).Gold);
            }

            // Assert - should have variance (not always the same value)
            Assert.True(goldResults.Count > 1);
        }

        [Fact]
        public void GenerateLoot_NothingDropChance_CanProduceEmptyResult()
        {
            // Arrange - the basic enemy table has a 30% chance of dropping nothing at all,
            // which the old CombatManager-based gold tests had no equivalent for.
            var generator = new LootGenerator(new Random(1));
            var table = LootTableTemplates.CreateBasicEnemyLoot();

            // Act - across enough rolls, at least one should come back fully empty
            bool sawEmptyResult = false;
            for (int i = 0; i < 50; i++)
            {
                var loot = generator.GenerateLoot(table, playerLevel: 10, areaLevel: 0);
                if (loot.Gold == 0 && loot.Items.Count == 0)
                {
                    sawEmptyResult = true;
                    break;
                }
            }

            Assert.True(sawEmptyResult);
        }

        #endregion
    }
}
