using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using GameLogic.Systems;

namespace GameLogic.Tests
{
    /// <summary>
    /// Unit tests for RNGManager
    /// Tests core RNG functionality, weighted selection, dice rolls, and collection operations
    /// </summary>
    public class RNGManagerTests
    {
        #region Initialization Tests

        [Fact]
        public void Constructor_WithoutSeed_CreatesInstance()
        {
            // Arrange & Act
            var rng = new RNGManager();

            // Assert
            Assert.NotNull(rng);
            Assert.Equal("System.Random", rng.CurrentAlgorithm);
        }

        [Fact]
        public void Constructor_WithSeed_CreatesReproducibleResults()
        {
            // Arrange
            int seed = 12345;
            var rng1 = new RNGManager(seed);
            var rng2 = new RNGManager(seed);

            // Act
            int result1 = rng1.Roll(1, 100);
            int result2 = rng2.Roll(1, 100);

            // Assert
            Assert.Equal(result1, result2); // Same seed = same results
        }

        [Fact]
        public void Constructor_WithStatisticsTracking_EnablesTracking()
        {
            // Arrange & Act
            var rng = new RNGManager(trackStatistics: true);
            rng.Roll(1, 100);
            rng.Roll(1, 100);

            // Assert
            Assert.Equal(2, rng.TotalRolls);
        }

        #endregion

        #region Basic Roll Tests

        [Fact]
        public void Roll_ReturnsValueInRange()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);

