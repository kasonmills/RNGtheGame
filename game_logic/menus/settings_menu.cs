using System;
using GameLogic.Systems;

namespace GameLogic.Menus
{
    /// <summary>
    /// Settings menu for configuring game options
    /// </summary>
    public static class SettingsMenu
    {
        /// <summary>
        /// Display the main settings menu
        /// </summary>
        public static void DisplaySettingsMenu(GameSettings settings, RNGManager rngManager, bool isDuringSaveFile)
        {
            bool exit = false;

            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("═══════════════════════════════════════");
                Console.WriteLine("         GAME SETTINGS");
                Console.WriteLine("═══════════════════════════════════════\n");

                Console.WriteLine("1. Display Settings");
                Console.WriteLine("2. Gameplay Settings");
                Console.WriteLine("3. RNG Settings");
                Console.WriteLine("4. Accessibility Settings");
                Console.WriteLine("5. Audio Settings (Placeholder)");
                if (!isDuringSaveFile)
                {
                    Console.WriteLine("6. View Difficulty (Cannot be changed)");
                }
                Console.WriteLine("7. Reset to Defaults");
                Console.WriteLine("8. Back to Main Menu");

                Console.Write("\nSelect an option: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        DisplaySettings(settings);
                        break;
                    case "2":
                        GameplaySettings(settings);
                        break;
                    case "3":
                        RngSettings(settings, rngManager);
                        break;
                    case "4":
                        AccessibilitySettings(settings);
                        break;
                    case "5":
                        AudioSettings(settings);
                        break;
                    case "6":
                        if (!isDuringSaveFile)
                        {
                            ViewDifficulty(settings);
                        }
                        break;
                    case "7":
                        ResetToDefaults(settings);
                        break;
                    case "8":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid option. Press any key to continue...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        /// <summary>
        /// Display settings submenu
        /// </summary>
        private static void DisplaySettings(GameSettings settings)
        {
            bool exit = false;

            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("═══════════════════════════════════════");
                Console.WriteLine("         DISPLAY SETTINGS");
                Console.WriteLine("═══════════════════════════════════════\n");

                Console.WriteLine($"1. Show Turn Order: {GetOnOff(settings.ShowTurnOrderAtStartOfRound)}");
                Console.WriteLine($"2. Detailed Combat Log: {GetOnOff(settings.ShowDetailedCombatLog)}");
                Console.WriteLine($"3. Show Damage Calculations: {GetOnOff(settings.ShowDamageCalculations)}");
                Console.WriteLine($"4. Show Enemy Stat Block: {GetOnOff(settings.ShowEnemyStatBlock)}");
                Console.WriteLine("\n5. Back");

                Console.Write("\nToggle an option: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        settings.ShowTurnOrderAtStartOfRound = !settings.ShowTurnOrderAtStartOfRound;
                        break;
                    case "2":
                        settings.ShowDetailedCombatLog = !settings.ShowDetailedCombatLog;
                        break;
                    case "3":
                        settings.ShowDamageCalculations = !settings.ShowDamageCalculations;
                        break;
                    case "4":
                        settings.ShowEnemyStatBlock = !settings.ShowEnemyStatBlock;
                        break;
                    case "5":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid option.");
                        Console.ReadKey();
                        break;
                }
            }
        }

        /// <summary>
        /// Gameplay settings submenu
        /// </summary>
        private static void GameplaySettings(GameSettings settings)
        {
            bool exit = false;

            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("═══════════════════════════════════════");
                Console.WriteLine("         GAMEPLAY SETTINGS");
                Console.WriteLine("═══════════════════════════════════════\n");

                Console.WriteLine($"1. Auto-Save: {GetOnOff(settings.AutoSaveEnabled)}");
                Console.WriteLine($"2. Auto-Save Interval: {settings.AutoSaveIntervalMinutes} minutes");
                Console.WriteLine($"3. Confirm Flee Action: {GetOnOff(settings.ConfirmFleeAction)}");
                Console.WriteLine($"4. Confirm Item Consumption: {GetOnOff(settings.ConfirmConsumeItem)}");
                Console.WriteLine("\n5. Back");

                Console.Write("\nSelect an option: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        settings.AutoSaveEnabled = !settings.AutoSaveEnabled;
                        break;
                    case "2":
                        AdjustAutoSaveInterval(settings);
                        break;
                    case "3":
                        settings.ConfirmFleeAction = !settings.ConfirmFleeAction;
                        break;
                    case "4":
                        settings.ConfirmConsumeItem = !settings.ConfirmConsumeItem;
                        break;
                    case "5":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid option.");
                        Console.ReadKey();
                        break;
                }
            }
        }

