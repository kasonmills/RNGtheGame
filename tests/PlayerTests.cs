using System;
using Xunit;
using GameLogic.Entities.Player;
using GameLogic.Items;

namespace GameLogic.Tests
{
    /// <summary>
    /// Unit tests for Player class
    /// Tests player initialization, leveling, combat, inventory, and equipment
    /// </summary>
    public class PlayerTests
    {
        #region Test Helper Methods

        private Player CreateTestPlayer(string name = "TestHero")
        {
            return new Player(name);
        }

        private Weapon CreateTestWeapon(string name = "Test Sword")
        {
            return new Weapon(name, "A test weapon", 100, 10, 15, 85, 10, 1, 1);
        }

        private Armor CreateTestArmor(string name = "Test Armor")
        {
            return new Armor(name, "A test armor", 100, 5, 1, 1);
        }

        private Item CreateTestItem(string name = "Test Item")
        {
            return new Item(name, "A test item", 50, 1, 1);
        }

        #endregion

        #region Initialization Tests

        [Fact]
        public void Constructor_SetsName()
        {
            // Act
            var player = new Player("Hero");

            // Assert
            Assert.Equal("Hero", player.Name);
        }

        [Fact]
        public void Constructor_StartsAtLevelOne()
        {
            // Act
            var player = CreateTestPlayer();

            // Assert
            Assert.Equal(1, player.Level);
        }

        [Fact]
        public void Constructor_StartsWithZeroExperience()
        {
            // Act
            var player = CreateTestPlayer();

            // Assert
            Assert.Equal(0, player.Experience);
        }

        [Fact]
        public void Constructor_StartsWithFullHealth()
        {
            // Act
            var player = CreateTestPlayer();

            // Assert
            Assert.Equal(player.MaxHealth, player.Health);
            Assert.True(player.Health > 0);
        }

        [Fact]
        public void Constructor_StartsWithZeroGold()
        {
            // Act
            var player = CreateTestPlayer();

            // Assert
            Assert.Equal(0, player.Gold);
        }

        [Fact]
        public void Constructor_InitializesEmptyInventory()
        {
            // Act
            var player = CreateTestPlayer();

            // Assert
            Assert.NotNull(player.Inventory);
            Assert.Empty(player.Inventory);
        }

        [Fact]
        public void Constructor_InitializesEmptyAbilitiesList()
        {
            // Act
            var player = CreateTestPlayer();

            // Assert
            Assert.NotNull(player.Abilities);
            Assert.Empty(player.Abilities);
        }

        [Fact]
        public void Constructor_StartsWithNoEquipment()
        {
            // Act
            var player = CreateTestPlayer();

            // Assert
            Assert.Null(player.EquippedWeapon);
            Assert.Null(player.EquippedArmor);
        }

        [Fact]
        public void Constructor_InheritsFromEntity()
        {
            // Act
            var player = CreateTestPlayer();

            // Assert - Should have Entity properties
            Assert.NotNull(player.ActiveEffects);
            Assert.IsAssignableFrom<GameLogic.Entities.Entity>(player);
        }

        #endregion

        #region Health and Damage Tests

        [Fact]
        public void TakeDamage_ReducesHealth()
        {
            // Arrange
            var player = CreateTestPlayer();
            int initialHealth = player.Health;

            // Act
            player.TakeDamage(10);

            // Assert
            Assert.Equal(initialHealth - 10, player.Health);
        }

        [Fact]
        public void TakeDamage_CannotGoNegative()
        {
            // Arrange
            var player = CreateTestPlayer();

            // Act
            player.TakeDamage(9999);

            // Assert
            Assert.Equal(0, player.Health);
        }

        [Fact]
        public void Heal_IncreasesHealth()
        {
            // Arrange
            var player = CreateTestPlayer();
            player.Health = player.MaxHealth / 2;

            // Act
            player.Heal(10);

            // Assert
            Assert.Equal((player.MaxHealth / 2) + 10, player.Health);
        }

