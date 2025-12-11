using System;
using GameLogic.Entities.Player;
using GameLogic.Entities.Enemies;
using GameLogic.Systems;
using GameLogic.Abilities;
using GameLogic.Abilities.PlayerAbilities;
using GameLogic.Abilities.EnemyAbilities;
using GameLogic.Abilities.WeaponMasteries;
using Enemy = GameLogic.Entities.Enemies.EnemyBase;

namespace GameLogic.Combat
{
    /// <summary>
    /// Handles all damage calculations for combat
    /// Considers weapons, armor, abilities, level scaling, crits, etc.
    /// </summary>
    public class DamageCalculator
    {
        private RNGManager _rngManager;

        public DamageCalculator(RNGManager rngManager)
        {
            _rngManager = rngManager;
        }

        /// <summary>
        /// Calculate damage for attacks
        /// </summary>
        public DamageResult CalculatePlayerAttackDamage(Player attacker, Enemy target)
        {
            DamageResult result = new DamageResult();

            // Step 1: Check if attack hits
            int accuracy = GetPlayerAccuracy(attacker);
            int hitRoll = _rngManager.Roll(1, 100);
            
            if (hitRoll > accuracy)
            {
                result.Missed = true;
                return result;
            }

            // Step 2: Calculate base damage
            int baseDamage = GetPlayerBaseDamage(attacker);

            // Step 3: Apply level scaling
            int levelBonus = attacker.Level / 2; // +1 damage per 2 levels
            baseDamage += levelBonus;

            // Step 4: Apply passive ability modifiers
            baseDamage = ApplyPlayerPassiveAbilities(attacker, baseDamage);

            // Step 5: Apply AttackBoost effect if active
            if (attacker.HasEffect<AttackBoostEffect>())
            {
                var attackBoost = attacker.GetEffect<AttackBoostEffect>();
                double multiplier = attackBoost.GetDamageMultiplier();
                baseDamage = (int)(baseDamage * multiplier);
            }

            // Step 5b: Apply StrengthBoost effect from consumables if active
            if (attacker.HasEffect<StrengthBoostEffect>())
            {
                var strengthBoost = attacker.GetEffect<StrengthBoostEffect>();
                baseDamage += strengthBoost.GetDamageBonus();
            }

            // Step 6: Check for critical hit
            int critChance = GetPlayerCritChance(attacker);
            int critRoll = _rngManager.Roll(1, 100);

            if (critRoll <= critChance)
            {
                result.IsCritical = true;

                // Get crit multiplier (default 1.5x, or modified by Executioner)
                double critMultiplier = 1.5;
                if (attacker.SelectedAbility is ExecutionerAbility executioner)
                {
                    critMultiplier = executioner.GetCritMultiplier();
                }

                baseDamage = (int)(baseDamage * critMultiplier);
            }

            // Step 6: Apply enemy RageEffect defense penalty if active
            // (Enraged enemies take more damage due to lowered defense)
            if (target.HasEffect<RageEffect>())
            {
                var rage = target.GetEffect<RageEffect>();
                double defenseMultiplier = rage.GetDefenseMultiplier();
                baseDamage = (int)(baseDamage * defenseMultiplier);
            }

            // Step 7: Apply enemy defense
            int finalDamage = ApplyEnemyDefenseReduction(baseDamage, target);

            result.RawDamage = baseDamage;
            result.FinalDamage = finalDamage;
            result.DamageReduced = baseDamage - finalDamage;

            return result;
        }

        /// <summary>
        /// Calculate damage for an enemy attack
        /// </summary>
        public DamageResult CalculateEnemyAttackDamage(Enemy attacker, Player target)
        {
            DamageResult result = new DamageResult();

            // Step 1: Check if attack hits
            int hitRoll = _rngManager.Roll(1, 100);
            
            if (hitRoll > attacker.Accuracy)
            {
                result.Missed = true;
                return result;
            }

            // Step 2: Calculate base damage
            int baseDamage = _rngManager.Roll(attacker.MinDamage, attacker.MaxDamage);

            // Step 3: Apply RageEffect if active
            if (attacker.HasEffect<RageEffect>())
            {
                var rage = attacker.GetEffect<RageEffect>();
                double multiplier = rage.GetDamageMultiplier();
                baseDamage = (int)(baseDamage * multiplier);
            }

            // Step 4: Check for critical hit
            int critRoll = _rngManager.Roll(1, 100);

            if (critRoll <= attacker.CritChance)
            {
                result.IsCritical = true;
                baseDamage = (int)(baseDamage * 1.5f);
            }

            // Step 5: Apply player armor reduction
            int finalDamage = ApplyArmorReduction(baseDamage, target);

            result.RawDamage = baseDamage;
            result.FinalDamage = finalDamage;
            result.DamageReduced = baseDamage - finalDamage;
            
            return result;
        }

