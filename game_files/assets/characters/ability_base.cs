using System;
using System.Collections.Generic;

namespace game_files;

public abstract class Ability
{
    public List<Ability> Abilities; // List of all abilities in the game
    public string Name; // each ability needs a name
    public string Description; // a brief description
    public int Ability_level; // what level the ability is on

    public string Ability_target;

    // Constructor
    public Ability(string name, string description, int ability_level, string ability_target)
    {
        Name = name;
        Description = description;
        Ability_level = ability_level;
        Ability_target = ability_target;
    }

    // Applies the ability's effect to the player if that is what the ability does.
    public abstract void ApplyEffectToPlayer(Weapon weapon);

    // this method will apply the effect to enemies if the ability of the player can do that.
    public abstract void ApplyEffectToTarget(Weapon weapon);
}