            // Act & Assert
            for (int i = 0; i < 100; i++)
            {
                int result = rng.Roll(1, 10);
                Assert.InRange(result, 1, 10);
            }
        }

        [Fact]
        public void Roll_InclusiveOnBothEnds()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            var results = new HashSet<int>();

            // Act - Roll many times to get distribution
            for (int i = 0; i < 1000; i++)
            {
                results.Add(rng.Roll(1, 5));
            }

            // Assert - Should eventually get both 1 and 5 (inclusive)
            Assert.Contains(1, results);
            Assert.Contains(5, results);
        }

        [Fact]
        public void Next_ExclusiveMax()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);

            // Act & Assert
            for (int i = 0; i < 100; i++)
            {
                int result = rng.Next(0, 10);
                Assert.InRange(result, 0, 9); // 10 should never appear
            }
        }

        [Fact]
        public void NextDouble_ReturnsValueBetweenZeroAndOne()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);

            // Act & Assert
            for (int i = 0; i < 100; i++)
            {
                double result = rng.NextDouble();
                Assert.InRange(result, 0.0, 0.999999);
            }
        }

        [Fact]
        public void NextFloat_ReturnsValueBetweenZeroAndOne()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);

            // Act & Assert
            for (int i = 0; i < 100; i++)
            {
                float result = rng.NextFloat();
                Assert.InRange(result, 0.0f, 0.999999f);
            }
        }

        #endregion

        #region Boolean Tests

        [Fact]
        public void NextBool_Returns50PercentDistribution()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            int trueCount = 0;
            int totalRolls = 1000;

            // Act
            for (int i = 0; i < totalRolls; i++)
            {
                if (rng.NextBool())
                    trueCount++;
            }

            // Assert - Should be roughly 50% (allow 10% variance)
            double truePercentage = (double)trueCount / totalRolls;
            Assert.InRange(truePercentage, 0.4, 0.6);
        }

        [Fact]
        public void NextBool_WithProbability_ReturnsCorrectDistribution()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            int trueCount = 0;
            int totalRolls = 1000;
            float probability = 0.75f; // 75% chance

            // Act
            for (int i = 0; i < totalRolls; i++)
            {
                if (rng.NextBool(probability))
                    trueCount++;
            }

            // Assert - Should be roughly 75% (allow 10% variance)
            double truePercentage = (double)trueCount / totalRolls;
            Assert.InRange(truePercentage, 0.65, 0.85);
        }

        #endregion

        #region Percentage Tests

        [Fact]
        public void RollPercentage_100Percent_AlwaysSucceeds()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);

            // Act & Assert
            for (int i = 0; i < 100; i++)
            {
                Assert.True(rng.RollPercentage(100));
            }
        }

        [Fact]
        public void RollPercentage_0Percent_AlwaysFails()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);

            // Act & Assert
            for (int i = 0; i < 100; i++)
            {
                Assert.False(rng.RollPercentage(0));
            }
        }

        [Fact]
        public void RollPercentage_50Percent_ReturnsCorrectDistribution()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            int successCount = 0;
            int totalRolls = 1000;

            // Act
            for (int i = 0; i < totalRolls; i++)
            {
                if (rng.RollPercentage(50))
                    successCount++;
            }

            // Assert - Should be roughly 50% (allow 10% variance)
            double successRate = (double)successCount / totalRolls;
            Assert.InRange(successRate, 0.4, 0.6);
        }

        [Fact]
        public void RollChance_ReturnsCorrectDistribution()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            int successCount = 0;
            int totalRolls = 1000;
            float chance = 0.25f; // 25% chance

            // Act
            for (int i = 0; i < totalRolls; i++)
            {
                if (rng.RollChance(chance))
                    successCount++;
            }

            // Assert - Should be roughly 25% (allow 10% variance)
            double successRate = (double)successCount / totalRolls;
            Assert.InRange(successRate, 0.15, 0.35);
        }

        #endregion

        #region Dice Roll Tests

        [Fact]
        public void RollDice_SingleDie_ReturnsValidRange()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);

            // Act & Assert
            for (int i = 0; i < 100; i++)
            {
                int result = rng.RollDice(1, 6); // 1d6
                Assert.InRange(result, 1, 6);
            }
        }

        [Fact]
        public void RollDice_MultipleDice_ReturnsValidRange()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);

            // Act & Assert
            for (int i = 0; i < 100; i++)
            {
                int result = rng.RollDice(3, 6); // 3d6
                Assert.InRange(result, 3, 18); // Min: 3x1, Max: 3x6
            }
        }

        [Fact]
        public void RollD20_ReturnsValidRange()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);

            // Act & Assert
            for (int i = 0; i < 100; i++)
            {
                int result = rng.RollD20();
                Assert.InRange(result, 1, 20);
            }
        }

        [Fact]
        public void RollD100_ReturnsValidRange()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);

            // Act & Assert
            for (int i = 0; i < 100; i++)
            {
                int result = rng.RollD100();
                Assert.InRange(result, 1, 100);
            }
        }

        #endregion

        #region Range Tests

        [Fact]
        public void Range_Int_ReturnsValidRange()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);

            // Act & Assert
            for (int i = 0; i < 100; i++)
            {
                int result = rng.Range(5, 15);
                Assert.InRange(result, 5, 15);
            }
        }

        [Fact]
        public void Range_Float_ReturnsValidRange()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);

            // Act & Assert
            for (int i = 0; i < 100; i++)
            {
                float result = rng.Range(1.5f, 3.5f);
                Assert.InRange(result, 1.5f, 3.5f);
            }
        }

        #endregion

        #region Weighted Selection Tests

        [Fact]
        public void SelectWeightedIndex_ThrowsOnNullWeights()
        {
            // Arrange
            var rng = new RNGManager();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => rng.SelectWeightedIndex(null));
        }

        [Fact]
        public void SelectWeightedIndex_ThrowsOnEmptyWeights()
        {
            // Arrange
            var rng = new RNGManager();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => rng.SelectWeightedIndex(new int[0]));
        }

        [Fact]
        public void SelectWeightedIndex_ThrowsOnZeroTotalWeight()
        {
            // Arrange
            var rng = new RNGManager();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => rng.SelectWeightedIndex(new[] { 0, 0, 0 }));
        }

        [Fact]
        public void SelectWeightedIndex_ReturnsValidIndex()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            var weights = new[] { 10, 20, 30 };

            // Act & Assert
            for (int i = 0; i < 100; i++)
            {
                int index = rng.SelectWeightedIndex(weights);
                Assert.InRange(index, 0, 2);
            }
        }

        [Fact]
        public void SelectWeightedIndex_RespectsWeights()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            var weights = new[] { 1, 99 }; // Second index has 99% weight
            int secondIndexCount = 0;
            int totalRolls = 1000;

            // Act
            for (int i = 0; i < totalRolls; i++)
            {
                if (rng.SelectWeightedIndex(weights) == 1)
                    secondIndexCount++;
            }

            // Assert - Index 1 should appear roughly 99% of the time (allow 10% variance)
            double secondIndexRate = (double)secondIndexCount / totalRolls;
            Assert.InRange(secondIndexRate, 0.89, 1.0);
        }

        [Fact]
        public void SelectWeightedItem_ReturnsCorrectItem()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            var items = new List<string> { "Common", "Rare", "Legendary" };
            var weights = new List<int> { 70, 25, 5 };

            // Act
            var results = new Dictionary<string, int>();
            for (int i = 0; i < 1000; i++)
            {
                string item = rng.SelectWeightedItem(items, weights);
                if (!results.ContainsKey(item))
                    results[item] = 0;
                results[item]++;
            }

            // Assert - All items should appear, Common most frequently
            Assert.True(results.ContainsKey("Common"));
            Assert.True(results.ContainsKey("Rare"));
            Assert.True(results.ContainsKey("Legendary"));
            Assert.True(results["Common"] > results["Rare"]);
            Assert.True(results["Rare"] > results["Legendary"]);
        }

        #endregion

        #region Collection Tests

        [Fact]
        public void Shuffle_ChangesListOrder()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            var original = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var shuffled = new List<int>(original);

            // Act
            rng.Shuffle(shuffled);

            // Assert - Should be different order (very unlikely to be same)
            Assert.NotEqual(original, shuffled);
        }

        [Fact]
        public void Shuffle_ContainsSameElements()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            var original = new List<int> { 1, 2, 3, 4, 5 };
            var shuffled = new List<int>(original);

            // Act
            rng.Shuffle(shuffled);

            // Assert - Same elements, different order
            Assert.Equal(original.OrderBy(x => x), shuffled.OrderBy(x => x));
        }

        [Fact]
        public void SelectRandom_List_ReturnsValidItem()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            var list = new List<string> { "A", "B", "C", "D", "E" };

            // Act & Assert
            for (int i = 0; i < 100; i++)
            {
                string item = rng.SelectRandom(list);
                Assert.Contains(item, list);
            }
        }

        [Fact]
        public void SelectRandom_Array_ReturnsValidItem()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            var array = new[] { "A", "B", "C", "D", "E" };

            // Act & Assert
            for (int i = 0; i < 100; i++)
            {
                string item = rng.SelectRandom(array);
                Assert.Contains(item, array);
            }
        }

        [Fact]
        public void SelectRandomUnique_ReturnsCorrectCount()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            var list = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            // Act
            var selected = rng.SelectRandomUnique(list, 5);

            // Assert
            Assert.Equal(5, selected.Count);
        }

        [Fact]
        public void SelectRandomUnique_ReturnsUniqueItems()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            var list = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            // Act
            var selected = rng.SelectRandomUnique(list, 5);

            // Assert - All items should be unique
            Assert.Equal(selected.Count, selected.Distinct().Count());
        }

        [Fact]
        public void SelectRandomUnique_ThrowsWhenCountExceedsList()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            var list = new List<int> { 1, 2, 3 };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => rng.SelectRandomUnique(list, 5));
        }

        #endregion

        #region Statistics Tests

        [Fact]
        public void Statistics_Disabled_DoesNotTrack()
        {
            // Arrange
            var rng = new RNGManager(trackStatistics: false);

            // Act
            rng.Roll(1, 100);
            rng.Roll(1, 100);

            // Assert
            Assert.Equal(0, rng.TotalRolls);
        }

        [Fact]
        public void Statistics_Enabled_TracksRolls()
        {
            // Arrange
            var rng = new RNGManager(trackStatistics: true);

            // Act
            rng.Roll(1, 100);
            rng.NextDouble();
            rng.RollPercentage(50);

            // Assert
            Assert.Equal(3, rng.TotalRolls);
        }

        [Fact]
        public void ResetStatistics_ResetsCount()
        {
            // Arrange
            var rng = new RNGManager(trackStatistics: true);
            rng.Roll(1, 100);
            rng.Roll(1, 100);

            // Act
            rng.ResetStatistics();

            // Assert
            Assert.Equal(0, rng.TotalRolls);
        }

        [Fact]
        public void SetStatisticsTracking_EnablesTracking()
        {
            // Arrange
            var rng = new RNGManager(trackStatistics: false);

            // Act
            rng.SetStatisticsTracking(true);
            rng.Roll(1, 100);

            // Assert
            Assert.Equal(1, rng.TotalRolls);
        }

        #endregion

        #region Algorithm Management Tests

        [Fact]
        public void CurrentAlgorithm_ReturnsSystemRandom()
        {
            // Arrange & Act
            var rng = new RNGManager();

            // Assert
            Assert.Equal("System.Random", rng.CurrentAlgorithm);
        }

        [Fact]
        public void GetAvailableAlgorithms_ContainsSystemRandom()
        {
            // Arrange & Act
            var rng = new RNGManager();
            var algorithms = rng.GetAvailableAlgorithms();

            // Assert
            Assert.Contains("system", algorithms);
        }

        [Fact]
        public void GetInfo_ReturnsValidString()
        {
            // Arrange
            var rng = new RNGManager(trackStatistics: true);
            rng.Roll(1, 100);

            // Act
            string info = rng.GetInfo();

            // Assert
            Assert.Contains("System.Random", info);
            Assert.Contains("Total Rolls", info);
        }

        #endregion
    }
}