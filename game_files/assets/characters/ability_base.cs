using System;
using System.Collections.Generic;

namespace game_files;

public abstract class Ability
{
    public string Name; // each ability needs a name
    public string Description; // a brief description
    public int Ability_level; // what level the ability is on

    public string Ability_target;

    public abstract void Execute(Player player, Entity target = null);
}
