using System;
using System.Collections.Generic;

namespace game_files;

public abstract class Entity
{
    public string Name; // each entity needs a name
    public string Description; // a brief description
    public int level; // level of the entity
    public int Health; // health points
    public int MaxHealth; // maximum health points

    public abstract void Execute(Entity target = null);
}
