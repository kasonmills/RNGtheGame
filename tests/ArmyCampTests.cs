using System.Collections.Generic;
using System.Linq;
using Xunit;
using GameLogic.Combat;
using GameLogic.Systems;
using GameLogic.Items;
using GameLogic.Entities.Player;
using GameLogic.Entities.Enemies;
using GameLogic.Entities.Enemies.EnemyTypes;
using GameLogic.Entities.NPCs.Companions;

namespace GameLogic.Tests
{
    /// <summary>
    /// The army camp challenge (built 2026-11-25 from the rules decided 2026-11-23): waves of
    /// Army Soldiers, the pot, leaving, surrendering, and the per-fight rules behind it.
    /// </summary>
    public class ArmyCampTests
    {
        private static Player StrongPlayer() => new Player("TestHero") { Level = 10, Health = 50000, MaxHealth = 50000 };

        /// <summary>Play the current wave out: every soldier is one hit from down, the player attacks.</summary>
        private static void WinTheWave(CombatManager combat, Player player)
        {
            foreach (var enemy in combat.Enemies) enemy.Health = 1;

            for (int i = 0; i < 200 && combat.IsCombatActive; i++)
            {
                if (combat.IsWaitingForPlayerAction)
                {
                    var target = combat.Enemies.First(e => e.IsAlive());
                    combat.SubmitPlayerAction(CombatAction.Attack(player, target));
                }
            }
        }

        #region The numbers

        [Theory]
        [InlineData(1, 1)]
        [InlineData(5, 5)]
        [InlineData(8, 8)]
        [InlineData(12, 8)]
        public void SoldiersInWave_OneMoreEachWave_AtMost8(int wave, int soldiers)
        {
            Assert.Equal(soldiers, ArmyCampChallenge.SoldiersInWave(wave));
        }

        [Theory]
        [InlineData(1, 10)]
        [InlineData(3, 10)]
        [InlineData(4, 11)]
        [InlineData(7, 12)]
        public void SoldierLevel_UpOneEvery3Waves(int wave, int level)
        {
            Assert.Equal(level, ArmyCampChallenge.SoldierLevel(10, wave));
        }

        [Fact]
        public void Prize_Every5thWave_RarerAsItGoes()
        {
            Assert.Null(ArmyCampChallenge.PrizeRarity(4));
            Assert.Equal(ItemRarity.Uncommon, ArmyCampChallenge.PrizeRarity(5));
            Assert.Null(ArmyCampChallenge.PrizeRarity(6));
            Assert.Equal(ItemRarity.Rare, ArmyCampChallenge.PrizeRarity(10));
            Assert.Equal(ItemRarity.Legendary, ArmyCampChallenge.PrizeRarity(15));
            Assert.Equal(ItemRarity.Legendary, ArmyCampChallenge.PrizeRarity(20));
        }

        #endregion

        #region The wave rules

        [Fact]
        public void Wave_RefusesFleeAndItems_AllowsSurrender()
        {
            var combat = new CombatManager(new RNGManager(seed: 42));
            var player = StrongPlayer();
            var potion = ItemDatabase.GetConsumable("minor health potion");
            player.Inventory.AddItem(potion);

            combat.StartCombat(player, new List<EnemyBase> { new ArmySoldier(5) }, rules: FightRules.ArmyCampWave);
            Assert.True(combat.IsWaitingForPlayerAction);

            Assert.False(combat.ValidateAction(CombatAction.Flee(player), out _));
            Assert.False(combat.ValidateAction(CombatAction.UseItem(player, potion, player), out _));

            CombatResult result = null;
            combat.CombatFinished += r => result = r;
            Assert.True(combat.SubmitPlayerAction(CombatAction.Surrender(player)));

            Assert.False(combat.IsCombatActive);
            Assert.Equal(CombatOutcome.Surrendered, result.Outcome);
            Assert.True(result.PartySurvived);
        }

        [Fact]
        public void NormalFight_RefusesSurrender()
        {
            var combat = new CombatManager(new RNGManager(seed: 42));
            var player = StrongPlayer();
            combat.StartCombat(player, new List<EnemyBase> { new ArmySoldier(5) });

            Assert.False(combat.ValidateAction(CombatAction.Surrender(player), out _));
            Assert.True(combat.ValidateAction(CombatAction.Flee(player), out _));
        }

