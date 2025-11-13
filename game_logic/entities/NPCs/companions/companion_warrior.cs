using System;
using GameLogic.Abilities;
using GameLogic.Entities;
using GameLogic.Systems;

namespace GameLogic.Entities.NPCs.Companions
{
    /// <summary>
    /// Warrior companion - Tanky melee fighter with defensive abilities
    /// Unique Ability: "Shield Bash" - Stuns enemy and reduces their damage
    /// Passive Bonus: +10% max health for player when in party
    /// </summary>
    public class CompanionWarrior : CompanionBase
    {
        public override string PassiveBonusDescription => "+10% Max Health for player";

        private int _playerMaxHealthBonus = 0; // Track the bonus we applied

        public CompanionWarrior(int startingLevel = 1)
            : base("Garrick the Brave", startingLevel, CompanionAIStyle.Defensive)
        {
            Description = "A stalwart warrior who shields his allies from harm.";
            Greeting = "I'll protect you with my life!";

            // Add some dialogue
            AddDialogue("Stay behind me!");
            AddDialogue("Their attacks mean nothing to me.");
            AddDialogue("I've faced worse odds.");

            // Set starting stats (higher health, medium damage)
            MaxHealth = 120 + (startingLevel * 12);  // More health than other companions
            Health = MaxHealth;

            // Initialize unique ability
            UniqueAbility = new ShieldBashAbility(this);

            // Equip starting weapon
            EquippedWeapon = Data.GameDatabase.GetWeapon("Rusty Sword");
            EquippedArmor = Data.GameDatabase.GetArmor("Rusty Chainmail");
        }

        /// <summary>
        /// Apply warrior's passive bonus: +10% max health to player
        /// </summary>
        public override void ApplyPassiveBonus(Player.Player player)
        {
            _playerMaxHealthBonus = (int)(player.MaxHealth * 0.10);
            player.MaxHealth += _playerMaxHealthBonus;
            player.Health += _playerMaxHealthBonus; // Also heal the amount

            Console.WriteLine($"{Name}'s presence bolsters you! (+{_playerMaxHealthBonus} Max HP)");
        }

        /// <summary>
        /// Remove warrior's passive bonus
        /// </summary>
        public override void RemovePassiveBonus(Player.Player player)
        {
            player.MaxHealth -= _playerMaxHealthBonus;

            // Make sure current health doesn't exceed new max
            if (player.Health > player.MaxHealth)
            {
                player.Health = player.MaxHealth;
            }

            Console.WriteLine($"{Name}'s bonus fades. (-{_playerMaxHealthBonus} Max HP)");
            _playerMaxHealthBonus = 0;
        }
    }

    /// <summary>
    /// Shield Bash - Unique ability for warrior companion
    /// Deals damage and applies a debuff that reduces enemy damage output
    /// </summary>
    public class ShieldBashAbility : Ability
    {
        private readonly CompanionWarrior _warrior;

        public ShieldBashAbility(CompanionWarrior warrior)
        {
            _warrior = warrior;
            Name = "Shield Bash";
            Description = "Bash the enemy with your shield, stunning them and reducing their damage";
            TargetType = AbilityTarget.SingleEnemy;
            Rarity = AbilityRarity.Uncommon;

            ManaCost = 0;
            Cooldown = 4;  // 4 turn cooldown
            Level = warrior.Level;
        }

        public override void Execute(Entity user, Entity target, RNGManager rng)
        {
            Console.WriteLine($"\n{_warrior.Name} uses {Name}!");

            // Deal damage based on warrior's level
            int baseDamage = 5 + (_warrior.Level * 2);  // Scales with level
            int damage = rng.Roll(baseDamage, baseDamage + 5);

            target.TakeDamage(damage);
            Console.WriteLine($"{target.Name} takes {damage} damage from the shield bash!");

            // Apply Stunned effect
            int duration = 2;  // 2 turns
            int damageReduction = 20 + (_warrior.Level / 2);  // 20-70% damage reduction

            var stunnedEffect = new ShieldBashEffect(duration, damageReduction);
            target.AddEffect(stunnedEffect);

            Console.WriteLine($"{target.Name} is stunned! Damage reduced by {damageReduction}% for {duration} turns!");

            // Set cooldown
            CurrentCooldown = Cooldown;

            // Gain experience for using ability
            GainExperience(10);
        }

        public override string GetInfo()
        {
            int baseDamage = 5 + (_warrior.Level * 2);
            int damageReduction = 20 + (_warrior.Level / 2);
            string cooldownInfo = CurrentCooldown > 0 ? $" (Cooldown: {CurrentCooldown})" : "";

            return $"{Name}{cooldownInfo}\n" +
                   $"{Description}\n" +
                   $"Damage: {baseDamage}-{baseDamage + 5}\n" +
                   $"Effect: -{damageReduction}% enemy damage for 2 turns";
        }

        protected override void OnLevelUp()
        {
            Console.WriteLine($"{Name} is now more powerful!");
        }
    }

    /// <summary>
    /// Shield Bash debuff effect - Reduces enemy damage output
    /// </summary>
    public class ShieldBashEffect : AbilityEffect
    {
        private readonly int _damageReduction;

        public ShieldBashEffect(int duration, int damageReduction)
            : base(duration, damageReduction)
        {
            _damageReduction = damageReduction;
            Name = "Stunned";
            Description = $"Damage reduced by {damageReduction}%";
            Type = EffectType.Debuff;
            CanStack = false;
        }

        public override void ApplyEffect(Entity target, RNGManager rng)
        {
            // Passive effect - damage reduction is checked during damage calculation
            Console.WriteLine($"  💫 {Name}: {target.Name}'s attacks are weakened! ({Duration} turns left)");
        }

        /// <summary>
        /// Get damage multiplier for affected entity
        /// If reduction is 30%, multiplier is 0.70 (deal 70% damage)
        /// </summary>
        public double GetDamageMultiplier()
        {
            return 1.0 - (_damageReduction / 100.0);
        }

        public override void OnApplied(Entity target)
        {
            Console.WriteLine($"💫 {target.Name} is stunned by the shield bash!");
        }

        public override void OnExpired(Entity target)
        {
            Console.WriteLine($"💫 {target.Name} recovers from the stun.");
        }

        public override string GetDisplayInfo()
        {
            return $"{Name} (-{_damageReduction}% dmg, {Duration} turns)";
        }
    }
}