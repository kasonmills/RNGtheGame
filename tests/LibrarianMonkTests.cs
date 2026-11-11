using System.Collections.Generic;
using System.Linq;
using Xunit;
using GameLogic.Abilities;
using GameLogic.Combat;
using GameLogic.Systems;
using GameLogic.Entities.Player;
using GameLogic.Entities.Enemies;
using GameLogic.Entities.Enemies.Bosses;

namespace GameLogic.Tests
{
    /// <summary>
    /// Boss #2, the Librarian-Monk, and his shield (decided 2026-11-11): every hit halved
    /// (rounded down) until the shield's own health runs out; up at the start of every fight;
    /// a 3-turn cooldown from the raise; a 40/50/60/70% chance to raise it again by difficulty.
    /// </summary>
    public class LibrarianMonkTests
    {
        private static BossEnemy MakeMonk(int level = 8)
        {
            return new BossEnemy(BossStatSheet.LibrarianMonk, "Librarian-Monk", "Keeper of the Library",
                "A test monk.", level, BossMechanicType.Defensive)
            {
                Mechanic = new LibrarianMonkMechanic()
            };
        }

        #region The shield effect

        [Theory]
        [InlineData(10, 5)]   // 10 -> he takes 5, the shield 5
        [InlineData(11, 6)]   // 11 -> he takes 5 (rounded down), the shield 6
        [InlineData(1, 1)]    // 1 -> he takes 0
        public void Shield_TakesHalfRoundedUp_SoTheHolderTakesHalfRoundedDown(int damage, int expectedBlocked)
        {
            var shield = new ShieldEffect(100);

            int blocked = shield.Absorb(damage);

            Assert.Equal(expectedBlocked, blocked);
            Assert.Equal(100 - expectedBlocked, shield.ShieldHealth);
        }

        [Fact]
        public void Shield_OnlyBlocksWhatItHasLeft_ThenBreaks()
        {
            var shield = new ShieldEffect(4);

            int blocked = shield.Absorb(20); // Wants to block 10, has 4

            Assert.Equal(4, blocked);
            Assert.True(shield.IsBroken);
            Assert.Equal(0, shield.Absorb(20));
        }

        [Fact]
        public void Shield_DoesNotWearOffAtTheEndOfTheRound()
        {
            var shield = new ShieldEffect(50);

            shield.TickDuration();
            shield.TickDuration();

            Assert.False(shield.IsExpired());
            Assert.Equal(EffectType.Buff, shield.Type);
        }

        #endregion

        #region The mechanic

        [Theory]
        [InlineData(DifficultyLevel.Normal, 40)]
        [InlineData(DifficultyLevel.Hard, 50)]
        [InlineData(DifficultyLevel.Difficult, 60)]
        [InlineData(DifficultyLevel.Unfair, 70)]
        public void RaiseChance_RisesWithDifficulty(DifficultyLevel difficulty, int expected)
        {
            Assert.Equal(expected, LibrarianMonkMechanic.GetRaiseChance(difficulty));
        }

        [Theory]
        [InlineData(DifficultyLevel.Normal, 77)]     // 102 x 0.75 = 76.5 -> 77
        [InlineData(DifficultyLevel.Hard, 102)]      // 20 x 1.7 x 3 at level 8
        [InlineData(DifficultyLevel.Unfair, 204)]
        public void FightStart_RaisesAFullShield_ScaledByLevelAndDifficulty(DifficultyLevel difficulty, int expected)
        {
            var monk = MakeMonk(level: 8);
            var mechanic = (LibrarianMonkMechanic)monk.Mechanic;
            mechanic.Reset();

            string message = mechanic.OnFightStarting(monk, new DifficultyScaler(difficulty));

            var shield = monk.GetEffect<ShieldEffect>();
            Assert.NotNull(shield);
            Assert.Equal(expected, shield.MaxShieldHealth);
            Assert.Equal(expected, mechanic.ShieldHealth);
            Assert.NotNull(message);
        }

        [Fact]
        public void Cooldown_StartsOnHisFirstTurn_SoTurnFourIsTheFirstChanceToRaiseAgain()
        {
            var monk = MakeMonk();
            var mechanic = (LibrarianMonkMechanic)monk.Mechanic;
            mechanic.Reset();
            mechanic.OnFightStarting(monk, new DifficultyScaler(DifficultyLevel.Unfair));

            Assert.False(mechanic.CooldownOver(1));
            Assert.False(mechanic.CooldownOver(2));
            Assert.False(mechanic.CooldownOver(3));
            Assert.True(mechanic.CooldownOver(4));
        }

        [Fact]
        public void ShieldDownDuringTheCooldown_HeJustAttacks()
        {
            var rng = new RNGManager(seed: 7);
            var monk = MakeMonk();
            var mechanic = (LibrarianMonkMechanic)monk.Mechanic;
            mechanic.Reset();
            mechanic.OnFightStarting(monk, new DifficultyScaler(DifficultyLevel.Unfair));
            monk.ActiveEffects.Clear(); // Broken on round 1

            for (int turn = 1; turn <= 3; turn++)
            {
                Assert.Null(mechanic.DecideAction(monk, monk, rng, out _)); // Null = normal attack
            }
        }

