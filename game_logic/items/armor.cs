using System;

namespace GameLogic.Items
{
    /// <summary>
    /// Armor type for different armor categories
    /// </summary>
    public enum ArmorType
    {
        Cloth,      // Light armor, low defense
        Leather,    // Medium armor
        Chainmail,  // Heavy armor
        Plate,      // Very heavy armor
        Robe,       // Mage armor
        Shield      // Shields
    }

    /// <summary>
    /// Armor slot for different body parts
    /// </summary>
    public enum ArmorSlot
    {
        Head,
        Chest,
        Legs,
        Feet,
        Hands,
        Shield
    }

    /// <summary>
    /// Armor class with level system (1-100)
    /// </summary>
    public class Armor : Item
    {
        // Armor Stats
        public int Defense { get; set; }
        public ArmorType Type { get; set; }
        public ArmorSlot Slot { get; set; }

        // Level Properties (Armor can level 1-100)
        public int Level { get; set; }
        public const int MaxLevel = 100;

        // Base stats for level scaling
        private readonly int _baseDefense;

        /// <summary>
        /// Constructor for armor
        /// </summary>
        public Armor(
            string name,
            string description,
            ArmorType type,
            ArmorSlot slot,
            int defense,
            ItemRarity rarity = ItemRarity.Common,
            int level = 1,
            int value = 0
        ) : base(name, description, rarity, ItemCategory.Armor, value)
        {
            Type = type;
            Slot = slot;
            Level = Math.Clamp(level, 1, MaxLevel);

            // Store base stats for scaling calculations
            _baseDefense = defense;

            // Apply level scaling
            UpdateStatsForLevel();

            // Armor is not stackable
            IsStackable = false;
            MaxStackSize = 1;
        }

        /// <summary>
        /// Level up the armor (up to level 100)
        /// Returns true if level up succeeded
        /// </summary>
        public bool LevelUp()
        {
            if (Level >= MaxLevel)
            {
                return false;
            }

            Level++;
            UpdateStatsForLevel();
            Console.WriteLine($"{Name} leveled up to +{Level}!");
            return true;
        }

        /// <summary>
        /// Update armor stats based on current level
        /// Scales defense with level
        /// </summary>
        private void UpdateStatsForLevel()
        {
            // Scale defense with level
            // Every 10 levels adds roughly 10% to base defense
            float levelScaling = 1.0f + ((Level - 1) * 0.01f);

            Defense = (int)(_baseDefense * levelScaling);
        }

        /// <summary>
        /// Check if armor can level up
        /// </summary>
        public bool CanLevelUp()
        {
            return Level < MaxLevel;
        }

        /// <summary>
        /// Get level progress percentage (for UI)
        /// </summary>
        public float GetLevelProgress()
        {
            return (float)Level / MaxLevel;
        }

        /// <summary>
        /// Override display name to include level
        /// </summary>
        public override string GetDisplayName()
        {
            string rarityPrefix = Rarity switch
            {
                ItemRarity.Common => "",
                ItemRarity.Uncommon => "[U] ",
                ItemRarity.Rare => "[R] ",
                ItemRarity.Epic => "[E] ",
                ItemRarity.Legendary => "[L] ",
                ItemRarity.Mythic => "[M] ",
                _ => ""
            };

            return $"{rarityPrefix}{Name} +{Level}";
        }

        /// <summary>
        /// Override GetInfo to include armor-specific stats
        /// </summary>
        public override string GetInfo()
        {
            string info = $"{GetDisplayName()}\n";
            info += $"{Description}\n";
            info += $"Type: {Type}\n";
            info += $"Slot: {Slot}\n";
            info += $"Rarity: {Rarity}\n";
            info += $"Level: {Level}/{MaxLevel}\n";
            info += $"Defense: {Defense}\n";
            info += $"Value: {Value} gold\n";

            return info;
        }

        /// <summary>
        /// Armor modifies player's defense stats when equipped
        /// </summary>
        public override void ModifyStat(Entities.Player.Player player)
        {
            // This is called when armor is equipped
            // The actual stat modification should be handled by the player's equipment system
            // This method exists to satisfy the abstract requirement from Item base class
        }
    }
}