using System;
using Xunit;
using GameLogic.Combat;
using GameLogic.Entities.Player;
using GameLogic.Entities.Enemies.EnemyTypes;

namespace GameLogic.Tests
{
    /// <summary>
    /// Unit tests for TurnManager
    /// Tests turn order, turn progression, and combat initialization
    /// </summary>
    public class TurnManagerTests
    {
        #region Test Helper Methods

        /// <summary>
        /// Create a test player
        /// </summary>
        private Player CreateTestPlayer(string name = "TestHero")
        {
            return new Player(name);
        }

        /// <summary>
        /// Create a test goblin enemy
        /// </summary>
        private Goblin CreateTestGoblin(int level = 10)
        {
            return new Goblin(level);
        }

        #endregion

        #region Initialization Tests

        [Fact]
        public void Constructor_InitializesWithZeroTurns()
        {
            // Act
            var turnManager = new TurnManager();

            // Assert
            Assert.Equal(0, turnManager.CurrentTurnNumber);
        }

        [Fact]
        public void InitializeCombat_SetsTurnNumberToZero()
        {
            // Arrange
            var turnManager = new TurnManager();
            var player = CreateTestPlayer();
            var enemy = CreateTestGoblin();

            // Act
            turnManager.InitializeCombat(player, enemy);

            // Assert
            Assert.Equal(0, turnManager.CurrentTurnNumber);
        }

        [Fact]
        public void InitializeCombat_PlayerGoesFirst()
        {
            // Arrange
            var turnManager = new TurnManager();
            var player = CreateTestPlayer();
            var enemy = CreateTestGoblin();

            // Act
            turnManager.InitializeCombat(player, enemy);

            // Assert
            Assert.True(turnManager.IsPlayerTurn());
            Assert.False(turnManager.IsEnemyTurn());
        }

        [Fact]
        public void InitializeCombat_ResetsQueue()
        {
            // Arrange
            var turnManager = new TurnManager();
            var player = CreateTestPlayer();
            var enemy = CreateTestGoblin();

            // Act - Initialize twice
            turnManager.InitializeCombat(player, enemy);
            turnManager.NextTurn(); // Advance to enemy turn
            turnManager.InitializeCombat(player, enemy); // Re-initialize

            // Assert - Should be back to player's turn
            Assert.True(turnManager.IsPlayerTurn());
            Assert.Equal(0, turnManager.CurrentTurnNumber);
        }

        #endregion

        #region Turn Order Tests

        [Fact]
        public void GetCurrentTurnOwner_AfterInitialize_ReturnsPlayer()
        {
            // Arrange
            var turnManager = new TurnManager();
            var player = CreateTestPlayer();
            var enemy = CreateTestGoblin();
            turnManager.InitializeCombat(player, enemy);

            // Act
            var currentOwner = turnManager.GetCurrentTurnOwner();

            // Assert
            Assert.Equal(TurnOwner.Player, currentOwner);
        }

        [Fact]
        public void NextTurn_SwitchesFromPlayerToEnemy()
        {
            // Arrange
            var turnManager = new TurnManager();
            var player = CreateTestPlayer();
            var enemy = CreateTestGoblin();
            turnManager.InitializeCombat(player, enemy);

            // Act
            turnManager.NextTurn();

            // Assert
            Assert.True(turnManager.IsEnemyTurn());
            Assert.False(turnManager.IsPlayerTurn());
        }

        [Fact]
        public void NextTurn_TwoCalls_CompletesRound()
        {
            // Arrange
            var turnManager = new TurnManager();
            var player = CreateTestPlayer();
            var enemy = CreateTestGoblin();
            turnManager.InitializeCombat(player, enemy);

            // Act
            turnManager.NextTurn(); // Player -> Enemy
            turnManager.NextTurn(); // Enemy -> Player (new round)

            // Assert - Should be turn 1 (0-indexed becomes 1)
            Assert.Equal(1, turnManager.CurrentTurnNumber);
            Assert.True(turnManager.IsPlayerTurn()); // Back to player
        }

