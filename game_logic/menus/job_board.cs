using System;
using System.Collections.Generic;
using System.Linq;
using GameLogic.Quests;

namespace GameLogic.Menus
{
    /// <summary>
    /// Job Board menu for discovering and managing non-boss quests
    /// Provides access to level, enemy kill, gold collection, weapon upgrade, equipment, and challenge quests
    /// </summary>
    public static class JobBoard
    {
        /// <summary>
        /// Display the Job Board menu
        /// </summary>
        public static void DisplayJobBoard(QuestManager questManager)
        {
            while (true)
            {
                Console.Clear();
                DisplayHeader();

                Console.WriteLine("═══ JOB BOARD MENU ═══");
                Console.WriteLine("[1] View Available Quests");
                Console.WriteLine("[2] View My Active Quests");
                Console.WriteLine("[3] View Completed Quests");
                Console.WriteLine("[4] Set Active Quest");
                Console.WriteLine("[5] Leave Job Board");
                Console.Write("\nChoice: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ViewAvailableQuests(questManager);
                        break;
                    case "2":
                        ViewActiveQuests(questManager);
                        break;
                    case "3":
                        ViewCompletedQuests(questManager);
                        break;
                    case "4":
                        SetActiveQuest(questManager);
                        break;
                    case "5":
                        return;
                    default:
                        Console.WriteLine("Invalid choice.");
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        /// <summary>
        /// Display Job Board header
        /// </summary>
        private static void DisplayHeader()
        {
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("            JOB BOARD");
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("Find work and prove your worth here!");
            Console.WriteLine();
        }

        /// <summary>
        /// View available quests (non-boss quests that aren't boss or final boss related)
        /// </summary>
        private static void ViewAvailableQuests(QuestManager questManager)
        {
            Console.Clear();
            Console.WriteLine("═══ AVAILABLE QUESTS ═══\n");

            var availableQuests = questManager.AllQuests.Values
                .Where(q => !(q is BossDefeatQuest) && !(q is FinalBossQuest) && q.State == QuestState.Available)
                .ToList();

            if (availableQuests.Count == 0)
            {
                Console.WriteLine("No new quests available at this time.");
                Console.WriteLine("Check back later for more opportunities!");
            }
            else
            {
                // Group quests by type
                var questsByType = availableQuests
                    .GroupBy(q => q.GetType().Name)
                    .OrderBy(g => g.Key);

                foreach (var group in questsByType)
                {
                    string typeName = FormatQuestTypeName(group.Key);
                    Console.WriteLine($"--- {typeName} ---");

                    int index = 1;
                    foreach (var quest in group)
                    {
                        Console.WriteLine($"[{index}] {quest.QuestName}");
                        Console.WriteLine($"    {quest.Description}");
                        if (quest.Reward.HasRewards())
                        {
                            Console.WriteLine($"    Reward: {quest.Reward.GetRewardSummary()}");
                        }
                        Console.WriteLine();
                        index++;
                    }
                }

                Console.Write("\nEnter quest number to view details (or 0 to go back): ");
                if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= availableQuests.Count)
                {
                    ViewAndAcceptQuest(questManager, availableQuests[choice - 1]);
                }
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        /// <summary>
        /// View a quest and offer to accept it
        /// </summary>
        private static void ViewAndAcceptQuest(QuestManager questManager, Quest quest)
        {
            Console.Clear();
            quest.DisplayQuestInfo();

            Console.Write("\n\nAccept this quest? [Y/N]: ");
            string choice = Console.ReadLine();

            if (choice?.ToUpper() == "Y")
            {
                if (questManager.AcceptQuest(quest.QuestId))
                {
                    Console.WriteLine($"\n✓ Quest Accepted: {quest.QuestName}");
                    Console.WriteLine("The quest has been added to your active quests.");
                }
                else
                {
                    Console.WriteLine("\nFailed to accept quest.");
                }
            }
        }

        /// <summary>
        /// View active quests and their progress
        /// </summary>
        private static void ViewActiveQuests(QuestManager questManager)
        {
            Console.Clear();
            Console.WriteLine("═══ MY ACTIVE QUESTS ═══\n");

            var activeQuests = questManager.AllQuests.Values
                .Where(q => !(q is BossDefeatQuest) && !(q is FinalBossQuest) && q.State == QuestState.Accepted)
                .ToList();

            if (activeQuests.Count == 0)
            {
                Console.WriteLine("You have no active quests.");
                Console.WriteLine("Visit the Job Board to find new quests!");
            }
            else
            {
                for (int i = 0; i < activeQuests.Count; i++)
                {
                    var quest = activeQuests[i];
                    string activeMarker = quest.QuestId == questManager.ActiveQuestId ? "★ " : "  ";

                    Console.WriteLine($"{activeMarker}[{i + 1}] {quest.QuestName} ({quest.GetProgressPercentage()}%)");
                    foreach (var objective in quest.Objectives)
                    {
                        string checkmark = objective.IsCompleted ? "✓" : " ";
                        Console.WriteLine($"     [{checkmark}] {objective.Description} ({objective.GetProgressString()})");
                    }
                    Console.WriteLine();
                }

                Console.Write("\nEnter quest number to view details (or 0 to go back): ");
                if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= activeQuests.Count)
                {
                    ViewQuestDetails(questManager, activeQuests[choice - 1]);
                }
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        /// <summary>
        /// View completed quests and claim rewards
        /// </summary>
        private static void ViewCompletedQuests(QuestManager questManager)
        {
            Console.Clear();
            Console.WriteLine("═══ COMPLETED QUESTS ═══\n");

            var completedQuests = questManager.AllQuests.Values
                .Where(q => !(q is BossDefeatQuest) && !(q is FinalBossQuest) && q.State == QuestState.Completed)
                .ToList();

            var claimedQuests = questManager.AllQuests.Values
                .Where(q => !(q is BossDefeatQuest) && !(q is FinalBossQuest) && q.State == QuestState.Claimed)
                .Take(10) // Show last 10 claimed quests
                .ToList();

            if (completedQuests.Count == 0 && claimedQuests.Count == 0)
            {
                Console.WriteLine("You haven't completed any quests yet.");
            }
            else
            {
                // Completed (ready to claim)
                if (completedQuests.Count > 0)
                {
                    Console.WriteLine("--- Ready to Claim ---");
                    for (int i = 0; i < completedQuests.Count; i++)
                    {
                        var quest = completedQuests[i];
                        Console.WriteLine($"[{i + 1}] {quest.QuestName} - Reward: {quest.Reward.GetRewardSummary()}");
                    }
                    Console.WriteLine();

                    Console.WriteLine("[C] Claim all rewards");
                    Console.Write("Enter quest number to claim individual reward, or C for all: ");
                    string choice = Console.ReadLine();

                    if (choice?.ToUpper() == "C")
                    {
                        ClaimAllRewards(questManager, completedQuests);
                    }
                    else if (int.TryParse(choice, out int questNum) && questNum > 0 && questNum <= completedQuests.Count)
                    {
                        ClaimQuestReward(questManager, completedQuests[questNum - 1]);
                    }
                }

                // Recently claimed
                if (claimedQuests.Count > 0)
                {
                    Console.WriteLine("\n--- Recently Claimed ---");
                    foreach (var quest in claimedQuests)
                    {
                        Console.WriteLine($"✓ {quest.QuestName}");
                    }
                }
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        /// <summary>
        /// Set which quest is actively tracked
        /// </summary>
        private static void SetActiveQuest(QuestManager questManager)
        {
            Console.Clear();
            Console.WriteLine("═══ SET ACTIVE QUEST ═══\n");

            var acceptedQuests = questManager.GetAcceptedQuests();

            if (acceptedQuests.Count == 0)
            {
                Console.WriteLine("You have no active quests to track.");
            }
            else
            {
                Console.WriteLine("Select a quest to track:\n");

                for (int i = 0; i < acceptedQuests.Count; i++)
                {
                    var quest = acceptedQuests[i];
                    string activeMarker = quest.QuestId == questManager.ActiveQuestId ? "★ " : "  ";
                    Console.WriteLine($"{activeMarker}[{i + 1}] {quest.QuestName} ({quest.GetProgressPercentage()}%)");
                }

                Console.WriteLine("\n[0] Clear active quest");
                Console.Write("\nChoice: ");

                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    if (choice == 0)
                    {
                        questManager.ClearActiveQuest();
                        Console.WriteLine("\n✓ Active quest cleared.");
                    }
                    else if (choice > 0 && choice <= acceptedQuests.Count)
                    {
                        questManager.SetActiveQuest(acceptedQuests[choice - 1].QuestId);
                        Console.WriteLine($"\n✓ Now tracking: {acceptedQuests[choice - 1].QuestName}");
                    }
                }
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        /// <summary>
        /// View detailed information about a quest
        /// </summary>
        private static void ViewQuestDetails(QuestManager questManager, Quest quest)
        {
            Console.Clear();
            quest.DisplayQuestInfo();

            if (quest.State == QuestState.Accepted && quest.QuestId != questManager.ActiveQuestId)
            {
                Console.Write("\n\nSet as active quest? [Y/N]: ");
                string choice = Console.ReadLine();

                if (choice?.ToUpper() == "Y")
                {
                    questManager.SetActiveQuest(quest.QuestId);
                    Console.WriteLine($"\n✓ Now tracking: {quest.QuestName}");
                }
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        /// <summary>
        /// Claim rewards for a single quest
        /// </summary>
        private static void ClaimQuestReward(QuestManager questManager, Quest quest)
        {
            var reward = questManager.ClaimQuestRewards(quest.QuestId);
            if (reward != null)
            {
                Console.WriteLine($"\n✓ Quest Completed: {quest.QuestName}");
                reward.DisplayRewards();
                // Note: Actual reward application (gold, XP) happens in GameManager
            }
        }

        /// <summary>
        /// Claim all quest rewards
        /// </summary>
        private static void ClaimAllRewards(QuestManager questManager, List<Quest> quests)
        {
            Console.WriteLine("\n═══ CLAIMING ALL REWARDS ═══");

            foreach (var quest in quests)
            {
                var reward = questManager.ClaimQuestRewards(quest.QuestId);
                if (reward != null)
                {
                    Console.WriteLine($"\n✓ {quest.QuestName}");
                    reward.DisplayRewards();
                }
            }

            Console.WriteLine("\n🎉 All rewards claimed!");
        }

        /// <summary>
        /// Format quest type name for display
        /// </summary>
        private static string FormatQuestTypeName(string typeName)
        {
            return typeName switch
            {
                "LevelQuest" => "Level Progression",
                "EnemyKillQuest" => "Enemy Slaying",
                "GoldCollectionQuest" => "Wealth Accumulation",
                "WeaponUpgradeQuest" => "Weapon Mastery",
                "EquipmentQuest" => "Equipment Goals",
                "ChallengeQuest" => "Special Challenges",
                _ => typeName
            };
        }
    }
}