using System.Collections.Generic;
using System.Linq;
using Xunit;
using GameLogic.Abilities.EnemyAbilities;
using GameLogic.Combat;
using GameLogic.Entities;
using GameLogic.Systems;
using GameLogic.Entities.Player;
using GameLogic.Entities.Enemies;
using GameLogic.Entities.Enemies.Bosses;

namespace GameLogic.Tests
{
    /// <summary>
    /// Bosses #4/#5, the Cyclops Brothers (placeholder names "Fast" and "Slow", 2026-11-11): one
    /// fight and one place in the boss order; they always attack together; when one falls the
    /// other enrages and brings him back at 25% at the end of the round.
    /// </summary>
    public class CyclopsBrothersTests
    {
        private static BossEnemy GetFast()
        {
            return BossDefinitions.GetAllBosses().First(b => b.BossId == BossStatSheet.CyclopsBrotherOne);
        }

        private static BossReactionContext Context(IReadOnlyList<EnemyBase> allies, IReadOnlyList<EnemyBase> fallen)
        {
            return new BossReactionContext(1, new RNGManager(seed: 1), DifficultyLevel.Hard,
                allies, fallen, new List<Entity>(), _ => { }, _ => false);
        }

        #region In the boss list

        [Fact]
        public void TheBrothers_AreOneEntry_FastWithSlowAsHisPartner()
        {
            var bosses = BossDefinitions.GetAllBosses();
            var fast = bosses[3];

            Assert.Equal(BossStatSheet.CyclopsBrotherOne, fast.BossId);
            Assert.Equal("Fast", fast.Name);
            Assert.NotNull(fast.PartnerBoss);
            Assert.Equal("Slow", fast.PartnerBoss.Name);
            Assert.Equal(BossStatSheet.CyclopsBrotherTwo, fast.PartnerBoss.BossId);
            Assert.DoesNotContain(bosses, b => b.BossId == BossStatSheet.CyclopsBrotherTwo);
            Assert.IsType<CyclopsBrotherMechanic>(fast.Mechanic);
            Assert.IsType<CyclopsBrotherMechanic>(fast.PartnerBoss.Mechanic);
            Assert.NotSame(fast.Mechanic, fast.PartnerBoss.Mechanic);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(16)]
        [InlineData(45)]
        public void Fast_IsFasterThanSlow_AtTheSameLevel(int level)
        {
            var fast = GetFast();
            fast.InitializeStats(level);
            fast.PartnerBoss.InitializeStats(level);

            Assert.True(fast.Speed > fast.PartnerBoss.Speed);
        }

        [Fact]
        public void TheFight_BringsInBothBrothers()
        {
            var fast = GetFast();

            var enemies = fast.GetFightEnemies();

            Assert.Equal(2, enemies.Count);
            Assert.Same(fast, enemies[0]);
            Assert.Same(fast.PartnerBoss, enemies[1]);
        }

        #endregion

        #region The mechanic

        [Fact]
        public void EachBrother_JoinsTheOthersAttack_ButNotHisOwnOrAStrangers()
        {
            var fast = GetFast();
            var slow = fast.PartnerBoss;
            var stranger = new BossEnemy("stranger", "Stranger", "the Test", "A test boss.", 5);
            var target = new Player("TestHero");

            var fastJoins = (IAttackPartnerMechanic)fast.Mechanic;
            var slowJoins = (IAttackPartnerMechanic)slow.Mechanic;

            Assert.True(fastJoins.JoinsAttack(fast, slow, target));
            Assert.True(slowJoins.JoinsAttack(slow, fast, target));
            Assert.False(fastJoins.JoinsAttack(fast, fast, target));
            Assert.False(fastJoins.JoinsAttack(fast, stranger, target));
        }

        [Fact]
        public void WhenHisBrotherFalls_HeEnrages()
        {
            var fast = GetFast();
            var slow = fast.PartnerBoss;
            fast.InitializeStats(16);
            var reactive = (IReactiveMechanic)slow.Mechanic;

            reactive.OnCombatEvent(slow, new CombatEvent { Type = CombatEventType.Defeated, Target = fast },
                Context(new List<EnemyBase>(), new List<EnemyBase> { fast }));

            Assert.True(slow.HasEffect<RageEffect>());
        }

        [Fact]
        public void SomeoneElseFalling_DoesNotEnrageHim()
        {
            var fast = GetFast();
            var slow = fast.PartnerBoss;
            var reactive = (IReactiveMechanic)slow.Mechanic;

            reactive.OnCombatEvent(slow, new CombatEvent { Type = CombatEventType.Defeated, Target = new Player("TestHero") },
                Context(new List<EnemyBase> { fast }, new List<EnemyBase>()));

            Assert.False(slow.HasEffect<RageEffect>());
        }

        [Fact]
        public void AtTheEndOfTheRound_HeRevivesHisBrotherAtAQuarterHealth()
        {
            var fast = GetFast();
            var slow = fast.PartnerBoss;
            fast.InitializeStats(16);
            fast.Health = 0;
            var endOfRound = (IEndOfRoundTurnMechanic)slow.Mechanic;
            var context = Context(new List<EnemyBase>(), new List<EnemyBase> { fast });

            Assert.True(endOfRound.WantsEndOfRoundTurn(slow, context));

            var action = endOfRound.TakeEndOfRoundTurn(slow, context, out string message);

            Assert.Null(action);
            Assert.NotNull(message);
            Assert.Equal(fast.MaxHealth * 25 / 100, fast.Health);
        }

        [Fact]
        public void NoBonusTurn_WhileHisBrotherIsStanding()
        {
            var fast = GetFast();
            var slow = fast.PartnerBoss;
            var endOfRound = (IEndOfRoundTurnMechanic)slow.Mechanic;

            Assert.False(endOfRound.WantsEndOfRoundTurn(slow, Context(new List<EnemyBase> { fast }, new List<EnemyBase>())));
        }

        #endregion

        #region In a fight

        [Fact]
        public void Fight_TheyAttackTogether_AndAFallenBrotherComesBack()
        {
            var rng = new RNGManager(seed: 42);
            var combat = new CombatManager(rng) { Difficulty = DifficultyLevel.Hard };
            var player = new Player("TestHero") { Level = 10, Health = 5000, MaxHealth = 5000 };
            var fast = GetFast();
            var slow = fast.PartnerBoss;

            var events = new List<CombatEvent>();
            combat.CombatEventOccurred += e => events.Add(e);

            combat.StartCombat(player, fast.GetFightEnemies());

            // One hit away from falling
            fast.Health = 1;

            for (int i = 0; i < 40 && combat.IsCombatActive; i++)
            {
                if (combat.IsWaitingForPlayerAction)
                {
                    var target = fast.IsAlive() ? fast : slow;
                    combat.SubmitPlayerAction(GameLogic.Combat.CombatAction.Attack(player, target));
                }
                if (events.Any(e => e.Type == CombatEventType.Revived && e.Target == fast)) break;
            }

            // Their attacks were joined
            Assert.Contains(events, e => e.Type == CombatEventType.AttackJoined);

            // Fast fell, Slow raged, and Fast came back at a quarter health
            int fell = events.FindIndex(e => e.Type == CombatEventType.Defeated && e.Target == fast);
            Assert.True(fell >= 0);
            Assert.Contains(events.Skip(fell), e => e.Type == CombatEventType.EffectApplied && e.Target == slow && e.EffectName == "Rage");
            var revived = events.Skip(fell).First(e => e.Type == CombatEventType.Revived && e.Target == fast);
            Assert.Equal(fast.MaxHealth * 25 / 100, revived.Amount);
        }

        #endregion
    }
}
