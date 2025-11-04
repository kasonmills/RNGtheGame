using system;

namespace game_logic;

public class SaveSystem
{
    public void SaveGame(string filePath)
    {
        Console.WriteLine($"Game saved to {filePath}");
    }

    public void LoadGame(string filePath)
    {
        Console.WriteLine($"Game loaded from {filePath}");
    }
}