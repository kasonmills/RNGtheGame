using System.Collections.Generic;
using System.Linq;
using Xunit;
using GameLogic.Combat;
using GameLogic.Systems;
using GameLogic.Abilities.EnemyAbilities;
using GameLogic.Abilities.PlayerAbilities;
using GameLogic.Entities.Player;
using GameLogic.Entities.Enemies;
using GameLogic.Entities.Enemies.EnemyTypes;

namespace GameLogic.Tests
{
    /// <summary>
    /// Effects and cooldowns count the owner's own turns (fixed 2026-11-25): they count down at
    /// the end of each of the owner's turns, except the turn they were applied or used in.
    /// </summary>
    public class EffectTimingTests
    {
        [Fact]
        public void EffectAppliedOnTheTargetsOwnTurn_IsNotCountedDownByThatTurn()
        {
            var rng = new RNGManager(seed: 1);
            var player = new Player("TestHero");

            player.StartOwnTurn();
            player.AddEffect(new SlowedEffect(duration: 3, speedReduction: 2));
            player.ProcessTurnEndEffects(rng);

            Assert.Equal(3, player.GetEffect<SlowedEffect>().Duration);
        }

        [Fact]
        public void ThreeTurnEffect_LastsExactlyThreeOfTheTargetsTurns()
        {
            var rng = new RNGManager(seed: 1);
            var player = new Player("TestHero");
            player.AddEffect(new SlowedEffect(duration: 3, speedReduction: 2)); // Between their turns

            for (int turn = 1; turn <= 3; turn++)
            {
                Assert.True(player.HasEffect<SlowedEffect>(), $"turn {turn}");
                player.StartOwnTurn();
                player.ProcessTurnEndEffects(rng);
            }

            Assert.False(player.HasEffect<SlowedEffect>());
        }

        [Fact]
        public void ReappliedDuringTheirTurn_GetsItsFullFreshTime()
        {
            var rng = new RNGManager(seed: 1);
            var player = new Player("TestHero");
            player.AddEffect(new SlowedEffect(duration: 3, speedReduction: 2));

            player.StartOwnTurn();
            player.ProcessTurnEndEffects(rng); // 2 left

            player.StartOwnTurn();
            player.AddEffect(new SlowedEffect(duration: 3, speedReduction: 2)); // Back to 3
            player.ProcessTurnEndEffects(rng);

            Assert.Equal(3, player.GetEffect<SlowedEffect>().Duration);
        }

        [Fact]
        public void Fight_AttackBoost_LastsThreeTurns_AndItsCooldownBlocksThatManyTurns()
        {
            var combat = new CombatManager(new RNGManager(seed: 42));
            var player = new Player("TestHero") { Level = 10, Health = 5000, MaxHealth = 5000 };
            var boost = new AttackBoost();
            player.SetAbility(boost);
            var goblin = new Goblin(1);

            combat.StartCombat(player, new List<EnemyBase> { goblin });
            goblin.MaxHealth = 100000; // Stays in the fight and never flees
            goblin.Health = 100000;

            Assert.True(combat.IsWaitingForPlayerAction);
            combat.SubmitPlayerAction(CombatAction.UseAbility(player, boost, player));
            int cooldown = boost.CurrentCooldown;
            Assert.Equal(boost.GetEffectiveCooldown(), cooldown);
            Assert.True(cooldown >= 3);

            var hasBuff = new List<bool>();
            var canUse = new List<bool>();
            for (int turn = 0; turn <= cooldown; turn++) // The player's next turns
            {
                Assert.True(combat.IsWaitingForPlayerAction);
                hasBuff.Add(player.HasEffect<AttackBoostEffect>());
                canUse.Add(combat.CanUseAbility(player));
                combat.SubmitPlayerAction(CombatAction.Defend(player));
            }

            // The buff is there for the next 3 turns, then gone
            Assert.Equal(new[] { true, true, true, false }, hasBuff.Take(4));
            // The cooldown blocks the next `cooldown` turns; the one after, it can be used
            Assert.All(canUse.Take(cooldown), usable => Assert.False(usable));
            Assert.True(canUse[cooldown]);
        }
    }
}
