using System;
using GameLogic.Progression;

namespace GameLogic.Quests
{
    /// <summary>
    /// Quest to defeat a specific boss and obtain their champion key
    /// </summary>
    public class BossDefeatQuest : Quest
    {
        public string BossId { get; private set; }
        public string BossName { get; private set; }

        public BossDefeatQuest(string bossId, string bossName, int goldReward, int xpReward)
            : base(
                questId: $"boss_defeat_{bossId}",
                questName: $"Defeat {bossName}",
                description: $"Challenge and defeat the Champion known as {bossName}. Claim their Champion Key as proof of your victory.",
                reward: new QuestReward(goldReward, xpReward))
        {
            BossId = bossId;
            BossName = bossName;

            // Single objective: defeat the boss
            Objectives.Add(new QuestObjective($"Defeat {bossName}", 1));
        }

        /// <summary>
        /// Update progress when a boss is defeated
        /// </summary>
        public void OnBossDefeated(string defeatedBossId)
        {
            if (defeatedBossId == BossId)
            {
                Objectives[0].UpdateProgress(1);
                CheckCompletion();
            }
        }

        /// <summary>
        /// Check if this boss has been defeated (retroactive check)
        /// </summary>
        public void CheckBossStatus(BossManager bossManager)
        {
            if (bossManager.IsBossDefeated(BossId))
            {
                Objectives[0].SetProgress(1);
                CheckCompletion();
            }
        }
    }
}