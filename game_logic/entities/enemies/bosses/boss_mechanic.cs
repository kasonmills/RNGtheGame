using GameLogic.Combat;
using GameLogic.Entities;
using GameLogic.Systems;
// This file's namespace (GameLogic.Entities.Enemies.Bosses) nests under GameLogic.Entities,
// whose CombatAction enum wins enclosing-namespace lookup over both a plain `using
// GameLogic.Combat;` and even a `using CombatAction = ...` alias -- enclosing-namespace
// members beat using directives entirely. So every CombatAction below must be fully
// qualified as GameLogic.Combat.CombatAction, not just imported.

namespace GameLogic.Entities.Enemies.Bosses
{
    /// <summary>
    /// A boss's unique combat mechanic. BossEnemy is one shared concrete class (not
    /// subclassed per boss), so bespoke per-boss behavior plugs in here instead.
    /// </summary>
    public interface IBossMechanic
    {
        /// <summary>
        /// Called at the start of each fight against this boss, so repeat encounters
        /// don't carry over stale state from a previous fight.
        /// </summary>
        void Reset();

        /// <summary>
        /// Decide this boss's action for the current turn. Return null to fall back to
        /// the generic Behavior-based AI (CombatManager.DetermineEnemyAction's existing
        /// switch) instead. `message`, if non-null, is logged before the action resolves.
        /// </summary>
        GameLogic.Combat.CombatAction DecideAction(BossEnemy self, Entity target, RNGManager rng, out string message);
    }

    /// <summary>
    /// Optional, for a boss that acts in more than one turn slot per round (the Princess:
    /// two turns per round, decided 2026-10-14). A boss mechanic implements this next to
    /// IBossMechanic. Each turn is its own slot in the round, with its own speed, and is
    /// never shared with another enemy. A boss whose mechanic doesn't implement this gets
    /// one turn per round, as normal.
    ///
    /// Turn numbers start at 1 (1 = the boss's normal turn, 2 = its first extra turn...).
    /// </summary>
    public interface IMultiTurnMechanic
    {
        /// <summary>
        /// How many turns the boss gets this round (1 = normal). Asked once at the start of
        /// every round, when the turn slots are built.
        /// </summary>
        int GetTurnsThisRound(BossEnemy self);

        /// <summary>
        /// The speed of an extra turn (turnNumber 2 or more), given the speed of the boss's
        /// first turn this round. Asked when the round's slots are built. CombatManager keeps
        /// the result at 1 or more.
        /// </summary>
        int GetExtraTurnSpeed(BossEnemy self, int turnNumber, int firstTurnSpeed, RNGManager rng, DifficultyLevel difficulty);

        /// <summary>
        /// Asked when an extra turn (turnNumber 2 or more) comes up, before anything happens.
        /// Return false to skip it (e.g. the Princess skips her second turn when only one
        /// opponent is left). `livingOpponents` = party members still standing.
        /// </summary>
        bool ShouldTakeExtraTurn(BossEnemy self, int turnNumber, int livingOpponents);

        /// <summary>
        /// Called right before the boss acts on one of its turns (including turn 1), so
        /// DecideAction can tell its turns apart. Turn 1 also marks the start of a new round
        /// for this boss.
        /// </summary>
        void OnTurnStarting(BossEnemy self, int turnNumber);
    }

    /// <summary>
    /// Optional, for a boss that reacts to things happening in the fight outside its own turn
    /// (the Cyclops enraging when his brother falls, the Slime Dragon spawning draglings when
    /// hit). A boss mechanic implements this next to IBossMechanic.
    ///
    /// How it works: the events from one action (or one effect tick) are collected while it
    /// resolves. Once it has been resolved and reported, every living boss still in the fight
    /// whose mechanic implements this gets those events, one at a time, in order. Changes a
    /// reaction makes (health, effects) are reported as events too, and bosses can react to
    /// those in the same way - up to 10 passes, so two reactions can't set each other off
    /// forever. A boss does not react to its own defeat (it is no longer alive).
    ///
    /// Useful events: AttackLanded (Actor hit Target), Damaged (Target lost Amount health, from
    /// any source), Defeated, Revived, EffectApplied / EffectRemoved. A reaction is not a turn:
    /// it doesn't end the boss's Defend or use up its slot.
    /// </summary>
    public interface IReactiveMechanic
    {
        void OnCombatEvent(BossEnemy self, CombatEvent combatEvent, BossReactionContext context);
    }