        [Fact]
        public void NextTurn_AlternatesCorrectly()
        {
            // Arrange
            var turnManager = new TurnManager();
            var player = CreateTestPlayer();
            var enemy = CreateTestGoblin();
            turnManager.InitializeCombat(player, enemy);

            // Act & Assert - Verify pattern: P, E, P, E, P, E
            Assert.True(turnManager.IsPlayerTurn());  // Turn 0: Player

            turnManager.NextTurn();
            Assert.True(turnManager.IsEnemyTurn());   // Turn 0: Enemy

            turnManager.NextTurn();
            Assert.True(turnManager.IsPlayerTurn());  // Turn 1: Player

            turnManager.NextTurn();
            Assert.True(turnManager.IsEnemyTurn());   // Turn 1: Enemy

            turnManager.NextTurn();
            Assert.True(turnManager.IsPlayerTurn());  // Turn 2: Player

            turnManager.NextTurn();
            Assert.True(turnManager.IsEnemyTurn());   // Turn 2: Enemy
        }

        [Fact]
        public void GetCurrentTurnOwner_RefillsQueueWhenEmpty()
        {
            // Arrange
            var turnManager = new TurnManager();
            var player = CreateTestPlayer();
            var enemy = CreateTestGoblin();
            turnManager.InitializeCombat(player, enemy);

            // Act - Advance through multiple rounds
            for (int i = 0; i < 10; i++)
            {
                turnManager.NextTurn();
            }

            // Assert - Should still return valid turn owner (queue refills automatically)
            var currentOwner = turnManager.GetCurrentTurnOwner();
            Assert.True(currentOwner == TurnOwner.Player || currentOwner == TurnOwner.Enemy);
        }

        #endregion

        #region Turn Counter Tests

        [Fact]
        public void CurrentTurnNumber_StartsAtZero()
        {
            // Arrange
            var turnManager = new TurnManager();
            var player = CreateTestPlayer();
            var enemy = CreateTestGoblin();

            // Act
            turnManager.InitializeCombat(player, enemy);

            // Assert
            Assert.Equal(0, turnManager.CurrentTurnNumber);
        }

        [Fact]
        public void CurrentTurnNumber_IncrementsAfterFullRound()
        {
            // Arrange
            var turnManager = new TurnManager();
            var player = CreateTestPlayer();
            var enemy = CreateTestGoblin();
            turnManager.InitializeCombat(player, enemy);

            // Act - Complete one full round (player turn + enemy turn)
            turnManager.NextTurn(); // Player -> Enemy
            turnManager.NextTurn(); // Enemy -> Player (round complete)

            // Assert
            Assert.Equal(1, turnManager.CurrentTurnNumber);
        }

        [Fact]
        public void CurrentTurnNumber_IncrementsCorrectlyOverMultipleRounds()
        {
            // Arrange
            var turnManager = new TurnManager();
            var player = CreateTestPlayer();
            var enemy = CreateTestGoblin();
            turnManager.InitializeCombat(player, enemy);

            // Act - Complete 5 full rounds (10 turns total)
            for (int i = 0; i < 10; i++)
            {
                turnManager.NextTurn();
            }

            // Assert - Should be on turn 5 (0 -> 1 -> 2 -> 3 -> 4 -> 5)
            Assert.Equal(5, turnManager.CurrentTurnNumber);
        }

        [Fact]
        public void CurrentTurnNumber_StaysCorrectDuringRound()
        {
            // Arrange
            var turnManager = new TurnManager();
            var player = CreateTestPlayer();
            var enemy = CreateTestGoblin();
            turnManager.InitializeCombat(player, enemy);

            // Act & Assert
            Assert.Equal(0, turnManager.CurrentTurnNumber); // Player's turn in round 0

            turnManager.NextTurn();
            Assert.Equal(0, turnManager.CurrentTurnNumber); // Enemy's turn in round 0

            turnManager.NextTurn();
            Assert.Equal(1, turnManager.CurrentTurnNumber); // Player's turn in round 1
        }

        #endregion

        #region IsPlayerTurn/IsEnemyTurn Tests

        [Fact]
        public void IsPlayerTurn_InitialState_ReturnsTrue()
        {
            // Arrange
            var turnManager = new TurnManager();
            var player = CreateTestPlayer();
            var enemy = CreateTestGoblin();
            turnManager.InitializeCombat(player, enemy);

            // Act
            bool isPlayerTurn = turnManager.IsPlayerTurn();

            // Assert
            Assert.True(isPlayerTurn);
        }

        [Fact]
        public void IsEnemyTurn_InitialState_ReturnsFalse()
        {
            // Arrange
            var turnManager = new TurnManager();
            var player = CreateTestPlayer();
            var enemy = CreateTestGoblin();
            turnManager.InitializeCombat(player, enemy);

            // Act
            bool isEnemyTurn = turnManager.IsEnemyTurn();

            // Assert
            Assert.False(isEnemyTurn);
        }

