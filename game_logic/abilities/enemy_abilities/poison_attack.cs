using GameLogic.Entities;
using GameLogic.Systems;

namespace GameLogic.Abilities.EnemyAbilities
{
    /// <summary>
    /// Poison Attack - Enemy ability that deals damage and applies poison DoT
    /// Scales with enemy level (Level 1 = weak poison, Level 100 = deadly poison)
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

            ManaCost = 0;
            Cooldown = 3; // 3 turn cooldown

            // Override level to match enemy level
            Level = enemyLevel;
            MaxLevel = 100;
        }

        /// <summary>
        /// Execute the poison attack
        /// Deals normal attack damage + applies Poisoned effect
        /// </summary>
        public override void Execute(Entity user, Entity target, RNGManager rng)
        {
            Console.WriteLine($"\n{user.Name} uses {Name}!");

            // Calculate poison damage based on enemy level
            // Level 1: 2-4 damage per turn for 3 turns
            // Level 100: 15-25 damage per turn for 5 turns
            int minPoisonDamage = 2 + (_enemyLevel / 10);        // 2 to 12
            int maxPoisonDamage = 4 + (_enemyLevel / 5);         // 4 to 24

            // Duration also scales
            int duration = 3 + (_enemyLevel / 50);  // 3 to 5 turns

            // Create and apply poison effect
            var poisonEffect = new PoisonedEffect(
                duration: duration,
                minDamage: minPoisonDamage,
                maxDamage: maxPoisonDamage
            );

            target.AddEffect(poisonEffect);

            Console.WriteLine($"The attack was venomous! Poison will deal {minPoisonDamage}-{maxPoisonDamage} damage per turn for {duration} turns!");

            // Set cooldown
            CurrentCooldown = Cooldown;
        }

        /// <summary>
        /// Get info about the poison attack
        /// </summary>
        public override string GetInfo()
        {
            int minPoisonDamage = 2 + (_enemyLevel / 10);
            int maxPoisonDamage = 4 + (_enemyLevel / 5);
            int duration = 3 + (_enemyLevel / 50);

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

            Console.WriteLine($"  💀 {Name}: {target.Name} takes {damage} poison damage! ({Duration} turns left)");
            Console.WriteLine($"     {target.Name}'s HP: {target.Health}/{target.MaxHealth}");
        }

        public override void OnApplied(Entity target)
        {
            Console.WriteLine($"☠ {target.Name} is poisoned!");
        }

        public override void OnExpired(Entity target)
        {
            Console.WriteLine($"☠ The poison wore off from {target.Name}.");
        }

        public override string GetDisplayInfo()
        {
            string stackInfo = StackCount > 1 ? $" x{StackCount}" : "";
            return $"{Name}{stackInfo} ({_minDamage}-{_maxDamage} dmg, {Duration} turns)";
        }
    }
}