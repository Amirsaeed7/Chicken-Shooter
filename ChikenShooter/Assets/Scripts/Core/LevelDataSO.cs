using UnityEngine;

[CreateAssetMenu(menuName = "Chicken Shooter/Level Data", fileName = "LevelData")]
public class LevelDataSO : ScriptableObject
{
    public int levelIndex = 1;
    public string sceneName = "Level1";
    public string stageTheme = "Planetary Orbit";
    public AudioClip music;
    public Color backgroundColor = Color.black;
    public WaveDataSO[] waves;
}
