using System;

namespace game_logic;

public class Dungeon
{
    public string Name { get; set; }
    public int Level { get; set; }

    public Dungeon(string name, int level)
    {
        Name = name;
        Level = level;
    }

    public void Enter()
    {
        Console.WriteLine($"Entering the dungeon: {Name} at level {Level}");
    }
}