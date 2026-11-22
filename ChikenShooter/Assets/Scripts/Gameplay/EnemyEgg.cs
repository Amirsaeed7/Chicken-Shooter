using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyEgg : MonoBehaviour
{
    [SerializeField] private float fallSpeed = 3.5f;

    private void Update()
    {
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;
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
