using System;
using System.Collections.Generic;

namespace game_files;

public class HealingAbility : Ability
{
    private int healAmount;

    public HealingAbility() : base("Healing Light", "Restores 20 HP.", 2)
    {
        healAmount = 20;
    }

    public override void ApplyEffect(PlayerStats playerStats, Weapon weapon, Armor armor)
    {
        playerStats.Heal(healAmount);
        Console.WriteLine($"{Name} used! Restored {healAmount} HP.");
    }

    public override void RemoveEffect(PlayerStats playerStats, Weapon weapon, Armor armor)
    {
        // Healing is a one-time effect, so nothing to remove
    }
}