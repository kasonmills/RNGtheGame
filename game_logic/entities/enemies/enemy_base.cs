using System;
using System.Collections.Generic;
using GameLogic.Items;
using GameLogic.Abilities;

namespace GameLogic.Entities.Enemies
{
    /// <summary>
    /// Base class for all enemies
    /// Specific enemy types (Goblin, Dragon, etc.) inherit from this
    /// </summary>
    public abstract class EnemyBase : Entity
    {
        // Basic Attributes (Name, Level, Health, MaxHealth, Speed inherited from Entity)

        // Combat Stats - These define the enemy's combat capabilities
        public int MinDamage { get; set; }
        public int MaxDamage { get; set; }
        public int Accuracy { get; set; }      // Hit chance (1-100)
        public int CritChance { get; set; }    // Critical hit chance (1-100)
        public int Defense { get; set; }       // Damage reduction

        // Rewards
        public int GoldValue { get; set; }     // Base gold dropped
        public int XPValue { get; set; }       // Base XP given
        public List<Item> LootTable { get; set; } // Possible item drops

        // Enemy Classification
        public EnemyType Type { get; set; }
        public EnemyBehavior Behavior { get; set; }

        // Abilities (optional - some enemies have special abilities)
        public Ability SpecialAbility { get; set; }

        /// <summary>
        /// Constructor - child classes must call this
        /// </summary>
        protected EnemyBase() : base()
        {
            LootTable = new List<Item>();
        }

        /// <summary>
        /// Initialize enemy stats based on level
        /// Child classes override this to set their specific stat scaling
        /// </summary>
        public abstract void InitializeStats(int level);

        /// <summary>
        /// Get enemy description for display
        /// </summary>
        public virtual string GetDescription()
        {
            return $"{Name} (Level {Level})";
        }

        /// <summary>
        /// Get combat status display
        /// </summary>
        public string GetCombatStatus()
        {
            return $"{Name} - HP: {Health}/{MaxHealth}";
        }

        /// <summary>
        /// Determine AI action during combat
        /// Can be overridden by specific enemy types for unique behavior
        /// </summary>
        public virtual EnemyAction DecideAction()
        {
            // Default AI: Always attack
            // Specific enemies can override for smarter behavior
            return EnemyAction.Attack;
        }

        /// <summary>
        /// Get loot drops when defeated
        /// Uses RNG to determine which items from the loot table drop
        /// </summary>
        public virtual List<Item> GetLootDrops(Systems.RNGManager rng)
        {
            var drops = new List<Item>();

            if (LootTable == null || LootTable.Count == 0)
            {
                return drops;
            }

            // Each item in the loot table has a chance to drop
            foreach (var item in LootTable)
            {
                // Base drop chance based on item rarity
                int dropChance = item.Rarity switch
                {
                    ItemRarity.Common => 50,      // 50% chance
                    ItemRarity.Uncommon => 25,    // 25% chance
                    ItemRarity.Rare => 10,        // 10% chance
                    ItemRarity.Epic => 5,         // 5% chance
                    ItemRarity.Legendary => 2,    // 2% chance
                    ItemRarity.Mythic => 1,       // 1% chance
                    _ => 20                       // Default 20%
                };

                // Boss/Elite enemies have better drop rates
                if (Type == EnemyType.Boss)
                {
                    dropChance = Math.Min(dropChance * 3, 100);
                }
                else if (Type == EnemyType.Elite || Type == EnemyType.Miniboss)
                {
                    dropChance = Math.Min(dropChance * 2, 100);
                }

                // Roll for drop
                if (rng.Roll(1, 100) <= dropChance)
                {
                    drops.Add(item);
                }
            }

            return drops;
        }

        /// <summary>
        /// Implementation of abstract Execute method from Entity
        /// For enemies, this executes their AI action (attack, defend, use ability)
        /// </summary>
        public override void Execute(Entity target = null)
        {
            // Default implementation - perform basic attack
            // Specific enemy types can override for more complex behavior
            var action = DecideAction();

            // This would normally trigger combat logic
            // For now, placeholder
            Console.WriteLine($"{Name} executes action: {action}");
        }
    }

    /// <summary>
    /// Types of enemies (affects stats and rewards)
    /// </summary>
    public enum EnemyType
    {
        Common,      // Basic enemy - standard stats and rewards
        Elite,       // Stronger than common - better stats and rewards
        Boss,        // Much stronger - unique and powerful
        Miniboss     // Between elite and boss
    }

    /// <summary>
    /// AI behavior patterns for enemies
    /// </summary>
    public enum EnemyBehavior
    {
        Aggressive,  // Always attacks
        Defensive,   // Sometimes defends, especially at low HP
        Tactical,    // Uses abilities strategically
        Berserker,   // High risk/high reward - big damage, low accuracy
        Cautious,    // Defends when low on health
        Balanced     // Mix of attacking and defending
    }

    /// <summary>
    /// Possible actions an enemy can take
    /// </summary>
    public enum EnemyAction
    {
        Attack,
        Defend,
        UseAbility,
        Flee  // Some enemies might flee at low HP
    }
}