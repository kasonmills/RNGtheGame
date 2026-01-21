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
        Wand
    }

    /// <summary>
    /// Upgrade path chosen when leveling up a weapon
    /// </summary>
    public enum WeaponUpgradePath
    {
        IncreaseMinDamage,  // Increases minimum damage by 1-2 (narrows damage range, more consistent)
        ShiftDamageRange,   // Shifts entire range up by 1 (both min and max increase)
        IncreaseMaxDamage,  // Increases maximum damage by 1-3 (widens damage range, higher ceiling)
        Balanced            // Balanced upgrade (used as default)
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

        // Experience System
        public int Experience { get; set; }
        public int ExperienceToNextLevel { get; private set; }
        public bool ReadyForUpgrade { get; private set; }  // True when XP requirement is met

        // Base stats for level scaling
        private int _baseMinDamage;
        private int _baseMaxDamage;
        private int _baseCritChance;
        private int _baseAccuracy;

        // Upgrade ranges (min, max) for each upgrade path
        public (int min, int max) MinDamageUpgradeRange { get; set; }  // Range for IncreaseMinDamage path
        public (int minShift, int maxShift) DamageShiftRange { get; set; }  // How much min and max shift for ShiftDamageRange path
        public (int min, int max) MaxDamageUpgradeRange { get; set; }  // Range for IncreaseMaxDamage path

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
            int value = 0,
            (int, int)? minDamageUpgradeRange = null,
            (int, int)? damageShiftRange = null,
            (int, int)? maxDamageUpgradeRange = null
        ) : base(name, description, rarity, ItemCategory.Weapon, value)
        {
            Type = type;
            Level = Math.Clamp(level, 1, MaxLevel);

            // Store base stats
            _baseMinDamage = minDamage;
            _baseMaxDamage = maxDamage;
            _baseCritChance = critChance;
            _baseAccuracy = accuracy;

            // Set current stats to base (no automatic scaling)
            MinDamage = minDamage;
            MaxDamage = maxDamage;
            CritChance = critChance;
            Accuracy = accuracy;

            // Set upgrade ranges - must be explicitly provided by weapon definition
            if (!minDamageUpgradeRange.HasValue || !damageShiftRange.HasValue || !maxDamageUpgradeRange.HasValue)
            {
                throw new ArgumentException($"Weapon '{name}' must have all upgrade ranges defined (minDamageUpgradeRange, damageShiftRange, maxDamageUpgradeRange)");
            }

            MinDamageUpgradeRange = minDamageUpgradeRange.Value;
            DamageShiftRange = damageShiftRange.Value;
            MaxDamageUpgradeRange = maxDamageUpgradeRange.Value;

            // Initialize experience system
            Experience = 0;
            ExperienceToNextLevel = CalculateExperienceRequired(Level);
            ReadyForUpgrade = false;

            // Weapons are stackable if they have the same level
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
        /// Get base XP requirement based on weapon rarity
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
        /// Complete the upgrade with default balanced path
        /// </summary>
        public bool CompleteUpgrade()
        {
            return CompleteUpgrade(WeaponUpgradePath.Balanced, new Systems.RNGManager());
        }

        /// <summary>
        /// Complete the upgrade at the blacksmith with chosen path (requires gold payment)
        /// This should be called by the blacksmith NPC/shop after taking payment
        /// </summary>
        /// <param name="path">The upgrade path chosen by the player</param>
        /// <param name="rng">RNG manager for random upgrade values</param>
        public bool CompleteUpgrade(WeaponUpgradePath path, Systems.RNGManager rng)
        {
            if (!ReadyForUpgrade || Level >= MaxLevel)
            {
                return false;
            }

            Level++;
            Experience -= ExperienceToNextLevel;
            ExperienceToNextLevel = CalculateExperienceRequired(Level);
            ReadyForUpgrade = false;

            // Apply the chosen upgrade path
            ApplyUpgradePath(path, rng);

            Console.WriteLine($"⚒️  {GetDisplayName()} has been upgraded! Now at +{Level}!");
            return true;
        }

        /// <summary>
        /// Apply the chosen upgrade path to weapon stats
        /// Uses the weapon's defined upgrade ranges
        /// </summary>
        private void ApplyUpgradePath(WeaponUpgradePath path, Systems.RNGManager rng)
        {
            int oldMin = MinDamage;
            int oldMax = MaxDamage;

            switch (path)
            {
                case WeaponUpgradePath.IncreaseMinDamage:
                    // Increase min damage using weapon's MinDamageUpgradeRange
                    int minIncrease = rng.Roll(MinDamageUpgradeRange.min, MinDamageUpgradeRange.max);
                    MinDamage += minIncrease;

                    // Ensure min doesn't exceed max
                    if (MinDamage > MaxDamage)
                    {
                        MinDamage = MaxDamage;
                    }

                    Console.WriteLine($"  Min Damage: {oldMin} → {MinDamage} (+{minIncrease})");
                    Console.WriteLine($"  Damage Range: {oldMin}-{oldMax} → {MinDamage}-{MaxDamage}");
                    Console.WriteLine($"  ✓ More consistent damage!");
                    break;

                case WeaponUpgradePath.ShiftDamageRange:
                    // Shift range using weapon's DamageShiftRange
                    MinDamage += DamageShiftRange.minShift;
                    MaxDamage += DamageShiftRange.maxShift;

                    // Ensure min doesn't exceed max (important for weapons like wands with +2 min/+1 max shift)
                    if (MinDamage > MaxDamage)
                    {
                        MinDamage = MaxDamage;
                    }

                    Console.WriteLine($"  Damage Range: {oldMin}-{oldMax} → {MinDamage}-{MaxDamage}");
                    Console.WriteLine($"  ✓ Balanced upgrade!");
                    break;

                case WeaponUpgradePath.IncreaseMaxDamage:
                    // Increase max damage using weapon's MaxDamageUpgradeRange
                    int maxIncrease = rng.Roll(MaxDamageUpgradeRange.min, MaxDamageUpgradeRange.max);
                    MaxDamage += maxIncrease;

                    Console.WriteLine($"  Max Damage: {oldMax} → {MaxDamage} (+{maxIncrease})");
                    Console.WriteLine($"  Damage Range: {oldMin}-{oldMax} → {MinDamage}-{MaxDamage}");
                    Console.WriteLine($"  ✓ Higher damage potential!");
                    break;

                case WeaponUpgradePath.Balanced:
                    // Balanced upgrade - small increase to both
                    MinDamage += 1;
                    MaxDamage += 1;
                    Console.WriteLine($"  Damage Range: {oldMin}-{oldMax} → {MinDamage}-{MaxDamage}");
                    Console.WriteLine($"  ✓ Balanced improvement!");
                    break;
            }

            // Update base stats to match current stats (for any future calculations)
            _baseMinDamage = MinDamage;
            _baseMaxDamage = MaxDamage;
        }

        /// <summary>
        /// Legacy method - kept for backwards compatibility
        /// Use GainExperience() and CompleteUpgrade(path, rng) instead for the new system
        /// </summary>
        [Obsolete("Use GainExperience() and CompleteUpgrade(path, rng) instead")]
        public bool LevelUp()
        {
            if (Level >= MaxLevel)
            {
                return false;
            }

            Level++;
            Console.WriteLine($"{Name} leveled up to +{Level}! (Use CompleteUpgrade with path choice for stat increases)");
            return true;
        }

        /// <summary>
        /// Check if weapon can level up
        /// </summary>
        public bool CanLevelUp()
        {
            return Level < MaxLevel;
        }

        /// <summary>
        /// Display upgrade path options to the player
        /// Returns the chosen path, or null if cancelled
        /// </summary>
        public static WeaponUpgradePath? GetPlayerUpgradeChoice(Weapon weapon)
        {
            Console.WriteLine($"\n=== Upgrade {weapon.GetDisplayName()} ===");
            Console.WriteLine($"Current Stats: {weapon.MinDamage}-{weapon.MaxDamage} damage");
            Console.WriteLine($"\nChoose your upgrade path:");
            Console.WriteLine();

            // Option 1: Increase Minimum Damage
            var minRange = weapon.MinDamageUpgradeRange;
            string minRangeText = minRange.min == minRange.max ? $"+{minRange.min}" : $"+{minRange.min} to +{minRange.max}";
            int minResultLow = weapon.MinDamage + minRange.min;
            int minResultHigh = weapon.MinDamage + minRange.max;
            string minResultText = minRange.min == minRange.max ? $"{minResultLow}" : $"{minResultLow}~{minResultHigh}";

            Console.WriteLine($"1. Increase Minimum Damage ({minRangeText})");
            Console.WriteLine($"   {weapon.MinDamage}-{weapon.MaxDamage} → {minResultText}-{weapon.MaxDamage}");
            Console.WriteLine("   ✓ More consistent damage (narrows range)");
            Console.WriteLine();

            // Option 2: Shift Damage Range
            var shiftRange = weapon.DamageShiftRange;
            string shiftText = shiftRange.minShift == shiftRange.maxShift && shiftRange.minShift == 1
                ? "+1 to both"
                : $"+{shiftRange.minShift} min, +{shiftRange.maxShift} max";

            Console.WriteLine($"2. Shift Damage Range ({shiftText})");
            Console.WriteLine($"   {weapon.MinDamage}-{weapon.MaxDamage} → {weapon.MinDamage + shiftRange.minShift}-{weapon.MaxDamage + shiftRange.maxShift}");
            Console.WriteLine("   ✓ Balanced overall upgrade");
            Console.WriteLine();

            // Option 3: Increase Maximum Damage
            var maxRange = weapon.MaxDamageUpgradeRange;
            string maxRangeText = maxRange.min == maxRange.max ? $"+{maxRange.min}" : $"+{maxRange.min} to +{maxRange.max}";
            int maxResultLow = weapon.MaxDamage + maxRange.min;
            int maxResultHigh = weapon.MaxDamage + maxRange.max;
            string maxResultText = maxRange.min == maxRange.max ? $"{maxResultLow}" : $"{maxResultLow}~{maxResultHigh}";

            Console.WriteLine($"3. Increase Maximum Damage ({maxRangeText})");
            Console.WriteLine($"   {weapon.MinDamage}-{weapon.MaxDamage} → {weapon.MinDamage}-{maxResultText}");
            Console.WriteLine("   ✓ Higher damage ceiling (wider range)");
            Console.WriteLine();

            Console.WriteLine("4. Cancel");
            Console.WriteLine();

            Console.Write("Your choice (1-4): ");
            string input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    return WeaponUpgradePath.IncreaseMinDamage;
                case "2":
                    return WeaponUpgradePath.ShiftDamageRange;
                case "3":
                    return WeaponUpgradePath.IncreaseMaxDamage;
                case "4":
                default:
                    Console.WriteLine("Upgrade cancelled.");
                    return null;
            }
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