        /// <summary>
        /// Get player's base damage from weapon or unarmed
        /// </summary>
        private int GetPlayerBaseDamage(Player player)
        {
            if (player.EquippedWeapon != null)
            {
                // Armed: Use weapon damage range
                return _rngManager.Roll(
                    player.EquippedWeapon.MinDamage,
                    player.EquippedWeapon.MaxDamage
                );
            }
            else
            {
                // Unarmed: Weak damage
                return _rngManager.Roll(1, 3);
            }
        }

        /// <summary>
        /// Get player's accuracy
        /// </summary>
        private int GetPlayerAccuracy(Player player)
        {
            int baseAccuracy;

            if (player.EquippedWeapon != null)
            {
                baseAccuracy = player.EquippedWeapon.Accuracy;
            }
            else
            {
                // Unarmed: Lower accuracy
                baseAccuracy = 60;
            }

            // Apply passive bonuses from selected ability
            if (player.SelectedAbility != null)
            {
                // CriticalStrike ability provides small accuracy bonus (better precision)
                if (player.SelectedAbility is CriticalStrike)
                {
                    int accuracyBonus = player.SelectedAbility.Level / 10; // +1 accuracy per 10 levels
                    baseAccuracy += accuracyBonus;
                }
                // Precision Training ability provides scaling accuracy bonus
                else if (player.SelectedAbility is PrecisionTrainingAbility precisionTraining)
                {
                    double accuracyBonus = precisionTraining.GetAccuracyBonus();
                    baseAccuracy = (int)(baseAccuracy + (baseAccuracy * accuracyBonus));
                }
                // Weapon Mastery abilities provide accuracy bonus when wielding correct weapon
                else if (IsWeaponMasteryAbility(player.SelectedAbility, out var weaponMastery))
                {
                    if (weaponMastery.IsWieldingCorrectWeapon(player))
                    {
                        double accuracyMultiplier = weaponMastery.GetAccuracyMultiplier();
                        baseAccuracy = (int)(baseAccuracy + (baseAccuracy * accuracyMultiplier));
                    }
                }
            }

            return baseAccuracy;
        }

        /// <summary>
        /// Get player's critical hit chance
        /// </summary>
        private int GetPlayerCritChance(Player player)
        {
            int critChance;

            if (player.EquippedWeapon != null)
            {
                critChance = player.EquippedWeapon.CritChance;

                // Apply CriticalStrike effect if active
                if (player.HasEffect<CriticalStrikeEffect>())
                {
                    var critBoost = player.GetEffect<CriticalStrikeEffect>();
                    critChance += critBoost.GetCritChanceBonus();
                }
            }
            else
            {
                // Unarmed: Minimal crit chance (still benefit from CriticalStrike effect)
                critChance = 2;

                if (player.HasEffect<CriticalStrikeEffect>())
                {
                    var critBoost = player.GetEffect<CriticalStrikeEffect>();
                    critChance += critBoost.GetCritChanceBonus();
                }
            }

            // Apply Executioner passive ability modifier (can be negative or positive)
            if (player.SelectedAbility is ExecutionerAbility executioner)
            {
                int critModifier = executioner.GetCritChanceModifier();
                critChance += critModifier;
            }

            // Apply Weapon Mastery crit chance bonus
            if (player.SelectedAbility != null && IsWeaponMasteryAbility(player.SelectedAbility, out var weaponMastery))
            {
                if (weaponMastery.IsWieldingCorrectWeapon(player))
                {
                    critChance += weaponMastery.GetCritChanceBonus();
                }
            }

            // Ensure crit chance doesn't go below 0 or above 100
            return Math.Max(0, Math.Min(100, critChance));
        }

        /// <summary>
        /// Apply passive ability modifiers to damage
        /// Each ability provides a small passive bonus based on its level
        /// </summary>
        private int ApplyPlayerPassiveAbilities(Player player, int baseDamage)
        {
            if (player.SelectedAbility == null)
                return baseDamage;

            double damageMultiplier = 1.0;

            // AttackBoost: Provides passive damage increase even when not activated
            if (player.SelectedAbility is AttackBoost)
            {
                // Small passive bonus: 0.1% to 10% based on ability level
                double passiveBonus = player.SelectedAbility.Level * 0.1; // 0.1% per level
                damageMultiplier += passiveBonus / 100.0;
            }
            // CriticalStrike: Provides small passive damage bonus (precision strikes)
            else if (player.SelectedAbility is CriticalStrike)
            {
                // Smaller passive bonus: 0.05% to 5% based on ability level
                double passiveBonus = player.SelectedAbility.Level * 0.05; // 0.05% per level
                damageMultiplier += passiveBonus / 100.0;
            }
            // Weapon Mastery: Provides damage bonus when wielding correct weapon
            else if (IsWeaponMasteryAbility(player.SelectedAbility, out var weaponMastery))
            {
                if (weaponMastery.IsWieldingCorrectWeapon(player))
                {
                    damageMultiplier = weaponMastery.GetDamageMultiplier();
                }
            }
            // DefenseBoost: No damage bonus (defensive ability)
            // HealingAbility: No damage bonus (support ability)

            return (int)(baseDamage * damageMultiplier);
        }

