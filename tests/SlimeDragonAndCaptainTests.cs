using System.Collections.Generic;
using System.Linq;
using Xunit;
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
    /// Boss #7, the Slime Dragon (draglings: 1 per hit, at most 6 alive, refilled to 3 at the end
    /// of a round; beating him stops the spawning but every dragling must still fall), and boss
    /// #8, the Captain of the Royal Guard (crit chance by difficulty at 2.5x). Built 2026-11-11.
    /// </summary>
    public class SlimeDragonAndCaptainTests
    {
        private static BossEnemy MakeDragon(int level = 26)
        {
            return new BossEnemy(BossStatSheet.SlimeDragon, "Slime Dragon", "the Ever-Dripping", "A test dragon.", level)
            {
                Mechanic = new SlimeDragonMechanic()
            };
        }

        /// <summary>A context whose Summon adds to `allies`, like a real fight.</summary>
        private static BossReactionContext Context(List<EnemyBase> allies, List<EnemyBase> summoned = null)
        {
            return new BossReactionContext(1, new RNGManager(seed: 1), DifficultyLevel.Hard,
                allies, new List<EnemyBase>(), new List<Entity>(), _ => { },
                enemy => { allies.Add(enemy); summoned?.Add(enemy); return true; });
        }

        #region Draglings

        [Fact]
        public void Dragling_IsAtThePartysLevel_AndInTheDiminishingRewardsGroup()
        {
            var dragon = MakeDragon(level: 26);

            var dragling = SlimeDragonMechanic.MakeDragling(dragon);

            Assert.Equal(21, dragling.Level);
            Assert.Equal(Dragling.RewardsGroup, dragling.DiminishingRewardsGroup);
            Assert.Equal(EnemyType.Common, dragling.Type);
            Assert.Equal(120, dragling.MaxHealth);
        }

        [Fact]
        public void AHitOnHim_SpawnsOneDragling()
        {
            var dragon = MakeDragon();
            var allies = new List<EnemyBase>();
            var reactive = (IReactiveMechanic)dragon.Mechanic;

            reactive.OnCombatEvent(dragon, new CombatEvent { Type = CombatEventType.AttackLanded, Target = dragon }, Context(allies));

            Assert.Single(allies);
            Assert.IsType<Dragling>(allies[0]);
        }

        [Fact]
        public void AHitOnSomeoneElse_SpawnsNothing()
        {
            var dragon = MakeDragon();
            var allies = new List<EnemyBase>();
            var reactive = (IReactiveMechanic)dragon.Mechanic;

            reactive.OnCombatEvent(dragon, new CombatEvent { Type = CombatEventType.AttackLanded, Target = new Player("TestHero") }, Context(allies));
            reactive.OnCombatEvent(dragon, new CombatEvent { Type = CombatEventType.AttackMissed, Target = dragon }, Context(allies));

            Assert.Empty(allies);
        }

        [Fact]
        public void NoMoreThanSixDraglings_FromHits()
        {
            var dragon = MakeDragon();
            var allies = new List<EnemyBase>();
            var reactive = (IReactiveMechanic)dragon.Mechanic;

            for (int i = 0; i < 10; i++)
            {
                reactive.OnCombatEvent(dragon, new CombatEvent { Type = CombatEventType.AttackLanded, Target = dragon }, Context(allies));
            }

            Assert.Equal(SlimeDragonMechanic.MaxDraglings, allies.Count);
        }

        [Theory]
        [InlineData(0, true, 3)]
        [InlineData(1, true, 2)]
        [InlineData(2, true, 1)]
        [InlineData(3, false, 0)]
        [InlineData(5, false, 0)]
        public void EndOfRound_RefillsToThree(int alive, bool wantsTurn, int expectedSpawned)
        {
            var dragon = MakeDragon();
            var allies = Enumerable.Range(0, alive).Select(_ => (EnemyBase)new Dragling(21)).ToList();
            var summoned = new List<EnemyBase>();
            var context = Context(allies, summoned);
            var endOfRound = (IEndOfRoundTurnMechanic)dragon.Mechanic;

            Assert.Equal(wantsTurn, endOfRound.WantsEndOfRoundTurn(dragon, context));

            if (wantsTurn)
            {
                endOfRound.TakeEndOfRoundTurn(dragon, context, out _);
                Assert.Equal(expectedSpawned, summoned.Count);
                Assert.Equal(SlimeDragonMechanic.RefillTo, allies.Count);
            }
        }

        [Fact]
        public void Fight_DraglingsJoin_AndBeatingHimStopsThemButNotTheFight()
        {
            var rng = new RNGManager(seed: 42);
            var combat = new CombatManager(rng) { Difficulty = DifficultyLevel.Hard };
            var player = new Player("TestHero") { Level = 10, Health = 50000, MaxHealth = 50000 };
            var dragon = MakeDragon();

            var events = new List<CombatEvent>();
            combat.CombatEventOccurred += e => events.Add(e);

            combat.StartCombat(player, new List<EnemyBase> { dragon });

            // Phase 1: fight until draglings have joined (a hit, or the end-of-round refill)
            for (int i = 0; i < 30 && combat.IsCombatActive
                && !events.Any(e => e.Type == CombatEventType.EnemyJoined); i++)
            {
                if (combat.IsWaitingForPlayerAction)
                {
                    combat.SubmitPlayerAction(GameLogic.Combat.CombatAction.Attack(player, dragon));
                }
            }
            Assert.Contains(events, e => e.Type == CombatEventType.EnemyJoined && e.Target is Dragling);

            // Phase 2: one hit from falling - bring him down
            dragon.Health = 1;
            for (int i = 0; i < 60 && combat.IsCombatActive && dragon.IsAlive(); i++)
            {
                if (combat.IsWaitingForPlayerAction)
                {
                    combat.SubmitPlayerAction(GameLogic.Combat.CombatAction.Attack(player, dragon));
                }
            }

            Assert.False(dragon.IsAlive());
            int fell = events.FindLastIndex(e => e.Type == CombatEventType.Defeated && e.Target == dragon);

            // Draglings came (the end-of-round refill at the latest)
            Assert.Contains(events.Take(fell), e => e.Type == CombatEventType.EnemyJoined && e.Target is Dragling);

            // No new draglings once he's down
            Assert.DoesNotContain(events.Skip(fell), e => e.Type == CombatEventType.EnemyJoined);

            // The draglings left keep the fight going
            if (combat.Enemies.Any(e => e.IsAlive() && !combat.HasFled(e)))
            {
                Assert.True(combat.IsCombatActive);
            }
        }

        [Fact]
        public void TheSlimeDragonIsBossSeven_TheSixthEntry()
        {
            var bosses = BossDefinitions.GetAllBosses();

            Assert.Equal(BossStatSheet.SlimeDragon, bosses[5].BossId);
            Assert.IsType<SlimeDragonMechanic>(bosses[5].Mechanic);
        }

        #endregion

        #region Captain

        [Theory]
        [InlineData(DifficultyLevel.Normal, 50)]
        [InlineData(DifficultyLevel.Hard, 55)]
        [InlineData(DifficultyLevel.Difficult, 60)]
        [InlineData(DifficultyLevel.Unfair, 65)]
        public void Captain_CritChanceRisesWithDifficulty_AtTwoAndAHalfTimes(DifficultyLevel difficulty, int expected)
        {
            var captain = BossDefinitions.GetAllBosses().First(b => b.BossId == BossStatSheet.Captain);
            var mechanic = (IFightStartMechanic)captain.Mechanic;

            mechanic.OnFightStarting(captain, new DifficultyScaler(difficulty));

            Assert.Equal(expected, captain.CritChance);
            Assert.Equal(2.5, captain.CritMultiplier);
        }

        [Fact]
        public void Captain_IsBossEight_TheSeventhEntry_AndRisk()
        {
            var bosses = BossDefinitions.GetAllBosses();
            var captain = bosses[6];

            Assert.Equal(BossStatSheet.Captain, captain.BossId);
            Assert.Equal("Risk", captain.Name);
            Assert.Equal("Captain of the Royal Guard", captain.Title);
            Assert.IsType<CaptainMechanic>(captain.Mechanic);
        }

        [Fact]
        public void Captain_InAFight_HasTheDifficultysCritChance()
        {
            var combat = new CombatManager(new RNGManager(seed: 42)) { Difficulty = DifficultyLevel.Unfair };
            var player = new Player("TestHero") { Level = 10, Health = 5000, MaxHealth = 5000 };
            var captain = BossDefinitions.GetAllBosses().First(b => b.BossId == BossStatSheet.Captain);

            combat.StartCombat(player, new List<EnemyBase> { captain });

            Assert.Equal(65, captain.CritChance);
        }

        #endregion
    }
}
