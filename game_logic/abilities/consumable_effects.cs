using System;
using GameLogic.Entities;
using GameLogic.Systems;

namespace GameLogic.Abilities
{
    /// <summary>
    /// Temporary strength boost from consumables
    /// </summary>
    public class StrengthBoostEffect : AbilityEffect
    {
        private int _damageBonus;

        public StrengthBoostEffect(int potency, int duration) : base("Strength Boost", potency, duration)
        {
            Type = EffectType.Buff;
            CanStack = false;
            _damageBonus = potency;
        }

        public override void OnApplied(Entity target)
        {
            Console.WriteLine($"{target.Name} feels stronger! (+{_damageBonus} damage)");
        }

        public override void ApplyEffect(Entity target, RNGManager rng)
        {
            // Passive buff - damage bonus is applied during damage calculation
        }

        public override void OnExpired(Entity target)
        {
            Console.WriteLine($"{target.Name}'s strength boost faded.");
        }

        public int GetDamageBonus()
        {
            return _damageBonus;
        }
    }

    /// <summary>
    /// Temporary resistance boost from consumables - flat damage reduction
    /// </summary>
    public class ResistanceEffect : AbilityEffect
    {
        private int _damageReduction;

        public ResistanceEffect(int potency, int duration) : base("Resistance", potency, duration)
        {
            Type = EffectType.Buff;
            CanStack = false;
            _damageReduction = potency;
        }

        public override void OnApplied(Entity target)
        {
            Console.WriteLine($"{target.Name} feels more resilient! (-{_damageReduction} damage taken)");
        }

        public override void ApplyEffect(Entity target, RNGManager rng)
        {
            // Passive buff - damage reduction is applied during damage calculation
        }

        public override void OnExpired(Entity target)
        {
            Console.WriteLine($"{target.Name}'s resistance faded.");
        }

        public int GetDamageReduction()
        {
            return _damageReduction;
        }
    }

    /// <summary>
    /// Regeneration over time from consumables
    /// </summary>
    public class RegenerationEffect : AbilityEffect
    {
        public RegenerationEffect(int potency, int duration) : base("Regeneration", potency, duration)
        {
            Type = EffectType.HealOverTime;
            CanStack = false;
        }

        public override void OnApplied(Entity target)
        {
            Console.WriteLine($"{target.Name} begins regenerating health!");
        }

        public override void ApplyEffect(Entity target, RNGManager rng)
        {
            target.Heal(Potency);
            Console.WriteLine($"{target.Name} regenerated {Potency} HP!");
        }

        public override void OnExpired(Entity target)
        {
            Console.WriteLine($"{target.Name}'s regeneration ended.");
        }
    }

    /// <summary>
    /// Speed/Haste boost from consumables
    /// </summary>
    public class HasteEffect : AbilityEffect
    {
        public HasteEffect(int potency, int duration) : base("Haste", potency, duration)
        {
            Type = EffectType.Buff;
            CanStack = false;
        }

        public override void OnApplied(Entity target)
        {
            Console.WriteLine($"{target.Name} feels faster!");
        }

        public override void ApplyEffect(Entity target, RNGManager rng)
        {
            // Passive buff - speed bonus could be used for turn order
        }

        public override void OnExpired(Entity target)
        {
            Console.WriteLine($"{target.Name}'s haste ended.");
        }
    }

    /// <summary>
    /// Burn damage over time effect
    /// </summary>
    public class BurnEffect : AbilityEffect
    {
        private int _minDamage;
        private int _maxDamage;

        public BurnEffect(int potency, int duration) : base("Burning", potency, duration)
        {
            Type = EffectType.DamageOverTime;
            CanStack = true;
            _minDamage = Math.Max(1, potency - 2);
            _maxDamage = potency + 2;
        }

        public override void OnApplied(Entity target)
        {
            Console.WriteLine($"{target.Name} is burning!");
        }

        public override void ApplyEffect(Entity target, RNGManager rng)
        {
            int damage = rng.Roll(_minDamage, _maxDamage) * StackCount;
            target.TakeDamage(damage);
            Console.WriteLine($"{target.Name} takes {damage} burn damage!");
        }

        public override void OnExpired(Entity target)
        {
            Console.WriteLine($"{target.Name} is no longer burning.");
        }
    }

    /// <summary>
    /// Bleed damage over time effect
    /// </summary>
    public class BleedEffect : AbilityEffect
    {
        public BleedEffect(int potency, int duration) : base("Bleeding", potency, duration)
        {
            Type = EffectType.DamageOverTime;
            CanStack = true;
        }

        public override void OnApplied(Entity target)
        {
            Console.WriteLine($"{target.Name} is bleeding!");
        }

        public override void ApplyEffect(Entity target, RNGManager rng)
        {
            int damage = Potency * StackCount;
            target.TakeDamage(damage);
            Console.WriteLine($"{target.Name} loses {damage} HP from bleeding!");
        }

        public override void OnExpired(Entity target)
        {
            Console.WriteLine($"{target.Name}'s bleeding stopped.");
        }
    }
}