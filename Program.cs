using System;

using GameLogic.Core;

namespace game;

class Program
{
    /*
    this is the main program hub all will be controled by the gamemanager
    */
    static void Main(string[] args)
    {
        GameManager game = new GameManager();
        game.StartNewGame();
        // Run text-based game loop for testing
        game.Run();
    }
}

