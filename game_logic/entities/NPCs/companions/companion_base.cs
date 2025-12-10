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
        /// Get effective speed including Swift Tactics passive ability bonus
        /// Overrides base GetEffectiveSpeed() to add companion speed buff
        /// </summary>
        public int GetEffectiveSpeed(Player.Player player = null)
        {
            int baseSpeed = Speed + SpeedModifier;

            // Apply Swift Tactics passive ability bonus (if player has it)
            if (player != null && player.SelectedAbility is Abilities.SwiftTacticsAbility swiftTactics)
            {
                double speedMultiplier = swiftTactics.GetSpeedMultiplier();
                baseSpeed = (int)(baseSpeed * speedMultiplier);
            }

            return Math.Max(1, baseSpeed);  // Minimum speed of 1
        }

        /// <summary>
        /// Companion attacks an enemy (used during combat)
        /// </summary>
        public virtual DamageResult AttackEnemy(Enemy target, DamageCalculator damageCalc, RNGManager rng, Player.Player player = null)
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

            // Apply Precision Training passive ability bonus (if player has it)
            if (player != null && player.SelectedAbility is Abilities.PrecisionTrainingAbility precisionTraining)
            {
                double accuracyBonus = precisionTraining.GetAccuracyBonus();
                accuracy = (int)(accuracy + (accuracy * accuracyBonus));
            }

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

            // Apply Rallying Cry passive ability bonus (if player has it)
            if (player != null && player.SelectedAbility is Abilities.RallyingCryAbility rallyingCry)
            {
                double damageMultiplier = rallyingCry.GetDamageMultiplier();
                int originalDamage = baseDamage;
                baseDamage = (int)(baseDamage * damageMultiplier);
                int bonusDamage = baseDamage - originalDamage;

                if (bonusDamage > 0)
                {
                    Console.WriteLine($"Rallying Cry increased damage by {bonusDamage}! (+{rallyingCry.GetPassiveBonusValue()}%)");
                }
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
        /// Returns action type: 0 = Attack, 1 = Ability, 2 = Defend
        /// </summary>
        public virtual int DecideCombatAction(Enemy target, RNGManager rng)
        {
            // Calculate health percentage for decision-making
            float healthPercent = (float)Health / MaxHealth;

            // AI decision based on style
            switch (AIStyle)
            {
                case CompanionAIStyle.Aggressive:
                    // Rarely defends, prefers attacking and abilities
                    if (UniqueAbility != null && !UniqueAbility.IsOnCooldown() && target.Health > target.MaxHealth / 2)
                    {
                        return 1; // Use ability
                    }
                    // Only defend if very low health (below 20%)
                    if (healthPercent < 0.2f && rng.Roll(1, 100) <= 40)
                    {
                        return 2; // Defend
                    }
                    return 0; // Attack

                case CompanionAIStyle.Defensive:
                    // Defends more frequently when health is low
                    if (healthPercent < 0.4f && rng.Roll(1, 100) <= 60)
                    {
                        return 2; // Defend
                    }
                    if (UniqueAbility != null && !UniqueAbility.IsOnCooldown() && rng.Roll(1, 100) <= 30)
                    {
                        return 1; // Use ability
                    }
                    return 0; // Attack

                case CompanionAIStyle.Balanced:
                    // Balanced approach to all actions
                    if (healthPercent < 0.3f && rng.Roll(1, 100) <= 50)
                    {
                        return 2; // Defend
                    }
                    if (UniqueAbility != null && !UniqueAbility.IsOnCooldown() && rng.Roll(1, 100) <= 50)
                    {
                        return 1; // Use ability
                    }
                    return 0; // Attack

                case CompanionAIStyle.Supportive:
                    // Uses support abilities frequently, defends allies
                    if (UniqueAbility != null && !UniqueAbility.IsOnCooldown())
                    {
                        if (UniqueAbility.TargetType == AbilityTarget.Self || UniqueAbility.TargetType == AbilityTarget.Ally)
                        {
                            return 1; // Use support ability
                        }
                    }
                    // Defend when moderately injured
                    if (healthPercent < 0.5f && rng.Roll(1, 100) <= 40)
                    {
                        return 2; // Defend
                    }
                    return 0; // Attack

                case CompanionAIStyle.Strategic:
                    // Smart decision-making based on situation
                    if (UniqueAbility != null && !UniqueAbility.IsOnCooldown() && target.Health > (target.MaxHealth * 3) / 4)
                    {
                        return 1; // Use ability early
                    }
                    // Defend when health is below 35%
                    if (healthPercent < 0.35f && rng.Roll(1, 100) <= 70)
                    {
                        return 2; // Defend
                    }
                    return 0; // Attack

                default:
                    return 0; // Attack by default
            }
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