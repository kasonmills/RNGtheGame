using System.Collections.Generic;
using System.Linq;
using Xunit;
using GameLogic.Combat;
using GameLogic.Systems;
using GameLogic.Entities.Player;
using GameLogic.Entities.Enemies;
using GameLogic.Entities.Enemies.EnemyTypes;

namespace GameLogic.Tests
{
    /// <summary>
    /// The regular enemies built from the standard enemy (docs/enemy_roster.md, built
    /// 2026-11-24). The numbers are the sheet's worked numbers (Hard, before difficulty).
    /// </summary>
    public class EnemyRosterTests
    {
        #region The standard enemy

        [Theory]
        [InlineData(1, 26, 2, 6, 0, 10)]
        [InlineData(20, 140, 12, 25, 5, 15)]
        [InlineData(40, 260, 22, 45, 10, 20)]
        public void StandardEnemy_MatchesTheSheet(int level, int health, int minDamage, int maxDamage, int defense, int speed)
        {
            Assert.Equal(health, StandardEnemy.StandardHealth(level));
            Assert.Equal(minDamage, StandardEnemy.StandardMinDamage(level));
            Assert.Equal(maxDamage, StandardEnemy.StandardMaxDamage(level));
            Assert.Equal(defense, StandardEnemy.StandardDefense(level));
            Assert.Equal(speed, StandardEnemy.StandardSpeed(level));
        }

        #endregion

        #region Worked numbers

        private static void AssertStats(EnemyBase enemy, int health, int minDamage, int maxDamage)
        {
            Assert.Equal(health, enemy.MaxHealth);
            Assert.Equal(health, enemy.Health);
            Assert.Equal(minDamage, enemy.MinDamage);
            Assert.Equal(maxDamage, enemy.MaxDamage);
        }

        [Fact]
        public void Goblin_Level1And20()
        {
            var goblin = new Goblin(1);
            AssertStats(goblin, 18, 2, 5);
            Assert.Equal(12, goblin.Speed); // 10 + 2
            Assert.Equal(75, goblin.Accuracy);

            AssertStats(new Goblin(20), 98, 10, 20);
        }

        [Fact]
        public void Bandit_IsTheStandardEnemy_WithHighCrit()
        {
            var bandit = new Bandit(20);
            AssertStats(bandit, 140, 12, 25);
            Assert.Equal(5, bandit.Defense);
            Assert.Equal(18, bandit.Speed); // 15 + 3
            Assert.Equal(25, bandit.CritChance);
        }

        [Fact]
        public void Wolf_Slime_Spider_Level1()
        {
            AssertStats(new Wolf(1), 21, 2, 5);
            AssertStats(new Slime(1), 36, 1, 4);
            AssertStats(new Spider(1), 21, 2, 5);
        }

        [Fact]
        public void Level20_Enemies()
        {
            AssertStats(new BigSlime(20), 350, 11, 22);
            AssertStats(new Fungusoar(20), 350, 16, 32);
            AssertStats(new Imp(20), 84, 10, 20);
            AssertStats(new KingdomBArcher(20), 112, 13, 28);

            var beetle = new GiantBeetle(20);
            AssertStats(beetle, 252, 12, 25);
            Assert.Equal(12, beetle.Defense); // 5 x 2.5 = 12.5, rounded down

            var soldier = new KingdomBSoldier(20);
            AssertStats(soldier, 182, 13, 28);
            Assert.Equal(10, soldier.Defense);
        }

        [Fact]
        public void Level40_Enemies()
        {
            var deserter = new Deserter(40);
            AssertStats(deserter, 286, 22, 45);
            Assert.Equal(15, deserter.Defense);

            AssertStats(new CastleGuard(40), 260, 22, 45);

            var dragon = new Dragon(40);
            AssertStats(dragon, 1040, 44, 90);
            Assert.Equal(30, dragon.Defense);
            Assert.Equal(EnemyType.Elite, dragon.Type);
        }

        #endregion

        #region Behaviour

        [Fact]
        public void FleeLines_FollowTheSheet()
        {
            Assert.Equal(0.25, new Goblin(1).FleeBelowHealth, 3);
            Assert.Equal(100, new Goblin(1).FleeAttemptChance); // Cowardly - always tries
            Assert.Equal(0.40, new Bandit(1).FleeBelowHealth, 3);
            Assert.Equal(0.30, new Dragling(1).FleeBelowHealth, 3); // Wild ones; never in the boss fight
            Assert.Equal(0.0, new Slime(1).FleeBelowHealth, 3);
            Assert.Equal(0.0, new Dragon(30).FleeBelowHealth, 3);
            Assert.Equal(0.0, new ArmySoldier(20).FleeBelowHealth, 3);
        }

        [Fact]
        public void TargetingAndRows_FollowTheSheet()
        {
            Assert.Equal(EnemyTargetPattern.Random, new Goblin(1).Targeting);
            Assert.Equal(EnemyTargetPattern.WeakestDefense, new Spider(1).Targeting);
            Assert.Equal(EnemyTargetPattern.LastAttacker, new GiantBeetle(20).Targeting);
            Assert.Equal(EnemyTargetPattern.BiggestThreat, new Fungusoar(10).Targeting);
            Assert.True(new KingdomBArcher(20).IsBackRow);
            Assert.True(new Imp(20).IsBackRow);
            Assert.False(new KingdomBSoldier(20).IsBackRow);
        }

        [Fact]
        public void BigSlime_SplitsIntoTwoSlimes_AtItsLevel()
        {
            var spawns = new BigSlime(18).GetSpawnsOnDefeat();

            Assert.Equal(2, spawns.Count);
            Assert.All(spawns, s => Assert.IsType<Slime>(s));
            Assert.All(spawns, s => Assert.Equal(18, s.Level));
            Assert.All(spawns, s => Assert.Equal(EnemyRoster.SlimeRewardsGroup, s.DiminishingRewardsGroup));
        }

        [Fact]
        public void OnHitEffect_AlwaysOrNever_ByChance()
        {
            var rng = new RNGManager(seed: 42);
            int bleeds = 0;
            for (int i = 0; i < 1000; i++)
            {
                if (new Wolf(10).RollOnHitEffect(rng) != null) bleeds++;
            }

            Assert.InRange(bleeds, 100, 200); // About 15%
            Assert.Null(new Goblin(10).RollOnHitEffect(rng)); // Plain hits
        }

        [Fact]
        public void Fight_BeatingABigSlime_BringsInTwoSlimes()
        {
            var rng = new RNGManager(seed: 42);
            var combat = new CombatManager(rng);
            var player = new Player("TestHero") { Level = 10, Health = 5000, MaxHealth = 5000 };
            var bigSlime = new BigSlime(10);

            var events = new List<CombatEvent>();
            combat.CombatEventOccurred += e => events.Add(e);

            combat.StartCombat(player, new List<EnemyBase> { bigSlime });
            bigSlime.Health = 1; // One hit away

            for (int i = 0; i < 40 && combat.IsCombatActive
                && !events.Any(e => e.Type == CombatEventType.EnemyJoined); i++)
            {
                if (combat.IsWaitingForPlayerAction)
                {
                    combat.SubmitPlayerAction(CombatAction.Attack(player, bigSlime));
                }
            }

            Assert.False(bigSlime.IsAlive());
            Assert.Equal(2, events.Count(e => e.Type == CombatEventType.EnemyJoined && e.Target is Slime));
            Assert.True(combat.IsCombatActive); // The slimes keep the fight going
        }

        #endregion
    }
}
