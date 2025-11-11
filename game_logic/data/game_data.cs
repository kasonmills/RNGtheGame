using System.Collections.Generic;
using GameLogic.Items;
using GameLogic.Entities.Enemies;
using GameLogic.Abilities.PlayerAbilities;

namespace GameLogic.Data
{
    /// <summary>
    /// Central database for all game content
    /// Contains definitions for items, enemies, abilities, and configuration
    /// </summary>
    public static class GameDatabase
    {
        #region Weapons Database

        /// <summary>
        /// Get a weapon by name
        /// </summary>
        public static Weapon GetWeapon(string name)
        {
            return AllWeapons.Find(w => w.Name == name);
        }

        /// <summary>
        /// Get weapons by rarity
        /// </summary>
        public static List<Weapon> GetWeaponsByRarity(ItemRarity rarity)
        {
            return AllWeapons.FindAll(w => w.Rarity == rarity);
        }

        /// <summary>
        /// All weapons in the game
        /// Format: Name, MinDamage, MaxDamage, Accuracy, CritChance, Type, Rarity
        /// </summary>
        public static List<Weapon> AllWeapons => new List<Weapon>
        {
            // Common Weapons (Starter gear)
            new Weapon("Rusty Sword", 1, 3, 65, 3, WeaponType.Sword, ItemRarity.Common),
            new Weapon("Worn Dagger", 1, 2, 70, 5, WeaponType.Dagger, ItemRarity.Common),
            new Weapon("Cracked Staff", 1, 4, 60, 2, WeaponType.Staff, ItemRarity.Common),
            new Weapon("Old Bow", 2, 4, 55, 4, WeaponType.Bow, ItemRarity.Common),

            // Uncommon Weapons (Mid-tier)
            new Weapon("Iron Sword", 4, 8, 70, 5, WeaponType.Sword, ItemRarity.Uncommon),
            new Weapon("Sharp Dagger", 3, 6, 75, 8, WeaponType.Dagger, ItemRarity.Uncommon),
            new Weapon("Oak Staff", 3, 9, 65, 4, WeaponType.Staff, ItemRarity.Uncommon),
            new Weapon("Longbow", 5, 9, 60, 6, WeaponType.Bow, ItemRarity.Uncommon),

            // Rare Weapons (High-tier)
            new Weapon("Steel Greatsword", 8, 15, 75, 7, WeaponType.Sword, ItemRarity.Rare),
            new Weapon("Assassin's Blade", 6, 12, 80, 12, WeaponType.Dagger, ItemRarity.Rare),
            new Weapon("Arcane Staff", 7, 16, 70, 6, WeaponType.Staff, ItemRarity.Rare),
            new Weapon("Composite Bow", 9, 14, 65, 9, WeaponType.Bow, ItemRarity.Rare),

            // Epic Weapons (Exceptional gear)
            new Weapon("Flamebrand Sword", 12, 22, 80, 10, WeaponType.Sword, ItemRarity.Epic),
            new Weapon("Viper's Fang", 10, 18, 85, 15, WeaponType.Dagger, ItemRarity.Epic),
            new Weapon("Staff of the Archmage", 11, 24, 75, 8, WeaponType.Staff, ItemRarity.Epic),
            new Weapon("Elven Warbow", 14, 20, 70, 12, WeaponType.Bow, ItemRarity.Epic),

            // Legendary Weapons (Best in slot)
            new Weapon("Excalibur", 20, 35, 85, 15, WeaponType.Sword, ItemRarity.Legendary),
            new Weapon("Nightbringer", 15, 28, 90, 20, WeaponType.Dagger, ItemRarity.Legendary),
            new Weapon("Staff of Eternity", 18, 38, 80, 12, WeaponType.Staff, ItemRarity.Legendary),
            new Weapon("Celestial Bow", 22, 32, 75, 18, WeaponType.Bow, ItemRarity.Legendary),
        };

        #endregion

        #region Armor Database

        /// <summary>
        /// Get armor by name
        /// </summary>
        public static Armor GetArmor(string name)
        {
            return AllArmor.Find(a => a.Name == name);
        }

        /// <summary>
        /// Get armor by rarity
        /// </summary>
        public static List<Armor> GetArmorByRarity(ItemRarity rarity)
        {
            return AllArmor.FindAll(a => a.Rarity == rarity);
        }

        /// <summary>
        /// All armor in the game
        /// Format: Name, Defense, Type, Rarity
        /// </summary>
        public static List<Armor> AllArmor => new List<Armor>
        {
            // Common Armor (Starter gear)
            new Armor("Tattered Cloth", 1, ArmorType.Light, ItemRarity.Common),
            new Armor("Worn Leather", 2, ArmorType.Medium, ItemRarity.Common),
            new Armor("Rusty Chainmail", 3, ArmorType.Heavy, ItemRarity.Common),

            // Uncommon Armor (Mid-tier)
            new Armor("Linen Robes", 3, ArmorType.Light, ItemRarity.Uncommon),
            new Armor("Leather Armor", 5, ArmorType.Medium, ItemRarity.Uncommon),
            new Armor("Iron Chainmail", 7, ArmorType.Heavy, ItemRarity.Uncommon),

            // Rare Armor (High-tier)
            new Armor("Silk Robes", 6, ArmorType.Light, ItemRarity.Rare),
            new Armor("Reinforced Leather", 9, ArmorType.Medium, ItemRarity.Rare),
            new Armor("Steel Plate", 12, ArmorType.Heavy, ItemRarity.Rare),

            // Epic Armor (Exceptional gear)
            new Armor("Archmage Robes", 10, ArmorType.Light, ItemRarity.Epic),
            new Armor("Dragon Scale Armor", 14, ArmorType.Medium, ItemRarity.Epic),
            new Armor("Mythril Plate", 18, ArmorType.Heavy, ItemRarity.Epic),

            // Legendary Armor (Best in slot)
            new Armor("Robes of the Eternal", 15, ArmorType.Light, ItemRarity.Legendary),
            new Armor("Shadow Stalker Armor", 20, ArmorType.Medium, ItemRarity.Legendary),
            new Armor("Titan's Bulwark", 25, ArmorType.Heavy, ItemRarity.Legendary),
        };

