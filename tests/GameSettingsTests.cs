using System;
using Xunit;
using GameLogic.Systems;

namespace GameLogic.Tests
{
    /// <summary>
    /// Unit tests for GameSettings
    /// Tests settings initialization, modification, and difficulty system
    /// </summary>
    public class GameSettingsTests
    {
        #region Initialization Tests

        [Fact]
        public void Constructor_InitializesWithDefaultValues()
        {
            // Act
            var settings = new GameSettings();

            // Assert
            Assert.True(settings.ShowTurnOrderAtStartOfRound);
            Assert.True(settings.ShowDetailedCombatLog);
            Assert.False(settings.ShowDamageCalculations);
            Assert.True(settings.ShowEnemyStatBlock);
            Assert.True(settings.AutoSaveEnabled);
            Assert.Equal(5, settings.AutoSaveIntervalMinutes);
            Assert.True(settings.ConfirmFleeAction);
            Assert.False(settings.ConfirmConsumeItem);
            Assert.Equal("system", settings.RngAlgorithm);
            Assert.False(settings.RngStatisticsTracking);
            Assert.True(settings.ColoredText);
            Assert.True(settings.UseEmojis);
            Assert.Equal(0, settings.TextSpeed);
            Assert.True(settings.SoundEffectsEnabled);
            Assert.Equal(70, settings.SoundEffectsVolume);
            Assert.True(settings.MusicEnabled);
            Assert.Equal(50, settings.MusicVolume);
            Assert.Equal(DifficultyLevel.Normal, settings.Difficulty);
        }

        #endregion

        #region Display Settings Tests

        [Fact]
        public void ShowTurnOrderAtStartOfRound_CanBeModified()
        {
            // Arrange
            var settings = new GameSettings();

            // Act
            settings.ShowTurnOrderAtStartOfRound = false;

            // Assert
            Assert.False(settings.ShowTurnOrderAtStartOfRound);
        }

        [Fact]
        public void ShowDetailedCombatLog_CanBeModified()
        {
            // Arrange
            var settings = new GameSettings();

            // Act
            settings.ShowDetailedCombatLog = false;

            // Assert
            Assert.False(settings.ShowDetailedCombatLog);
        }

        [Fact]
        public void ShowDamageCalculations_CanBeModified()
        {
            // Arrange
            var settings = new GameSettings();

            // Act
            settings.ShowDamageCalculations = true;

            // Assert
            Assert.True(settings.ShowDamageCalculations);
        }

        [Fact]
        public void ShowEnemyStatBlock_CanBeModified()
        {
            // Arrange
            var settings = new GameSettings();

            // Act
            settings.ShowEnemyStatBlock = false;

            // Assert
            Assert.False(settings.ShowEnemyStatBlock);
        }

        #endregion

        #region Gameplay Settings Tests

        [Fact]
        public void AutoSaveEnabled_CanBeModified()
        {
            // Arrange
            var settings = new GameSettings();

            // Act
            settings.AutoSaveEnabled = false;

            // Assert
            Assert.False(settings.AutoSaveEnabled);
        }

        [Fact]
        public void AutoSaveIntervalMinutes_CanBeModified()
        {
            // Arrange
            var settings = new GameSettings();

            // Act
            settings.AutoSaveIntervalMinutes = 10;

            // Assert
            Assert.Equal(10, settings.AutoSaveIntervalMinutes);
        }

        [Fact]
        public void ConfirmFleeAction_CanBeModified()
        {
            // Arrange
            var settings = new GameSettings();

            // Act
            settings.ConfirmFleeAction = false;

            // Assert
            Assert.False(settings.ConfirmFleeAction);
        }

        [Fact]
        public void ConfirmConsumeItem_CanBeModified()
        {
            // Arrange
            var settings = new GameSettings();

            // Act
            settings.ConfirmConsumeItem = true;

            // Assert
            Assert.True(settings.ConfirmConsumeItem);
        }

        #endregion

        #region RNG Settings Tests

        [Fact]
        public void RngAlgorithm_CanBeModified()
        {
            // Arrange
            var settings = new GameSettings();

            // Act
            settings.RngAlgorithm = "mersenne";

            // Assert
            Assert.Equal("mersenne", settings.RngAlgorithm);
        }

