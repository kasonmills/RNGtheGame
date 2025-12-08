using System;

namespace GameLogic.Quests
{
    /// <summary>
    /// Quest to reach a specific character level
    /// </summary>
    public class LevelQuest : Quest
    {
        public int TargetLevel { get; private set; }

        public LevelQuest(int targetLevel, int goldReward, int xpReward)
            : base(
                questId: $"level_quest_{targetLevel}",
                questName: $"Reach Level {targetLevel}",
                description: $"Train and grow stronger until you reach level {targetLevel}. Gain experience through combat and overcome challenges to level up.",
                reward: new QuestReward(goldReward, xpReward))
        {
            TargetLevel = targetLevel;

            Objectives.Add(new QuestObjective($"Reach level {targetLevel}", targetLevel));
        }

        /// <summary>
        /// Update progress when player levels up
        /// </summary>
        public void UpdateLevel(int currentLevel)
        {
            Objectives[0].SetProgress(currentLevel);
            CheckCompletion();
        }
    }
}