        [Fact]
        public void Heal_CannotExceedMaxHealth()
        {
            // Arrange
            var player = CreateTestPlayer();

            // Act
            player.Heal(9999);

            // Assert
            Assert.Equal(player.MaxHealth, player.Health);
        }

        [Fact]
        public void IsAlive_HealthAboveZero_ReturnsTrue()
        {
            // Arrange
            var player = CreateTestPlayer();
            player.Health = 1;

            // Act
            bool isAlive = player.IsAlive();

            // Assert
            Assert.True(isAlive);
        }

        [Fact]
        public void IsAlive_HealthZero_ReturnsFalse()
        {
            // Arrange
            var player = CreateTestPlayer();
            player.Health = 0;

            // Act
            bool isAlive = player.IsAlive();

            // Assert
            Assert.False(isAlive);
        }

        #endregion

        #region Experience and Leveling Tests

        [Fact]
        public void AddExperience_IncreasesExperience()
        {
            // Arrange
            var player = CreateTestPlayer();

            // Act
            player.AddExperience(50);

            // Assert
            Assert.Equal(50, player.Experience);
        }

        [Fact]
        public void AddExperience_EnoughXP_LevelsUp()
        {
            // Arrange
            var player = CreateTestPlayer();
            int initialLevel = player.Level;

            // Act - Add enough XP to level up (120 for level 1->2)
            player.AddExperience(120);

            // Assert
            Assert.Equal(initialLevel + 1, player.Level);
        }

        [Fact]
        public void AddExperience_LevelUp_IncreasesMaxHealth()
        {
            // Arrange
            var player = CreateTestPlayer();
            int initialMaxHealth = player.MaxHealth;

            // Act
            player.AddExperience(120); // Level up

            // Assert
            Assert.True(player.MaxHealth > initialMaxHealth);
        }

        [Fact]
        public void AddExperience_LevelUp_FullHealsPlayer()
        {
            // Arrange
            var player = CreateTestPlayer();
            player.Health = 1; // Near death

            // Act
            player.AddExperience(120); // Level up

            // Assert
            Assert.Equal(player.MaxHealth, player.Health);
        }

        [Fact]
        public void AddExperience_MultipleLevelUps_LevelsUpMultipleTimes()
        {
            // Arrange
            var player = CreateTestPlayer();

            // Act - Add massive XP
            player.AddExperience(10000);

            // Assert - Should be much higher level
            Assert.True(player.Level > 5);
        }

        [Fact]
        public void AddExperience_ReducesExcessXP()
        {
            // Arrange
            var player = CreateTestPlayer();

            // Act
            player.AddExperience(150); // 120 needed for level 2, 30 excess

            // Assert - Should have leftover XP
            Assert.True(player.Experience >= 0);
            Assert.Equal(2, player.Level);
        }

        #endregion

        #region Inventory Tests

        [Fact]
        public void AddToInventory_AddsItem()
        {
            // Arrange
            var player = CreateTestPlayer();
            var item = CreateTestItem();

            // Act
            player.AddToInventory(item);

            // Assert
            Assert.Contains(item, player.Inventory);
            Assert.Single(player.Inventory);
        }

        [Fact]
        public void AddToInventory_MultipleItems_AddsAll()
        {
            // Arrange
            var player = CreateTestPlayer();
            var item1 = CreateTestItem("Item 1");
            var item2 = CreateTestItem("Item 2");
            var item3 = CreateTestItem("Item 3");

            // Act
            player.AddToInventory(item1);
            player.AddToInventory(item2);
            player.AddToInventory(item3);

            // Assert
            Assert.Equal(3, player.Inventory.Count);
            Assert.Contains(item1, player.Inventory);
            Assert.Contains(item2, player.Inventory);
            Assert.Contains(item3, player.Inventory);
        }

        #endregion

        #region Equipment Tests

