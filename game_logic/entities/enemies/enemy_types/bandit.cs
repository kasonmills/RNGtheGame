using System;

namespace RNGtheGame.core
{
    public class Bandit : Enemy
    {
        public Bandit()
        {
            Name = "Bandit";
            Description = "A sneaky thief who attacks from the shadows.";
            Level = 1;
            Health = 15;
            MaxHealth = 15;
        }
    }
}