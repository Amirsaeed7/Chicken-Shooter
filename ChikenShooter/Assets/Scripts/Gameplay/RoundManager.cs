using System.Collections;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    [SerializeField] private LevelDataSO levelData;
    [SerializeField] private EnemyEgg eggPrefab;
    [SerializeField] private Drop foodPrefab;
    [SerializeField] private Drop giftPrefab;
    [SerializeField] private BossController bossPrefab;

    private int roundIndex;
    private bool roundComplete;
    private bool roundStarted;

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
            roundStarted = true;
            yield break;
        }

        if (wave.isAsteroidRound)
            SpawnAsteroids(wave);
        else
            SpawnFormation(wave);
        roundStarted = true;
    }

    private void SpawnFormation(WaveDataSO wave)
    {
        int columns = wave.formation == FormationPattern.Rows ? Mathf.Min(wave.enemyCount, 8) : Mathf.CeilToInt(Mathf.Sqrt(wave.enemyCount));
        int rows = Mathf.CeilToInt((float)wave.enemyCount / columns);

        for (int i = 0; i < wave.enemyCount; i++)
        {
            int row = i / columns;
            int column = i % columns;
            Vector3 position = GetFormationPosition(wave.formation, row, column, columns, rows);
            var prefab = wave.enemyPrefab;
            var enemyObject = prefab != null ? Instantiate(prefab, position, Quaternion.identity) : CreateEnemy(position, wave);
            if (enemyObject.TryGetComponent(out Enemy enemy))
                enemy.Configure(wave.enemyHealth, wave.movementSpeed, wave.eggDropRate, eggPrefab, foodPrefab, giftPrefab, i % 7 == 0);
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
        go.AddComponent<Enemy>();
        return go;
    }

    private void SpawnAsteroids(WaveDataSO wave)
    {
        for (int i = 0; i < Mathf.Max(8, wave.enemyCount); i++)
        {
            float x = Mathf.Lerp(-6f, 6f, (i + 1f) / (Mathf.Max(8, wave.enemyCount) + 1f));
            var asteroid = CreateEnemy(new Vector3(x, 3.8f + (i % 3) * 0.7f, 0f), wave);
            asteroid.name = "Asteroid";
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
        bossObject.Configure(wave.bossHealth, eggPrefab, null);
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
        return go.AddComponent<BossController>();
    }

    private void Update()
    {
        if (roundComplete || !roundStarted) return;
        if (FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length == 0 &&
            FindObjectsByType<BossController>(FindObjectsSortMode.None).Length == 0)
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
