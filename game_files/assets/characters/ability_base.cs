using System;
using System.Collections.Generic;

namespace game_files;

public class Ability
{
    public List<Ability> Abilities; // List of all abilities in the game
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
    public abstract void ApplyEffectToTarget(Player playerStats, Weapon weapon, Armor armor);

     public void select_ability(Ability ability)
    {
        // this method lets the player select what ability they want.
        switch (ability)
        {
            // I haven't decided how many abilites they can choose from but I know there will be at least two..
            case " ": // I am keeping it blank for now because I need to work on this more.
            {
                break;
            }
            default:
            {
                break;
            }
        }
    }
}


