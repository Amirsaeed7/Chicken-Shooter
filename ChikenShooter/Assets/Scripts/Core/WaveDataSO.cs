using UnityEngine;

public enum FormationPattern { Rows, Grid, VFormation, Diamond }
public enum EnemyArchetype { Standard, Armored, Cyber, Asteroid }

[CreateAssetMenu(menuName = "Chicken Shooter/Wave Data", fileName = "WaveData")]
public class WaveDataSO : ScriptableObject
{
    public EnemyArchetype enemyType = EnemyArchetype.Standard;
    public GameObject enemyPrefab;
    public int enemyCount = 12;
    public float movementSpeed = 2f;
    public float eggDropRate = 1.5f;
    public FormationPattern formation = FormationPattern.Grid;
    public bool isAsteroidRound;
    public bool isBossRound;
    public int enemyHealth = 1;
    public float bossHealth = 300f;
    public int obstacleCount = 4;
    public float obstacleSpawnInterval = 1.5f;
}
