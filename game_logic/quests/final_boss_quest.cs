using System;
using GameLogic.Progression;

namespace GameLogic.Quests
{
    /// <summary>
    /// Special quest for defeating the final boss
    /// This quest REQUIRES acceptance before it can be completed (unique requirement)
    /// No rewards as the game ends upon completion
    /// </summary>
    public class FinalBossQuest : Quest
    {
        public string FinalBossId { get; private set; }
        public string FinalBossName { get; private set; }

        public FinalBossQuest(string finalBossId, string finalBossName)
            : base(
                questId: "final_boss_quest",
                questName: "The Ultimate Champion",
                description: $"Face the ultimate challenge and defeat {finalBossName}, the Final Champion. This is the culmination of your journey. Victory here will prove you are the greatest warrior in the realm.",
                reward: new QuestReward(), // No rewards - game ends
                requiresAcceptance: true)   // MUST accept before completing
        {
            FinalBossId = finalBossId;
            FinalBossName = finalBossName;

            // Objectives
            Objectives.Add(new QuestObjective("Collect 10 Champion Keys", 10));
            Objectives.Add(new QuestObjective($"Enter the Final Gate", 1));
            Objectives.Add(new QuestObjective($"Defeat {finalBossName}", 1));
        }

        /// <summary>
        /// Update key count progress
        /// </summary>
        public void UpdateKeyCount(int keyCount)
        {
            Objectives[0].SetProgress(keyCount);
            CheckCompletion();
        }

        /// <summary>
        /// Mark final gate as entered
        /// </summary>
        public void OnFinalGateEntered()
        {
            Objectives[1].SetProgress(1);
            CheckCompletion();
        }

        /// <summary>
        /// Mark final boss as defeated
        /// </summary>
        public void OnFinalBossDefeated()
        {
            Objectives[2].SetProgress(1);
            CheckCompletion();
        }

        /// <summary>
        /// Check progress based on current game state (retroactive check)
        /// </summary>
        public void CheckProgress(BossManager bossManager, int keyCount)
        {
            // Update key count
            Objectives[0].SetProgress(keyCount);

            // Check if final gate is unlocked (means they entered it)
            if (bossManager.FinalGateUnlocked)
            {
                Objectives[1].SetProgress(1);
            }

            // Check if final boss is defeated
            if (bossManager.IsBossDefeated(FinalBossId))
            {
                Objectives[2].SetProgress(1);
            }

            // Note: Will NOT auto-complete because RequiresAcceptanceToComplete = true
        }

        protected override void OnCompleted()
        {
            Console.WriteLine("\n═══════════════════════════════════════");
            Console.WriteLine("    🏆 FINAL QUEST COMPLETED! 🏆");
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("You have proven yourself the ultimate Champion!");
            Console.WriteLine("Your legend will be remembered for all time.");
            Console.WriteLine("═══════════════════════════════════════\n");
        }
    }
}