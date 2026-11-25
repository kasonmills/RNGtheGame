using System;
using Xunit;
using GameLogic.Systems;
using GameLogic.Entities.Player;
using GameLogic.Entities.NPCs.Companions;

namespace GameLogic.Tests
{
    /// <summary>
    /// The party's side of the difficulty numbers (decided 2026-11-23, built 2026-11-24): the hit
    /// chance penalty and the party's max health.
    /// </summary>
    public class DifficultyPartyTests
    {
        [Theory]
        [InlineData(DifficultyLevel.Normal, 0)]
        [InlineData(DifficultyLevel.Hard, 5)]
        [InlineData(DifficultyLevel.Difficult, 10)]
        [InlineData(DifficultyLevel.Unfair, 15)]
        public void PartyAccuracyPenalty_MatchesTheDecidedNumbers(DifficultyLevel difficulty, int expected)
        {
            Assert.Equal(expected, GameSettings.GetPartyAccuracyPenalty(difficulty));
        }

        [Theory]
        [InlineData(DifficultyLevel.Normal, 1.2)]
        [InlineData(DifficultyLevel.Hard, 1.0)]
        [InlineData(DifficultyLevel.Difficult, 0.9)]
        [InlineData(DifficultyLevel.Unfair, 0.8)]
        public void PartyMaxHealthMultiplier_MatchesTheDecidedNumbers(DifficultyLevel difficulty, double expected)
        {
            Assert.Equal(expected, GameSettings.GetPartyMaxHealthMultiplier(difficulty), 3);
        }

        [Theory]
        [InlineData(1.2, 120)]
        [InlineData(1.0, 100)]
        [InlineData(0.9, 90)]
        [InlineData(0.8, 80)]
        public void ApplyMaxHealthMultiplier_ScalesANewPlayer_AndKeepsThemAtFullHealth(double multiplier, int expected)
        {
            var player = new Player("TestHero"); // 100 HP

            player.ApplyMaxHealthMultiplier(multiplier);

            Assert.Equal(expected, player.MaxHealth);
            Assert.Equal(expected, player.Health);
            Assert.Equal(multiplier, player.MaxHealthMultiplier, 3);
        }

        [Fact]
        public void ApplyMaxHealthMultiplier_TwiceWithTheSameNumber_DoesNotStack()
        {
            var player = new Player("TestHero");

            player.ApplyMaxHealthMultiplier(0.8);
            player.ApplyMaxHealthMultiplier(0.8);

            Assert.Equal(80, player.MaxHealth);
        }

        [Fact]
        public void RestoreMaxHealthMultiplier_LeavesTheSavedMaxHealthAlone()
        {
            var player = new Player("TestHero") { MaxHealth = 250, Health = 250 };

            player.RestoreMaxHealthMultiplier(0.8);

            Assert.Equal(250, player.MaxHealth);
            Assert.Equal(0.8, player.MaxHealthMultiplier, 3);
        }

        [Fact]
        public void CompanionLevelUp_HealthGainIsScaled()
        {
            var mage = new CompanionMage(1); // 88 HP
            mage.ApplyMaxHealthMultiplier(0.8); // 70.4 -> 70
            Assert.Equal(70, mage.MaxHealth);

            mage.LevelUp(); // +10 at level 2, x0.8 = 8

            Assert.Equal(78, mage.MaxHealth);
            Assert.Equal(78, mage.Health);
        }

        [Fact]
        public void RecruitCompanion_CopiesThePlayersMultiplier()
        {
            var player = new Player("TestHero");
            player.ApplyMaxHealthMultiplier(1.2);
            var mage = new CompanionMage(1); // 88 HP
            var party = new PartyManager();

            party.RecruitCompanion(player, mage);

            Assert.Equal(1.2, mage.MaxHealthMultiplier, 3);
            Assert.Equal(106, mage.MaxHealth); // 88 x 1.2 = 105.6 -> 106
        }
    }
}
