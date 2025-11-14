using System;

namespace GameLogic.Items
{
    /// <summary>
    /// Type of consumable effect
    /// </summary>
    public enum ConsumableType
    {
        HealthPotion,       // Restores health
        ManaPotion,         // Restores mana (if mana system exists)
        BuffPotion,         // Applies temporary buff
        Food,               // Restores health over time
        Elixir,             // Powerful multi-effect consumables
        Bomb,               // Throwable damage items
        Antidote,           // Cures poison/debuffs
        RevivePotion        // Revives in combat
    }

    /// <summary>
    /// Consumable class with level system (1-10)
    /// Higher level consumables have stronger effects
    /// </summary>
    public class Consumable : Item
    {
        // Consumable Properties
        public ConsumableType Type { get; set; }
        public int EffectPower { get; set; }  // Base effect strength (healing amount, damage, etc.)

        // Level Properties (Consumables can level 1-10)
        public int Level { get; set; }
        public const int MaxLevel = 10;

        // Base stats for level scaling
        private readonly int _baseEffectPower;

        /// <summary>
        /// Constructor for consumables
        /// </summary>
        public Consumable(
            string name,
            string description,
            ConsumableType type,
            int effectPower,
            ItemRarity rarity = ItemRarity.Common,
            int level = 1,
            int value = 0
        ) : base(name, description, rarity, ItemCategory.Consumable, value)
        {
            Type = type;
            Level = Math.Clamp(level, 1, MaxLevel);

            // Store base effect power for scaling
            _baseEffectPower = effectPower;

            // Apply level scaling
            UpdateStatsForLevel();

            // Consumables are stackable
            IsStackable = true;
            MaxStackSize = 99;
        }

        /// <summary>
        /// Level up the consumable (up to level 10)
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
            Console.WriteLine($"{Name} leveled up to level {Level}!");
            return true;
        }

        /// <summary>
        /// Update consumable stats based on current level
        /// Scales effect power with level
        /// </summary>
        private void UpdateStatsForLevel()
        {
            // Scale effect power with level
            // Every level adds 15% to base effect power
            float levelScaling = 1.0f + ((Level - 1) * 0.15f);

            EffectPower = (int)(_baseEffectPower * levelScaling);
        }

        /// <summary>
        /// Check if consumable can level up
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
        /// Override display name to include level for higher level consumables
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

            string levelSuffix = Level > 1 ? $" (Lv.{Level})" : "";
            string stackSuffix = Quantity > 1 ? $" x{Quantity}" : "";

            return $"{rarityPrefix}{Name}{levelSuffix}{stackSuffix}";
        }

        /// <summary>
        /// Override GetInfo to include consumable-specific stats
        /// </summary>
        public override string GetInfo()
        {
            string info = $"{GetDisplayName()}\n";
            info += $"{Description}\n";
            info += $"Type: {Type}\n";
            info += $"Rarity: {Rarity}\n";
            info += $"Level: {Level}/{MaxLevel}\n";
            info += $"Effect Power: {EffectPower}\n";
            info += $"Quantity: {Quantity}/{MaxStackSize}\n";
            info += $"Value: {Value} gold each\n";

            return info;
        }

        /// <summary>
        /// Use the consumable
        /// Applies the consumable's effect to the player
        /// </summary>
        public override void Use(Entities.Player.Player player)
        {
            if (Quantity <= 0)
            {
                Console.WriteLine($"No {Name} remaining!");
                return;
            }

            // Apply effect based on consumable type
            switch (Type)
            {
                case ConsumableType.HealthPotion:
                    player.Heal(EffectPower);
                    Console.WriteLine($"{player.Name} used {Name} and restored {EffectPower} HP!");
                    break;

                case ConsumableType.ManaPotion:
                    // TODO: Implement mana system if needed
                    Console.WriteLine($"{player.Name} used {Name} and restored {EffectPower} MP!");
                    break;

                case ConsumableType.BuffPotion:
                    // TODO: Implement buff system
                    Console.WriteLine($"{player.Name} used {Name} and gained a temporary buff!");
                    break;

                case ConsumableType.Food:
                    player.Heal(EffectPower / 2);
                    Console.WriteLine($"{player.Name} ate {Name} and restored {EffectPower / 2} HP!");
                    break;

                case ConsumableType.Elixir:
                    player.Heal(EffectPower);
                    Console.WriteLine($"{player.Name} used {Name} - a powerful elixir!");
                    break;

                case ConsumableType.Bomb:
                    Console.WriteLine($"{player.Name} used {Name} - it will deal {EffectPower} damage!");
                    // Bomb damage should be applied in combat
                    break;

                case ConsumableType.Antidote:
                    Console.WriteLine($"{player.Name} used {Name} and cured status ailments!");
                    // TODO: Implement status effect removal
                    break;

                case ConsumableType.RevivePotion:
                    Console.WriteLine($"{player.Name} used {Name} - revival effect activated!");
                    // TODO: Implement revival logic
                    break;

                default:
                    Console.WriteLine($"{player.Name} used {Name}.");
                    break;
            }

            // Consume one from the stack
            RemoveFromStack(1);
        }

        /// <summary>
        /// Consumables don't modify base stats
        /// They have instant effects when used
        /// </summary>
        public override void ModifyStat(Entities.Player.Player player)
        {
            // Consumables don't modify permanent stats
            // This method exists to satisfy the abstract requirement from Item base class
        }
    }
}