using System;

namespace game_logic.abilities;

public abstract class Rage : EnemyAbility
{
    public override string Name => "Rage";

    public override void Activate()
    {
        Console.WriteLine("The enemy enters a rage!");
        // Logic to increase enemy's attack power or speed
    }
}