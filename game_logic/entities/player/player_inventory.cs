using System;
using System.Collections.Generic;

namespace game_files;

public class AttackBoost : Ability
{
    // child class for abilities that boost attack for the player or all teammates

    // the attack boost could mean several things. It could bring up the floor (minimum damage)
    // it could increase the ceiling (maximum damage)
    // it could increase critical hit chance
    // it should shift the whole damage range (increase the minimum and maximum by a small amount)
    private int attackIncrease;

    public AttackBoost(string name, string description, int level, string target) : base(name, description, level, target)
    {
        // ability affect if it only targets the player
        attackIncrease = 5;
    }

    public AttackBoost(string name, string description, int level, string target, int amount_of_targets) : base(name, description, level, target)
    {
        // ability affect for the players teammates
        attackIncrease = 5;
    }

    public override void ApplyEffectToPlayer(Weapon weapon)
    {
        Console.WriteLine($"{Name} applied! Strength increased by {attackIncrease}.");
    }

    public override void ApplyEffectToTarget(Weapon weapon)
    {
    
    }

    public void RemoveEffect(Weapon weapon)
    {
        Console.WriteLine($"{Name} effect removed! Strength returned to normal.");
    }
}