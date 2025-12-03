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
                    value: 10,
                    minDamageUpgradeRange: (1, 2),  // Sword: +1 to +2 min damage
                    damageShiftRange: (1, 1),        // Sword: +1/+1 shift
                    maxDamageUpgradeRange: (1, 3)    // Sword: +1 to +3 max damage
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
                    value: 50,
                    minDamageUpgradeRange: (1, 2),  // Sword: +1 to +2 min damage
                    damageShiftRange: (1, 1),        // Sword: +1/+1 shift
                    maxDamageUpgradeRange: (1, 3)    // Sword: +1 to +3 max damage
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
                    value: 150,
                    minDamageUpgradeRange: (1, 2),  // Sword: +1 to +2 min damage
                    damageShiftRange: (1, 1),        // Sword: +1/+1 shift
                    maxDamageUpgradeRange: (1, 3)    // Sword: +1 to +3 max damage
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
                    value: 15,
                    minDamageUpgradeRange: (0, 2),  // Axe: +0 to +2 min damage (can miss)
                    damageShiftRange: (1, 2),        // Axe: +1 min, +2 max (widens range)
                    maxDamageUpgradeRange: (1, 4)    // Axe: +1 to +4 max damage (huge ceiling)
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
                    value: 200,
                    minDamageUpgradeRange: (0, 2),  // Axe: +0 to +2 min damage (can miss)
                    damageShiftRange: (1, 2),        // Axe: +1 min, +2 max (widens range)
                    maxDamageUpgradeRange: (1, 4)    // Axe: +1 to +4 max damage (huge ceiling)
                ),

                // Maces
                "blunt mace" => new Weapon(
                    "Blunt Mace",
                    "A heavy mace designed to crush armor and bone.",
                    WeaponType.Mace,
                    minDamage: 8,
                    maxDamage: 15,
                    critChance: 18,
                    accuracy: 60,
                    ItemRarity.Common,
                    level,
                    value: 80,
                    minDamageUpgradeRange: (0, 1),  // Mace: +0 to +1 min damage (low growth)
                    damageShiftRange: (0, 1),        // Mace: +0 to +1 on both (slow growth)
                    maxDamageUpgradeRange: (0, 2)    // Mace: +0 to +2 max damage (low growth)
                ),

                "spiked mace" => new Weapon(
                    "Spiked Mace",
                    "A brutal mace with metal spikes for maximum damage.",
                    WeaponType.Mace,
                    minDamage: 10,
                    maxDamage: 18,
                    critChance: 20,
                    accuracy: 58,
                    ItemRarity.Uncommon,
                    level,
                    value: 175,
                    minDamageUpgradeRange: (0, 1),  // Mace: +0 to +1 min damage (low growth)
                    damageShiftRange: (0, 1),        // Mace: +0 to +1 on both (slow growth)
                    maxDamageUpgradeRange: (0, 2)    // Mace: +0 to +2 max damage (low growth)
                ),

                // Spears
                "hunting spear" => new Weapon(
                    "Hunting Spear",
                    "A well-balanced spear with excellent reach and precision.",
                    WeaponType.Spear,
                    minDamage: 5,
                    maxDamage: 11,
                    critChance: 16,
                    accuracy: 85,
                    ItemRarity.Common,
                    level,
                    value: 60,
                    minDamageUpgradeRange: (0, 1),  // Spear: +0 to +1 min damage (smaller growth)
                    damageShiftRange: (1, 1),        // Spear: +1/+1 shift
                    maxDamageUpgradeRange: (0, 2)    // Spear: +0 to +2 max damage (smaller growth)
                ),

                "war spear" => new Weapon(
                    "War Spear",
                    "A deadly spear designed for combat, offering superior accuracy and killing power.",
                    WeaponType.Spear,
                    minDamage: 8,
                    maxDamage: 16,
                    critChance: 18,
                    accuracy: 88,
                    ItemRarity.Uncommon,
                    level,
                    value: 150,
                    minDamageUpgradeRange: (0, 1),  // Spear: +0 to +1 min damage (smaller growth)
                    damageShiftRange: (1, 1),        // Spear: +1/+1 shift
                    maxDamageUpgradeRange: (0, 2)    // Spear: +0 to +2 max damage (smaller growth)
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
                    value: 20,
                    minDamageUpgradeRange: (1, 3),  // Staff: +1 to +3 min damage
                    damageShiftRange: (1, 1),        // Staff: +1/+1 shift
                    maxDamageUpgradeRange: (0, 3)    // Staff: +0 to +3 max damage (can miss)
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
                    value: 300,
                    minDamageUpgradeRange: (1, 3),  // Staff: +1 to +3 min damage
                    damageShiftRange: (1, 1),        // Staff: +1/+1 shift
                    maxDamageUpgradeRange: (0, 3)    // Staff: +0 to +3 max damage (can miss)
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
                    value: 40,
                    minDamageUpgradeRange: (0, 1),  // Bow: +0 to +1 min damage
                    damageShiftRange: (0, 2),        // Bow: +0 to +2 on BOTH (random shift amount)
                    maxDamageUpgradeRange: (1, 7)    // Bow: +1 to +7 max damage (extreme range)
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
                    value: 180,
                    minDamageUpgradeRange: (0, 1),  // Bow: +0 to +1 min damage
                    damageShiftRange: (0, 2),        // Bow: +0 to +2 on BOTH (random shift amount)
                    maxDamageUpgradeRange: (1, 7)    // Bow: +1 to +7 max damage (extreme range)
                ),

                // Crossbows
                "light crossbow" => new Weapon(
                    "Light Crossbow",
                    "A compact crossbow with reliable accuracy.",
                    WeaponType.Crossbow,
                    minDamage: 6,
                    maxDamage: 12,
                    critChance: 5,
                    accuracy: 85,
                    ItemRarity.Common,
                    level,
                    value: 70,
                    minDamageUpgradeRange: (0, 1),  // Crossbow: +0 to +1 min damage
                    damageShiftRange: (0, 2),        // Crossbow: +0 to +2 on BOTH (random shift like bow)
                    maxDamageUpgradeRange: (1, 7)    // Crossbow: +1 to +7 max damage (extreme range like bow)
                ),

                "heavy crossbow" => new Weapon(
                    "Heavy Crossbow",
                    "A powerful crossbow that hits with devastating force.",
                    WeaponType.Crossbow,
                    minDamage: 10,
                    maxDamage: 20,
                    critChance: 7,
                    accuracy: 88,
                    ItemRarity.Uncommon,
                    level,
                    value: 200,
                    minDamageUpgradeRange: (0, 1),  // Crossbow: +0 to +1 min damage
                    damageShiftRange: (0, 2),        // Crossbow: +0 to +2 on BOTH (random shift like bow)
                    maxDamageUpgradeRange: (1, 7)    // Crossbow: +1 to +7 max damage (extreme range like bow)
                ),

                // Wands
                "apprentice wand" => new Weapon(
                    "Apprentice Wand",
                    "A simple wand for casting basic spells with consistent power.",
                    WeaponType.Wand,
                    minDamage: 4,
                    maxDamage: 7,
                    critChance: 8,
                    accuracy: 82,
                    ItemRarity.Common,
                    level,
                    value: 50,
                    minDamageUpgradeRange: (1, 3),  // Wand: +1 to +3 min damage (strong min growth)
                    damageShiftRange: (2, 1),        // Wand: +2 min, +1 max (narrows range)
                    maxDamageUpgradeRange: (0, 2)    // Wand: +0 to +2 max damage (weak max growth)
                ),

                "arcane wand" => new Weapon(
                    "Arcane Wand",
                    "A finely crafted wand that channels magic with precise, focused energy.",
                    WeaponType.Wand,
                    minDamage: 8,
                    maxDamage: 12,
                    critChance: 10,
                    accuracy: 85,
                    ItemRarity.Uncommon,
                    level,
                    value: 160,
                    minDamageUpgradeRange: (1, 3),  // Wand: +1 to +3 min damage (strong min growth)
                    damageShiftRange: (2, 1),        // Wand: +2 min, +1 max (narrows range)
                    maxDamageUpgradeRange: (0, 2)    // Wand: +0 to +2 max damage (weak max growth)
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
                    value: 8,
                    minDamageUpgradeRange: (0, 1),  // Dagger: +0 to +1 min damage
                    damageShiftRange: (1, 1),        // Dagger: +1/+1 shift
                    maxDamageUpgradeRange: (0, 2)    // Dagger: +0 to +2 max damage
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
                    value: 250,
                    minDamageUpgradeRange: (0, 1),  // Dagger: +0 to +1 min damage
                    damageShiftRange: (1, 1),        // Dagger: +1/+1 shift
                    maxDamageUpgradeRange: (0, 2)    // Dagger: +0 to +2 max damage
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
                ("Blunt Mace", WeaponType.Mace, ItemRarity.Common),
                ("Spiked Mace", WeaponType.Mace, ItemRarity.Uncommon),
                ("Hunting Spear", WeaponType.Spear, ItemRarity.Common),
                ("War Spear", WeaponType.Spear, ItemRarity.Uncommon),
                ("Wooden Staff", WeaponType.Staff, ItemRarity.Common),
                ("Arcane Staff", WeaponType.Staff, ItemRarity.Rare),
                ("Hunting Bow", WeaponType.Bow, ItemRarity.Common),
                ("Longbow", WeaponType.Bow, ItemRarity.Uncommon),
                ("Light Crossbow", WeaponType.Crossbow, ItemRarity.Common),
                ("Heavy Crossbow", WeaponType.Crossbow, ItemRarity.Uncommon),
                ("Apprentice Wand", WeaponType.Wand, ItemRarity.Common),
                ("Arcane Wand", WeaponType.Wand, ItemRarity.Uncommon),
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

                // Revival Potions
                // Level 1-4: Minor Revival Potion (10%-40% HP restoration)
                "minor revival potion" => new Consumable(
                    "Minor Revival Potion",
                    "Revives a fallen ally with minimal health restoration.",
                    ConsumableType.RevivePotion,
                    effectPower: 0, // Not used - revival scales with level in UseRevivePotion()
                    ItemRarity.Uncommon,
                    Math.Clamp(level, 1, 4), // Levels 1-4
                    value: 50
                ),

                // Level 5-8: Revival Potion (50%-80% HP restoration)
                "revival potion" => new Consumable(
                    "Revival Potion",
                    "Revives a fallen ally with moderate health restoration.",
                    ConsumableType.RevivePotion,
                    effectPower: 0, // Not used - revival scales with level in UseRevivePotion()
                    ItemRarity.Rare,
                    Math.Clamp(level, 5, 8), // Levels 5-8
                    value: 100
                ),

                // Level 9-10: Greater Revival Potion (90%-100% HP restoration)
                "greater revival potion" => new Consumable(
                    "Greater Revival Potion",
                    "Revives a fallen ally with substantial health restoration, nearly full strength.",
                    ConsumableType.RevivePotion,
                    effectPower: 0, // Not used - revival scales with level in UseRevivePotion()
                    ItemRarity.Epic,
                    Math.Clamp(level, 9, 10), // Levels 9-10
                    value: 200
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
                ("Minor Revival Potion", ConsumableType.RevivePotion, ItemRarity.Uncommon),
                ("Revival Potion", ConsumableType.RevivePotion, ItemRarity.Rare),
                ("Greater Revival Potion", ConsumableType.RevivePotion, ItemRarity.Epic),
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