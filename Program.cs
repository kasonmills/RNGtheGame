using System;

/// <summary>
/// The console program (`dotnet run` in the repo root). The game itself is played through
/// the Godot project, so this only says so and exits (decided 2026-10-13: keep it, cut
/// down; 2026-11-03: a message only). It no longer calls GameManager.Run(), the old console
/// loop. This is the console program itself, so it is the one place that prints.
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("RNG the Game is played through the Godot project (godot_project/).");
        Console.WriteLine("Open it in Godot to play. This console program does nothing else.");
    }
}
