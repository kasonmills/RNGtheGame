using System;
using System.Collections.Generic;
using System.Linq;
using GameLogic.Abilities;
using GameLogic.Systems;

namespace GameLogic.Entities
{
    /// <summary>
    /// Base class for all entities (Player, Enemies, NPCs)
    /// </summary>
    public abstract class Entity
    {
        // Basic Attributes
        public string Name { get; set; }
        public string Description { get; set; }
        public int Level { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }

        // Combat Stats
        public int Speed { get; set; }  // Base speed - determines turn order (higher = acts first)
        public int SpeedModifier { get; set; }  // Temporary speed adjustment from last action (resets each round)

        // Action Tracking
        public CombatAction LastAction { get; set; }  // Track what entity did last turn

        // Active Effects (buffs, debuffs, status effects)
        public List<AbilityEffect> ActiveEffects { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        protected Entity()
        {
            ActiveEffects = new List<AbilityEffect>();
            SpeedModifier = 0;
            LastAction = CombatAction.None;
        }

        /// <summary>
        /// Get effective speed for turn order (base speed + temporary modifier)
        /// </summary>
        public int GetEffectiveSpeed()
        {
            return Math.Max(1, Speed + SpeedModifier);  // Minimum speed of 1
        }

        /// <summary>
        /// Reset speed modifier to 0 (called at start of each round)
        /// </summary>
        public void ResetSpeedModifier()
        {
            SpeedModifier = 0;
        }

        /// <summary>
        /// Take damage from an attack or effect
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
        /// Heal the entity
        /// </summary>
        public virtual void Heal(int amount)
        {
            Health += amount;

            if (Health > MaxHealth)
            {
                Health = MaxHealth;
            }
        }

        /// <summary>
        /// Check if entity is alive
        /// </summary>
        public bool IsAlive()
        {
            return Health > 0;
        }

        /// <summary>
        /// Add an effect to this entity
        /// </summary>
        public void AddEffect(AbilityEffect effect)
        {
            // Check if effect already exists
            var existingEffect = ActiveEffects.FirstOrDefault(e => e.GetType() == effect.GetType());

            if (existingEffect != null)
            {
                // Effect already exists
                if (effect.CanStack)
                {
                    // Stack the effect
                    existingEffect.AddStack();
                    existingEffect.RefreshDuration();
                }
                else
                {
                    // Just refresh duration
                    existingEffect.RefreshDuration();
                }
            }
            else
            {
                // New effect - add it
                ActiveEffects.Add(effect);
                effect.OnApplied(this);
                Console.WriteLine($"{Name} is affected by {effect.Name}!");
            }
        }

        /// <summary>
        /// Remove an effect from this entity
        /// </summary>
        public void RemoveEffect(AbilityEffect effect)
        {
            if (ActiveEffects.Contains(effect))
            {
                effect.OnExpired(this);
                ActiveEffects.Remove(effect);
                Console.WriteLine($"{effect.Name} wore off from {Name}.");
            }
        }

        /// <summary>
        /// Process all active effects (called at start of entity's turn)
        /// Effects tick down at the end of each round, not each turn
        /// </summary>
        public void ProcessEffects(RNGManager rng)
        {
            if (ActiveEffects.Count == 0) return;

            Console.WriteLine($"\n--- {Name}'s Status Effects ---");

            // Create a copy to avoid modification during iteration
            var effectsToProcess = new List<AbilityEffect>(ActiveEffects);

            foreach (var effect in effectsToProcess)
            {
                // Apply the effect (damage, healing, etc.)
                effect.ApplyEffect(this, rng);

                // NOTE: Duration is NOT reduced here
                // Duration ticks down at the END of each round (after all entities act)
                // This is handled separately in the combat manager
            }
        }

        /// <summary>
        /// Tick down all effect durations by 1 round
        /// Called at the end of each round by the combat manager
        /// </summary>
        public void TickEffectDurations()
        {
            if (ActiveEffects.Count == 0) return;

            // Create a copy to avoid modification during iteration
            var effectsToProcess = new List<AbilityEffect>(ActiveEffects);

            foreach (var effect in effectsToProcess)
            {
                // Reduce duration
                effect.TickDuration();

                // Remove if expired
                if (effect.IsExpired())
                {
                    RemoveEffect(effect);
                }
            }
        }

        /// <summary>
        /// Get a list of all active effect names (for UI display)
        /// </summary>
        public string GetActiveEffectsDisplay()
        {
            if (ActiveEffects.Count == 0)
            {
                return "None";
            }

            return string.Join(", ", ActiveEffects.Select(e => e.GetDisplayInfo()));
        }

        /// <summary>
        /// Check if entity has a specific effect type
        /// </summary>
        public bool HasEffect<T>() where T : AbilityEffect
        {
            return ActiveEffects.Any(e => e is T);
        }

        /// <summary>
        /// Get a specific effect if it exists
        /// </summary>
        public T GetEffect<T>() where T : AbilityEffect
        {
            return ActiveEffects.FirstOrDefault(e => e is T) as T;
        }

        /// <summary>
        /// Remove all negative status effects (used by antidotes and cleansing items)
        /// </summary>
        public void RemoveNegativeEffects()
        {
            // Find all negative effects (Debuffs, DamageOverTime, CrowdControl)
            var negativeEffects = ActiveEffects.Where(e =>
                e.Type == EffectType.Debuff ||
                e.Type == EffectType.DamageOverTime ||
                e.Type == EffectType.CrowdControl).ToList();

            if (negativeEffects.Count == 0)
            {
                Console.WriteLine($"{Name} has no negative effects to remove.");
                return;
            }

            // Remove each negative effect
            foreach (var effect in negativeEffects)
            {
                effect.OnExpired(this);
                ActiveEffects.Remove(effect);
                Console.WriteLine($"{effect.Name} was cured from {Name}!");
            }
        }

        /// <summary>
        /// Abstract method for entity-specific behavior
        /// </summary>
        public abstract void Execute(Entity target = null);
    }

    /// <summary>
    /// Types of combat actions that affect speed modifier for next round
    /// </summary>
    public enum CombatAction
    {
        None,           // No action taken yet
        Attack,         // Basic attack (reduces speed next round: -1 to -3)
        Defend,         // Defensive action (increases speed next round: +1 to +3)
        UseAbility,     // Used special ability (variable speed: -1 to +1)
        UseItem         // Used consumable item (similar to ability: -1 to +1)
    }
}
