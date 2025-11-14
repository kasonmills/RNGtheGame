using System;
using System.Collections.Generic;
using Xunit;
using GameLogic.Combat;
using GameLogic.Systems;
using GameLogic.Entities.Player;
using GameLogic.Entities.Enemies;
using GameLogic.Entities.Enemies.EnemyTypes;
using GameLogic.Items;

namespace GameLogic.Tests
{
    /// <summary>
    /// Unit tests for DamageCalculator
    /// Tests damage calculations, critical hits, armor reduction, and accuracy checks
    ///
    /// NOTE: Some tests involving ability effects (AttackBoost, DefenseBoost, etc.)
    /// are commented out because Player and EnemyBase don't currently inherit from Entity.
    /// These tests should be uncommented once the Entity inheritance is implemented.
    /// </summary>
    public class DamageCalculatorTests
    {
        #region Test Helper Methods

        /// <summary>
        /// Create a test player with basic stats
        /// </summary>
        private Player CreateTestPlayer(string name = "TestHero", int level = 10)
        {
            var player = new Player(name);
            player.Level = level;
            player.Health = 100;
            player.MaxHealth = 100;
            return player;
        }

        /// <summary>
        /// Create a test goblin enemy
        /// </summary>
        private Goblin CreateTestGoblin(int level = 10)
        {
            return new Goblin(level);
        }

        /// <summary>
        /// Create a test weapon with specified stats
        /// </summary>
        private Weapon CreateTestWeapon(string name, int minDmg, int maxDmg, int accuracy = 85, int critChance = 10)
        {
            return new Weapon(
                name: name,
                description: "Test weapon",
                value: 100,
                minDamage: minDmg,
                maxDamage: maxDmg,
                accuracy: accuracy,
                critChance: critChance,
                level: 1,
                rarity: 1
            );
        }

        /// <summary>
        /// Create test armor with specified defense
        /// </summary>
        private Armor CreateTestArmor(string name, int defense)
        {
            return new Armor(
                name: name,
                description: "Test armor",
                value: 100,
                defense: defense,
                level: 1,
                rarity: 1
            );
        }

        #endregion

        #region Player Attack Tests

        [Fact]
        public void CalculatePlayerAttackDamage_WithWeapon_ReturnsPositiveDamage()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            var calculator = new DamageCalculator(rng);
            var player = CreateTestPlayer();
            var enemy = CreateTestGoblin();
            var weapon = CreateTestWeapon("Sword", minDmg: 10, maxDmg: 15, accuracy: 100); // 100% accuracy
            player.EquipWeapon(weapon);

            // Act
            var result = calculator.CalculatePlayerAttackDamage(player, enemy);

            // Assert
            Assert.False(result.Missed);
            Assert.True(result.FinalDamage > 0);
        }

        [Fact]
        public void CalculatePlayerAttackDamage_Unarmed_DealsDamage()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            var calculator = new DamageCalculator(rng);
            var player = CreateTestPlayer();
            var enemy = CreateTestGoblin();
            // No weapon equipped

            // Act - Run multiple times to account for low accuracy
            DamageResult result = null;
            for (int i = 0; i < 100; i++)
            {
                result = calculator.CalculatePlayerAttackDamage(player, enemy);
                if (!result.Missed)
                    break;
            }

