using System;

namespace RNGtheGame.core;

public class TurnManager
{
    private int currentTurn;

    public TurnManager()
    {
        currentTurn = 0;
    }

    public void NextTurn()
    {
        currentTurn++;
        Console.WriteLine($"Turn {currentTurn} begins!");
    }
}