        /// <summary>
        /// RNG settings submenu
        /// </summary>
        private static void RngSettings(GameSettings settings, RNGManager rngManager)
        {
            bool exit = false;

            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("═══════════════════════════════════════");
                Console.WriteLine("         RNG SETTINGS");
                Console.WriteLine("═══════════════════════════════════════\n");

                Console.WriteLine($"Current Algorithm: {rngManager.CurrentAlgorithm}");
                Console.WriteLine($"Statistics Tracking: {GetOnOff(settings.RngStatisticsTracking)}");
                if (settings.RngStatisticsTracking)
                {
                    Console.WriteLine($"Total Rolls: {rngManager.TotalRolls:N0}");
                }

                Console.WriteLine("\n1. Switch RNG Algorithm (Currently only System.Random available)");
                Console.WriteLine("2. Toggle Statistics Tracking");
                if (settings.RngStatisticsTracking)
                {
                    Console.WriteLine("3. Reset Statistics Counter");
                }
                Console.WriteLine("\n4. Back");

                Console.Write("\nSelect an option: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        SwitchRngAlgorithm(settings, rngManager);
                        break;
                    case "2":
                        settings.RngStatisticsTracking = !settings.RngStatisticsTracking;
                        rngManager.SetStatisticsTracking(settings.RngStatisticsTracking);
                        break;
                    case "3":
                        if (settings.RngStatisticsTracking)
                        {
                            rngManager.ResetStatistics();
                            Console.WriteLine("Statistics counter reset!");
                            Console.ReadKey();
                        }
                        break;
                    case "4":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid option.");
                        Console.ReadKey();
                        break;
                }
            }
        }

