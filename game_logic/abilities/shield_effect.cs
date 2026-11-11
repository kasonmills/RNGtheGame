using System;
using GameLogic.Entities;
using GameLogic.Systems;

namespace GameLogic.Abilities
{
    /// <summary>
    /// A raised shield with its own health (first user: the Librarian-Monk, decided 2026-11-11).
    ///
    /// While it is up, every attack that lands on its holder is cut in half, rounded down - the
    /// holder takes the smaller half, and the shield takes the rest out of its own health
    /// (CombatManager.ProcessAttack, ShieldBlocked event). When the shield's health runs out it
    /// breaks and is removed (EffectRemoved event); a hit bigger than what the shield has left
    /// only has that much blocked, and the rest goes through.
    ///
    /// Only attacks are blocked - damage over time (poison, burn) is not. It doesn't count down
    /// at the end of the round; it lasts until it breaks or the fight ends. It is a buff, so
    /// cures that remove harmful effects leave it alone.
    /// </summary>
    public class ShieldEffect : AbilityEffect
    {
        /// <summary>The shield's health left.</summary>
        public int ShieldHealth { get; private set; }

        /// <summary>The shield's full health when it was raised.</summary>
        public int MaxShieldHealth { get; }

        public bool IsBroken => ShieldHealth <= 0;

        public ShieldEffect(int shieldHealth)
            : base(duration: 1, potency: 0)
        {
            Name = "Shield";
            Description = "Halves the damage of every attack until the shield breaks.";
            Type = EffectType.Buff;
            CanStack = false;
            MaxShieldHealth = Math.Max(1, shieldHealth);
            ShieldHealth = MaxShieldHealth;
        }

        /// <summary>
        /// Block part of a hit. Returns how much the shield took: half the damage rounded up
        /// (so the holder takes half rounded down), but never more than the shield has left.
        /// The shield's health goes down by the same amount.
        /// </summary>
        public int Absorb(int damage)
        {
            if (damage <= 0 || IsBroken) return 0;

            int wantToBlock = damage - (damage / 2);
            int blocked = Math.Min(wantToBlock, ShieldHealth);
            ShieldHealth -= blocked;
            return blocked;
        }

        public override void ApplyEffect(Entity target, RNGManager rng)
        {
            // Nothing each turn - CombatManager checks for the shield when an attack lands.
        }

        /// <summary>
        /// Doesn't wear off at the end of the round - only breaking removes it.
        /// </summary>
        public override void TickDuration()
        {
        }

        public override string GetDisplayInfo()
        {
            return $"{Name} ({ShieldHealth}/{MaxShieldHealth})";
        }

        public override string GetDetailedInfo()
        {
            return $"{Name} ({ShieldHealth}/{MaxShieldHealth})\n{Description}";
        }
    }
}
