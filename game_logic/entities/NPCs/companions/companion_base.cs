using System;
using GameLogic.Abilities;
using GameLogic.Combat;
using GameLogic.Entities.Enemies;
using GameLogic.Items;
using GameLogic.Systems;

namespace GameLogic.Entities.NPCs.Companions
{
    /// <summary>
    /// Combat AI strategy for companions
    /// </summary>
    public enum CompanionAIStyle
    {
        Aggressive,     // Prioritizes damage, attacks most damaged enemies
        Defensive,      // Prioritizes protecting player, attacks closest threats
        Balanced,       // Mix of offense and defense
        Supportive,     // Uses abilities to buff/heal allies
        Strategic       // Targets high-priority enemies (mages, healers, etc.)
    }

    /// <summary>
    /// Base class for all companions that travel with the player
    /// Extends NPCBase with combat-specific functionality
    /// </summary>
    public abstract class CompanionBase : NPCBase
    {
        // Combat Stats
        public int Experience { get; set; }
        public int ExperienceToNextLevel { get; set; }

        // Combat AI
        public CompanionAIStyle AIStyle { get; set; }
        public Ability UniqueAbility { get; protected set; }  // Each companion has a signature ability

        // Companion-specific bonuses (passive effects)
        public abstract string PassiveBonusDescription { get; }

        /// <summary>
        /// Constructor for companions
        /// </summary>
        protected CompanionBase(string name, int startingLevel, CompanionAIStyle aiStyle)
            : base(name, startingLevel, 100, 100)  // Calls NPCBase companion constructor
        {
            AIStyle = aiStyle;
            Experience = 0;
            ExperienceToNextLevel = CalculateExperienceRequired(startingLevel);
        }

        /// <summary>
        /// Calculate how much experience is needed for next level
        /// Uses same formula as player for consistency
        /// </summary>
        private int CalculateExperienceRequired(int level)
        {
            return (int)(Data.GameDatabase.Config.BaseXPPerLevel * Math.Pow(Data.GameDatabase.Config.XPScalingFactor, level - 1));
        }

        /// <summary>
        /// Give experience to companion
        /// </summary>
        public void GainExperience(int amount)
        {
            Experience += amount;
            Console.WriteLine($"{Name} gained {amount} XP!");

            // Check for level up
            while (Experience >= ExperienceToNextLevel && Level < 100)
            {
                Experience -= ExperienceToNextLevel;
                LevelUp();
                ExperienceToNextLevel = CalculateExperienceRequired(Level);
            }
        }

        /// <summary>
        /// Companion attacks an enemy (used during combat)
        /// </summary>
        public virtual DamageResult AttackEnemy(Enemy target, DamageCalculator damageCalc, RNGManager rng)
        {
            // Get base damage from weapon
            int minDamage = EquippedWeapon != null ? EquippedWeapon.MinDamage : 1;
            int maxDamage = EquippedWeapon != null ? EquippedWeapon.MaxDamage : 3;
            int baseDamage = rng.Roll(minDamage, maxDamage);

            // Apply level scaling (companions scale similarly to player)
            int levelBonus = Level / 2;
            baseDamage += levelBonus;

            // Check for hit
            int accuracy = EquippedWeapon != null ? EquippedWeapon.Accuracy : 60;
            int hitRoll = rng.Roll(1, 100);

            DamageResult result = new DamageResult();

            if (hitRoll > accuracy)
            {
                result.Missed = true;
                Console.WriteLine($"{Name}'s attack missed!");
                return result;
            }

            // Check for critical hit
            int critChance = EquippedWeapon != null ? EquippedWeapon.CritChance : 2;
            int critRoll = rng.Roll(1, 100);

            if (critRoll <= critChance)
            {
                result.IsCritical = true;
                baseDamage = (int)(baseDamage * 1.5f);
            }

            result.RawDamage = baseDamage;
            result.FinalDamage = baseDamage;

            // Apply damage to target
            target.TakeDamage(baseDamage);

            string critText = result.IsCritical ? " Critical hit!" : "";
            Console.WriteLine($"{Name} attacks {target.Name} for {baseDamage} damage!{critText}");

            return result;
        }

        /// <summary>
        /// Use the companion's unique ability
        /// </summary>
        public virtual void UseUniqueAbility(Entity target, RNGManager rng)
        {
            if (UniqueAbility == null)
            {
                Console.WriteLine($"{Name} has no unique ability!");
                return;
            }

            if (UniqueAbility.IsOnCooldown())
            {
                Console.WriteLine($"{Name}'s {UniqueAbility.Name} is on cooldown! ({UniqueAbility.CurrentCooldown} turns left)");
                return;
            }

            // Execute the ability
            UniqueAbility.Execute(this, target, rng);
        }

