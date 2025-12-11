using GameLogic.Entities;
using GameLogic.Systems;

namespace GameLogic.Abilities
{
    /// <summary>
    /// Base class for all abilities (both active combat skills and passive bonuses)
    /// Abilities are SCALABLE - they level up independently and get stronger
    /// </summary>
    public abstract class Ability
    {
        // Basic Info
        public string Name { get; protected set; }
        public string Description { get; protected set; }

        // Ability Type
        public bool IsPassive { get; protected set; } = false;  // True for passive abilities, false for active

        // Leveling System (1-100)
        public int Level { get; set; }
        public int MaxLevel { get; protected set; } = 100;
        public int Experience { get; set; }
        public int ExperienceToNextLevel { get; protected set; } = 100; // Increases per level

        // Cooldown System (only for active abilities)
        public int Cooldown { get; protected set; }  // Turns until can use again
        public int CurrentCooldown { get; set; }     // Current cooldown remaining

        // Combat Experience Tracking
        public int CombatUsageCount { get; private set; }  // How many times used in current combat

        // Targeting (only relevant for active abilities)
        public AbilityTarget TargetType { get; protected set; }

        // Rarity (affects how powerful the ability can become)
        public AbilityRarity Rarity { get; protected set; }

        /// <summary>
        /// Constructor - abilities start at level 1
        /// </summary>
        protected Ability()
        {
            Level = 1;
            Experience = 0;
            CurrentCooldown = 0;
        }

        /// <summary>
        /// Execute the ability's effect (only for active abilities)
        /// Passive abilities don't execute - they provide permanent bonuses
        /// Child classes implement this with their specific logic
        /// </summary>
        public virtual void Execute(Entity user, Entity target, RNGManager rng)
        {
            if (IsPassive)
            {
                Console.WriteLine($"{Name} is a passive ability and cannot be activated.");
                return;
            }
        }

        /// <summary>
        /// Get the current passive bonus value (only for passive abilities)
        /// Override this in passive ability implementations
        /// </summary>
        public virtual int GetPassiveBonusValue()
        {
            if (!IsPassive)
            {
                return 0;
            }
            // Override in child classes
            return 0;
        }

