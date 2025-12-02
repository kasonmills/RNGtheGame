using System;

namespace GameLogic.Items
{
    /// <summary>
    /// Type of consumable effect
    /// </summary>
    public enum ConsumableType
    {
        HealthPotion,       // Restores health
        BuffPotion,         // Applies temporary buff
        Food,               // Restores health over time
        Elixir,             // Powerful multi-effect consumables
        Bomb,               // Throwable damage items
        Antidote,           // Cures poison/debuffs
        RevivePotion,       // Revives in combat
        AttackPotion,       // Temporary attack boost (like Attack Boost ability)
        DefensePotion,      // Temporary defense boost (like Defense Boost ability)
        CriticalPotion,     // Temporary crit chance boost (like Critical Strike ability)
        HealingPotion       // Instant healing (like Healing ability)
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
        /// Get the duration in turns for buff potions based on level
        /// Level 1-5: 1 turn
        /// Level 6-10: 2 turns
        /// </summary>
        private int GetPotionDuration()
        {
            return Level <= 5 ? 1 : 2;
        }

        /// <summary>
        /// Use a revival potion on a target entity (player, companion, or enemy)
        /// Scales from 1 HP at level 1 to full HP at level 10
        /// </summary>
        /// <param name="user">The entity using the potion</param>
        /// <param name="target">The entity to be revived</param>
        /// <returns>True if revival was successful, false if target is alive or other error</returns>
        public bool UseRevivePotion(Entities.Entity user, Entities.Entity target)
        {
            // Check if target is already alive
            if (target.IsAlive())
            {
                Console.WriteLine($"{target.Name} is still alive! Revival potions can only be used on defeated targets.");
                return false;
            }

            // Check if we have any potions
            if (Quantity <= 0)
            {
                Console.WriteLine($"No {Name} remaining!");
                return false;
            }

            Console.WriteLine($"\n💚 {user.Name} used {Name} on {target.Name}!");
            Console.WriteLine("✨ A warm light envelops the body...");

            // Calculate revival HP based on potion level
            // Level 1: 1 HP (minimum)
            // Level 5: 50% of max HP
            // Level 10: 100% of max HP (full revival)
            float revivePercentage = (float)Level / MaxLevel;
            int reviveHP = Math.Max(1, (int)(target.MaxHealth * revivePercentage));

            // Revive the target
            target.Health = reviveHP;

            Console.WriteLine($"💚 {target.Name} has been revived with {reviveHP}/{target.MaxHealth} HP!");

            // Show scaling info
            if (Level == 1)
            {
                Console.WriteLine("(Level 1 Revival Potion - minimal restoration)");
            }
            else if (Level == MaxLevel)
            {
                Console.WriteLine("(Level 10 Revival Potion - full restoration!)");
            }
            else
            {
                Console.WriteLine($"(Level {Level} Revival Potion - {revivePercentage:P0} restoration)");
            }

            // Remove any lingering negative effects
            target.ActiveEffects.Clear();
            Console.WriteLine($"All negative effects have been cleared from {target.Name}.");

            // Consume one from the stack
            RemoveFromStack(1);

            return true;
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

                case ConsumableType.BuffPotion:
                    Console.WriteLine($"{player.Name} used {Name}!");
                    // Apply appropriate buff based on effect power
                    // Higher power = stronger buff, 3 turn duration
                    if (EffectPower <= 5)
                    {
                        player.AddEffect(new Abilities.StrengthBoostEffect(EffectPower, 3));
                    }
                    else if (EffectPower <= 10)
                    {
                        player.AddEffect(new Abilities.ResistanceEffect(EffectPower / 2, 3));
                    }
                    else
                    {
                        // Strong potions give regeneration
                        player.AddEffect(new Abilities.RegenerationEffect(EffectPower / 3, 3));
                    }
                    break;

                case ConsumableType.Food:
                    player.Heal(EffectPower / 2);
                    Console.WriteLine($"{player.Name} ate {Name} and restored {EffectPower / 2} HP!");
                    break;

                case ConsumableType.Elixir:
                    // Elixirs are rare, expensive, multi-effect consumables
                    // Currently implemented: Combat Elixir (attack + defense boost)
                    // Future: Healing elixirs, specialty elixirs, etc.

                    Console.WriteLine($"{player.Name} used {Name} - a powerful combat elixir!");

                    // Combat elixirs provide dual benefits at reduced power
                    // Level 1 = 10% of Lv100 ability power, Level 10 = 60% of Lv100 ability power
                    // Attack ability maxes at 70%, so: Lv1 = 7%, Lv10 = 42%
                    double elixirAttackBoost = 7 + (Level - 1) * 3.89;
                    // Defense ability maxes at 60%, so: Lv1 = 6%, Lv10 = 36%
                    double elixirDefenseBoost = 6 + (Level - 1) * 3.33;

                    int elixirDuration = 1;  // Always 1 turn for elixirs (shorter than potions)

                    // Apply attack boost
                    var elixirAttackEffect = new Abilities.PlayerAbilities.AttackBoostEffect(
                        duration: elixirDuration,
                        potency: (int)elixirAttackBoost
                    );
                    player.AddEffect(elixirAttackEffect);

                    // Apply defense boost
                    var elixirDefenseEffect = new Abilities.PlayerAbilities.DefenseBoostEffect(
                        duration: elixirDuration,
                        potency: (int)elixirDefenseBoost
                    );
                    player.AddEffect(elixirDefenseEffect);

                    Console.WriteLine($"Attack increased by {elixirAttackBoost:F0}% and defense increased by {elixirDefenseBoost:F0}% for 1 turn!");
                    break;

                case ConsumableType.Bomb:
                    Console.WriteLine($"{player.Name} used {Name} - it will deal {EffectPower} damage!");
                    // Bomb damage should be applied in combat
                    break;

                case ConsumableType.Antidote:
                    Console.WriteLine($"{player.Name} used {Name}!");
                    player.RemoveNegativeEffects();
                    break;

                case ConsumableType.RevivePotion:
                    Console.WriteLine($"{player.Name} has a {Name}!");
                    Console.WriteLine("Revival potions must be used on a specific target during combat.");
                    Console.WriteLine("(Use the item targeting system in combat to revive fallen allies)");
                    break;

                case ConsumableType.AttackPotion:
                    Console.WriteLine($"{player.Name} used {Name}!");
                    // Calculate attack boost based on level (scales like AttackBoost ability)
                    // Level 1: ~10%, Level 10: ~70%
                    double attackBoost = 10 + (Level - 1) * 6.67; // Roughly 10% to 70%
                    int duration = GetPotionDuration();
                    var attackEffect = new Abilities.PlayerAbilities.AttackBoostEffect(
                        duration: duration,
                        potency: (int)attackBoost
                    );
                    player.AddEffect(attackEffect);
                    Console.WriteLine($"Attack damage increased by {attackBoost:F0}% for {duration} turn{(duration > 1 ? "s" : "")}!");
                    break;

                case ConsumableType.DefensePotion:
                    Console.WriteLine($"{player.Name} used {Name}!");
                    // Calculate defense boost based on level (scales like DefenseBoost ability)
                    // Level 1: ~10%, Level 10: ~60%
                    double defenseBoost = 10 + (Level - 1) * 5.56; // Roughly 10% to 60%
                    int defenseDuration = GetPotionDuration();
                    var defenseEffect = new Abilities.PlayerAbilities.DefenseBoostEffect(
                        duration: defenseDuration,
                        potency: (int)defenseBoost
                    );
                    player.AddEffect(defenseEffect);
                    Console.WriteLine($"Damage taken reduced by {defenseBoost:F0}% for {defenseDuration} turn{(defenseDuration > 1 ? "s" : "")}!");
                    break;

                case ConsumableType.CriticalPotion:
                    Console.WriteLine($"{player.Name} used {Name}!");
                    // Calculate crit chance boost based on level (scales like CriticalStrike ability)
                    // Level 1: ~5%, Level 10: ~50%
                    double critBoost = 5 + (Level - 1) * 5.0; // Roughly 5% to 50%
                    int critDuration = GetPotionDuration();
                    var critEffect = new Abilities.PlayerAbilities.CriticalStrikeEffect(
                        duration: critDuration,
                        potency: (int)critBoost
                    );
                    player.AddEffect(critEffect);
                    Console.WriteLine($"Critical hit chance increased by {critBoost:F0}% for {critDuration} turn{(critDuration > 1 ? "s" : "")}!");
                    break;

                case ConsumableType.HealingPotion:
                    Console.WriteLine($"{player.Name} used {Name}!");
                    // Healing scales with effect power (which scales with level)
                    player.Heal(EffectPower);
                    Console.WriteLine($"Restored {EffectPower} HP! (Current: {player.Health}/{player.MaxHealth})");
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