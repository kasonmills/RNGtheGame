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
    /// Armor slot - represents a complete armor set
    /// </summary>
    public enum ArmorSlot
    {
        FullSet  // Represents complete armor ensemble (helmet, chestplate, greaves, gauntlets, boots)
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

        // Experience System
        public int Experience { get; set; }
        public int ExperienceToNextLevel { get; private set; }
        public bool ReadyForUpgrade { get; private set; }  // True when XP requirement is met

        // Base stats for level scaling
        private readonly int _baseDefense;

        /// <summary>
        /// Constructor for armor
        /// </summary>
        public Armor(
            string name,
            string description,
            ArmorType type,
            int defense,
            ItemRarity rarity = ItemRarity.Common,
            int level = 1,
            int value = 0
        ) : base(name, description, rarity, ItemCategory.Armor, value)
        {
            Type = type;
            Slot = ArmorSlot.FullSet;  // All armor is now full sets
            Level = Math.Clamp(level, 1, MaxLevel);

            // Store base stats for scaling calculations
            _baseDefense = defense;

            // Apply level scaling
            UpdateStatsForLevel();

            // Initialize experience system
            Experience = 0;
            ExperienceToNextLevel = CalculateExperienceRequired(Level);
            ReadyForUpgrade = false;

            // Armor is stackable if they have the same level
            IsStackable = true;
            MaxStackSize = 99;
        }

        /// <summary>
        /// Calculate experience required for next level
        /// Uses rarity-based exponential scaling: baseXP * (1.035^(Level-1))
        /// Base XP by rarity:
        /// - Common: 100 XP
        /// - Uncommon: 200 XP
        /// - Rare: 300 XP
        /// - Epic: 400 XP
        /// - Legendary: 500 XP
        /// - Mythic: 600 XP
        /// </summary>
        private int CalculateExperienceRequired(int currentLevel)
        {
            // Get base XP requirement based on rarity
            double baseXP = GetBaseXPByRarity();

            // Apply exponential scaling: base * (1.035^(Level-1))
            // This matches the player and ability leveling formula
            for (int i = 1; i < currentLevel; i++)
            {
                baseXP *= 1.035;
            }

            return Convert.ToInt32(baseXP);
        }

        /// <summary>
        /// Get base XP requirement based on armor rarity
        /// </summary>
        private int GetBaseXPByRarity()
        {
            return Rarity switch
            {
                ItemRarity.Common => 100,
                ItemRarity.Uncommon => 200,
                ItemRarity.Rare => 300,
                ItemRarity.Epic => 400,
                ItemRarity.Legendary => 500,
                ItemRarity.Mythic => 600,
                _ => 100
            };
        }

        /// <summary>
        /// Gain experience towards next level
        /// </summary>
        public void GainExperience(int amount)
        {
            if (Level >= MaxLevel || ReadyForUpgrade)
            {
                return; // Already at max level or already ready for upgrade
            }

            Experience += amount;
            Console.WriteLine($"{GetDisplayName()} gained {amount} XP! ({Experience}/{ExperienceToNextLevel})");

            // Check if ready for upgrade
            if (Experience >= ExperienceToNextLevel)
            {
                ReadyForUpgrade = true;
                Console.WriteLine($"⚒️  {GetDisplayName()} has enough experience! Take it to a blacksmith to upgrade (Cost: {GetUpgradeCost()} gold)");
            }
        }

        /// <summary>
        /// Restore the ReadyForUpgrade state (used by save system)
        /// </summary>
        internal void RestoreUpgradeReadyState()
        {
            if (Level < MaxLevel && Experience >= ExperienceToNextLevel)
            {
                ReadyForUpgrade = true;
            }
        }

        /// <summary>
        /// Calculate the gold cost to upgrade at blacksmith
        /// Cost scales with level and rarity
        /// </summary>
        public int GetUpgradeCost()
        {
            if (!ReadyForUpgrade || Level >= MaxLevel)
            {
                return 0;
            }

            // Base cost: 30 gold per level
            int baseCost = 30 * (Level + 1);

            // Rarity multiplier
            double rarityMultiplier = Rarity switch
            {
                ItemRarity.Common => 1.0,
                ItemRarity.Uncommon => 1.5,
                ItemRarity.Rare => 2.0,
                ItemRarity.Epic => 3.0,
                ItemRarity.Legendary => 5.0,
                ItemRarity.Mythic => 10.0,
                _ => 1.0
            };

            return (int)(baseCost * rarityMultiplier);
        }

        /// <summary>
        /// Complete the upgrade at the blacksmith (requires gold payment)
        /// This should be called by the blacksmith NPC/shop after taking payment
        /// </summary>
        public bool CompleteUpgrade()
        {
            if (!ReadyForUpgrade || Level >= MaxLevel)
            {
                return false;
            }

            Level++;
            Experience -= ExperienceToNextLevel;
            ExperienceToNextLevel = CalculateExperienceRequired(Level);
            ReadyForUpgrade = false;

            UpdateStatsForLevel();
            Console.WriteLine($"⚒️  {GetDisplayName()} has been upgraded! Now at +{Level}!");
            return true;
        }

        /// <summary>
        /// Legacy method - kept for backwards compatibility
        /// Use CompleteUpgrade() instead for the new system
        /// </summary>
        [Obsolete("Use GainExperience() and CompleteUpgrade() instead")]
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

            // Show experience progress
            if (Level < MaxLevel)
            {
                if (ReadyForUpgrade)
                {
                    info += $"⚒️  READY FOR UPGRADE! (Cost: {GetUpgradeCost()} gold at blacksmith)\n";
                }
                else
                {
                    info += $"Experience: {Experience}/{ExperienceToNextLevel}\n";
                }
            }

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