using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour, IPooledObject
{
    [SerializeField] private float lifetime = 4f;
    [SerializeField] private bool isPlayerProjectile = true;
    [SerializeField] private float damage = 1f;

    private float direction = 1f;
    private float speed = 12f;
    private float timer;

    public void Configure(float speed, float damage, bool playerProjectile)
    {
        this.speed = speed;
        this.damage = damage;
        isPlayerProjectile = playerProjectile;
        direction = playerProjectile ? 1f : -1f;
        timer = 0f;
    }

    public void OnSpawn()
    {
        timer = 0f;
    }

    public void OnDespawn()
    {
    }

    private void Update()
    {
        transform.position += Vector3.up * direction * speed * Time.deltaTime;
        timer += Time.deltaTime;
        if (timer >= lifetime) ObjectPool.Instance?.Despawn(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isPlayerProjectile)
        {
            if (other.TryGetComponent(out Enemy enemy))
            {
                enemy.TakeDamage(damage);
                ObjectPool.Instance?.Despawn(gameObject);
            }
            else if (other.TryGetComponent(out BossController boss))
            {
                boss.TakeDamage(damage);
                ObjectPool.Instance?.Despawn(gameObject);
            }
            else if (other.TryGetComponent(out Obstacle obstacle))
            {
                obstacle.TakeDamage(damage);
                ObjectPool.Instance?.Despawn(gameObject);
            }
        }
        else if (other.TryGetComponent(out PlayerController player))
        {
            player.TakeDamage();
            ObjectPool.Instance?.Despawn(gameObject);
        }
    }
}