            // Assert - Should eventually hit and deal 1-3 damage (unarmed)
            Assert.False(result.Missed);
            Assert.InRange(result.FinalDamage, 1, 20); // Include level bonuses
        }

        [Fact]
        public void CalculatePlayerAttackDamage_IncludesLevelBonus()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            var calculator = new DamageCalculator(rng);
            var lowLevelPlayer = CreateTestPlayer("Hero1", level: 2);
            var highLevelPlayer = CreateTestPlayer("Hero2", level: 50);
            var weapon = CreateTestWeapon("Sword", minDmg: 10, maxDmg: 10, accuracy: 100); // Fixed damage
            lowLevelPlayer.EquipWeapon(weapon);
            highLevelPlayer.EquipWeapon(CreateTestWeapon("Sword", minDmg: 10, maxDmg: 10, accuracy: 100));
            var enemy = CreateTestGoblin();

            // Act
            var lowLevelResult = calculator.CalculatePlayerAttackDamage(lowLevelPlayer, enemy);
            var highLevelResult = calculator.CalculatePlayerAttackDamage(highLevelPlayer, enemy);

            // Assert - Level 50 should have +25 damage bonus (50 / 2)
            Assert.True(highLevelResult.FinalDamage > lowLevelResult.FinalDamage);
            int expectedBonus = (50 / 2) - (2 / 2); // 25 - 1 = 24 extra damage
            Assert.True(highLevelResult.FinalDamage >= lowLevelResult.FinalDamage + expectedBonus - 2);
        }

        [Fact]
        public void CalculatePlayerAttackDamage_LowAccuracyWeapon_CanMiss()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            var calculator = new DamageCalculator(rng);
            var player = CreateTestPlayer();
            var enemy = CreateTestGoblin();
            var lowAccuracyWeapon = CreateTestWeapon("Inaccurate Weapon", minDmg: 10, maxDmg: 15, accuracy: 10); // 10% hit
            player.EquipWeapon(lowAccuracyWeapon);

            // Act - Try 100 attacks
            int missCount = 0;
            for (int i = 0; i < 100; i++)
            {
                var result = calculator.CalculatePlayerAttackDamage(player, enemy);
                if (result.Missed)
                    missCount++;
            }

            // Assert - Should miss most of the time
            Assert.True(missCount > 50); // At 10% accuracy, should miss ~90% of the time
        }

        [Fact]
        public void CalculatePlayerAttackDamage_CriticalHit_Increases Damage()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            var calculator = new DamageCalculator(rng);
            var player = CreateTestPlayer();
            var enemy = CreateTestGoblin();
            var highCritWeapon = CreateTestWeapon("Critical Blade", minDmg: 20, maxDmg: 20, accuracy: 100, critChance: 100);
            player.EquipWeapon(highCritWeapon);

            // Act
            var result = calculator.CalculatePlayerAttackDamage(player, enemy);

            // Assert - Should always crit with 100% crit chance
            Assert.True(result.IsCritical);
            // Base damage 20 + level bonus (10/2 = 5) = 25, then * 1.5 = 37.5 = 37
            Assert.True(result.FinalDamage >= 35); // Account for rounding
        }

        [Fact]
        public void CalculatePlayerAttackDamage_Miss_ReturnsMissedResult()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            var calculator = new DamageCalculator(rng);
            var player = CreateTestPlayer();
            var enemy = CreateTestGoblin();
            var weapon = CreateTestWeapon("Weapon", minDmg: 10, maxDmg: 15, accuracy: 1); // 1% accuracy
            player.EquipWeapon(weapon);

            // Act - Run multiple times to get a miss
            DamageResult result = null;
            for (int i = 0; i < 100; i++)
            {
                result = calculator.CalculatePlayerAttackDamage(player, enemy);
                if (result.Missed)
                    break;
            }

            // Assert
            Assert.True(result.Missed);
            Assert.Equal(0, result.FinalDamage);
        }

        #endregion

        #region Enemy Attack Tests

        [Fact]
        public void CalculateEnemyAttackDamage_ReturnsValidDamage()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            var calculator = new DamageCalculator(rng);
            var enemy = CreateTestGoblin(level: 10);
            var player = CreateTestPlayer();

            // Act
            var result = calculator.CalculateEnemyAttackDamage(enemy, player);

            // Assert
            if (!result.Missed)
            {
                Assert.True(result.FinalDamage > 0);
                Assert.InRange(result.RawDamage, enemy.MinDamage, enemy.MaxDamage + 10); // Allow for crits
            }
        }

        [Fact]
        public void CalculateEnemyAttackDamage_WithCrit_IncreasesDamage()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            var calculator = new DamageCalculator(rng);
            var enemy = CreateTestGoblin(level: 10);
            enemy.CritChance = 100; // Force crit
            enemy.Accuracy = 100; // Force hit
            var player = CreateTestPlayer();

            // Act
            var result = calculator.CalculateEnemyAttackDamage(enemy, player);

            // Assert
            Assert.True(result.IsCritical);
            Assert.True(result.RawDamage >= enemy.MinDamage * 1.5);
        }

        [Fact]
        public void CalculateEnemyAttackDamage_CanMiss()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            var calculator = new DamageCalculator(rng);
            var enemy = CreateTestGoblin(level: 10);
            enemy.Accuracy = 10; // Very low accuracy
            var player = CreateTestPlayer();

            // Act - Try 100 attacks
            int missCount = 0;
            for (int i = 0; i < 100; i++)
            {
                var result = calculator.CalculateEnemyAttackDamage(enemy, player);
                if (result.Missed)
                    missCount++;
            }

            // Assert - Should miss most attacks
            Assert.True(missCount > 50);
        }

        [Fact]
        public void CalculateEnemyAttackDamage_PlayerWithArmor_ReducesDamage()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            var calculator = new DamageCalculator(rng);
            var enemy = CreateTestGoblin(level: 10);
            enemy.MinDamage = 20;
            enemy.MaxDamage = 20;
            enemy.Accuracy = 100; // Always hit
            enemy.CritChance = 0; // Never crit

            var playerNoArmor = CreateTestPlayer("NoArmor");
            var playerWithArmor = CreateTestPlayer("WithArmor");
            var armor = CreateTestArmor("Plate Mail", defense: 10);
            playerWithArmor.EquipArmor(armor);

            // Act
            var resultNoArmor = calculator.CalculateEnemyAttackDamage(enemy, playerNoArmor);
            var resultWithArmor = calculator.CalculateEnemyAttackDamage(enemy, playerWithArmor);

            // Assert
            Assert.True(resultWithArmor.FinalDamage < resultNoArmor.FinalDamage);
            Assert.Equal(10, resultWithArmor.DamageReduced);
        }

        [Fact]
        public void CalculateEnemyAttackDamage_ArmorNeverReducesDamageBelow1()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            var calculator = new DamageCalculator(rng);
            var enemy = CreateTestGoblin(level: 1);
            enemy.MinDamage = 1;
            enemy.MaxDamage = 1;
            enemy.Accuracy = 100; // Always hit

            var player = CreateTestPlayer();
            var heavyArmor = CreateTestArmor("Super Armor", defense: 999);
            player.EquipArmor(heavyArmor);

            // Act
            var result = calculator.CalculateEnemyAttackDamage(enemy, player);

            // Assert - Damage should be reduced to minimum of 1
            Assert.Equal(1, result.FinalDamage);
        }

        [Fact]
        public void CalculateEnemyAttackDamage_Miss_ReturnsZeroDamage()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            var calculator = new DamageCalculator(rng);
            var enemy = CreateTestGoblin(level: 10);
            enemy.Accuracy = 1; // 1% hit chance
            var player = CreateTestPlayer();

            // Act - Try multiple times to get a miss
            DamageResult result = null;
            for (int i = 0; i < 100; i++)
            {
                result = calculator.CalculateEnemyAttackDamage(enemy, player);
                if (result.Missed)
                    break;
            }

            // Assert
            Assert.True(result.Missed);
            Assert.Equal(0, result.FinalDamage);
            Assert.Equal(0, result.RawDamage);
        }

        #endregion

        #region Utility Method Tests

        [Fact]
        public void CalculateDamage_WithModifier_AppliesMultiplier()
        {
            // Arrange
            var rng = new RNGManager();
            var calculator = new DamageCalculator(rng);

            // Act
            int result = calculator.CalculateDamage(baseDamage: 100, modifier: 1.5f);

            // Assert
            Assert.Equal(150, result);
        }

        [Fact]
        public void CalculateDamage_WithZeroModifier_ReturnsZero()
        {
            // Arrange
            var rng = new RNGManager();
            var calculator = new DamageCalculator(rng);

            // Act
            int result = calculator.CalculateDamage(baseDamage: 100, modifier: 0f);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void CalculateDamage_WithNegativeModifier_ReturnsNegative()
        {
            // Arrange
            var rng = new RNGManager();
            var calculator = new DamageCalculator(rng);

            // Act
            int result = calculator.CalculateDamage(baseDamage: 100, modifier: -0.5f);

            // Assert
            Assert.Equal(-50, result);
        }

        [Fact]
        public void CalculateHealing_BelowMaxHealth_ReturnsFullHealing()
        {
            // Arrange
            var rng = new RNGManager();
            var calculator = new DamageCalculator(rng);
            var player = CreateTestPlayer();
            player.Health = 50;
            player.MaxHealth = 100;

            // Act
            int healing = calculator.CalculateHealing(baseHealing: 30, target: player);

            // Assert
            Assert.Equal(30, healing);
        }

        [Fact]
        public void CalculateHealing_AboveMaxHealth_ClampsToMax()
        {
            // Arrange
            var rng = new RNGManager();
            var calculator = new DamageCalculator(rng);
            var player = CreateTestPlayer();
            player.Health = 95;
            player.MaxHealth = 100;

            // Act
            int healing = calculator.CalculateHealing(baseHealing: 30, target: player);

            // Assert - Can only heal 5 HP to reach max
            Assert.Equal(5, healing);
        }

        [Fact]
        public void CalculateHealing_AtMaxHealth_ReturnsZero()
        {
            // Arrange
            var rng = new RNGManager();
            var calculator = new DamageCalculator(rng);
            var player = CreateTestPlayer();
            player.Health = 100;
            player.MaxHealth = 100;

            // Act
            int healing = calculator.CalculateHealing(baseHealing: 50, target: player);

            // Assert
            Assert.Equal(0, healing);
        }

        #endregion

        #region DamageResult Tests

        [Fact]
        public void DamageResult_DefaultConstructor_InitializesCorrectly()
        {
            // Act
            var result = new DamageResult();

            // Assert
            Assert.Equal(0, result.RawDamage);
            Assert.Equal(0, result.FinalDamage);
            Assert.Equal(0, result.DamageReduced);
            Assert.False(result.Missed);
            Assert.False(result.IsCritical);
        }

        [Fact]
        public void DamageResult_CanSetProperties()
        {
            // Arrange
            var result = new DamageResult();

            // Act
            result.RawDamage = 50;
            result.FinalDamage = 40;
            result.DamageReduced = 10;
            result.Missed = true;
            result.IsCritical = true;

            // Assert
            Assert.Equal(50, result.RawDamage);
            Assert.Equal(40, result.FinalDamage);
            Assert.Equal(10, result.DamageReduced);
            Assert.True(result.Missed);
            Assert.True(result.IsCritical);
        }

        #endregion

        #region Statistical Tests

        [Fact]
        public void CalculatePlayerAttackDamage_CritChance_MatchesExpectedRate()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            var calculator = new DamageCalculator(rng);
            var player = CreateTestPlayer();
            var enemy = CreateTestGoblin();
            var weapon = CreateTestWeapon("Sword", minDmg: 10, maxDmg: 10, accuracy: 100, critChance: 20); // 20% crit
            player.EquipWeapon(weapon);

            // Act - Perform 1000 attacks
            int critCount = 0;
            int totalHits = 0;
            for (int i = 0; i < 1000; i++)
            {
                var result = calculator.CalculatePlayerAttackDamage(player, enemy);
                if (!result.Missed)
                {
                    totalHits++;
                    if (result.IsCritical)
                        critCount++;
                }
            }

            // Assert - Crit rate should be around 20% (allow 10% variance)
            double critRate = (double)critCount / totalHits;
            Assert.InRange(critRate, 0.10, 0.30);
        }

        [Fact]
        public void CalculateEnemyAttackDamage_AccuracyRate_MatchesExpectedRate()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            var calculator = new DamageCalculator(rng);
            var enemy = CreateTestGoblin();
            enemy.Accuracy = 75; // 75% hit rate
            var player = CreateTestPlayer();

            // Act - Perform 1000 attacks
            int hitCount = 0;
            for (int i = 0; i < 1000; i++)
            {
                var result = calculator.CalculateEnemyAttackDamage(enemy, player);
                if (!result.Missed)
                    hitCount++;
            }

            // Assert - Hit rate should be around 75% (allow 10% variance)
            double hitRate = (double)hitCount / 1000;
            Assert.InRange(hitRate, 0.65, 0.85);
        }

        [Fact]
        public void CalculatePlayerAttackDamage_DamageRange_WithinExpectedBounds()
        {
            // Arrange
            var rng = new RNGManager(seed: 42);
            var calculator = new DamageCalculator(rng);
            var player = CreateTestPlayer(level: 1); // Level 1 to avoid level bonuses
            var enemy = CreateTestGoblin();
            var weapon = CreateTestWeapon("Sword", minDmg: 10, maxDmg: 20, accuracy: 100, critChance: 0); // No crits
            player.EquipWeapon(weapon);

            // Act - Perform 100 attacks
            var damages = new List<int>();
            for (int i = 0; i < 100; i++)
            {
                var result = calculator.CalculatePlayerAttackDamage(player, enemy);
                if (!result.Missed && !result.IsCritical)
                    damages.Add(result.FinalDamage);
            }

            // Assert - All damage should be in range 10-20 (weapon damage) + 0 (level 1 bonus)
            foreach (var damage in damages)
            {
                Assert.InRange(damage, 10, 20);
            }
        }

        #endregion
    }
}