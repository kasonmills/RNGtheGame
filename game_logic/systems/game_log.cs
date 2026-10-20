using System;

namespace GameLogic.Systems
{
    /// <summary>
    /// One place for game_logic to send its text messages (an item used, an effect starting or
    /// wearing off, an ability levelling up) instead of printing them to the console, which a
    /// Godot screen never sees.
    /// While a fight is running, CombatManager listens and passes every message on through
    /// CombatMessage, so it lands in the combat log. With nobody listening (the console program,
    /// tests) the message is printed to the console as before.
    /// TODO-GODOT: messages outside a fight (level ups, equipping) can be shown by subscribing
    /// to MessageLogged - or, better, replaced by proper events as each screen is built.
    /// </summary>
    public static class GameLog
    {
        /// <summary>
        /// Fired for every message. When nothing is subscribed, the message goes to the console.
        /// </summary>
        public static event Action<string> MessageLogged;

        public static void Write(string message)
        {
            var handler = MessageLogged;
            if (handler != null)
            {
                handler(message);
            }
            else
            {
                Console.WriteLine(message);
            }
        }
    }
}
