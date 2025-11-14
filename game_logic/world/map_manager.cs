using System;

namespace GameLogic.World;

public class MapManager
{
    private string _currentLocationName = "Starting Area";

    public void LoadMap(string mapName)
    {
        Console.WriteLine($"Loading map: {mapName}");
        // TODO: Implement map loading logic here
    }

    /// <summary>
    /// Generate a new map for a new game
    /// </summary>
    public void GenerateNewMap()
    {
        Console.WriteLine("Generating new map...");
        _currentLocationName = "Starting Area";
        // TODO: Implement map generation logic
    }

    /// <summary>
    /// Get the name of the current location
    /// </summary>
    public string GetCurrentLocationName()
    {
        return _currentLocationName;
    }
}