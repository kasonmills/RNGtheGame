using System;
using System.Collections.Generic;
using GameLogic.Items;

namespace GameLogic.Data
{
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
        
        // Equipment (store names/IDs to reconstruct later)
        public string EquippedWeaponName { get; set; }
        public string EquippedArmorName { get; set; }
        
        // Inventory (list of item names/IDs)
        public List<string> InventoryItems { get; set; }
        
        // World State
        public int CurrentMapNodeId { get; set; }
        public int MapSeed { get; set; } // For regenerating the same map
        
        // Timestamps
        public DateTime SaveDate { get; set; }
        public TimeSpan PlayTime { get; set; }
        
        // Constructor
        public SaveData()
        {
            InventoryItems = new List<string>();
            SaveDate = DateTime.Now;
        }
    }
}