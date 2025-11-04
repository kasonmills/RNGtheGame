using System;

namespace game_logic.abilities;

public class Consumable : Item
{
    public Consumable(string name, string description) : base(name, description)
    {
    }

    public override void Use(Character user)
    {
        Console.WriteLine($"{user.Name} uses {Name}.");
        // Implement consumable-specific logic here
    }
}