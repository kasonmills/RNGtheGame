using System;
using System.Linq;
using GameLogic.Systems;

namespace GameLogic.Menus
{
    /// <summary>
    /// Menu for displaying player statistics and achievements
    /// </summary>
    public static class StatisticsMenu
    {
        /// <summary>
        /// Display the main statistics menu
        /// </summary>
        public static void DisplayStatisticsMenu(StatisticsTracker stats)
        {
            if (stats == null)
            {
                Console.WriteLine("No statistics available.");
                Console.ReadKey();
                return;
            }

            bool exit = false;

            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("═══════════════════════════════════════");
                Console.WriteLine("         PLAYER STATISTICS");
                Console.WriteLine("═══════════════════════════════════════\n");

                Console.WriteLine("1. Combat Statistics");
                Console.WriteLine("2. Economic Statistics");
                Console.WriteLine("3. Equipment Statistics");
                Console.WriteLine("4. Item Usage Statistics");
                Console.WriteLine("5. Exploration Statistics");
                Console.WriteLine("6. Progression Statistics");
                Console.WriteLine("7. Achievement Statistics");
                Console.WriteLine("8. Summary Overview");
                Console.WriteLine("9. Back");

                Console.Write("\nSelect category: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        DisplayCombatStatistics(stats);
                        break;
                    case "2":
                        DisplayEconomicStatistics(stats);
                        break;
                    case "3":
                        DisplayEquipmentStatistics(stats);
                        break;
                    case "4":
                        DisplayItemUsageStatistics(stats);
                        break;
                    case "5":
                        DisplayExplorationStatistics(stats);
                        break;
                    case "6":
                        DisplayProgressionStatistics(stats);
                        break;
                    case "7":
                        DisplayAchievementStatistics(stats);
                        break;
                    case "8":
                        DisplaySummaryOverview(stats);
                        break;
                    case "9":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid option.");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private static void DisplayCombatStatistics(StatisticsTracker stats)
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("         COMBAT STATISTICS");
            Console.WriteLine("═══════════════════════════════════════\n");

            Console.WriteLine("=== Battle Record ===");
            Console.WriteLine($"Total Battles: {stats.TotalBattlesFought}");
            Console.WriteLine($"Victories: {stats.BattlesWon} ({stats.GetWinRate():F1}%)");
            Console.WriteLine($"Defeats: {stats.BattlesLost}");
            Console.WriteLine($"Fled: {stats.BattlesFled}");
            Console.WriteLine();

            Console.WriteLine("=== Damage Statistics ===");
            Console.WriteLine($"Total Damage Dealt: {stats.TotalDamageDealt:N0}");
            Console.WriteLine($"Total Damage Taken: {stats.TotalDamageTaken:N0}");
            Console.WriteLine($"Highest Single Hit: {stats.HighestDamageDealt:N0}");
            Console.WriteLine($"Average Damage/Battle: {stats.GetAverageDamagePerBattle():F1}");
            Console.WriteLine($"Critical Hits: {stats.CriticalHitsLanded}");
            Console.WriteLine();

            Console.WriteLine("=== Enemy Kills ===");
            Console.WriteLine($"Total Enemies Killed: {stats.EnemiesKilled}");
            Console.WriteLine($"Most Killed Enemy: {stats.GetMostKilledEnemy()} ({stats.GetMostKilledEnemyCount()})");
            Console.WriteLine();

            if (stats.KillsByEnemyType.Count > 0)
            {
                Console.WriteLine("Kill Breakdown (Top 5):");
                var topKills = stats.KillsByEnemyType.OrderByDescending(x => x.Value).Take(5);
                foreach (var kill in topKills)
                {
                    Console.WriteLine($"  {kill.Key}: {kill.Value}");
                }
                Console.WriteLine();
            }

            Console.WriteLine("=== Boss Statistics ===");
            Console.WriteLine($"Boss Attempts: {stats.BossAttempts}");
            Console.WriteLine($"Bosses Defeated: {stats.BossesDefeated}");
            Console.WriteLine($"Boss Win Rate: {stats.GetBossWinRate():F1}%");
            Console.WriteLine();

            if (stats.BossDefeats.Count > 0)
            {
                Console.WriteLine("Bosses Defeated:");
                foreach (var boss in stats.BossDefeats.OrderByDescending(x => x.Value))
                {
                    Console.WriteLine($"  {boss.Key}: {boss.Value} time(s)");
                }
                Console.WriteLine();
            }

            Console.WriteLine("=== Other ===");
            Console.WriteLine($"Player Deaths: {stats.TotalPlayerDeaths}");
            Console.WriteLine($"Current Win Streak: {stats.CurrentWinStreak}");
            Console.WriteLine($"Longest Win Streak: {stats.LongestWinStreak}");

            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }

        private static void DisplayEconomicStatistics(StatisticsTracker stats)
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("         ECONOMIC STATISTICS");
            Console.WriteLine("═══════════════════════════════════════\n");

            Console.WriteLine("=== Gold Flow ===");
            Console.WriteLine($"Total Gold Earned: {stats.TotalGoldEarned:N0}g");
            Console.WriteLine($"Total Gold Spent: {stats.TotalGoldSpent:N0}g");
            Console.WriteLine($"Net Gold: {stats.GetNetGold():N0}g");
            Console.WriteLine($"Current Gold: {stats.CurrentGold:N0}g");
            Console.WriteLine();

            Console.WriteLine("=== Trading ===");
            Console.WriteLine($"Items Bought: {stats.ItemsBought}");
            Console.WriteLine($"Items Sold: {stats.ItemsSold}");
            Console.WriteLine($"Total Value Bought: {stats.TotalValueBought:N0}g");
            Console.WriteLine($"Total Value Sold: {stats.TotalValueSold:N0}g");
            Console.WriteLine();

            if (stats.MostExpensivePurchase > 0)
            {
                Console.WriteLine("=== Biggest Purchase ===");
                Console.WriteLine($"Item: {stats.MostExpensivePurchaseName}");
                Console.WriteLine($"Cost: {stats.MostExpensivePurchase:N0}g");
            }

            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }

