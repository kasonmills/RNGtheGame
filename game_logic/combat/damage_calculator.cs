using System;
using GameLogic.Entities.Player;
using GameLogic.Entities.Enemies;
using GameLogic.Systems;

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

            // Step 5: Check for critical hit
            int critChance = GetPlayerCritChance(attacker);
            int critRoll = _rngManager.Roll(1, 100);
            
            if (critRoll <= critChance)
            {
                result.IsCritical = true;
                baseDamage = (int)(baseDamage * 1.5f); // 50% bonus
            }

            // Step 6: Apply enemy defense/armor (handled separately)
            result.RawDamage = baseDamage;
            result.FinalDamage = baseDamage;
            
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

            // Step 3: Check for critical hit
            int critRoll = _rngManager.Roll(1, 100);
            
            if (critRoll <= attacker.CritChance)
            {
                result.IsCritical = true;
                baseDamage = (int)(baseDamage * 1.5f);
            }

            // Step 4: Apply player armor reduction
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
            if (player.EquippedWeapon != null)
            {
                int accuracy = player.EquippedWeapon.Accuracy;
                
                // TODO: Apply accuracy-boosting passive abilities
                // if (player has AccuracyBoost ability)
                // {
                //     accuracy += 10;
                // }
                
                return accuracy;
            }
            else
            {
                // Unarmed: Lower accuracy
                return 60;
            }
        }

        /// <summary>
        /// Get player's critical hit chance
        /// </summary>
        private int GetPlayerCritChance(Player player)
        {
            if (player.EquippedWeapon != null)
            {
                int critChance = player.EquippedWeapon.CritChance;
                
                // TODO: Apply crit-boosting passive abilities
                // if (player has CriticalStrike ability)
                // {
                //     critChance += 10;
                // }
                
                return critChance;
            }
            else
            {
                // Unarmed: Minimal crit chance
                return 2;
            }
        }

        /// <summary>
        /// Apply passive ability modifiers to damage
        /// </summary>
        private int ApplyPlayerPassiveAbilities(Player player, int baseDamage)
        {
            int modifiedDamage = baseDamage;
            
            // TODO: Check player's passive abilities and apply modifiers
            // Example:
            // if (player.SelectedAbility is AttackBoost && !ability.RequiresActivation)
            // {
            //     modifiedDamage += ability.BonusDamage;
            // }
            // 
            // if (player.SelectedAbility is SwordBoost && player.EquippedWeapon.Type == WeaponType.Sword)
            // {
            //     modifiedDamage = (int)(modifiedDamage * 1.15f); // 15% bonus
            // }
            
            return modifiedDamage;
        }

        /// <summary>
        /// Apply armor reduction to incoming damage
        /// </summary>
        private int ApplyArmorReduction(int rawDamage, Player target)
        {
            if (target.EquippedArmor != null)
            {
                int defense = target.EquippedArmor.Defense;
                
                // TODO: Apply defense-boosting passive abilities
                // if (target has DefenseBoost ability)
                // {
                //     defense += 5;
                // }
                
                // Armor reduces damage but never below 1
                int finalDamage = Math.Max(1, rawDamage - defense);
                return finalDamage;
            }
            else
            {
                // No armor: Full damage
                return rawDamage;
            }
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
            int actualHealing = baseHealing;
            
            // TODO: Apply healing-boosting passive abilities
            // if (target has HealingBoost ability)
            // {
            //     actualHealing = (int)(actualHealing * 1.25f); // 25% bonus
            // }
            
            // Can't heal above max health
            int maxPossibleHealing = target.MaxHealth - target.Health;
            return Math.Min(actualHealing, maxPossibleHealing);
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