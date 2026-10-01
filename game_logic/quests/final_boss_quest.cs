using System;
using GameLogic.Entities.Enemies.Bosses;

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
                questName: "The Final Challenge",
                description: $"Face the ultimate challenge and defeat {finalBossName}. This is the culmination of your journey. Victory here will prove you are the greatest warrior in the realm.",
                reward: new QuestReward(), // No rewards - game ends
                requiresAcceptance: true)   // MUST accept before completing
        {
            FinalBossId = finalBossId;
            FinalBossName = finalBossName;

            // Objectives
            Objectives.Add(new QuestObjective($"Defeat {finalBossName}", 1));
        }

        /// <summary>
        /// Mark final boss as defeated
        /// </summary>
        public void OnFinalBossDefeated()
        {
            Objectives[0].SetProgress(1);
            CheckCompletion();
        }

        /// <summary>
        /// Check progress based on current game state (retroactive check)
        /// </summary>
        public void CheckProgress(BossManager bossManager)
        {
            if (bossManager.IsBossDefeated(FinalBossId))
            {
                Objectives[0].SetProgress(1);
            }

            // Note: Will NOT auto-complete because RequiresAcceptanceToComplete = true
        }

        protected override void OnCompleted()
        {
            Console.WriteLine("\n═══════════════════════════════════════");
            Console.WriteLine("    🏆 FINAL QUEST COMPLETED! 🏆");
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("You have proven yourself the realm's greatest warrior!");
            Console.WriteLine("Your legend will be remembered for all time.");
            Console.WriteLine("═══════════════════════════════════════\n");
        }
    }
}