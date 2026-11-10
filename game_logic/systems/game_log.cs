using System;

namespace GameLogic.Systems
{
    /// <summary>
    /// One place for game_logic to send its text messages (an item used, an effect starting or
    /// wearing off, an ability levelling up) instead of printing them to the console, which a
    /// Godot screen never sees.
    /// While a fight is running, CombatManager listens and passes every message on through
    /// CombatMessage, so it lands in the combat log. Since 2026-11-10 every message that used to
    /// be printed to the console anywhere in game_logic comes through here too (shops, the
    /// blacksmith, levelling, equipping, saving, bosses, quests...). With nobody listening the
    /// message is simply dropped - game_logic never prints to the console.
    /// TODO-GODOT: an autoload can subscribe to MessageLogged to show messages outside a fight
    /// (pop-ups, a message log) - or, better, they get replaced by proper events as each screen
    /// is built.
    /// </summary>
    public static class GameLog
    {
        /// <summary>
        /// Fired for every message. When nothing is subscribed, the message is dropped.
        /// </summary>
        public static event Action<string> MessageLogged;

        public static void Write(string message)
        {
            MessageLogged?.Invoke(message);

            // No console fallback any more (no console output, 2026-11-10). It was:
            // else
            // {
            //     Console.WriteLine(message);
            // }
        }
    }
}