        /// <summary>
        /// Check if ability can be used right now
        /// </summary>
        public virtual bool CanUse(Entity user)
        {
            // Check cooldown
            if (CurrentCooldown > 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Level up the ability, increasing its power
        /// </summary>
        public void LevelUp()
        {
            if (Level >= MaxLevel)
            {
                Console.WriteLine($"{Name} is already at max level!");
                return;
            }

            Level++;
            Experience = 0;
            ExperienceToNextLevel = CalculateNextLevelXP();

            Console.WriteLine($"{Name} leveled up to Level {Level}!");
            OnLevelUp();
        }

        /// <summary>
        /// Add experience to the ability
        /// </summary>
        public void GainExperience(int xp)
        {
            if (Level >= MaxLevel) return;

            Experience += xp;

            while (Experience >= ExperienceToNextLevel && Level < MaxLevel)
            {
                LevelUp();
            }
        }

        /// <summary>
        /// Calculate XP needed for next level (exponential scaling based on rarity)
        /// Base XP requirement determined by ability rarity:
        /// Common: 100, Uncommon: 200, Rare: 300, Epic: 400, Legendary: 500
        /// Uses same exponential formula as player: base * (1.035^(Level-1))
        /// </summary>
        protected virtual int CalculateNextLevelXP()
        {
            // Get base XP requirement based on rarity
            double baseXP = GetBaseXPByRarity();

            // Apply exponential scaling: base * (1.035^(Level-1))
            for (int i = 1; i < Level; i++)
            {
                baseXP *= 1.035;
            }

            return Convert.ToInt32(baseXP);
        }

        /// <summary>
        /// Get base XP requirement based on ability rarity
        /// </summary>
        private int GetBaseXPByRarity()
        {
            switch (Rarity)
            {
                case AbilityRarity.Common:
                    return 100;
                case AbilityRarity.Uncommon:
                    return 200;
                case AbilityRarity.Rare:
                    return 300;
                case AbilityRarity.Epic:
                    return 400;
                case AbilityRarity.Legendary:
                    return 500;
                default:
                    return 100; // Fallback to Common
            }
        }

        /// <summary>
        /// Called when ability levels up - child classes can override
        /// </summary>
        protected virtual void OnLevelUp()
        {
            // Child classes can implement special logic here
        }

        /// <summary>
        /// Calculate experience gain based on usage count in current combat
        /// First use: 10 XP
        /// Each additional use: +15% more than previous (cumulative)
        /// </summary>
        public int GetCombatScaledExperience()
        {
            const int baseXP = 10;

            if (CombatUsageCount == 0)
            {
                return baseXP; // First use: 10 XP
            }

            // Calculate scaled XP: 10 * (1.15^usageCount)
            double multiplier = Math.Pow(1.15, CombatUsageCount);
            int scaledXP = (int)Math.Round(baseXP * multiplier);

            return scaledXP;
        }

        /// <summary>
        /// Increment usage count and gain scaled experience
        /// Called when ability is used in combat
        /// </summary>
        public void GainCombatExperience()
        {
            int xpGained = GetCombatScaledExperience();
            CombatUsageCount++;
            GainExperience(xpGained);

            if (CombatUsageCount == 1)
            {
                Console.WriteLine($"{Name} gained {xpGained} XP!");
            }
            else
            {
                Console.WriteLine($"{Name} gained {xpGained} XP! (Combat use #{CombatUsageCount})");
            }
        }

        /// <summary>
        /// Reset combat usage count (called when combat ends)
        /// </summary>
        public void ResetCombatUsage()
        {
            CombatUsageCount = 0;
        }

        /// <summary>
        /// Reduce cooldown by 1 (called each turn)
        /// </summary>
        public void ReduceCooldown()
        {
            if (CurrentCooldown > 0)
            {
                CurrentCooldown--;
            }
        }

        /// <summary>
        /// Get the effective cooldown based on ability level
        /// Levels 1-24: No reduction
        /// Levels 25-74: -1 turn cooldown
        /// Levels 75-100: -2 turns cooldown
        /// </summary>
        public int GetEffectiveCooldown()
        {
            int reduction = 0;

            if (Level >= 75)
            {
                reduction = 2;
            }
            else if (Level >= 25)
            {
                reduction = 1;
            }

            // Cooldown can't go below 1 turn
            return Math.Max(1, Cooldown - reduction);
        }

        /// <summary>
        /// Get ability info including level and power
        /// </summary>
        public virtual string GetInfo()
        {
            int effectiveCooldown = GetEffectiveCooldown();
            string cooldownInfo = effectiveCooldown != Cooldown
                ? $" (Cooldown: {effectiveCooldown} turns, reduced from {Cooldown})"
                : $" (Cooldown: {effectiveCooldown} turns)";

            return $"{Name} (Lv.{Level}/{MaxLevel}){cooldownInfo}\n{Description}";
        }

        /// <summary>
        /// Get the scaled value based on ability level
        /// Used by child classes to scale their effects
        /// minValue = value at level 1
        /// maxValue = value at max level (100)
        /// </summary>
        protected double GetScaledValue(double minValue, double maxValue)
        {
            // Linear scaling from minValue to maxValue
            double range = maxValue - minValue;
            double progress = (double)(Level - 1) / (MaxLevel - 1);
            return minValue + (range * progress);
        }

        /// <summary>
        /// Get scaled integer value (rounded)
        /// </summary>
        protected int GetScaledValueInt(int minValue, int maxValue)
        {
            return (int)Math.Round(GetScaledValue(minValue, maxValue));
        }
    }

    /// <summary>
    /// Targeting type for abilities
    /// </summary>
    public enum AbilityTarget
    {
        Self,           // Only affects the user
        SingleEnemy,    // Target one enemy
        AllEnemies,     // Affects all enemies
        SingleAlly,     // Target one ally (for team abilities)
        AllAllies,      // Affects all allies
        Area            // Affects area (enemies and allies)
    }

    /// <summary>
    /// Rarity affects how powerful abilities can become
    /// </summary>
    public enum AbilityRarity
    {
        Common,         // Basic abilities
        Uncommon,       // Slightly better scaling
        Rare,           // Good scaling
        Epic,           // Great scaling
        Legendary       // Amazing scaling, unique effects
    }
}
