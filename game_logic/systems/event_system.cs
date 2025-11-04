using System;

namespace game_logic;

public class EventSystem
{
    public void TriggerEvent(string eventName)
    {
        Console.WriteLine($"Event triggered: {eventName}");
    }

    public void EndEvent(string eventName)
    {
        Console.WriteLine($"Event ended: {eventName}");
    }
}