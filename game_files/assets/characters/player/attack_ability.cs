using System;
using System.Collections.Generic;

namespace game_files;

public class AttackBoost : Ability
{
    private int attackIncrease;

    public AttackBoost(string name, string description) : base(name, description)
    {
        attackIncrease = 5;
    }

    public override void ApplyEffectToPlayer(Weapon weapon, Armor armor)
    {
        Console.WriteLine($"{Name} applied! Strength increased by {attackIncrease}.");
    }

    public override void ApplyEffectToTarget(Weapon weapon, Armor armor)
    {
    
    }

    public void RemoveEffect(Weapon weapon, Armor armor)
    {
        Console.WriteLine($"{Name} effect removed! Strength returned to normal.");
    }
}