using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class Obstacle : MonoBehaviour
{
    [SerializeField] private float fallSpeed = 2.5f;
    [SerializeField] private float health = 2f;

    private float rotationSpeed;

    private void Awake()
    {
        var body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        var renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
            renderer.sprite = RuntimeSpriteFactory.CreateAsteroid();
        rotationSpeed = Random.Range(-90f, 90f);
    }

    public void Configure(float speed, float health)
    {
        fallSpeed = speed;
        this.health = health;
    }

    private void Update()
    {
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        if (transform.position.y < -6f)
            Destroy(gameObject);
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0f)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            player.TakeDamage();
            Destroy(gameObject);
        }
    }
}