        /// <summary>
        /// Decide what action to take during combat based on AI style
        /// Returns true if ability was used, false if basic attack should be used
        /// </summary>
        public virtual bool DecideCombatAction(Enemy target, RNGManager rng)
        {
            // Check if unique ability is available and should be used
            if (UniqueAbility != null && !UniqueAbility.IsOnCooldown())
            {
                // AI decision based on style
                switch (AIStyle)
                {
                    case CompanionAIStyle.Aggressive:
                        // Use ability if enemy health is above 50%
                        if (target.Health > target.MaxHealth / 2)
                        {
                            UseUniqueAbility(target, rng);
                            return true;
                        }
                        break;

                    case CompanionAIStyle.Defensive:
                        // Use ability if player health is low (would need player reference)
                        // For now, use ability randomly 30% of the time
                        if (rng.Roll(1, 100) <= 30)
                        {
                            UseUniqueAbility(target, rng);
                            return true;
                        }
                        break;

                    case CompanionAIStyle.Balanced:
                        // Use ability 50% of the time when available
                        if (rng.Roll(1, 100) <= 50)
                        {
                            UseUniqueAbility(target, rng);
                            return true;
                        }
                        break;

                    case CompanionAIStyle.Supportive:
                        // Always use support abilities when available
                        if (UniqueAbility.TargetType == AbilityTarget.Self || UniqueAbility.TargetType == AbilityTarget.Ally)
                        {
                            UseUniqueAbility(this, rng);  // Target self for support abilities
                            return true;
                        }
                        break;

                    case CompanionAIStyle.Strategic:
                        // Use ability on first turn or when enemy health is above 75%
                        if (target.Health > (target.MaxHealth * 3) / 4)
                        {
                            UseUniqueAbility(target, rng);
                            return true;
                        }
                        break;
                }
            }

            // Default to basic attack
            return false;
        }

        /// <summary>
        /// Reduce cooldowns at end of turn
        /// </summary>
        public void TickCooldowns()
        {
            if (UniqueAbility != null)
            {
                UniqueAbility.TickCooldown();
            }
        }

        /// <summary>
        /// Apply companion's passive bonus
        /// This is called by the game manager to apply persistent bonuses
        /// Each companion subclass implements their own passive
        /// </summary>
        public abstract void ApplyPassiveBonus(Entities.Player.Player player);

        /// <summary>
        /// Remove companion's passive bonus
        /// Called when companion leaves party
        /// </summary>
        public abstract void RemovePassiveBonus(Entities.Player.Player player);

        /// <summary>
        /// Get detailed companion info
        /// </summary>
        public string GetDetailedInfo()
        {
            string weaponInfo = EquippedWeapon != null ? $"{EquippedWeapon.Name} ({EquippedWeapon.MinDamage}-{EquippedWeapon.MaxDamage} dmg)" : "Unarmed (1-3 dmg)";
            string armorInfo = EquippedArmor != null ? $"{EquippedArmor.Name} ({EquippedArmor.Defense} def)" : "None (0 def)";
            string abilityInfo = UniqueAbility != null ? UniqueAbility.GetInfo() : "No special ability";

            return $"╔══════════════════════════════════════╗\n" +
                   $"  {Name} (Level {Level})\n" +
                   $"╠══════════════════════════════════════╣\n" +
                   $"  HP: {Health}/{MaxHealth}\n" +
                   $"  XP: {Experience}/{ExperienceToNextLevel}\n" +
                   $"  AI Style: {AIStyle}\n" +
                   $"  Status: {(InParty ? "In Party" : "Available")}\n" +
                   $"╠══════════════════════════════════════╣\n" +
                   $"  Weapon: {weaponInfo}\n" +
                   $"  Armor: {armorInfo}\n" +
                   $"╠══════════════════════════════════════╣\n" +
                   $"  Unique Ability:\n" +
                   $"  {abilityInfo}\n" +
                   $"╠══════════════════════════════════════╣\n" +
                   $"  Passive Bonus:\n" +
                   $"  {PassiveBonusDescription}\n" +
                   $"╚══════════════════════════════════════╝";
        }

        /// <summary>
        /// Override Execute to handle companion combat behavior
        /// </summary>
        public override void Execute(Entity target = null)
        {
            if (!InParty || target == null)
            {
                base.Execute(target);
                return;
            }

            Console.WriteLine($"{Name} is ready to fight!");
        }
    }
}