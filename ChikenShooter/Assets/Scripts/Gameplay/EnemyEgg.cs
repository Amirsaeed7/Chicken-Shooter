using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyEgg : MonoBehaviour
{
    [SerializeField] private float fallSpeed = 3.5f;
    private Vector2 direction = Vector2.down;

    public static EnemyEgg Spawn(Vector3 position, Vector2 direction, float speed = 3.5f)
    {
        var eggObject = new GameObject("Enemy Egg");
        eggObject.transform.position = position;
        var renderer = eggObject.AddComponent<SpriteRenderer>();
        renderer.sprite = RuntimeSpriteFactory.CreateEgg();
        renderer.sortingOrder = 7;
        eggObject.AddComponent<CircleCollider2D>().isTrigger = true;
        var body = eggObject.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        var egg = eggObject.AddComponent<EnemyEgg>();
        egg.Configure(direction, speed);
        return egg;
    }

    public void Configure(Vector2 direction, float speed)
    {
        this.direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector2.down;
        fallSpeed = speed;
        transform.up = -this.direction;
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * fallSpeed * Time.deltaTime);
        if (transform.position.y < -6f) Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            player.TakeDamage();
            DestroyEgg();
        }
    }

    public void DestroyEgg()
    {
        Destroy(gameObject);
    }
}
