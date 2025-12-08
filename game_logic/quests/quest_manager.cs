using System;
using System.Collections.Generic;
using System.Linq;

namespace GameLogic.Quests
{
    /// <summary>
    /// Manages all quests in the game
    /// Tracks quest states, active quest, and handles quest progression
    /// </summary>
    public class QuestManager
    {
        // Quest Storage
        private Dictionary<string, Quest> _allQuests;
        private string _activeQuestId;  // Currently focused quest

        // Public Getters
        public Dictionary<string, Quest> AllQuests => _allQuests;
        public string ActiveQuestId => _activeQuestId;

        public QuestManager()
        {
            _allQuests = new Dictionary<string, Quest>();
            _activeQuestId = null;
        }

        /// <summary>
        /// Register a quest in the system
        /// </summary>
        public void RegisterQuest(Quest quest)
        {
            if (!_allQuests.ContainsKey(quest.QuestId))
            {
                _allQuests[quest.QuestId] = quest;
            }
        }

        /// <summary>
        /// Register multiple quests at once
        /// </summary>
        public void RegisterQuests(params Quest[] quests)
        {
            foreach (var quest in quests)
            {
                RegisterQuest(quest);
            }
        }

        /// <summary>
        /// Get a quest by ID
        /// </summary>
        public Quest GetQuest(string questId)
        {
            _allQuests.TryGetValue(questId, out Quest quest);
            return quest;
        }

        /// <summary>
        /// Discover a quest (make it available to player)
        /// </summary>
        public void DiscoverQuest(string questId)
        {
            var quest = GetQuest(questId);
            if (quest != null)
            {
                quest.Discover();
            }
        }

        /// <summary>
        /// Accept a quest
        /// </summary>
        public bool AcceptQuest(string questId)
        {
            var quest = GetQuest(questId);
            if (quest != null && quest.State == QuestState.Available)
            {
                quest.Accept();

                // Set as active quest if no active quest
                if (string.IsNullOrEmpty(_activeQuestId))
                {
                    SetActiveQuest(questId);
                }

                return true;
            }
            return false;
        }