        [Fact]
        public void IsPlayerTurn_AfterNextTurn_ReturnsFalse()
        {
            // Arrange
            var turnManager = new TurnManager();
            var player = CreateTestPlayer();
            var enemy = CreateTestGoblin();
            turnManager.InitializeCombat(player, enemy);

            // Act
            turnManager.NextTurn();

            // Assert
            Assert.False(turnManager.IsPlayerTurn());
        }

        [Fact]
        public void IsEnemyTurn_AfterNextTurn_ReturnsTrue()
        {
            // Arrange
            var turnManager = new TurnManager();
            var player = CreateTestPlayer();
            var enemy = CreateTestGoblin();
            turnManager.InitializeCombat(player, enemy);

            // Act
            turnManager.NextTurn();

            // Assert
            Assert.True(turnManager.IsEnemyTurn());
        }

        [Fact]
        public void IsPlayerTurn_IsEnemyTurn_AreMutuallyExclusive()
        {
            // Arrange
            var turnManager = new TurnManager();
            var player = CreateTestPlayer();
            var enemy = CreateTestGoblin();
            turnManager.InitializeCombat(player, enemy);

            // Act & Assert - Check over multiple rounds
            for (int i = 0; i < 20; i++)
            {
                bool isPlayer = turnManager.IsPlayerTurn();
                bool isEnemy = turnManager.IsEnemyTurn();

                // Exactly one should be true
                Assert.True(isPlayer ^ isEnemy); // XOR - exactly one is true
                Assert.NotEqual(isPlayer, isEnemy);

                turnManager.NextTurn();
            }
        }

        #endregion

        #region Reset Tests

        [Fact]
        public void Reset_ClearsTurnNumber()
        {
            // Arrange
            var turnManager = new TurnManager();
            var player = CreateTestPlayer();
            var enemy = CreateTestGoblin();
            turnManager.InitializeCombat(player, enemy);

            // Advance several rounds
            for (int i = 0; i < 10; i++)
            {
                turnManager.NextTurn();
            }

            // Act
            turnManager.Reset();

            // Assert
            Assert.Equal(0, turnManager.CurrentTurnNumber);
        }

        [Fact]
        public void Reset_AllowsNewInitialization()
        {
            // Arrange
            var turnManager = new TurnManager();
            var player1 = CreateTestPlayer("Hero1");
            var enemy1 = CreateTestGoblin();
            turnManager.InitializeCombat(player1, enemy1);
            turnManager.NextTurn();
            turnManager.Reset();

            // Act
            var player2 = CreateTestPlayer("Hero2");
            var enemy2 = CreateTestGoblin();
            turnManager.InitializeCombat(player2, enemy2);

            // Assert - Should be able to start fresh
            Assert.Equal(0, turnManager.CurrentTurnNumber);
            Assert.True(turnManager.IsPlayerTurn());
        }

        #endregion

        #region GetTurnStatus Tests

        [Fact]
        public void GetTurnStatus_ReturnsPlayerName()
        {
            // Arrange
            var turnManager = new TurnManager();
            var player = CreateTestPlayer("TestHero");
            var enemy = CreateTestGoblin();
            turnManager.InitializeCombat(player, enemy);

            // Act
            string status = turnManager.GetTurnStatus();

            // Assert
            Assert.Contains("TestHero", status);
            Assert.Contains("Turn 1", status);
        }

        [Fact]
        public void GetTurnStatus_ReturnsEnemyName()
        {
            // Arrange
            var turnManager = new TurnManager();
            var player = CreateTestPlayer("TestHero");
            var enemy = CreateTestGoblin();
            turnManager.InitializeCombat(player, enemy);

            // Act
            turnManager.NextTurn(); // Switch to enemy turn
            string status = turnManager.GetTurnStatus();

            // Assert
            Assert.Contains("Goblin", status);
            Assert.Contains("Turn 1", status);
        }

        [Fact]
        public void GetTurnStatus_IncludesTurnNumber()
        {
            // Arrange
            var turnManager = new TurnManager();
            var player = CreateTestPlayer("TestHero");
            var enemy = CreateTestGoblin();
            turnManager.InitializeCombat(player, enemy);

            // Act - Advance to turn 3
            for (int i = 0; i < 4; i++)
            {
                turnManager.NextTurn();
            }
            string status = turnManager.GetTurnStatus();

            // Assert - Turn number is 0-indexed internally, displayed as 1-indexed
            Assert.Contains("Turn 3", status);
        }

