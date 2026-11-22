using System;

[Serializable]
public class SaveData
{
    public int levelIndex = 1;
    public int roundIndex = 1;
    public int score;
    public int lives = 3;
    public int bombs = 2;
    public int powerLevel = 1;
    public float foodProgress;
}
