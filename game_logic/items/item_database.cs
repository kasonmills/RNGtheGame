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

                // Common Swords (continued)
                "worn blade" => new Weapon(
                    "Worn Blade",
                    "A training sword with a dull edge.",
                    WeaponType.Sword,
                    minDamage: 3,
                    maxDamage: 6,
                    critChance: 5,
                    accuracy: 68,
                    ItemRarity.Common,
                    level,
                    value: 8,
                    minDamageUpgradeRange: (1, 2),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (1, 3)
                ),

                "bronze sword" => new Weapon(
                    "Bronze Sword",
                    "Ancient bronze weapon, brittle but functional.",
                    WeaponType.Sword,
                    minDamage: 4,
                    maxDamage: 8,
                    critChance: 6,
                    accuracy: 72,
                    ItemRarity.Common,
                    level,
                    value: 15,
                    minDamageUpgradeRange: (1, 2),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (1, 3)
                ),

                "militia sword" => new Weapon(
                    "Militia Sword",
                    "Standard issue for city guards.",
                    WeaponType.Sword,
                    minDamage: 5,
                    maxDamage: 10,
                    critChance: 7,
                    accuracy: 73,
                    ItemRarity.Common,
                    level,
                    value: 40,
                    minDamageUpgradeRange: (1, 2),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (1, 3)
                ),

                "scimitar" => new Weapon(
                    "Scimitar",
                    "Curved blade with swift strikes.",
                    WeaponType.Sword,
                    minDamage: 4,
                    maxDamage: 9,
                    critChance: 9,
                    accuracy: 76,
                    ItemRarity.Common,
                    level,
                    value: 45,
                    minDamageUpgradeRange: (1, 2),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (1, 3)
                ),

                "gladius" => new Weapon(
                    "Gladius",
                    "Short Roman-style sword.",
                    WeaponType.Sword,
                    minDamage: 6,
                    maxDamage: 11,
                    critChance: 8,
                    accuracy: 74,
                    ItemRarity.Common,
                    level,
                    value: 55,
                    minDamageUpgradeRange: (1, 2),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (1, 3)
                ),

                "cutlass" => new Weapon(
                    "Cutlass",
                    "Sailor's blade, good for close combat.",
                    WeaponType.Sword,
                    minDamage: 5,
                    maxDamage: 11,
                    critChance: 7,
                    accuracy: 72,
                    ItemRarity.Common,
                    level,
                    value: 48,
                    minDamageUpgradeRange: (1, 2),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (1, 3)
                ),

                // Uncommon Swords
                "silver sword" => new Weapon(
                    "Silver Sword",
                    "Effective against monsters and undead.",
                    WeaponType.Sword,
                    minDamage: 11,
                    maxDamage: 19,
                    critChance: 11,
                    accuracy: 82,
                    ItemRarity.Uncommon,
                    level,
                    value: 180,
                    minDamageUpgradeRange: (1, 2),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (1, 3)
                ),

                "bastard sword" => new Weapon(
                    "Bastard Sword",
                    "Versatile one-and-a-half handed weapon.",
                    WeaponType.Sword,
                    minDamage: 13,
                    maxDamage: 22,
                    critChance: 10,
                    accuracy: 78,
                    ItemRarity.Uncommon,
                    level,
                    value: 220,
                    minDamageUpgradeRange: (1, 2),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (1, 3)
                ),

                "falchion" => new Weapon(
                    "Falchion",
                    "Heavy slashing blade.",
                    WeaponType.Sword,
                    minDamage: 12,
                    maxDamage: 21,
                    critChance: 12,
                    accuracy: 79,
                    ItemRarity.Uncommon,
                    level,
                    value: 195,
                    minDamageUpgradeRange: (1, 2),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (1, 3)
                ),

                "longsword" => new Weapon(
                    "Longsword",
                    "Well-balanced knightly weapon.",
                    WeaponType.Sword,
                    minDamage: 11,
                    maxDamage: 20,
                    critChance: 11,
                    accuracy: 81,
                    ItemRarity.Uncommon,
                    level,
                    value: 190,
                    minDamageUpgradeRange: (1, 2),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (1, 3)
                ),

                // Rare Swords
                "mithril blade" => new Weapon(
                    "Mithril Blade",
                    "Lightweight legendary metal, never dulls.",
                    WeaponType.Sword,
                    minDamage: 15,
                    maxDamage: 28,
                    critChance: 14,
                    accuracy: 86,
                    ItemRarity.Rare,
                    level,
                    value: 400,
                    minDamageUpgradeRange: (1, 2),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (1, 3)
                ),

                "flamberge" => new Weapon(
                    "Flamberge",
                    "Wavy blade causes devastating wounds.",
                    WeaponType.Sword,
                    minDamage: 16,
                    maxDamage: 30,
                    critChance: 13,
                    accuracy: 83,
                    ItemRarity.Rare,
                    level,
                    value: 380,
                    minDamageUpgradeRange: (1, 2),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (1, 3)
                ),

                "katana" => new Weapon(
                    "Katana",
                    "Masterwork eastern blade, razor sharp.",
                    WeaponType.Sword,
                    minDamage: 14,
                    maxDamage: 29,
                    critChance: 16,
                    accuracy: 88,
                    ItemRarity.Rare,
                    level,
                    value: 420,
                    minDamageUpgradeRange: (1, 2),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (1, 3)
                ),

                "vorpal blade" => new Weapon(
                    "Vorpal Blade",
                    "Enchanted to cut through anything.",
                    WeaponType.Sword,
                    minDamage: 17,
                    maxDamage: 31,
                    critChance: 15,
                    accuracy: 85,
                    ItemRarity.Rare,
                    level,
                    value: 450,
                    minDamageUpgradeRange: (1, 2),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (1, 3)
                ),

                // Epic Swords
                "dragonforged blade" => new Weapon(
                    "Dragonforged Blade",
                    "Tempered in dragonfire, burns enemies.",
                    WeaponType.Sword,
                    minDamage: 22,
                    maxDamage: 40,
                    critChance: 18,
                    accuracy: 90,
                    ItemRarity.Epic,
                    level,
                    value: 800,
                    minDamageUpgradeRange: (1, 2),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (1, 3)
                ),

                "stormbreaker sword" => new Weapon(
                    "Stormbreaker Sword",
                    "Crackles with lightning energy.",
                    WeaponType.Sword,
                    minDamage: 20,
                    maxDamage: 38,
                    critChance: 20,
                    accuracy: 89,
                    ItemRarity.Epic,
                    level,
                    value: 750,
                    minDamageUpgradeRange: (1, 2),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (1, 3)
                ),

                "soulreaver" => new Weapon(
                    "Soulreaver",
                    "Drinks the essence of fallen foes.",
                    WeaponType.Sword,
                    minDamage: 21,
                    maxDamage: 39,
                    critChance: 19,
                    accuracy: 91,
                    ItemRarity.Epic,
                    level,
                    value: 820,
                    minDamageUpgradeRange: (1, 2),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (1, 3)
                ),

                // Legendary Swords
                "eternal edge" => new Weapon(
                    "Eternal Edge",
                    "Never dulls, ancient artifact of war.",
                    WeaponType.Sword,
                    minDamage: 30,
                    maxDamage: 55,
                    critChance: 25,
                    accuracy: 95,
                    ItemRarity.Legendary,
                    level,
                    value: 2000,
                    minDamageUpgradeRange: (1, 2),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (1, 3)
                ),

                "excalibur" => new Weapon(
                    "Excalibur",
                    "The legendary blade of kings.",
                    WeaponType.Sword,
                    minDamage: 32,
                    maxDamage: 58,
                    critChance: 26,
                    accuracy: 96,
                    ItemRarity.Legendary,
                    level,
                    value: 2500,
                    minDamageUpgradeRange: (1, 2),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (1, 3)
                ),

                "muramasa" => new Weapon(
                    "Muramasa",
                    "Cursed blade of unmatched sharpness.",
                    WeaponType.Sword,
                    minDamage: 31,
                    maxDamage: 57,
                    critChance: 28,
                    accuracy: 94,
                    ItemRarity.Legendary,
                    level,
                    value: 2200,
                    minDamageUpgradeRange: (1, 2),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (1, 3)
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

                // Common Axes (continued)
                "woodcutter's axe" => new Weapon(
                    "Woodcutter's Axe",
                    "Simple axe used for chopping wood.",
                    WeaponType.Axe,
                    minDamage: 3,
                    maxDamage: 9,
                    critChance: 10,
                    accuracy: 65,
                    ItemRarity.Common,
                    level,
                    value: 12,
                    minDamageUpgradeRange: (0, 2),
                    damageShiftRange: (1, 2),
                    maxDamageUpgradeRange: (1, 4)
                ),

                "hatchet" => new Weapon(
                    "Hatchet",
                    "A small, light axe for one-handed use.",
                    WeaponType.Axe,
                    minDamage: 4,
                    maxDamage: 8,
                    critChance: 11,
                    accuracy: 68,
                    ItemRarity.Common,
                    level,
                    value: 15,
                    minDamageUpgradeRange: (0, 2),
                    damageShiftRange: (1, 2),
                    maxDamageUpgradeRange: (1, 4)
                ),

                "iron axe" => new Weapon(
                    "Iron Axe",
                    "Reliable iron axe for combat.",
                    WeaponType.Axe,
                    minDamage: 5,
                    maxDamage: 11,
                    critChance: 12,
                    accuracy: 66,
                    ItemRarity.Common,
                    level,
                    value: 20,
                    minDamageUpgradeRange: (0, 2),
                    damageShiftRange: (1, 2),
                    maxDamageUpgradeRange: (1, 4)
                ),

                "throwing axe" => new Weapon(
                    "Throwing Axe",
                    "Balanced for throwing or melee combat.",
                    WeaponType.Axe,
                    minDamage: 4,
                    maxDamage: 10,
                    critChance: 13,
                    accuracy: 64,
                    ItemRarity.Common,
                    level,
                    value: 18,
                    minDamageUpgradeRange: (0, 2),
                    damageShiftRange: (1, 2),
                    maxDamageUpgradeRange: (1, 4)
                ),

                "bearded axe" => new Weapon(
                    "Bearded Axe",
                    "Features an extended lower blade.",
                    WeaponType.Axe,
                    minDamage: 6,
                    maxDamage: 12,
                    critChance: 11,
                    accuracy: 67,
                    ItemRarity.Common,
                    level,
                    value: 22,
                    minDamageUpgradeRange: (0, 2),
                    damageShiftRange: (1, 2),
                    maxDamageUpgradeRange: (1, 4)
                ),

                "hand axe" => new Weapon(
                    "Hand Axe",
                    "Versatile one-handed combat axe.",
                    WeaponType.Axe,
                    minDamage: 5,
                    maxDamage: 10,
                    critChance: 12,
                    accuracy: 69,
                    ItemRarity.Common,
                    level,
                    value: 17,
                    minDamageUpgradeRange: (0, 2),
                    damageShiftRange: (1, 2),
                    maxDamageUpgradeRange: (1, 4)
                ),

                "cleaver" => new Weapon(
                    "Cleaver",
                    "Wide-bladed axe for chopping.",
                    WeaponType.Axe,
                    minDamage: 6,
                    maxDamage: 11,
                    critChance: 10,
                    accuracy: 65,
                    ItemRarity.Common,
                    level,
                    value: 19,
                    minDamageUpgradeRange: (0, 2),
                    damageShiftRange: (1, 2),
                    maxDamageUpgradeRange: (1, 4)
                ),

                // Uncommon Axes (continued)
                "great axe" => new Weapon(
                    "Great Axe",
                    "Massive two-handed axe.",
                    WeaponType.Axe,
                    minDamage: 10,
                    maxDamage: 22,
                    critChance: 14,
                    accuracy: 68,
                    ItemRarity.Uncommon,
                    level,
                    value: 180,
                    minDamageUpgradeRange: (0, 2),
                    damageShiftRange: (1, 2),
                    maxDamageUpgradeRange: (1, 4)
                ),

                "berserker axe" => new Weapon(
                    "Berserker Axe",
                    "Jagged axe that seems to thirst for battle.",
                    WeaponType.Axe,
                    minDamage: 11,
                    maxDamage: 24,
                    critChance: 16,
                    accuracy: 67,
                    ItemRarity.Uncommon,
                    level,
                    value: 210,
                    minDamageUpgradeRange: (0, 2),
                    damageShiftRange: (1, 2),
                    maxDamageUpgradeRange: (1, 4)
                ),

                "double-bladed axe" => new Weapon(
                    "Double-Bladed Axe",
                    "Deadly axe with blades on both sides.",
                    WeaponType.Axe,
                    minDamage: 13,
                    maxDamage: 23,
                    critChance: 15,
                    accuracy: 69,
                    ItemRarity.Uncommon,
                    level,
                    value: 220,
                    minDamageUpgradeRange: (0, 2),
                    damageShiftRange: (1, 2),
                    maxDamageUpgradeRange: (1, 4)
                ),

                "war axe" => new Weapon(
                    "War Axe",
                    "Battle-hardened military axe.",
                    WeaponType.Axe,
                    minDamage: 12,
                    maxDamage: 26,
                    critChance: 14,
                    accuracy: 70,
                    ItemRarity.Uncommon,
                    level,
                    value: 200,
                    minDamageUpgradeRange: (0, 2),
                    damageShiftRange: (1, 2),
                    maxDamageUpgradeRange: (1, 4)
                ),

                // Rare Axes
                "executioner's axe" => new Weapon(
                    "Executioner's Axe",
                    "Heavy axe designed for clean, powerful strikes.",
                    WeaponType.Axe,
                    minDamage: 18,
                    maxDamage: 35,
                    critChance: 18,
                    accuracy: 72,
                    ItemRarity.Rare,
                    level,
                    value: 500,
                    minDamageUpgradeRange: (0, 2),
                    damageShiftRange: (1, 2),
                    maxDamageUpgradeRange: (1, 4)
                ),

                "frost axe" => new Weapon(
                    "Frost Axe",
                    "Enchanted with eternal ice.",
                    WeaponType.Axe,
                    minDamage: 16,
                    maxDamage: 33,
                    critChance: 17,
                    accuracy: 74,
                    ItemRarity.Rare,
                    level,
                    value: 550,
                    minDamageUpgradeRange: (0, 2),
                    damageShiftRange: (1, 2),
                    maxDamageUpgradeRange: (1, 4)
                ),

                "thunderstrike axe" => new Weapon(
                    "Thunderstrike Axe",
                    "Crackles with lightning on impact.",
                    WeaponType.Axe,
                    minDamage: 17,
                    maxDamage: 36,
                    critChance: 19,
                    accuracy: 73,
                    ItemRarity.Rare,
                    level,
                    value: 600,
                    minDamageUpgradeRange: (0, 2),
                    damageShiftRange: (1, 2),
                    maxDamageUpgradeRange: (1, 4)
                ),

                "adamantine axe" => new Weapon(
                    "Adamantine Axe",
                    "Forged from unbreakable metal.",
                    WeaponType.Axe,
                    minDamage: 19,
                    maxDamage: 34,
                    critChance: 16,
                    accuracy: 75,
                    ItemRarity.Rare,
                    level,
                    value: 580,
                    minDamageUpgradeRange: (0, 2),
                    damageShiftRange: (1, 2),
                    maxDamageUpgradeRange: (1, 4)
                ),

                // Epic Axes
                "titan's cleaver" => new Weapon(
                    "Titan's Cleaver",
                    "Colossal axe wielded by ancient giants.",
                    WeaponType.Axe,
                    minDamage: 24,
                    maxDamage: 48,
                    critChance: 22,
                    accuracy: 78,
                    ItemRarity.Epic,
                    level,
                    value: 1200,
                    minDamageUpgradeRange: (0, 2),
                    damageShiftRange: (1, 2),
                    maxDamageUpgradeRange: (1, 4)
                ),

                "hellfire axe" => new Weapon(
                    "Hellfire Axe",
                    "Burns with infernal flames.",
                    WeaponType.Axe,
                    minDamage: 25,
                    maxDamage: 50,
                    critChance: 23,
                    accuracy: 77,
                    ItemRarity.Epic,
                    level,
                    value: 1300,
                    minDamageUpgradeRange: (0, 2),
                    damageShiftRange: (1, 2),
                    maxDamageUpgradeRange: (1, 4)
                ),

                "worldbreaker" => new Weapon(
                    "Worldbreaker",
                    "Legendary axe said to split mountains.",
                    WeaponType.Axe,
                    minDamage: 26,
                    maxDamage: 49,
                    critChance: 21,
                    accuracy: 79,
                    ItemRarity.Epic,
                    level,
                    value: 1250,
                    minDamageUpgradeRange: (0, 2),
                    damageShiftRange: (1, 2),
                    maxDamageUpgradeRange: (1, 4)
                ),

                // Legendary Axes
                "jarnbjorn" => new Weapon(
                    "Jarnbjorn",
                    "The iron bear axe of Norse legend.",
                    WeaponType.Axe,
                    minDamage: 30,
                    maxDamage: 60,
                    critChance: 26,
                    accuracy: 82,
                    ItemRarity.Legendary,
                    level,
                    value: 2000,
                    minDamageUpgradeRange: (0, 2),
                    damageShiftRange: (1, 2),
                    maxDamageUpgradeRange: (1, 4)
                ),

                "ginnungagap" => new Weapon(
                    "Ginnungagap",
                    "Named for the primordial void, cleaves reality.",
                    WeaponType.Axe,
                    minDamage: 32,
                    maxDamage: 62,
                    critChance: 27,
                    accuracy: 83,
                    ItemRarity.Legendary,
                    level,
                    value: 2300,
                    minDamageUpgradeRange: (0, 2),
                    damageShiftRange: (1, 2),
                    maxDamageUpgradeRange: (1, 4)
                ),

                "ragnarok's edge" => new Weapon(
                    "Ragnarok's Edge",
                    "The end of all things, forged into steel.",
                    WeaponType.Axe,
                    minDamage: 31,
                    maxDamage: 61,
                    critChance: 28,
                    accuracy: 81,
                    ItemRarity.Legendary,
                    level,
                    value: 2100,
                    minDamageUpgradeRange: (0, 2),
                    damageShiftRange: (1, 2),
                    maxDamageUpgradeRange: (1, 4)
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

                // Common Maces (continued)
                "wooden club" => new Weapon(
                    "Wooden Club",
                    "Basic club carved from hardwood.",
                    WeaponType.Mace,
                    minDamage: 6,
                    maxDamage: 12,
                    critChance: 15,
                    accuracy: 62,
                    ItemRarity.Common,
                    level,
                    value: 60,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "morning star" => new Weapon(
                    "Morning Star",
                    "Mace with a spiked metal ball.",
                    WeaponType.Mace,
                    minDamage: 7,
                    maxDamage: 14,
                    critChance: 17,
                    accuracy: 59,
                    ItemRarity.Common,
                    level,
                    value: 75,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "flanged mace" => new Weapon(
                    "Flanged Mace",
                    "Metal mace with ridged flanges for crushing.",
                    WeaponType.Mace,
                    minDamage: 8,
                    maxDamage: 13,
                    critChance: 16,
                    accuracy: 61,
                    ItemRarity.Common,
                    level,
                    value: 70,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "cudgel" => new Weapon(
                    "Cudgel",
                    "Short, thick club.",
                    WeaponType.Mace,
                    minDamage: 7,
                    maxDamage: 12,
                    critChance: 14,
                    accuracy: 63,
                    ItemRarity.Common,
                    level,
                    value: 65,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "warhammer" => new Weapon(
                    "Warhammer",
                    "Heavy hammer designed for battle.",
                    WeaponType.Mace,
                    minDamage: 9,
                    maxDamage: 16,
                    critChance: 19,
                    accuracy: 58,
                    ItemRarity.Common,
                    level,
                    value: 85,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "iron club" => new Weapon(
                    "Iron Club",
                    "Solid iron club with devastating weight.",
                    WeaponType.Mace,
                    minDamage: 8,
                    maxDamage: 15,
                    critChance: 16,
                    accuracy: 60,
                    ItemRarity.Common,
                    level,
                    value: 78,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "mallet" => new Weapon(
                    "Mallet",
                    "Large wooden hammer.",
                    WeaponType.Mace,
                    minDamage: 7,
                    maxDamage: 13,
                    critChance: 15,
                    accuracy: 61,
                    ItemRarity.Common,
                    level,
                    value: 68,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                // Uncommon Maces (continued)
                "heavy mace" => new Weapon(
                    "Heavy Mace",
                    "Reinforced mace for serious combat.",
                    WeaponType.Mace,
                    minDamage: 11,
                    maxDamage: 20,
                    critChance: 21,
                    accuracy: 57,
                    ItemRarity.Uncommon,
                    level,
                    value: 185,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "ball and chain" => new Weapon(
                    "Ball and Chain",
                    "Chained weapon with devastating momentum.",
                    WeaponType.Mace,
                    minDamage: 12,
                    maxDamage: 22,
                    critChance: 22,
                    accuracy: 55,
                    ItemRarity.Uncommon,
                    level,
                    value: 200,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "maul" => new Weapon(
                    "Maul",
                    "Massive two-handed hammer.",
                    WeaponType.Mace,
                    minDamage: 13,
                    maxDamage: 21,
                    critChance: 20,
                    accuracy: 56,
                    ItemRarity.Uncommon,
                    level,
                    value: 195,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "grand mace" => new Weapon(
                    "Grand Mace",
                    "Ceremonial mace of exceptional craftsmanship.",
                    WeaponType.Mace,
                    minDamage: 11,
                    maxDamage: 19,
                    critChance: 19,
                    accuracy: 59,
                    ItemRarity.Uncommon,
                    level,
                    value: 180,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                // Rare Maces
                "blessed mace" => new Weapon(
                    "Blessed Mace",
                    "Holy weapon imbued with divine power.",
                    WeaponType.Mace,
                    minDamage: 16,
                    maxDamage: 30,
                    critChance: 25,
                    accuracy: 65,
                    ItemRarity.Rare,
                    level,
                    value: 480,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "meteor hammer" => new Weapon(
                    "Meteor Hammer",
                    "Forged from a fallen star.",
                    WeaponType.Mace,
                    minDamage: 18,
                    maxDamage: 32,
                    critChance: 26,
                    accuracy: 63,
                    ItemRarity.Rare,
                    level,
                    value: 520,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "obsidian crusher" => new Weapon(
                    "Obsidian Crusher",
                    "Dark volcanic glass shaped into a deadly mace.",
                    WeaponType.Mace,
                    minDamage: 17,
                    maxDamage: 31,
                    critChance: 27,
                    accuracy: 62,
                    ItemRarity.Rare,
                    level,
                    value: 500,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "diamond mace" => new Weapon(
                    "Diamond Mace",
                    "Studded with precious gems, crushes with elegance.",
                    WeaponType.Mace,
                    minDamage: 19,
                    maxDamage: 33,
                    critChance: 24,
                    accuracy: 66,
                    ItemRarity.Rare,
                    level,
                    value: 550,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                // Epic Maces
                "skullcrusher" => new Weapon(
                    "Skullcrusher",
                    "Legendary weapon feared across battlefields.",
                    WeaponType.Mace,
                    minDamage: 25,
                    maxDamage: 45,
                    critChance: 30,
                    accuracy: 68,
                    ItemRarity.Epic,
                    level,
                    value: 1100,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "earthshaker" => new Weapon(
                    "Earthshaker",
                    "Strikes with the force of an earthquake.",
                    WeaponType.Mace,
                    minDamage: 26,
                    maxDamage: 47,
                    critChance: 31,
                    accuracy: 67,
                    ItemRarity.Epic,
                    level,
                    value: 1150,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "bonecrusher" => new Weapon(
                    "Bonecrusher",
                    "No armor can withstand this brutal weapon.",
                    WeaponType.Mace,
                    minDamage: 27,
                    maxDamage: 46,
                    critChance: 29,
                    accuracy: 69,
                    ItemRarity.Epic,
                    level,
                    value: 1120,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                // Legendary Maces
                "mjolnir" => new Weapon(
                    "Mjolnir",
                    "The hammer of Thor, returns when thrown.",
                    WeaponType.Mace,
                    minDamage: 32,
                    maxDamage: 58,
                    critChance: 35,
                    accuracy: 72,
                    ItemRarity.Legendary,
                    level,
                    value: 1900,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "frostmourne's grip" => new Weapon(
                    "Frostmourne's Grip",
                    "Frozen scepter that steals warmth and life.",
                    WeaponType.Mace,
                    minDamage: 33,
                    maxDamage: 60,
                    critChance: 36,
                    accuracy: 71,
                    ItemRarity.Legendary,
                    level,
                    value: 2000,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "the peacemaker" => new Weapon(
                    "The Peacemaker",
                    "Ironically named, ends all conflicts permanently.",
                    WeaponType.Mace,
                    minDamage: 31,
                    maxDamage: 59,
                    critChance: 37,
                    accuracy: 70,
                    ItemRarity.Legendary,
                    level,
                    value: 1950,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 1),
                    maxDamageUpgradeRange: (0, 2)
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

                // Common Spears (continued)
                "javelin" => new Weapon(
                    "Javelin",
                    "Light throwing spear.",
                    WeaponType.Spear,
                    minDamage: 4,
                    maxDamage: 10,
                    critChance: 15,
                    accuracy: 83,
                    ItemRarity.Common,
                    level,
                    value: 55,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "pike" => new Weapon(
                    "Pike",
                    "Long infantry spear.",
                    WeaponType.Spear,
                    minDamage: 6,
                    maxDamage: 12,
                    critChance: 17,
                    accuracy: 86,
                    ItemRarity.Common,
                    level,
                    value: 65,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "lance" => new Weapon(
                    "Lance",
                    "Cavalry spear for mounted combat.",
                    WeaponType.Spear,
                    minDamage: 7,
                    maxDamage: 13,
                    critChance: 18,
                    accuracy: 84,
                    ItemRarity.Common,
                    level,
                    value: 70,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "partisan" => new Weapon(
                    "Partisan",
                    "Spear with side blades.",
                    WeaponType.Spear,
                    minDamage: 6,
                    maxDamage: 11,
                    critChance: 16,
                    accuracy: 87,
                    ItemRarity.Common,
                    level,
                    value: 62,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "trident" => new Weapon(
                    "Trident",
                    "Three-pronged fishing spear.",
                    WeaponType.Spear,
                    minDamage: 5,
                    maxDamage: 12,
                    critChance: 17,
                    accuracy: 82,
                    ItemRarity.Common,
                    level,
                    value: 68,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "halberd" => new Weapon(
                    "Halberd",
                    "Combination of spear and axe.",
                    WeaponType.Spear,
                    minDamage: 7,
                    maxDamage: 14,
                    critChance: 19,
                    accuracy: 85,
                    ItemRarity.Common,
                    level,
                    value: 75,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "glaive" => new Weapon(
                    "Glaive",
                    "Polearm with a single-edged blade.",
                    WeaponType.Spear,
                    minDamage: 6,
                    maxDamage: 13,
                    critChance: 16,
                    accuracy: 84,
                    ItemRarity.Common,
                    level,
                    value: 67,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                // Uncommon Spears (continued)
                "pilum" => new Weapon(
                    "Pilum",
                    "Roman heavy javelin designed to penetrate shields.",
                    WeaponType.Spear,
                    minDamage: 9,
                    maxDamage: 17,
                    critChance: 19,
                    accuracy: 87,
                    ItemRarity.Uncommon,
                    level,
                    value: 160,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "naginata" => new Weapon(
                    "Naginata",
                    "Japanese polearm with curved blade.",
                    WeaponType.Spear,
                    minDamage: 10,
                    maxDamage: 18,
                    critChance: 20,
                    accuracy: 89,
                    ItemRarity.Uncommon,
                    level,
                    value: 170,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "ranseur" => new Weapon(
                    "Ranseur",
                    "Polearm with forked head for disarming.",
                    WeaponType.Spear,
                    minDamage: 9,
                    maxDamage: 16,
                    critChance: 18,
                    accuracy: 90,
                    ItemRarity.Uncommon,
                    level,
                    value: 155,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "spetum" => new Weapon(
                    "Spetum",
                    "Medieval polearm with side spikes.",
                    WeaponType.Spear,
                    minDamage: 8,
                    maxDamage: 15,
                    critChance: 17,
                    accuracy: 88,
                    ItemRarity.Uncommon,
                    level,
                    value: 148,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                // Rare Spears
                "dragon lance" => new Weapon(
                    "Dragon Lance",
                    "Enchanted spear adorned with dragon motifs.",
                    WeaponType.Spear,
                    minDamage: 14,
                    maxDamage: 28,
                    critChance: 24,
                    accuracy: 92,
                    ItemRarity.Rare,
                    level,
                    value: 450,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "phoenix pike" => new Weapon(
                    "Phoenix Pike",
                    "Burns with eternal flames.",
                    WeaponType.Spear,
                    minDamage: 15,
                    maxDamage: 30,
                    critChance: 25,
                    accuracy: 93,
                    ItemRarity.Rare,
                    level,
                    value: 480,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "viper's fang" => new Weapon(
                    "Viper's Fang",
                    "Poisoned spear that strikes with deadly precision.",
                    WeaponType.Spear,
                    minDamage: 13,
                    maxDamage: 27,
                    critChance: 26,
                    accuracy: 94,
                    ItemRarity.Rare,
                    level,
                    value: 460,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "celestial halberd" => new Weapon(
                    "Celestial Halberd",
                    "Blessed by the heavens.",
                    WeaponType.Spear,
                    minDamage: 16,
                    maxDamage: 29,
                    critChance: 23,
                    accuracy: 91,
                    ItemRarity.Rare,
                    level,
                    value: 470,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                // Epic Spears
                "gungnir" => new Weapon(
                    "Gungnir",
                    "Odin's legendary spear that never misses.",
                    WeaponType.Spear,
                    minDamage: 22,
                    maxDamage: 42,
                    critChance: 30,
                    accuracy: 97,
                    ItemRarity.Epic,
                    level,
                    value: 1050,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "longinus" => new Weapon(
                    "Longinus",
                    "The spear of destiny.",
                    WeaponType.Spear,
                    minDamage: 23,
                    maxDamage: 44,
                    critChance: 31,
                    accuracy: 98,
                    ItemRarity.Epic,
                    level,
                    value: 1100,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "rhongomyniad" => new Weapon(
                    "Rhongomyniad",
                    "The spear that shines to the ends of the world.",
                    WeaponType.Spear,
                    minDamage: 24,
                    maxDamage: 43,
                    critChance: 29,
                    accuracy: 96,
                    ItemRarity.Epic,
                    level,
                    value: 1080,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                // Legendary Spears
                "gae bolg" => new Weapon(
                    "Gae Bolg",
                    "Spear of piercing death, reverses causality.",
                    WeaponType.Spear,
                    minDamage: 28,
                    maxDamage: 54,
                    critChance: 35,
                    accuracy: 99,
                    ItemRarity.Legendary,
                    level,
                    value: 1800,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "vasavi shakti" => new Weapon(
                    "Vasavi Shakti",
                    "Divine spear that strikes with thunder.",
                    WeaponType.Spear,
                    minDamage: 29,
                    maxDamage: 56,
                    critChance: 36,
                    accuracy: 100,
                    ItemRarity.Legendary,
                    level,
                    value: 1900,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "brionac" => new Weapon(
                    "Brionac",
                    "The five-pointed spear of the sea god.",
                    WeaponType.Spear,
                    minDamage: 30,
                    maxDamage: 55,
                    critChance: 34,
                    accuracy: 98,
                    ItemRarity.Legendary,
                    level,
                    value: 1850,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
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

                // Common Staves (continued)
                "oak staff" => new Weapon(
                    "Oak Staff",
                    "Sturdy staff carved from oak.",
                    WeaponType.Staff,
                    minDamage: 3,
                    maxDamage: 7,
                    critChance: 4,
                    accuracy: 86,
                    ItemRarity.Common,
                    level,
                    value: 25,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 3)
                ),

                "quarterstaff" => new Weapon(
                    "Quarterstaff",
                    "Long wooden staff for combat and magic.",
                    WeaponType.Staff,
                    minDamage: 4,
                    maxDamage: 8,
                    critChance: 5,
                    accuracy: 87,
                    ItemRarity.Common,
                    level,
                    value: 30,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 3)
                ),

                "mystic rod" => new Weapon(
                    "Mystic Rod",
                    "Short magical rod.",
                    WeaponType.Staff,
                    minDamage: 2,
                    maxDamage: 6,
                    critChance: 6,
                    accuracy: 88,
                    ItemRarity.Common,
                    level,
                    value: 28,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 3)
                ),

                "bone staff" => new Weapon(
                    "Bone Staff",
                    "Staff carved from ancient bones.",
                    WeaponType.Staff,
                    minDamage: 3,
                    maxDamage: 8,
                    critChance: 4,
                    accuracy: 84,
                    ItemRarity.Common,
                    level,
                    value: 32,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 3)
                ),

                "pine staff" => new Weapon(
                    "Pine Staff",
                    "Lightweight pine wood staff.",
                    WeaponType.Staff,
                    minDamage: 2,
                    maxDamage: 7,
                    critChance: 3,
                    accuracy: 85,
                    ItemRarity.Common,
                    level,
                    value: 22,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 3)
                ),

                "crystal rod" => new Weapon(
                    "Crystal Rod",
                    "Rod tipped with a small crystal.",
                    WeaponType.Staff,
                    minDamage: 4,
                    maxDamage: 9,
                    critChance: 6,
                    accuracy: 89,
                    ItemRarity.Common,
                    level,
                    value: 35,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 3)
                ),

                "twisted staff" => new Weapon(
                    "Twisted Staff",
                    "Gnarled wood with natural energy.",
                    WeaponType.Staff,
                    minDamage: 3,
                    maxDamage: 7,
                    critChance: 5,
                    accuracy: 86,
                    ItemRarity.Common,
                    level,
                    value: 27,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 3)
                ),

                // Uncommon Staves
                "battle staff" => new Weapon(
                    "Battle Staff",
                    "Reinforced for combat magic.",
                    WeaponType.Staff,
                    minDamage: 6,
                    maxDamage: 14,
                    critChance: 7,
                    accuracy: 90,
                    ItemRarity.Uncommon,
                    level,
                    value: 140,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 3)
                ),

                "jade staff" => new Weapon(
                    "Jade Staff",
                    "Carved from precious jade stone.",
                    WeaponType.Staff,
                    minDamage: 7,
                    maxDamage: 15,
                    critChance: 8,
                    accuracy: 91,
                    ItemRarity.Uncommon,
                    level,
                    value: 150,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 3)
                ),

                "runic staff" => new Weapon(
                    "Runic Staff",
                    "Inscribed with ancient runes.",
                    WeaponType.Staff,
                    minDamage: 6,
                    maxDamage: 13,
                    critChance: 6,
                    accuracy: 92,
                    ItemRarity.Uncommon,
                    level,
                    value: 145,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 3)
                ),

                "serpent staff" => new Weapon(
                    "Serpent Staff",
                    "Topped with a coiled serpent.",
                    WeaponType.Staff,
                    minDamage: 7,
                    maxDamage: 14,
                    critChance: 7,
                    accuracy: 89,
                    ItemRarity.Uncommon,
                    level,
                    value: 142,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 3)
                ),

                "moonstone staff" => new Weapon(
                    "Moonstone Staff",
                    "Glows with lunar energy.",
                    WeaponType.Staff,
                    minDamage: 8,
                    maxDamage: 16,
                    critChance: 8,
                    accuracy: 93,
                    ItemRarity.Uncommon,
                    level,
                    value: 160,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 3)
                ),

                // Rare Staves (continued)
                "voidwood staff" => new Weapon(
                    "Voidwood Staff",
                    "Wood from the edge of reality.",
                    WeaponType.Staff,
                    minDamage: 11,
                    maxDamage: 25,
                    critChance: 10,
                    accuracy: 94,
                    ItemRarity.Rare,
                    level,
                    value: 380,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 3)
                ),

                "stormcaller staff" => new Weapon(
                    "Stormcaller Staff",
                    "Commands the fury of storms.",
                    WeaponType.Staff,
                    minDamage: 12,
                    maxDamage: 26,
                    critChance: 11,
                    accuracy: 95,
                    ItemRarity.Rare,
                    level,
                    value: 400,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 3)
                ),

                "elderwood staff" => new Weapon(
                    "Elderwood Staff",
                    "Crafted from a thousand-year-old tree.",
                    WeaponType.Staff,
                    minDamage: 10,
                    maxDamage: 24,
                    critChance: 9,
                    accuracy: 96,
                    ItemRarity.Rare,
                    level,
                    value: 370,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 3)
                ),

                // Epic Staves
                "staff of power" => new Weapon(
                    "Staff of Power",
                    "Radiates pure magical energy.",
                    WeaponType.Staff,
                    minDamage: 18,
                    maxDamage: 38,
                    critChance: 15,
                    accuracy: 97,
                    ItemRarity.Epic,
                    level,
                    value: 950,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 3)
                ),

                "astral scepter" => new Weapon(
                    "Astral Scepter",
                    "Channels the power of the stars.",
                    WeaponType.Staff,
                    minDamage: 19,
                    maxDamage: 40,
                    critChance: 16,
                    accuracy: 98,
                    ItemRarity.Epic,
                    level,
                    value: 1000,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 3)
                ),

                "shadowfell staff" => new Weapon(
                    "Shadowfell Staff",
                    "Forged in darkness itself.",
                    WeaponType.Staff,
                    minDamage: 20,
                    maxDamage: 39,
                    critChance: 14,
                    accuracy: 96,
                    ItemRarity.Epic,
                    level,
                    value: 980,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 3)
                ),

                // Legendary Staves
                "staff of the magi" => new Weapon(
                    "Staff of the Magi",
                    "The ultimate arcane focus.",
                    WeaponType.Staff,
                    minDamage: 26,
                    maxDamage: 52,
                    critChance: 20,
                    accuracy: 99,
                    ItemRarity.Legendary,
                    level,
                    value: 1700,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 3)
                ),

                "cosmos staff" => new Weapon(
                    "Cosmos Staff",
                    "Contains the power of creation.",
                    WeaponType.Staff,
                    minDamage: 27,
                    maxDamage: 54,
                    critChance: 21,
                    accuracy: 100,
                    ItemRarity.Legendary,
                    level,
                    value: 1800,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 3)
                ),

                "timekeeper's rod" => new Weapon(
                    "Timekeeper's Rod",
                    "Bends time and space to the wielder's will.",
                    WeaponType.Staff,
                    minDamage: 28,
                    maxDamage: 53,
                    critChance: 19,
                    accuracy: 98,
                    ItemRarity.Legendary,
                    level,
                    value: 1750,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 3)
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

                // Common Bows (continued)
                "short bow" => new Weapon(
                    "Short Bow",
                    "Compact bow for quick shots.",
                    WeaponType.Bow,
                    minDamage: 3,
                    maxDamage: 8,
                    critChance: 9,
                    accuracy: 73,
                    ItemRarity.Common,
                    level,
                    value: 35,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                "recurve bow" => new Weapon(
                    "Recurve Bow",
                    "Curved tips provide extra power.",
                    WeaponType.Bow,
                    minDamage: 5,
                    maxDamage: 10,
                    critChance: 11,
                    accuracy: 76,
                    ItemRarity.Common,
                    level,
                    value: 45,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                "composite bow" => new Weapon(
                    "Composite Bow",
                    "Multiple materials for strength.",
                    WeaponType.Bow,
                    minDamage: 5,
                    maxDamage: 11,
                    critChance: 10,
                    accuracy: 77,
                    ItemRarity.Common,
                    level,
                    value: 48,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                "flatbow" => new Weapon(
                    "Flatbow",
                    "Wide limbs distribute force evenly.",
                    WeaponType.Bow,
                    minDamage: 4,
                    maxDamage: 9,
                    critChance: 10,
                    accuracy: 74,
                    ItemRarity.Common,
                    level,
                    value: 42,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                "elm bow" => new Weapon(
                    "Elm Bow",
                    "Crafted from flexible elm wood.",
                    WeaponType.Bow,
                    minDamage: 4,
                    maxDamage: 10,
                    critChance: 11,
                    accuracy: 75,
                    ItemRarity.Common,
                    level,
                    value: 43,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                "yew bow" => new Weapon(
                    "Yew Bow",
                    "Traditional bow made from yew wood.",
                    WeaponType.Bow,
                    minDamage: 5,
                    maxDamage: 11,
                    critChance: 12,
                    accuracy: 78,
                    ItemRarity.Common,
                    level,
                    value: 50,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                "horse bow" => new Weapon(
                    "Horse Bow",
                    "Designed for mounted archery.",
                    WeaponType.Bow,
                    minDamage: 4,
                    maxDamage: 9,
                    critChance: 10,
                    accuracy: 76,
                    ItemRarity.Common,
                    level,
                    value: 44,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                // Uncommon Bows (continued)
                "war bow" => new Weapon(
                    "War Bow",
                    "Heavy military bow.",
                    WeaponType.Bow,
                    minDamage: 8,
                    maxDamage: 16,
                    critChance: 13,
                    accuracy: 79,
                    ItemRarity.Uncommon,
                    level,
                    value: 175,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                "elven bow" => new Weapon(
                    "Elven Bow",
                    "Elegant bow crafted by elven artisans.",
                    WeaponType.Bow,
                    minDamage: 10,
                    maxDamage: 18,
                    critChance: 14,
                    accuracy: 82,
                    ItemRarity.Uncommon,
                    level,
                    value: 190,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                "greatbow" => new Weapon(
                    "Greatbow",
                    "Massive bow requiring great strength.",
                    WeaponType.Bow,
                    minDamage: 11,
                    maxDamage: 19,
                    critChance: 13,
                    accuracy: 78,
                    ItemRarity.Uncommon,
                    level,
                    value: 185,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                "horn bow" => new Weapon(
                    "Horn Bow",
                    "Reinforced with horn for power.",
                    WeaponType.Bow,
                    minDamage: 9,
                    maxDamage: 17,
                    critChance: 12,
                    accuracy: 81,
                    ItemRarity.Uncommon,
                    level,
                    value: 178,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                // Rare Bows
                "dragonbone bow" => new Weapon(
                    "Dragonbone Bow",
                    "Forged from the bones of a dragon.",
                    WeaponType.Bow,
                    minDamage: 14,
                    maxDamage: 30,
                    critChance: 18,
                    accuracy: 84,
                    ItemRarity.Rare,
                    level,
                    value: 420,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                "windcaller bow" => new Weapon(
                    "Windcaller Bow",
                    "Arrows fly on the wings of wind.",
                    WeaponType.Bow,
                    minDamage: 15,
                    maxDamage: 32,
                    critChance: 19,
                    accuracy: 85,
                    ItemRarity.Rare,
                    level,
                    value: 440,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                "silver moon bow" => new Weapon(
                    "Silver Moon Bow",
                    "Blessed by moonlight.",
                    WeaponType.Bow,
                    minDamage: 13,
                    maxDamage: 29,
                    critChance: 20,
                    accuracy: 86,
                    ItemRarity.Rare,
                    level,
                    value: 430,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                "phoenix feather bow" => new Weapon(
                    "Phoenix Feather Bow",
                    "Strung with phoenix feathers.",
                    WeaponType.Bow,
                    minDamage: 16,
                    maxDamage: 31,
                    critChance: 17,
                    accuracy: 83,
                    ItemRarity.Rare,
                    level,
                    value: 410,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                // Epic Bows
                "artemis's rage" => new Weapon(
                    "Artemis's Rage",
                    "Divine bow of the hunt goddess.",
                    WeaponType.Bow,
                    minDamage: 22,
                    maxDamage: 48,
                    critChance: 25,
                    accuracy: 90,
                    ItemRarity.Epic,
                    level,
                    value: 1000,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                "starshard bow" => new Weapon(
                    "Starshard Bow",
                    "Fires arrows of pure starlight.",
                    WeaponType.Bow,
                    minDamage: 23,
                    maxDamage: 50,
                    critChance: 26,
                    accuracy: 91,
                    ItemRarity.Epic,
                    level,
                    value: 1050,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                "whisperwind" => new Weapon(
                    "Whisperwind",
                    "Silent as the wind, deadly as a storm.",
                    WeaponType.Bow,
                    minDamage: 24,
                    maxDamage: 49,
                    critChance: 24,
                    accuracy: 89,
                    ItemRarity.Epic,
                    level,
                    value: 1020,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                // Legendary Bows
                "yoichi's bow" => new Weapon(
                    "Yoichi's Bow",
                    "The legendary bow of the greatest archer.",
                    WeaponType.Bow,
                    minDamage: 28,
                    maxDamage: 62,
                    critChance: 30,
                    accuracy: 94,
                    ItemRarity.Legendary,
                    level,
                    value: 1700,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                "failnaught" => new Weapon(
                    "Failnaught",
                    "The unfailing bow, never misses its mark.",
                    WeaponType.Bow,
                    minDamage: 29,
                    maxDamage: 64,
                    critChance: 31,
                    accuracy: 95,
                    ItemRarity.Legendary,
                    level,
                    value: 1800,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                "gandiva" => new Weapon(
                    "Gandiva",
                    "Divine bow of the warrior god.",
                    WeaponType.Bow,
                    minDamage: 30,
                    maxDamage: 63,
                    critChance: 29,
                    accuracy: 93,
                    ItemRarity.Legendary,
                    level,
                    value: 1750,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
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

                // Common Crossbows (continued)
                "hand crossbow" => new Weapon(
                    "Hand Crossbow",
                    "Small one-handed crossbow.",
                    WeaponType.Crossbow,
                    minDamage: 5,
                    maxDamage: 10,
                    critChance: 4,
                    accuracy: 83,
                    ItemRarity.Common,
                    level,
                    value: 65,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                "pistol crossbow" => new Weapon(
                    "Pistol Crossbow",
                    "Compact and easy to reload.",
                    WeaponType.Crossbow,
                    minDamage: 6,
                    maxDamage: 11,
                    critChance: 5,
                    accuracy: 84,
                    ItemRarity.Common,
                    level,
                    value: 68,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                "hunting crossbow" => new Weapon(
                    "Hunting Crossbow",
                    "Designed for tracking game.",
                    WeaponType.Crossbow,
                    minDamage: 7,
                    maxDamage: 13,
                    critChance: 6,
                    accuracy: 86,
                    ItemRarity.Common,
                    level,
                    value: 75,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                "repeating crossbow" => new Weapon(
                    "Repeating Crossbow",
                    "Multiple bolt magazine for rapid fire.",
                    WeaponType.Crossbow,
                    minDamage: 5,
                    maxDamage: 11,
                    critChance: 5,
                    accuracy: 82,
                    ItemRarity.Common,
                    level,
                    value: 72,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                "arbalest" => new Weapon(
                    "Arbalest",
                    "Late medieval crossbow design.",
                    WeaponType.Crossbow,
                    minDamage: 7,
                    maxDamage: 14,
                    critChance: 6,
                    accuracy: 87,
                    ItemRarity.Common,
                    level,
                    value: 78,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                "steel crossbow" => new Weapon(
                    "Steel Crossbow",
                    "Reinforced with steel components.",
                    WeaponType.Crossbow,
                    minDamage: 8,
                    maxDamage: 13,
                    critChance: 5,
                    accuracy: 85,
                    ItemRarity.Common,
                    level,
                    value: 74,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                "chu-ko-nu" => new Weapon(
                    "Chu-Ko-Nu",
                    "Ancient Chinese repeating crossbow.",
                    WeaponType.Crossbow,
                    minDamage: 6,
                    maxDamage: 12,
                    critChance: 6,
                    accuracy: 83,
                    ItemRarity.Common,
                    level,
                    value: 70,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                // Uncommon Crossbows (continued)
                "siege crossbow" => new Weapon(
                    "Siege Crossbow",
                    "Built for penetrating armor.",
                    WeaponType.Crossbow,
                    minDamage: 11,
                    maxDamage: 21,
                    critChance: 8,
                    accuracy: 89,
                    ItemRarity.Uncommon,
                    level,
                    value: 210,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                "dwarven crossbow" => new Weapon(
                    "Dwarven Crossbow",
                    "Masterwork crossbow of dwarven craftsmanship.",
                    WeaponType.Crossbow,
                    minDamage: 12,
                    maxDamage: 22,
                    critChance: 9,
                    accuracy: 90,
                    ItemRarity.Uncommon,
                    level,
                    value: 220,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                "sniper crossbow" => new Weapon(
                    "Sniper Crossbow",
                    "Long-range precision weapon.",
                    WeaponType.Crossbow,
                    minDamage: 10,
                    maxDamage: 19,
                    critChance: 7,
                    accuracy: 92,
                    ItemRarity.Uncommon,
                    level,
                    value: 205,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                "windlass crossbow" => new Weapon(
                    "Windlass Crossbow",
                    "Mechanical crank for maximum draw strength.",
                    WeaponType.Crossbow,
                    minDamage: 11,
                    maxDamage: 20,
                    critChance: 8,
                    accuracy: 87,
                    ItemRarity.Uncommon,
                    level,
                    value: 195,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                // Rare Crossbows
                "dragonfire crossbow" => new Weapon(
                    "Dragonfire Crossbow",
                    "Bolts ignite with dragonfire.",
                    WeaponType.Crossbow,
                    minDamage: 16,
                    maxDamage: 34,
                    critChance: 12,
                    accuracy: 93,
                    ItemRarity.Rare,
                    level,
                    value: 490,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                "venomstrike" => new Weapon(
                    "Venomstrike",
                    "Coats bolts in deadly poison.",
                    WeaponType.Crossbow,
                    minDamage: 15,
                    maxDamage: 33,
                    critChance: 13,
                    accuracy: 94,
                    ItemRarity.Rare,
                    level,
                    value: 480,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                "adamant crossbow" => new Weapon(
                    "Adamant Crossbow",
                    "Unbreakable adamantine construction.",
                    WeaponType.Crossbow,
                    minDamage: 17,
                    maxDamage: 35,
                    critChance: 11,
                    accuracy: 91,
                    ItemRarity.Rare,
                    level,
                    value: 500,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                "thunderbolt" => new Weapon(
                    "Thunderbolt",
                    "Bolts crackle with electricity.",
                    WeaponType.Crossbow,
                    minDamage: 14,
                    maxDamage: 32,
                    critChance: 14,
                    accuracy: 95,
                    ItemRarity.Rare,
                    level,
                    value: 470,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                // Epic Crossbows
                "wyrm's bane" => new Weapon(
                    "Wyrm's Bane",
                    "Legendary dragon-slaying crossbow.",
                    WeaponType.Crossbow,
                    minDamage: 24,
                    maxDamage: 52,
                    critChance: 18,
                    accuracy: 97,
                    ItemRarity.Epic,
                    level,
                    value: 1100,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                "guillotine" => new Weapon(
                    "Guillotine",
                    "Execution-grade precision weapon.",
                    WeaponType.Crossbow,
                    minDamage: 25,
                    maxDamage: 54,
                    critChance: 19,
                    accuracy: 98,
                    ItemRarity.Epic,
                    level,
                    value: 1150,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                "shadowpiercer" => new Weapon(
                    "Shadowpiercer",
                    "Fires bolts through the veil of shadows.",
                    WeaponType.Crossbow,
                    minDamage: 23,
                    maxDamage: 51,
                    critChance: 20,
                    accuracy: 99,
                    ItemRarity.Epic,
                    level,
                    value: 1080,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                // Legendary Crossbows
                "hellsing" => new Weapon(
                    "Hellsing",
                    "The ultimate vampire-hunting weapon.",
                    WeaponType.Crossbow,
                    minDamage: 30,
                    maxDamage: 66,
                    critChance: 24,
                    accuracy: 100,
                    ItemRarity.Legendary,
                    level,
                    value: 1900,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                "van helsing's legacy" => new Weapon(
                    "Van Helsing's Legacy",
                    "Passed down through generations of hunters.",
                    WeaponType.Crossbow,
                    minDamage: 31,
                    maxDamage: 68,
                    critChance: 25,
                    accuracy: 100,
                    ItemRarity.Legendary,
                    level,
                    value: 2000,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
                ),

                "ballista prime" => new Weapon(
                    "Ballista Prime",
                    "Ancient siege weapon reforged as handheld terror.",
                    WeaponType.Crossbow,
                    minDamage: 32,
                    maxDamage: 67,
                    critChance: 23,
                    accuracy: 98,
                    ItemRarity.Legendary,
                    level,
                    value: 1950,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (0, 2),
                    maxDamageUpgradeRange: (1, 7)
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

                // Common Wands (continued)
                "maple wand" => new Weapon(
                    "Maple Wand",
                    "Simple wand carved from maple.",
                    WeaponType.Wand,
                    minDamage: 3,
                    maxDamage: 6,
                    critChance: 7,
                    accuracy: 80,
                    ItemRarity.Common,
                    level,
                    value: 45,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (2, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "cherry wand" => new Weapon(
                    "Cherry Wand",
                    "Wand made from cherry wood.",
                    WeaponType.Wand,
                    minDamage: 4,
                    maxDamage: 7,
                    critChance: 8,
                    accuracy: 81,
                    ItemRarity.Common,
                    level,
                    value: 48,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (2, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "willow wand" => new Weapon(
                    "Willow Wand",
                    "Flexible willow wood wand.",
                    WeaponType.Wand,
                    minDamage: 3,
                    maxDamage: 6,
                    critChance: 9,
                    accuracy: 83,
                    ItemRarity.Common,
                    level,
                    value: 52,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (2, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "ash wand" => new Weapon(
                    "Ash Wand",
                    "Sturdy wand from ash tree.",
                    WeaponType.Wand,
                    minDamage: 5,
                    maxDamage: 8,
                    critChance: 7,
                    accuracy: 79,
                    ItemRarity.Common,
                    level,
                    value: 46,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (2, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "rowan wand" => new Weapon(
                    "Rowan Wand",
                    "Protective rowan wood.",
                    WeaponType.Wand,
                    minDamage: 4,
                    maxDamage: 8,
                    critChance: 8,
                    accuracy: 82,
                    ItemRarity.Common,
                    level,
                    value: 51,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (2, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "hazel wand" => new Weapon(
                    "Hazel Wand",
                    "Wand of wisdom and divination.",
                    WeaponType.Wand,
                    minDamage: 5,
                    maxDamage: 7,
                    critChance: 9,
                    accuracy: 84,
                    ItemRarity.Common,
                    level,
                    value: 53,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (2, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "ebony wand" => new Weapon(
                    "Ebony Wand",
                    "Dark wood with potent magic.",
                    WeaponType.Wand,
                    minDamage: 4,
                    maxDamage: 7,
                    critChance: 10,
                    accuracy: 81,
                    ItemRarity.Common,
                    level,
                    value: 55,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (2, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                // Uncommon Wands (continued)
                "silver wand" => new Weapon(
                    "Silver Wand",
                    "Wand inlaid with silver filigree.",
                    WeaponType.Wand,
                    minDamage: 7,
                    maxDamage: 11,
                    critChance: 11,
                    accuracy: 86,
                    ItemRarity.Uncommon,
                    level,
                    value: 165,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (2, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "crystal wand" => new Weapon(
                    "Crystal Wand",
                    "Focuses magic through pure crystal.",
                    WeaponType.Wand,
                    minDamage: 9,
                    maxDamage: 13,
                    critChance: 12,
                    accuracy: 87,
                    ItemRarity.Uncommon,
                    level,
                    value: 175,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (2, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "ivory wand" => new Weapon(
                    "Ivory Wand",
                    "Carved from ancient ivory.",
                    WeaponType.Wand,
                    minDamage: 8,
                    maxDamage: 11,
                    critChance: 10,
                    accuracy: 85,
                    ItemRarity.Uncommon,
                    level,
                    value: 158,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (2, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "phoenix feather wand" => new Weapon(
                    "Phoenix Feather Wand",
                    "Contains a phoenix feather core.",
                    WeaponType.Wand,
                    minDamage: 7,
                    maxDamage: 12,
                    critChance: 13,
                    accuracy: 88,
                    ItemRarity.Uncommon,
                    level,
                    value: 180,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (2, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                // Rare Wands
                "mithril wand" => new Weapon(
                    "Mithril Wand",
                    "Lightweight and incredibly powerful.",
                    WeaponType.Wand,
                    minDamage: 12,
                    maxDamage: 18,
                    critChance: 16,
                    accuracy: 91,
                    ItemRarity.Rare,
                    level,
                    value: 390,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (2, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "dragon heartstring wand" => new Weapon(
                    "Dragon Heartstring Wand",
                    "Wand with a dragon heartstring core.",
                    WeaponType.Wand,
                    minDamage: 13,
                    maxDamage: 19,
                    critChance: 17,
                    accuracy: 92,
                    ItemRarity.Rare,
                    level,
                    value: 410,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (2, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "unicorn hair wand" => new Weapon(
                    "Unicorn Hair Wand",
                    "Pure and consistent magical output.",
                    WeaponType.Wand,
                    minDamage: 11,
                    maxDamage: 17,
                    critChance: 15,
                    accuracy: 94,
                    ItemRarity.Rare,
                    level,
                    value: 400,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (2, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "thunderwood wand" => new Weapon(
                    "Thunderwood Wand",
                    "Wood struck by lightning, channels electricity.",
                    WeaponType.Wand,
                    minDamage: 14,
                    maxDamage: 20,
                    critChance: 18,
                    accuracy: 90,
                    ItemRarity.Rare,
                    level,
                    value: 420,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (2, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                // Epic Wands
                "deathstick" => new Weapon(
                    "Deathstick",
                    "Wand of legend, unbeatable in duels.",
                    WeaponType.Wand,
                    minDamage: 20,
                    maxDamage: 30,
                    critChance: 24,
                    accuracy: 96,
                    ItemRarity.Epic,
                    level,
                    value: 900,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (2, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "celestial rod" => new Weapon(
                    "Celestial Rod",
                    "Blessed by the stars themselves.",
                    WeaponType.Wand,
                    minDamage: 21,
                    maxDamage: 32,
                    critChance: 25,
                    accuracy: 97,
                    ItemRarity.Epic,
                    level,
                    value: 950,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (2, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "void scepter" => new Weapon(
                    "Void Scepter",
                    "Draws power from the abyss.",
                    WeaponType.Wand,
                    minDamage: 22,
                    maxDamage: 31,
                    critChance: 23,
                    accuracy: 95,
                    ItemRarity.Epic,
                    level,
                    value: 920,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (2, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                // Legendary Wands
                "elder wand" => new Weapon(
                    "Elder Wand",
                    "The most powerful wand ever created.",
                    WeaponType.Wand,
                    minDamage: 28,
                    maxDamage: 42,
                    critChance: 30,
                    accuracy: 99,
                    ItemRarity.Legendary,
                    level,
                    value: 1650,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (2, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "reality warp wand" => new Weapon(
                    "Reality Warp Wand",
                    "Bends reality to the caster's will.",
                    WeaponType.Wand,
                    minDamage: 29,
                    maxDamage: 44,
                    critChance: 31,
                    accuracy: 100,
                    ItemRarity.Legendary,
                    level,
                    value: 1750,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (2, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "infinity focus" => new Weapon(
                    "Infinity Focus",
                    "Contains infinite magical potential.",
                    WeaponType.Wand,
                    minDamage: 30,
                    maxDamage: 43,
                    critChance: 29,
                    accuracy: 98,
                    ItemRarity.Legendary,
                    level,
                    value: 1700,
                    minDamageUpgradeRange: (1, 3),
                    damageShiftRange: (2, 1),
                    maxDamageUpgradeRange: (0, 2)
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

                // Common Daggers (continued)
                "shiv" => new Weapon(
                    "Shiv",
                    "Crude improvised blade.",
                    WeaponType.Dagger,
                    minDamage: 1,
                    maxDamage: 4,
                    critChance: 14,
                    accuracy: 78,
                    ItemRarity.Common,
                    level,
                    value: 5,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "stiletto" => new Weapon(
                    "Stiletto",
                    "Thin, precise piercing dagger.",
                    WeaponType.Dagger,
                    minDamage: 3,
                    maxDamage: 6,
                    critChance: 17,
                    accuracy: 82,
                    ItemRarity.Common,
                    level,
                    value: 12,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "dirk" => new Weapon(
                    "Dirk",
                    "Long dagger for stabbing.",
                    WeaponType.Dagger,
                    minDamage: 2,
                    maxDamage: 5,
                    critChance: 16,
                    accuracy: 81,
                    ItemRarity.Common,
                    level,
                    value: 10,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "kukri" => new Weapon(
                    "Kukri",
                    "Curved Nepalese knife.",
                    WeaponType.Dagger,
                    minDamage: 3,
                    maxDamage: 7,
                    critChance: 18,
                    accuracy: 79,
                    ItemRarity.Common,
                    level,
                    value: 14,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "tanto" => new Weapon(
                    "Tanto",
                    "Japanese short blade.",
                    WeaponType.Dagger,
                    minDamage: 2,
                    maxDamage: 6,
                    critChance: 16,
                    accuracy: 83,
                    ItemRarity.Common,
                    level,
                    value: 11,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "knife" => new Weapon(
                    "Knife",
                    "Simple utility knife.",
                    WeaponType.Dagger,
                    minDamage: 2,
                    maxDamage: 4,
                    critChance: 15,
                    accuracy: 80,
                    ItemRarity.Common,
                    level,
                    value: 7,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "parrying dagger" => new Weapon(
                    "Parrying Dagger",
                    "Dagger designed for defense.",
                    WeaponType.Dagger,
                    minDamage: 3,
                    maxDamage: 5,
                    critChance: 14,
                    accuracy: 84,
                    ItemRarity.Common,
                    level,
                    value: 13,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                // Uncommon Daggers
                "rondel dagger" => new Weapon(
                    "Rondel Dagger",
                    "Armor-piercing dagger.",
                    WeaponType.Dagger,
                    minDamage: 5,
                    maxDamage: 10,
                    critChance: 20,
                    accuracy: 84,
                    ItemRarity.Uncommon,
                    level,
                    value: 130,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "main gauche" => new Weapon(
                    "Main Gauche",
                    "Left-hand fighting dagger.",
                    WeaponType.Dagger,
                    minDamage: 6,
                    maxDamage: 11,
                    critChance: 21,
                    accuracy: 86,
                    ItemRarity.Uncommon,
                    level,
                    value: 140,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "kris" => new Weapon(
                    "Kris",
                    "Wavy-bladed Indonesian dagger.",
                    WeaponType.Dagger,
                    minDamage: 5,
                    maxDamage: 9,
                    critChance: 22,
                    accuracy: 83,
                    ItemRarity.Uncommon,
                    level,
                    value: 135,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "jambiya" => new Weapon(
                    "Jambiya",
                    "Curved Arabian dagger.",
                    WeaponType.Dagger,
                    minDamage: 4,
                    maxDamage: 10,
                    critChance: 19,
                    accuracy: 82,
                    ItemRarity.Uncommon,
                    level,
                    value: 125,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "sai" => new Weapon(
                    "Sai",
                    "Three-pronged defensive dagger.",
                    WeaponType.Dagger,
                    minDamage: 6,
                    maxDamage: 12,
                    critChance: 20,
                    accuracy: 85,
                    ItemRarity.Uncommon,
                    level,
                    value: 145,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                // Rare Daggers (continued)
                "shadowstrike" => new Weapon(
                    "Shadowstrike",
                    "Dagger that strikes from darkness.",
                    WeaponType.Dagger,
                    minDamage: 9,
                    maxDamage: 18,
                    critChance: 28,
                    accuracy: 88,
                    ItemRarity.Rare,
                    level,
                    value: 280,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "soul drinker" => new Weapon(
                    "Soul Drinker",
                    "Drains life force with each strike.",
                    WeaponType.Dagger,
                    minDamage: 8,
                    maxDamage: 16,
                    critChance: 30,
                    accuracy: 87,
                    ItemRarity.Rare,
                    level,
                    value: 270,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "fang of the serpent" => new Weapon(
                    "Fang of the Serpent",
                    "Poisoned dagger shaped like a fang.",
                    WeaponType.Dagger,
                    minDamage: 10,
                    maxDamage: 19,
                    critChance: 29,
                    accuracy: 86,
                    ItemRarity.Rare,
                    level,
                    value: 290,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                // Epic Daggers
                "misericorde" => new Weapon(
                    "Misericorde",
                    "The mercy dagger, delivers final blows.",
                    WeaponType.Dagger,
                    minDamage: 15,
                    maxDamage: 30,
                    critChance: 38,
                    accuracy: 90,
                    ItemRarity.Epic,
                    level,
                    value: 850,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "nightbringer" => new Weapon(
                    "Nightbringer",
                    "Blade forged in eternal darkness.",
                    WeaponType.Dagger,
                    minDamage: 16,
                    maxDamage: 32,
                    critChance: 39,
                    accuracy: 91,
                    ItemRarity.Epic,
                    level,
                    value: 900,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "widow's kiss" => new Weapon(
                    "Widow's Kiss",
                    "One cut is all it takes.",
                    WeaponType.Dagger,
                    minDamage: 17,
                    maxDamage: 31,
                    critChance: 37,
                    accuracy: 89,
                    ItemRarity.Epic,
                    level,
                    value: 870,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                // Legendary Daggers
                "carnwennan" => new Weapon(
                    "Carnwennan",
                    "King Arthur's dagger of shadows.",
                    WeaponType.Dagger,
                    minDamage: 22,
                    maxDamage: 44,
                    critChance: 45,
                    accuracy: 93,
                    ItemRarity.Legendary,
                    level,
                    value: 1600,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "loki's needle" => new Weapon(
                    "Loki's Needle",
                    "The trickster god's favorite tool.",
                    WeaponType.Dagger,
                    minDamage: 23,
                    maxDamage: 46,
                    critChance: 46,
                    accuracy: 94,
                    ItemRarity.Legendary,
                    level,
                    value: 1700,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
                ),

                "bloodletter" => new Weapon(
                    "Bloodletter",
                    "Ancient blade that never stops bleeding.",
                    WeaponType.Dagger,
                    minDamage: 24,
                    maxDamage: 45,
                    critChance: 44,
                    accuracy: 92,
                    ItemRarity.Legendary,
                    level,
                    value: 1650,
                    minDamageUpgradeRange: (0, 1),
                    damageShiftRange: (1, 1),
                    maxDamageUpgradeRange: (0, 2)
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

                // CHAMPION BOSS KEYS (15 unique keys, one per boss)
                // These keys are required to unlock the Final Gate (need 10 of 15)

                "flame_warden_key" => new QuestItem(
                    "Eternal Flame Key",
                    "A key forged from the heart of the Flame Warden. Burns with eternal fire and radiates immense heat. Can unlock flame-sealed doors and counts toward the Final Gate.",
                    "flame_warden_key",
                    isKeyItem: true
                ),

                "frost_tyrant_key" => new QuestItem(
                    "Frozen Heart Key",
                    "Carved from the frozen core of the Frost Tyrant. Ice-cold to the touch, yet never melts. Unlocks frost-sealed passages and counts toward the Final Gate.",
                    "frost_tyrant_key",
                    isKeyItem: true
                ),

                "thunder_lord_key" => new QuestItem(
                    "Storm Core Key",
                    "Crystallized essence of the Thunder Lord's power. Crackles with electricity and emits a faint hum. Opens storm barriers and counts toward the Final Gate.",
                    "thunder_lord_key",
                    isKeyItem: true
                ),

                "shadow_reaper_key" => new QuestItem(
                    "Void Shard Key",
                    "A fragment of pure shadow from the Shadow Reaper. Absorbs light around it and feels unnaturally cold. Pierces shadow seals and counts toward the Final Gate.",
                    "shadow_reaper_key",
                    isKeyItem: true
                ),

                "stone_guardian_key" => new QuestItem(
                    "Earth Titan Key",
                    "Carved from the heart-stone of the ancient Stone Guardian. Impossibly heavy yet perfectly balanced. Breaks through stone barriers and counts toward the Final Gate.",
                    "stone_guardian_key",
                    isKeyItem: true
                ),

                "serpent_queen_key" => new QuestItem(
                    "Venom Fang Key",
                    "Crafted from the Serpent Queen's fang, dripping with eternal venom. The poison never fades. Dissolves venom-locked gates and counts toward the Final Gate.",
                    "serpent_queen_key",
                    isKeyItem: true
                ),

                "iron_colossus_key" => new QuestItem(
                    "Forged Iron Key",
                    "Molded from the indestructible plating of the Iron Colossus. Unbreakable and perfectly machined. Opens reinforced doors and counts toward the Final Gate.",
                    "iron_colossus_key",
                    isKeyItem: true
                ),

                "blood_knight_key" => new QuestItem(
                    "Crimson Soul Key",
                    "Crystallized from the Blood Knight's essence. Pulses with a faint heartbeat. Unlocks blood-sealed chambers and counts toward the Final Gate.",
                    "blood_knight_key",
                    isKeyItem: true
                ),

                "arcane_archon_key" => new QuestItem(
                    "Prismatic Arcane Key",
                    "Pure condensed magic from the Arcane Archon. Shifts through rainbow colors. Dispels magical wards and counts toward the Final Gate.",
                    "arcane_archon_key",
                    isKeyItem: true
                ),

                "plague_bearer_key" => new QuestItem(
                    "Pestilence Key",
                    "Extracted from the Plague Bearer's corrupted heart. Emits a sickly green glow. Cleaves through plague barriers and counts toward the Final Gate.",
                    "plague_bearer_key",
                    isKeyItem: true
                ),

                "sky_sovereign_key" => new QuestItem(
                    "Celestial Wing Key",
                    "A feather-shaped key from the Sky Sovereign's wings. Impossibly light and glows with starlight. Opens sky gates and counts toward the Final Gate.",
                    "sky_sovereign_key",
                    isKeyItem: true
                ),

                "abyssal_horror_key" => new QuestItem(
                    "Deep Abyss Key",
                    "Dredged from the depths by the Abyssal Horror. Dark water perpetually drips from it. Parts deep-sea seals and counts toward the Final Gate.",
                    "abyssal_horror_key",
                    isKeyItem: true
                ),

                "solar_phoenix_key" => new QuestItem(
                    "Phoenix Ember Key",
                    "Reborn from the Solar Phoenix's ashes. Burns eternally with holy fire. Ignites sacred locks and counts toward the Final Gate.",
                    "solar_phoenix_key",
                    isKeyItem: true
                ),

                "void_dragon_key" => new QuestItem(
                    "Void Scale Key",
                    "A scale from the Void Dragon that exists between dimensions. Phases in and out of reality. Transcends dimensional barriers and counts toward the Final Gate.",
                    "void_dragon_key",
                    isKeyItem: true
                ),

                "time_keeper_key" => new QuestItem(
                    "Eternal Hourglass Key",
                    "Stolen from the Time Keeper's collection. Sand flows upward within it. Unlocks temporal seals and counts toward the Final Gate.",
                    "time_keeper_key",
                    isKeyItem: true
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