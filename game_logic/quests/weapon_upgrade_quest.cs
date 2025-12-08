using System;

namespace GameLogic.Quests
{
    /// <summary>
    /// Quest to upgrade a weapon to a specific level
    /// </summary>
    public class WeaponUpgradeQuest : Quest
    {
        public int TargetLevel { get; private set; }

        public WeaponUpgradeQuest(int targetLevel, int goldReward, int xpReward)
            : base(
                questId: $"weapon_upgrade_{targetLevel}",
                questName: $"Upgrade Weapon to Level {targetLevel}",
                description: $"Master your craft by upgrading any weapon to level {targetLevel}. Use your weapon in combat to gain experience and visit the smithy to upgrade it.",
                reward: new QuestReward(goldReward, xpReward))
        {
            TargetLevel = targetLevel;

            Objectives.Add(new QuestObjective($"Upgrade a weapon to level {targetLevel}", targetLevel));
        }

        /// <summary>
        /// Update progress when weapon is upgraded
        /// </summary>
        public void OnWeaponUpgraded(int newLevel)
        {
            if (newLevel > Objectives[0].CurrentProgress)
            {
                Objectives[0].SetProgress(newLevel);
                CheckCompletion();
            }
        }

        /// <summary>
        /// Check current weapon level (for retroactive progress)
        /// </summary>
        public void CheckWeaponLevel(int currentMaxWeaponLevel)
        {
            Objectives[0].SetProgress(currentMaxWeaponLevel);
            CheckCompletion();
        }
    }
}