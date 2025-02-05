using System;
using System.Collections.Generic;

namespace game_files;

public class AttackBoost : Ability
{
    private int attackIncrease;

    public AttackBoost() : base("Attack Boost", "Temporarily increases attack power by 5.", 3)
    {
        attackIncrease = 5;
    }

    public override void ApplyEffect(PlayerStats playerStats, Weapon weapon, Armor armor)
    {
        playerStats.Strength += attackIncrease;
        Console.WriteLine($"{Name} applied! Strength increased by {attackIncrease}.");
    }

    public override void RemoveEffect(PlayerStats playerStats, Weapon weapon, Armor armor)
    {
        playerStats.Strength -= attackIncrease;
        Console.WriteLine($"{Name} effect removed! Strength returned to normal.");
    }
}