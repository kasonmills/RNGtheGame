using System;
using System.Collections.Generic;
using System.Linq;
using GameLogic.Items;

namespace GameLogic.Progression
{
    /// <summary>
    /// Types of loot sources
    /// </summary>
    public enum LootSourceType
    {
        Enemy,          // Dropped by enemies
        Chest,          // Found in chests
        Boss,           // Boss-specific loot
        Quest,          // Quest rewards
        Shop,           // Merchant inventory
        Crafting        // Crafting results
    }

    /// <summary>
    /// Represents a single loot entry with weight and conditions
    /// </summary>
    public class LootEntry
    {
        public ItemCategory Category { get; set; }
        public ItemRarity MinRarity { get; set; }
        public ItemRarity MaxRarity { get; set; }
        public int Weight { get; set; }              // Higher weight = more common
        public int MinLevel { get; set; }            // Minimum item level
        public int MaxLevel { get; set; }            // Maximum item level
        public int MinQuantity { get; set; }         // For stackable items
        public int MaxQuantity { get; set; }
        public string SpecificItem { get; set; }     // For guaranteed specific items

        public LootEntry(
            ItemCategory category,
            int weight,
            ItemRarity minRarity = ItemRarity.Common,
            ItemRarity maxRarity = ItemRarity.Mythic,
            int minLevel = 1,
            int maxLevel = 100,
            int minQuantity = 1,
            int maxQuantity = 1,
            string specificItem = null)
        {
            Category = category;
            Weight = weight;
            MinRarity = minRarity;
            MaxRarity = maxRarity;
            MinLevel = minLevel;
            MaxLevel = maxLevel;
            MinQuantity = minQuantity;
            MaxQuantity = maxQuantity;
            SpecificItem = specificItem;
        }
    }

    /// <summary>
    /// Loot table configuration for a specific source
    /// </summary>
    public class LootTable
    {
        public string Name { get; set; }
        public LootSourceType SourceType { get; set; }
        public List<LootEntry> Entries { get; set; }

        // Gold drop range
        public int MinGold { get; set; }
        public int MaxGold { get; set; }

        // Rarity probability modifiers (0.0 - 1.0)
        public float CommonChance { get; set; } = 0.60f;    // 60%
        public float UncommonChance { get; set; } = 0.25f;  // 25%
        public float RareChance { get; set; } = 0.10f;      // 10%
        public float EpicChance { get; set; } = 0.04f;      // 4%
        public float LegendaryChance { get; set; } = 0.01f; // 1%
        public float MythicChance { get; set; } = 0.001f;   // 0.1%

        // Drop settings
        public int MinDrops { get; set; } = 1;
        public int MaxDrops { get; set; } = 1;
        public float NothingDropChance { get; set; } = 0.0f; // Chance to drop nothing

        public LootTable(string name, LootSourceType sourceType)
        {
            Name = name;
            SourceType = sourceType;
            Entries = new List<LootEntry>();
        }

        public void AddEntry(LootEntry entry)
        {
            Entries.Add(entry);
        }
    }

    /// <summary>
    /// Result of a loot roll
    /// </summary>
    public class LootResult
    {
        public List<Item> Items { get; set; }
        public int Gold { get; set; }

        public LootResult()
        {
            Items = new List<Item>();
            Gold = 0;
        }

        public void AddItem(Item item)
        {
            Items.Add(item);
        }
    }

    /// <summary>
    /// Loot generation system
    /// Generates loot based on context (player level, area, difficulty)
    /// </summary>
    public class LootGenerator
    {
        private Random _rng;

        public LootGenerator(Random rng = null)
        {
            _rng = rng ?? new Random();
        }

        /// <summary>
        /// Generate loot from a loot table
        /// </summary>
        public LootResult GenerateLoot(
            LootTable table,
            int playerLevel,
            float luckModifier = 1.0f,
            int areaLevel = 0)
        {
            var result = new LootResult();

            // Check if nothing drops
            if (_rng.NextDouble() < table.NothingDropChance)
            {
                return result; // Empty result
            }

            // Generate gold
            if (table.MaxGold > 0)
            {
                // Scale gold to player/area level
                int levelMultiplier = Math.Max(playerLevel, areaLevel);
                int scaledMinGold = table.MinGold + (levelMultiplier / 2);
                int scaledMaxGold = table.MaxGold + levelMultiplier;

                result.Gold = _rng.Next(scaledMinGold, scaledMaxGold + 1);
            }

            // Determine number of item drops
            int numDrops = _rng.Next(table.MinDrops, table.MaxDrops + 1);

            // Generate each item drop
            for (int i = 0; i < numDrops; i++)
            {
                var item = GenerateSingleItem(table, playerLevel, luckModifier, areaLevel);
                if (item != null)
                {
                    result.AddItem(item);
                }
            }

            return result;
        }

