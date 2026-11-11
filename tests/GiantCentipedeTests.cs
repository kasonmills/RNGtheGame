using System.Collections.Generic;
using Xunit;
using GameLogic.Abilities.EnemyAbilities;
using GameLogic.Combat;
using GameLogic.Entities;
using GameLogic.Systems;
using GameLogic.Entities.Player;
using GameLogic.Entities.Enemies.Bosses;

namespace GameLogic.Tests
{
    /// <summary>
    /// Boss #3, the Giant House Centipede, and his poison bite (decided 2026-10-16, built
    /// 2026-11-11): a chance to poison on every bite, rolled from a band per difficulty; the
    /// duration rolled from a band too; stacking, but a restack only adds the band's minimum;
    /// and a strong lean towards targets who aren't poisoned yet.
    /// </summary>
    public class GiantCentipedeTests
    {
        private static (BossEnemy boss, GiantCentipedeMechanic mechanic) MakeCentipede(
            DifficultyLevel difficulty = DifficultyLevel.Hard, int level = 12)
        {
            var boss = new BossEnemy(BossStatSheet.Centipede, "Giant House Centipede", "the Many-Legged",
                "A test centipede.", level)
            {
                Mechanic = new GiantCentipedeMechanic()
            };
            var mechanic = (GiantCentipedeMechanic)boss.Mechanic;
            mechanic.Reset();
            mechanic.OnFightStarting(boss, new DifficultyScaler(difficulty));
            return (boss, mechanic);
        }

        #region Bands

        [Theory]
        [InlineData(DifficultyLevel.Normal, 25, 35, 2, 3)]
        [InlineData(DifficultyLevel.Hard, 35, 45, 3, 4)]
        [InlineData(DifficultyLevel.Difficult, 45, 55, 3, 5)]
        [InlineData(DifficultyLevel.Unfair, 55, 65, 4, 6)]
        public void Bands_MatchTheDesign(DifficultyLevel difficulty, int chanceMin, int chanceMax, int turnsMin, int turnsMax)
        {
            Assert.Equal((chanceMin, chanceMax), GiantCentipedeMechanic.GetPoisonChanceBand(difficulty));
            Assert.Equal((turnsMin, turnsMax), GiantCentipedeMechanic.GetPoisonDurationBand(difficulty));
        }

        #endregion

        #region The bite

        [Theory]
        [InlineData(DifficultyLevel.Normal, 0.30)]
        [InlineData(DifficultyLevel.Hard, 0.40)]
        [InlineData(DifficultyLevel.Unfair, 0.60)]
        public void EveryTurnIsABite_AndAboutTheBandsChancePoisons(DifficultyLevel difficulty, double expectedRate)
        {
            var rng = new RNGManager(seed: 42);
            var (boss, mechanic) = MakeCentipede(difficulty);
            var target = new Player("TestHero");
            var (turnsMin, turnsMax) = GiantCentipedeMechanic.GetPoisonDurationBand(difficulty);
            var (damageMin, damageMax, _) = PoisonAttack.GetPoisonStatsForLevel(boss.Level);

            int poisoned = 0;
            const int bites = 4000;
            for (int i = 0; i < bites; i++)
            {
                var action = mechanic.DecideAction(boss, target, rng, out _);

                Assert.Equal(ActionType.Attack, action.Type);
                Assert.Same(target, action.Target);

                if (action.AppliesEffect is CentipedePoisonEffect poison)
                {
                    poisoned++;
                    Assert.InRange(poison.Duration, turnsMin, turnsMax);
                    Assert.Equal(turnsMin, poison.RestackTurns);
                    Assert.Equal(damageMin, poison.MinDamage);
                    Assert.Equal(damageMax, poison.MaxDamage);
                }
                else
                {
                    Assert.Null(action.AppliesEffect);
                }
            }

            Assert.InRange((double)poisoned / bites, expectedRate - 0.04, expectedRate + 0.04);
        }

