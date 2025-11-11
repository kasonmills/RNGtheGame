using System;
using System.Collections.Generic;

namespace GameLogic.Items;

public class QuestItem : Item
{
    public QuestItem(string name, string description) : base(name, description)
    {
    }

    public override void Use(Character user)
    {
        Console.WriteLine($"{user.Name} uses {Name}.");
        // Implement quest item-specific logic here
    }
}
