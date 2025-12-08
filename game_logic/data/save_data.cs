using System;
using System.Collections.Generic;
using GameLogic.Items;
using GameLogic.Quests;

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

        // Constructor
        public SaveData()
        {
            InventoryItems = new List<SerializedItem>();
            SaveDate = DateTime.Now;
            DefeatedBossIds = new List<string>();
            BossTimesDefeated = new Dictionary<string, int>();
            Quests = new List<SerializedQuest>();
        }
    }
}