using GameLogic.Entities;
using GameLogic.Systems;

namespace GameLogic.Abilities
{
    /// <summary>
    /// "Skip your next turn" - a general effect any boss or ability can put on a target (first
    /// user: the Princess's "force a skip", decided 2026-10-14).
    ///
    /// The target loses their next possible turn: this round's turn if it hasn't come yet,
    /// otherwise their turn next round. CombatManager uses the effect up when that turn comes:
    /// - The turn does nothing at all - nothing is done or started on it (not even a Defend).
    /// - A Defend from their previous turn still ends on time.
    /// - Their status effects still tick at the end of the lost turn (my default - not decided),
    ///   so a skip doesn't pause a poison.
    /// - An end-of-round bonus turn (IEndOfRoundTurnMechanic) doesn't use it up (my default).
    ///
    /// It doesn't count down at the end of the round - it lasts until it is used up (or cured:
    /// it is crowd control, so an antidote or Iron Will removes it). Putting it on someone who
    /// already has one doesn't add a second lost turn.
    /// </summary>
    public class SkipTurnEffect : AbilityEffect
    {
        public SkipTurnEffect()
            : base(duration: 1, potency: 0)
        {
            Name = "Turn Lost";
            Description = "Loses their next turn.";
            Type = EffectType.CrowdControl;
            CanStack = false;
        }

        public override void ApplyEffect(Entity target, RNGManager rng)
        {
            // Nothing each turn - CombatManager checks for this effect when the target's turn comes.
        }

        /// <summary>
        /// Doesn't wear off at the end of the round - only used up by the lost turn.
        /// </summary>
        public override void TickDuration()
        {
        }

        public override string GetDisplayInfo()
        {
            return $"{Name} (next turn)";
        }

        public override string GetDetailedInfo()
        {
            return $"{Name}\n{Description}";
        }
    }
}