        /// <summary>
        /// Complete a quest (if objectives are met)
        /// </summary>
        public bool CompleteQuest(string questId)
        {
            var quest = GetQuest(questId);
            if (quest != null && quest.CanComplete())
            {
                quest.Complete();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Claim quest rewards
        /// </summary>
        public QuestReward ClaimQuestRewards(string questId)
        {
            var quest = GetQuest(questId);
            if (quest != null && quest.State == QuestState.Completed)
            {
                quest.ClaimRewards();

                // Clear active quest if this was it
                if (_activeQuestId == questId)
                {
                    _activeQuestId = null;
                }

                return quest.Reward;
            }
            return null;
        }

        /// <summary>
        /// Set the active/focused quest
        /// </summary>
        public void SetActiveQuest(string questId)
        {
            var quest = GetQuest(questId);
            if (quest != null && (quest.State == QuestState.Accepted || quest.State == QuestState.Available))
            {
                _activeQuestId = questId;
            }
        }

        /// <summary>
        /// Clear active quest
        /// </summary>
        public void ClearActiveQuest()
        {
            _activeQuestId = null;
        }

        /// <summary>
        /// Get the currently active quest
        /// </summary>
        public Quest GetActiveQuest()
        {
            if (string.IsNullOrEmpty(_activeQuestId))
                return null;

            return GetQuest(_activeQuestId);
        }

        /// <summary>
        /// Get all quests with a specific state
        /// </summary>
        public List<Quest> GetQuestsByState(QuestState state)
        {
            return _allQuests.Values.Where(q => q.State == state).ToList();
        }

        /// <summary>
        /// Get all available quests
        /// </summary>
        public List<Quest> GetAvailableQuests()
        {
            return GetQuestsByState(QuestState.Available);
        }

        /// <summary>
        /// Get all accepted quests
        /// </summary>
        public List<Quest> GetAcceptedQuests()
        {
            return GetQuestsByState(QuestState.Accepted);
        }

        /// <summary>
        /// Get all completed quests (not yet claimed)
        /// </summary>
        public List<Quest> GetCompletedQuests()
        {
            return GetQuestsByState(QuestState.Completed);
        }

        /// <summary>
        /// Get all claimed quests
        /// </summary>
        public List<Quest> GetClaimedQuests()
        {
            return GetQuestsByState(QuestState.Claimed);
        }

        /// <summary>
        /// Check all quests to see if any can auto-complete
        /// (for retroactive completion feature)
        /// </summary>
        public void CheckQuestCompletion()
        {
            foreach (var quest in _allQuests.Values)
            {
                if (quest.CanComplete() && quest.State != QuestState.Completed && quest.State != QuestState.Claimed)
                {
                    quest.Complete();
                }
            }
        }

        /// <summary>
        /// Display quest log
        /// </summary>
        public void DisplayQuestLog()
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("            QUEST LOG");
            Console.WriteLine("═══════════════════════════════════════\n");

            var activeQuests = GetAcceptedQuests();
            var completedQuests = GetCompletedQuests();
            var claimedQuests = GetClaimedQuests();

            // Active Quests
            if (activeQuests.Count > 0)
            {
                Console.WriteLine("═══ ACTIVE QUESTS ═══");
                foreach (var quest in activeQuests)
                {
                    string activeMarker = quest.QuestId == _activeQuestId ? "★ " : "  ";
                    Console.WriteLine($"{activeMarker}{quest.GetQuestSummary()}");
                }
                Console.WriteLine();
            }

            // Completed Quests (awaiting claim)
            if (completedQuests.Count > 0)
            {
                Console.WriteLine("═══ COMPLETED QUESTS (Claim Rewards) ═══");
                foreach (var quest in completedQuests)
                {
                    Console.WriteLine($"  {quest.GetQuestSummary()}");
                }
                Console.WriteLine();
            }

            // Claimed Quests
            if (claimedQuests.Count > 0)
            {
                Console.WriteLine("═══ CLAIMED QUESTS ═══");
                foreach (var quest in claimedQuests)
                {
                    Console.WriteLine($"  {quest.QuestName}");
                }
                Console.WriteLine();
            }

            if (activeQuests.Count == 0 && completedQuests.Count == 0 && claimedQuests.Count == 0)
            {
                Console.WriteLine("No quests in progress.");
                Console.WriteLine("Visit the Job Board or talk to NPCs to find quests!\n");
            }

            Console.WriteLine("═══════════════════════════════════════");
        }

        /// <summary>
        /// Display active quest tracker (for HUD/status display)
        /// </summary>
        public void DisplayActiveQuestTracker()
        {
            var activeQuest = GetActiveQuest();
            if (activeQuest == null)
            {
                Console.WriteLine("No active quest tracked.");
                return;
            }

            Console.WriteLine($"═══ ACTIVE QUEST: {activeQuest.QuestName} ═══");
            foreach (var objective in activeQuest.Objectives)
            {
                string checkmark = objective.IsCompleted ? "✓" : " ";
                Console.WriteLine($"  [{checkmark}] {objective.Description} ({objective.GetProgressString()})");
            }
        }

        /// <summary>
        /// Get quest progress summary
        /// </summary>
        public string GetQuestProgressSummary()
        {
            int totalQuests = _allQuests.Count;
            int completedCount = GetCompletedQuests().Count + GetClaimedQuests().Count;
            int activeCount = GetAcceptedQuests().Count;

            return $"Quests: {completedCount}/{totalQuests} completed | {activeCount} active";
        }

        /// <summary>
        /// Reset all quests (for new game)
        /// </summary>
        public void Reset()
        {
            _allQuests.Clear();
            _activeQuestId = null;
        }

        // === Setter Methods for Save System ===

        /// <summary>
        /// Set quest state (used when loading from save)
        /// </summary>
        public void SetQuestState(string questId, QuestState state)
        {
            var quest = GetQuest(questId);
            if (quest != null)
            {
                // Use reflection or make State setter public in Quest class
                // For now, we'll need to add a public setter or a method in Quest class
                typeof(Quest).GetProperty("State").SetValue(quest, state);
            }
        }

        /// <summary>
        /// Set objective progress (used when loading from save)
        /// </summary>
        public void SetObjectiveProgress(string questId, int objectiveIndex, int progress)
        {
            var quest = GetQuest(questId);
            if (quest != null && objectiveIndex < quest.Objectives.Count)
            {
                quest.Objectives[objectiveIndex].SetProgress(progress);
            }
        }

        /// <summary>
        /// Set active quest ID (used when loading from save)
        /// </summary>
        public void SetActiveQuestId(string questId)
        {
            _activeQuestId = questId;
        }
    }
}