        [Fact]
        public void RngStatisticsTracking_CanBeModified()
        {
            // Arrange
            var settings = new GameSettings();

            // Act
            settings.RngStatisticsTracking = true;

            // Assert
            Assert.True(settings.RngStatisticsTracking);
        }

        #endregion

        #region Accessibility Settings Tests

        [Fact]
        public void ColoredText_CanBeModified()
        {
            // Arrange
            var settings = new GameSettings();

            // Act
            settings.ColoredText = false;

            // Assert
            Assert.False(settings.ColoredText);
        }

        [Fact]
        public void UseEmojis_CanBeModified()
        {
            // Arrange
            var settings = new GameSettings();

            // Act
            settings.UseEmojis = false;

            // Assert
            Assert.False(settings.UseEmojis);
        }

        [Fact]
        public void TextSpeed_CanBeModified()
        {
            // Arrange
            var settings = new GameSettings();

            // Act
            settings.TextSpeed = 3;

            // Assert
            Assert.Equal(3, settings.TextSpeed);
        }

        #endregion

        #region Audio Settings Tests

        [Fact]
        public void SoundEffectsEnabled_CanBeModified()
        {
            // Arrange
            var settings = new GameSettings();

            // Act
            settings.SoundEffectsEnabled = false;

            // Assert
            Assert.False(settings.SoundEffectsEnabled);
        }

        [Fact]
        public void SoundEffectsVolume_CanBeModified()
        {
            // Arrange
            var settings = new GameSettings();

            // Act
            settings.SoundEffectsVolume = 100;

            // Assert
            Assert.Equal(100, settings.SoundEffectsVolume);
        }

        [Fact]
        public void MusicEnabled_CanBeModified()
        {
            // Arrange
            var settings = new GameSettings();

            // Act
            settings.MusicEnabled = false;

            // Assert
            Assert.False(settings.MusicEnabled);
        }

        [Fact]
        public void MusicVolume_CanBeModified()
        {
            // Arrange
            var settings = new GameSettings();

            // Act
            settings.MusicVolume = 25;

            // Assert
            Assert.Equal(25, settings.MusicVolume);
        }

        #endregion

        #region Difficulty Settings Tests

        [Fact]
        public void Difficulty_CanBeSet()
        {
            // Arrange
            var settings = new GameSettings();

            // Act
            settings.Difficulty = DifficultyLevel.Hard;

            // Assert
            Assert.Equal(DifficultyLevel.Hard, settings.Difficulty);
        }

        [Theory]
        [InlineData(DifficultyLevel.Easy, 0.75f)]
        [InlineData(DifficultyLevel.Normal, 1.0f)]
        [InlineData(DifficultyLevel.Hard, 1.5f)]
        [InlineData(DifficultyLevel.VeryHard, 2.0f)]
        public void GetDifficultyMultiplier_ReturnsCorrectValue(DifficultyLevel difficulty, float expected)
        {
            // Arrange
            var settings = new GameSettings { Difficulty = difficulty };

            // Act
            float multiplier = settings.GetDifficultyMultiplier();

            // Assert
            Assert.Equal(expected, multiplier);
        }

        [Theory]
        [InlineData(DifficultyLevel.Easy, 0.8f)]
        [InlineData(DifficultyLevel.Normal, 1.0f)]
        [InlineData(DifficultyLevel.Hard, 1.3f)]
        [InlineData(DifficultyLevel.VeryHard, 1.5f)]
        public void GetRewardMultiplier_ReturnsCorrectValue(DifficultyLevel difficulty, float expected)
        {
            // Arrange
            var settings = new GameSettings { Difficulty = difficulty };

            // Act
            float multiplier = settings.GetRewardMultiplier();

            // Assert
            Assert.Equal(expected, multiplier);
        }

        #endregion

        #region Reset To Defaults Tests

        [Fact]
        public void ResetToDefaults_ResetsDisplaySettings()
        {
            // Arrange
            var settings = new GameSettings();
            settings.ShowTurnOrderAtStartOfRound = false;
            settings.ShowDetailedCombatLog = false;
            settings.ShowDamageCalculations = true;
            settings.ShowEnemyStatBlock = false;

            // Act
            settings.ResetToDefaults();

            // Assert
            Assert.True(settings.ShowTurnOrderAtStartOfRound);
            Assert.True(settings.ShowDetailedCombatLog);
            Assert.False(settings.ShowDamageCalculations);
            Assert.True(settings.ShowEnemyStatBlock);
        }

