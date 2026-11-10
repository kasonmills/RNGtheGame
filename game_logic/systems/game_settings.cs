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
        public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Hard;

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

        // Enemy-strength and reward multipliers per difficulty live in DifficultyScaler (and the
        // XP numbers in LevelingSystem) - the old unused copies here were removed 2026-11-10.

        /// <summary>
        /// Hit chance taken off the player's and companions' attacks on each difficulty
        /// (percentage points): Normal 0, Hard 7, Difficult 14, Unfair 21. Enemies are not
        /// affected, and Boon is exempt (decided 2026-10-21).
        /// </summary>
        public static int GetPartyAccuracyPenalty(DifficultyLevel difficulty)
        {
            return difficulty switch
            {
                DifficultyLevel.Normal => 0,
                DifficultyLevel.Hard => 7,
                DifficultyLevel.Difficult => 14,
                DifficultyLevel.Unfair => 21,
                _ => 0
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
        // Numbers in DifficultyScaler / LevelingSystem / GetPartyAccuracyPenalty:
        // enemy health+damage, XP, gold, party hit chance
        Normal,     // 75%,  80% XP,  100% gold, -0 hit chance
        Hard,       // 100%, 100% XP, 100% gold, -7  (the 1x baseline)
        Difficult,  // 150%, 125% XP, 120% gold, -14
        Unfair      // 200%, 150% XP, 140% gold, -21
    }
}