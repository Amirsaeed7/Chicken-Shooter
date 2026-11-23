using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BossController : MonoBehaviour
{
    public enum BossState { EggRain, Dive, Ranged, Enraged }

    [SerializeField] private EnemyEgg eggPrefab;
    [SerializeField] private Enemy miniEnemyPrefab;

    private float maxHealth = 300f;
    private float health;
    private float stateTimer;
    private BossState state = BossState.EggRain;
    private Vector3 target;
    private float shotTimer;

    public float HealthNormalized => Mathf.Clamp01(health / maxHealth);
    public BossState CurrentState => state;

    private void Awake()
    {
        target = transform.position;
    }

    private void OnEnable()
    {
        health = maxHealth;
        state = BossState.EggRain;
        stateTimer = 5f;
        shotTimer = 2f;
    }

    public void Configure(float health, EnemyEgg eggPrefab, Enemy miniEnemyPrefab)
    {
        maxHealth = health;
        this.health = health;
        this.eggPrefab = eggPrefab;
        this.miniEnemyPrefab = miniEnemyPrefab;
    }

    private void Update()
    {
        stateTimer -= Time.deltaTime;
        shotTimer -= Time.deltaTime;

        if (health / maxHealth < 0.3f)
            state = BossState.Enraged;
        else if (stateTimer <= 0f)
            AdvanceState();

        switch (state)
        {
            case BossState.EggRain:
                MoveHorizontally();
                if (shotTimer <= 0f)
                {
                    ShootEggCluster();
                    shotTimer = 2f;
                }
                break;
            case BossState.Dive:
                MoveTowardPlayer();
                if (shotTimer <= 0f)
                {
                    ShootEggCluster();
                    shotTimer = 2.2f;
                }
                break;
            case BossState.Ranged:
                MoveHorizontally();
                if (shotTimer <= 0f)
                {
                    ShootEggCluster();
                    shotTimer = 2.5f;
                }
                break;
            case BossState.Enraged:
                MoveTowardPlayer();
                if (shotTimer <= 0f)
                {
                    ShootEggCluster();
                    if (miniEnemyPrefab != null) SpawnMiniEnemy();
                    shotTimer = 1.8f;
                }
                break;
        }
    }

    private void AdvanceState()
    {
        state = state == BossState.Ranged ? BossState.EggRain : (BossState)((int)state + 1);
        stateTimer = state == BossState.Dive ? 4f : 5f;
    }

    private void MoveHorizontally()
    {
        float x = Mathf.Sin(Time.time * 0.8f) * 4.5f;
        transform.position = Vector3.Lerp(transform.position, new Vector3(x, 3.5f, 0f), 2f * Time.deltaTime);
    }

    private void MoveTowardPlayer()
    {
        var player = FindFirstObjectByType<PlayerController>();
        target = player != null ? player.transform.position + Vector3.up : transform.position;
        transform.position = Vector3.Lerp(transform.position, new Vector3(target.x, 2.8f, 0f), 2.5f * Time.deltaTime);
    }

    private void ShootEggCluster()
    {
        var player = FindFirstObjectByType<PlayerController>();
        Vector2 direction = player != null
            ? ((Vector2)player.transform.position - (Vector2)transform.position).normalized
            : Vector2.down;
        int spread = state == BossState.EggRain || state == BossState.Enraged ? 1 : 0;
        float angleStep = state == BossState.Enraged ? 18f : 12f;
        float speed = state == BossState.Enraged ? 4.5f : 3.5f;

        for (int i = -spread; i <= spread; i++)
        {
            Vector2 eggDirection = state == BossState.Ranged || state == BossState.Enraged
                ? Rotate(direction, i * angleStep)
                : Rotate(Vector2.down, i * angleStep);
            Vector3 position = transform.position + Vector3.right * i * 0.35f;
            EnemyEgg egg;
            if (eggPrefab != null)
            {
                egg = Instantiate(eggPrefab, position, Quaternion.identity);
                egg.Configure(eggDirection, speed);
            }
            else
            {
                egg = EnemyEgg.Spawn(position, eggDirection, speed);
            }
        }
    }

    private static Vector2 Rotate(Vector2 vector, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sine = Mathf.Sin(radians);
        float cosine = Mathf.Cos(radians);
        return new Vector2(vector.x * cosine - vector.y * sine, vector.x * sine + vector.y * cosine);
    }

    private void SpawnMiniEnemy()
    {
        var mini = Instantiate(miniEnemyPrefab, transform.position + Random.insideUnitSphere * 0.5f, Quaternion.identity);
        mini.transform.position = new Vector3(mini.transform.position.x, 3f, 0f);
        mini.Configure(1f, 3f, 2f, eggPrefab, null, null, true);
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0f)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.Data.score += 5000;
                GameManager.Instance.NotifyStateChanged();
            }
            Destroy(gameObject);
            FindFirstObjectByType<RoundManager>()?.CompleteRound();
        }
    }

    public void TakeBombDamage()
    {
        TakeDamage(maxHealth * 0.18f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Projectile projectile) && projectile.isActiveAndEnabled)
        {
            // Damage is handled by projectile's enemy detection after boss is assigned as Enemy.
        }
    }
}
