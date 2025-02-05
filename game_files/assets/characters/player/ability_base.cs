using System;
using System.Collections.Generic;

namespace game_files;

public abstract class Ability
{
    public string Name;
    public string Description;
    public int Ability_level;
    public string what_stat_applies_to;

    // Constructor
    public Ability(string name, string description)
    {
        Name = name;
        Description = description;
    }

    // Applies the ability's effect to the player if that is what the ability does.
    public abstract void ApplyEffectToPlayer(Player playerStats, Weapon weapon, Armor armor);

    // this method will apply the effect to enemies if the ability of the player can do that.
    public abstract void ApplyEffectToEnemy(Player playerStats, Weapon weapon, Armor armor);
}


