using System;
using System.Collections.Generic;
using System.Linq;

namespace GameLogic.Items
{
    /// <summary>
    /// Central database for all items in the game
    /// Provides factory methods for creating weapons, armor, consumables, and quest items
    /// </summary>
    public static class ItemDatabase
    {
        #region Weapon Factory Methods

        /// <summary>
        /// Get a weapon by name
        /// </summary>
        public static Weapon GetWeapon(string weaponName, int level = 1)
        {
            return weaponName.ToLower() switch
            {
                // Swords
                "rusty sword" => new Weapon(
                    "Rusty Sword",
                    "A worn blade that has seen better days.",
                    WeaponType.Sword,
                    minDamage: 3,
                    maxDamage: 7,
                    critChance: 5,
                    accuracy: 70,
                    ItemRarity.Common,
                    level,
                    value: 10
                ),

                "iron sword" => new Weapon(
                    "Iron Sword",
                    "A standard iron blade.",
                    WeaponType.Sword,
                    minDamage: 5,
                    maxDamage: 12,
                    critChance: 8,
                    accuracy: 75,
                    ItemRarity.Common,
                    level,
                    value: 50
                ),

                "steel sword" => new Weapon(
                    "Steel Sword",
                    "A well-crafted steel blade.",
                    WeaponType.Sword,
                    minDamage: 10,
                    maxDamage: 20,
                    critChance: 10,
                    accuracy: 80,
                    ItemRarity.Uncommon,
                    level,
                    value: 150
                ),

                // Axes
                "rusty axe" => new Weapon(
                    "Rusty Axe",
                    "A crude axe with a chipped blade.",
                    WeaponType.Axe,
                    minDamage: 4,
                    maxDamage: 10,
                    critChance: 12,
                    accuracy: 65,
                    ItemRarity.Common,
                    level,
                    value: 15
                ),

                "battle axe" => new Weapon(
                    "Battle Axe",
                    "A heavy axe designed for cleaving armor.",
                    WeaponType.Axe,
                    minDamage: 12,
                    maxDamage: 25,
                    critChance: 15,
                    accuracy: 70,
                    ItemRarity.Uncommon,
                    level,
                    value: 200
                ),

                // Staves
                "wooden staff" => new Weapon(
                    "Wooden Staff",
                    "A simple staff for channeling magic.",
                    WeaponType.Staff,
                    minDamage: 2,
                    maxDamage: 6,
                    critChance: 3,
                    accuracy: 85,
                    ItemRarity.Common,
                    level,
                    value: 20
                ),

                "arcane staff" => new Weapon(
                    "Arcane Staff",
                    "A staff imbued with magical energy.",
                    WeaponType.Staff,
                    minDamage: 8,
                    maxDamage: 18,
                    critChance: 5,
                    accuracy: 90,
                    ItemRarity.Rare,
                    level,
                    value: 300
                ),

                // Bows
                "hunting bow" => new Weapon(
                    "Hunting Bow",
                    "A simple bow for hunting game.",
                    WeaponType.Bow,
                    minDamage: 4,
                    maxDamage: 9,
                    critChance: 10,
                    accuracy: 75,
                    ItemRarity.Common,
                    level,
                    value: 40
                ),

                "longbow" => new Weapon(
                    "Longbow",
                    "A powerful longbow with extended range.",
                    WeaponType.Bow,
                    minDamage: 9,
                    maxDamage: 17,
                    critChance: 12,
                    accuracy: 80,
                    ItemRarity.Uncommon,
                    level,
                    value: 180
                ),

                // Daggers
                "rusty dagger" => new Weapon(
                    "Rusty Dagger",
                    "A small, corroded blade.",
                    WeaponType.Dagger,
                    minDamage: 2,
                    maxDamage: 5,
                    critChance: 15,
                    accuracy: 80,
                    ItemRarity.Common,
                    level,
                    value: 8
                ),

                "assassin's blade" => new Weapon(
                    "Assassin's Blade",
                    "A deadly dagger favored by rogues.",
                    WeaponType.Dagger,
                    minDamage: 7,
                    maxDamage: 14,
                    critChance: 25,
                    accuracy: 85,
                    ItemRarity.Rare,
                    level,
                    value: 250
                ),

                _ => throw new ArgumentException($"Weapon '{weaponName}' not found in database.")
            };
        }

