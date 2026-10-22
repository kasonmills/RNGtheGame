using GameLogic.Entities;
using GameLogic.Systems;

namespace GameLogic.Abilities.EnemyAbilities
{
    /// <summary>
    /// Poison Attack - Enemy ability that deals damage and applies poison DoT
    /// Scales with enemy level (Level 1 = weak poison, level cap 40 = deadly poison)
    /// </summary>
    public class PoisonAttack : Ability
    {
        private readonly int _enemyLevel;

        public PoisonAttack(int enemyLevel)
        {
            _enemyLevel = enemyLevel;
            Name = "Poison Attack";
            Description = "A venomous strike that poisons the target";
            TargetType = AbilityTarget.SingleEnemy; // Targets the player
            Rarity = AbilityRarity.Uncommon;

            Cooldown = 3; // 3 turn cooldown

            // Override level to match enemy level
            Level = enemyLevel;
        }

        /// <summary>
        /// Execute the poison attack
        /// Deals normal attack damage + applies Poisoned effect
        /// </summary>
        public override void Execute(Entity user, Entity target, RNGManager rng)
        {

            // Calculate poison damage based on enemy level (same range as before, reached at
            // the level cap of 40 instead of 100; bosses above the cap use the cap's numbers)
            // Level 1: 2-4 damage per turn for 3 turns
            // Level 40: 12-24 damage per turn for 5 turns
            var (minPoisonDamage, maxPoisonDamage, duration) = GetPoisonStats();

            // Create and apply poison effect
            var poisonEffect = new PoisonedEffect(
                duration: duration,
                minDamage: minPoisonDamage,
                maxDamage: maxPoisonDamage
            );

            target.AddEffect(poisonEffect);

            GameLog.Write($"The attack was venomous! Poison will deal {minPoisonDamage}-{maxPoisonDamage} damage per turn for {duration} turns!");

            // Set cooldown (uses level-based reduction)
            CurrentCooldown = GetEffectiveCooldown();
        }

        /// <summary>
        /// Poison damage per turn (min, max) and duration for this enemy's level
        /// </summary>
        private (int min, int max, int duration) GetPoisonStats()
        {
            int level = Math.Min(_enemyLevel, GameLogic.Items.LevelCaps.MaxLevel);
            return (2 + (level / 4),     // 2 to 12
                    4 + (level / 2),     // 4 to 24
                    3 + (level / 20));   // 3 to 5 turns
        }

        /// <summary>
        /// Get info about the poison attack
        /// </summary>
        public override string GetInfo()
        {
            var (minPoisonDamage, maxPoisonDamage, duration) = GetPoisonStats();

            return $"{Name} (Enemy Lv.{_enemyLevel})\n" +
                   $"{Description}\n" +
                   $"Poison: {minPoisonDamage}-{maxPoisonDamage} dmg/turn for {duration} turns";
        }
    }

    /// <summary>
    /// Poisoned status effect - Damage over Time
    /// </summary>
    public class PoisonedEffect : AbilityEffect
    {
        private readonly int _minDamage;
        private readonly int _maxDamage;

        public PoisonedEffect(int duration, int minDamage, int maxDamage)
            : base(duration, (minDamage + maxDamage) / 2) // Potency = average damage
        {
            _minDamage = minDamage;
            _maxDamage = maxDamage;

            Name = "Poisoned";
            Description = $"Taking {minDamage}-{maxDamage} poison damage each turn";
            Type = EffectType.DamageOverTime;
            CanStack = true;  // Multiple poisons can stack!
        }

        /// <summary>
        /// Apply poison damage each turn
        /// </summary>
        public override void ApplyEffect(Entity target, RNGManager rng)
        {
            // Roll damage within range
            int damage = rng.Roll(_minDamage, _maxDamage);

            // Apply damage
            target.TakeDamage(damage);

            GameLog.Write($"  💀 {Name}: {target.Name} takes {damage} poison damage! ({Duration} turns left)");
            GameLog.Write($"     {target.Name}'s HP: {target.Health}/{target.MaxHealth}");
        }

        public override void OnApplied(Entity target)
        {
            GameLog.Write($"☠ {target.Name} is poisoned!");
        }

        public override void OnExpired(Entity target)
        {
            GameLog.Write($"☠ The poison wore off from {target.Name}.");
        }

        public override string GetDisplayInfo()
        {
            string stackInfo = StackCount > 1 ? $" x{StackCount}" : "";
            return $"{Name}{stackInfo} ({_minDamage}-{_maxDamage} dmg, {Duration} turns)";
        }
    }
}