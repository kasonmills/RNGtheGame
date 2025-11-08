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
    public abstract class EnemyBase // TODO: Can inherit from Entity once that's built
    {
        // Basic Attributes
        public string Name { get; set; }
        public int Level { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }

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
        protected EnemyBase()
        {
            LootTable = new List<Item>();
        }

        /// <summary>
        /// Initialize enemy stats based on level
        /// Child classes override this to set their specific stat scaling
        /// </summary>
        public abstract void InitializeStats(int level);

        /// <summary>
        /// Take damage from an attack
        /// </summary>
        public virtual void TakeDamage(int damage)
        {
            Health -= damage;
            
            if (Health < 0)
            {
                Health = 0;
            }
        }

        /// <summary>
        /// Check if enemy is still alive
        /// </summary>
        public bool IsAlive()
        {
            return Health > 0;
        }

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
        /// </summary>
        public virtual List<Item> GetLootDrops()
        {
            // TODO: Implement loot drop logic based on LootTable
            // For now, return empty list
            return new List<Item>();
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