        [Fact]
        public void ResetToDefaults_ResetsGameplaySettings()
        {
            // Arrange
            var settings = new GameSettings();
            settings.AutoSaveEnabled = false;
            settings.AutoSaveIntervalMinutes = 20;
            settings.ConfirmFleeAction = false;
            settings.ConfirmConsumeItem = true;

            // Act
            settings.ResetToDefaults();

            // Assert
            Assert.True(settings.AutoSaveEnabled);
            Assert.Equal(5, settings.AutoSaveIntervalMinutes);
            Assert.True(settings.ConfirmFleeAction);
            Assert.False(settings.ConfirmConsumeItem);
        }

        [Fact]
        public void ResetToDefaults_ResetsRNGSettings()
        {
            // Arrange
            var settings = new GameSettings();
            settings.RngAlgorithm = "mersenne";
            settings.RngStatisticsTracking = true;

            // Act
            settings.ResetToDefaults();

            // Assert
            Assert.Equal("system", settings.RngAlgorithm);
            Assert.False(settings.RngStatisticsTracking);
        }

        [Fact]
        public void ResetToDefaults_ResetsAccessibilitySettings()
        {
            // Arrange
            var settings = new GameSettings();
            settings.ColoredText = false;
            settings.UseEmojis = false;
            settings.TextSpeed = 5;

            // Act
            settings.ResetToDefaults();

            // Assert
            Assert.True(settings.ColoredText);
            Assert.True(settings.UseEmojis);
            Assert.Equal(0, settings.TextSpeed);
        }

        [Fact]
        public void ResetToDefaults_ResetsAudioSettings()
        {
            // Arrange
            var settings = new GameSettings();
            settings.SoundEffectsEnabled = false;
            settings.SoundEffectsVolume = 0;
            settings.MusicEnabled = false;
            settings.MusicVolume = 0;

            // Act
            settings.ResetToDefaults();

            // Assert
            Assert.True(settings.SoundEffectsEnabled);
            Assert.Equal(70, settings.SoundEffectsVolume);
            Assert.True(settings.MusicEnabled);
            Assert.Equal(50, settings.MusicVolume);
        }

        [Fact]
        public void ResetToDefaults_DoesNotResetDifficulty()
        {
            // Arrange
            var settings = new GameSettings();
            settings.Difficulty = DifficultyLevel.Hard;
            settings.ShowTurnOrderAtStartOfRound = false;

            // Act
            settings.ResetToDefaults();

            // Assert
            Assert.Equal(DifficultyLevel.Hard, settings.Difficulty); // Difficulty should not change
            Assert.True(settings.ShowTurnOrderAtStartOfRound); // Other settings should reset
        }

        #endregion

        #region Clone Tests

        [Fact]
        public void Clone_CreatesIndependentCopy()
        {
            // Arrange
            var original = new GameSettings();
            original.ShowTurnOrderAtStartOfRound = false;
            original.AutoSaveIntervalMinutes = 10;
            original.Difficulty = DifficultyLevel.Hard;

            // Act
            var clone = original.Clone();
            clone.ShowTurnOrderAtStartOfRound = true;
            clone.AutoSaveIntervalMinutes = 20;

            // Assert
            Assert.False(original.ShowTurnOrderAtStartOfRound);
            Assert.Equal(10, original.AutoSaveIntervalMinutes);
            Assert.True(clone.ShowTurnOrderAtStartOfRound);
            Assert.Equal(20, clone.AutoSaveIntervalMinutes);
            Assert.Equal(DifficultyLevel.Hard, clone.Difficulty);
        }

