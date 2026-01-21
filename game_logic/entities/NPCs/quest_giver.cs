using System;
using System.Collections.Generic;
using System.Linq;
using GameLogic.Quests;
using GameLogic.Progression;
using Quest = GameLogic.Quests.Quest;

namespace GameLogic.Entities.NPCs
{
    /// <summary>
    /// NPC that provides boss-related quests to the player
    /// Located in the town/hub area
    /// </summary>
    public class QuestGiver
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        private QuestManager _questManager;
        private BossManager _bossManager;

        public QuestGiver(string name, QuestManager questManager, BossManager bossManager)
        {
            Name = name;
            Description = "A seasoned warrior who knows the locations of all the Champions in the realm. They offer guidance to those brave enough to seek them out.";
            _questManager = questManager;
            _bossManager = bossManager;
        }

        /// <summary>
        /// Interact with the Quest Giver
        /// </summary>
        public void Interact()
        {
            while (true)
            {
                Console.Clear();
                DisplayGreeting();

                Console.WriteLine("\n═══ QUEST GIVER OPTIONS ═══");
                Console.WriteLine("[1] View Available Boss Quests");
                Console.WriteLine("[2] View My Boss Quests");
                Console.WriteLine("[3] View Final Boss Quest");
                Console.WriteLine("[4] Talk");
                Console.WriteLine("[5] Leave");
                Console.Write("\nChoice: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ViewAvailableBossQuests();
                        break;
                    case "2":
                        ViewMyBossQuests();
                        break;
                    case "3":
                        ViewFinalBossQuest();
                        break;
                    case "4":
                        Talk();
                        break;
                    case "5":
                        Console.WriteLine($"\n{Name}: \"May fortune favor you on your journey, Champion.\"");
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
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
        /// Display greeting message
        /// </summary>
        private void DisplayGreeting()
        {
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine($"        {Name.ToUpper()}");
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine($"\n{Name}: \"Greetings, brave warrior. I know of powerful Champions");
            Console.WriteLine("scattered across the realm. Each holds a Champion Key.");
            Console.WriteLine("Defeat 10 of them to unlock the path to the Final Champion.\"");
        }

        /// <summary>
        /// View available boss quests that can be accepted
        /// </summary>
        private void ViewAvailableBossQuests()
        {
            Console.Clear();
            Console.WriteLine("═══ AVAILABLE BOSS QUESTS ═══\n");

            var availableBossQuests = _questManager.AllQuests.Values
                .Where(q => q is BossDefeatQuest && q.State == QuestState.Available)
                .Cast<BossDefeatQuest>()
                .ToList();

            if (availableBossQuests.Count == 0)
            {
                Console.WriteLine("No new boss quests available.");
                Console.WriteLine("\nAll boss quests have been discovered!");
            }
            else
            {
                for (int i = 0; i < availableBossQuests.Count; i++)
                {
                    var quest = availableBossQuests[i];
                    Console.WriteLine($"[{i + 1}] {quest.QuestName}");
                    Console.WriteLine($"    {quest.Description}");
                    Console.WriteLine($"    Reward: {quest.Reward.GetRewardSummary()}");
                    Console.WriteLine();
                }

                Console.Write("\nEnter quest number to accept (or 0 to go back): ");
                if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= availableBossQuests.Count)
                {
                    var selectedQuest = availableBossQuests[choice - 1];
                    AcceptQuest(selectedQuest);
                }
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        /// <summary>
        /// View accepted boss quests and their progress
        /// </summary>
        private void ViewMyBossQuests()
        {
            Console.Clear();
            Console.WriteLine("═══ MY BOSS QUESTS ═══\n");

            var myBossQuests = _questManager.AllQuests.Values
                .Where(q => q is BossDefeatQuest && (q.State == QuestState.Accepted || q.State == QuestState.Completed))
                .Cast<BossDefeatQuest>()
                .ToList();

            if (myBossQuests.Count == 0)
            {
                Console.WriteLine("You haven't accepted any boss quests yet.");
            }
            else
            {
                foreach (var quest in myBossQuests)
                {
                    Console.WriteLine($"• {quest.QuestName} - {quest.GetStateString()}");
                    foreach (var objective in quest.Objectives)
                    {
                        string checkmark = objective.IsCompleted ? "✓" : " ";
                        Console.WriteLine($"  [{checkmark}] {objective.Description} ({objective.GetProgressString()})");
                    }

                    if (quest.State == QuestState.Completed)
                    {
                        Console.WriteLine($"  🏆 READY TO CLAIM! Reward: {quest.Reward.GetRewardSummary()}");
                    }

                    Console.WriteLine();
                }

                // Option to claim completed quests
                var completedQuests = myBossQuests.Where(q => q.State == QuestState.Completed).ToList();
                if (completedQuests.Count > 0)
                {
                    Console.WriteLine("\n[C] Claim all completed quest rewards");
                    Console.Write("Choice: ");
                    string choice = Console.ReadLine();

                    if (choice?.ToUpper() == "C")
                    {
                        ClaimCompletedQuests(completedQuests);
                    }
                }
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        /// <summary>
        /// View the final boss quest
        /// </summary>
        private void ViewFinalBossQuest()
        {
            Console.Clear();
            Console.WriteLine("═══ FINAL BOSS QUEST ═══\n");

            var finalQuest = _questManager.AllQuests.Values.FirstOrDefault(q => q is FinalBossQuest) as FinalBossQuest;

            if (finalQuest == null)
            {
                Console.WriteLine("The Final Boss Quest is not yet available.");
            }
            else
            {
                finalQuest.DisplayQuestInfo();

                if (finalQuest.State == QuestState.Available)
                {
                    Console.WriteLine("\n⚠️  WARNING: This is a point of no return!");
                    Console.WriteLine("Accepting this quest means you are ready to face the ultimate challenge.");
                    Console.Write("\nAccept this quest? [Y/N]: ");
                    string choice = Console.ReadLine();

                    if (choice?.ToUpper() == "Y")
                    {
                        AcceptQuest(finalQuest);
                    }
                }
                else if (finalQuest.State == QuestState.Completed)
                {
                    Console.WriteLine("\n🏆 You have completed the ultimate challenge!");
                    Console.WriteLine("The Final Boss has been defeated!");
                }
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        /// <summary>
        /// Accept a quest
        /// </summary>
        private void AcceptQuest(Quest quest)
        {
            _questManager.AcceptQuest(quest.QuestId);

            Console.WriteLine($"\n✓ Quest Accepted: {quest.QuestName}");
            Console.WriteLine($"{Name}: \"Good luck, warrior. May your blade strike true!\"");
        }

        /// <summary>
        /// Claim rewards for completed quests
        /// </summary>
        private void ClaimCompletedQuests(List<BossDefeatQuest> completedQuests)
        {
            Console.WriteLine("\n═══ CLAIMING REWARDS ═══");

            foreach (var quest in completedQuests)
            {
                var reward = _questManager.ClaimQuestRewards(quest.QuestId);
                if (reward != null)
                {
                    Console.WriteLine($"\n✓ {quest.QuestName} - Claimed!");
                    reward.DisplayRewards();
                    // Note: Actual reward application (gold, XP) happens in GameManager
                }
            }

            Console.WriteLine($"\n{Name}: \"Well done, warrior! You've proven your worth!\"");
        }

        /// <summary>
        /// Talk to the Quest Giver for lore/hints
        /// </summary>
        private void Talk()
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine($"        TALKING WITH {Name.ToUpper()}");
            Console.WriteLine("═══════════════════════════════════════\n");

            int bossesDefeated = _bossManager.BossesDefeated;
            int keysNeeded = BossManager.KEYS_REQUIRED;

            if (bossesDefeated == 0)
            {
                Console.WriteLine($"{Name}: \"The Champions are scattered across the land.\"");
                Console.WriteLine("\"Each one guards a legendary key. Defeat them to prove your strength.\"");
                Console.WriteLine("\"But beware - these are no ordinary foes. They will test your limits.\"");
            }
            else if (bossesDefeated < keysNeeded)
            {
                Console.WriteLine($"{Name}: \"You've defeated {bossesDefeated} Champion{(bossesDefeated > 1 ? "s" : "")} so far.\"");
                Console.WriteLine($"\"Only {keysNeeded - bossesDefeated} more key{(keysNeeded - bossesDefeated > 1 ? "s" : "")} stand between you and the Final Gate.\"");
                Console.WriteLine("\"Keep pushing forward, warrior. Your legend grows with each victory.\"");
            }
            else if (!_bossManager.FinalGateUnlocked)
            {
                Console.WriteLine($"{Name}: \"Incredible! You've collected {keysNeeded} Champion Keys!\"");
                Console.WriteLine("\"The Final Gate awaits you. When you're ready, use those keys to enter.\"");
                Console.WriteLine("\"Inside waits the ultimate challenge. Prepare yourself well.\"");
            }
            else
            {
                Console.WriteLine($"{Name}: \"The Final Gate is open. The ultimate Champion awaits.\"");
                Console.WriteLine("\"This is what you've been training for. Show them what you're made of!\"");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
    }
}