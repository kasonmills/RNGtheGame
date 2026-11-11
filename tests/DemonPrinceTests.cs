using System.Collections.Generic;
using System.Linq;
using Xunit;
using GameLogic.Combat;
using GameLogic.Systems;
using GameLogic.Entities.Player;
using GameLogic.Entities.Enemies.Bosses;

namespace GameLogic.Tests
{
    /// <summary>
    /// Boss #6, the Demon Prince (decided 2026-10-22, built 2026-11-11 with the existing
    /// conditions only): every hit puts a random condition on its target, each equally likely;
    /// a 0-damage hit still lands it.
    /// </summary>
    public class DemonPrinceTests
    {
        private static BossEnemy MakePrince(int level = 21)
        {
            return new BossEnemy(BossStatSheet.DemonPrince, "Demon Prince", "Prince of Chaos", "A test prince.", level)
            {
                Mechanic = new DemonPrinceMechanic()
            };
        }

        [Fact]
        public void Pool_HasTheFiveExistingConditions()
        {
            var names = DemonPrinceMechanic.ConditionPool.Select(c => c.Name).ToList();

            Assert.Equal(new[] { "Poisoned", "Burning", "Bleeding", "Grappled", "Pinned" }, names);
        }

        [Fact]
        public void EachCondition_BuildsAnEffectWithItsName()
        {
            foreach (var (name, build) in DemonPrinceMechanic.ConditionPool)
            {
                var effect = build(21);
                Assert.NotNull(effect);
                Assert.Equal(name, effect.Name);
                Assert.True(effect.Duration > 0, name);
            }
        }

        [Theory]
        [InlineData(1, 3, 4)]    // Poison formula 2-4 -> 3 a turn; slow 4
        [InlineData(21, 10, 6)]  // 7-14 -> 10; slow 4 + 2
        [InlineData(45, 19, 8)]  // 13-26 -> 19; slow 4 + 4
        public void Strength_ComesFromHisLevel(int level, int expectedDot, int expectedSlow)
        {
            Assert.Equal(expectedDot, DemonPrinceMechanic.DotDamage(level));
            Assert.Equal(expectedSlow, DemonPrinceMechanic.SlowAmount(level));
        }

        [Fact]
        public void EveryTurnIsAnAttack_WithOneConditionEachEquallyLikely()
        {
            var rng = new RNGManager(seed: 42);
            var prince = MakePrince();
            var target = new Player("TestHero");
            var counts = new Dictionary<string, int>();
            const int attacks = 5000;

            for (int i = 0; i < attacks; i++)
            {
                var action = prince.Mechanic.DecideAction(prince, target, rng, out _);

                Assert.Equal(ActionType.Attack, action.Type);
                Assert.Same(target, action.Target);
                Assert.True(action.EffectEvenWithoutDamage);
                Assert.NotNull(action.AppliesEffect);

                counts.TryGetValue(action.AppliesEffect.Name, out int n);
                counts[action.AppliesEffect.Name] = n + 1;
            }

            Assert.Equal(5, counts.Count);
            foreach (var pair in counts)
            {
                Assert.InRange((double)pair.Value / attacks, 0.17, 0.23); // 1 in 5 each
            }
        }

        [Fact]
        public void TheDemonPrinceIsBossSix_TheFifthEntry()
        {
            var bosses = BossDefinitions.GetAllBosses();

            // Skarn, Monk, Centipede, the Cyclops Brothers (one entry), the Demon Prince
            Assert.Equal(BossStatSheet.DemonPrince, bosses[4].BossId);
            Assert.IsType<DemonPrinceMechanic>(bosses[4].Mechanic);
        }
    }
}
