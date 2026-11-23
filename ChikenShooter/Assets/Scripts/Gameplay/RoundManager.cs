using System.Collections;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    private const int EnemyCountReduction = 1;
    private const int ObstacleCountReduction = 1;
    private const float EnemySpeedScale = 0.9f;
    private const float EggIntervalScale = 1.15f;
    private const float BossHealthScale = 0.9f;

    [SerializeField] private LevelDataSO levelData;
    [SerializeField] private EnemyEgg eggPrefab;
    [SerializeField] private Drop foodPrefab;
    [SerializeField] private Drop giftPrefab;
    [SerializeField] private BossController bossPrefab;
    [SerializeField] private Obstacle obstaclePrefab;

    private int roundIndex;
    private bool roundComplete;
    private bool roundStarted;
    private bool obstaclesSpawning;

    private void Start()
    {
        if (GameManager.Instance != null)
            roundIndex = Mathf.Max(1, GameManager.Instance.Data.roundIndex);
        else
            roundIndex = 1;

        StartCoroutine(RunRound());
    }

    private IEnumerator RunRound()
    {
        yield return new WaitForSeconds(0.5f);
        if (levelData == null || levelData.waves == null || levelData.waves.Length == 0)
        {
            Debug.LogWarning("RoundManager has no LevelData or waves.");
            yield break;
        }

        int waveIndex = Mathf.Clamp(roundIndex - 1, 0, levelData.waves.Length - 1);
        var wave = levelData.waves[waveIndex];
        if (wave.isBossRound)
        {
            SpawnBoss(wave);
            StartObstacleWave(wave);
            roundStarted = true;
            yield break;
        }

        if (wave.isAsteroidRound)
            SpawnAsteroids(wave);
        else
            SpawnFormation(wave);
        StartObstacleWave(wave);
        roundStarted = true;
    }

    private void StartObstacleWave(WaveDataSO wave)
    {
        int count = Mathf.Max(3, wave.obstacleCount - ObstacleCountReduction);
        obstaclesSpawning = true;
        StartCoroutine(SpawnObstacles(wave, count));
    }

    private IEnumerator SpawnObstacles(WaveDataSO wave, int count)
    {
        var camera = Camera.main;
        for (int i = 0; i < count; i++)
        {
            Vector3 position;
            if (camera != null)
            {
                position = camera.ViewportToWorldPoint(new Vector3(Random.Range(0.1f, 0.9f), 1.08f, 0f));
                position.z = 0f;
            }
            else
            {
                position = new Vector3(Random.Range(-4.5f, 4.5f), 6f, 0f);
            }

            SpawnObstacle(position, Random.Range(2f, 3.5f), Random.Range(1.5f, 3f));
            if (i < count - 1)
                yield return new WaitForSeconds(Mathf.Max(0.5f, wave.obstacleSpawnInterval));
        }
        obstaclesSpawning = false;
    }

    private Obstacle SpawnObstacle(Vector3 position, float fallSpeed, float health)
    {
        var obstacle = obstaclePrefab != null
            ? Instantiate(obstaclePrefab, position, Quaternion.identity)
            : CreateObstacle(position);
        obstacle.Configure(fallSpeed, health);
        return obstacle;
    }

    private Obstacle CreateObstacle(Vector3 position)
    {
        var go = new GameObject("Falling Asteroid");
        go.transform.position = position;
        go.transform.localScale = Vector3.one * Random.Range(0.65f, 1.05f);
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = RuntimeSpriteFactory.CreateAsteroid();
        renderer.sortingOrder = 6;
        go.AddComponent<CircleCollider2D>().isTrigger = true;
        go.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        return go.AddComponent<Obstacle>();
    }

    private void SpawnFormation(WaveDataSO wave)
    {
        int enemyCount = Mathf.Max(1, wave.enemyCount - EnemyCountReduction);
        int columns = wave.formation == FormationPattern.Rows ? Mathf.Min(enemyCount, 8) : Mathf.CeilToInt(Mathf.Sqrt(enemyCount));
        int rows = Mathf.CeilToInt((float)enemyCount / columns);

        for (int i = 0; i < enemyCount; i++)
        {
            int row = i / columns;
            int column = i % columns;
            Vector3 position = GetFormationPosition(wave.formation, row, column, columns, rows);
            var prefab = wave.enemyPrefab;
            var enemyObject = prefab != null ? Instantiate(prefab, position, Quaternion.identity) : CreateEnemy(position, wave);
            if (enemyObject.TryGetComponent(out Enemy enemy))
            {
                var renderer = enemyObject.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.sprite = RuntimeSpriteFactory.CreateChicken(wave.enemyType);
                    renderer.color = Color.white;
                }
                enemy.Configure(
                    Mathf.Max(1, wave.enemyHealth - 1),
                    wave.movementSpeed * EnemySpeedScale,
                    wave.eggDropRate * EggIntervalScale,
                    eggPrefab,
                    foodPrefab,
                    giftPrefab,
                    i % 7 == 0);
            }
        }
    }

    private GameObject CreateEnemy(Vector3 position, WaveDataSO wave)
    {
        var go = new GameObject(wave.enemyType.ToString() + " Chicken");
        go.transform.position = position;
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = RuntimeSpriteFactory.CreateCircle(Color.white);
        renderer.color = wave.enemyType switch
        {
            EnemyArchetype.Armored => new Color(0.55f, 0.75f, 1f),
            EnemyArchetype.Cyber => new Color(0.8f, 0.25f, 1f),
            EnemyArchetype.Asteroid => Color.gray,
            _ => new Color(1f, 0.82f, 0.35f)
        };
        renderer.sortingOrder = 5;
        go.transform.localScale = Vector3.one * 0.75f;
        go.AddComponent<CircleCollider2D>().isTrigger = true;
        go.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        go.AddComponent<Enemy>();
        return go;
    }

    private void SpawnAsteroids(WaveDataSO wave)
    {
        int asteroidCount = Mathf.Max(7, wave.enemyCount - EnemyCountReduction);
        for (int i = 0; i < asteroidCount; i++)
        {
            float x = Mathf.Lerp(-6f, 6f, (i + 1f) / (asteroidCount + 1f));
            var asteroid = SpawnObstacle(
                new Vector3(x, 6f + (i % 3) * 0.7f, 0f),
                Random.Range(2f, 3.5f),
                Random.Range(1.5f, 3f));
            asteroid.name = "Falling Asteroid";
        }
    }

    private Vector3 GetFormationPosition(FormationPattern pattern, int row, int column, int columns, int rows)
    {
        float xSpacing = 1.25f;
        float y = 3.4f - row * 0.9f;
        float x = (column - (columns - 1) * 0.5f) * xSpacing;

        if (pattern == FormationPattern.VFormation)
            x = (column - (columns - 1) * 0.5f) * xSpacing + row * (row % 2 == 0 ? 0.35f : -0.35f);
        else if (pattern == FormationPattern.Diamond)
            x += Mathf.Abs(row - (rows - 1) * 0.5f) * 0.45f;

        return new Vector3(x, y, 0f);
    }

    private void SpawnBoss(WaveDataSO wave)
    {
        var bossObject = bossPrefab != null
            ? Instantiate(bossPrefab, new Vector3(0f, 3.5f, 0f), Quaternion.identity)
            : CreateBoss(wave);
        var renderer = bossObject.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.sprite = RuntimeSpriteFactory.CreateBoss();
            renderer.color = Color.white;
        }
        bossObject.Configure(wave.bossHealth * BossHealthScale, eggPrefab, null);
    }

    private BossController CreateBoss(WaveDataSO wave)
    {
        var go = new GameObject("Stage Boss");
        go.transform.position = new Vector3(0f, 3.5f, 0f);
        go.transform.localScale = Vector3.one * 2.2f;
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = RuntimeSpriteFactory.CreateCircle(Color.red);
        renderer.color = new Color(0.95f, 0.35f, 0.25f);
        renderer.sortingOrder = 4;
        var collider = go.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        go.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        return go.AddComponent<BossController>();
    }

    private void Update()
    {
        if (roundComplete || !roundStarted) return;
        if (FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length == 0 &&
            FindObjectsByType<BossController>(FindObjectsSortMode.None).Length == 0 &&
            !obstaclesSpawning &&
            FindObjectsByType<Obstacle>(FindObjectsSortMode.None).Length == 0)
        {
            CompleteRound();
        }
    }

    public void CompleteRound()
    {
        if (roundComplete) return;
        roundComplete = true;

        int expectedRounds = levelData != null ? levelData.waves.Length : 5;
        if (roundIndex >= expectedRounds)
        {
            StartCoroutine(ShowLevelComplete());
        }
        else
        {
            GameManager.Instance.Data.roundIndex = roundIndex + 1;
            GameManager.Instance.SaveDataToDisk();
            StartCoroutine(RestartNextRound());
        }
    }

    private IEnumerator RestartNextRound()
    {
        yield return new WaitForSeconds(1.5f);
        UnityEngine.SceneManagement.SceneManager.LoadScene(gameObject.scene.name);
    }

    private IEnumerator ShowLevelComplete()
    {
        yield return new WaitForSeconds(1.5f);
        int levelIndex = levelData != null ? levelData.levelIndex : 1;
        GameManager.Instance.CompleteLevel(levelIndex);
    }
}
