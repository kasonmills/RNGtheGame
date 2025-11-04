using System;

namespace game_logic;

public class City
{
    public string Name { get; set; }
    public int Population { get; set; }
    public LocationType LocationType { get; set; }

    public City(string name, int population, LocationType locationType)
    {
        Name = name;
        Population = population;
        LocationType = locationType;
    }
}