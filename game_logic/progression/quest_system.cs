using System;
using System.Collections.Generic;
using System.Linq;
using GameLogic.Items;

namespace GameLogic.Progression
{
    /// <summary>
    /// Types of quests available
    /// </summary>
    public enum QuestType
    {
        Main,           // Main storyline quests
        Side,           // Optional side quests
        Bounty,         // Kill X enemies
        Collection,     // Collect X items
        Delivery,       // Deliver item to NPC
        Exploration,    // Visit specific locations
        Boss,           // Defeat a boss enemy
        Repeat,          // repeatable quests
        Achievement     // Long-term achievement quests
    }

    /// <summary>
    /// Quest status
    /// </summary>
    public enum QuestStatus
    {
        NotStarted,
        Active,
        Completed,
        Failed,
        TurnedIn        // Completed and reward claimed
    }

    /// <summary>
    /// Types of quest objectives
    /// </summary>
    public enum ObjectiveType
    {
        KillEnemies,        // Kill X of enemy type
        CollectItems,       // Collect X items
        TalkToNPC,          // Talk to specific NPC
        ReachLocation,      // Reach a location
        DefeatBoss,         // Defeat a specific boss
        EscortNPC,          // Protect NPC
        UseItem,            // Use specific item
        EquipItem,          // Equip specific item type
        ReachLevel,         // Reach character level
        GainGold            // Accumulate X gold
    }

    /// <summary>
    /// A single objective within a quest
    /// </summary>
    public class QuestObjective
    {
        public string Description { get; set; }
        public ObjectiveType Type { get; set; }
        public string TargetId { get; set; }       // Enemy name, item name, NPC name, etc.
        public int RequiredAmount { get; set; }
        public int CurrentAmount { get; set; }
        public bool IsCompleted => CurrentAmount >= RequiredAmount;
        public bool IsOptional { get; set; }       // Optional objectives

        public QuestObjective(
            string description,
            ObjectiveType type,
            string targetId,
            int requiredAmount,
            bool isOptional = false)
        {
            Description = description;
            Type = type;
            TargetId = targetId;
            RequiredAmount = requiredAmount;
            CurrentAmount = 0;
            IsOptional = isOptional;
        }

        /// <summary>
        /// Update objective progress
        /// </summary>
        public bool UpdateProgress(int amount = 1)
        {
            if (IsCompleted) return false;

            CurrentAmount += amount;
            if (CurrentAmount > RequiredAmount)
            {
                CurrentAmount = RequiredAmount;
            }

            return IsCompleted;
        }

        /// <summary>
        /// Get progress display string
        /// </summary>
        public string GetProgressDisplay()
        {
            string optional = IsOptional ? " (Optional)" : "";
            string status = IsCompleted ? "[✓]" : "[ ]";
            return $"{status} {Description} ({CurrentAmount}/{RequiredAmount}){optional}";
        }
    }

    /// <summary>
    /// Quest rewards
    /// </summary>
    public class QuestReward
    {
        public int ExperiencePoints { get; set; }
        public int Gold { get; set; }
        public List<Item> Items { get; set; }
        public string UnlockedArea { get; set; }       // Unlock new area
        public string UnlockedCompanion { get; set; }  // Unlock companion
        public string UnlockedAbility { get; set; }    // Unlock ability

        public QuestReward()
        {
            Items = new List<Item>();
        }

        public void AddItem(Item item)
        {
            Items.Add(item);
        }
    }

    /// <summary>
    /// A single quest
    /// </summary>
    public class Quest
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public QuestType Type { get; set; }
        public QuestStatus Status { get; set; }

        public int RequiredLevel { get; set; }         // Minimum level to accept
        public List<string> Prerequisites { get; set; } // Required completed quests
        public List<QuestObjective> Objectives { get; set; }
        public QuestReward Reward { get; set; }

        public string QuestGiverNPC { get; set; }      // NPC who gives quest
        public string QuestTurnInNPC { get; set; }     // NPC to turn in (can be same)
        public bool IsRepeatable { get; set; }         // Can be repeated

        public Quest(string id, string name, string description, QuestType type)
        {
            Id = id;
            Name = name;
            Description = description;
            Type = type;
            Status = QuestStatus.NotStarted;
            Prerequisites = new List<string>();
            Objectives = new List<QuestObjective>();
            Reward = new QuestReward();
            RequiredLevel = 1;
            IsRepeatable = false;
        }

        /// <summary>
        /// Check if all required objectives are complete
        /// </summary>
        public bool AreRequiredObjectivesComplete()
        {
            return Objectives
                .Where(obj => !obj.IsOptional)
                .All(obj => obj.IsCompleted);
        }

        /// <summary>
        /// Check if all objectives (including optional) are complete
        /// </summary>
        public bool AreAllObjectivesComplete()
        {
            return Objectives.All(obj => obj.IsCompleted);
        }

