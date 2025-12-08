using System;

namespace GameLogic.Quests
{
    /// <summary>
    /// Quest to accumulate a certain amount of gold
    /// Tracks total gold earned (not current gold held)
    /// </summary>
    public class GoldCollectionQuest : Quest
    {
        public int RequiredGold { get; private set; }
        private int _totalGoldEarned;

        public GoldCollectionQuest(int requiredGold, int goldReward, int xpReward)
            : base(
                questId: $"gold_quest_{requiredGold}",
                questName: $"Accumulate {requiredGold} Gold",
                description: $"Build your wealth by earning a total of {requiredGold} gold through combat, quests, and trading. This tracks total gold earned, not current gold held.",
                reward: new QuestReward(goldReward, xpReward))
        {
            RequiredGold = requiredGold;
            _totalGoldEarned = 0;

            Objectives.Add(new QuestObjective($"Earn {requiredGold} gold total", requiredGold));
        }

        /// <summary>
        /// Update progress when gold is earned
        /// </summary>
        public void OnGoldEarned(int amount)
        {
            if (amount > 0)
            {
                _totalGoldEarned += amount;
                Objectives[0].SetProgress(_totalGoldEarned);
                CheckCompletion();
            }
        }

        /// <summary>
        /// Set total gold earned (for retroactive progress or save loading)
        /// </summary>
        public void SetTotalGoldEarned(int total)
        {
            _totalGoldEarned = total;
            Objectives[0].SetProgress(_totalGoldEarned);
            CheckCompletion();
        }

        /// <summary>
        /// Get total gold earned for this quest
        /// </summary>
        public int GetTotalGoldEarned()
        {
            return _totalGoldEarned;
        }
    }
}