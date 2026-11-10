using System;
using System.Collections.Generic;
using GameLogic.Items;
using GameLogic.Abilities;
using GameLogic.Systems;

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
        public int AggressionLevel { get; set; } // 0-100, how likely this enemy is to notice/charge the player in the overworld
        public int FleeChance { get; set; } = 30; // 1-100: the chance of the party escaping this enemy, and of this enemy escaping the party (30 until each enemy type is tuned)

        // Rewards
        public int GoldValue { get; set; }     // Base gold dropped
        public int XPValue { get; set; }       // Base XP given
        public List<Item> LootTable { get; set; } // Possible item drops

        /// <summary>
        /// Enemies with the same group name pay less XP and gold the more of them are beaten in
        /// one fight (CombatManager.DiminishingRewardsFullCount / DiminishingRewardsFactor), so
        /// summoned enemies can't be farmed endlessly. Null (the default) = always full rewards.
        /// First user: the Slime Dragon's draglings, "Dragling" (decided 2026-11-10).
        /// </summary>
        public string DiminishingRewardsGroup { get; set; }

        /// <summary>
        /// True once DifficultyScaler.ApplyDifficultyToEnemy has scaled this enemy's health and
        /// damage for the game's difficulty - it is only done once per enemy. Anything that
        /// re-runs InitializeStats must set it back to false.
        /// </summary>
        public bool DifficultyApplied { get; set; }

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
                    ItemRarity.Legendary => 2,    // 2% chance
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
            GameLog.Write($"{Name} executes action: {action}");
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