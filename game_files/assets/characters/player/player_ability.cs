using System;
using System.Collections.Generic;

namespace game_files;

// help modify the play based on the ability they chose at the begining of the game.
public class PlayerAbility
{
    public List<Ability> Abilities; // List of all abilities in the game
    private Player _playerStats; // Reference to player stats
    private Weapon _equippedWeapon;
    private Armor _equippedArmor;

    // Constructor
    public PlayerAbility(Player playerStats, Weapon equippedWeapon, Armor equippedArmor)
    {
        // this would be defining one unique ability note a whole list of them... But I will need a list of all the abilites 
        // that are possible but only one is active for the player
        //ability = List<Ability>("");
        _playerStats = playerStats;
        _equippedWeapon = equippedWeapon;
        _equippedArmor = equippedArmor;
    }

    // this method lets the player select what ability they want.
    public void select_ability(Ability ability)
    {
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