    /// <summary>
    /// Optional, for a boss that can take a bonus turn at the end of a round (the Cyclops
    /// reviving his fallen brother, the Slime Dragon spawning draglings). A boss mechanic
    /// implements this next to IBossMechanic.
    ///
    /// How it works: once every slot of the round has been played, each living boss still in
    /// the fight whose mechanic implements this is asked WantsEndOfRoundTurn. If it says no,
    /// nothing is added (not shown, not skipped). If it says yes, a bonus slot is added last in
    /// TurnOrder (EndOfRoundTurnAdded event) and TakeEndOfRoundTurn is called when it comes up -
    /// still before the round's end (effect countdown, cooldowns, next round's speeds). Several
    /// bosses wanting one act fastest first.
    ///
    /// A bonus turn is not one of the boss's normal turns: it doesn't end the boss's Defend,
    /// doesn't use up a lost turn (SkipTurnEffect), doesn't tick its status effects, doesn't
    /// change its speed for next round, and an attack on it isn't joined (IAttackPartnerMechanic).
    /// </summary>
    public interface IEndOfRoundTurnMechanic
    {
        /// <summary>
        /// Asked once when the round's slots have all been played (e.g. "is my brother down?",
        /// "are fewer than 3 draglings alive?").
        /// </summary>
        bool WantsEndOfRoundTurn(BossEnemy self, BossReactionContext context);

        /// <summary>
        /// Take the bonus turn. Do it through the context (revive one of context.FallenAllies by
        /// setting its Health, or context.Summon new enemies) and return null, or return an action
        /// for CombatManager to resolve (an attack, a Defend...). Either way the changes are
        /// reported as combat events. `message`, if non-null, is logged first.
        /// </summary>
        GameLogic.Combat.CombatAction TakeEndOfRoundTurn(BossEnemy self, BossReactionContext context, out string message);
    }

    /// <summary>
    /// Optional, for a boss that joins another enemy's attack (the Cyclops Brothers always
    /// attack together, decided 2026-10-15). A boss mechanic implements this next to
    /// IBossMechanic.
    ///
    /// After an enemy attacks on its own turn (not on an end-of-round bonus turn), every other
    /// living boss still in the fight whose mechanic implements this is asked JoinsAttack. If
    /// yes, it makes a normal attack on the same target straight after (AttackJoined event, then
    /// the attack's own events). Only attacks are joined - abilities, Defend and the rest aren't.
    ///
    /// Joining is not the joiner's turn: it doesn't end his Defend, doesn't use up his own slot
    /// and doesn't change his speed for next round. If the first attack already took the target
    /// down, the joiner doesn't attack (my default - not decided).
    /// </summary>
    public interface IAttackPartnerMechanic
    {
        bool JoinsAttack(BossEnemy self, Entity attacker, Entity target);
    }

    /// <summary>
    /// What a boss can see and do outside its own DecideAction (IReactiveMechanic,
    /// IEndOfRoundTurnMechanic). Built by CombatManager.
    /// </summary>
    public class BossReactionContext
    {
        private readonly System.Action<string> _log;
        private readonly System.Func<EnemyBase, bool> _summon;

        public int Round { get; }
        public RNGManager Rng { get; }
        public DifficultyLevel Difficulty { get; }

        /// <summary>The other enemies still in the fight (not down, not fled).</summary>
        public System.Collections.Generic.IReadOnlyList<EnemyBase> Allies { get; }

        /// <summary>
        /// The other enemies who are down (not fled). Revive one by setting its Health above 0
        /// (e.g. the Cyclops bringing his brother back at 25%) - CombatManager reports the revive
        /// and the revived enemy is back in the turn order from the next round.
        /// </summary>
        public System.Collections.Generic.IReadOnlyList<EnemyBase> FallenAllies { get; }

        /// <summary>The party members still standing.</summary>
        public System.Collections.Generic.IReadOnlyList<Entity> Opponents { get; }

