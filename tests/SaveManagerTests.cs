using System;
using System.IO;
using Xunit;
using GameLogic.Data;
using GameLogic.Entities.Player;
using GameLogic.Items;

namespace GameLogic.Tests
{
    /// <summary>
    /// Unit tests for SaveManager
    /// Tests save/load functionality, file I/O, and data integrity
    /// </summary>
    public class SaveManagerTests : IDisposable
    {
        private readonly string _testSaveDirectory;
        private readonly string _originalSaveDirectory;

        public SaveManagerTests()
        {
            // Redirect save directory to a temp location for testing
            _testSaveDirectory = Path.Combine(Path.GetTempPath(), "RNGtheGame_Tests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testSaveDirectory);

            // Store original save directory using reflection
            var saveDirField = typeof(SaveManager).GetField("SaveDirectory",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            _originalSaveDirectory = (string)saveDirField.GetValue(null);

            // Set test directory
            saveDirField.SetValue(null, _testSaveDirectory);
        }

        public void Dispose()
        {
            // Restore original save directory
            var saveDirField = typeof(SaveManager).GetField("SaveDirectory",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            saveDirField.SetValue(null, _originalSaveDirectory);

            // Clean up test directory
            if (Directory.Exists(_testSaveDirectory))
            {
                Directory.Delete(_testSaveDirectory, recursive: true);
            }
        }

        #region Test Helper Methods

        private Player CreateTestPlayer(string name = "TestHero")
        {
            var player = new Player(name);
            player.Level = 10;
            player.Experience = 500;
            player.Health = 80;
            player.MaxHealth = 100;
            player.Gold = 250;
            player.PlayTime = TimeSpan.FromHours(5);
            return player;
        }

        private Weapon CreateTestWeapon()
        {
            return new Weapon("Test Sword", "A test weapon", 100, 10, 15, 85, 10, 1, 1);
        }

        private Armor CreateTestArmor()
        {
            return new Armor("Test Armor", "A test armor", 100, 5, 1, 1);
        }

        #endregion

        #region Initialization Tests

        [Fact]
        public void Initialize_CreatesDirectoryIfNotExists()
        {
            // Arrange - Delete test directory
            if (Directory.Exists(_testSaveDirectory))
            {
                Directory.Delete(_testSaveDirectory, recursive: true);
            }

            // Act
            SaveManager.Initialize();

            // Assert
            Assert.True(Directory.Exists(_testSaveDirectory));
        }

        [Fact]
        public void Initialize_DoesNotThrowIfDirectoryExists()
        {
            // Arrange
            Directory.CreateDirectory(_testSaveDirectory);

            // Act & Assert
            var exception = Record.Exception(() => SaveManager.Initialize());
            Assert.Null(exception);
        }

        #endregion

        #region Save Game Tests

        [Fact]
        public void SaveGame_ValidPlayer_ReturnsTrue()
        {
            // Arrange
            var player = CreateTestPlayer();

            // Act
            bool result = SaveManager.SaveGame(player, "test_save");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void SaveGame_CreatesFile()
        {
            // Arrange
            var player = CreateTestPlayer();

            // Act
            SaveManager.SaveGame(player, "test_save");

            // Assert
            string filePath = Path.Combine(_testSaveDirectory, "test_save.json");
            Assert.True(File.Exists(filePath));
        }

        [Fact]
        public void SaveGame_FileContainsPlayerData()
        {
            // Arrange
            var player = CreateTestPlayer("SavedHero");
            player.Gold = 999;

            // Act
            SaveManager.SaveGame(player, "test_save");

            // Assert
            string filePath = Path.Combine(_testSaveDirectory, "test_save.json");
            string content = File.ReadAllText(filePath);
            Assert.Contains("SavedHero", content);
            Assert.Contains("999", content);
        }

        [Fact]
        public void SaveGame_WithEquipment_SavesEquipmentNames()
        {
            // Arrange
            var player = CreateTestPlayer();
            player.EquipWeapon(CreateTestWeapon());
            player.EquipArmor(CreateTestArmor());

            // Act
            SaveManager.SaveGame(player, "test_save");

            // Assert
            string filePath = Path.Combine(_testSaveDirectory, "test_save.json");
            string content = File.ReadAllText(filePath);
            Assert.Contains("Test Sword", content);
            Assert.Contains("Test Armor", content);
        }

        [Fact]
        public void SaveGame_OverwritesExistingFile()
        {
            // Arrange
            var player1 = CreateTestPlayer("FirstHero");
            var player2 = CreateTestPlayer("SecondHero");

            // Act
            SaveManager.SaveGame(player1, "test_save");
            SaveManager.SaveGame(player2, "test_save");

            // Assert
            var loadedData = SaveManager.LoadGame("test_save");
            Assert.Equal("SecondHero", loadedData.PlayerName);
        }

        #endregion

        #region Load Game Tests

        [Fact]
        public void LoadGame_NonExistentFile_ReturnsNull()
        {
            // Act
            var result = SaveManager.LoadGame("nonexistent_save");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void LoadGame_ValidFile_ReturnsNotNull()
        {
            // Arrange
            var player = CreateTestPlayer();
            SaveManager.SaveGame(player, "test_save");

            // Act
            var result = SaveManager.LoadGame("test_save");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void LoadGame_RestoresPlayerName()
        {
            // Arrange
            var player = CreateTestPlayer("LoadTestHero");
            SaveManager.SaveGame(player, "test_save");

            // Act
            var saveData = SaveManager.LoadGame("test_save");

            // Assert
            Assert.Equal("LoadTestHero", saveData.PlayerName);
        }

        [Fact]
        public void LoadGame_RestoresAllStats()
        {
            // Arrange
            var player = CreateTestPlayer();
            player.Level = 15;
            player.Experience = 1000;
            player.Health = 75;
            player.MaxHealth = 120;
            player.Gold = 500;
            SaveManager.SaveGame(player, "test_save");

            // Act
            var saveData = SaveManager.LoadGame("test_save");

            // Assert
            Assert.Equal(15, saveData.Level);
            Assert.Equal(1000, saveData.Experience);
            Assert.Equal(75, saveData.Health);
            Assert.Equal(120, saveData.MaxHealth);
            Assert.Equal(500, saveData.Gold);
        }

        [Fact]
        public void LoadGame_RestoresEquipment()
        {
            // Arrange
            var player = CreateTestPlayer();
            player.EquipWeapon(CreateTestWeapon());
            player.EquipArmor(CreateTestArmor());
            SaveManager.SaveGame(player, "test_save");

            // Act
            var saveData = SaveManager.LoadGame("test_save");

            // Assert
            Assert.Equal("Test Sword", saveData.EquippedWeaponName);
            Assert.Equal("Test Armor", saveData.EquippedArmorName);
        }

        [Fact]
        public void LoadGame_RestoresPlayTime()
        {
            // Arrange
            var player = CreateTestPlayer();
            player.PlayTime = TimeSpan.FromHours(10.5);
            SaveManager.SaveGame(player, "test_save");

            // Act
            var saveData = SaveManager.LoadGame("test_save");

            // Assert
            Assert.Equal(TimeSpan.FromHours(10.5), saveData.PlayTime);
        }

        #endregion

        #region AutoSave/QuickSave Tests

        [Fact]
        public void AutoSave_SavesWithCorrectName()
        {
            // Arrange
            var player = CreateTestPlayer();

            // Act
            bool result = SaveManager.AutoSave(player);

            // Assert
            Assert.True(result);
            string filePath = Path.Combine(_testSaveDirectory, "autosave.json");
            Assert.True(File.Exists(filePath));
        }

        [Fact]
        public void QuickSave_SavesWithCorrectName()
        {
            // Arrange
            var player = CreateTestPlayer();

            // Act
            bool result = SaveManager.QuickSave(player);

            // Assert
            Assert.True(result);
            string filePath = Path.Combine(_testSaveDirectory, "quicksave.json");
            Assert.True(File.Exists(filePath));
        }

        [Fact]
        public void LoadAutoSave_LoadsAutosaveFile()
        {
            // Arrange
            var player = CreateTestPlayer("AutoSaveHero");
            SaveManager.AutoSave(player);

            // Act
            var saveData = SaveManager.LoadAutoSave();

            // Assert
            Assert.NotNull(saveData);
            Assert.Equal("AutoSaveHero", saveData.PlayerName);
        }

        [Fact]
        public void LoadQuickSave_LoadsQuicksaveFile()
        {
            // Arrange
            var player = CreateTestPlayer("QuickSaveHero");
            SaveManager.QuickSave(player);

            // Act
            var saveData = SaveManager.LoadQuickSave();

            // Assert
            Assert.NotNull(saveData);
            Assert.Equal("QuickSaveHero", saveData.PlayerName);
        }

        #endregion

        #region Save File Management Tests

        [Fact]
        public void SaveExists_ExistingFile_ReturnsTrue()
        {
            // Arrange
            var player = CreateTestPlayer();
            SaveManager.SaveGame(player, "test_save");

            // Act
            bool exists = SaveManager.SaveExists("test_save");

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public void SaveExists_NonExistentFile_ReturnsFalse()
        {
            // Act
            bool exists = SaveManager.SaveExists("nonexistent");

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public void DeleteSave_ExistingFile_ReturnsTrue()
        {
            // Arrange
            var player = CreateTestPlayer();
            SaveManager.SaveGame(player, "test_save");

            // Act
            bool result = SaveManager.DeleteSave("test_save");

            // Assert
            Assert.True(result);
            Assert.False(SaveManager.SaveExists("test_save"));
        }

        [Fact]
        public void DeleteSave_NonExistentFile_ReturnsFalse()
        {
            // Act
            bool result = SaveManager.DeleteSave("nonexistent");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetAllSaveFiles_ReturnsAllSaves()
        {
            // Arrange
            var player = CreateTestPlayer();
            SaveManager.SaveGame(player, "save1");
            SaveManager.SaveGame(player, "save2");
            SaveManager.SaveGame(player, "save3");

            // Act
            string[] saveFiles = SaveManager.GetAllSaveFiles();

            // Assert
            Assert.Contains("save1", saveFiles);
            Assert.Contains("save2", saveFiles);
            Assert.Contains("save3", saveFiles);
        }

        [Fact]
        public void GetAllSaveFiles_EmptyDirectory_ReturnsEmpty()
        {
            // Act
            string[] saveFiles = SaveManager.GetAllSaveFiles();

            // Assert
            Assert.Empty(saveFiles);
        }

        #endregion

        #region Save File Info Tests

        [Fact]
        public void GetSaveFileInfo_ValidSave_ReturnsInfo()
        {
            // Arrange
            var player = CreateTestPlayer("InfoHero");
            player.Level = 20;
            SaveManager.SaveGame(player, "test_save");

            // Act
            var info = SaveManager.GetSaveFileInfo("test_save");

            // Assert
            Assert.NotNull(info);
            Assert.Equal("test_save", info.SlotName);
            Assert.Equal("InfoHero", info.PlayerName);
            Assert.Equal(20, info.Level);
        }

        [Fact]
        public void GetSaveFileInfo_NonExistentSave_ReturnsNull()
        {
            // Act
            var info = SaveManager.GetSaveFileInfo("nonexistent");

            // Assert
            Assert.Null(info);
        }

        [Fact]
        public void SaveFileInfo_ToString_FormatsCorrectly()
        {
            // Arrange
            var player = CreateTestPlayer("DisplayHero");
            player.Level = 15;
            SaveManager.SaveGame(player, "test_save");

            // Act
            var info = SaveManager.GetSaveFileInfo("test_save");
            string formatted = info.ToString();

            // Assert
            Assert.Contains("test_save", formatted);
            Assert.Contains("DisplayHero", formatted);
            Assert.Contains("15", formatted);
        }

        #endregion

        #region Player.LoadFromSave Tests

        [Fact]
        public void PlayerLoadFromSave_RestoresPlayerState()
        {
            // Arrange
            var originalPlayer = CreateTestPlayer("LoadedHero");
            originalPlayer.Level = 25;
            originalPlayer.Gold = 1000;
            SaveManager.SaveGame(originalPlayer, "test_save");
            var saveData = SaveManager.LoadGame("test_save");

            // Act
            var loadedPlayer = Player.LoadFromSave(saveData);

            // Assert
            Assert.Equal("LoadedHero", loadedPlayer.Name);
            Assert.Equal(25, loadedPlayer.Level);
            Assert.Equal(1000, loadedPlayer.Gold);
        }

        [Fact]
        public void PlayerLoadFromSave_RestoresHealthAndExperience()
        {
            // Arrange
            var originalPlayer = CreateTestPlayer();
            originalPlayer.Experience = 2500;
            originalPlayer.Health = 60;
            originalPlayer.MaxHealth = 150;
            SaveManager.SaveGame(originalPlayer, "test_save");
            var saveData = SaveManager.LoadGame("test_save");

            // Act
            var loadedPlayer = Player.LoadFromSave(saveData);

            // Assert
            Assert.Equal(2500, loadedPlayer.Experience);
            Assert.Equal(60, loadedPlayer.Health);
            Assert.Equal(150, loadedPlayer.MaxHealth);
        }

        #endregion

        #region Data Integrity Tests

        [Fact]
        public void SaveLoad_RoundTrip_PreservesAllData()
        {
            // Arrange
            var original = CreateTestPlayer("RoundTripHero");
            original.Level = 30;
            original.Experience = 5000;
            original.Health = 90;
            original.MaxHealth = 200;
            original.Gold = 750;
            original.PlayTime = TimeSpan.FromHours(20);

            // Act
            SaveManager.SaveGame(original, "roundtrip");
            var saveData = SaveManager.LoadGame("roundtrip");
            var loaded = Player.LoadFromSave(saveData);

            // Assert - All critical data preserved
            Assert.Equal(original.Name, loaded.Name);
            Assert.Equal(original.Level, loaded.Level);
            Assert.Equal(original.Experience, loaded.Experience);
            Assert.Equal(original.Health, loaded.Health);
            Assert.Equal(original.MaxHealth, loaded.MaxHealth);
            Assert.Equal(original.Gold, loaded.Gold);
        }

        [Fact]
        public void SaveData_HasSaveDate()
        {
            // Arrange
            var player = CreateTestPlayer();
            var beforeSave = DateTime.Now.AddSeconds(-1);

            // Act
            SaveManager.SaveGame(player, "test_save");
            var saveData = SaveManager.LoadGame("test_save");
            var afterSave = DateTime.Now.AddSeconds(1);

            // Assert
            Assert.True(saveData.SaveDate >= beforeSave);
            Assert.True(saveData.SaveDate <= afterSave);
        }

        #endregion
    }
}