        /// <summary>
        /// Get all available weapons
        /// </summary>
        public static List<(string Name, WeaponType Type, ItemRarity Rarity)> GetAllWeapons()
        {
            return new List<(string, WeaponType, ItemRarity)>
            {
                ("Rusty Sword", WeaponType.Sword, ItemRarity.Common),
                ("Iron Sword", WeaponType.Sword, ItemRarity.Common),
                ("Steel Sword", WeaponType.Sword, ItemRarity.Uncommon),
                ("Rusty Axe", WeaponType.Axe, ItemRarity.Common),
                ("Battle Axe", WeaponType.Axe, ItemRarity.Uncommon),
                ("Wooden Staff", WeaponType.Staff, ItemRarity.Common),
                ("Arcane Staff", WeaponType.Staff, ItemRarity.Rare),
                ("Hunting Bow", WeaponType.Bow, ItemRarity.Common),
                ("Longbow", WeaponType.Bow, ItemRarity.Uncommon),
                ("Rusty Dagger", WeaponType.Dagger, ItemRarity.Common),
                ("Assassin's Blade", WeaponType.Dagger, ItemRarity.Rare)
            };
        }

        #endregion

        #region Armor Factory Methods

        /// <summary>
        /// Get armor by name
        /// </summary>
        public static Armor GetArmor(string armorName, int level = 1)
        {
            return armorName.ToLower() switch
            {
                // Chest Armor
                "rusty chainmail" => new Armor(
                    "Rusty Chainmail",
                    "Old chainmail armor that barely holds together.",
                    ArmorType.Chainmail,
                    ArmorSlot.Chest,
                    defense: 5,
                    ItemRarity.Common,
                    level,
                    value: 20
                ),

                "iron chestplate" => new Armor(
                    "Iron Chestplate",
                    "Solid iron protection for the torso.",
                    ArmorType.Plate,
                    ArmorSlot.Chest,
                    defense: 12,
                    ItemRarity.Common,
                    level,
                    value: 80
                ),

                "steel chestplate" => new Armor(
                    "Steel Chestplate",
                    "Heavy steel armor providing excellent protection.",
                    ArmorType.Plate,
                    ArmorSlot.Chest,
                    defense: 20,
                    ItemRarity.Uncommon,
                    level,
                    value: 200
                ),

                // Helmets
                "leather cap" => new Armor(
                    "Leather Cap",
                    "A simple leather head covering.",
                    ArmorType.Leather,
                    ArmorSlot.Head,
                    defense: 2,
                    ItemRarity.Common,
                    level,
                    value: 10
                ),

                "iron helmet" => new Armor(
                    "Iron Helmet",
                    "A sturdy iron helmet.",
                    ArmorType.Plate,
                    ArmorSlot.Head,
                    defense: 6,
                    ItemRarity.Common,
                    level,
                    value: 40
                ),

                // Legs
                "cloth pants" => new Armor(
                    "Cloth Pants",
                    "Basic cloth leg protection.",
                    ArmorType.Cloth,
                    ArmorSlot.Legs,
                    defense: 1,
                    ItemRarity.Common,
                    level,
                    value: 5
                ),

                "leather leggings" => new Armor(
                    "Leather Leggings",
                    "Flexible leather leg armor.",
                    ArmorType.Leather,
                    ArmorSlot.Legs,
                    defense: 4,
                    ItemRarity.Common,
                    level,
                    value: 25
                ),

                // Robes (Mage armor)
                "apprentice robe" => new Armor(
                    "Apprentice Robe",
                    "A simple robe for novice mages.",
                    ArmorType.Robe,
                    ArmorSlot.Chest,
                    defense: 3,
                    ItemRarity.Common,
                    level,
                    value: 30
                ),

                "arcane robe" => new Armor(
                    "Arcane Robe",
                    "A robe woven with protective enchantments.",
                    ArmorType.Robe,
                    ArmorSlot.Chest,
                    defense: 8,
                    ItemRarity.Rare,
                    level,
                    value: 150
                ),

                _ => throw new ArgumentException($"Armor '{armorName}' not found in database.")
            };
        }

