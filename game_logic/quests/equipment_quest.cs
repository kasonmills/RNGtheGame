using System;

namespace GameLogic.Quests
{
    /// <summary>
    /// Quest to equip a full set of equipment (weapon + armor)
    /// Can require specific quality levels
    /// </summary>
    public class EquipmentQuest : Quest
    {
        public int MinimumLevel { get; private set; }

        public EquipmentQuest(int minimumLevel, int goldReward, int xpReward)
            : base(
                questId: $"equipment_quest_{minimumLevel}",
                questName: $"Equip Full Level {minimumLevel}+ Gear",
                description: $"Become a fully equipped warrior by equipping both a weapon and armor of at least level {minimumLevel}. Find or upgrade your equipment to meet this requirement.",
                reward: new QuestReward(goldReward, xpReward))
        {
            MinimumLevel = minimumLevel;

            Objectives.Add(new QuestObjective($"Equip weapon (Level {minimumLevel}+)", 1));
            Objectives.Add(new QuestObjective($"Equip armor (Level {minimumLevel}+)", 1));
        }

        /// <summary>
        /// Check equipment status
        /// </summary>
        public void CheckEquipment(int weaponLevel, int armorLevel)
        {
            // Check weapon
            if (weaponLevel >= MinimumLevel)
            {
                Objectives[0].SetProgress(1);
            }
            else
            {
                Objectives[0].SetProgress(0);
            }

            // Check armor
            if (armorLevel >= MinimumLevel)
            {
                Objectives[1].SetProgress(1);
            }
            else
            {
                Objectives[1].SetProgress(0);
            }

            CheckCompletion();
        }
    }
}