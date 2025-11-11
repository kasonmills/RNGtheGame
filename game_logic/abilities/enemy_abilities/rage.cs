using GameLogic.Entities;
using GameLogic.Systems;

namespace GameLogic.Abilities.EnemyAbilities
{
    /// <summary>
    /// Rage - Enemy ability that increases damage but reduces defense
    /// Scales with enemy level (Level 1 = weak rage, Level 100 = powerful rage)
    /// Classic berserker mechanic: high risk (reduced defense) for high reward (increased damage)
    /// </summary>
    public class Rage : Ability
    {
        private readonly int _enemyLevel;

        public Rage(int enemyLevel)
        {
            _enemyLevel = enemyLevel;
            Name = "Rage";
            Description = "Enter a furious rage, increasing attack power at the cost of defense";
            TargetType = AbilityTarget.Self; // Targets the enemy itself
            Rarity = AbilityRarity.Uncommon;

            ManaCost = 0;
            Cooldown = 5; // 5 turn cooldown

            // Override level to match enemy level
            Level = enemyLevel;
            MaxLevel = 100;
        }

        /// <summary>
        /// Execute the rage ability
        /// Applies RageEffect which increases damage but reduces defense
        /// </summary>
        public override void Execute(Entity user, Entity target, RNGManager rng)
        {
            Console.WriteLine($"\n{user.Name} uses {Name}!");

            // Calculate damage increase based on enemy level
            // Level 1: +20% damage
            // Level 100: +80% damage
            int damageBonus = 20 + (_enemyLevel * 60 / 99);  // 20 to 80

            // Calculate defense decrease based on enemy level
            // Level 1: -10% defense
            // Level 100: -40% defense
            int defenseReduction = 10 + (_enemyLevel * 30 / 99);  // 10 to 40

            // Duration scales slightly
            int duration = 3 + (_enemyLevel / 50);  // 3 to 5 turns

            // Create and apply rage effect
            var rageEffect = new RageEffect(
                duration: duration,
                damageBonus: damageBonus,
                defenseReduction: defenseReduction
            );

            user.AddEffect(rageEffect);

            Console.WriteLine($"{user.Name} enters a furious rage! (+{damageBonus}% damage, -{defenseReduction}% defense for {duration} turns)");

            // Set cooldown
            CurrentCooldown = Cooldown;
        }

        /// <summary>
        /// Get info about the rage ability
        /// </summary>
        public override string GetInfo()
        {
            int damageBonus = 20 + (_enemyLevel * 60 / 99);
            int defenseReduction = 10 + (_enemyLevel * 30 / 99);
            int duration = 3 + (_enemyLevel / 50);

            return $"{Name} (Enemy Lv.{_enemyLevel})\n" +
                   $"{Description}\n" +
                   $"+{damageBonus}% damage, -{defenseReduction}% defense for {duration} turns";
        }
    }

    /// <summary>
    /// Rage status effect - Increases damage output but reduces defense
    /// </summary>
    public class RageEffect : AbilityEffect
    {
        private readonly int _damageBonus;
        private readonly int _defenseReduction;

        public RageEffect(int duration, int damageBonus, int defenseReduction)
            : base(duration, damageBonus) // Potency = damage bonus for sorting purposes
        {
            _damageBonus = damageBonus;
            _defenseReduction = defenseReduction;

            Name = "Rage";
            Description = $"Attack +{damageBonus}%, Defense -{defenseReduction}%";
            Type = EffectType.Buff; // Self-buff despite the defense penalty
            CanStack = false;  // Only one rage at a time
        }

        /// <summary>
        /// Apply rage effect each turn (passive buff/debuff)
        /// </summary>
        public override void ApplyEffect(Entity target, RNGManager rng)
        {
            // This effect is passive - the modifiers are checked during damage calculation
            // No per-turn action needed
            Console.WriteLine($"  💢 {Name}: +{_damageBonus}% damage, -{_defenseReduction}% defense ({Duration} turns left)");
        }

        /// <summary>
        /// Get the damage multiplier for this effect
        /// </summary>
        public double GetDamageMultiplier()
        {
            return 1.0 + (_damageBonus / 100.0);
        }

        /// <summary>
        /// Get the defense multiplier for this effect
        /// (Defense reduction means taking MORE damage)
        /// </summary>
        public double GetDefenseMultiplier()
        {
            return 1.0 + (_defenseReduction / 100.0);
        }

        public override void OnApplied(Entity target)
        {
            Console.WriteLine($"💢 {target.Name} is consumed by rage!");
        }

        public override void OnExpired(Entity target)
        {
            Console.WriteLine($"💢 {target.Name}'s rage subsided.");
        }

        public override string GetDisplayInfo()
        {
            return $"{Name} (+{_damageBonus}% dmg, -{_defenseReduction}% def, {Duration} turns)";
        }
    }
}