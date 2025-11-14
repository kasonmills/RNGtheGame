using System;
using System.IO;
using Xunit;
using GameLogic.Combat;
using GameLogic.Systems;
using GameLogic.Entities.Player;
using GameLogic.Entities.Enemies.EnemyTypes;

namespace GameLogic.Tests
{
    /// <summary>
    /// Unit tests for CombatManager
    /// Tests combat orchestration, reward calculation, and combat state management
    ///
    /// NOTE: CombatManager has heavy Console I/O integration, so many tests focus on
    /// the underlying logic (reward calculation, state management) rather than full
    /// combat flow which would require mocking Console or refactoring for testability.
    /// </summary>
    public class CombatManagerTests
    {
        #region Test Helper Methods

        /// <summary>
        /// Create a test player
        /// </summary>
        private Player CreateTestPlayer(string name = "TestHero", int level = 10)
        {
            var player = new Player(name);
            player.Level = level;
            player.Health = 100;
            player.MaxHealth = 100;
            return player;
        }

        /// <summary>
        /// Create a test goblin enemy
        /// </summary>
        private Goblin CreateTestGoblin(int level = 10)
        {
            return new Goblin(level);
        }

        /// <summary>
        /// Redirect console output to capture it for testing
        /// </summary>
        private StringWriter RedirectConsoleOutput()
        {
            var output = new StringWriter();
            Console.SetOut(output);
            return output;
        }

        /// <summary>
        /// Restore console output to standard output
        /// </summary>
        private void RestoreConsoleOutput()
        {
            var standardOutput = new StreamWriter(Console.OpenStandardOutput());
            standardOutput.AutoFlush = true;
            Console.SetOut(standardOutput);
        }

        #endregion

        #region Initialization Tests

        [Fact]
        public void Constructor_InitializesWithRNGManager()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);

            // Act
            var combatManager = new CombatManager(rng);

