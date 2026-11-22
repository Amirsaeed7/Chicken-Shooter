using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Drop : MonoBehaviour
{
    [SerializeField] private float fallSpeed = 2.5f;
    [SerializeField] private bool weaponGift;

    private void Update()
    {
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;
        if (transform.position.y < -6f) Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out PlayerController player)) return;

        if (weaponGift)
        {
            player.CollectGift();
        }
        else
        {
            GameManager.Instance.Data.score += 25;
            GameManager.Instance.Data.foodProgress = Mathf.Min(100f, GameManager.Instance.Data.foodProgress + 20f);
            if (GameManager.Instance.Data.foodProgress >= 100f)
            {
                GameManager.Instance.Data.foodProgress = 0f;
                GameManager.Instance.Data.bombs++;
            }
            GameManager.Instance.NotifyStateChanged();
        }
        Destroy(gameObject);
    }
}
