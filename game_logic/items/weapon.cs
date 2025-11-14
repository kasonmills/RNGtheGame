using System;

namespace GameLogic.Items
{
    /// <summary>
    /// Weapon type for different weapon categories
    /// </summary>
    public enum WeaponType
    {
        Sword,
        Axe,
        Mace,
        Dagger,
        Spear,
        Staff,
        Bow,
        Crossbow,
        Wand,
        Fist  // Unarmed/fist weapons
    }

    /// <summary>
    /// Weapon class with level system (1-100)
    /// </summary>
    public class Weapon : Item
    {
        // Weapon Stats
        public int MinDamage { get; set; }
        public int MaxDamage { get; set; }
        public int CritChance { get; set; }
        public int Accuracy { get; set; }
        public WeaponType Type { get; set; }

        // Level Properties (Weapons can level 1-100)
        public int Level { get; set; }
        public const int MaxLevel = 100;

        // Base stats for level scaling
        private int _baseMinDamage;
        private int _baseMaxDamage;
        private int _baseCritChance;
        private int _baseAccuracy;

        /// <summary>
        /// Constructor for weapons
        /// </summary>
        public Weapon(
            string name,
            string description,
            WeaponType type,
            int minDamage,
            int maxDamage,
            int critChance,
            int accuracy,
            ItemRarity rarity = ItemRarity.Common,
            int level = 1,
            int value = 0
        ) : base(name, description, rarity, ItemCategory.Weapon, value)
        {
            Type = type;
            Level = Math.Clamp(level, 1, MaxLevel);

            // Store base stats for scaling calculations
            _baseMinDamage = minDamage;
            _baseMaxDamage = maxDamage;
            _baseCritChance = critChance;
            _baseAccuracy = accuracy;

            // Apply level scaling
            UpdateStatsForLevel();

            // Weapons are not stackable
            IsStackable = false;
            MaxStackSize = 1;
        }

        /// <summary>
        /// Level up the weapon (up to level 100)
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
        /// Update weapon stats based on current level
        /// Scales damage, crit, and accuracy with level
        /// </summary>
        private void UpdateStatsForLevel()
        {
            // Scale stats with level
            // Every 10 levels adds roughly 10% to base stats
            float levelScaling = 1.0f + ((Level - 1) * 0.01f);

            MinDamage = (int)(_baseMinDamage * levelScaling);
            MaxDamage = (int)(_baseMaxDamage * levelScaling);
            CritChance = (int)(_baseCritChance * levelScaling);
            Accuracy = (int)(_baseAccuracy * levelScaling);

            // Cap accuracy at 95%
            if (Accuracy > 95)
            {
                Accuracy = 95;
            }

            // Cap crit chance at 50%
            if (CritChance > 50)
            {
                CritChance = 50;
            }
        }

        /// <summary>
        /// Check if weapon can level up
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
        /// Override GetInfo to include weapon-specific stats
        /// </summary>
        public override string GetInfo()
        {
            string info = $"{GetDisplayName()}\n";
            info += $"{Description}\n";
            info += $"Type: {Type}\n";
            info += $"Rarity: {Rarity}\n";
            info += $"Level: {Level}/{MaxLevel}\n";
            info += $"Damage: {MinDamage}-{MaxDamage}\n";
            info += $"Crit Chance: {CritChance}%\n";
            info += $"Accuracy: {Accuracy}%\n";
            info += $"Value: {Value} gold\n";

            return info;
        }

        /// <summary>
        /// Weapons modify player's attack stats when equipped
        /// </summary>
        public override void ModifyStat(Entities.Player.Player player)
        {
            // This is called when weapon is equipped
            // The actual stat modification should be handled by the player's equipment system
            // This method exists to satisfy the abstract requirement from Item base class
        }
    }
}