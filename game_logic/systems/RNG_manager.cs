using System;

namespace game_logic;

public class RNGManager
{
    private Random _random;

    public RNGManager()
    {
        _random = new Random();
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
}