        [Fact]
        public void ShieldStillUp_HeNeverRaisesAnother()
        {
            var rng = new RNGManager(seed: 7);
            var monk = MakeMonk();
            var mechanic = (LibrarianMonkMechanic)monk.Mechanic;
            mechanic.Reset();
            mechanic.OnFightStarting(monk, new DifficultyScaler(DifficultyLevel.Unfair));

            for (int turn = 1; turn <= 20; turn++)
            {
                Assert.Null(mechanic.DecideAction(monk, monk, rng, out _));
            }
        }

        [Fact]
        public void AfterTheCooldown_HeRaisesItAboutAsOftenAsTheDifficultySays()
        {
            var rng = new RNGManager(seed: 42);
            int raises = 0;
            const int trials = 2000;

            for (int i = 0; i < trials; i++)
            {
                var monk = MakeMonk();
                var mechanic = (LibrarianMonkMechanic)monk.Mechanic;
                mechanic.Reset();
                mechanic.OnFightStarting(monk, new DifficultyScaler(DifficultyLevel.Hard));
                monk.ActiveEffects.Clear(); // Shield broken

                for (int turn = 1; turn <= 3; turn++)
                {
                    mechanic.DecideAction(monk, monk, rng, out _);
                }

                var action = mechanic.DecideAction(monk, monk, rng, out string message); // Turn 4
                if (action != null)
                {
                    Assert.Equal(ActionType.InflictEffect, action.Type);
                    Assert.Same(monk, action.Target);
                    Assert.IsType<ShieldEffect>(action.AppliesEffect);
                    Assert.NotNull(message);
                    raises++;
                }
            }

            double rate = (double)raises / trials;
            Assert.InRange(rate, 0.45, 0.55); // Hard: 50%
        }

        #endregion

        #region In a fight

        [Fact]
        public void TheMonkIsBossTwo_RightAfterSkarn()
        {
            var bosses = BossDefinitions.GetAllBosses();

            Assert.Equal(BossStatSheet.Skarn, bosses[0].BossId);
            Assert.Equal(BossStatSheet.LibrarianMonk, bosses[1].BossId);
            Assert.IsType<LibrarianMonkMechanic>(bosses[1].Mechanic);
            Assert.False(bosses[1].HasFixedLevel);
        }

        [Fact]
        public void Fight_StartsWithHisShieldUp_AndEveryHitOnItIsHalved()
        {
            var rng = new RNGManager(seed: 42);
            var combat = new CombatManager(rng) { Difficulty = DifficultyLevel.Hard };
            var player = new Player("TestHero") { Level = 10, Health = 400, MaxHealth = 400 };
            var monk = MakeMonk();

            var events = new List<CombatEvent>();
            combat.CombatEventOccurred += e => events.Add(e);

            combat.StartCombat(player, new List<EnemyBase> { monk });

            // Raised before round 1, right after the fight started
            int started = events.FindIndex(e => e.Type == CombatEventType.CombatStarted);
            int raised = events.FindIndex(e => e.Type == CombatEventType.EffectApplied
                && e.Target == monk && e.EffectName == "Shield");
            int firstRound = events.FindIndex(e => e.Type == CombatEventType.RoundStarted);
            Assert.True(started >= 0 && raised > started && raised < firstRound);

            // Hit him for a while
            for (int i = 0; i < 40 && combat.IsCombatActive; i++)
            {
                if (combat.IsWaitingForPlayerAction)
                {
                    combat.SubmitPlayerAction(CombatAction.Attack(player, monk));
                }
            }

            // Every blocked hit: the shield took half rounded up, the monk the rest (unless the
            // shield ran out partway, when it takes only what it had left)
            var blocks = events.Select((e, index) => (e, index))
                .Where(x => x.e.Type == CombatEventType.ShieldBlocked).ToList();
            Assert.NotEmpty(blocks);
            foreach (var (block, index) in blocks)
            {
                // AttackLanded follows ShieldBlocked; its Amount is what got through
                int landedIndex = events.FindIndex(index, e => e.Type == CombatEventType.AttackLanded);
                var landed = events[landedIndex];
                Assert.Same(monk, block.Target);
                Assert.Same(monk, landed.Target);

                // Did the shield break on this hit? (EffectRemoved comes once the attack is resolved)
                int nextTurn = events.FindIndex(landedIndex, e => e.Type == CombatEventType.TurnStarted);
                int end = nextTurn < 0 ? events.Count : nextTurn;
                bool brokeHere = events.Skip(landedIndex).Take(end - landedIndex)
                    .Any(e => e.Type == CombatEventType.EffectRemoved && e.Target == monk && e.EffectName == "Shield");

                if (brokeHere)
                {
                    // It could only block what it had left
                    Assert.True(block.Amount <= landed.Amount + 1);
                }
                else
                {
                    // Half rounded down got through: the shield took the same, or 1 more on an odd hit
                    Assert.True(block.Amount == landed.Amount || block.Amount == landed.Amount + 1,
                        $"blocked {block.Amount}, took {landed.Amount}");
                }
            }
        }

        #endregion
    }
}
