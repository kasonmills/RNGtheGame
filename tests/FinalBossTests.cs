using System.Collections.Generic;
using System.Linq;
using Xunit;
using GameLogic.Abilities;
using GameLogic.Combat;
using GameLogic.Entities;
using GameLogic.Systems;
using GameLogic.Entities.Player;
using GameLogic.Entities.Enemies;
using GameLogic.Entities.Enemies.Bosses;
using GameLogic.Entities.Enemies.EnemyTypes;

namespace GameLogic.Tests
{
    /// <summary>
    /// Boss #9, the final boss (decided 2026-10-14, built 2026-11-11): two turns a round with a
    /// slower second turn, picking between attack, force a skip and calling two castle guards;
    /// never the same person twice in a round; no second turn with one opponent left.
    /// </summary>
    public class FinalBossTests
    {
        private static BossEnemy MakeFinalBoss(DifficultyLevel difficulty = DifficultyLevel.Hard)
        {
            var boss = BossDefinitions.GetAllBosses().Last();
            boss.InitializeStats(40);
            var mechanic = (FinalBossMechanic)boss.Mechanic;
            mechanic.Reset();
            mechanic.OnFightStarting(boss, new DifficultyScaler(difficulty));
            return boss;
        }

        private static FinalBossMechanic Mech(BossEnemy boss) => (FinalBossMechanic)boss.Mechanic;

        #region In the boss list

        [Fact]
        public void SheIsTheLastBoss_AndTheFinalBoss()
        {
            var bossManager = new BossManager();
            bossManager.RegisterBosses(BossDefinitions.GetAllBosses().ToArray());
            var last = BossDefinitions.GetAllBosses().Last();

            Assert.Equal(BossStatSheet.FinalBoss, last.BossId);
            Assert.True(bossManager.IsFinalBoss(last.BossId));
            Assert.IsType<FinalBossMechanic>(last.Mechanic);
            Assert.Equal(8, bossManager.TotalBosses); // 9 bosses, the Cyclops Brothers as one entry
        }

        #endregion

        #region Two turns

        [Fact]
        public void TwoTurnsARound_ButNoSecondTurnWithOneOpponentLeft()
        {
            var boss = MakeFinalBoss();

            Assert.Equal(2, Mech(boss).GetTurnsThisRound(boss));
            Assert.True(Mech(boss).ShouldTakeExtraTurn(boss, 2, livingOpponents: 2));
            Assert.False(Mech(boss).ShouldTakeExtraTurn(boss, 2, livingOpponents: 1));
        }

        [Theory]
        [InlineData(DifficultyLevel.Normal, 6, 8)]
        [InlineData(DifficultyLevel.Hard, 4, 6)]
        [InlineData(DifficultyLevel.Difficult, 2, 4)]
        [InlineData(DifficultyLevel.Unfair, 1, 2)]
        public void SecondTurn_IsSlowerByTheDifficultysGap(DifficultyLevel difficulty, int min, int max)
        {
            var rng = new RNGManager(seed: 42);
            var boss = MakeFinalBoss(difficulty);

            for (int i = 0; i < 200; i++)
            {
                int speed = Mech(boss).GetExtraTurnSpeed(boss, 2, firstTurnSpeed: 30, rng, difficulty);
                Assert.InRange(30 - speed, min, max);
            }
        }

        #endregion

        #region Her choices

        [Fact]
        public void Weights_ShiftTowardsSkips_AtHalfHealth()
        {
            var boss = MakeFinalBoss();

            Assert.Equal((50, 25, 25), FinalBossMechanic.GetActionWeights(boss));

            boss.Health = boss.MaxHealth / 2;
            Assert.Equal((40, 40, 20), FinalBossMechanic.GetActionWeights(boss));
        }

        [Fact]
        public void AtFullHealth_SheAttacksAboutHalfTheTime_SkipsAndCallsGuardsAQuarterEach()
        {
            var rng = new RNGManager(seed: 42);
            var target = new Player("TestHero");
            int attacks = 0, skips = 0, guards = 0;
            const int trials = 4000;

            for (int i = 0; i < trials; i++)
            {
                var boss = MakeFinalBoss(); // Fresh each time: no guards alive, no one acted on
                var action = boss.Mechanic.DecideAction(boss, target, rng, out _);
                switch (action.Type)
                {
                    case ActionType.Attack: attacks++; break;
                    case ActionType.InflictEffect: skips++; break;
                    case ActionType.Summon: guards++; break;
                }
            }

            Assert.InRange((double)attacks / trials, 0.46, 0.54);
            Assert.InRange((double)skips / trials, 0.21, 0.29);
            Assert.InRange((double)guards / trials, 0.21, 0.29);
        }

