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
    /// Combat text is read from CombatManager.CombatMessage / CombatEventOccurred - there is no
    /// console output to capture (removed 2026-11-10).
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

        // The console-capture helpers that used to be here were removed 2026-11-10: game_logic
        // no longer prints to the console - combat text is read from CombatMessage instead.

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
            // Arrange - CalculateXPReward is internal (not private); the test project has
            // InternalsVisibleTo access, so this calls it directly instead of via reflection.
            var rng = new RNGManager(seed: 42);
            var combatManager = new CombatManager(rng);
            var enemy = CreateTestGoblin(level: 10);

            using (var output = RedirectConsoleOutput())
            {
                // Act
                int xpReward = combatManager.CalculateXPReward(enemy);

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
            var lowLevelEnemy = CreateTestGoblin(level: 5);
            var highLevelEnemy = CreateTestGoblin(level: 40); // 40 is the real level cap, not 50/100

            using (var output = RedirectConsoleOutput())
            {
                // Act
                int lowLevelXP = combatManager.CalculateXPReward(lowLevelEnemy);
                int highLevelXP = combatManager.CalculateXPReward(highLevelEnemy);

                // Assert - Higher level enemies should give more XP
                Assert.True(highLevelXP > lowLevelXP);
            }

            RestoreConsoleOutput();
        }

        [Fact]
        public void CalculateXPReward_HasVariance()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            var combatManager = new CombatManager(rng);
            var enemy = CreateTestGoblin(level: 10);

            using (var output = RedirectConsoleOutput())
            {
                // Act - Calculate XP multiple times
                var xpResults = new System.Collections.Generic.HashSet<int>();
                for (int i = 0; i < 20; i++)
                {
                    xpResults.Add(combatManager.CalculateXPReward(enemy));
                }

                // Assert - Should have variance (not always the same value)
                Assert.True(xpResults.Count > 1);
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
            var enemy = CreateTestGoblin(level: 10);

            // Combat text comes through CombatMessage (no console output since 2026-11-10)
            int bonusCount = 0;
            combatManager.CombatMessage += message =>
            {
                if (message.Contains("Bonus XP!"))
                {
                    bonusCount++;
                }
            };

            // Act - Calculate XP 1000 times and count the bonus messages
            for (int i = 0; i < 1000; i++)
            {
                combatManager.CalculateXPReward(enemy);
            }

            // Assert - Bonus should occur ~10% of the time (allow 5% variance)
            double bonusRate = (double)bonusCount / 1000;
            Assert.InRange(bonusRate, 0.05, 0.15);
        }

        // Gold reward tests used to live here, against a CombatManager.CalculateGoldReward
        // method. Gold no longer comes from CombatManager at all - it's part of the loot
        // table roll (Progression.LootGenerator.GenerateLoot().Gold). See LootGeneratorTests.cs.

        #endregion
    }
}