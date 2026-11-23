using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private WeaponDataSO weaponData;
    [SerializeField] private PlayerInput playerInput;

    private Vector2 moveInput;
    private float invincibleTimer;
    private SpriteRenderer spriteRenderer;
    private float fireTimer;
    private int lastBombUseFrame = -1;

    public int Lives => GameManager.Instance.Data.lives;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.sprite = RuntimeSpriteFactory.CreateSpaceship();
        if (playerInput == null) playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        if (playerInput != null && playerInput.actions != null)
        {
            var move = playerInput.actions.FindAction("Move");
            var bomb = playerInput.actions.FindAction("Bomb");
            if (move != null) move.performed += OnMove;
            if (move != null) move.canceled += OnMove;
            if (bomb != null) bomb.performed += OnBomb;
        }
    }

    private void OnDisable()
    {
        if (playerInput != null && playerInput.actions != null)
        {
            var move = playerInput.actions.FindAction("Move");
            var bomb = playerInput.actions.FindAction("Bomb");
            if (move != null)
            {
                move.performed -= OnMove;
                move.canceled -= OnMove;
            }
            if (bomb != null) bomb.performed -= OnBomb;
        }
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.xKey.wasPressedThisFrame)
            UseBomb();

        if (Keyboard.current != null &&
            (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame))
            FindFirstObjectByType<PauseMenu>()?.Toggle();

        transform.Translate(moveInput * moveSpeed * Time.deltaTime, Space.World);
        var bounds = Camera.main.ViewportToWorldPoint(new Vector3(0.07f, 0.07f, 0f));
        var max = Camera.main.ViewportToWorldPoint(new Vector3(0.93f, 0.93f, 0f));
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, bounds.x, max.x),
            Mathf.Clamp(transform.position.y, bounds.y, max.y),
            transform.position.z);

        fireTimer -= Time.deltaTime;
        if (invincibleTimer > 0f)
        {
            invincibleTimer -= Time.deltaTime;
            if (spriteRenderer != null)
                spriteRenderer.enabled = Mathf.FloorToInt(invincibleTimer * 12f) % 2 == 0;
        }
        else if (spriteRenderer != null) spriteRenderer.enabled = true;

        bool firing = false;
        if (playerInput != null && playerInput.actions != null)
        {
            var attack = playerInput.actions.FindAction("Attack");
            if (attack != null) firing = attack.IsPressed();
        }
        else if (Keyboard.current != null)
        {
            firing = Keyboard.current.spaceKey.isPressed;
        }

        if (firing) TryFire();
    }

    private void TryFire()
    {
        if (weaponData == null || weaponData.projectilePrefab == null || fireTimer > 0f) return;
        fireTimer = weaponData.fireRate;
        var power = Mathf.Clamp(GameManager.Instance.Data.powerLevel, 1, 3);

        if (power == 1)
        {
            FireProjectile(0f);
        }
        else if (power == 2)
        {
            FireProjectile(-0.25f);
            FireProjectile(0.25f);
        }
        else
        {
            FireProjectile(0f);
            FireProjectile(-0.35f);
            FireProjectile(0.35f);
        }
    }

    private void FireProjectile(float offset)
    {
        var position = transform.position + Vector3.up * 0.5f + Vector3.right * offset;
        if (ObjectPool.Instance != null)
        {
            var pooled = ObjectPool.Instance.Spawn(weaponData.projectilePrefab.gameObject, position, Quaternion.identity);
            if (pooled != null && pooled.TryGetComponent(out Projectile projectile))
                projectile.Configure(weaponData.projectileSpeed, weaponData.damage, true);
        }
        else
        {
            var projectile = Instantiate(weaponData.projectilePrefab, position, Quaternion.identity);
            projectile.Configure(weaponData.projectileSpeed, weaponData.damage, true);
        }
    }

    private void OnBomb(InputAction.CallbackContext context)
    {
        if (context.performed)
            UseBomb();
    }

    private void UseBomb()
    {
        if (Time.timeScale <= 0f || lastBombUseFrame == Time.frameCount ||
            GameManager.Instance == null || GameManager.Instance.Data.bombs <= 0)
            return;

        lastBombUseFrame = Time.frameCount;
        GameManager.Instance.Data.bombs--;
        StartCoroutine(ShowBombExplosion());
        foreach (var enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
        {
            if (enemy.gameObject.activeInHierarchy)
                enemy.TakeBombDamage();
        }
        foreach (var boss in FindObjectsByType<BossController>(FindObjectsSortMode.None))
        {
            if (boss.gameObject.activeInHierarchy)
                boss.TakeBombDamage();
        }
        foreach (var egg in FindObjectsByType<EnemyEgg>(FindObjectsSortMode.None))
            egg.DestroyEgg();
        GameManager.Instance.NotifyStateChanged();
        GameManager.Instance.SaveDataToDisk();
    }

    private IEnumerator ShowBombExplosion()
    {
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("Could not display the bomb explosion because the scene has no Canvas.");
            yield break;
        }

        var explosionObject = new GameObject("Bomb Explosion", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        explosionObject.transform.SetParent(canvas.transform, false);
        var rect = explosionObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(560f, 560f);

        var image = explosionObject.GetComponent<Image>();
        image.sprite = RuntimeSpriteFactory.CreateExplosion();
        image.preserveAspect = true;
        image.raycastTarget = false;

        const float duration = 0.8f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float scale = Mathf.Lerp(0.2f, 1.3f, progress);
            rect.localScale = Vector3.one * scale;
            image.color = new Color(1f, 1f, 1f, 1f - progress);
            yield return null;
        }
        Destroy(explosionObject);
    }

    public void TakeDamage()
    {
        if (invincibleTimer > 0f) return;
        GameManager.Instance.Data.lives--;
        GameManager.Instance.Data.powerLevel = Mathf.Max(1, GameManager.Instance.Data.powerLevel - 1);
        invincibleTimer = 1.5f;
        GameManager.Instance.NotifyStateChanged();
        GameManager.Instance.SaveDataToDisk();

        if (GameManager.Instance.Data.lives <= 0)
            GameManager.Instance.GameOver();
    }

    public void CollectGift()
    {
        GameManager.Instance.Data.powerLevel = Mathf.Min(3, GameManager.Instance.Data.powerLevel + 1);
        GameManager.Instance.NotifyStateChanged();
    }
}
