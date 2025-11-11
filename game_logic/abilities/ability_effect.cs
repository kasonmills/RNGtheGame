using GameLogic.Entities;
using GameLogic.Systems;

namespace GameLogic.Abilities
{
    /// <summary>
    /// Base class for ongoing status effects (buffs, debuffs, DoT, etc.)
    /// These are applied TO entities and persist for multiple turns
    /// Examples: Poison, Burn, Attack Boost, Defense Boost, Stun
    /// </summary>
    public abstract class AbilityEffect
    {
        // Basic Info
        public string Name { get; protected set; }
        public string Description { get; protected set; }

        // Duration (in turns)
        public int Duration { get; set; }           // Turns remaining
        public int MaxDuration { get; protected set; }   // Original duration

        // Potency (how strong the effect is)
        public int Potency { get; set; }            // Strength of effect

        // Effect Type
        public EffectType Type { get; protected set; }

        // Stacking behavior
        public bool CanStack { get; protected set; }  // Can multiple instances exist?
        public int StackCount { get; set; }           // How many stacks?

        /// <summary>
        /// Constructor
        /// </summary>
        protected AbilityEffect(int duration, int potency)
        {
            Duration = duration;
            MaxDuration = duration;
            Potency = potency;
            StackCount = 1;
        }

        /// <summary>
        /// Apply the effect each turn
        /// This is called during the entity's turn processing
        /// </summary>
        public abstract void ApplyEffect(Entity target, RNGManager rng);

        /// <summary>
        /// Called when effect is first applied to an entity
        /// </summary>
        public virtual void OnApplied(Entity target)
        {
            // Child classes can override for initial application effects
        }

        /// <summary>
        /// Called when effect expires or is removed
        /// </summary>
        public virtual void OnExpired(Entity target)
        {
            // Child classes can override for cleanup/final effects
        }

        /// <summary>
        /// Reduce duration by 1 turn
        /// </summary>
        public void TickDuration()
        {
            if (Duration > 0)
            {
                Duration--;
            }
        }

        /// <summary>
        /// Check if effect has expired
        /// </summary>
        public bool IsExpired()
        {
            return Duration <= 0;
        }

        /// <summary>
        /// Refresh the effect duration (for reapplication)
        /// </summary>
        public void RefreshDuration()
        {
            Duration = MaxDuration;
        }

        /// <summary>
        /// Add a stack to this effect
        /// </summary>
        public void AddStack()
        {
            if (CanStack)
            {
                StackCount++;
            }
        }

        /// <summary>
        /// Get display info for UI
        /// </summary>
        public virtual string GetDisplayInfo()
        {
            string stackInfo = CanStack && StackCount > 1 ? $" x{StackCount}" : "";
            return $"{Name}{stackInfo} ({Duration} turns)";
        }

        /// <summary>
        /// Get detailed description
        /// </summary>
        public virtual string GetDetailedInfo()
        {
            return $"{Name}\n{Description}\nDuration: {Duration}/{MaxDuration} turns\nPotency: {Potency}";
        }
    }

    /// <summary>
    /// Types of effects
    /// </summary>
    public enum EffectType
    {
        Buff,           // Positive effect
        Debuff,         // Negative effect
        DamageOverTime, // DoT (poison, burn, bleed)
        HealOverTime,   // HoT (regeneration)
        CrowdControl,  // Stun, sleep, freeze
        StatModifier    // Changes stats temporarily
    }
}