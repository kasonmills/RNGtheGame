using System;

namespace RNGtheGame.core;

    public class DamageCalculator
    {
        public int CalculateDamage(int baseDamage, float modifier)
        {
            return (int)(baseDamage * modifier);
        }
    }