        private static void DisplayEquipmentStatistics(StatisticsTracker stats)
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("         EQUIPMENT STATISTICS");
            Console.WriteLine("═══════════════════════════════════════\n");

            Console.WriteLine("=== Upgrades ===");
            Console.WriteLine($"Weapon Upgrades: {stats.WeaponUpgradesPerformed}");
            Console.WriteLine($"Armor Upgrades: {stats.ArmorUpgradesPerformed}");
            Console.WriteLine($"Total Upgrades: {stats.WeaponUpgradesPerformed + stats.ArmorUpgradesPerformed}");
            Console.WriteLine();

            Console.WriteLine("=== Equipment Levels ===");
            Console.WriteLine($"Highest Weapon Level: {stats.HighestWeaponLevel}");
            Console.WriteLine($"Highest Armor Level: {stats.HighestArmorLevel}");
            Console.WriteLine();

            Console.WriteLine("=== Equipment Changes ===");
            Console.WriteLine($"Total Equipment Swaps: {stats.TotalEquipmentChanges}");
            Console.WriteLine($"Favorite Weapon: {stats.GetFavoriteWeapon()}");
            Console.WriteLine();

            if (stats.WeaponsUsed.Count > 0)
            {
                Console.WriteLine("Weapons Used:");
                var topWeapons = stats.WeaponsUsed.OrderByDescending(x => x.Value).Take(10);
                foreach (var weapon in topWeapons)
                {
                    Console.WriteLine($"  {weapon.Key}: Equipped {weapon.Value} time(s)");
                }
            }

            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }

        private static void DisplayItemUsageStatistics(StatisticsTracker stats)
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("         ITEM USAGE STATISTICS");
            Console.WriteLine("═══════════════════════════════════════\n");

            Console.WriteLine("=== Consumables ===");
            Console.WriteLine($"Total Consumables Used: {stats.ConsumablesUsed}");
            Console.WriteLine($"Healing Potions Used: {stats.HealingPotionsUsed}");
            Console.WriteLine($"Revival Potions Used: {stats.RevivalPotionsUsed}");
            Console.WriteLine();

            if (stats.ConsumablesByType.Count > 0)
            {
                Console.WriteLine("Most Used Consumable:");
                Console.WriteLine($"  {stats.GetMostUsedConsumable()}");
                Console.WriteLine();

                Console.WriteLine("Consumable Breakdown (Top 10):");
                var topConsumables = stats.ConsumablesByType.OrderByDescending(x => x.Value).Take(10);
                foreach (var consumable in topConsumables)
                {
                    Console.WriteLine($"  {consumable.Key}: {consumable.Value}");
                }
                Console.WriteLine();
            }

