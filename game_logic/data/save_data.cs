using System;
using System.Collections.Generic;
using GameLogic.Items;

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

        // Armor-specific
        public int? Defense { get; set; }

        // Consumable-specific
        public string ConsumableType { get; set; }
        public int? EffectPower { get; set; }
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
        
        // Constructor
        public SaveData()
        {
            InventoryItems = new List<SerializedItem>();
            SaveDate = DateTime.Now;
        }
    }
}