        /// <summary>
        /// Apply armor reduction to incoming damage (for player defense)
        /// </summary>
        private int ApplyArmorReduction(int rawDamage, Player target)
        {
            double damageMultiplier = 1.0;
            int flatReduction = 0;

            // Apply DefenseBoost effect if active (percentage reduction)
            if (target.HasEffect<DefenseBoostEffect>())
            {
                var defenseBoost = target.GetEffect<DefenseBoostEffect>();
                damageMultiplier *= defenseBoost.GetDamageMultiplier();
            }

            // Apply ResistanceEffect from consumables if active (flat reduction)
            if (target.HasEffect<ResistanceEffect>())
            {
                var resistance = target.GetEffect<ResistanceEffect>();
                flatReduction += resistance.GetDamageReduction();
            }

            // Apply damage multiplier from effects first
            int adjustedDamage = (int)(rawDamage * damageMultiplier);

            // Then apply flat reduction
            adjustedDamage = Math.Max(1, adjustedDamage - flatReduction);

            if (target.EquippedArmor != null)
            {
                int defense = target.EquippedArmor.Defense;

                // Armor reduces damage but never below 1
                int finalDamage = Math.Max(1, adjustedDamage - defense);
                return finalDamage;
            }
            else
            {
                // No armor: Return adjusted damage (with effects if active)
                return Math.Max(1, adjustedDamage);
            }
        }

        /// <summary>
        /// Apply enemy defense reduction to incoming damage (for enemy defense)
        /// </summary>
        private int ApplyEnemyDefenseReduction(int rawDamage, Enemy target)
        {
            // Check if enemy has defense stat
            if (target.Defense <= 0)
            {
                // No defense: Return raw damage
                return Math.Max(1, rawDamage);
            }

            // Enemy defense works as flat damage reduction
            // Defense reduces damage but never below 1 (always deal at least 1 damage)
            int finalDamage = Math.Max(1, rawDamage - target.Defense);

            return finalDamage;
        }

        /// <summary>
        /// Calculate damage with a simple modifier (for special abilities)
        /// </summary>
        public int CalculateDamage(int baseDamage, float modifier)
        {
            return (int)(baseDamage * modifier);
        }

        /// <summary>
        /// Calculate healing amount (for healing abilities/items)
        /// </summary>
        public int CalculateHealing(int baseHealing, Player target)
        {
            double healingMultiplier = 1.0;

            // Apply passive healing bonuses from selected ability
            if (target.SelectedAbility != null)
            {
                // HealingAbility: Provides passive healing increase
                if (target.SelectedAbility is HealingAbility)
                {
                    // Passive bonus: 0.2% to 20% based on ability level
                    double passiveBonus = target.SelectedAbility.Level * 0.2; // 0.2% per level
                    healingMultiplier += passiveBonus / 100.0;
                }
                // DefenseBoost: Provides small healing bonus (regeneration)
                else if (target.SelectedAbility is DefenseBoost)
                {
                    // Small passive bonus: 0.1% to 10% based on ability level
                    double passiveBonus = target.SelectedAbility.Level * 0.1; // 0.1% per level
                    healingMultiplier += passiveBonus / 100.0;
                }
            }

            int actualHealing = (int)(baseHealing * healingMultiplier);

            // Can't heal above max health
            int maxPossibleHealing = target.MaxHealth - target.Health;
            return Math.Min(actualHealing, maxPossibleHealing);
        }

        /// <summary>
        /// Helper method to check if an ability is a weapon mastery ability
        /// Returns true if it's a mastery ability, with the ability cast to the base interface
        /// </summary>
        private bool IsWeaponMasteryAbility(Ability ability, out dynamic weaponMastery)
        {
            weaponMastery = null;

            if (ability is SwordMasteryAbility sword) { weaponMastery = sword; return true; }
            if (ability is AxeMasteryAbility axe) { weaponMastery = axe; return true; }
            if (ability is MaceMasteryAbility mace) { weaponMastery = mace; return true; }
            if (ability is DaggerMasteryAbility dagger) { weaponMastery = dagger; return true; }
            if (ability is SpearMasteryAbility spear) { weaponMastery = spear; return true; }
            if (ability is StaffMasteryAbility staff) { weaponMastery = staff; return true; }
            if (ability is BowMasteryAbility bow) { weaponMastery = bow; return true; }
            if (ability is CrossbowMasteryAbility crossbow) { weaponMastery = crossbow; return true; }
            if (ability is WandMasteryAbility wand) { weaponMastery = wand; return true; }

            return false;
        }
    }

    /// <summary>
    /// Result of a damage calculation
    /// </summary>
    public class DamageResult
    {
        public int RawDamage { get; set; }          // Damage before armor
        public int FinalDamage { get; set; }        // Damage after armor
        public int DamageReduced { get; set; }      // Amount reduced by armor
        public bool Missed { get; set; }            // Did the attack miss?
        public bool IsCritical { get; set; }        // Was it a critical hit?

        public DamageResult()
        {
            RawDamage = 0;
            FinalDamage = 0;
            DamageReduced = 0;
            Missed = false;
            IsCritical = false;
        }
    }
}