            Console.WriteLine("=== Abilities ===");
            Console.WriteLine($"Abilities Activated: {stats.AbilitiesActivated}");

            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }

        private static void DisplayExplorationStatistics(StatisticsTracker stats)
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("         EXPLORATION STATISTICS");
            Console.WriteLine("═══════════════════════════════════════\n");

            Console.WriteLine("=== Interactions ===");
            Console.WriteLine($"Shops Visited: {stats.ShopsVisited}");
            Console.WriteLine($"NPCs Interacted With: {stats.NPCsInteracted}");
            Console.WriteLine();

            Console.WriteLine("=== Quests ===");
            Console.WriteLine($"Quests Accepted: {stats.QuestsAccepted}");
            Console.WriteLine($"Quests Completed: {stats.QuestsCompleted}");
            Console.WriteLine($"Quests Claimed: {stats.QuestsClaimed}");

            if (stats.QuestsAccepted > 0)
            {
                double completionRate = (double)stats.QuestsCompleted / stats.QuestsAccepted * 100;
                Console.WriteLine($"Completion Rate: {completionRate:F1}%");
            }

            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }

        private static void DisplayProgressionStatistics(StatisticsTracker stats)
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("         PROGRESSION STATISTICS");
            Console.WriteLine("═══════════════════════════════════════\n");

            Console.WriteLine("=== Level Progress ===");
            Console.WriteLine($"Current Level: {stats.CurrentLevel}");
            Console.WriteLine($"Highest Level Reached: {stats.HighestLevelReached}");
            Console.WriteLine($"Levels Gained: {stats.LevelsGained}");
            Console.WriteLine($"Total Experience Earned: {stats.TotalExperienceEarned:N0}");

            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }

        private static void DisplayAchievementStatistics(StatisticsTracker stats)
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("         ACHIEVEMENT STATISTICS");
            Console.WriteLine("═══════════════════════════════════════\n");

            Console.WriteLine("=== Battle Achievements ===");
            Console.WriteLine($"Flawless Victories: {stats.FlawlessVictories}");
            Console.WriteLine($"  (Won without taking damage)");
            Console.WriteLine();
            Console.WriteLine($"Close Call Victories: {stats.CloseCallVictories}");
            Console.WriteLine($"  (Won with less than 10% HP)");
            Console.WriteLine();
            Console.WriteLine($"Perfect Crit Runs: {stats.PerfectCritRuns}");
            Console.WriteLine($"  (All attacks were critical hits)");
            Console.WriteLine();

            Console.WriteLine("=== Streaks ===");
            Console.WriteLine($"Current Win Streak: {stats.CurrentWinStreak}");
            Console.WriteLine($"Longest Win Streak: {stats.LongestWinStreak}");

            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }

        private static void DisplaySummaryOverview(StatisticsTracker stats)
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("         SUMMARY OVERVIEW");
            Console.WriteLine("═══════════════════════════════════════\n");

            Console.WriteLine("=== Session Info ===");
            Console.WriteLine($"Game Version: {stats.GameVersion}");
            Console.WriteLine($"Started: {stats.GameStartDate:yyyy-MM-dd HH:mm}");
            Console.WriteLine($"Total Play Time: {stats.GetFormattedPlayTime()}");
            Console.WriteLine($"Game Sessions: {stats.GameSessions}");
            Console.WriteLine($"Games Saved: {stats.GamesSaved}");
            Console.WriteLine($"Games Loaded: {stats.GamesLoaded}");
            Console.WriteLine();

            Console.WriteLine("=== Quick Stats ===");
            Console.WriteLine($"Level: {stats.CurrentLevel}");
            Console.WriteLine($"Battles: {stats.BattlesWon}W / {stats.BattlesLost}L / {stats.BattlesFled}F");
            Console.WriteLine($"Win Rate: {stats.GetWinRate():F1}%");
            Console.WriteLine($"Enemies Killed: {stats.EnemiesKilled}");
            Console.WriteLine($"Bosses Defeated: {stats.BossesDefeated}");
            Console.WriteLine();

            Console.WriteLine($"Gold Earned: {stats.TotalGoldEarned:N0}g");
            Console.WriteLine($"Current Gold: {stats.CurrentGold:N0}g");
            Console.WriteLine($"Items Bought: {stats.ItemsBought}");
            Console.WriteLine($"Items Sold: {stats.ItemsSold}");
            Console.WriteLine();

            Console.WriteLine($"Quests Completed: {stats.QuestsCompleted}");
            Console.WriteLine($"Equipment Upgrades: {stats.WeaponUpgradesPerformed + stats.ArmorUpgradesPerformed}");
            Console.WriteLine($"Consumables Used: {stats.ConsumablesUsed}");
            Console.WriteLine();

            Console.WriteLine("=== Highlights ===");
            Console.WriteLine($"Longest Win Streak: {stats.LongestWinStreak}");
            Console.WriteLine($"Highest Damage: {stats.HighestDamageDealt:N0}");
            Console.WriteLine($"Flawless Victories: {stats.FlawlessVictories}");
            Console.WriteLine($"Favorite Weapon: {stats.GetFavoriteWeapon()}");

            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }
    }
}