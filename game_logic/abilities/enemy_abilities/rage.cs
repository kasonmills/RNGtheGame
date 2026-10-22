using GameLogic.Entities;
using GameLogic.Systems;

namespace GameLogic.Abilities.EnemyAbilities
{
    /// <summary>
    /// Rage - Enemy ability that increases damage but reduces defense
    /// Scales with enemy level (Level 1 = weak rage, level cap 40 = powerful rage)
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

            Cooldown = 5; // 5 turn cooldown

            // Override level to match enemy level
            Level = enemyLevel;
        }

        /// <summary>
        /// Damage bonus %, defense reduction % and duration for this enemy's level. Same range as
        /// before, reached at the level cap of 40 instead of 100; bosses above the cap use the cap's numbers.
        /// Level 1: +20% damage, -10% defense, 3 turns. Level 40: +80% damage, -40% defense, 5 turns.
        /// </summary>
        private (int damageBonus, int defenseReduction, int duration) GetRageStats()
        {
            int level = Math.Min(_enemyLevel, GameLogic.Items.LevelCaps.MaxLevel);
            int steps = GameLogic.Items.LevelCaps.MaxLevel - 1;
            return (20 + ((level - 1) * 60 / steps),    // 20 to 80
                    10 + ((level - 1) * 30 / steps),    // 10 to 40
                    3 + (level / 20));                  // 3 to 5 turns
        }

        /// <summary>
        /// Execute the rage ability
        /// Applies RageEffect which increases damage but reduces defense
        /// </summary>
        public override void Execute(Entity user, Entity target, RNGManager rng)
        {

            // Damage up, defense down and duration all scale with enemy level
            var (damageBonus, defenseReduction, duration) = GetRageStats();

            // Create and apply rage effect
            var rageEffect = new RageEffect(
                duration: duration,
                damageBonus: damageBonus,
                defenseReduction: defenseReduction
            );

            user.AddEffect(rageEffect);

            GameLog.Write($"{user.Name} enters a furious rage! (+{damageBonus}% damage, -{defenseReduction}% defense for {duration} turns)");

            // Set cooldown (uses level-based reduction)
            CurrentCooldown = GetEffectiveCooldown();
        }

        /// <summary>
        /// Get info about the rage ability
        /// </summary>
        public override string GetInfo()
        {
            var (damageBonus, defenseReduction, duration) = GetRageStats();

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
            GameLog.Write($"  💢 {Name}: +{_damageBonus}% damage, -{_defenseReduction}% defense ({Duration} turns left)");
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
            GameLog.Write($"💢 {target.Name} is consumed by rage!");
        }

        public override void OnExpired(Entity target)
        {
            GameLog.Write($"💢 {target.Name}'s rage subsided.");
        }

        public override string GetDisplayInfo()
        {
            return $"{Name} (+{_damageBonus}% dmg, -{_defenseReduction}% def, {Duration} turns)";
        }
    }
}