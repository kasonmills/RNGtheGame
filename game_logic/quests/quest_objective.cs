using System;

namespace GameLogic.Quests
{
    /// <summary>
    /// Represents a single objective within a quest
    /// Tracks progress towards completion
    /// </summary>
    public class QuestObjective
    {
        public string Description { get; set; }
        public int CurrentProgress { get; set; }
        public int RequiredProgress { get; set; }
        public bool IsCompleted => CurrentProgress >= RequiredProgress;

        public QuestObjective(string description, int requiredProgress)
        {
            Description = description;
            RequiredProgress = requiredProgress;
            CurrentProgress = 0;
        }

        /// <summary>
        /// Update progress towards this objective
        /// </summary>
        /// <param name="amount">Amount to add to progress</param>
        public void UpdateProgress(int amount)
        {
            CurrentProgress += amount;
            if (CurrentProgress > RequiredProgress)
            {
                CurrentProgress = RequiredProgress;
            }
        }

        /// <summary>
        /// Set progress to a specific value
        /// </summary>
        public void SetProgress(int progress)
        {
            CurrentProgress = progress;
            if (CurrentProgress > RequiredProgress)
            {
                CurrentProgress = RequiredProgress;
            }
        }

        /// <summary>
        /// Get progress as a formatted string
        /// </summary>
        public string GetProgressString()
        {
            return $"{CurrentProgress}/{RequiredProgress}";
        }

        /// <summary>
        /// Get progress as a percentage (0-100)
        /// </summary>
        public int GetProgressPercentage()
        {
            if (RequiredProgress == 0) return 100;
            return (int)((CurrentProgress / (float)RequiredProgress) * 100);
        }

        /// <summary>
        /// Reset progress to 0
        /// </summary>
        public void Reset()
        {
            CurrentProgress = 0;
        }
    }
}