using System;

namespace GameLogic.Systems;

public class RNGManager
{
    private Random _random;

    public RNGManager()
    {
        _random = new Random();
    }

    /// <summary>
    /// Rolls a random number between min (inclusive) and max (inclusive).
    /// This is the primary RNG method used throughout the game.
    /// </summary>
    public int Roll(int min, int max)
    {
        return _random.Next(min, max + 1);
    }

    public int GetRandomInt(int min, int max)
    {
        return _random.Next(min, max);
    }

    public double GetRandomDouble()
    {
        return _random.NextDouble();
    }

    public bool GetRandomBool()
    {
        return _random.Next(0, 2) == 0;
    }

    /// <summary>
    /// Rolls a percentage chance (1-100). Returns true if roll succeeds.
    /// </summary>
    public bool RollPercentage(int successChance)
    {
        return Roll(1, 100) <= successChance;
    }
}