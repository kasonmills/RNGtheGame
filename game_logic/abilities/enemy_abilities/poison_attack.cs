using System;

namespace game_logic.abilities;

public abstract class PoisonAttack : EnemyAbility
{
    public override string Name => "Poison Attack";

    public override void Activate()
    {
        Console.WriteLine("The enemy uses Poison Attack!");
        // Logic to apply poison effect to the player
    }
}