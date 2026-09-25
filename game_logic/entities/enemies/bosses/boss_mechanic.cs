using GameLogic.Combat;
using GameLogic.Entities;
using GameLogic.Systems;

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
        CombatAction DecideAction(BossEnemy self, Entity target, RNGManager rng, out string message);
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

        public CombatAction DecideAction(BossEnemy self, Entity target, RNGManager rng, out string message)
        {
            // Turn 2 of a charge: unleash the stoop attack.
            if (_isCharging)
            {
                _isCharging = false;
                var chargeTarget = _chargeTarget;
                _chargeTarget = null;
                message = $"{self.Name} slams down in a devastating stoop attack!";
                return new CombatAction(ActionType.Attack, self, chargeTarget) { DamageMultiplier = 2.5 };
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
                return CombatAction.None(self);
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
                return CombatAction.None(self);
            }

            // Remaining chance: fall back to a normal attack via the generic AI.
            message = null;
            return null;
        }

        private CombatAction StartGrapple(BossEnemy self, Entity target, RNGManager rng, out string message)
        {
            int duration = rng.Roll(1, 3);
            _grappleTurnsRemaining = duration - 1; // this turn is the hit itself
            _grappledTarget = target;
            message = $"{self.Name} seizes {target.Name} in a crushing grapple!";

            var slow = new Abilities.EnemyAbilities.GrappledEffect(duration, speedReduction: 6);
            return new CombatAction(ActionType.Attack, self, target) { AppliesEffect = slow };
        }
    }
}