        /// <summary>
        /// Get all available armor
        /// </summary>
        public static List<(string Name, ArmorType Type, ArmorSlot Slot, ItemRarity Rarity)> GetAllArmor()
        {
            return new List<(string, ArmorType, ArmorSlot, ItemRarity)>
            {
                ("Rusty Chainmail", ArmorType.Chainmail, ArmorSlot.Chest, ItemRarity.Common),
                ("Iron Chestplate", ArmorType.Plate, ArmorSlot.Chest, ItemRarity.Common),
                ("Steel Chestplate", ArmorType.Plate, ArmorSlot.Chest, ItemRarity.Uncommon),
                ("Leather Cap", ArmorType.Leather, ArmorSlot.Head, ItemRarity.Common),
                ("Iron Helmet", ArmorType.Plate, ArmorSlot.Head, ItemRarity.Common),
                ("Cloth Pants", ArmorType.Cloth, ArmorSlot.Legs, ItemRarity.Common),
                ("Leather Leggings", ArmorType.Leather, ArmorSlot.Legs, ItemRarity.Common),
                ("Apprentice Robe", ArmorType.Robe, ArmorSlot.Chest, ItemRarity.Common),
                ("Arcane Robe", ArmorType.Robe, ArmorSlot.Chest, ItemRarity.Rare)
            };
        }

        #endregion

        #region Consumable Factory Methods

        /// <summary>
        /// Get a consumable by name
        /// </summary>
        public static Consumable GetConsumable(string consumableName, int level = 1, int quantity = 1)
        {
            var consumable = consumableName.ToLower() switch
            {
                // Health Potions
                "minor health potion" => new Consumable(
                    "Minor Health Potion",
                    "Restores a small amount of health.",
                    ConsumableType.HealthPotion,
                    effectPower: 25,
                    ItemRarity.Common,
                    level,
                    value: 10
                ),

                "health potion" => new Consumable(
                    "Health Potion",
                    "Restores a moderate amount of health.",
                    ConsumableType.HealthPotion,
                    effectPower: 50,
                    ItemRarity.Common,
                    level,
                    value: 25
                ),

                "greater health potion" => new Consumable(
                    "Greater Health Potion",
                    "Restores a large amount of health.",
                    ConsumableType.HealthPotion,
                    effectPower: 100,
                    ItemRarity.Uncommon,
                    level,
                    value: 60
                ),

                // Food
                "bread" => new Consumable(
                    "Bread",
                    "Simple bread that restores a bit of health.",
                    ConsumableType.Food,
                    effectPower: 20,
                    ItemRarity.Common,
                    level,
                    value: 3
                ),

                "cooked meat" => new Consumable(
                    "Cooked Meat",
                    "Hearty meat that restores health.",
                    ConsumableType.Food,
                    effectPower: 40,
                    ItemRarity.Common,
                    level,
                    value: 8
                ),

                // Elixirs (Rare and expensive multi-effect consumables)
                "elixir of vitality" => new Consumable(
                    "Elixir of Vitality",
                    "A powerful elixir that greatly restores health.",
                    ConsumableType.Elixir,
                    effectPower: 150,
                    ItemRarity.Rare,
                    level,
                    value: 100
                ),

                "combat elixir" => new Consumable(
                    "Combat Elixir",
                    "A rare elixir that boosts both attack and defense simultaneously for 1 turn.",
                    ConsumableType.Elixir,
                    effectPower: 30, // Not directly used, effects calculated by level in Use()
                    ItemRarity.Epic,
                    level,
                    value: 200
                ),

                // Bombs
                "fire bomb" => new Consumable(
                    "Fire Bomb",
                    "An explosive device that deals fire damage.",
                    ConsumableType.Bomb,
                    effectPower: 40,
                    ItemRarity.Uncommon,
                    level,
                    value: 30
                ),

                // Antidotes
                "antidote" => new Consumable(
                    "Antidote",
                    "Cures poison and other ailments.",
                    ConsumableType.Antidote,
                    effectPower: 0,
                    ItemRarity.Common,
                    level,
                    value: 15
                ),

                _ => throw new ArgumentException($"Consumable '{consumableName}' not found in database.")
            };

            consumable.Quantity = quantity;
            return consumable;
        }

