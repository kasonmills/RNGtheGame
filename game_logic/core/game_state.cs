using System;

namespace GameLogic.Core
{
    /// <summary>
    /// Defines all possible game states.
    /// Fixed states are entered with ChangeState; overlays are pushed on top of the current
    /// state with PushState and popped back to it (core/ to-do 17, reshaped 2026-11-03).
    /// Every overlay below has an Open/Close pair on GameManager that checks where it may
    /// be opened from.
    /// </summary>
    public enum GameState
    {
        // === Fixed states ===
        MainMenu,    // Title screen, new game, load game
        Playing,     // Exploring the world, navigating map
        Combat,      // In battle with an enemy
        GameOver,    // The whole party is down
        Cutscene,    // Watching a cutscene (the opening intro before the first boss fight)
        Ending,      // The ending, after the final boss is beaten; goes to Credits
        Credits,     // The credits roll after the ending; goes back to the main menu

        // === Overlays (pushed on top, popped back) ===
        Tutorial,    // Over Combat during the first boss fight's lessons; pops back to Combat when they are done
        Paused,      // Pause menu - from Playing, Combat or Tutorial
        PartyMenu,   // Player + inventory + companions, one menu (replaces the old Inventory and Stats) - from Playing or Paused
        QuestLog,    // Every quest and the tracked quest - from Playing or Paused
        Statistics,  // This save's statistics, read only - from Playing or Paused
        Settings,    // Adjusting game settings - from MainMenu or Paused
        SaveScreen,  // Choosing a slot to save to - from Playing, after the tutorial (and at a save point, once those exist)
        Dialogue,    // Talking to an NPC - from Playing
        Shopping,    // In a shop (buy/sell items) - from Playing
        Blacksmith,  // Upgrading weapons and armor - from Playing
        QuestGiver   // The quest giver's boss quests - from Playing

        // Removed 2026-11-03: Inventory and Stats (merged into PartyMenu); Loading - Godot
        // shows its own loading screen if one is ever needed, so the game logic never has to
        // wait in a "loading" state; JobBoard - no job board in the MVP, each shop/NPC hands
        // out its own unlock quest.
    }
}
