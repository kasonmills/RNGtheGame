using System;

namespace GameLogic.Items
{
    /// <summary>
    /// Quest items - special items required for quests
    /// Quest items cannot be leveled, sold, or traded
    /// </summary>
    public class QuestItem : Item
    {
        // Quest-specific properties
        public string QuestId { get; set; }  // Which quest this item belongs to
        public bool IsKeyItem { get; set; }  // Key items that cannot be discarded

        /// <summary>
        /// Constructor for quest items
        /// </summary>
        public QuestItem(
            string name,
            string description,
            string questId = "",
            bool isKeyItem = false
        ) : base(name, description, ItemRarity.Common, ItemCategory.QuestItem, value: 0)
        {
            QuestId = questId;
            IsKeyItem = isKeyItem;

            // Quest items have special properties
            IsSellable = false;   // Cannot sell quest items
            IsTradeable = false;  // Cannot trade quest items
            IsStackable = false;  // Quest items don't stack
            MaxStackSize = 1;
            Value = 0;  // Quest items have no monetary value
        }

        /// <summary>
        /// Override GetInfo to include quest-specific information
        /// </summary>
        public override string GetInfo()
        {
            string info = $"{GetDisplayName()}\n";
            info += $"{Description}\n";
            info += $"Category: Quest Item\n";

            if (IsKeyItem)
            {
                info += "[KEY ITEM - Cannot be discarded]\n";
            }

            if (!string.IsNullOrEmpty(QuestId))
            {
                info += $"Related Quest: {QuestId}\n";
            }

            return info;
        }

        /// <summary>
        /// Use the quest item
        /// Quest items typically trigger quest events or dialogue
        /// </summary>
        public override void Use(Entities.Player.Player player)
        {
            Console.WriteLine($"{player.Name} examines the {Name}.");
            Console.WriteLine(Description);

            // Quest items might trigger specific events
            // TODO: Implement quest event system
        }

        /// <summary>
        /// Quest items don't modify stats
        /// </summary>
        public override void ModifyStat(Entities.Player.Player player)
        {
            // Quest items don't modify stats
            // This method exists to satisfy the abstract requirement from Item base class
        }
    }
}