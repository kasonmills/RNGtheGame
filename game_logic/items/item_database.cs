using System;

namespace GameLogic.Items;

public class ItemDatabase
{
    public static Item GetItemByName(string itemName)
    {
        // This is a placeholder implementation.
        // In a real implementation, this method would look up the item in a database or collection.
        if (itemName == "Health Potion")
        {
            return new Consumable("Health Potion", "Restores a small amount of health.");
        }
        else if (itemName == "Ancient Amulet")
        {
            return new QuestItem("Ancient Amulet", "A mysterious amulet needed for a quest.");
        }
        else
        {
            throw new ArgumentException($"Item '{itemName}' not found in the database.");
        }
    }
}