        /// <summary>
        /// Get overall completion percentage
        /// </summary>
        public float GetCompletionPercentage()
        {
            if (Objectives.Count == 0) return 0f;

            int totalProgress = Objectives.Sum(obj => obj.CurrentAmount);
            int totalRequired = Objectives.Sum(obj => obj.RequiredAmount);

            return totalRequired > 0 ? (float)totalProgress / totalRequired : 0f;
        }

        /// <summary>
        /// Add an objective to the quest
        /// </summary>
        public void AddObjective(QuestObjective objective)
        {
            Objectives.Add(objective);
        }

        /// <summary>
        /// Get detailed quest info
        /// </summary>
        public string GetQuestInfo()
        {
            string info = $"╔══════════════════════════════════════╗\n";
            info += $"  {Name}\n";
            info += $"╠══════════════════════════════════════╣\n";
            info += $"  Type: {Type}\n";
            info += $"  Status: {Status}\n";
            info += $"  Level Requirement: {RequiredLevel}\n";
            info += $"╠══════════════════════════════════════╣\n";
            info += $"  {Description}\n";
            info += $"╠══════════════════════════════════════╣\n";
            info += $"  Objectives:\n";

            foreach (var obj in Objectives)
            {
                info += $"  {obj.GetProgressDisplay()}\n";
            }

            info += $"╠══════════════════════════════════════╣\n";
            info += $"  Rewards:\n";
            if (Reward.ExperiencePoints > 0)
                info += $"  • {Reward.ExperiencePoints} XP\n";
            if (Reward.Gold > 0)
                info += $"  • {Reward.Gold} Gold\n";
            if (Reward.Items.Count > 0)
                info += $"  • {Reward.Items.Count} Item(s)\n";
            info += $"╚══════════════════════════════════════╝\n";

            return info;
        }
    }

    /// <summary>
    /// Quest manager that tracks all quests
    /// </summary>
    public class QuestSystem
    {
        private Dictionary<string, Quest> _allQuests;
        private List<Quest> _activeQuests;
        private List<Quest> _completedQuests;
        private int _maxActiveQuests;

        public IReadOnlyList<Quest> ActiveQuests => _activeQuests.AsReadOnly();
        public IReadOnlyList<Quest> CompletedQuests => _completedQuests.AsReadOnly();

        public QuestSystem(int maxActiveQuests = 10)
        {
            _allQuests = new Dictionary<string, Quest>();
            _activeQuests = new List<Quest>();
            _completedQuests = new List<Quest>();
            _maxActiveQuests = maxActiveQuests;
        }

        /// <summary>
        /// Register a quest in the system
        /// </summary>
        public void RegisterQuest(Quest quest)
        {
            if (!_allQuests.ContainsKey(quest.Id))
            {
                _allQuests[quest.Id] = quest;
            }
        }

        /// <summary>
        /// Check if player can accept a quest
        /// </summary>
        public bool CanAcceptQuest(Quest quest, int playerLevel)
        {
            // Check if already active
            if (_activeQuests.Any(q => q.Id == quest.Id))
            {
                return false;
            }

            // Check if already completed (and not repeatable)
            if (!quest.IsRepeatable && _completedQuests.Any(q => q.Id == quest.Id))
            {
                return false;
            }

            // Check level requirement
            if (playerLevel < quest.RequiredLevel)
            {
                return false;
            }

            // Check prerequisites
            foreach (var prereqId in quest.Prerequisites)
            {
                if (!_completedQuests.Any(q => q.Id == prereqId))
                {
                    return false;
                }
            }

            // Check active quest limit
            if (_activeQuests.Count >= _maxActiveQuests)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Start/accept a quest
        /// </summary>
        public bool StartQuest(string questId, int playerLevel)
        {
            if (!_allQuests.TryGetValue(questId, out Quest quest))
            {
                Console.WriteLine($"Quest '{questId}' not found.");
                return false;
            }

            if (!CanAcceptQuest(quest, playerLevel))
            {
                Console.WriteLine($"Cannot accept quest '{quest.Name}'.");
                return false;
            }

            quest.Status = QuestStatus.Active;
            _activeQuests.Add(quest);
            Console.WriteLine($"Quest started: {quest.Name}");
            return true;
        }

        /// <summary>
        /// Abandon a quest
        /// </summary>
        public bool AbandonQuest(string questId)
        {
            var quest = _activeQuests.FirstOrDefault(q => q.Id == questId);
            if (quest == null)
            {
                Console.WriteLine("Quest not found in active quests.");
                return false;
            }

            _activeQuests.Remove(quest);
            quest.Status = QuestStatus.NotStarted;

            // Reset objectives
            foreach (var obj in quest.Objectives)
            {
                obj.CurrentAmount = 0;
            }

            Console.WriteLine($"Quest abandoned: {quest.Name}");
            return true;
        }

        /// <summary>
        /// Update quest progress for a specific objective type
        /// </summary>
        public void UpdateQuestProgress(ObjectiveType type, string targetId, int amount = 1)
        {
            foreach (var quest in _activeQuests)
            {
                foreach (var objective in quest.Objectives)
                {
                    if (objective.Type == type &&
                        (string.IsNullOrEmpty(targetId) || objective.TargetId.Equals(targetId, StringComparison.OrdinalIgnoreCase)))
                    {
                        if (objective.UpdateProgress(amount))
                        {
                            Console.WriteLine($"Quest objective completed: {objective.Description}");

                            // Check if quest is complete
                            if (quest.AreRequiredObjectivesComplete())
                            {
                                quest.Status = QuestStatus.Completed;
                                Console.WriteLine($"Quest ready to turn in: {quest.Name}");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Turn in a completed quest and claim rewards
        /// </summary>
        public QuestReward TurnInQuest(string questId, Entities.Player.Player player)
        {
            var quest = _activeQuests.FirstOrDefault(q => q.Id == questId);
            if (quest == null)
            {
                Console.WriteLine("Quest not found in active quests.");
                return null;
            }

            if (!quest.AreRequiredObjectivesComplete())
            {
                Console.WriteLine("Quest objectives not completed yet.");
                return null;
            }

            // Remove from active quests
            _activeQuests.Remove(quest);

            // Add to completed quests
            quest.Status = QuestStatus.TurnedIn;
            if (!quest.IsRepeatable)
            {
                _completedQuests.Add(quest);
            }
            else
            {
                // Reset repeatable quest
                quest.Status = QuestStatus.NotStarted;
                foreach (var obj in quest.Objectives)
                {
                    obj.CurrentAmount = 0;
                }
            }

            // Grant rewards
            if (quest.Reward.ExperiencePoints > 0)
            {
                player.AddExperience(quest.Reward.ExperiencePoints);
            }

            if (quest.Reward.Gold > 0)
            {
                player.Gold += quest.Reward.Gold;
                Console.WriteLine($"Received {quest.Reward.Gold} gold!");
            }

            foreach (var item in quest.Reward.Items)
            {
                player.Inventory.Add(item);
                Console.WriteLine($"Received: {item.GetDisplayName()}");
            }

            Console.WriteLine($"Quest completed: {quest.Name}");

            return quest.Reward;
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
        /// Get all available quests for a player
        /// </summary>
        public List<Quest> GetAvailableQuests(int playerLevel)
        {
            return _allQuests.Values
                .Where(q => q.Status == QuestStatus.NotStarted && CanAcceptQuest(q, playerLevel))
                .ToList();
        }

        /// <summary>
        /// Get summary of all active quests
        /// </summary>
        public string GetActiveQuestsSummary()
        {
            if (_activeQuests.Count == 0)
            {
                return "No active quests.";
            }

            string summary = "═══ Active Quests ═══\n";
            foreach (var quest in _activeQuests)
            {
                float completion = quest.GetCompletionPercentage() * 100;
                summary += $"• {quest.Name} ({completion:F0}%)\n";
            }
            return summary;
        }
    }

    /// <summary>
    /// Predefined quest templates for easy quest creation
    /// </summary>
    public static class QuestTemplates
    {
        /// <summary>
        /// Create a simple kill quest
        /// </summary>
        public static Quest CreateKillQuest(
            string id,
            string name,
            string enemyType,
            int killCount,
            int xpReward,
            int goldReward)
        {
            var quest = new Quest(id, name, $"Defeat {killCount} {enemyType}.", QuestType.Bounty);

            quest.AddObjective(new QuestObjective(
                $"Defeat {killCount} {enemyType}",
                ObjectiveType.KillEnemies,
                enemyType,
                killCount
            ));

            quest.Reward.ExperiencePoints = xpReward;
            quest.Reward.Gold = goldReward;

            return quest;
        }

        /// <summary>
        /// Create a collection quest
        /// </summary>
        public static Quest CreateCollectionQuest(
            string id,
            string name,
            string itemName,
            int itemCount,
            int xpReward,
            Item rewardItem)
        {
            var quest = new Quest(id, name, $"Collect {itemCount} {itemName}.", QuestType.Collection);

            quest.AddObjective(new QuestObjective(
                $"Collect {itemCount} {itemName}",
                ObjectiveType.CollectItems,
                itemName,
                itemCount
            ));

            quest.Reward.ExperiencePoints = xpReward;
            quest.Reward.AddItem(rewardItem);

            return quest;
        }

        /// <summary>
        /// Create a boss quest
        /// </summary>
        public static Quest CreateBossQuest(
            string id,
            string name,
            string description,
            string bossName,
            int requiredLevel,
            int xpReward,
            int goldReward,
            List<Item> rewardItems)
        {
            var quest = new Quest(id, name, description, QuestType.Boss);
            quest.RequiredLevel = requiredLevel;

            quest.AddObjective(new QuestObjective(
                $"Defeat {bossName}",
                ObjectiveType.DefeatBoss,
                bossName,
                1
            ));

            quest.Reward.ExperiencePoints = xpReward;
            quest.Reward.Gold = goldReward;
            foreach (var item in rewardItems)
            {
                quest.Reward.AddItem(item);
            }

            return quest;
        }
    }
}