using System;
using System.Collections.Generic;

namespace game_files;

// Base Item Class (Extend for Weapons, Armor, etc.)
public class Item
{
    public string Name;

    public Item(string name)
    {
        Name = name;
    }

    public Item(string name, Ability ability)
    {
        Name = name;
        Ability Ability1 = ability;
    }
}