        /// <summary>
        /// Get all available consumables
        /// </summary>
        public static List<(string Name, ConsumableType Type, ItemRarity Rarity)> GetAllConsumables()
        {
            return new List<(string, ConsumableType, ItemRarity)>
            {
                ("Minor Health Potion", ConsumableType.HealthPotion, ItemRarity.Common),
                ("Health Potion", ConsumableType.HealthPotion, ItemRarity.Common),
                ("Greater Health Potion", ConsumableType.HealthPotion, ItemRarity.Uncommon),
                ("Bread", ConsumableType.Food, ItemRarity.Common),
                ("Cooked Meat", ConsumableType.Food, ItemRarity.Common),
                ("Elixir of Vitality", ConsumableType.Elixir, ItemRarity.Rare),
                ("Combat Elixir", ConsumableType.Elixir, ItemRarity.Epic),
                ("Fire Bomb", ConsumableType.Bomb, ItemRarity.Uncommon),
                ("Antidote", ConsumableType.Antidote, ItemRarity.Common)
            };
        }

        #endregion

        #region Quest Item Factory Methods

        /// <summary>
        /// Get a quest item by name
        /// </summary>
        public static QuestItem GetQuestItem(string questItemName, string questId = "")
        {
            return questItemName.ToLower() switch
            {
                "ancient amulet" => new QuestItem(
                    "Ancient Amulet",
                    "A mysterious amulet that radiates with ancient power.",
                    questId,
                    isKeyItem: true
                ),

                "mysterious letter" => new QuestItem(
                    "Mysterious Letter",
                    "A sealed letter with no return address.",
                    questId,
                    isKeyItem: false
                ),

                "king's signet" => new QuestItem(
                    "King's Signet",
                    "The official seal of the kingdom.",
                    questId,
                    isKeyItem: true
                ),

                "old map" => new QuestItem(
                    "Old Map",
                    "A weathered map showing a marked location.",
                    questId,
                    isKeyItem: false
                ),

                "crystal shard" => new QuestItem(
                    "Crystal Shard",
                    "A fragment of a larger magical crystal.",
                    questId,
                    isKeyItem: false
                ),

                _ => throw new ArgumentException($"Quest item '{questItemName}' not found in database.")
            };
        }

        /// <summary>
        /// Get all available quest items
        /// </summary>
        public static List<(string Name, bool IsKeyItem)> GetAllQuestItems()
        {
            return new List<(string, bool)>
            {
                ("Ancient Amulet", true),
                ("Mysterious Letter", false),
                ("King's Signet", true),
                ("Old Map", false),
                ("Crystal Shard", false)
            };
        }

        #endregion

        #region Generic Item Lookup

        /// <summary>
        /// Get any item by name (searches all categories)
        /// </summary>
        public static Item GetItemByName(string itemName, int level = 1, int quantity = 1)
        {
            // Try weapons first
            try { return GetWeapon(itemName, level); }
            catch (ArgumentException) { }

            // Try armor
            try { return GetArmor(itemName, level); }
            catch (ArgumentException) { }

            // Try consumables
            try { return GetConsumable(itemName, level, quantity); }
            catch (ArgumentException) { }

            // Try quest items
            try { return GetQuestItem(itemName); }
            catch (ArgumentException) { }

            throw new ArgumentException($"Item '{itemName}' not found in the database.");
        }

        /// <summary>
        /// Get a random item of a specific category
        /// </summary>
        public static Item GetRandomItem(ItemCategory category, Random rng, int level = 1)
        {
            return category switch
            {
                ItemCategory.Weapon => GetRandomWeapon(rng, level),
                ItemCategory.Armor => GetRandomArmor(rng, level),
                ItemCategory.Consumable => GetRandomConsumable(rng, level),
                _ => throw new ArgumentException($"Cannot generate random item for category {category}")
            };
        }

        private static Weapon GetRandomWeapon(Random rng, int level)
        {
            var weapons = GetAllWeapons();
            var chosen = weapons[rng.Next(weapons.Count)];
            return GetWeapon(chosen.Name, level);
        }

        private static Armor GetRandomArmor(Random rng, int level)
        {
            var armors = GetAllArmor();
            var chosen = armors[rng.Next(armors.Count)];
            return GetArmor(chosen.Name, level);
        }

        private static Consumable GetRandomConsumable(Random rng, int level)
        {
            var consumables = GetAllConsumables();
            var chosen = consumables[rng.Next(consumables.Count)];
            return GetConsumable(chosen.Name, level, quantity: rng.Next(1, 5));
        }

        #endregion
    }
}