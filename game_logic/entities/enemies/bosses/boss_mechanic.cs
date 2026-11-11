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
    /// Optional, for a boss that does something before the first round (the Librarian-Monk
    /// starts every fight with his shield raised - shown in the pre-fight cutscene). A boss
    /// mechanic implements this next to IBossMechanic.
    ///
    /// Called once per fight by CombatManager.StartCombat, after the boss's stats are rebuilt
    /// and the difficulty applied, and right after the CombatStarted event. Changes it makes
    /// (an effect added, health) are reported as combat events. Not a turn. Returns a line for
    /// the combat log, or null.
    /// </summary>
    public interface IFightStartMechanic
    {
        string OnFightStarting(BossEnemy self, DifficultyScaler scaler);
    }

    /// <summary>
    /// Optional, for a boss that picks its own targets (the Giant House Centipede favours
    /// party members who aren't poisoned yet). A boss mechanic implements this next to
    /// IBossMechanic.
    ///
    /// Asked on each of the boss's turns, before DecideAction; the chosen target is what
    /// DecideAction is given. `opponents` = the party members still standing (never empty).
    /// Return null to keep the normal pick (70% the most wounded, otherwise random).
    /// </summary>
    public interface ITargetingMechanic
    {
        Entity ChooseTarget(BossEnemy self, System.Collections.Generic.IReadOnlyList<Entity> opponents, RNGManager rng);
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

    /// <summary>
    /// The Librarian-Monk's unique mechanic - a shield (decided 2026-11-03 / 2026-11-11):
    /// - He starts every fight with it raised (the pre-fight cutscene shows him raising it).
    ///   That counts as raised on his turn 1, so the cooldown starts on his first turn.
    /// - While it is up, every attack on him is halved, rounded down; the shield takes the
    ///   rest out of its own health and breaks at 0 (ShieldEffect).
    /// - Raising it has a 3-turn cooldown, counted from the turn it was raised. Once the
    ///   cooldown is over, on any of his turns where the shield is down he has a chance to
    ///   raise it again - Normal 40%, Hard 50%, Difficult 60%, Unfair 70% - which uses his turn.
    ///   Otherwise he attacks as normal.
    /// - The shield's health: ShieldHealthAtLevel1 grown with his level like boss health
    ///   (x(1 + 10% a level) x3), then the difficulty on top. Every raise is a full shield.
    /// Turns are his own turns that come up - a lost turn (SkipTurnEffect) doesn't count (my default).
    /// </summary>
    public class LibrarianMonkMechanic : IBossMechanic, IFightStartMechanic
    {
        /// <summary>Shield health at level 1, before growth and difficulty (my default - 20 makes it about a third of his health).</summary>
        public const int ShieldHealthAtLevel1 = 20;

        /// <summary>Turns from one raise until he can raise it again (decided 2026-11-11).</summary>
        public const int ShieldCooldownTurns = 3;

        private int _turn;
        private int _raisedOnTurn;
        private int _shieldHealth;
        private DifficultyLevel _difficulty = DifficultyLevel.Hard;

        /// <summary>The full health of each shield he raises this fight.</summary>
        public int ShieldHealth => _shieldHealth;

        /// <summary>The chance (1-100) he raises the shield on a turn when he can.</summary>
        public static int GetRaiseChance(DifficultyLevel difficulty)
        {
            return difficulty switch
            {
                DifficultyLevel.Normal => 40,
                DifficultyLevel.Hard => 50,
                DifficultyLevel.Difficult => 60,
                DifficultyLevel.Unfair => 70,
                _ => 50
            };
        }

        public void Reset()
        {
            _turn = 0;
            _raisedOnTurn = 0;
            _shieldHealth = 0;
        }

        public string OnFightStarting(BossEnemy self, DifficultyScaler scaler)
        {
            _difficulty = scaler.GetGameDifficulty();
            _shieldHealth = scaler.ScaleForDifficulty(
                DifficultyScaler.BossHealthAtLevel(ShieldHealthAtLevel1, self.Level));

            self.AddEffect(new Abilities.ShieldEffect(_shieldHealth));
            _raisedOnTurn = 1; // The cooldown starts on his first turn
            return $"{self.Name} raises a shimmering shield!";
        }

        /// <summary>Whether the cooldown is over on this turn.</summary>
        public bool CooldownOver(int turn) => turn - _raisedOnTurn >= ShieldCooldownTurns;

        public GameLogic.Combat.CombatAction DecideAction(BossEnemy self, Entity target, RNGManager rng, out string message)
        {
            _turn++;

            if (!self.HasEffect<Abilities.ShieldEffect>() && CooldownOver(_turn)
                && rng.Roll(1, 100) <= GetRaiseChance(_difficulty))
            {
                _raisedOnTurn = _turn;
                message = $"{self.Name} raises his shield again!";
                return GameLogic.Combat.CombatAction.Inflict(self, self, new Abilities.ShieldEffect(_shieldHealth));
            }

            // Shield up, cooldown not over, or the roll failed: a normal attack via the generic AI
            message = null;
            return null;
        }
    }

    /// <summary>
    /// The Giant House Centipede's unique mechanic - a poison bite (decided 2026-10-16):
    /// - Every one of his turns is a normal bite (an attack). Each bite has a chance to poison,
    ///   rolled fresh from his difficulty's band every time: Normal 25-35%, Hard 35-45%,
    ///   Difficult 45-55%, Unfair 55-65%. The poison only lands if the bite hits.
    /// - The poison's duration is rolled too: Normal 2-3 turns, Hard 3-4, Difficult 3-5,
    ///   Unfair 4-6. Damage per turn comes from his level (the existing poison formula).
    /// - Poison stacks, but a restack only adds the band's minimum duration (CentipedePoisonEffect).
    /// - Targeting: party members who aren't poisoned are about 3-4x as likely to be picked
    ///   (weights 7 to 2, so 3.5x) - he doesn't strictly avoid the poisoned ones.
    /// </summary>
    public class GiantCentipedeMechanic : IBossMechanic, IFightStartMechanic, ITargetingMechanic
    {
        /// <summary>Pick weight for a party member who isn't poisoned, and for one who is (3.5x).</summary>
        public const int UnpoisonedWeight = 7;
        public const int PoisonedWeight = 2;

        private DifficultyLevel _difficulty = DifficultyLevel.Hard;

        /// <summary>The poison chance band (min %, max %) for a difficulty.</summary>
        public static (int min, int max) GetPoisonChanceBand(DifficultyLevel difficulty)
        {
            return difficulty switch
            {
                DifficultyLevel.Normal => (25, 35),
                DifficultyLevel.Hard => (35, 45),
                DifficultyLevel.Difficult => (45, 55),
                DifficultyLevel.Unfair => (55, 65),
                _ => (35, 45)
            };
        }

        /// <summary>The poison duration band (min turns, max turns) for a difficulty.</summary>
        public static (int min, int max) GetPoisonDurationBand(DifficultyLevel difficulty)
        {
            return difficulty switch
            {
                DifficultyLevel.Normal => (2, 3),
                DifficultyLevel.Hard => (3, 4),
                DifficultyLevel.Difficult => (3, 5),
                DifficultyLevel.Unfair => (4, 6),
                _ => (3, 4)
            };
        }

        public void Reset()
        {
        }

        public string OnFightStarting(BossEnemy self, DifficultyScaler scaler)
        {
            _difficulty = scaler.GetGameDifficulty();
            return null;
        }

        public Entity ChooseTarget(BossEnemy self, System.Collections.Generic.IReadOnlyList<Entity> opponents, RNGManager rng)
        {
            if (opponents == null || opponents.Count == 0) return null;

            int total = 0;
            foreach (var opponent in opponents)
            {
                total += Weight(opponent);
            }

            int roll = rng.Roll(1, total);
            foreach (var opponent in opponents)
            {
                roll -= Weight(opponent);
                if (roll <= 0) return opponent;
            }
            return opponents[opponents.Count - 1];
        }

        private static int Weight(Entity opponent)
        {
            return opponent.HasEffect<Abilities.EnemyAbilities.PoisonedEffect>() ? PoisonedWeight : UnpoisonedWeight;
        }

        public GameLogic.Combat.CombatAction DecideAction(BossEnemy self, Entity target, RNGManager rng, out string message)
        {
            message = null;
            var bite = new GameLogic.Combat.CombatAction(ActionType.Attack, self, target);

            var (chanceMin, chanceMax) = GetPoisonChanceBand(_difficulty);
            int chance = rng.Roll(chanceMin, chanceMax);
            if (rng.Roll(1, 100) <= chance)
            {
                var (durationMin, durationMax) = GetPoisonDurationBand(_difficulty);
                var (damageMin, damageMax, _) = Abilities.EnemyAbilities.PoisonAttack.GetPoisonStatsForLevel(self.Level);
                bite.AppliesEffect = new Abilities.EnemyAbilities.CentipedePoisonEffect(
                    rng.Roll(durationMin, durationMax), damageMin, damageMax, restackTurns: durationMin);
            }

            return bite;
        }
    }

    /// <summary>
    /// The Cyclops Brothers' unique mechanic (decided 2026-10-15, built 2026-11-11). Each brother
    /// has his own copy; they find each other through the fight (the other boss with this
    /// mechanic), so nothing has to link them.
    /// - They always attack together: when one attacks on his own turn, the other joins in on
    ///   the same target (IAttackPartnerMechanic). Only attacks are joined. Joining isn't his
    ///   turn - his Defend stays up and his own slot still comes.
    /// - When one falls, the other enrages (the existing Rage effect, at his level - my default:
    ///   the enrage details aren't decided) and, in a bonus turn at the end of the round, brings
    ///   his brother back at 25% health. No limit on revives. Both down in the same round = win.
    /// - On his own turns he uses the normal boss AI.
    /// </summary>
    public class CyclopsBrotherMechanic : IBossMechanic, IAttackPartnerMechanic, IReactiveMechanic, IEndOfRoundTurnMechanic
    {
        /// <summary>A revived brother comes back with this share of his health (decided).</summary>
        public const int ReviveHealthPercent = 25;

        public void Reset()
        {
        }

        public GameLogic.Combat.CombatAction DecideAction(BossEnemy self, Entity target, RNGManager rng, out string message)
        {
            message = null;
            return null; // The normal boss AI
        }

        private static bool IsBrother(BossEnemy self, Entity other)
        {
            return other != self && other is BossEnemy boss && boss.Mechanic is CyclopsBrotherMechanic;
        }

        public bool JoinsAttack(BossEnemy self, Entity attacker, Entity target)
        {
            return IsBrother(self, attacker);
        }

        public void OnCombatEvent(BossEnemy self, CombatEvent combatEvent, BossReactionContext context)
        {
            if (combatEvent.Type != CombatEventType.Defeated || !IsBrother(self, combatEvent.Target)) return;

            var (damageBonus, defenseReduction, duration) =
                Abilities.EnemyAbilities.Rage.GetRageStatsForLevel(self.Level);
            self.AddEffect(new Abilities.EnemyAbilities.RageEffect(duration, damageBonus, defenseReduction));
            context.Log($"{self.Name} roars in fury at {combatEvent.Target.Name}'s fall! (+{damageBonus}% damage, -{defenseReduction}% defense for {duration} turns)");
        }

        private static EnemyBase FallenBrother(BossEnemy self, BossReactionContext context)
        {
            foreach (var fallen in context.FallenAllies)
            {
                if (IsBrother(self, fallen)) return fallen;
            }
            return null;
        }

        public bool WantsEndOfRoundTurn(BossEnemy self, BossReactionContext context)
        {
            return FallenBrother(self, context) != null;
        }

        public GameLogic.Combat.CombatAction TakeEndOfRoundTurn(BossEnemy self, BossReactionContext context, out string message)
        {
            var brother = FallenBrother(self, context);
            if (brother == null)
            {
                message = null;
                return null;
            }

            brother.Health = System.Math.Max(1, brother.MaxHealth * ReviveHealthPercent / 100);
            message = $"{self.Name} hauls {brother.Name} back to his feet!";
            return null;
        }
    }

    /// <summary>
    /// The Demon Prince's unique mechanic - a random condition on every hit (decided 2026-10-22,
    /// built 2026-11-11 with the existing conditions only):
    /// - Every one of his turns is an attack. Every hit puts one condition on the target, picked
    ///   from the pool with equal chance. Nothing on a miss; a hit that does 0 damage still lands it.
    /// - Several conditions can sit on one target at once. Landing one the target already has
    ///   follows that condition's normal rule (damage over time stacks, the rest refresh) - my
    ///   default, not decided.
    /// - Each condition's strength and duration come from his level (my default numbers below).
    /// - His lower attack power is in his stat sheet row (BossStatSheet).
    /// His unique conditions aren't designed yet (to-do section 2) - each one is one more entry
    /// in ConditionPool.
    /// </summary>
    public class DemonPrinceMechanic : IBossMechanic
    {
        /// <summary>
        /// The conditions he picks from, each built for his level. Add his unique ones here.
        /// </summary>
        public static readonly System.Collections.Generic.IReadOnlyList<(string Name, System.Func<int, Abilities.AbilityEffect> Build)> ConditionPool =
            new System.Collections.Generic.List<(string, System.Func<int, Abilities.AbilityEffect>)>
            {
                ("Poisoned", level =>
                {
                    var (min, max, duration) = Abilities.EnemyAbilities.PoisonAttack.GetPoisonStatsForLevel(level);
                    return new Abilities.EnemyAbilities.PoisonedEffect(duration, min, max);
                }),
                ("Burning", level => new Abilities.BurnEffect(potency: DotDamage(level), duration: 3)),
                ("Bleeding", level => new Abilities.BleedEffect(potency: DotDamage(level), duration: 3)),
                ("Grappled", level => new Abilities.EnemyAbilities.GrappledEffect(duration: 2, speedReduction: SlowAmount(level))),
                ("Pinned", level => new NPCs.Companions.PinnedEffect(duration: 2, speedReduction: SlowAmount(level))),
            };

        /// <summary>Damage per turn for his burn and bleed: the middle of the poison formula at his level.</summary>
        public static int DotDamage(int level)
        {
            var (min, max, _) = Abilities.EnemyAbilities.PoisonAttack.GetPoisonStatsForLevel(level);
            return (min + max) / 2;
        }

        /// <summary>Speed taken away by his grapple and pin: 4 + 1 per 10 levels.</summary>
        public static int SlowAmount(int level) => 4 + level / 10;

        public void Reset()
        {
        }

        public GameLogic.Combat.CombatAction DecideAction(BossEnemy self, Entity target, RNGManager rng, out string message)
        {
            var (_, build) = ConditionPool[rng.Roll(0, ConditionPool.Count - 1)];
            message = null;

            return new GameLogic.Combat.CombatAction(ActionType.Attack, self, target)
            {
                AppliesEffect = build(self.Level),
                EffectEvenWithoutDamage = true
            };
        }
    }

    /// <summary>
    /// The Slime Dragon's unique mechanic - he spawns draglings (decided 2026-11-09 / 2026-11-11):
    /// - Every time an attack lands on him, a dragling joins the fight (it acts from the next
    ///   round) - unless MaxDraglings are already alive.
    /// - When a round ends with fewer than RefillTo draglings alive, he takes a bonus turn and
    ///   spawns enough to bring it back to RefillTo. With RefillTo or more alive, no bonus turn.
    /// - Beating him stops the spawning, but the fight goes on until every dragling is down too
    ///   (decided 2026-11-11 - a fallen boss doesn't react or take bonus turns, and a fight is only
    ///   won when every enemy is down).
    /// - Draglings fight at the party's level (his level - 5). Diminishing rewards after 10 in one fight.
    /// - On his own turns he uses the normal boss AI.
    /// Numbers are my defaults (2026-11-11): 1 per hit, at most 6 alive, refill to 3.
    /// </summary>
    public class SlimeDragonMechanic : IBossMechanic, IReactiveMechanic, IEndOfRoundTurnMechanic
    {
        public const int DraglingsPerHit = 1;
        public const int MaxDraglings = 6;
        public const int RefillTo = 3;

        public void Reset()
        {
        }

        public GameLogic.Combat.CombatAction DecideAction(BossEnemy self, Entity target, RNGManager rng, out string message)
        {
            message = null;
            return null; // The normal boss AI
        }

        /// <summary>Draglings alive in the fight.</summary>
        public static int CountDraglings(BossReactionContext context)
        {
            int count = 0;
            foreach (var ally in context.Allies)
            {
                if (ally is EnemyTypes.Dragling) count++;
            }
            return count;
        }

        /// <summary>A dragling at the party's level (the dragon's level - 5, at least 1).</summary>
        public static EnemyTypes.Dragling MakeDragling(BossEnemy self)
        {
            return new EnemyTypes.Dragling(System.Math.Max(1, self.Level - DifficultyScaler.BossLevelsAboveParty));
        }

        private static int Spawn(BossEnemy self, BossReactionContext context, int howMany)
        {
            int spawned = 0;
            for (int i = 0; i < howMany; i++)
            {
                if (!context.Summon(MakeDragling(self))) break; // The fight is full (8 enemies)
                spawned++;
            }
            return spawned;
        }

        public void OnCombatEvent(BossEnemy self, CombatEvent combatEvent, BossReactionContext context)
        {
            if (combatEvent.Type != CombatEventType.AttackLanded || combatEvent.Target != self) return;

            int room = MaxDraglings - CountDraglings(context);
            if (room <= 0) return;

            if (Spawn(self, context, System.Math.Min(DraglingsPerHit, room)) > 0)
            {
                context.Log($"Slime splatters from {self.Name} and squirms into a dragling!");
            }
        }

        public bool WantsEndOfRoundTurn(BossEnemy self, BossReactionContext context)
        {
            return CountDraglings(context) < RefillTo;
        }

        public GameLogic.Combat.CombatAction TakeEndOfRoundTurn(BossEnemy self, BossReactionContext context, out string message)
        {
            int spawned = Spawn(self, context, RefillTo - CountDraglings(context));
            message = spawned > 0
                ? $"{self.Name} heaves, and {spawned} dragling{(spawned > 1 ? "s" : "")} ooze{(spawned > 1 ? "" : "s")} out!"
                : null;
            return null;
        }
    }

    /// <summary>
    /// The Captain of the Royal Guard's unique mechanic (Risk as a boss - boss #8): a very high
    /// crit chance that rises with the difficulty, at a very high crit multiplier (2.5x, from his
    /// stat sheet row). Set when the fight starts. My defaults (2026-11-11): Normal 50%, Hard 55%,
    /// Difficult 60%, Unfair 65%. On his turns he uses the normal boss AI.
    /// </summary>
    public class CaptainMechanic : IBossMechanic, IFightStartMechanic
    {
        public const double CritMultiplier = 2.5;

        public static int GetCritChance(DifficultyLevel difficulty)
        {
            return difficulty switch
            {
                DifficultyLevel.Normal => 50,
                DifficultyLevel.Hard => 55,
                DifficultyLevel.Difficult => 60,
                DifficultyLevel.Unfair => 65,
                _ => 55
            };
        }

        public void Reset()
        {
        }

        public string OnFightStarting(BossEnemy self, DifficultyScaler scaler)
        {
            self.CritChance = GetCritChance(scaler.GetGameDifficulty());
            self.CritMultiplier = CritMultiplier;
            return null;
        }

        public GameLogic.Combat.CombatAction DecideAction(BossEnemy self, Entity target, RNGManager rng, out string message)
        {
            message = null;
            return null; // The normal boss AI
        }
    }

    /// <summary>
    /// The final boss's unique mechanic (the Princess - a hidden twist: no foreshadowing text
    /// anywhere). Decided 2026-10-14, built 2026-11-11:
    /// - Two turns per round, each its own opponent slot. Her second turn's speed is her speed
    ///   minus a random amount - a wide gap on easy difficulties, narrow on hard ones.
    /// - Each turn she picks one of: force a skip (a random opponent loses their next turn),
    ///   attack (a random target), or call two castle guards (only when none of hers are alive;
    ///   it uses her turn).
    /// - Her two actions in a round never hit the same person. With only one opponent left, her
    ///   second turn is skipped.
    /// My defaults (2026-11-11), not decided: second-turn slowdown Normal 6-8, Hard 4-6,
    /// Difficult 2-4, Unfair 1-2; action weights attack 50 / skip 25 / guards 25, shifting to
    /// 40 / 40 / 20 at half health or below (she leans on skips when hurt). A skip goes to an
    /// opponent who hasn't already lost a turn; if they have, she attacks instead.
    /// </summary>
    public class FinalBossMechanic : IBossMechanic, IMultiTurnMechanic, IFightStartMechanic, ITargetingMechanic
    {
        public const int GuardsPerCall = 2;

        private DifficultyLevel _difficulty = DifficultyLevel.Hard;
        private readonly System.Collections.Generic.HashSet<Entity> _actedOnThisRound = new();
        private readonly System.Collections.Generic.List<EnemyBase> _guards = new();

        /// <summary>How much slower her second turn is (min, max) on a difficulty.</summary>
        public static (int min, int max) GetSecondTurnSlowdown(DifficultyLevel difficulty)
        {
            return difficulty switch
            {
                DifficultyLevel.Normal => (6, 8),
                DifficultyLevel.Hard => (4, 6),
                DifficultyLevel.Difficult => (2, 4),
                DifficultyLevel.Unfair => (1, 2),
                _ => (4, 6)
            };
        }

        /// <summary>Action weights (attack, skip, guards) for her health.</summary>
        public static (int attack, int skip, int guards) GetActionWeights(BossEnemy self)
        {
            bool hurt = self.Health * 2 <= self.MaxHealth;
            return hurt ? (40, 40, 20) : (50, 25, 25);
        }

        /// <summary>Whether any guard she called is still standing.</summary>
        public bool GuardsAlive()
        {
            foreach (var guard in _guards)
            {
                if (guard.IsAlive()) return true;
            }
            return false;
        }

        public void Reset()
        {
            _actedOnThisRound.Clear();
            _guards.Clear();
        }

        public string OnFightStarting(BossEnemy self, DifficultyScaler scaler)
        {
            _difficulty = scaler.GetGameDifficulty();
            return null;
        }

        // ---- Two turns a round ----

        public int GetTurnsThisRound(BossEnemy self) => 2;

        public int GetExtraTurnSpeed(BossEnemy self, int turnNumber, int firstTurnSpeed, RNGManager rng, DifficultyLevel difficulty)
        {
            var (min, max) = GetSecondTurnSlowdown(difficulty);
            return firstTurnSpeed - rng.Roll(min, max);
        }

        public bool ShouldTakeExtraTurn(BossEnemy self, int turnNumber, int livingOpponents)
        {
            return livingOpponents > 1;
        }

        public void OnTurnStarting(BossEnemy self, int turnNumber)
        {
            if (turnNumber == 1)
            {
                _actedOnThisRound.Clear(); // A new round - anyone can be picked again
            }
        }

        // ---- Targets: random, never the same person twice in a round ----

        public Entity ChooseTarget(BossEnemy self, System.Collections.Generic.IReadOnlyList<Entity> opponents, RNGManager rng)
        {
            var choices = new System.Collections.Generic.List<Entity>();
            foreach (var opponent in opponents)
            {
                if (!_actedOnThisRound.Contains(opponent)) choices.Add(opponent);
            }
            if (choices.Count == 0) choices.AddRange(opponents);
            if (choices.Count == 0) return null;

            return choices[rng.Roll(0, choices.Count - 1)];
        }

        // ---- Her turn ----

        public GameLogic.Combat.CombatAction DecideAction(BossEnemy self, Entity target, RNGManager rng, out string message)
        {
            var (attackWeight, skipWeight, guardWeight) = GetActionWeights(self);
            if (GuardsAlive()) guardWeight = 0;

            int roll = rng.Roll(1, attackWeight + skipWeight + guardWeight);

            // Call two castle guards (at the party's level)
            if (roll > attackWeight + skipWeight)
            {
                var called = new System.Collections.Generic.List<EnemyBase>();
                int guardLevel = System.Math.Max(1, self.Level - DifficultyScaler.BossLevelsAboveParty);
                for (int i = 0; i < GuardsPerCall; i++)
                {
                    var guard = new EnemyTypes.CastleGuard(guardLevel);
                    called.Add(guard);
                    _guards.Add(guard);
                }
                message = $"{self.Name} calls for her guards!";
                return GameLogic.Combat.CombatAction.Summon(self, called);
            }

            _actedOnThisRound.Add(target);

            // Force a skip - only on someone who hasn't already lost a turn (otherwise attack)
            if (roll > attackWeight && !target.HasEffect<Abilities.SkipTurnEffect>())
            {
                message = $"{self.Name} fixes {target.Name} with a cold stare - {target.Name} can't move!";
                return GameLogic.Combat.CombatAction.Inflict(self, target, new Abilities.SkipTurnEffect());
            }

            message = null;
            return new GameLogic.Combat.CombatAction(ActionType.Attack, self, target);
        }
    }
}