        [Fact]
        public void Wave_KeepsTheRewards_FixedLineupFights_AndTheFallenStayDown()
        {
            var combat = new CombatManager(new RNGManager(seed: 42));
            var player = StrongPlayer();
            var mage = new CompanionMage(1); // Not "in the party" - the fixed lineup still fights
            int xpBefore = player.Experience;
            int goldBefore = player.Gold;

            CombatResult result = null;
            combat.CombatFinished += r => result = r;
            combat.StartCombat(player, new List<EnemyBase> { new ArmySoldier(5) },
                new List<GameLogic.Entities.Entity> { mage }, rules: FightRules.ArmyCampWave);

            Assert.Contains(mage, combat.Party);
            mage.Health = 0; // Down for the rest of the fight

            WinTheWave(combat, player);

            Assert.Equal(CombatOutcome.Won, result.Outcome);
            Assert.True(result.ExperienceGained > 0);  // Worked out for the pot...
            Assert.Equal(xpBefore, player.Experience); // ...but not given
            Assert.Equal(goldBefore, player.Gold);
            Assert.Empty(result.ItemsGained);          // Soldiers drop no normal loot here
            Assert.Equal(0, mage.Health);              // No "back on their feet"
        }

        #endregion

        #region An attempt

        [Fact]
        public void Attempt_WinAWave_ThenLeave_KeepsTheWholePot_AndHealsEveryone()
        {
            var rng = new RNGManager(seed: 42);
            var combat = new CombatManager(rng);
            var player = StrongPlayer();
            var camp = new ArmyCampChallenge(combat, rng) { Statistics = new StatisticsTracker() };
            int xpBefore = player.Experience;

            Assert.True(camp.Start(player, new List<CompanionBase>(), out _));
            Assert.Equal(ArmyCampState.InWave, camp.State);
            Assert.Single(combat.Enemies);
            Assert.All(combat.Enemies, e => Assert.IsType<ArmySoldier>(e));

            WinTheWave(combat, player);
            Assert.Equal(ArmyCampState.BetweenWaves, camp.State);
            Assert.Equal(1, camp.WavesBeaten);
            Assert.Equal(2, camp.CurrentWave);
            int pot = camp.PotExperience;
            Assert.True(pot > 0);
            Assert.Equal(xpBefore, player.Experience); // Nothing paid yet

            player.Health = 10; // Hurt from the wave
            Assert.True(camp.Leave());

            Assert.Equal(ArmyCampState.Finished, camp.State);
            Assert.True(camp.LastResult.KeptWholePot);
            Assert.Equal(pot, camp.LastResult.ExperienceGained);
            Assert.Equal(player.MaxHealth, player.Health);
            Assert.True(camp.LastResult.NewBestWave);
            Assert.Equal(1, camp.Statistics.ArmyCampBestWave);
        }

        [Fact]
        public void Attempt_SecondWaveHasTwoSoldiers_AndSurrenderingKeepsHalf()
        {
            var rng = new RNGManager(seed: 42);
            var combat = new CombatManager(rng);
            var player = StrongPlayer();
            var camp = new ArmyCampChallenge(combat, rng);

            camp.Start(player, new List<CompanionBase>(), out _);
            WinTheWave(combat, player);
            int pot = camp.PotExperience;
            int potGold = camp.PotGold;

            Assert.True(camp.NextWave());
            Assert.Equal(2, combat.Enemies.Count);

            Assert.True(combat.IsWaitingForPlayerAction); // The fight runs until the player's turn
            Assert.True(combat.SubmitPlayerAction(CombatAction.Surrender(player)));

            Assert.Equal(ArmyCampState.Finished, camp.State);
            Assert.False(camp.LastResult.KeptWholePot);
            Assert.Equal(1, camp.LastResult.WavesBeaten);
            Assert.Equal(pot / 2, camp.LastResult.ExperienceGained);
            Assert.Equal(potGold / 2, camp.LastResult.GoldGained);
        }

        [Fact]
        public void Attempt_RefusesMoreThan3Companions_AndLeaveOnlyBetweenWaves()
        {
            var combat = new CombatManager(new RNGManager(seed: 42));
            var camp = new ArmyCampChallenge(combat, new RNGManager(seed: 1));
            var four = Enumerable.Range(0, 4).Select(_ => (CompanionBase)new CompanionMage(1)).ToList();

            Assert.False(camp.Start(StrongPlayer(), four, out string reason));
            Assert.NotNull(reason);
            Assert.False(camp.Leave());
            Assert.False(camp.NextWave());
        }

        #endregion
    }
}
