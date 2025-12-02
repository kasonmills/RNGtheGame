using System;
using GameLogic.Abilities;
using GameLogic.Entities;
using GameLogic.Systems;

namespace GameLogic.Entities.NPCs.Companions
{
    /// <summary>
    /// Rogue companion - High crit chance, stealth and gold find
    /// Unique Ability: "Backstab" - Guaranteed critical hit with massive damage
    /// Passive Bonus: +25% gold drops from enemies
    /// </summary>
    public class CompanionRogue : CompanionBase
    {
        public override string PassiveBonusDescription => "+25% Gold from enemies";

        private double _goldBonus = 1.25; // 25% bonus

        public CompanionRogue(int startingLevel = 1)
            : base("Shadow", startingLevel, CompanionAIStyle.Aggressive)
        {
            Description = "A silent assassin who strikes from the shadows.";
            Greeting = "The shadows are my home.";

            // Add some dialogue
            AddDialogue("They'll never see me coming.");
            AddDialogue("Light feet, quick blade.");
            AddDialogue("Every lock has a key.");

            // Set starting stats (balanced health, high crit)
            MaxHealth = 100 + (startingLevel * 10);  // Standard health
            Health = MaxHealth;

            // Speed: Very fast - light armor and agile
            Speed = 15 + (startingLevel / 2); // Rogues are the fastest companions

            // Initialize unique ability
            UniqueAbility = new BackstabAbility(this);

            // Equip starting weapon (daggers for rogues)
            EquippedWeapon = Data.GameDatabase.GetWeapon("Worn Dagger");
            EquippedArmor = Data.GameDatabase.GetArmor("Worn Leather");
        }

        /// <summary>
        /// Apply rogue's passive bonus: +25% gold from enemies
        /// </summary>
        public override void ApplyPassiveBonus(Player.Player player)
        {
            Console.WriteLine($"{Name}'s keen eyes spot hidden treasure! (+25% Gold drops)");
        }

        /// <summary>
        /// Remove rogue's passive bonus
        /// </summary>
        public override void RemovePassiveBonus(Player.Player player)
        {
            Console.WriteLine($"{Name}'s treasure sense fades. (-25% Gold drops)");
        }

        /// <summary>
        /// Get the gold multiplier for this rogue
        /// </summary>
        public double GetGoldMultiplier()
        {
            return InParty ? _goldBonus : 1.0;
        }
    }

    /// <summary>
    /// Backstab - Unique ability for rogue companion
    /// Guaranteed critical hit with high damage multiplier
    /// </summary>
    public class BackstabAbility : Ability
    {
        private readonly CompanionRogue _rogue;

        public BackstabAbility(CompanionRogue rogue)
        {
            _rogue = rogue;
            Name = "Backstab";
            Description = "Strike from the shadows for guaranteed critical damage";
            TargetType = AbilityTarget.SingleEnemy;
            Rarity = AbilityRarity.Epic;

            Cooldown = 3;  // 3 turn cooldown
            Level = rogue.Level;
        }

        public override void Execute(Entity user, Entity target, RNGManager rng)
        {
            Console.WriteLine($"\n{_rogue.Name} uses {Name}!");
            Console.WriteLine($"🗡️  {_rogue.Name} vanishes into the shadows...");

            // Calculate backstab damage (weapon damage x high multiplier)
            int weaponMin = _rogue.EquippedWeapon != null ? _rogue.EquippedWeapon.MinDamage : 1;
            int weaponMax = _rogue.EquippedWeapon != null ? _rogue.EquippedWeapon.MaxDamage : 3;
            int baseDamage = rng.Roll(weaponMin, weaponMax);

            // Apply level scaling
            int levelBonus = _rogue.Level / 2;
            baseDamage += levelBonus;

            // Guaranteed crit with high multiplier (scales with level)
            double critMultiplier = 2.5 + (_rogue.Level * 0.02);  // 2.5x to 4.5x at level 100
            int finalDamage = (int)(baseDamage * critMultiplier);

            target.TakeDamage(finalDamage);
            Console.WriteLine($"⚔️  CRITICAL BACKSTAB! {target.Name} takes {finalDamage} damage! ({critMultiplier:F1}x multiplier)");

            // Chance to apply bleeding effect
            int bleedChance = 40 + _rogue.Level / 2;  // 40-90% chance
            if (rng.Roll(1, 100) <= bleedChance)
            {
                int bleedDamage = _rogue.Level + 5;
                var bleedEffect = new BleedingEffect(3, bleedDamage);
                target.AddEffect(bleedEffect);
                Console.WriteLine($"🩸 {target.Name} is bleeding! ({bleedDamage} dmg/turn for 3 turns)");
            }

            // Set cooldown (uses level-based reduction)
            CurrentCooldown = GetEffectiveCooldown();

            // Gain scaled combat experience
            GainCombatExperience();
        }

        public override string GetInfo()
        {
            double critMultiplier = 2.5 + (_rogue.Level * 0.02);
            int bleedChance = 40 + _rogue.Level / 2;
            int bleedDamage = _rogue.Level + 5;
            string cooldownInfo = CurrentCooldown > 0 ? $" (Cooldown: {CurrentCooldown})" : "";

            return $"{Name}{cooldownInfo}\n" +
                   $"{Description}\n" +
                   $"Multiplier: {critMultiplier:F1}x weapon damage (guaranteed crit)\n" +
                   $"Effect: {bleedChance}% chance to cause bleeding ({bleedDamage} dmg/turn for 3 turns)";
        }

        protected override void OnLevelUp()
        {
            Console.WriteLine($"{Name} becomes deadlier!");
        }
    }

    /// <summary>
    /// Bleeding status effect - Damage over time from backstab
    /// </summary>
    public class BleedingEffect : AbilityEffect
    {
        private readonly int _damagePerTurn;

        public BleedingEffect(int duration, int damagePerTurn)
            : base(duration, damagePerTurn)
        {
            _damagePerTurn = damagePerTurn;
            Name = "Bleeding";
            Description = $"Taking {damagePerTurn} damage per turn";
            Type = EffectType.DamageOverTime;
            CanStack = true;  // Multiple bleeds can stack
        }

        public override void ApplyEffect(Entity target, RNGManager rng)
        {
            target.TakeDamage(_damagePerTurn);
            Console.WriteLine($"  🩸 {Name}: {target.Name} loses {_damagePerTurn} HP from bleeding! ({Duration} turns left)");
        }

        public override void OnApplied(Entity target)
        {
            Console.WriteLine($"🩸 {target.Name} is bleeding!");
        }

        public override void OnExpired(Entity target)
        {
            Console.WriteLine($"🩸 {target.Name}'s bleeding stops.");
        }

        public override string GetDisplayInfo()
        {
            string stackInfo = StackCount > 1 ? $" x{StackCount}" : "";
            return $"{Name}{stackInfo} ({_damagePerTurn} dmg, {Duration} turns)";
        }
    }
}