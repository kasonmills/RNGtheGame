using System;

namespace RNGtheGame.core;

public class CombatAction
{
    public string ActionName { get; set; }
    public int Damage { get; set; }

    public CombatAction(string actionName, int damage)
    {
        ActionName = actionName;
        Damage = damage;
    }

    public void Execute(Entity attacker, Entity defender)
    {
        Console.WriteLine($"{attacker.Name} uses {ActionName} on {defender.Name}, dealing {Damage} damage!");
        // Logic to apply damage to defender would go here
    }    
}