        [Fact]
        public void CallingGuards_BringsTwoCastleGuardsAtThePartysLevel_AndNotAgainWhileTheyStand()
        {
            var rng = new RNGManager(seed: 42);
            var boss = MakeFinalBoss();
            var target = new Player("TestHero");

            GameLogic.Combat.CombatAction summon = null;
            for (int i = 0; i < 200 && summon == null; i++)
            {
                var action = boss.Mechanic.DecideAction(boss, target, rng, out _);
                if (action.Type == ActionType.Summon) summon = action;
            }

            Assert.NotNull(summon);
            Assert.Equal(2, summon.SummonedEnemies.Count);
            Assert.All(summon.SummonedEnemies, guard =>
            {
                Assert.IsType<CastleGuard>(guard);
                Assert.Equal(35, guard.Level); // Her level 40 - 5
            });
            Assert.True(Mech(boss).GuardsAlive());

            for (int i = 0; i < 300; i++)
            {
                Assert.NotEqual(ActionType.Summon, boss.Mechanic.DecideAction(boss, target, rng, out _).Type);
            }

            // Once they are down she can call again
            foreach (var guard in summon.SummonedEnemies) guard.Health = 0;
            Assert.False(Mech(boss).GuardsAlive());
        }

        [Fact]
        public void ASkip_PutsALostTurnOnTheTarget_ButNeverASecondOne()
        {
            var rng = new RNGManager(seed: 42);
            var target = new Player("TestHero");
            target.AddEffect(new SkipTurnEffect()); // Already losing a turn

            for (int i = 0; i < 500; i++)
            {
                var boss = MakeFinalBoss();
                var action = boss.Mechanic.DecideAction(boss, target, rng, out _);
                Assert.NotEqual(ActionType.InflictEffect, action.Type);
            }
        }

        [Fact]
        public void HerTwoActionsInARound_NeverHitTheSamePerson()
        {
            var rng = new RNGManager(seed: 42);
            var opponents = new List<Entity> { new Player("A"), new Player("B"), new Player("C") };

            for (int i = 0; i < 500; i++)
            {
                var boss = MakeFinalBoss();
                var mechanic = Mech(boss);

                mechanic.OnTurnStarting(boss, 1);
                var first = mechanic.ChooseTarget(boss, opponents, rng);
                var firstAction = mechanic.DecideAction(boss, first, rng, out _);

                mechanic.OnTurnStarting(boss, 2);
                var second = mechanic.ChooseTarget(boss, opponents, rng);

                if (firstAction.Type != ActionType.Summon)
                {
                    Assert.NotSame(first, second);
                }

                // A new round - the first target can be picked again
                mechanic.OnTurnStarting(boss, 1);
                Assert.Contains(first, Enumerable.Range(0, 60).Select(_ => mechanic.ChooseTarget(boss, opponents, rng)));
            }
        }

        #endregion

        #region In a fight

        [Fact]
        public void Fight_SheHasTwoSlots_AndSkipsHerSecondAgainstALoneOpponent()
        {
            var combat = new CombatManager(new RNGManager(seed: 42)) { Difficulty = DifficultyLevel.Hard };
            var player = new Player("TestHero") { Level = 10, Health = 50000, MaxHealth = 50000 };
            var boss = BossDefinitions.GetAllBosses().Last();

            var events = new List<CombatEvent>();
            combat.CombatEventOccurred += e => events.Add(e);

            combat.StartCombat(player, new List<EnemyBase> { boss });

            var herSlots = combat.TurnOrder.Where(slot => slot.Members.Contains(boss)).ToList();
            Assert.Equal(2, herSlots.Count);
            Assert.Contains(herSlots, slot => slot.TurnNumber == 2);
            Assert.True(herSlots.First(s => s.TurnNumber == 2).Speed < herSlots.First(s => s.TurnNumber == 1).Speed);

            for (int i = 0; i < 6 && combat.IsCombatActive; i++)
            {
                if (combat.IsWaitingForPlayerAction)
                {
                    combat.SubmitPlayerAction(GameLogic.Combat.CombatAction.Defend(player));
                }
            }

            // Only the player is standing, so her second turn never happens
            Assert.Contains(events, e => e.Type == CombatEventType.TurnSkipped && e.Actor == boss);
        }

        #endregion
    }
}