        #endregion

        #region Enemy Factory Methods

        /// <summary>
        /// Create an enemy by type name and level
        /// </summary>
        public static Enemy CreateEnemy(string enemyType, int level)
        {
            return enemyType.ToLower() switch
            {
                "goblin" => new Goblin(level),
                "dragon" => new Dragon(level),
                "bandit" => new Bandit(level),
                _ => new Goblin(level) // Default to goblin
            };
        }

        /// <summary>
        /// Get a random enemy for the given level range
        /// Uses RNG for selection
        /// </summary>
        public static string GetRandomEnemyType(int playerLevel, Systems.RNGManager rng)
        {
            // Basic enemies for low levels
            if (playerLevel < 10)
            {
                return "goblin";
            }
            // Mix of enemies for mid levels
            else if (playerLevel < 50)
            {
                int roll = rng.Roll(1, 100);
                if (roll <= 60) return "goblin";
                else return "bandit";
            }
            // All enemy types for high levels
            else
            {
                int roll = rng.Roll(1, 100);
                if (roll <= 40) return "goblin";
                else if (roll <= 70) return "bandit";
                else return "dragon";
            }
        }

        #endregion

        #region Player Ability Definitions

        /// <summary>
        /// Create a new instance of a player ability by name
        /// </summary>
        public static Abilities.Ability CreatePlayerAbility(string abilityName)
        {
            return abilityName.ToLower() switch
            {
                "heal" => new HealingAbility(),
                "attack boost" => new AttackBoost(),
                "defense boost" => new DefenseBoost(),
                "critical strike" => new CriticalStrike(),
                _ => null
            };
        }

        /// <summary>
        /// Get list of all available player abilities
        /// </summary>
        public static List<string> GetAllPlayerAbilityNames()
        {
            return new List<string>
            {
                "Heal",
                "Attack Boost",
                "Defense Boost",
                "Critical Strike"
            };
        }

        #endregion

        #region Game Balance Configuration

        /// <summary>
        /// Configuration values for game balance
        /// </summary>
        public static class Config
        {
            // Experience scaling
            public const int BaseXPPerLevel = 100;
            public const double XPScalingFactor = 1.5;

            // Combat configuration
            public const int BaseCritMultiplier = 150; // 150% damage (1.5x)
            public const int MinDamage = 1; // Minimum damage after all reductions

            // Loot drop chances (out of 100)
            public const int CommonDropChance = 60;
            public const int UncommonDropChance = 25;
            public const int RareDropChance = 10;
            public const int EpicDropChance = 4;
            public const int LegendaryDropChance = 1;

            // Gold rewards
            public const int BaseGoldReward = 10;
            public const double GoldScalingFactor = 1.2;

            // Player starting stats
            public const int PlayerStartingHP = 100;
            public const int PlayerStartingLevel = 1;

            // Difficulty multipliers (for future difficulty slider)
            public const double EasyDifficultyMultiplier = 0.75;
            public const double NormalDifficultyMultiplier = 1.0;
            public const double HardDifficultyMultiplier = 1.5;
            public const double InsaneDifficultyMultiplier = 2.0;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get a random weapon drop based on player level
        /// </summary>
        public static Weapon GetRandomWeaponDrop(int playerLevel, Systems.RNGManager rng)
        {
            ItemRarity rarity = RollItemRarity(rng);
            List<Weapon> possibleWeapons = GetWeaponsByRarity(rarity);

            if (possibleWeapons.Count == 0)
                return AllWeapons[0]; // Fallback to first weapon

            int index = rng.Roll(0, possibleWeapons.Count - 1);
            return possibleWeapons[index];
        }

        /// <summary>
        /// Get a random armor drop based on player level
        /// </summary>
        public static Armor GetRandomArmorDrop(int playerLevel, Systems.RNGManager rng)
        {
            ItemRarity rarity = RollItemRarity(rng);
            List<Armor> possibleArmor = GetArmorByRarity(rarity);

            if (possibleArmor.Count == 0)
                return AllArmor[0]; // Fallback to first armor

            int index = rng.Roll(0, possibleArmor.Count - 1);
            return possibleArmor[index];
        }

        /// <summary>
        /// Roll for item rarity based on drop chances
        /// </summary>
        private static ItemRarity RollItemRarity(Systems.RNGManager rng)
        {
            int roll = rng.Roll(1, 100);

            if (roll <= Config.LegendaryDropChance)
                return ItemRarity.Legendary;
            else if (roll <= Config.LegendaryDropChance + Config.EpicDropChance)
                return ItemRarity.Epic;
            else if (roll <= Config.LegendaryDropChance + Config.EpicDropChance + Config.RareDropChance)
                return ItemRarity.Rare;
            else if (roll <= Config.LegendaryDropChance + Config.EpicDropChance + Config.RareDropChance + Config.UncommonDropChance)
                return ItemRarity.Uncommon;
            else
                return ItemRarity.Common;
        }

        #endregion
    }
}
