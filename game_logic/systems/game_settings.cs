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
        /// (percentage points): Normal 0, Hard 5, Difficult 10, Unfair 15 (decided 2026-11-23;
        /// was 0/7/14/21). Enemies are not affected, and Boon is exempt with his own crossbow.
        /// </summary>
        public static int GetPartyAccuracyPenalty(DifficultyLevel difficulty)
        {
            return difficulty switch
            {
                DifficultyLevel.Normal => 0,
                DifficultyLevel.Hard => 5,
                DifficultyLevel.Difficult => 10,
                DifficultyLevel.Unfair => 15,
                _ => 0
            };
        }

        /// <summary>
        /// The party's max health on each difficulty (decided 2026-11-23): Normal 120%, Hard 100%,
        /// Difficult 90%, Unfair 80% - for the player and every companion, on top of the enemy
        /// scaling. It's a %, so the gap lasts at every level (Entity.MaxHealthMultiplier).
        /// </summary>
        public static double GetPartyMaxHealthMultiplier(DifficultyLevel difficulty)
        {
            return difficulty switch
            {
                DifficultyLevel.Normal => 1.2,
                DifficultyLevel.Hard => 1.0,
                DifficultyLevel.Difficult => 0.9,
                DifficultyLevel.Unfair => 0.8,
                _ => 1.0
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
        // Numbers (decided 2026-11-23) in DifficultyScaler / LevelingSystem / GameSettings:
        // enemy health, enemy damage, XP, gold, party hit chance, party max health
        Normal,     // 75%,  75%,  80% XP,  100% gold, -0,  120% HP
        Hard,       // 100%, 100%, 100% XP, 100% gold, -5,  100% HP (the 1x baseline)
        Difficult,  // 175%, 150%, 130% XP, 125% gold, -10, 90% HP
        Unfair      // 275%, 225%, 160% XP, 150% gold, -15, 80% HP, plus bad luck
    }
}