using System;

public static class LevelProgression
{
    public const int Columns = 10;
    private const int StartingRows = 3;

    public static float GetPlayerMovementMultiplier(int level)
    {
        if (level < 1) throw new ArgumentOutOfRangeException(nameof(level));
        return 1f + (level - 1) * GameSettings.PlayerMovementBonusPerLevel;
    }

    public static float GetPlayerFireRateMultiplier(int level)
    {
        if (level < 1) throw new ArgumentOutOfRangeException(nameof(level));
        return 1f + (level - 1) * GameSettings.PlayerFireRateBonusPerLevel;
    }

    public static int GetAlienCount(int level)
    {
        if (level < 1) throw new ArgumentOutOfRangeException(nameof(level));

        int count = Columns * StartingRows;
        for (int currentLevel = 1; currentLevel < level; currentLevel++)
        {
            int completedRows = count / Columns;
            int spacesInNextRow = Columns - count % Columns;
            count = checked(count + Math.Min(completedRows, spacesInNextRow));
        }
        return count;
    }
}