        [Fact]
        public void EquipWeapon_SetsEquippedWeapon()
        {
            // Arrange
            var player = CreateTestPlayer();
            var weapon = CreateTestWeapon();

            // Act
            player.EquipWeapon(weapon);

            // Assert
            Assert.Equal(weapon, player.EquippedWeapon);
        }

        [Fact]
        public void EquipWeapon_ReplacesOldWeapon()
        {
            // Arrange
            var player = CreateTestPlayer();
            var weapon1 = CreateTestWeapon("Sword 1");
            var weapon2 = CreateTestWeapon("Sword 2");

            // Act
            player.EquipWeapon(weapon1);
            player.EquipWeapon(weapon2);

            // Assert
            Assert.Equal(weapon2, player.EquippedWeapon);
        }

        [Fact]
        public void EquipArmor_SetsEquippedArmor()
        {
            // Arrange
            var player = CreateTestPlayer();
            var armor = CreateTestArmor();

            // Act
            player.EquipArmor(armor);

            // Assert
            Assert.Equal(armor, player.EquippedArmor);
        }

        [Fact]
        public void EquipArmor_ReplacesOldArmor()
        {
            // Arrange
            var player = CreateTestPlayer();
            var armor1 = CreateTestArmor("Armor 1");
            var armor2 = CreateTestArmor("Armor 2");

            // Act
            player.EquipArmor(armor1);
            player.EquipArmor(armor2);

            // Assert
            Assert.Equal(armor2, player.EquippedArmor);
        }

        [Fact]
        public void EquipWeapon_CanEquipMultipleItemTypes()
        {
            // Arrange
            var player = CreateTestPlayer();
            var weapon = CreateTestWeapon();
            var armor = CreateTestArmor();

            // Act
            player.EquipWeapon(weapon);
            player.EquipArmor(armor);

            // Assert
            Assert.Equal(weapon, player.EquippedWeapon);
            Assert.Equal(armor, player.EquippedArmor);
        }

        #endregion

        #region Gold Tests

        [Fact]
        public void Gold_CanBeIncreased()
        {
            // Arrange
            var player = CreateTestPlayer();

            // Act
            player.Gold += 100;

            // Assert
            Assert.Equal(100, player.Gold);
        }

        [Fact]
        public void Gold_CanBeDecreased()
        {
            // Arrange
            var player = CreateTestPlayer();
            player.Gold = 100;

            // Act
            player.Gold -= 50;

            // Assert
            Assert.Equal(50, player.Gold);
        }

        #endregion

        #region Entity Methods Tests

        [Fact]
        public void Execute_DoesNotThrow()
        {
            // Arrange
            var player = CreateTestPlayer();

            // Act & Assert
            var exception = Record.Exception(() => player.Execute());
            Assert.Null(exception);
        }

        [Fact]
        public void HasEffect_NoEffects_ReturnsFalse()
        {
            // Arrange
            var player = CreateTestPlayer();

            // Act
            bool hasEffect = player.HasEffect<GameLogic.Abilities.AbilityEffect>();

            // Assert
            Assert.False(hasEffect);
        }

        [Fact]
        public void ActiveEffects_InitiallyEmpty()
        {
            // Arrange
            var player = CreateTestPlayer();

            // Act & Assert
            Assert.Empty(player.ActiveEffects);
        }

        #endregion

        #region PlayTime Tests

        [Fact]
        public void PlayTime_InitiallyZero()
        {
            // Arrange & Act
            var player = CreateTestPlayer();

            // Assert
            Assert.Equal(TimeSpan.Zero, player.PlayTime);
        }

        [Fact]
        public void PlayTime_CanBeSet()
        {
            // Arrange
            var player = CreateTestPlayer();
            var playTime = TimeSpan.FromHours(5);

            // Act
            player.PlayTime = playTime;

            // Assert
            Assert.Equal(playTime, player.PlayTime);
        }

        #endregion
    }
}