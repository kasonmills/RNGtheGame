using System;
namespace RNGtheGame.core
{
    public class CombatManager
    {
        public void StartCombat(Entity attacker, Entity defender)
        {
            Console.WriteLine($"{attacker.Name} engages in combat with {defender.Name}!");
            // Combat logic would go here
        }
    }
}