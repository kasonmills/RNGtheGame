using System;

namespace GameLogic.Systems
{
    /// <summary>
    /// Stores all configurable game settings
    /// </summary>
    public class GameSettings
    {
        // ===== Display Settings =====
        public bool ShowTurnOrderAtStartOfRound { get; set; } = true;
        public bool ShowDetailedCombatLog { get; set; } = true;
        public bool ShowDamageCalculations { get; set; } = false;
        public bool ShowEnemyStatBlock { get; set; } = true;

        // ===== Gameplay Settings =====
        public bool AutoSaveEnabled { get; set; } = true;
        public int AutoSaveIntervalMinutes { get; set; } = 5;
        public bool ConfirmFleeAction { get; set; } = true;
        public bool ConfirmConsumeItem { get; set; } = false;

        // ===== RNG Settings =====
        public string RngAlgorithm { get; set; } = "system";
        public bool RngStatisticsTracking { get; set; } = false;

        // ===== Accessibility Settings =====
        public bool ColoredText { get; set; } = true;
        public bool UseEmojis { get; set; } = true;
        public int TextSpeed { get; set; } = 0; // 0 = instant, 1-5 = delay

        // ===== Audio Settings (for future implementation) =====
        public bool SoundEffectsEnabled { get; set; } = true;
        public int SoundEffectsVolume { get; set; } = 70; // 0-100
        public bool MusicEnabled { get; set; } = true;
        public int MusicVolume { get; set; } = 50; // 0-100

        // ===== Difficulty Settings (set once at save creation, immutable after) =====
        /// <summary>
        /// Difficulty level - set once at save file creation and cannot be changed
        /// </summary>
        public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Normal;

        /// <summary>
        /// Create default settings
        /// </summary>
        public GameSettings()
        {
            // Defaults are set via property initialization above
        }

        /// <summary>
        /// Reset adjustable settings to default values (does NOT reset difficulty)
        /// </summary>
        public void ResetToDefaults()
        {
            ShowTurnOrderAtStartOfRound = true;
            ShowDetailedCombatLog = true;
            ShowDamageCalculations = false;
            ShowEnemyStatBlock = true;

            AutoSaveEnabled = true;
            AutoSaveIntervalMinutes = 5;
            ConfirmFleeAction = true;
            ConfirmConsumeItem = false;

            RngAlgorithm = "system";
            RngStatisticsTracking = false;

            ColoredText = true;
            UseEmojis = true;
            TextSpeed = 0;

            SoundEffectsEnabled = true;
            SoundEffectsVolume = 70;
            MusicEnabled = true;
            MusicVolume = 50;

            // NOTE: Difficulty is NOT reset - it's immutable after save creation
        }

        /// <summary>
        /// Get difficulty multiplier for various stats
        /// </summary>
        public float GetDifficultyMultiplier()
        {
            return Difficulty switch
            {
                DifficultyLevel.Easy => 0.75f,
                DifficultyLevel.Normal => 1.0f,
                DifficultyLevel.Hard => 1.5f,
                DifficultyLevel.VeryHard => 2.0f,
                _ => 1.0f
            };
        }

        /// <summary>
        /// Get gold/XP reward multiplier based on difficulty
        /// </summary>
        public float GetRewardMultiplier()
        {
            return Difficulty switch
            {
                DifficultyLevel.Easy => 0.8f,
                DifficultyLevel.Normal => 1.0f,
                DifficultyLevel.Hard => 1.3f,
                DifficultyLevel.VeryHard => 1.5f,
                _ => 1.0f
            };
        }

        /// <summary>
        /// Clone settings
        /// </summary>
        public GameSettings Clone()
        {
            return new GameSettings
            {
                ShowTurnOrderAtStartOfRound = this.ShowTurnOrderAtStartOfRound,
                ShowDetailedCombatLog = this.ShowDetailedCombatLog,
                ShowDamageCalculations = this.ShowDamageCalculations,
                ShowEnemyStatBlock = this.ShowEnemyStatBlock,

                AutoSaveEnabled = this.AutoSaveEnabled,
                AutoSaveIntervalMinutes = this.AutoSaveIntervalMinutes,
                ConfirmFleeAction = this.ConfirmFleeAction,
                ConfirmConsumeItem = this.ConfirmConsumeItem,

                RngAlgorithm = this.RngAlgorithm,
                RngStatisticsTracking = this.RngStatisticsTracking,

                ColoredText = this.ColoredText,
                UseEmojis = this.UseEmojis,
                TextSpeed = this.TextSpeed,

                SoundEffectsEnabled = this.SoundEffectsEnabled,
                SoundEffectsVolume = this.SoundEffectsVolume,
                MusicEnabled = this.MusicEnabled,
                MusicVolume = this.MusicVolume,

                Difficulty = this.Difficulty
            };
        }
    }

    /// <summary>
    /// Game difficulty levels - set once at save creation, immutable after
    /// </summary>
    public enum DifficultyLevel
    {
        Easy,       // 75% enemy stats, 80% rewards
        Normal,     // 100% enemy stats, 100% rewards
        Hard,       // 150% enemy stats, 130% rewards
        VeryHard    // 200% enemy stats, 150% rewards
    }
}