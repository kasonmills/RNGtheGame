using System;
using GameLogic.Abilities;
using GameLogic.Entities;
using GameLogic.Systems;

namespace GameLogic.Entities.NPCs.Companions
{
    /// <summary>
    /// Ranger companion - Ranged damage dealer with high accuracy
    /// Unique Ability: "Multishot" - Fires multiple arrows for guaranteed hits
    /// Passive Bonus: +10% accuracy for player
    /// </summary>
    public class CompanionRanger : CompanionBase
    {
        public override string PassiveBonusDescription => "+10% Accuracy for player";

        private int _accuracyBonus = 10;

        public CompanionRanger(int startingLevel = 1)
            : base("Hawk", startingLevel, CompanionAIStyle.Balanced)
        {
            Description = "A skilled archer who never misses their mark.";
            Greeting = "My arrows always find their target.";

            // Add some dialogue
            AddDialogue("I see everything from here.");
            AddDialogue("The wind is favorable.");
            AddDialogue("One shot, one kill.");

            // Set starting stats (medium health, high accuracy)
            MaxHealth = 95 + (startingLevel * 9);
            Health = MaxHealth;

            // Initialize unique ability
            UniqueAbility = new MultishotAbility(this);

            // Equip starting weapon (bow for rangers)
            EquippedWeapon = Data.GameDatabase.GetWeapon("Old Bow");
            EquippedArmor = Data.GameDatabase.GetArmor("Worn Leather");
        }

        /// <summary>
        /// Apply ranger's passive bonus: +10% accuracy
        /// </summary>
        public override void ApplyPassiveBonus(Player.Player player)
        {
            Console.WriteLine($"{Name}'s keen eye steadies your aim! (+{_accuracyBonus}% Accuracy)");
        }

        /// <summary>
        /// Remove ranger's passive bonus
        /// </summary>
        public override void RemovePassiveBonus(Player.Player player)
        {
            Console.WriteLine($"{Name}'s guidance fades. (-{_accuracyBonus}% Accuracy)");
        }

        /// <summary>
        /// Get accuracy bonus amount
        /// </summary>
        public int GetAccuracyBonus()
        {
            return InParty ? _accuracyBonus : 0;
        }
    }

    /// <summary>
    /// Multishot - Unique ability for ranger companion
    /// Fires multiple arrows that always hit
    /// </summary>
    public class MultishotAbility : Ability
    {
        private readonly CompanionRanger _ranger;

        public MultishotAbility(CompanionRanger ranger)
        {
            _ranger = ranger;
            Name = "Multishot";
            Description = "Fire a volley of arrows that never miss";
            TargetType = AbilityTarget.SingleEnemy;
            Rarity = AbilityRarity.Uncommon;

            ManaCost = 0;
            Cooldown = 3;  // 3 turn cooldown
            Level = ranger.Level;
        }

        public override void Execute(Entity user, Entity target, RNGManager rng)
        {
            Console.WriteLine($"\n{_ranger.Name} uses {Name}!");
            Console.WriteLine($"🏹 {_ranger.Name} draws multiple arrows at once!");

            // Calculate number of arrows (scales with level)
            int arrowCount = 3 + (_ranger.Level / 20);  // 3-8 arrows at max level

            // Calculate damage per arrow
            int weaponMin = _ranger.EquippedWeapon != null ? _ranger.EquippedWeapon.MinDamage : 2;
            int weaponMax = _ranger.EquippedWeapon != null ? _ranger.EquippedWeapon.MaxDamage : 4;

            int totalDamage = 0;
            int critCount = 0;

            // Fire each arrow
            for (int i = 0; i < arrowCount; i++)
            {
                int damage = rng.Roll(weaponMin, weaponMax);
                int levelBonus = _ranger.Level / 3;  // Slight level scaling
                damage += levelBonus;

                // Each arrow has independent crit chance
                int critChance = _ranger.EquippedWeapon != null ? _ranger.EquippedWeapon.CritChance : 4;
                critChance += 5;  // Bonus crit chance for multishot

                if (rng.Roll(1, 100) <= critChance)
                {
                    damage = (int)(damage * 1.5f);
                    critCount++;
                }

                totalDamage += damage;
                target.TakeDamage(damage);

                Console.WriteLine($"  🎯 Arrow {i + 1}: {damage} damage{(rng.Roll(1, 100) <= critChance ? " (CRIT!)" : "")}");
            }

            Console.WriteLine($"🏹 Total damage: {totalDamage} HP ({arrowCount} arrows, {critCount} crits!)");

            // Chance to apply marked effect for extra damage
            if (_ranger.Level >= 10)
            {
                int markChance = 40 + _ranger.Level / 2;
                if (rng.Roll(1, 100) <= markChance)
                {
                    var markEffect = new MarkedEffect(2, 15);
                    target.AddEffect(markEffect);
                    Console.WriteLine($"🎯 {target.Name} is marked! (+15% damage taken for 2 turns)");
                }
            }

            // Set cooldown
            CurrentCooldown = Cooldown;

            // Gain experience for using ability
            GainExperience(10);
        }

        public override string GetInfo()
        {
            int arrowCount = 3 + (_ranger.Level / 20);
            int weaponMin = _ranger.EquippedWeapon != null ? _ranger.EquippedWeapon.MinDamage : 2;
            int weaponMax = _ranger.EquippedWeapon != null ? _ranger.EquippedWeapon.MaxDamage : 4;
            int markChance = _ranger.Level >= 10 ? 40 + _ranger.Level / 2 : 0;
            string cooldownInfo = CurrentCooldown > 0 ? $" (Cooldown: {CurrentCooldown})" : "";

            string markInfo = markChance > 0 ? $"\nEffect: {markChance}% chance to mark target (+15% dmg taken)" : "";

            return $"{Name}{cooldownInfo}\n" +
                   $"{Description}\n" +
                   $"Arrows: {arrowCount}x guaranteed hits\n" +
                   $"Damage: {weaponMin}-{weaponMax} per arrow (with crit chance){markInfo}";
        }

        protected override void OnLevelUp()
        {
            Console.WriteLine($"{Name} becomes more precise!");
        }
    }

    /// <summary>
    /// Marked status effect - Increases damage taken
    /// </summary>
    public class MarkedEffect : AbilityEffect
    {
        private readonly int _damageIncrease;

        public MarkedEffect(int duration, int damageIncrease)
            : base(duration, damageIncrease)
        {
            _damageIncrease = damageIncrease;
            Name = "Marked";
            Description = $"Taking +{damageIncrease}% damage";
            Type = EffectType.Debuff;
            CanStack = false;
        }

        public override void ApplyEffect(Entity target, RNGManager rng)
        {
            // Passive effect - damage increase is checked during damage calculation
            Console.WriteLine($"  🎯 {Name}: {target.Name} is vulnerable! ({Duration} turns left)");
        }

        /// <summary>
        /// Get damage multiplier for marked targets
        /// If marked for 15%, multiplier is 1.15 (take 115% damage)
        /// </summary>
        public double GetDamageMultiplier()
        {
            return 1.0 + (_damageIncrease / 100.0);
        }

        public override void OnApplied(Entity target)
        {
            Console.WriteLine($"🎯 {target.Name} has been marked!");
        }

        public override void OnExpired(Entity target)
        {
            Console.WriteLine($"🎯 The mark on {target.Name} fades.");
        }

        public override string GetDisplayInfo()
        {
            return $"{Name} (+{_damageIncrease}% dmg taken, {Duration} turns)";
        }
    }
}