using System;
using System.Collections.Generic;
using GameLogic.Combat;
using GameLogic.Entities;
using GameLogic.Entities.NPCs.Companions;

namespace GameLogic.Core
{
    /// <summary>
    /// Owns the opening tutorial's content and its "try every action once" combat gate.
    /// TODO-GODOT: an intro/story scene reads GetIntroStoryText(), then calls
    /// GameManager.RecruitStarterCompanion() and GameManager.BeginEagleBearEncounter().
    /// During that fight, submit actions via TrySubmitAction() instead of calling
    /// CombatManager.SubmitPlayerAction() directly - it's safe to always route through this
    /// even after the gate lifts, since it just forwards once the checklist is complete.
    /// </summary>
    public class TutorialManager
    {
        private static readonly ActionType[] RequiredActionTypes =
        {
            ActionType.Attack,
            ActionType.UseAbility,
            ActionType.UseItem,
            ActionType.Defend
        };

        private readonly HashSet<ActionType> _usedActionTypes = new HashSet<ActionType>();

        /// <summary>
        /// Fired for tutorial-specific narration (rejected repeats, "you're on your own now").
        /// TODO-GODOT: subscribe to this alongside CombatManager.CombatMessage during the tutorial fight.
        /// </summary>
        public event Action<string> TutorialMessage;

        /// <summary>
        /// True until every required action type has been tried at least once this fight.
        /// </summary>
        public bool IsTeachingActions => _usedActionTypes.Count < RequiredActionTypes.Length;

        /// <summary>
        /// The story setup: a farmer/peasant answering the call to war between the two kingdoms.
        /// Draft prose - content, easy to revise later.
        /// </summary>
        public static string GetIntroStoryText()
        {
            return "The war between the two kingdoms has reached even the outlying villages. "
                + "You were a farmer until yesterday - now you carry a weapon instead of a plow, "
                + "answering the call to become a soldier before the fighting reaches your home.\n\n"
                + "On the road to the muster, you hear a scream from the treeline: the princess's "
                + "escort has been ambushed by something huge. There's no one else close enough to help.";
        }

        /// <summary>
        /// Narration once the Eagle Bear fight actually begins.
        /// </summary>
        public static string GetEncounterIntroText()
        {
            return "A monstrous shape bursts from the trees - taloned wings, a crushing bear's bulk. "
                + "The princess is pinned behind it, unarmed. There's no running from this one.";
        }

        /// <summary>
        /// A fresh instance of the companion recruited before the fight.
        /// </summary>
        public CompanionBase GetStarterCompanion()
        {
            return new CompanionWarrior();
        }

        /// <summary>
        /// Skarn, the Eagle Bear - boss #1, the tutorial fight. Returns BossManager's own
        /// registered instance (not a fresh throwaway copy) so scaling/defeat-tracking
        /// stays consistent with the rest of the boss system.
        /// </summary>
        public Entities.Enemies.Bosses.BossEnemy CreateEagleBearEncounter(Entities.Enemies.Bosses.BossManager bossManager)
        {
            _usedActionTypes.Clear();
            return bossManager.GetBoss("skarn_eagle_bear");
        }

        /// <summary>
        /// Submit a player action during the tutorial fight. Rejects repeating an
        /// already-used action type until every required type has been tried once;
        /// otherwise forwards to CombatManager.SubmitPlayerAction(). Once the checklist
        /// is complete, every submission just passes through unblocked.
        /// </summary>
        public bool TrySubmitAction(CombatManager combatManager, CombatAction action)
        {
            if (action.Type == ActionType.Flee)
            {
                TutorialMessage?.Invoke("You can't leave the princess to face this alone!");
                return false;
            }

            if (IsTeachingActions && _usedActionTypes.Contains(action.Type))
            {
                TutorialMessage?.Invoke($"Try a different action first - you haven't tested everything yet!");
                return false;
            }

            bool wasTeaching = IsTeachingActions;
            _usedActionTypes.Add(action.Type);
            combatManager.SubmitPlayerAction(action);

            if (wasTeaching && !IsTeachingActions)
            {
                TutorialMessage?.Invoke("You've learned the basics - now finish off the Eagle Bear!");
            }

            return true;
        }
    }
}
