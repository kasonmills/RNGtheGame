using System.Collections.Generic;
using GameLogic.Abilities;
using GameLogic.Entities;
using GameLogic.Items;
using Enemy = GameLogic.Entities.Enemies.EnemyBase;

namespace GameLogic.Combat
{
    /// <summary>
    /// What kind of thing happened in a fight - one event per thing, so a combat screen can
    /// show each at the right moment (a damage number, "Miss", a health bar moving).
    /// </summary>
    public enum CombatEventType
    {
        CombatStarted,
        RoundStarted,       // Round = the new round number; read CombatManager.TurnOrder for the 8 slots
        TurnStarted,        // Actor = whose turn it is
        TurnSkipped,        // Actor's turn came up but was skipped: a party member who is down, a boss's
                            // extra turn called off, or a lost turn (SkipTurnEffect - Message = "Turn lost")
        EndOfRoundTurnAdded,// Actor (a boss) gets a bonus turn at the end of this round - TurnOrder has a new last slot
        WaitingForInput,    // Actor = the party member the player is choosing an action for
        ActionRejected,     // Message = why the submitted action was refused (nothing happened)

        AttackLanded,       // Actor hit Target; IsCritical; Amount = damage after defense
        AttackMissed,       // Actor missed Target
        AttackEvaded,       // Target dodged (Evasion ability)
        AttackBlocked,      // Attack landed but did no damage (Risk's armor); status effects still apply
        ShieldBlocked,      // Target's shield (ShieldEffect) took Amount of Actor's hit - read the effect for
                            // what is left; a broken shield is EffectRemoved "Shield"
        AttackJoined,       // Actor joins another enemy's attack on Target (the Cyclops Brothers) - the
                            // joined attack's own AttackLanded/AttackMissed follows
        AbilityUsed,        // Actor used Ability on Target
        ItemUsed,           // Actor used Item on Target

        Damaged,            // Target lost Amount health (from any source - attack, ability, poison)
        Healed,             // Target gained Amount health
        Defeated,           // Target went down
        Revived,            // Target came back up with Amount health
        EnemyJoined,        // Target (a new enemy) joined the fight, called by Actor (null if no one) - acts from next round

        DefendStarted,      // Actor is defending until their next turn
        DefendEnded,        // Actor's defend ran out (their next turn came)
        EffectApplied,      // Target gained the effect named EffectName
        EffectRemoved,      // Target lost the effect named EffectName

        FleeSucceeded,      // Actor got away (an enemy, or the whole party)
        FleeFailed,

        CombatEnded         // Read the CombatResult from CombatManager.CombatFinished / LastResult
    }

    /// <summary>
    /// One thing that happened in a fight. Which fields are filled depends on Type.
    /// </summary>
    public class CombatEvent
    {
        public CombatEventType Type { get; set; }
        public int Round { get; set; }
        public Entity Actor { get; set; }
        public Entity Target { get; set; }
        public int Amount { get; set; }
        public bool IsCritical { get; set; }
        public Ability Ability { get; set; }
        public Item Item { get; set; }
        public string EffectName { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// How a fight ended. There is no draw in the MVP (decided 2026-10-14).
    /// </summary>
    public enum CombatOutcome
    {
        Won,            // Every enemy is down or fled, and at least one was defeated
        Lost,           // The whole party is down - Game Over
        PartyFled,      // The party escaped - no rewards
        OpponentFled    // Every enemy fled, none were defeated - 50% rewards
    }

    /// <summary>
    /// The end-of-fight summary for the results screen.
    /// </summary>
    public class CombatResult
    {
        public CombatOutcome Outcome { get; set; }
        public int RoundsFought { get; set; }
        public int ExperienceGained { get; set; }
        public int GoldGained { get; set; }
        public List<Item> ItemsGained { get; } = new List<Item>();
        public List<Enemy> DefeatedEnemies { get; } = new List<Enemy>();
        public List<Enemy> FledEnemies { get; } = new List<Enemy>();
        public int PlayerLevelsGained { get; set; }
        public Dictionary<Entity, int> CompanionLevelsGained { get; } = new Dictionary<Entity, int>();

        /// <summary>
        /// True when the game carries on in the overworld (anything but a loss).
        /// </summary>
        public bool PartySurvived => Outcome != CombatOutcome.Lost;
    }

    /// <summary>
    /// One of the 8 turn slots in a round - 4 for the party, 4 for the opponents. A party slot
    /// holds one party member; an opponent slot can hold more than one enemy when there are
    /// more than 4 (they act one after another in the same slot). An empty slot is skipped.
    ///
    /// A boss with more than one turn per round (IMultiTurnMechanic, e.g. the Princess) has
    /// one slot per turn, holding only that boss. TurnNumber says which of its turns the slot
    /// is (1 = its normal turn, 2 = its second...). Every other slot has TurnNumber 1.
    ///
    /// A boss's end-of-round bonus turn (IEndOfRoundTurnMechanic, e.g. the Cyclops revive) is
    /// added when the round ends, after every other filled slot, with IsEndOfRoundTurn true and
    /// Speed 0. It only exists in a round where the boss takes one.
    /// </summary>
    public class TurnSlot
    {
        public bool IsPartySlot { get; }
        public IReadOnlyList<Entity> Members { get; }
        public int Speed { get; }
        public int TurnNumber { get; }
        public bool IsEndOfRoundTurn { get; }
        public bool IsEmpty => Members.Count == 0;

        public TurnSlot(bool isPartySlot, List<Entity> members, int speed, int turnNumber = 1, bool isEndOfRoundTurn = false)
        {
            IsPartySlot = isPartySlot;
            Members = members;
            Speed = speed;
            TurnNumber = turnNumber;
            IsEndOfRoundTurn = isEndOfRoundTurn;
        }
    }
}