        /// <summary>
        /// Accessibility settings submenu
        /// </summary>
        private static void AccessibilitySettings(GameSettings settings)
        {
            bool exit = false;

            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("═══════════════════════════════════════");
                Console.WriteLine("         ACCESSIBILITY SETTINGS");
                Console.WriteLine("═══════════════════════════════════════\n");

                Console.WriteLine($"1. Colored Text: {GetOnOff(settings.ColoredText)}");
                Console.WriteLine($"2. Use Emojis: {GetOnOff(settings.UseEmojis)}");
                Console.WriteLine($"3. Text Speed: {GetTextSpeedLabel(settings.TextSpeed)}");
                Console.WriteLine("\n4. Back");

                Console.Write("\nSelect an option: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        settings.ColoredText = !settings.ColoredText;
                        break;
                    case "2":
                        settings.UseEmojis = !settings.UseEmojis;
                        break;
                    case "3":
                        AdjustTextSpeed(settings);
                        break;
                    case "4":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid option.");
                        Console.ReadKey();
                        break;
                }
            }
        }

        /// <summary>
        /// Audio settings submenu (placeholder for future GUI implementation)
        /// </summary>
        private static void AudioSettings(GameSettings settings)
        {
            bool exit = false;

            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("═══════════════════════════════════════");
                Console.WriteLine("         AUDIO SETTINGS");
                Console.WriteLine("       (Placeholder - For GUI Version)");
                Console.WriteLine("═══════════════════════════════════════\n");

                Console.WriteLine($"1. Sound Effects: {GetOnOff(settings.SoundEffectsEnabled)}");
                Console.WriteLine($"2. Sound Effects Volume: {settings.SoundEffectsVolume}%");
                Console.WriteLine($"3. Music: {GetOnOff(settings.MusicEnabled)}");
                Console.WriteLine($"4. Music Volume: {settings.MusicVolume}%");
                Console.WriteLine("\n5. Back");

                Console.WriteLine("\nNote: Audio features will be implemented in GUI version.");

                Console.Write("\nSelect an option: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        settings.SoundEffectsEnabled = !settings.SoundEffectsEnabled;
                        break;
                    case "2":
                        AdjustVolume("Sound Effects", ref settings.SoundEffectsVolume);
                        break;
                    case "3":
                        settings.MusicEnabled = !settings.MusicEnabled;
                        break;
                    case "4":
                        AdjustVolume("Music", ref settings.MusicVolume);
                        break;
                    case "5":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid option.");
                        Console.ReadKey();
                        break;
                }
            }
        }

        /// <summary>
        /// View difficulty setting (cannot be changed during save file)
        /// </summary>
        private static void ViewDifficulty(GameSettings settings)
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("         DIFFICULTY SETTING");
            Console.WriteLine("═══════════════════════════════════════\n");

            Console.WriteLine($"Current Difficulty: {settings.Difficulty}");
            Console.WriteLine($"Enemy Stats Multiplier: {settings.GetDifficultyMultiplier():P0}");
            Console.WriteLine($"Reward Multiplier: {settings.GetRewardMultiplier():P0}");

            Console.WriteLine("\nDifficulty Levels:");
            Console.WriteLine("• Easy: 75% enemy stats, 80% rewards");
            Console.WriteLine("• Normal: 100% enemy stats, 100% rewards");
            Console.WriteLine("• Hard: 150% enemy stats, 130% rewards");
            Console.WriteLine("• Very Hard: 200% enemy stats, 200% rewards");

            Console.WriteLine("\nNote: Difficulty is set at save file creation and cannot be changed.");
            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }

        /// <summary>
        /// Reset all settings to defaults (except difficulty)
        /// </summary>
        private static void ResetToDefaults(GameSettings settings)
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("Are you sure you want to reset all settings to defaults?");
            Console.WriteLine("(Difficulty will NOT be reset)");
            Console.WriteLine("═══════════════════════════════════════\n");

            Console.Write("Type 'yes' to confirm: ");
            string confirm = Console.ReadLine();

            if (confirm?.ToLower() == "yes")
            {
                settings.ResetToDefaults();
                Console.WriteLine("\nSettings reset to defaults!");
            }
            else
            {
                Console.WriteLine("\nReset cancelled.");
            }

            Console.ReadKey();
        }

        // ===== Helper Methods =====

        private static string GetOnOff(bool value)
        {
            return value ? "ON" : "OFF";
        }

        private static string GetTextSpeedLabel(int speed)
        {
            return speed switch
            {
                0 => "Instant",
                1 => "Very Fast",
                2 => "Fast",
                3 => "Normal",
                4 => "Slow",
                5 => "Very Slow",
                _ => "Instant"
            };
        }

        private static void AdjustAutoSaveInterval(GameSettings settings)
        {
            Console.Clear();
            Console.WriteLine("Set Auto-Save Interval (minutes):");
            Console.WriteLine("Current: " + settings.AutoSaveIntervalMinutes + " minutes");
            Console.Write("\nEnter new interval (1-60): ");

            if (int.TryParse(Console.ReadLine(), out int interval) && interval >= 1 && interval <= 60)
            {
                settings.AutoSaveIntervalMinutes = interval;
                Console.WriteLine($"Auto-save interval set to {interval} minutes.");
            }
            else
            {
                Console.WriteLine("Invalid interval. Must be between 1 and 60.");
            }

            Console.ReadKey();
        }

        private static void AdjustTextSpeed(GameSettings settings)
        {
            Console.Clear();
            Console.WriteLine("Text Speed Settings:");
            Console.WriteLine("0 = Instant");
            Console.WriteLine("1 = Very Fast");
            Console.WriteLine("2 = Fast");
            Console.WriteLine("3 = Normal");
            Console.WriteLine("4 = Slow");
            Console.WriteLine("5 = Very Slow");
            Console.Write("\nEnter speed (0-5): ");

            if (int.TryParse(Console.ReadLine(), out int speed) && speed >= 0 && speed <= 5)
            {
                settings.TextSpeed = speed;
                Console.WriteLine($"Text speed set to {GetTextSpeedLabel(speed)}.");
            }
            else
            {
                Console.WriteLine("Invalid speed. Must be between 0 and 5.");
            }

            Console.ReadKey();
        }

        private static void AdjustVolume(string settingName, ref int volume)
        {
            Console.Clear();
            Console.WriteLine($"Adjust {settingName} Volume:");
            Console.WriteLine($"Current: {volume}%");
            Console.Write("\nEnter volume (0-100): ");

            if (int.TryParse(Console.ReadLine(), out int newVolume) && newVolume >= 0 && newVolume <= 100)
            {
                volume = newVolume;
                Console.WriteLine($"{settingName} volume set to {newVolume}%.");
            }
            else
            {
                Console.WriteLine("Invalid volume. Must be between 0 and 100.");
            }

            Console.ReadKey();
        }

        private static void SwitchRngAlgorithm(GameSettings settings, RNGManager rngManager)
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("         AVAILABLE RNG ALGORITHMS");
            Console.WriteLine("═══════════════════════════════════════\n");

            var algorithms = rngManager.GetAvailableAlgorithms();
            Console.WriteLine($"Current: {rngManager.CurrentAlgorithm}\n");

            for (int i = 0; i < algorithms.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {algorithms[i]}");
            }

            Console.Write("\nSelect algorithm (or 0 to cancel): ");

            if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= algorithms.Count)
            {
                string selectedAlgorithm = algorithms[choice - 1];
                if (rngManager.SwitchAlgorithm(selectedAlgorithm))
                {
                    settings.RngAlgorithm = selectedAlgorithm;
                    Console.WriteLine($"RNG algorithm changed to {selectedAlgorithm}.");
                }
                else
                {
                    Console.WriteLine("Failed to switch algorithm.");
                }
            }
            else if (choice != 0)
            {
                Console.WriteLine("Invalid selection.");
            }

            Console.ReadKey();
        }
    }
}