        public BossReactionContext(int round, RNGManager rng, DifficultyLevel difficulty,
            System.Collections.Generic.IReadOnlyList<EnemyBase> allies,
            System.Collections.Generic.IReadOnlyList<EnemyBase> fallenAllies,
            System.Collections.Generic.IReadOnlyList<Entity> opponents,
            System.Action<string> log,
            System.Func<EnemyBase, bool> summon)
        {
            Round = round;
            Rng = rng;
            Difficulty = difficulty;
            Allies = allies;
            FallenAllies = fallenAllies;
            Opponents = opponents;
            _log = log;
            _summon = summon;
        }

        /// <summary>Add a line to the combat log (e.g. "The Cyclops roars in fury!").</summary>
        public void Log(string message) => _log?.Invoke(message);

        /// <summary>
        /// Bring a new enemy into the fight on this boss's side (e.g. a dragling). It acts from
        /// the next round. Returns false if it couldn't join (the fight is over, it is already
        /// in the fight, it is down, or the fight is at CombatManager.MaxActiveEnemies).
        /// </summary>
        public bool Summon(EnemyBase enemy) => _summon != null && _summon(enemy);
    }

    /// <summary>
    /// Skarn, the Eagle Bear's unique mechanic:
    /// - Above 50% HP: always grapples - one hit, then holds the target (slowing them,
    ///   itself taking no attack action) for a randomized 1-3 turns.
    /// - Below 50% HP: mixes grapples, normal attacks, and a two-turn charge/stoop
    ///   attack (telegraph, then a big hit).
    /// All percentages/numbers here are a first-pass balance choice - easy to retune.
    /// </summary>
    public class EagleBearMechanic : IBossMechanic
    {
        private int _grappleTurnsRemaining;
        private Entity _grappledTarget;
        private bool _isCharging;
        private Entity _chargeTarget;

        public void Reset()
        {
            _grappleTurnsRemaining = 0;
            _grappledTarget = null;
            _isCharging = false;
            _chargeTarget = null;
        }

        public GameLogic.Combat.CombatAction DecideAction(BossEnemy self, Entity target, RNGManager rng, out string message)
        {
            // Turn 2 of a charge: unleash the stoop attack.
            if (_isCharging)
            {
                _isCharging = false;
                var chargeTarget = _chargeTarget;
                _chargeTarget = null;
                message = $"{self.Name} slams down in a devastating stoop attack!";
                return new GameLogic.Combat.CombatAction(ActionType.Attack, self, chargeTarget) { DamageMultiplier = 2.5 };
            }

            // Holding an active grapple - no attack of its own while it lasts.
            if (_grappleTurnsRemaining > 0)
            {
                _grappleTurnsRemaining--;
                var holdTarget = _grappledTarget;
                if (_grappleTurnsRemaining <= 0)
                {
                    _grappledTarget = null;
                    message = $"{self.Name} releases {holdTarget.Name} from its grip!";
                }
                else
                {
                    message = $"{self.Name} tightens its grip, holding {holdTarget.Name} fast!";
                }
                return GameLogic.Combat.CombatAction.None(self);
            }

            float healthPercent = (float)self.Health / self.MaxHealth;

            if (healthPercent > 0.5f)
            {
                return StartGrapple(self, target, rng, out message);
            }

            // Below 50% HP: mix grapples, normal attacks, and charge cycles.
            int roll = rng.Roll(1, 100);
            if (roll <= 40)
            {
                return StartGrapple(self, target, rng, out message);
            }
            if (roll <= 70)
            {
                _isCharging = true;
                _chargeTarget = target;
                message = $"{self.Name} rears back, wings flaring - it's about to dive!";
                return GameLogic.Combat.CombatAction.None(self);
            }

            // Remaining chance: fall back to a normal attack via the generic AI.
            message = null;
            return null;
        }

        private GameLogic.Combat.CombatAction StartGrapple(BossEnemy self, Entity target, RNGManager rng, out string message)
        {
            int duration = rng.Roll(1, 3);
            _grappleTurnsRemaining = duration - 1; // this turn is the hit itself
            _grappledTarget = target;
            message = $"{self.Name} seizes {target.Name} in a crushing grapple!";

            var slow = new Abilities.EnemyAbilities.GrappledEffect(duration, speedReduction: 6);
            return new GameLogic.Combat.CombatAction(ActionType.Attack, self, target) { AppliesEffect = slow };
        }
    }
}
