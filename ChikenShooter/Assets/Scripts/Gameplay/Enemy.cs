using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Enemy : MonoBehaviour
{
    [SerializeField] private float health = 1f;
    [SerializeField] private float movementSpeed = 2f;
    [SerializeField] private float eggDropRate = 1.5f;
    [SerializeField] private EnemyEgg eggPrefab;
    [SerializeField] private Drop foodPrefab;
    [SerializeField] private Drop giftPrefab;
    [SerializeField] private bool diveBomber;

    private float eggTimer;
    private Vector3 formationBase;
    private float phase;
    private bool returnedToFormation;

    public void Configure(float health, float movementSpeed, float eggDropRate, EnemyEgg eggPrefab, Drop foodPrefab, Drop giftPrefab, bool diveBomber)
    {
        this.health = health;
        this.movementSpeed = movementSpeed;
        this.eggDropRate = eggDropRate;
        this.eggPrefab = eggPrefab;
        this.foodPrefab = foodPrefab;
        this.giftPrefab = giftPrefab;
        this.diveBomber = diveBomber;
        formationBase = transform.position;
        phase = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        eggTimer -= Time.deltaTime;
        if (diveBomber)
        {
            transform.position += (Vector3.down + Vector3.right * Mathf.Sin(Time.time * 3f + phase) * 0.5f) * movementSpeed * Time.deltaTime;
            if (transform.position.y < -5.5f) Destroy(gameObject);
        }
        else
        {
            transform.position = formationBase + Vector3.right * Mathf.Sin(Time.time * movementSpeed * 0.45f + phase) * 0.45f;
            if (eggTimer <= 0f)
            {
                eggTimer = eggDropRate * Random.Range(0.75f, 1.35f);
                if (eggPrefab != null) Instantiate(eggPrefab, transform.position, Quaternion.identity);
            }
        }
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0f) Die();
    }

    public void TakeBombDamage()
    {
        Die();
    }

    private void Die()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.Data.score += 100;
            GameManager.Instance.NotifyStateChanged();
        }

        float roll = Random.value;
        if (roll < 0.12f && giftPrefab != null)
            Instantiate(giftPrefab, transform.position, Quaternion.identity);
        else if (roll < 0.38f && foodPrefab != null)
            Instantiate(foodPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            player.TakeDamage();
            Die();
        }
    }
}