            // Assert - Constructor should not throw
            Assert.NotNull(combatManager);
        }

        #endregion

        #region Reward Calculation Tests

        /// <summary>
        /// NOTE: These tests use reflection to test private methods since they contain
        /// important game logic. Alternatively, these methods could be made internal
        /// and use InternalsVisibleTo attribute for testing.
        /// </summary>

        [Fact]
        public void CalculateXPReward_ReturnsValueInExpectedRange()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            var combatManager = new CombatManager(rng);
            var player = CreateTestPlayer();
            var enemy = CreateTestGoblin(level: 10);

            // Use reflection to access private field and method
            var playerField = typeof(CombatManager).GetField("_player",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var enemyField = typeof(CombatManager).GetField("_enemy",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var calculateXPMethod = typeof(CombatManager).GetMethod("CalculateXPReward",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            playerField.SetValue(combatManager, player);
            enemyField.SetValue(combatManager, enemy);

            // Redirect console to suppress output
            using (var output = RedirectConsoleOutput())
            {
                // Act
                int xpReward = (int)calculateXPMethod.Invoke(combatManager, null);

                // Assert - XP should be in range: level * 30 to level * 70 (plus potential 50% bonus)
                int minXP = 10 * 30; // 300
                int maxXP = (int)((10 * 70) * 1.5); // 1050 (with bonus)
                Assert.InRange(xpReward, minXP, maxXP);
            }

            RestoreConsoleOutput();
        }

        [Fact]
        public void CalculateXPReward_ScalesWithEnemyLevel()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            var combatManager = new CombatManager(rng);
            var player = CreateTestPlayer();
            var lowLevelEnemy = CreateTestGoblin(level: 5);
            var highLevelEnemy = CreateTestGoblin(level: 50);

            var playerField = typeof(CombatManager).GetField("_player",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var enemyField = typeof(CombatManager).GetField("_enemy",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var calculateXPMethod = typeof(CombatManager).GetMethod("CalculateXPReward",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            playerField.SetValue(combatManager, player);

            using (var output = RedirectConsoleOutput())
            {
                // Act - Test low level enemy
                enemyField.SetValue(combatManager, lowLevelEnemy);
                int lowLevelXP = (int)calculateXPMethod.Invoke(combatManager, null);

                // Test high level enemy
                enemyField.SetValue(combatManager, highLevelEnemy);
                int highLevelXP = (int)calculateXPMethod.Invoke(combatManager, null);

                // Assert - Higher level enemies should give more XP
                Assert.True(highLevelXP > lowLevelXP);
            }

            RestoreConsoleOutput();
        }

        [Fact]
        public void CalculateGoldReward_ReturnsValueInExpectedRange()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            var combatManager = new CombatManager(rng);
            var player = CreateTestPlayer();
            var enemy = CreateTestGoblin(level: 10);

            var playerField = typeof(CombatManager).GetField("_player",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var enemyField = typeof(CombatManager).GetField("_enemy",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var calculateGoldMethod = typeof(CombatManager).GetMethod("CalculateGoldReward",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            playerField.SetValue(combatManager, player);
            enemyField.SetValue(combatManager, enemy);

            using (var output = RedirectConsoleOutput())
            {
                // Act
                int goldReward = (int)calculateGoldMethod.Invoke(combatManager, null);

                // Assert - Gold should be in range: level * 3 to level * 20 (plus potential 2x bonus)
                int minGold = 10 * 3; // 30
                int maxGold = (10 * 20) * 2; // 400 (with bonus)
                Assert.InRange(goldReward, minGold, maxGold);
            }

            RestoreConsoleOutput();
        }

        [Fact]
        public void CalculateGoldReward_ScalesWithEnemyLevel()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            var combatManager = new CombatManager(rng);
            var player = CreateTestPlayer();
            var lowLevelEnemy = CreateTestGoblin(level: 5);
            var highLevelEnemy = CreateTestGoblin(level: 50);

            var playerField = typeof(CombatManager).GetField("_player",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var enemyField = typeof(CombatManager).GetField("_enemy",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var calculateGoldMethod = typeof(CombatManager).GetMethod("CalculateGoldReward",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            playerField.SetValue(combatManager, player);

            using (var output = RedirectConsoleOutput())
            {
                // Act - Test low level enemy
                enemyField.SetValue(combatManager, lowLevelEnemy);
                int lowLevelGold = (int)calculateGoldMethod.Invoke(combatManager, null);

                // Test high level enemy
                enemyField.SetValue(combatManager, highLevelEnemy);
                int highLevelGold = (int)calculateGoldMethod.Invoke(combatManager, null);

                // Assert - Higher level enemies should give more gold
                Assert.True(highLevelGold > lowLevelGold);
            }

            RestoreConsoleOutput();
        }

        [Fact]
        public void CalculateXPReward_HasVariance()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            var combatManager = new CombatManager(rng);
            var player = CreateTestPlayer();
            var enemy = CreateTestGoblin(level: 10);

            var playerField = typeof(CombatManager).GetField("_player",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var enemyField = typeof(CombatManager).GetField("_enemy",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var calculateXPMethod = typeof(CombatManager).GetMethod("CalculateXPReward",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            playerField.SetValue(combatManager, player);
            enemyField.SetValue(combatManager, enemy);

            using (var output = RedirectConsoleOutput())
            {
                // Act - Calculate XP multiple times
                var xpResults = new System.Collections.Generic.HashSet<int>();
                for (int i = 0; i < 20; i++)
                {
                    int xp = (int)calculateXPMethod.Invoke(combatManager, null);
                    xpResults.Add(xp);
                }

                // Assert - Should have variance (not always the same value)
                Assert.True(xpResults.Count > 1);
            }

            RestoreConsoleOutput();
        }

        [Fact]
        public void CalculateGoldReward_HasVariance()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            var combatManager = new CombatManager(rng);
            var player = CreateTestPlayer();
            var enemy = CreateTestGoblin(level: 10);

            var playerField = typeof(CombatManager).GetField("_player",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var enemyField = typeof(CombatManager).GetField("_enemy",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var calculateGoldMethod = typeof(CombatManager).GetMethod("CalculateGoldReward",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            playerField.SetValue(combatManager, player);
            enemyField.SetValue(combatManager, enemy);

            using (var output = RedirectConsoleOutput())
            {
                // Act - Calculate gold multiple times
                var goldResults = new System.Collections.Generic.HashSet<int>();
                for (int i = 0; i < 20; i++)
                {
                    int gold = (int)calculateGoldMethod.Invoke(combatManager, null);
                    goldResults.Add(gold);
                }

                // Assert - Should have variance
                Assert.True(goldResults.Count > 1);
            }

            RestoreConsoleOutput();
        }

        #endregion

        #region CombatAction Tests

        [Fact]
        public void CombatAction_Attack_CreatesCorrectAction()
        {
            // Arrange
            var player = CreateTestPlayer();
            var enemy = CreateTestGoblin();

            // Act
            var action = CombatAction.Attack(player, enemy);

            // Assert
            Assert.Equal(ActionType.Attack, action.Type);
            Assert.Equal(player, action.Actor);
            Assert.Equal(enemy, action.Target);
        }

        [Fact]
        public void CombatAction_Defend_CreatesCorrectAction()
        {
            // Arrange
            var player = CreateTestPlayer();

            // Act
            var action = CombatAction.Defend(player);

            // Assert
            Assert.Equal(ActionType.Defend, action.Type);
            Assert.Equal(player, action.Actor);
            Assert.Null(action.Target);
        }

        [Fact]
        public void CombatAction_Flee_CreatesCorrectAction()
        {
            // Arrange
            var player = CreateTestPlayer();

            // Act
            var action = CombatAction.Flee(player);

            // Assert
            Assert.Equal(ActionType.Flee, action.Type);
            Assert.Equal(player, action.Actor);
            Assert.Null(action.Target);
        }

        [Fact]
        public void CombatAction_Constructor_SetsProperties()
        {
            // Arrange
            var player = CreateTestPlayer();
            var enemy = CreateTestGoblin();

            // Act
            var action = new CombatAction(ActionType.Attack, player, enemy);

            // Assert
            Assert.Equal(ActionType.Attack, action.Type);
            Assert.Equal(player, action.Actor);
            Assert.Equal(enemy, action.Target);
        }

        [Fact]
        public void CombatAction_Constructor_AllowsNullTarget()
        {
            // Arrange
            var player = CreateTestPlayer();

            // Act
            var action = new CombatAction(ActionType.Defend, player, null);

            // Assert
            Assert.Equal(ActionType.Defend, action.Type);
            Assert.Equal(player, action.Actor);
            Assert.Null(action.Target);
        }

        #endregion

        #region ActionType Enum Tests

        [Fact]
        public void ActionType_HasAttackValue()
        {
            // Act
            var type = ActionType.Attack;

            // Assert
            Assert.Equal(ActionType.Attack, type);
        }

        [Fact]
        public void ActionType_HasUseAbilityValue()
        {
            // Act
            var type = ActionType.UseAbility;

            // Assert
            Assert.Equal(ActionType.UseAbility, type);
        }

        [Fact]
        public void ActionType_HasUseItemValue()
        {
            // Act
            var type = ActionType.UseItem;

            // Assert
            Assert.Equal(ActionType.UseItem, type);
        }

        [Fact]
        public void ActionType_HasDefendValue()
        {
            // Act
            var type = ActionType.Defend;

            // Assert
            Assert.Equal(ActionType.Defend, type);
        }

        [Fact]
        public void ActionType_HasFleeValue()
        {
            // Act
            var type = ActionType.Flee;

            // Assert
            Assert.Equal(ActionType.Flee, type);
        }

        [Fact]
        public void ActionType_AllValuesAreUnique()
        {
            // Arrange
            var values = new[]
            {
                ActionType.Attack,
                ActionType.UseAbility,
                ActionType.UseItem,
                ActionType.Defend,
                ActionType.Flee
            };

            // Act
            var uniqueValues = new System.Collections.Generic.HashSet<ActionType>(values);

            // Assert - All values should be unique
            Assert.Equal(values.Length, uniqueValues.Count);
        }

        #endregion

        #region Statistical Tests

        [Fact]
        public void CalculateXPReward_BonusOccursAtExpectedRate()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            var combatManager = new CombatManager(rng);
            var player = CreateTestPlayer();
            var enemy = CreateTestGoblin(level: 10);

            var playerField = typeof(CombatManager).GetField("_player",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var enemyField = typeof(CombatManager).GetField("_enemy",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var calculateXPMethod = typeof(CombatManager).GetMethod("CalculateXPReward",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            playerField.SetValue(combatManager, player);
            enemyField.SetValue(combatManager, enemy);

            using (var output = RedirectConsoleOutput())
            {
                // Act - Calculate XP 1000 times and check for bonus messages
                int bonusCount = 0;
                for (int i = 0; i < 1000; i++)
                {
                    output.GetStringBuilder().Clear();
                    int xp = (int)calculateXPMethod.Invoke(combatManager, null);

                    string consoleOutput = output.ToString();
                    if (consoleOutput.Contains("Bonus XP!"))
                    {
                        bonusCount++;
                    }
                }

                // Assert - Bonus should occur ~10% of the time (allow 5% variance)
                double bonusRate = (double)bonusCount / 1000;
                Assert.InRange(bonusRate, 0.05, 0.15);
            }

            RestoreConsoleOutput();
        }

        [Fact]
        public void CalculateGoldReward_TreasureBonusOccursAtExpectedRate()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            var combatManager = new CombatManager(rng);
            var player = CreateTestPlayer();
            var enemy = CreateTestGoblin(level: 10);

            var playerField = typeof(CombatManager).GetField("_player",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var enemyField = typeof(CombatManager).GetField("_enemy",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var calculateGoldMethod = typeof(CombatManager).GetMethod("CalculateGoldReward",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            playerField.SetValue(combatManager, player);
            enemyField.SetValue(combatManager, enemy);

            using (var output = RedirectConsoleOutput())
            {
                // Act - Calculate gold 1000 times and check for treasure bonus
                int treasureCount = 0;
                for (int i = 0; i < 1000; i++)
                {
                    output.GetStringBuilder().Clear();
                    int gold = (int)calculateGoldMethod.Invoke(combatManager, null);

                    string consoleOutput = output.ToString();
                    if (consoleOutput.Contains("extra gold"))
                    {
                        treasureCount++;
                    }
                }

                // Assert - Treasure bonus should occur ~5% of the time (allow 3% variance)
                double treasureRate = (double)treasureCount / 1000;
                Assert.InRange(treasureRate, 0.02, 0.08);
            }

            RestoreConsoleOutput();
        }

        #endregion
    }
}