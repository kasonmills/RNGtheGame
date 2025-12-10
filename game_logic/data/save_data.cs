using System;
using System.Collections.Generic;
using GameLogic.Items;
using GameLogic.Quests;
using GameLogic.Systems;

namespace GameLogic.Data
{
    /// <summary>
    /// Serializable representation of an item for saving
    /// </summary>
    public class SerializedItem
    {
        public string Name { get; set; }
        public int Level { get; set; }
        public int Quantity { get; set; }
        public string ItemType { get; set; } // "Weapon", "Armor", "Consumable", "QuestItem"

        // Weapon-specific
        public int? MinDamage { get; set; }
        public int? MaxDamage { get; set; }
        public int? CritChance { get; set; }
        public int? Accuracy { get; set; }
        public int? Experience { get; set; }
        public int? ExperienceToNextLevel { get; set; }
        public bool? ReadyForUpgrade { get; set; }

        // Armor-specific
        public int? Defense { get; set; }
        public int? ArmorExperience { get; set; }
        public int? ArmorExperienceToNextLevel { get; set; }
        public bool? ArmorReadyForUpgrade { get; set; }

        // Consumable-specific
        public string ConsumableType { get; set; }
        public int? EffectPower { get; set; }
    }

    /// <summary>
    /// Serializable representation of a quest objective
    /// </summary>
    public class SerializedQuestObjective
    {
        public string Description { get; set; }
        public int CurrentProgress { get; set; }
        public int RequiredProgress { get; set; }
    }

    /// <summary>
    /// Serializable representation of a quest for saving
    /// </summary>
    public class SerializedQuest
    {
        public string QuestId { get; set; }
        public string QuestName { get; set; }
        public string Description { get; set; }
        public string QuestState { get; set; }  // Store as string for compatibility
        public List<SerializedQuestObjective> Objectives { get; set; }

        // Rewards
        public int GoldReward { get; set; }
        public int XPReward { get; set; }

        // Additional data for specific quest types
        public int? TotalGoldEarned { get; set; }  // For GoldCollectionQuest
        public string QuestType { get; set; }  // Type of quest for reconstruction

        public SerializedQuest()
        {
            Objectives = new List<SerializedQuestObjective>();
        }
    }

