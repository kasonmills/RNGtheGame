using System;
using System.Collections.Generic;
using GameLogic.Abilities;
using GameLogic.Combat;
using GameLogic.Entities.Enemies;
using GameLogic.Items;
using GameLogic.Systems;
using Enemy = GameLogic.Entities.Enemies.EnemyBase;

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

        /// <summary>
        /// Highest level a companion can reach - the same as the player (decided 2026-10-21)
        /// </summary>
        public const int MaxLevel = 50;

        // Companion-specific bonuses (passive effects) - companions without one leave this out
        public virtual string PassiveBonusDescription => "None";

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
            // Base XP formula: 100 * level scaling factor
            return (int)(100 * Math.Pow(1.15, level - 1));
        }

        /// <summary>
        /// Give experience to companion
        /// </summary>
        public void GainExperience(int amount)
        {
            Experience += amount;
            GameLog.Write($"{Name} gained {amount} XP!");

            // Check for level up
            while (Experience >= ExperienceToNextLevel && Level < MaxLevel)
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
        /// Base damage roll for a basic attack, before effects, crits and defense: the weapon's
        /// range (1-3 unarmed) plus 1 per 2 levels. Companions that don't fight with a weapon override this.
        /// </summary>
        public virtual int RollAttackDamage(RNGManager rng)
        {
            int weaponDamage = EquippedWeapon != null
                ? rng.Roll(EquippedWeapon.MinDamage, EquippedWeapon.MaxDamage)
                : rng.Roll(1, 3);
            return weaponDamage + Level / 2; // Same level bonus as the player
        }

        /// <summary>
        /// Hit chance (1-100) for a basic attack, before the player's passives
        /// </summary>
        public virtual int AttackAccuracy => EquippedWeapon != null ? EquippedWeapon.Accuracy : 60;

        /// <summary>
        /// Critical hit chance (0-100) for a basic attack, before effects
        /// </summary>
        public virtual int AttackCritChance => EquippedWeapon != null ? EquippedWeapon.CritChance : 2;

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
                GameLog.Write($"{Name}'s attack missed!");
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
                    GameLog.Write($"Rallying Cry increased damage by {bonusDamage}! (+{rallyingCry.GetPassiveBonusValue()}%)");
                }
            }

            result.RawDamage = baseDamage;
            result.FinalDamage = baseDamage;

            // Apply damage to target
            target.TakeDamage(baseDamage);

            string critText = result.IsCritical ? " Critical hit!" : "";
            GameLog.Write($"{Name} attacks {target.Name} for {baseDamage} damage!{critText}");

            return result;
        }

        /// <summary>
        /// Use the companion's unique ability
        /// </summary>
        public virtual void UseUniqueAbility(Entity target, RNGManager rng)
        {
            if (UniqueAbility == null)
            {
                GameLog.Write($"{Name} has no unique ability!");
                return;
            }

            if (UniqueAbility.IsOnCooldown())
            {
                GameLog.Write($"{Name}'s {UniqueAbility.Name} is on cooldown! ({UniqueAbility.CurrentCooldown} turns left)");
                return;
            }

            // Execute the ability
            UniqueAbility.Execute(this, target, rng);
        }

        /// <summary>
        /// True once this companion's starting items have gone into the party bag
        /// </summary>
        public bool StartingItemsGiven { get; set; }

        /// <summary>
        /// Items this companion brings into the shared party bag when first recruited
        /// (e.g. Boon's starting bolts). None by default.
        /// </summary>
        public virtual List<Item> GetStartingPartyItems()
        {
            return new List<Item>();
        }

        /// <summary>
        /// Every ability this companion can use in a fight. Most have only their unique
        /// ability; a companion that learns more overrides this.
        /// </summary>
        public virtual List<Ability> GetCombatAbilities()
        {
            return UniqueAbility == null ? new List<Ability>() : new List<Ability> { UniqueAbility };
        }

        /// <summary>
        /// Which ability to use when the AI decides to use one (allies = the standing party).
        /// </summary>
        public virtual Ability ChooseCombatAbility(List<Entity> allies)
        {
            return UniqueAbility;
        }

        /// <summary>
        /// Decide what action to take during combat based on AI style
        /// (allies = the standing party, this companion included).
        /// Returns action type: 0 = Attack, 1 = Ability, 2 = Defend
        /// </summary>
        public virtual int DecideCombatAction(Enemy target, List<Entity> allies, RNGManager rng)
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
            foreach (var ability in GetCombatAbilities())
            {
                ability.TickCooldown();
            }
        }

        /// <summary>
        /// Apply companion's passive bonus
        /// This is called by the game manager to apply persistent bonuses
        /// Companions with a passive override this; by default there is none
        /// </summary>
        public virtual void ApplyPassiveBonus(Entities.Player.Player player)
        {
        }

        /// <summary>
        /// Remove companion's passive bonus
        /// Called when companion leaves party
        /// </summary>
        public virtual void RemovePassiveBonus(Entities.Player.Player player)
        {
        }

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

            GameLog.Write($"{Name} is ready to fight!");
        }
    }
}