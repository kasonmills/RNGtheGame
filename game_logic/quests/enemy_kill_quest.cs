using System;

namespace GameLogic.Quests
{
    /// <summary>
    /// Quest to defeat a certain number of enemies
    /// Can be specific enemy types or any enemies
    /// </summary>
    public class EnemyKillQuest : Quest
    {
        public int RequiredKills { get; private set; }
        public string EnemyType { get; private set; }  // Null = any enemy

        public EnemyKillQuest(string questId, string questName, int requiredKills, int goldReward, int xpReward, string enemyType = null)
            : base(
                questId: questId,
                questName: questName,
                description: GenerateDescription(requiredKills, enemyType),
                reward: new QuestReward(goldReward, xpReward))
        {
            RequiredKills = requiredKills;
            EnemyType = enemyType;

            string objectiveText = string.IsNullOrEmpty(enemyType)
                ? $"Defeat {requiredKills} enemies"
                : $"Defeat {requiredKills} {enemyType}";

            Objectives.Add(new QuestObjective(objectiveText, requiredKills));
        }

        /// <summary>
        /// Update progress when an enemy is defeated
        /// </summary>
        public void OnEnemyDefeated(string defeatedEnemyType)
        {
            // If no specific enemy type required, count all kills
            if (string.IsNullOrEmpty(EnemyType) || defeatedEnemyType == EnemyType)
            {
                Objectives[0].UpdateProgress(1);
                CheckCompletion();
            }
        }

        /// <summary>
        /// Set kill count directly (for retroactive progress)
        /// </summary>
        public void SetKillCount(int count)
        {
            Objectives[0].SetProgress(count);
            CheckCompletion();
        }

        private static string GenerateDescription(int requiredKills, string enemyType)
        {
            if (string.IsNullOrEmpty(enemyType))
            {
                return $"Prove your combat prowess by defeating {requiredKills} enemies. Any foes you encounter will count towards this goal.";
            }
            else
            {
                return $"Hunt down and defeat {requiredKills} {enemyType}. Show your mastery over this particular type of enemy.";
            }
        }
    }
}