        [Fact]
        public void GetTurnStatus_UpdatesWithTurnProgression()
        {
            // Arrange
            var turnManager = new TurnManager();
            var player = CreateTestPlayer("TestHero");
            var enemy = CreateTestGoblin();
            turnManager.InitializeCombat(player, enemy);

            // Act & Assert
            string status1 = turnManager.GetTurnStatus();
            Assert.Contains("Turn 1", status1);
            Assert.Contains("TestHero", status1);

            turnManager.NextTurn();
            string status2 = turnManager.GetTurnStatus();
            Assert.Contains("Turn 1", status2);
            Assert.Contains("Goblin", status2);

            turnManager.NextTurn();
            string status3 = turnManager.GetTurnStatus();
            Assert.Contains("Turn 2", status3);
            Assert.Contains("TestHero", status3);
        }

        #endregion

        #region CombatantTurn Tests

        [Fact]
        public void CombatantTurn_Constructor_SetsOwner()
        {
            // Act
            var playerTurn = new CombatantTurn(TurnOwner.Player);
            var enemyTurn = new CombatantTurn(TurnOwner.Enemy);

            // Assert
            Assert.Equal(TurnOwner.Player, playerTurn.Owner);
            Assert.Equal(TurnOwner.Enemy, enemyTurn.Owner);
        }

        [Fact]
        public void CombatantTurn_Constructor_WithName_SetsName()
        {
            // Act
            var turn = new CombatantTurn(TurnOwner.Enemy, "Goblin Warrior");

            // Assert
            Assert.Equal(TurnOwner.Enemy, turn.Owner);
            Assert.Equal("Goblin Warrior", turn.SpecificCombatantName);
        }

        [Fact]
        public void CombatantTurn_Constructor_WithoutName_LeavesNameNull()
        {
            // Act
            var turn = new CombatantTurn(TurnOwner.Player);

            // Assert
            Assert.Null(turn.SpecificCombatantName);
        }

        [Fact]
        public void CombatantTurn_Properties_CanBeModified()
        {
            // Arrange
            var turn = new CombatantTurn(TurnOwner.Player);

            // Act
            turn.Owner = TurnOwner.Enemy;
            turn.SpecificCombatantName = "Dragon";

            // Assert
            Assert.Equal(TurnOwner.Enemy, turn.Owner);
            Assert.Equal("Dragon", turn.SpecificCombatantName);
        }

        #endregion

        #region TurnOwner Enum Tests

        [Fact]
        public void TurnOwner_HasPlayerValue()
        {
            // Act
            var owner = TurnOwner.Player;

            // Assert
            Assert.Equal(TurnOwner.Player, owner);
        }

        [Fact]
        public void TurnOwner_HasEnemyValue()
        {
            // Act
            var owner = TurnOwner.Enemy;

            // Assert
            Assert.Equal(TurnOwner.Enemy, owner);
        }

        [Fact]
        public void TurnOwner_PlayerAndEnemy_AreDifferent()
        {
            // Assert
            Assert.NotEqual(TurnOwner.Player, TurnOwner.Enemy);
        }

        #endregion

        #region Edge Case Tests

        [Fact]
        public void GetCurrentTurnOwner_WithoutInitialization_DoesNotThrow()
        {
            // Arrange
            var turnManager = new TurnManager();

            // Act & Assert - Should handle gracefully by refilling queue
            var exception = Record.Exception(() => turnManager.GetCurrentTurnOwner());
            Assert.Null(exception);
        }

        [Fact]
        public void NextTurn_CalledManyTimes_DoesNotThrow()
        {
            // Arrange
            var turnManager = new TurnManager();
            var player = CreateTestPlayer();
            var enemy = CreateTestGoblin();
            turnManager.InitializeCombat(player, enemy);

            // Act & Assert - Advance 100 turns
            var exception = Record.Exception(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    turnManager.NextTurn();
                }
            });
            Assert.Null(exception);
        }

        [Fact]
        public void Reset_CalledMultipleTimes_DoesNotThrow()
        {
            // Arrange
            var turnManager = new TurnManager();
            var player = CreateTestPlayer();
            var enemy = CreateTestGoblin();
            turnManager.InitializeCombat(player, enemy);

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                turnManager.Reset();
                turnManager.Reset();
                turnManager.Reset();
            });
            Assert.Null(exception);
        }

        #endregion
    }
}