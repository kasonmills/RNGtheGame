using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace game_files;

// help modify the play based on the ability they chose at the begining of the game.
public class PlayerAbility : Ability
{
    private Player _playerStats; // Reference to player stats
    private Weapon _equippedWeapon;
    private Armor _equippedArmor;

    // Constructor
    public PlayerAbility(string name, string description) : base(name, description)
    {
        // this would be defining one unique ability note a whole list of them... But I will need a list of all the abilites 
        // that are possible but only one is active for the player
        //ability = List<Ability>("");
    }

    public override void ApplyEffectToPlayer(Weapon _equipedWeapon, Armor _equipedArmor)
    {

    }

    public override void ApplyEffectToTarget(Weapon weapon, Armor armor)
    {
    
    }
}