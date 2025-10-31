using System;
using System.Collections.Generic;

namespace game_files;

public class HealingAbility : Ability
{
    private int healAmount;

    public HealingAbility(string name, string description) :base(name, description)
    {
        healAmount = 20;
    }

    public override void ApplyEffectToPlayer()
    {
        Console.WriteLine($"{Name} used! Restored {healAmount} HP.");
    }

    public override void ApplyEffectToTarget()
    {
    
    }

    public void RemoveEffect()
    {
        // Healing is a one-time effect, so nothing to remove
    }
}