        #endregion

        #region Stacking

        [Fact]
        public void Restack_AddsAStack_AndOnlyTheBandsMinimumTurns()
        {
            var target = new Player("TestHero");

            target.AddEffect(new CentipedePoisonEffect(duration: 4, minDamage: 5, maxDamage: 10, restackTurns: 3));
            var poison = target.GetEffect<CentipedePoisonEffect>();
            poison.TickDuration(); // 3 turns left

            target.AddEffect(new CentipedePoisonEffect(duration: 4, minDamage: 5, maxDamage: 10, restackTurns: 3));

            Assert.Single(target.ActiveEffects);
            Assert.Equal(2, poison.StackCount);
            Assert.Equal(6, poison.Duration); // 3 left + 3, not back to a full 4
        }

        [Fact]
        public void StackedPoison_HitsOncePerStack()
        {
            var rng = new RNGManager(seed: 3);
            var target = new Player("TestHero") { Health = 1000, MaxHealth = 1000 };
            target.AddEffect(new CentipedePoisonEffect(4, minDamage: 5, maxDamage: 10, restackTurns: 3));
            target.AddEffect(new CentipedePoisonEffect(4, minDamage: 5, maxDamage: 10, restackTurns: 3));
            target.AddEffect(new CentipedePoisonEffect(4, minDamage: 5, maxDamage: 10, restackTurns: 3));

            target.GetEffect<CentipedePoisonEffect>().ApplyEffect(target, rng);

            Assert.InRange(1000 - target.Health, 15, 30); // 3 stacks x 5-10
        }

        [Fact]
        public void OrdinaryPoison_KeepsTheOldRestackRule()
        {
            var target = new Player("TestHero");
            target.AddEffect(new PoisonedEffect(duration: 4, minDamage: 2, maxDamage: 4));
            var poison = target.GetEffect<PoisonedEffect>();
            poison.TickDuration();
            poison.TickDuration();

            target.AddEffect(new PoisonedEffect(duration: 4, minDamage: 2, maxDamage: 4));

            Assert.Equal(2, poison.StackCount);
            Assert.Equal(4, poison.Duration); // Back to full
        }

        #endregion

        #region Targeting

        [Fact]
        public void Targeting_PrefersTheUnpoisoned_AboutThreeAndAHalfToOne()
        {
            var rng = new RNGManager(seed: 42);
            var (boss, mechanic) = MakeCentipede();
            var clean = new Player("Clean");
            var poisoned = new Player("Poisoned");
            poisoned.AddEffect(new PoisonedEffect(3, 2, 4));
            var opponents = new List<Entity> { clean, poisoned };

            int cleanPicks = 0;
            const int picks = 9000;
            for (int i = 0; i < picks; i++)
            {
                if (mechanic.ChooseTarget(boss, opponents, rng) == clean) cleanPicks++;
            }

            // Weights 7 to 2: 7/9 = 78% for the one who isn't poisoned
            Assert.InRange((double)cleanPicks / picks, 0.74, 0.82);
        }

        [Fact]
        public void Targeting_StillPicksThePoisoned_WhenEveryoneIs()
        {
            var rng = new RNGManager(seed: 42);
            var (boss, mechanic) = MakeCentipede();
            var onlyOne = new Player("Poisoned");
            onlyOne.AddEffect(new CentipedePoisonEffect(3, 2, 4, 3));

            Assert.Same(onlyOne, mechanic.ChooseTarget(boss, new List<Entity> { onlyOne }, rng));
        }

        #endregion

        [Fact]
        public void TheCentipedeIsBossThree()
        {
            var bosses = BossDefinitions.GetAllBosses();

            Assert.Equal(BossStatSheet.Centipede, bosses[2].BossId);
            Assert.IsType<GiantCentipedeMechanic>(bosses[2].Mechanic);
        }
    }
}
