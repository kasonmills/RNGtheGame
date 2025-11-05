using System;

namespace GameLogic.Core
{
    /// <summary>
    /// Defines all possible game states
    /// </summary>
    public enum GameState
    {
        MainMenu,    // Title screen, new game, load game
        Playing,     // Exploring the world, navigating map
        Combat,      // In battle with an enemy
        Paused,      // Game paused (can resume)
        GameOver,     // Player died
        Shopping,      // In a shop (buy/sell items)
        Inventory,   // Managing inventory
        Dialogue,    // In conversation with NPC
        Cutscene,     // Watching a cutscene
        Settings,      // Adjusting game settings
        Loading       // Loading game assets
    }
}