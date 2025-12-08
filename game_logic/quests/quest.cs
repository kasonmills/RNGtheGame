using System;
using System.Collections.Generic;
using System.Linq;

namespace GameLogic.Quests
{
    /// <summary>
    /// Quest state progression
    /// </summary>
    public enum QuestState
    {
        NotDiscovered,  // Quest exists but player hasn't found it yet
        Available,      // Quest discovered but not accepted
        Accepted,       // Player has accepted the quest
        Completed,      // All objectives met
        Claimed         // Rewards have been claimed
    }

    /// <summary>
    /// Base class for all quests in the game
    /// Tracks objectives, progress, and rewards
    /// </summary>
    public abstract class Quest
    {
        // Quest Identity
        public string QuestId { get; protected set; }
        public string QuestName { get; protected set; }
        public string Description { get; protected set; }

        // Quest State
        public QuestState State { get; set; }  // Public setter for save system

        // Quest Content
        public List<QuestObjective> Objectives { get; protected set; }
        public QuestReward Reward { get; protected set; }

        // Quest Configuration
        public bool RequiresAcceptanceToComplete { get; protected set; }  // True for final boss quest only

        protected Quest(string questId, string questName, string description, QuestReward reward, bool requiresAcceptance = false)
        {
            QuestId = questId;
            QuestName = questName;
            Description = description;
            Reward = reward;
            State = QuestState.NotDiscovered;
            Objectives = new List<QuestObjective>();
            RequiresAcceptanceToComplete = requiresAcceptance;
        }

        /// <summary>
        /// Check if all objectives are completed
        /// </summary>
        public virtual bool AreObjectivesComplete()
        {
            return Objectives.All(obj => obj.IsCompleted);
        }

        /// <summary>
        /// Check if quest can be completed (considers acceptance requirement)
        /// </summary>
        public virtual bool CanComplete()
        {
            if (!AreObjectivesComplete())
                return false;

            // If quest requires acceptance, must be in Accepted state
            if (RequiresAcceptanceToComplete)
                return State == QuestState.Accepted;

            // Otherwise, can complete from any state except NotDiscovered
            return State != QuestState.NotDiscovered;
        }

        /// <summary>
        /// Discover the quest (make it available to player)
        /// </summary>
        public virtual void Discover()
        {
            if (State == QuestState.NotDiscovered)
            {
                State = QuestState.Available;
            }
        }

        /// <summary>
        /// Accept the quest
        /// </summary>
        public virtual void Accept()
        {
            if (State == QuestState.Available)
            {
                State = QuestState.Accepted;
                OnAccepted();
            }
        }

        /// <summary>
        /// Complete the quest
        /// </summary>
        public virtual void Complete()
        {
            if (CanComplete() && State != QuestState.Completed && State != QuestState.Claimed)
            {
                State = QuestState.Completed;
                OnCompleted();
            }
        }

        /// <summary>
        /// Claim quest rewards
        /// </summary>
        public virtual void ClaimRewards()
        {
            if (State == QuestState.Completed)
            {
                State = QuestState.Claimed;
                OnRewardsClaimed();
            }
        }

        /// <summary>
        /// Get overall quest progress percentage
        /// </summary>
        public int GetProgressPercentage()
        {
            if (Objectives.Count == 0) return 0;

            int totalProgress = Objectives.Sum(obj => obj.GetProgressPercentage());
            return totalProgress / Objectives.Count;
        }

        /// <summary>
        /// Display quest info to console
        /// </summary>
        public virtual void DisplayQuestInfo()
        {
            Console.WriteLine($"═══ {QuestName} ═══");
            Console.WriteLine($"Status: {GetStateString()}");
            Console.WriteLine($"\n{Description}");

            Console.WriteLine("\nObjectives:");
            foreach (var objective in Objectives)
            {
                string checkmark = objective.IsCompleted ? "✓" : " ";
                Console.WriteLine($"  [{checkmark}] {objective.Description} ({objective.GetProgressString()})");
            }

            if (Reward.HasRewards())
            {
                Console.WriteLine($"\nRewards: {Reward.GetRewardSummary()}");
            }
        }

        /// <summary>
        /// Get quest state as a readable string
        /// </summary>
        public string GetStateString()
        {
            return State switch
            {
                QuestState.NotDiscovered => "Not Discovered",
                QuestState.Available => "Available",
                QuestState.Accepted => "In Progress",
                QuestState.Completed => "Completed",
                QuestState.Claimed => "Claimed",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Get a short summary of the quest for lists
        /// </summary>
        public virtual string GetQuestSummary()
        {
            string stateIcon = State switch
            {
                QuestState.Available => "❗",
                QuestState.Accepted => "📝",
                QuestState.Completed => "✓",
                QuestState.Claimed => "✓",
                _ => " "
            };

            int progress = GetProgressPercentage();
            return $"{stateIcon} {QuestName} ({progress}%)";
        }

        // === Virtual methods for subclass customization ===

        /// <summary>
        /// Called when quest is accepted
        /// </summary>
        protected virtual void OnAccepted()
        {
            // Override in subclasses if needed
        }

        /// <summary>
        /// Called when quest is completed
        /// </summary>
        protected virtual void OnCompleted()
        {
            // Override in subclasses if needed
        }

        /// <summary>
        /// Called when rewards are claimed
        /// </summary>
        protected virtual void OnRewardsClaimed()
        {
            // Override in subclasses if needed
        }

        /// <summary>
        /// Update quest progress (called by subclasses)
        /// </summary>
        protected void CheckCompletion()
        {
            if (CanComplete() && State != QuestState.Completed && State != QuestState.Claimed)
            {
                Complete();
            }
        }
    }
}