        [Fact]
        public void Clone_CopiesAllSettings()
        {
            // Arrange
            var original = new GameSettings
            {
                ShowTurnOrderAtStartOfRound = false,
                ShowDetailedCombatLog = false,
                ShowDamageCalculations = true,
                ShowEnemyStatBlock = false,
                AutoSaveEnabled = false,
                AutoSaveIntervalMinutes = 15,
                ConfirmFleeAction = false,
                ConfirmConsumeItem = true,
                RngAlgorithm = "mersenne",
                RngStatisticsTracking = true,
                ColoredText = false,
                UseEmojis = false,
                TextSpeed = 3,
                SoundEffectsEnabled = false,
                SoundEffectsVolume = 100,
                MusicEnabled = false,
                MusicVolume = 25,
                Difficulty = DifficultyLevel.VeryHard
            };

            // Act
            var clone = original.Clone();

            // Assert
            Assert.Equal(original.ShowTurnOrderAtStartOfRound, clone.ShowTurnOrderAtStartOfRound);
            Assert.Equal(original.ShowDetailedCombatLog, clone.ShowDetailedCombatLog);
            Assert.Equal(original.ShowDamageCalculations, clone.ShowDamageCalculations);
            Assert.Equal(original.ShowEnemyStatBlock, clone.ShowEnemyStatBlock);
            Assert.Equal(original.AutoSaveEnabled, clone.AutoSaveEnabled);
            Assert.Equal(original.AutoSaveIntervalMinutes, clone.AutoSaveIntervalMinutes);
            Assert.Equal(original.ConfirmFleeAction, clone.ConfirmFleeAction);
            Assert.Equal(original.ConfirmConsumeItem, clone.ConfirmConsumeItem);
            Assert.Equal(original.RngAlgorithm, clone.RngAlgorithm);
            Assert.Equal(original.RngStatisticsTracking, clone.RngStatisticsTracking);
            Assert.Equal(original.ColoredText, clone.ColoredText);
            Assert.Equal(original.UseEmojis, clone.UseEmojis);
            Assert.Equal(original.TextSpeed, clone.TextSpeed);
            Assert.Equal(original.SoundEffectsEnabled, clone.SoundEffectsEnabled);
            Assert.Equal(original.SoundEffectsVolume, clone.SoundEffectsVolume);
            Assert.Equal(original.MusicEnabled, clone.MusicEnabled);
            Assert.Equal(original.MusicVolume, clone.MusicVolume);
            Assert.Equal(original.Difficulty, clone.Difficulty);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void DifficultyMultipliers_AffectGameplay()
        {
            // Arrange - Simulate enemy with 100 HP on different difficulties
            int baseEnemyHP = 100;
            var easySettings = new GameSettings { Difficulty = DifficultyLevel.Easy };
            var normalSettings = new GameSettings { Difficulty = DifficultyLevel.Normal };
            var hardSettings = new GameSettings { Difficulty = DifficultyLevel.Hard };
            var veryHardSettings = new GameSettings { Difficulty = DifficultyLevel.VeryHard };

            // Act
            int easyHP = (int)(baseEnemyHP * easySettings.GetDifficultyMultiplier());
            int normalHP = (int)(baseEnemyHP * normalSettings.GetDifficultyMultiplier());
            int hardHP = (int)(baseEnemyHP * hardSettings.GetDifficultyMultiplier());
            int veryHardHP = (int)(baseEnemyHP * veryHardSettings.GetDifficultyMultiplier());

            // Assert
            Assert.Equal(75, easyHP);      // 75% of 100
            Assert.Equal(100, normalHP);   // 100% of 100
            Assert.Equal(150, hardHP);     // 150% of 100
            Assert.Equal(200, veryHardHP); // 200% of 100
        }

        [Fact]
        public void RewardMultipliers_AffectLoot()
        {
            // Arrange - Simulate quest reward of 100 gold on different difficulties
            int baseReward = 100;
            var easySettings = new GameSettings { Difficulty = DifficultyLevel.Easy };
            var normalSettings = new GameSettings { Difficulty = DifficultyLevel.Normal };
            var hardSettings = new GameSettings { Difficulty = DifficultyLevel.Hard };
            var veryHardSettings = new GameSettings { Difficulty = DifficultyLevel.VeryHard };

            // Act
            int easyReward = (int)(baseReward * easySettings.GetRewardMultiplier());
            int normalReward = (int)(baseReward * normalSettings.GetRewardMultiplier());
            int hardReward = (int)(baseReward * hardSettings.GetRewardMultiplier());
            int veryHardReward = (int)(baseReward * veryHardSettings.GetRewardMultiplier());

            // Assert
            Assert.Equal(80, easyReward);      // 80% of 100
            Assert.Equal(100, normalReward);   // 100% of 100
            Assert.Equal(130, hardReward);     // 130% of 100
            Assert.Equal(150, veryHardReward); // 150% of 100
        }

        #endregion
    }
}