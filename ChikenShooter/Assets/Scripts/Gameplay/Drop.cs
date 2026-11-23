using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Drop : MonoBehaviour
{
    [SerializeField] private float fallSpeed = 2.5f;
    [SerializeField] private bool weaponGift;

    public static Drop Spawn(Vector3 position, bool weaponGift)
    {
        var dropObject = new GameObject(weaponGift ? "Weapon Power-Up" : "Food");
        dropObject.transform.position = position;
        var renderer = dropObject.AddComponent<SpriteRenderer>();
        renderer.sprite = weaponGift ? RuntimeSpriteFactory.CreateWeaponGift() : RuntimeSpriteFactory.CreateFood();
        renderer.sortingOrder = 7;
        dropObject.AddComponent<CircleCollider2D>().isTrigger = true;
        var body = dropObject.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        var drop = dropObject.AddComponent<Drop>();
        drop.Configure(weaponGift);
        return drop;
    }

    private void Awake()
    {
        UpdateSprite();
    }

    private void Configure(bool isWeaponGift)
    {
        weaponGift = isWeaponGift;
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        var renderer = GetComponent<SpriteRenderer>();
        if (renderer == null) return;
        renderer.sprite = weaponGift ? RuntimeSpriteFactory.CreateWeaponGift() : RuntimeSpriteFactory.CreateFood();
        renderer.color = Color.white;
    }

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
        GameManager.Instance.SaveDataToDisk();
        Destroy(gameObject);
    }
}
