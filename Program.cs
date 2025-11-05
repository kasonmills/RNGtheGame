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
        // console test only comment out when using godot
        try
        {
            GameManager game = new GameManager();
            game.StartNewGame();
            game.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine("\n=== FATAL ERROR ===");
            Console.WriteLine($"The game crashed: {ex.Message}");
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
        // end console test
    }
}

