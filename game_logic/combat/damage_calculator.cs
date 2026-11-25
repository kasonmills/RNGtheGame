using System;
using GameLogic.Entities;
using GameLogic.Entities.NPCs;
using GameLogic.Entities.NPCs.Companions;
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

        /// <summary>
        /// The save's difficulty - harder difficulties lower the party's hit chance
        /// (GameSettings.GetPartyAccuracyPenalty)
        /// </summary>
        public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Normal;

        /// <summary>
        /// The player's and the companions' weapon damage (weapon roll + level bonus) is
        /// multiplied by this - 2× decided 2026-11-10, a first pass to be tested and adjusted.
        /// Chance's cat-form hit (his level to 100) is not multiplied. Weapon info screens still
        /// show the weapon's own numbers.
        /// </summary>
        public const int PartyDamageScale = 2;

        /// <summary>
        /// The most a party member's armor, Defense Boost and Defend can cut a hit by, all
        /// together (decided 2026-11-24 - the Bulwark ability will count towards it too).
        /// </summary>
        public const double MaxDamageReduction = 0.75;

        /// <summary>
        /// Defending halves every hit until the defender's next turn
        /// </summary>
        public const double DefendDamageMultiplier = 0.5;

        public DamageCalculator(RNGManager rngManager)
        {
            _rngManager = rngManager;
        }

        /// <summary>
        /// Calculate one attack - any attacker against any target (the player, a companion or
        /// an enemy, on either side). Same steps for everyone: hit roll, base damage, active
        /// effects, crit roll, then the target's defense.
        /// </summary>
        /// <param name="partyLeader">The player - needed for the player's passive abilities that
        /// boost companions (Precision Training, Rallying Cry). Can be null.</param>
        /// <param name="ignoreDefense">Skip the target's defense/armor (e.g. a piercing bolt).
        /// Defending still halves the hit.</param>
        /// <param name="targetDefending">The target chose Defend - the hit is halved (for a
        /// party member, within the 75% cap together with armor and Defense Boost).</param>
        /// <param name="hitChanceModifier">Added to the hit chance (e.g. -10 for an attack on
        /// the back row - Formation.BackRowHitPenalty). The hit chance stays at least 1.</param>
        public DamageResult CalculateAttack(Entity attacker, Entity target, Player partyLeader = null,
            bool ignoreDefense = false, bool targetDefending = false, int hitChanceModifier = 0)
        {
            DamageResult result = new DamageResult();

            // Luck on the attacker's rolls - Unfair's bad luck for the party (Fluke's aura later)
            int goodLuck = GetGoodLuckChance(attacker);
            int badLuck = GetBadLuckChance(attacker);

            // Step 1: Check if attack hits (a low roll is good)
            int hitRoll = RollWithLuck(() => _rngManager.Roll(1, 100), higherIsBetter: false, goodLuck, badLuck);

            int hitChance = hitChanceModifier == 0
                ? GetAccuracy(attacker, partyLeader)
                : Math.Max(1, GetAccuracy(attacker, partyLeader) + hitChanceModifier);
            if (hitRoll > hitChance)
            {
                result.Missed = true;
                return result;
            }

            // Step 2: Base damage (weapon or claws, level bonus, passive abilities)
            int baseDamage = RollWithLuck(() => GetBaseDamage(attacker), higherIsBetter: true, goodLuck, badLuck);

            // Step 3: Active effects on the attacker
            // (every % on damage rounds by chance - RNGManager.RoundByChance)
            if (attacker.HasEffect<AttackBoostEffect>())
            {
                var attackBoost = attacker.GetEffect<AttackBoostEffect>();
                baseDamage = _rngManager.RoundByChance(baseDamage * attackBoost.GetDamageMultiplier());
            }

            if (attacker.HasEffect<StrengthBoostEffect>())
            {
                var strengthBoost = attacker.GetEffect<StrengthBoostEffect>();
                baseDamage += strengthBoost.GetDamageBonus();
            }

            if (attacker.HasEffect<RageEffect>())
            {
                var rage = attacker.GetEffect<RageEffect>();
                baseDamage = _rngManager.RoundByChance(baseDamage * rage.GetDamageMultiplier());
            }

            // Step 4: Check for critical hit (a low roll is good)
            int critRoll = RollWithLuck(() => _rngManager.Roll(1, 100), higherIsBetter: false, goodLuck, badLuck);

            if (critRoll <= GetCritChance(attacker))
            {
                result.IsCritical = true;
                baseDamage = _rngManager.RoundByChance(baseDamage * GetCritMultiplier(attacker));
            }

            // Step 5: Rallying Cry (the player's passive) boosts companion hits
            if (attacker is CompanionBase && partyLeader?.SelectedAbility is RallyingCryAbility rallyingCry)
            {
                baseDamage = _rngManager.RoundByChance(baseDamage * rallyingCry.GetDamageMultiplier());
            }

            // Step 6: An enraged target has lowered defense and takes more damage
            if (target.HasEffect<RageEffect>())
            {
                var rage = target.GetEffect<RageEffect>();
                baseDamage = _rngManager.RoundByChance(baseDamage * rage.GetDefenseMultiplier());
            }

            // Step 7: The target's defense (enemy defense stat, or worn armor and defensive
            // effects) and Defend
            int finalDamage = target is Enemy enemyTarget
                ? ApplyEnemyDefenseReduction(baseDamage, enemyTarget, ignoreDefense, targetDefending)
                : ApplyArmorReduction(baseDamage, target, ignoreDefense, targetDefending);

            result.RawDamage = baseDamage;
            result.FinalDamage = finalDamage;
            result.DamageReduced = baseDamage - finalDamage;

            return result;
        }

        /// <summary>
        /// On Unfair, about 1 in 4 of the party's hit, crit and damage rolls are rolled twice
        /// and the worse is kept (decided 2026-11-23).
        /// </summary>
        public const int UnfairBadLuckChance = 25;

        /// <summary>
        /// Chance (0-100) that one of this attacker's rolls is rolled twice, keeping the worse:
        /// the party on Unfair. Enemies never get it.
        /// </summary>
        private int GetBadLuckChance(Entity attacker)
        {
            return Difficulty == DifficultyLevel.Unfair && !(attacker is Enemy) ? UnfairBadLuckChance : 0;
        }

        /// <summary>
        /// Chance (0-100) that one of this attacker's rolls is rolled twice, keeping the better.
        /// Nothing gives it yet - Fluke's party aura (10% -> 30% with his level) plugs in here
        /// when he's built.
        /// </summary>
        private int GetGoodLuckChance(Entity attacker)
        {
            return 0;
        }

        /// <summary>
        /// One roll, with luck: with goodLuckChance it's rolled twice and the better kept, with
        /// badLuckChance twice and the worse kept. If both trigger on the same roll they cancel
        /// out (my default). With both chances at 0 it's a plain roll - no extra random numbers
        /// are used.
        /// </summary>
        /// <param name="higherIsBetter">True for a damage roll; false for a hit or crit roll,
        /// where a low number succeeds.</param>
        private int RollWithLuck(Func<int> roll, bool higherIsBetter, int goodLuckChance, int badLuckChance)
        {
            int first = roll();

            bool goodLuck = goodLuckChance > 0 && _rngManager.Roll(1, 100) <= goodLuckChance;
            bool badLuck = badLuckChance > 0 && _rngManager.Roll(1, 100) <= badLuckChance;
            if (goodLuck == badLuck)
            {
                return first;
            }

            int second = roll();
            bool keepHigher = goodLuck == higherIsBetter;
            return keepHigher ? Math.Max(first, second) : Math.Min(first, second);
        }

        /// <summary>
        /// Calculate damage for a player attack on an enemy (kept for existing callers and tests)
        /// </summary>
        public DamageResult CalculatePlayerAttackDamage(Player attacker, Enemy target)
        {
            return CalculateAttack(attacker, target, attacker);
        }

        /// <summary>
        /// Calculate damage for an enemy attack on the player (kept for existing callers and tests)
        /// </summary>
        public DamageResult CalculateEnemyAttackDamage(Enemy attacker, Player target)
        {
            return CalculateAttack(attacker, target, target);
        }

        /// <summary>
        /// Hit chance (1-100) for any attacker. The player and companions lose a set amount
        /// on harder difficulties (except Boon with his own crossbow); enemies don't.
        /// </summary>
        private int GetAccuracy(Entity attacker, Player partyLeader)
        {
            if (attacker is Enemy || (attacker is CompanionBoon boon && boon.HasOwnCrossbow))
            {
                return GetBaseAccuracy(attacker, partyLeader);
            }

            int penalty = GameSettings.GetPartyAccuracyPenalty(Difficulty);
            return Math.Max(1, GetBaseAccuracy(attacker, partyLeader) - penalty);
        }

        /// <summary>
        /// Hit chance (1-100) before the difficulty penalty
        /// </summary>
        private int GetBaseAccuracy(Entity attacker, Player partyLeader)
        {
            switch (attacker)
            {
                case Player player:
                    return GetPlayerAccuracy(player);

                case NPCBase companion:
                    int accuracy = companion is CompanionBase partyCompanion
                        ? partyCompanion.AttackAccuracy
                        : companion.EquippedWeapon != null ? companion.EquippedWeapon.Accuracy : 60;

                    // Precision Training (the player's passive) improves companion accuracy
                    if (partyLeader?.SelectedAbility is PrecisionTrainingAbility precisionTraining)
                    {
                        accuracy = (int)(accuracy + (accuracy * precisionTraining.GetAccuracyBonus()));
                    }
                    return accuracy;

                case Enemy enemy:
                    return enemy.Accuracy;

                default:
                    return 100;
            }
        }

        /// <summary>
        /// Base damage roll for any attacker, before effects, crits and defense
        /// </summary>
        private int GetBaseDamage(Entity attacker)
        {
            switch (attacker)
            {
                case Player player:
                    int playerDamage = (GetPlayerBaseDamage(player) + player.Level / 2) * PartyDamageScale; // +1 damage per 2 levels, then ×2
                    return ApplyPlayerPassiveAbilities(player, playerDamage);

                case CompanionBase partyCompanion:
                    return partyCompanion.RollAttackDamage(_rngManager);

                case NPCBase companion:
                    int companionDamage = companion.EquippedWeapon != null
                        ? _rngManager.Roll(companion.EquippedWeapon.MinDamage, companion.EquippedWeapon.MaxDamage)
                        : _rngManager.Roll(1, 3);
                    return companionDamage + companion.Level / 2; // Same level bonus as the player

                case Enemy enemy:
                    return _rngManager.Roll(enemy.MinDamage, enemy.MaxDamage);

                default:
                    return 1;
            }
        }

        /// <summary>
        /// Critical hit chance (0-100) for any attacker
        /// </summary>
        private int GetCritChance(Entity attacker)
        {
            if (attacker is Player player)
            {
                return GetPlayerCritChance(player);
            }

            int critChance = attacker switch
            {
                CompanionBase partyCompanion => partyCompanion.AttackCritChance,
                NPCBase companion => companion.EquippedWeapon != null ? companion.EquippedWeapon.CritChance : 2,
                Enemy enemy => enemy.CritChance,
                _ => 0
            };

            if (attacker.HasEffect<CriticalStrikeEffect>())
            {
                critChance += attacker.GetEffect<CriticalStrikeEffect>().GetCritChanceBonus();
            }

            return Math.Max(0, Math.Min(100, critChance));
        }

        /// <summary>
        /// Critical hit damage multiplier - the attacker's own value (1.5 unless something
        /// changes it), or the Executioner ability's value for a player who has it
        /// </summary>
        private double GetCritMultiplier(Entity attacker)
        {
            if (attacker is Player player && player.SelectedAbility is ExecutionerAbility executioner)
            {
                return executioner.GetCritMultiplier();
            }

            return attacker.CritMultiplier;
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

            return _rngManager.RoundByChance(baseDamage * damageMultiplier);
        }

        /// <summary>
        /// The share of a hit that worn armor stops (decided 2026-11-24): defense / (defense + 100).
        /// Defense 25 = 20%, 30 = 23%, 103 = 51% - it never reaches 100%.
        /// </summary>
        public static double GetArmorReduction(int defense)
        {
            return defense <= 0 ? 0.0 : (double)defense / (defense + 100);
        }

        /// <summary>
        /// Apply armor and defensive effects to incoming damage (for the player and companions).
        /// Armor %, Defense Boost and Defend multiply together (each applies to what's left),
        /// and together they cut at most 75% (MaxDamageReduction). Then the flat Resistance from
        /// consumables. A hit always does at least 1.
        /// </summary>
        /// <param name="ignoreArmor">A piercing hit - armor, Defense Boost and Resistance are
        /// skipped. Defend still halves it.</param>
        private int ApplyArmorReduction(int rawDamage, Entity target, bool ignoreArmor, bool defending)
        {
            double damageMultiplier = 1.0;
            int flatReduction = 0;

            if (!ignoreArmor)
            {
                // Worn armor (% reduction)
                damageMultiplier *= 1.0 - GetArmorReduction(GetArmorDefense(target));

                // DefenseBoost effect (% reduction)
                if (target.HasEffect<DefenseBoostEffect>())
                {
                    var defenseBoost = target.GetEffect<DefenseBoostEffect>();
                    damageMultiplier *= defenseBoost.GetDamageMultiplier();
                }

                // ResistanceEffect from consumables (flat reduction, after the %)
                if (target.HasEffect<ResistanceEffect>())
                {
                    var resistance = target.GetEffect<ResistanceEffect>();
                    flatReduction += resistance.GetDamageReduction();
                }
            }

            if (defending)
            {
                damageMultiplier *= DefendDamageMultiplier;
            }

            // The 75% cap on the total
            damageMultiplier = Math.Max(1.0 - MaxDamageReduction, damageMultiplier);

            int adjustedDamage = _rngManager.RoundByChance(rawDamage * damageMultiplier);
            return Math.Max(1, adjustedDamage - flatReduction);
        }

        /// <summary>
        /// Defense from the armor a party member is wearing (0 if none)
        /// </summary>
        private int GetArmorDefense(Entity target)
        {
            return target switch
            {
                Player player => player.EquippedArmor?.Defense ?? 0,
                NPCBase companion => companion.EquippedArmor?.Defense ?? 0,
                _ => 0
            };
        }

        /// <summary>
        /// Apply enemy defense to incoming damage. Enemy defense stays a flat reduction
        /// (decided 2026-11-24 - the enemy roster's numbers are sized for it), then Defend
        /// halves what's left. A hit always does at least 1.
        /// </summary>
        /// <param name="ignoreDefense">A piercing hit - the defense stat is skipped. Defend
        /// still halves it.</param>
        private int ApplyEnemyDefenseReduction(int rawDamage, Enemy target, bool ignoreDefense, bool defending)
        {
            int damage = ignoreDefense || target.Defense <= 0
                ? rawDamage
                : rawDamage - target.Defense;

            if (defending)
            {
                damage = _rngManager.RoundByChance(damage * DefendDamageMultiplier);
            }

            return Math.Max(1, damage);
        }

        /// <summary>
        /// Calculate damage with a simple modifier (for special abilities). The fraction
        /// rounds by chance.
        /// </summary>
        public int CalculateDamage(int baseDamage, float modifier)
        {
            return _rngManager.RoundByChance(baseDamage * (double)modifier);
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

            int actualHealing = _rngManager.RoundByChance(baseHealing * healingMultiplier);

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