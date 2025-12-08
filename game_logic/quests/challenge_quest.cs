using System;

namespace GameLogic.Quests
{
    /// <summary>
    /// Special challenge quests with specific victory conditions
    /// Examples: flawless victory, speed run, specific combat achievements
    /// </summary>
    public class ChallengeQuest : Quest
    {
        public ChallengeType ChallengeType { get; private set; }

        public ChallengeQuest(string questId, string questName, string description, ChallengeType challengeType, int goldReward, int xpReward, int customRequirement = 0)
            : base(
                questId: questId,
                questName: questName,
                description: description,
                reward: new QuestReward(goldReward, xpReward))
        {
            ChallengeType = challengeType;

            // Add objectives based on challenge type
            switch (challengeType)
            {
                case Quests.ChallengeType.FlawlessVictory:
                    Objectives.Add(new QuestObjective("Win a battle without taking damage", 1));
                    break;

                case Quests.ChallengeType.MultiKill:
                    Objectives.Add(new QuestObjective("Defeat 3 enemies in a single battle", 1));
                    break;

                case Quests.ChallengeType.CriticalMaster:
                    Objectives.Add(new QuestObjective("Land 5 critical hits in a single battle", 1));
                    break;

                case Quests.ChallengeType.Survivor:
                    Objectives.Add(new QuestObjective("Win a battle with less than 10% health remaining", 1));
                    break;

                case Quests.ChallengeType.PerfectBlock:
                    Objectives.Add(new QuestObjective("Block 3 attacks in a single battle", 1));
                    break;

                case Quests.ChallengeType.WinStreak:
                    // Use custom requirement if provided, otherwise default to 5
                    int streakRequired = customRequirement > 0 ? customRequirement : 5;
                    Objectives.Add(new QuestObjective($"Win {streakRequired} battles in a row without fleeing", streakRequired));
                    break;
            }
        }

        /// <summary>
        /// Mark challenge as completed
        /// </summary>
        public void OnChallengeCompleted()
        {
            Objectives[0].UpdateProgress(1);
            CheckCompletion();
        }

        /// <summary>
        /// Update progress for multi-step challenges (like win streaks)
        /// </summary>
        public void UpdateChallengeProgress(int progress)
        {
            Objectives[0].SetProgress(progress);
            CheckCompletion();
        }

        /// <summary>
        /// Reset challenge progress (for streak-based challenges when player fails)
        /// </summary>
        public void ResetProgress()
        {
            Objectives[0].Reset();
        }
    }

    /// <summary>
    /// Types of challenge quests
    /// </summary>
    public enum ChallengeType
    {
        FlawlessVictory,    // Win without taking damage
        MultiKill,          // Defeat multiple enemies in one battle
        CriticalMaster,     // Land multiple critical hits
        Survivor,           // Win with very low health
        PerfectBlock,       // Block multiple attacks
        WinStreak           // Win multiple battles in a row
    }
}