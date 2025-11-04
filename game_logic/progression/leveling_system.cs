using System;

namespace game_logic;

public class LevelingSystem
{
    public int CurrentLevel { get; private set; }
    public int ExperiencePoints { get; private set; }
    private const int ExperiencePerLevel = 1000;

    public LevelingSystem()
    {
        CurrentLevel = 1;
        ExperiencePoints = 0;
    }

    public void GainExperience(int points)
    {
        ExperiencePoints += points;
        while (ExperiencePoints >= ExperiencePerLevel)
        {
            LevelUp();
            ExperiencePoints -= ExperiencePerLevel;
        }
    }

    private void LevelUp()
    {
        CurrentLevel++;
        Console.WriteLine($"Congratulations! You've reached level {CurrentLevel}.");
    }
}