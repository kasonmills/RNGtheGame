using System;

namespace GameLogic.World;

public class MapNode
{
    public string Name { get; set; }
    public string Description { get; set; }

    public MapNode(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public void EnterNode()
    {
        Console.WriteLine($"Entering node: {Name}");
        Console.WriteLine(Description);
        // Implement additional logic for entering the map node here
    }
}