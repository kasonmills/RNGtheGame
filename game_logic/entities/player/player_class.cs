using System;
using GameLogic.Items;

namespace GameLogic.Entities.Player
{
    /// <summary>
    /// Player class types - chosen at character creation, permanent
    /// </summary>
    public enum PlayerClass
    {
        Warrior,    // Heavy armor tank
        Rogue,      // Light armor speed fighter
        Mage,       // Spellcaster with robes
        Ranger,     // Balanced leather armor user
        Paladin     // Holy warrior hybrid
    }

    /// <summary>
    /// Class-specific bonuses and armor proficiencies
    /// </summary>
    public static class PlayerClassInfo
    {
        /// <summary>
        /// Get the display name for a class
        /// </summary>
        public static string GetClassName(PlayerClass playerClass)
        {
            return playerClass switch
            {
                PlayerClass.Warrior => "Warrior",
                PlayerClass.Rogue => "Rogue",
                PlayerClass.Mage => "Mage",
                PlayerClass.Ranger => "Ranger",
                PlayerClass.Paladin => "Paladin",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Get the description for a class
        /// </summary>
        public static string GetClassDescription(PlayerClass playerClass)
        {
            return playerClass switch
            {
                PlayerClass.Warrior => "Heavy armor tank with high survivability. Excels in melee combat.",
                PlayerClass.Rogue => "Swift combatant who strikes from the shadows. High speed and evasion.",
                PlayerClass.Mage => "Powerful spellcaster with magical prowess. Fragile but devastating.",
                PlayerClass.Ranger => "Versatile hunter balancing speed and protection. Skilled marksman.",
                PlayerClass.Paladin => "Holy warrior combining armor and magic. Divine protector.",
                _ => "Unknown class"
            };
        }

        /// <summary>
        /// Get health bonus percentage for a class
        /// </summary>
        public static double GetHealthBonus(PlayerClass playerClass)
        {
            return playerClass switch
            {
                PlayerClass.Warrior => 0.20,  // +20% health
                PlayerClass.Rogue => 0.0,     // No bonus
                PlayerClass.Mage => 0.0,      // No bonus
                PlayerClass.Ranger => 0.10,   // +10% health
                PlayerClass.Paladin => 0.15,  // +15% health
                _ => 0.0
            };
        }

        /// <summary>
        /// Get defense bonus percentage for a class
        /// </summary>
        public static double GetDefenseBonus(PlayerClass playerClass)
        {
            return playerClass switch
            {
                PlayerClass.Warrior => 0.05,  // +5% defense
                PlayerClass.Rogue => 0.0,     // No bonus
                PlayerClass.Mage => 0.0,      // No bonus
                PlayerClass.Ranger => 0.0,    // No bonus
                PlayerClass.Paladin => 0.05,  // +5% defense
                _ => 0.0
            };
        }

        /// <summary>
        /// Get speed bonus percentage for a class
        /// </summary>
        public static double GetSpeedBonus(PlayerClass playerClass)
        {
            return playerClass switch
            {
                PlayerClass.Warrior => 0.0,   // No bonus
                PlayerClass.Rogue => 0.30,    // +30% speed
                PlayerClass.Mage => 0.10,     // +10% speed
                PlayerClass.Ranger => 0.15,   // +15% speed
                PlayerClass.Paladin => 0.0,   // No bonus
                _ => 0.0
            };
        }

        /// <summary>
        /// Get ability power bonus percentage for a class
        /// Only affects Mage and Paladin
        /// </summary>
        public static double GetAbilityPowerBonus(PlayerClass playerClass)
        {
            return playerClass switch
            {
                PlayerClass.Warrior => 0.0,   // No bonus
                PlayerClass.Rogue => 0.0,     // No bonus
                PlayerClass.Mage => 0.10,     // +10% ability power
                PlayerClass.Ranger => 0.0,    // No bonus
                PlayerClass.Paladin => 0.05,  // +5% ability power
                _ => 0.0
            };
        }

        /// <summary>
        /// Get accuracy bonus percentage for a class
        /// </summary>
        public static double GetAccuracyBonus(PlayerClass playerClass)
        {
            return playerClass switch
            {
                PlayerClass.Warrior => 0.0,   // No bonus
                PlayerClass.Rogue => 0.0,     // No bonus
                PlayerClass.Mage => 0.0,      // No bonus
                PlayerClass.Ranger => 0.05,   // +5% accuracy
                PlayerClass.Paladin => 0.0,   // No bonus
                _ => 0.0
            };
        }

        /// <summary>
        /// Calculate speed penalty for wearing specific armor type
        /// Returns penalty as negative percentage (e.g., -0.50 = -50% speed)
        /// </summary>
        public static double GetArmorSpeedPenalty(PlayerClass playerClass, ArmorType armorType)
        {
            return (playerClass, armorType) switch
            {
                // Warrior - no speed penalties for heavy armor
                (PlayerClass.Warrior, ArmorType.Plate) => 0.0,
                (PlayerClass.Warrior, ArmorType.Chainmail) => 0.0,
                (PlayerClass.Warrior, ArmorType.Leather) => 0.0,
                (PlayerClass.Warrior, ArmorType.Cloth) => 0.0,
                (PlayerClass.Warrior, ArmorType.Robe) => 0.0,
                (PlayerClass.Warrior, ArmorType.Shield) => 0.0,

                // Rogue - severe penalties for heavy armor
                (PlayerClass.Rogue, ArmorType.Plate) => -0.50,      // -50% speed
                (PlayerClass.Rogue, ArmorType.Chainmail) => -0.30,  // -30% speed
                (PlayerClass.Rogue, ArmorType.Leather) => 0.0,
                (PlayerClass.Rogue, ArmorType.Cloth) => 0.0,
                (PlayerClass.Rogue, ArmorType.Robe) => -0.05,       // -5% speed
                (PlayerClass.Rogue, ArmorType.Shield) => -0.20,     // -20% speed

                // Mage - speed penalties for metal armor
                (PlayerClass.Mage, ArmorType.Plate) => -0.40,       // -40% speed
                (PlayerClass.Mage, ArmorType.Chainmail) => -0.25,   // -25% speed
                (PlayerClass.Mage, ArmorType.Leather) => 0.0,
                (PlayerClass.Mage, ArmorType.Cloth) => 0.0,
                (PlayerClass.Mage, ArmorType.Robe) => 0.0,
                (PlayerClass.Mage, ArmorType.Shield) => 0.0,

                // Ranger - penalties for heavy armor
                (PlayerClass.Ranger, ArmorType.Plate) => -0.30,     // -30% speed
                (PlayerClass.Ranger, ArmorType.Chainmail) => -0.15, // -15% speed
                (PlayerClass.Ranger, ArmorType.Leather) => 0.0,
                (PlayerClass.Ranger, ArmorType.Cloth) => 0.0,
                (PlayerClass.Ranger, ArmorType.Robe) => 0.0,
                (PlayerClass.Ranger, ArmorType.Shield) => 0.0,

                // Paladin - minimal penalties, can wear most armor
                (PlayerClass.Paladin, ArmorType.Plate) => 0.0,
                (PlayerClass.Paladin, ArmorType.Chainmail) => 0.0,
                (PlayerClass.Paladin, ArmorType.Leather) => 0.0,
                (PlayerClass.Paladin, ArmorType.Cloth) => 0.0,
                (PlayerClass.Paladin, ArmorType.Robe) => 0.0,
                (PlayerClass.Paladin, ArmorType.Shield) => 0.0,

                _ => 0.0
            };
        }

        /// <summary>
        /// Calculate defense penalty/bonus for wearing specific armor type
        /// Returns penalty as negative percentage or bonus as positive
        /// </summary>
        public static double GetArmorDefensePenalty(PlayerClass playerClass, ArmorType armorType)
        {
            return (playerClass, armorType) switch
            {
                // Warrior - penalties for light armor, bonus for shield
                (PlayerClass.Warrior, ArmorType.Plate) => 0.0,
                (PlayerClass.Warrior, ArmorType.Chainmail) => 0.0,
                (PlayerClass.Warrior, ArmorType.Leather) => -0.10,   // -10% defense
                (PlayerClass.Warrior, ArmorType.Cloth) => -0.15,     // -15% defense
                (PlayerClass.Warrior, ArmorType.Robe) => -0.20,      // -20% defense
                (PlayerClass.Warrior, ArmorType.Shield) => 0.10,     // +10% defense

                // Rogue - no defense penalties (relies on evasion)
                (PlayerClass.Rogue, ArmorType.Plate) => 0.0,
                (PlayerClass.Rogue, ArmorType.Chainmail) => 0.0,
                (PlayerClass.Rogue, ArmorType.Leather) => 0.0,
                (PlayerClass.Rogue, ArmorType.Cloth) => 0.0,
                (PlayerClass.Rogue, ArmorType.Robe) => 0.0,
                (PlayerClass.Rogue, ArmorType.Shield) => 0.0,

                // Mage - no defense penalties
                (PlayerClass.Mage, ArmorType.Plate) => 0.0,
                (PlayerClass.Mage, ArmorType.Chainmail) => 0.0,
                (PlayerClass.Mage, ArmorType.Leather) => 0.0,
                (PlayerClass.Mage, ArmorType.Cloth) => 0.0,
                (PlayerClass.Mage, ArmorType.Robe) => 0.0,
                (PlayerClass.Mage, ArmorType.Shield) => 0.0,

                // Ranger - penalties for light/magical armor
                (PlayerClass.Ranger, ArmorType.Plate) => 0.0,
                (PlayerClass.Ranger, ArmorType.Chainmail) => 0.0,
                (PlayerClass.Ranger, ArmorType.Leather) => 0.0,
                (PlayerClass.Ranger, ArmorType.Cloth) => -0.05,      // -5% defense
                (PlayerClass.Ranger, ArmorType.Robe) => -0.10,       // -10% defense
                (PlayerClass.Ranger, ArmorType.Shield) => 0.0,

                // Paladin - penalties for light armor, bonus for shield/plate
                (PlayerClass.Paladin, ArmorType.Plate) => 0.0,
                (PlayerClass.Paladin, ArmorType.Chainmail) => 0.0,
                (PlayerClass.Paladin, ArmorType.Leather) => -0.10,   // -10% defense
                (PlayerClass.Paladin, ArmorType.Cloth) => -0.15,     // -15% defense
                (PlayerClass.Paladin, ArmorType.Robe) => -0.10,      // -10% defense (but gives ability bonus)
                (PlayerClass.Paladin, ArmorType.Shield) => 0.05,     // +5% defense

                _ => 0.0
            };
        }

        /// <summary>
        /// Calculate ability power penalty/bonus for wearing specific armor type
        /// Only applies to Mage and Paladin
        /// </summary>
        public static double GetArmorAbilityPowerPenalty(PlayerClass playerClass, ArmorType armorType)
        {
            return (playerClass, armorType) switch
            {
                // Warrior - no ability power
                (PlayerClass.Warrior, _) => 0.0,

                // Rogue - no ability power
                (PlayerClass.Rogue, _) => 0.0,

                // Mage - severe penalties for metal armor, bonus for robes
                (PlayerClass.Mage, ArmorType.Plate) => -0.20,        // -20% ability power
                (PlayerClass.Mage, ArmorType.Chainmail) => -0.15,    // -15% ability power
                (PlayerClass.Mage, ArmorType.Leather) => -0.10,      // -10% ability power
                (PlayerClass.Mage, ArmorType.Cloth) => 0.0,
                (PlayerClass.Mage, ArmorType.Robe) => 0.10,          // +10% ability power
                (PlayerClass.Mage, ArmorType.Shield) => -0.10,       // -10% ability power

                // Ranger - no ability power
                (PlayerClass.Ranger, _) => 0.0,

                // Paladin - robes give bonus, no penalties
                (PlayerClass.Paladin, ArmorType.Plate) => 0.0,
                (PlayerClass.Paladin, ArmorType.Chainmail) => 0.0,
                (PlayerClass.Paladin, ArmorType.Leather) => 0.0,
                (PlayerClass.Paladin, ArmorType.Cloth) => 0.0,
                (PlayerClass.Paladin, ArmorType.Robe) => 0.05,       // +5% ability power
                (PlayerClass.Paladin, ArmorType.Shield) => 0.0,

                _ => 0.0
            };
        }

        /// <summary>
        /// Calculate accuracy penalty for wearing specific armor type
        /// Only applies to Ranger with shields
        /// </summary>
        public static double GetArmorAccuracyPenalty(PlayerClass playerClass, ArmorType armorType)
        {
            return (playerClass, armorType) switch
            {
                // Ranger - shield interferes with ranged weapons
                (PlayerClass.Ranger, ArmorType.Shield) => -0.15,     // -15% accuracy

                // All other combinations have no accuracy penalty
                _ => 0.0
            };
        }
    }
}