using System.Collections.Generic;
using Xunit;
using GameLogic.Core;
using GameLogic.Entities.NPCs;
using GameLogic.Entities.Player;
using GameLogic.Entities.Enemies.EnemyTypes;
using GameLogic.Systems;

namespace GameLogic.Tests
{
    /// <summary>
    /// Day and night (built 2026-11-25 from the user's design, docs/done.md): a 10-minute
    /// countdown per phase, a -60 to +60 second roll after battles, dialogue and cutscenes,
    /// a manual switch, night enemies, and shops closed at night.
    /// </summary>
    public class DayNightTests
    {
        #region The clock

        [Fact]
        public void NewClock_StartsAtDay_WithTenMinutes()
        {
            var clock = new DayNightCycle();
            Assert.Equal(DayPhase.Day, clock.Phase);
            Assert.Equal(600, clock.SecondsLeft);
        }

        [Fact]
        public void Advance_FlipsAtZero_AndStartsAFullCountdown()
        {
            var clock = new DayNightCycle();
            var flips = new List<DayPhase>();
            clock.PhaseChanged += flips.Add;

            clock.Advance(599);
            Assert.False(clock.IsNight);

            clock.Advance(1);
            Assert.True(clock.IsNight);
            Assert.Equal(600, clock.SecondsLeft);
            Assert.Equal(new[] { DayPhase.Night }, flips);
        }

        [Fact]
        public void RollAfterPause_IsMinus60ToPlus60_EveryWholeNumberPossible()
        {
            var rng = new RNGManager(seed: 3);
            var seen = new HashSet<int>();
            for (int i = 0; i < 3000; i++)
            {
                var clock = new DayNightCycle { Rng = rng };
                int roll = clock.RollAfterPause();
                Assert.InRange(roll, -60, 60);
                Assert.Equal(600 + roll, clock.SecondsLeft);
                seen.Add(roll);
            }

            Assert.Equal(121, seen.Count);
        }

        [Fact]
        public void RollToZeroOrBelow_FlipsThePhase()
        {
            var rng = new RNGManager(seed: 5);
            bool flipped = false;
            for (int i = 0; i < 50 && !flipped; i++)
            {
                var clock = new DayNightCycle(DayPhase.Day, 0.5) { Rng = rng };
                int roll = clock.RollAfterPause();
                if (roll <= 0)
                {
                    Assert.True(clock.IsNight);
                    Assert.Equal(600, clock.SecondsLeft);
                    flipped = true;
                }
                else
                {
                    Assert.False(clock.IsNight);
                }
            }

            Assert.True(flipped);
        }

        [Fact]
        public void SwitchPhase_FlipsAndStartsAFullCountdown()
        {
            var clock = new DayNightCycle(DayPhase.Day, 42);
            clock.SwitchPhase();
            Assert.True(clock.IsNight);
            Assert.Equal(600, clock.SecondsLeft);
        }

        [Theory]
        [InlineData(true, 123.5, 123.5)]
        [InlineData(false, null, 600.0)]   // An old save
        [InlineData(false, 9999.0, 600.0)] // Out of range
        [InlineData(true, 0.0, 600.0)]
        public void Restore_KeepsTheSavedTime_OrStartsFull(bool night, double? secondsLeft, double expected)
        {
            var clock = new DayNightCycle(night ? DayPhase.Night : DayPhase.Day, secondsLeft);
            Assert.Equal(night, clock.IsNight);
            Assert.Equal(expected, clock.SecondsLeft);
        }

        #endregion

        #region Night enemies

        [Fact]
        public void NightEnemy_IsTougher_MoreAggressive_AndPaysMore_OnlyOnce()
        {
            var goblin = new Goblin(20); // 98 HP, 10-20 damage, aggression 30
            int aggression = goblin.AggressionLevel;

            DayNightCycle.MakeNightEnemy(goblin);
            DayNightCycle.MakeNightEnemy(goblin); // No stacking

            Assert.True(goblin.IsNightEnemy);
            Assert.Equal(108, goblin.MaxHealth); // 107.8 rounded up
            Assert.Equal(108, goblin.Health);
            Assert.Equal(11, goblin.MinDamage);
            Assert.Equal(22, goblin.MaxDamage);
            Assert.Equal(System.Math.Min(100, aggression + 20), goblin.AggressionLevel);
            Assert.Equal(1.2, goblin.RewardMultiplier, 6);
        }

        #endregion

        #region In the game

        private static GameManager CreateGameInTheWorld()
        {
            var gameManager = new GameManager();
            var weapon = gameManager.GetStarterWeaponChoices()[0];
            var ability = Player.GetAvailableAbilities()[0];
            gameManager.StartNewGame("TestHero", DifficultyLevel.Normal, ability, weapon);
            gameManager.DayNight.Rng = new RNGManager(seed: 11);
            gameManager.ChangeState(GameState.Playing); // Past the intro (a cutscene - it rolls)

            // Back to day with a full 10 minutes, so each test starts from a known clock
            gameManager.SwitchDayNight();
            gameManager.SwitchDayNight();
            Assert.False(gameManager.DayNight.IsNight);
            Assert.Equal(600, gameManager.DayNight.SecondsLeft);
            return gameManager;
        }

        [Fact]
        public void Clock_OnlyRunsInTheWorld()
        {
            var game = CreateGameInTheWorld();

            game.AdvanceClock(10);
            Assert.Equal(590, game.DayNight.SecondsLeft);

            game.OpenPartyMenu();
            game.AdvanceClock(10); // The bag is open - paused
            Assert.Equal(590, game.DayNight.SecondsLeft);
        }

        [Fact]
        public void Menus_PauseWithNoRoll_ButAFightEndingRolls()
        {
            var game = CreateGameInTheWorld();

            game.OpenPauseMenu();
            game.ClosePauseMenu();
            Assert.Equal(600, game.DayNight.SecondsLeft); // No roll for a menu

            game.ChangeState(GameState.Combat);
            game.OpenPauseMenu();                  // Paused mid-fight - the fight isn't over
            game.ClosePauseMenu();
            Assert.Equal(600, game.DayNight.SecondsLeft);

            game.ChangeState(GameState.Playing);   // The fight is over - the roll
            Assert.InRange(game.DayNight.SecondsLeft, 540, 660);
        }

        [Fact]
        public void ShopAndBlacksmith_AreClosedAtNight()
        {
            var game = CreateGameInTheWorld();
            var blacksmith = new Blacksmith("Smith");

            Assert.True(game.SwitchDayNight());
            Assert.True(game.DayNight.IsNight);

            Assert.False(game.OpenBlacksmith(blacksmith));
            Assert.Equal(GameManager.ClosedForTheNightMessage, game.LastRefusalReason);
            Assert.Equal(GameState.Playing, game.CurrentState);

            game.SwitchDayNight(); // Morning
            Assert.True(game.OpenBlacksmith(blacksmith));
            Assert.Null(game.LastRefusalReason);
        }

        [Fact]
        public void PhaseChange_IsPassedOnByTheGameManager()
        {
            var game = CreateGameInTheWorld();
            var phases = new List<DayPhase>();
            game.DayPhaseChanged += phases.Add;

            game.SwitchDayNight();
            game.AdvanceClock(600);

            Assert.Equal(new[] { DayPhase.Night, DayPhase.Day }, phases);
        }

        #endregion
    }
}
