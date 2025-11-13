using System;
using GameLogic.Abilities;
using GameLogic.Entities;
using GameLogic.Systems;

namespace GameLogic.Entities.NPCs.Companions
{
    /// <summary>
    /// Mage companion - Powerful spellcaster with AoE damage
    /// Unique Ability: "Arcane Blast" - Deals high damage and has chance to hit multiple enemies
    /// Passive Bonus: +15% spell damage for player (affects staff weapons)
    /// </summary>
    public class CompanionMage : CompanionBase
    {
        public override string PassiveBonusDescription => "+15% Spell Damage (Staff weapons)";

        private bool _passiveApplied = false;

        public CompanionMage(int startingLevel = 1)
            : base("Lyra the Arcane", startingLevel, CompanionAIStyle.Strategic)
        {
            Description = "A talented mage who wields the raw power of arcane magic.";
            Greeting = "The arcane flows through me!";

            // Add some dialogue
            AddDialogue("Knowledge is power.");
            AddDialogue("I sense great magical energy here.");
            AddDialogue("Let me handle this with magic.");

            // Set starting stats (lower health, high damage potential)
            MaxHealth = 80 + (startingLevel * 8);  // Squishier than warrior
            Health = MaxHealth;

            // Initialize unique ability
            UniqueAbility = new ArcaneBlastAbility(this);

            // Equip starting weapon
            EquippedWeapon = Data.GameDatabase.GetWeapon("Cracked Staff");
            EquippedArmor = Data.GameDatabase.GetArmor("Tattered Cloth");
        }

        /// <summary>
        /// Apply mage's passive bonus: +15% spell damage (staff weapons)
        /// </summary>
        public override void ApplyPassiveBonus(Player.Player player)
        {
            // Store flag that we applied the bonus
            _passiveApplied = true;

            Console.WriteLine($"{Name}'s arcane knowledge empowers your spells! (+15% Staff damage)");
        }

        /// <summary>
        /// Remove mage's passive bonus
        /// </summary>
        public override void RemovePassiveBonus(Player.Player player)
        {
            _passiveApplied = false;
            Console.WriteLine($"{Name}'s arcane empowerment fades. (-15% Staff damage)");
        }

        /// <summary>
        /// Check if passive is active (used by damage calculations)
        /// </summary>
        public bool IsPassiveActive()
        {
            return _passiveApplied && InParty;
        }
    }

    /// <summary>
    /// Arcane Blast - Unique ability for mage companion
    /// Deals high magical damage with chance to hit multiple enemies (AoE potential)
    /// </summary>
    public class ArcaneBlastAbility : Ability
    {
        private readonly CompanionMage _mage;

        public ArcaneBlastAbility(CompanionMage mage)
        {
            _mage = mage;
            Name = "Arcane Blast";
            Description = "Unleash a powerful blast of arcane energy";
            TargetType = AbilityTarget.SingleEnemy;  // Primary target
            Rarity = AbilityRarity.Rare;

            ManaCost = 0;
            Cooldown = 5;  // 5 turn cooldown
            Level = mage.Level;
        }

        public override void Execute(Entity user, Entity target, RNGManager rng)
        {
            Console.WriteLine($"\n{_mage.Name} uses {Name}!");

            // Deal high damage based on mage's level
            int baseDamage = 10 + (_mage.Level * 3);  // High base damage
            int damage = rng.Roll(baseDamage, baseDamage + 10);

            target.TakeDamage(damage);
            Console.WriteLine($"💥 Arcane energy explodes! {target.Name} takes {damage} magic damage!");

            // Chance for secondary explosion (scales with level)
            int explosionChance = 30 + (_mage.Level / 2);  // 30-80% chance
            int explosionRoll = rng.Roll(1, 100);

            if (explosionRoll <= explosionChance)
            {
                int secondaryDamage = damage / 2;
                target.TakeDamage(secondaryDamage);
                Console.WriteLine($"⚡ The blast resonates! {target.Name} takes an additional {secondaryDamage} damage!");
            }

            // Set cooldown
            CurrentCooldown = Cooldown;

            // Gain experience for using ability
            GainExperience(10);
        }

        public override string GetInfo()
        {
            int baseDamage = 10 + (_mage.Level * 3);
            int explosionChance = 30 + (_mage.Level / 2);
            string cooldownInfo = CurrentCooldown > 0 ? $" (Cooldown: {CurrentCooldown})" : "";

            return $"{Name}{cooldownInfo}\n" +
                   $"{Description}\n" +
                   $"Damage: {baseDamage}-{baseDamage + 10} magic damage\n" +
                   $"Effect: {explosionChance}% chance for additional burst ({baseDamage / 2}-{(baseDamage + 10) / 2} dmg)";
        }

        protected override void OnLevelUp()
        {
            Console.WriteLine($"{Name}'s power intensifies!");
        }
    }
}