        /// <summary>
        /// Generate a single item from the loot table
        /// </summary>
        private Item GenerateSingleItem(
            LootTable table,
            int playerLevel,
            float luckModifier,
            int areaLevel)
        {
            if (table.Entries.Count == 0)
                return null;

            // Select a loot entry based on weights
            var selectedEntry = SelectWeightedEntry(table.Entries);
            if (selectedEntry == null)
                return null;

            // Determine item level (based on player level and area level)
            int itemLevel = CalculateItemLevel(
                playerLevel,
                areaLevel,
                selectedEntry.MinLevel,
                selectedEntry.MaxLevel);

            // Determine item rarity (affected by luck)
            ItemRarity rarity = DetermineRarity(
                table,
                luckModifier,
                selectedEntry.MinRarity,
                selectedEntry.MaxRarity);

            // Generate the specific item
            Item item = null;

            try
            {
                // Check for specific item override
                if (!string.IsNullOrEmpty(selectedEntry.SpecificItem))
                {
                    item = ItemDatabase.GetItemByName(selectedEntry.SpecificItem, itemLevel);
                    item.Rarity = rarity; // Override rarity
                }
                else
                {
                    // Generate random item of the selected category
                    item = ItemDatabase.GetRandomItem(selectedEntry.Category, _rng, itemLevel);
                    item.Rarity = rarity; // Override rarity
                }

                // Set quantity for stackable items
                if (item.IsStackable)
                {
                    int quantity = _rng.Next(selectedEntry.MinQuantity, selectedEntry.MaxQuantity + 1);
                    item.Quantity = quantity;
                }
            }
            catch (Exception)
            {
                // If item generation fails, return null
                return null;
            }

            return item;
        }

        /// <summary>
        /// Select a weighted entry from the list
        /// </summary>
        private LootEntry SelectWeightedEntry(List<LootEntry> entries)
        {
            int totalWeight = entries.Sum(e => e.Weight);
            if (totalWeight <= 0)
                return null;

            int roll = _rng.Next(0, totalWeight);
            int currentWeight = 0;

            foreach (var entry in entries)
            {
                currentWeight += entry.Weight;
                if (roll < currentWeight)
                {
                    return entry;
                }
            }

            return entries.Last(); // Fallback
        }

        /// <summary>
        /// Calculate item level based on player/area level
        /// </summary>
        private int CalculateItemLevel(int playerLevel, int areaLevel, int minLevel, int maxLevel)
        {
            // Use the higher of player level or area level
            int baseLevel = Math.Max(playerLevel, areaLevel);

            // Add some variance (±3 levels)
            int variance = _rng.Next(-3, 4);
            int itemLevel = baseLevel + variance;

            // Clamp to min/max bounds
            itemLevel = Math.Clamp(itemLevel, minLevel, maxLevel);

            return itemLevel;
        }

        /// <summary>
        /// Determine item rarity based on loot table probabilities and luck
        /// </summary>
        private ItemRarity DetermineRarity(
            LootTable table,
            float luckModifier,
            ItemRarity minRarity,
            ItemRarity maxRarity)
        {
            // Apply luck modifier to rarity chances (higher luck = better odds)
            float mythicChance = table.MythicChance * luckModifier;
            float legendaryChance = table.LegendaryChance * luckModifier;
            float epicChance = table.EpicChance * luckModifier;
            float rareChance = table.RareChance * luckModifier;
            float uncommonChance = table.UncommonChance * luckModifier;
            float commonChance = table.CommonChance;

            // Normalize if luck pushed total over 1.0
            float total = mythicChance + legendaryChance + epicChance + rareChance + uncommonChance + commonChance;
            if (total > 1.0f)
            {
                mythicChance /= total;
                legendaryChance /= total;
                epicChance /= total;
                rareChance /= total;
                uncommonChance /= total;
                commonChance /= total;
            }

            // Roll for rarity
            double roll = _rng.NextDouble();
            ItemRarity rarity;

            if (roll < mythicChance)
                rarity = ItemRarity.Mythic;
            else if (roll < mythicChance + legendaryChance)
                rarity = ItemRarity.Legendary;
            else if (roll < mythicChance + legendaryChance + epicChance)
                rarity = ItemRarity.Epic;
            else if (roll < mythicChance + legendaryChance + epicChance + rareChance)
                rarity = ItemRarity.Rare;
            else if (roll < mythicChance + legendaryChance + epicChance + rareChance + uncommonChance)
                rarity = ItemRarity.Uncommon;
            else
                rarity = ItemRarity.Common;

            // Clamp to min/max bounds
            if (rarity < minRarity)
                rarity = minRarity;
            if (rarity > maxRarity)
                rarity = maxRarity;

            return rarity;
        }
    }