    /// <summary>
    /// Contains all data needed to save/load a game
    /// </summary>
    public class SaveData
    {
        // Player Info
        public string PlayerName { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Gold { get; set; }
        
        // Player Ability (permanent choice)
        public string SelectedAbilityName { get; set; }
        public int SelectedAbilityLevel { get; set; }
        public int SelectedAbilityExperience { get; set; }
        
        // Equipment (store full details to reconstruct later)
        public SerializedItem EquippedWeapon { get; set; }
        public SerializedItem EquippedArmor { get; set; }

        // Inventory (list of serialized items with full properties)
        public List<SerializedItem> InventoryItems { get; set; }

        // Legacy support - keep old weapon/armor names for backward compatibility
        public string EquippedWeaponName { get; set; }
        public string EquippedArmorName { get; set; }
        
        // World State
        public int CurrentMapNodeId { get; set; }
        public int MapSeed { get; set; } // For regenerating the same map
        
        // Timestamps
        public DateTime SaveDate { get; set; }
        public TimeSpan PlayTime { get; set; }

        // Boss Progression
        public List<string> DefeatedBossIds { get; set; }          // IDs of defeated bosses
        public string FinalBossId { get; set; }                    // Selected final boss
        public int BossesDefeated { get; set; }                    // Count of unique bosses defeated
        public bool FinalGateUnlocked { get; set; }                // Is final gate accessible
        public Dictionary<string, int> BossTimesDefeated { get; set; }  // bossId -> defeat count

        // Quest System
        public List<SerializedQuest> Quests { get; set; }          // All quest states and progress
        public string ActiveQuestId { get; set; }                   // Currently focused quest

        // Game Settings
        public DifficultyLevel Difficulty { get; set; }            // Difficulty level (immutable after creation)
        public bool ShowTurnOrderAtStartOfRound { get; set; }
        public bool ShowDetailedCombatLog { get; set; }
        public bool ShowDamageCalculations { get; set; }
        public bool ShowEnemyStatBlock { get; set; }
        public bool AutoSaveEnabled { get; set; }
        public int AutoSaveIntervalMinutes { get; set; }
        public bool ConfirmFleeAction { get; set; }
        public bool ConfirmConsumeItem { get; set; }
        public string RngAlgorithm { get; set; }
        public bool RngStatisticsTracking { get; set; }
        public bool ColoredText { get; set; }
        public bool UseEmojis { get; set; }
        public int TextSpeed { get; set; }
        public bool SoundEffectsEnabled { get; set; }
        public int SoundEffectsVolume { get; set; }
        public bool MusicEnabled { get; set; }
        public int MusicVolume { get; set; }

        // Statistics
        public int TotalBattlesFought { get; set; }
        public int BattlesWon { get; set; }
        public int BattlesLost { get; set; }
        public int BattlesFled { get; set; }
        public long TotalDamageDealt { get; set; }
        public long TotalDamageTaken { get; set; }
        public int HighestDamageDealt { get; set; }
        public int CriticalHitsLanded { get; set; }
        public int TotalPlayerDeaths { get; set; }
        public int EnemiesKilled { get; set; }
        public Dictionary<string, int> KillsByEnemyType { get; set; }
        public int BossesDefeatedCount { get; set; }
        public int BossAttemptsCount { get; set; }
        public Dictionary<string, int> BossDefeatsDict { get; set; }
        public long TotalGoldEarned { get; set; }
        public long TotalGoldSpent { get; set; }
        public int ItemsBought { get; set; }
        public int ItemsSold { get; set; }
        public long TotalValueBought { get; set; }
        public long TotalValueSold { get; set; }
        public int MostExpensivePurchase { get; set; }
        public string MostExpensivePurchaseName { get; set; }
        public int WeaponUpgradesPerformed { get; set; }
        public int ArmorUpgradesPerformed { get; set; }
        public int HighestWeaponLevel { get; set; }
        public int HighestArmorLevel { get; set; }
        public Dictionary<string, int> WeaponsUsedDict { get; set; }
        public int TotalEquipmentChanges { get; set; }
        public int ConsumablesUsed { get; set; }
        public Dictionary<string, int> ConsumablesByTypeDict { get; set; }
        public int HealingPotionsUsed { get; set; }
        public int RevivalPotionsUsed { get; set; }
        public int AbilitiesActivated { get; set; }
        public int ShopsVisited { get; set; }
        public int NPCsInteracted { get; set; }
        public int QuestsCompletedCount { get; set; }
        public int QuestsAcceptedCount { get; set; }
        public int QuestsClaimedCount { get; set; }
        public int HighestLevelReached { get; set; }
        public long TotalExperienceEarned { get; set; }
        public int LevelsGained { get; set; }
        public DateTime GameStartDate { get; set; }
        public int GameSessions { get; set; }
        public int GamesSaved { get; set; }
        public int GamesLoaded { get; set; }
        public string GameVersion { get; set; }
        public int CurrentWinStreak { get; set; }
        public int LongestWinStreak { get; set; }
        public int FlawlessVictories { get; set; }
        public int CloseCallVictories { get; set; }
        public int PerfectCritRuns { get; set; }

        // Constructor
        public SaveData()
        {
            InventoryItems = new List<SerializedItem>();
            SaveDate = DateTime.Now;
            DefeatedBossIds = new List<string>();
            BossTimesDefeated = new Dictionary<string, int>();
            Quests = new List<SerializedQuest>();

            // Initialize statistics dictionaries
            KillsByEnemyType = new Dictionary<string, int>();
            BossDefeatsDict = new Dictionary<string, int>();
            WeaponsUsedDict = new Dictionary<string, int>();
            ConsumablesByTypeDict = new Dictionary<string, int>();
            GameStartDate = DateTime.Now;
            GameVersion = "1.0.0";

            // Default settings (will be overridden by actual settings)
            Difficulty = DifficultyLevel.Normal;
            ShowTurnOrderAtStartOfRound = true;
            ShowDetailedCombatLog = true;
            ShowDamageCalculations = false;
            ShowEnemyStatBlock = true;
            AutoSaveEnabled = true;
            AutoSaveIntervalMinutes = 5;
            ConfirmFleeAction = true;
            ConfirmConsumeItem = false;
            RngAlgorithm = "system";
            RngStatisticsTracking = false;
            ColoredText = true;
            UseEmojis = true;
            TextSpeed = 0;
            SoundEffectsEnabled = true;
            SoundEffectsVolume = 70;
            MusicEnabled = true;
            MusicVolume = 50;
        }
    }
}