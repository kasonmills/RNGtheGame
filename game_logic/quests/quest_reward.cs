using System;
using System.Collections.Generic;
using GameLogic.Items;

namespace GameLogic.Quests
{
    /// <summary>
    /// Represents the rewards for completing a quest
    /// Can include gold, experience, and items
    /// </summary>
    public class QuestReward
    {
        public int Gold { get; set; }
        public int Experience { get; set; }
        public List<Item> Items { get; set; }

        public QuestReward()
        {
            Gold = 0;
            Experience = 0;
            Items = new List<Item>();
        }

        public QuestReward(int gold, int experience, List<Item> items = null)
        {
            Gold = gold;
            Experience = experience;
            Items = items ?? new List<Item>();
        }

        /// <summary>
        /// Check if this reward has any contents
        /// </summary>
        public bool HasRewards()
        {
            return Gold > 0 || Experience > 0 || Items.Count > 0;
        }

        /// <summary>
        /// Get a formatted string of all rewards
        /// </summary>
        public string GetRewardSummary()
        {
            if (!HasRewards())
            {
                return "No rewards";
            }

            List<string> rewardParts = new List<string>();

            if (Gold > 0)
            {
                rewardParts.Add($"{Gold} Gold");
            }

            if (Experience > 0)
            {
                rewardParts.Add($"{Experience} XP");
            }

            if (Items.Count > 0)
            {
                foreach (var item in Items)
                {
                    rewardParts.Add(item.Name);
                }
            }

            return string.Join(", ", rewardParts);
        }

        /// <summary>
        /// Display rewards to console
        /// </summary>
        public void DisplayRewards()
        {
            if (!HasRewards())
            {
                Console.WriteLine("No rewards.");
                return;
            }

            Console.WriteLine("═══ QUEST REWARDS ═══");

            if (Gold > 0)
            {
                Console.WriteLine($"💰 Gold: {Gold}");
            }

            if (Experience > 0)
            {
                Console.WriteLine($"⭐ Experience: {Experience}");
            }

            if (Items.Count > 0)
            {
                Console.WriteLine("📦 Items:");
                foreach (var item in Items)
                {
                    Console.WriteLine($"   • {item.Name}");
                }
            }
        }
    }
}