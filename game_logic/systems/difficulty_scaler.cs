using system;

namespace GameLogic.Systems;

public class DifficultyScaler
{
    private int _baseDifficulty;
    private double _scalingFactor;

    public DifficultyScaler(int baseDifficulty, double scalingFactor)
    {
        _baseDifficulty = baseDifficulty;
        _scalingFactor = scalingFactor;
    }

    public int GetScaledDifficulty(int playerLevel)
    {
        return (int)(_baseDifficulty + (playerLevel * _scalingFactor));
    }
}