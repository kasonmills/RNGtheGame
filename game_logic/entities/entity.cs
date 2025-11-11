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

        // Active Effects (buffs, debuffs, status effects)
        public List<AbilityEffect> ActiveEffects { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        protected Entity()
        {
            ActiveEffects = new List<AbilityEffect>();
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
        /// Process all active effects (called at start of turn)
        /// </summary>
        public void ProcessEffects(RNGManager rng)
        {
            if (ActiveEffects.Count == 0) return;

            Console.WriteLine($"\n--- {Name}'s Status Effects ---");

            // Create a copy to avoid modification during iteration
            var effectsToProcess = new List<AbilityEffect>(ActiveEffects);

            foreach (var effect in effectsToProcess)
            {
                // Apply the effect
                effect.ApplyEffect(this, rng);

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
        /// Abstract method for entity-specific behavior
        /// </summary>
        public abstract void Execute(Entity target = null);
    }
}