    /// <summary>
    /// Predefined loot table templates
    /// </summary>
    public static class LootTableTemplates
    {
        /// <summary>
        /// Basic enemy loot table (low-level mobs)
        /// </summary>
        public static LootTable CreateBasicEnemyLoot()
        {
            var table = new LootTable("Basic Enemy", LootSourceType.Enemy)
            {
                MinGold = 5,
                MaxGold = 15,
                MinDrops = 0,
                MaxDrops = 1,
                NothingDropChance = 0.3f, // 30% chance to drop nothing
                CommonChance = 0.70f,
                UncommonChance = 0.25f,
                RareChance = 0.05f,
                EpicChance = 0.0f,
                LegendaryChance = 0.0f,
                MythicChance = 0.0f
            };

            table.AddEntry(new LootEntry(ItemCategory.Consumable, weight: 50, maxLevel: 10));
            table.AddEntry(new LootEntry(ItemCategory.Weapon, weight: 20, maxRarity: ItemRarity.Uncommon));
            table.AddEntry(new LootEntry(ItemCategory.Armor, weight: 20, maxRarity: ItemRarity.Uncommon));

            return table;
        }

        /// <summary>
        /// Elite enemy loot table (stronger enemies)
        /// </summary>
        public static LootTable CreateEliteEnemyLoot()
        {
            var table = new LootTable("Elite Enemy", LootSourceType.Enemy)
            {
                MinGold = 20,
                MaxGold = 50,
                MinDrops = 1,
                MaxDrops = 2,
                NothingDropChance = 0.0f,
                CommonChance = 0.40f,
                UncommonChance = 0.35f,
                RareChance = 0.20f,
                EpicChance = 0.05f,
                LegendaryChance = 0.0f,
                MythicChance = 0.0f
            };

            table.AddEntry(new LootEntry(ItemCategory.Weapon, weight: 35, maxRarity: ItemRarity.Rare));
            table.AddEntry(new LootEntry(ItemCategory.Armor, weight: 35, maxRarity: ItemRarity.Rare));
            table.AddEntry(new LootEntry(ItemCategory.Consumable, weight: 30, maxLevel: 10, minQuantity: 1, maxQuantity: 3));

            return table;
        }

        /// <summary>
        /// Boss loot table (guaranteed good loot)
        /// </summary>
        public static LootTable CreateBossLoot()
        {
            var table = new LootTable("Boss", LootSourceType.Boss)
            {
                MinGold = 100,
                MaxGold = 300,
                MinDrops = 2,
                MaxDrops = 4,
                NothingDropChance = 0.0f,
                CommonChance = 0.10f,
                UncommonChance = 0.30f,
                RareChance = 0.35f,
                EpicChance = 0.20f,
                LegendaryChance = 0.05f,
                MythicChance = 0.001f
            };

            table.AddEntry(new LootEntry(ItemCategory.Weapon, weight: 40, minRarity: ItemRarity.Uncommon));
            table.AddEntry(new LootEntry(ItemCategory.Armor, weight: 40, minRarity: ItemRarity.Uncommon));
            table.AddEntry(new LootEntry(ItemCategory.Consumable, weight: 20, minLevel: 5, maxLevel: 10, minQuantity: 2, maxQuantity: 5));

            return table;
        }

        /// <summary>
        /// Chest loot table
        /// </summary>
        public static LootTable CreateChestLoot()
        {
            var table = new LootTable("Chest", LootSourceType.Chest)
            {
                MinGold = 30,
                MaxGold = 100,
                MinDrops = 1,
                MaxDrops = 3,
                NothingDropChance = 0.05f,
                CommonChance = 0.50f,
                UncommonChance = 0.30f,
                RareChance = 0.15f,
                EpicChance = 0.04f,
                LegendaryChance = 0.01f,
                MythicChance = 0.0f
            };

            table.AddEntry(new LootEntry(ItemCategory.Weapon, weight: 30));
            table.AddEntry(new LootEntry(ItemCategory.Armor, weight: 30));
            table.AddEntry(new LootEntry(ItemCategory.Consumable, weight: 40, minQuantity: 1, maxQuantity: 5));

            return table;
        }
    }
}