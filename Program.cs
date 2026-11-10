using System;

/// <summary>
/// The console program (`dotnet run` in the repo root). The game itself is played through
/// the Godot project, so this does nothing (decided 2026-10-13: keep it, cut down;
/// 2026-11-03: a message only; 2026-11-10: no console output anywhere, so the message is
/// commented out too). It no longer calls GameManager.Run(), the old console loop.
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        // Console.WriteLine("RNG the Game is played through the Godot project (godot_project/).");
        // Console.WriteLine("Open it in Godot to play. This console program does nothing else.");
    }
}
