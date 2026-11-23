using UnityEngine;
using UnityEngine.UI;

public class HudController : MonoBehaviour
{
    [SerializeField] private Text levelRoundText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text powerText;
    [SerializeField] private Text foodText;
    [SerializeField] private Image[] lifeIcons;
    [SerializeField] private Image[] bombIcons;
    [SerializeField] private Slider foodProgress;
    [SerializeField] private Slider bossHealth;

    private void Awake()
    {
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("HUD could not find a Canvas to create missing resource and status elements.");
            return;
        }

        HideLegacyResourceLabels(canvas.transform);
        if (!HasAssignedIcons(lifeIcons))
            lifeIcons = CreateIconRow(canvas.transform, "Life", RuntimeSpriteFactory.CreateHeart(), -520f, 220f);
        if (!HasAssignedIcons(bombIcons))
            bombIcons = CreateIconRow(canvas.transform, "Bomb", RuntimeSpriteFactory.CreateBomb(), 390f, 260f);
        if (powerText == null)
            powerText = CreateStatusText(canvas.transform, "Power: 1/3", new Vector2(-430f, 120f));
        if (foodText == null)
            foodText = CreateStatusText(canvas.transform, "Food: 0%", new Vector2(-430f, -222f));
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StateChanged += Refresh;
        Refresh(GameManager.Instance != null ? GameManager.Instance.Data : new SaveData());
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StateChanged -= Refresh;
    }

    private void Update()
    {
        var boss = FindFirstObjectByType<BossController>();
        if (bossHealth != null)
        {
            bossHealth.gameObject.SetActive(boss != null);
            if (boss != null) bossHealth.value = boss.HealthNormalized;
        }
    }

    public void Refresh(SaveData data)
    {
        if (levelRoundText != null)
            levelRoundText.text = $"Level {data.levelIndex} - Round {data.roundIndex}";
        if (scoreText != null) scoreText.text = $"Score: {data.score}";
        if (powerText != null) powerText.text = $"Power: {Mathf.Clamp(data.powerLevel, 1, 3)}/3";
        if (foodText != null) foodText.text = $"Food: {Mathf.RoundToInt(data.foodProgress)}%";
        SetVisibleIcons(lifeIcons, data.lives);
        SetVisibleIcons(bombIcons, data.bombs);
        if (foodProgress != null) foodProgress.value = data.foodProgress / 100f;
    }

    private static void SetVisibleIcons(Image[] icons, int count)
    {
        if (icons == null) return;
        for (int i = 0; i < icons.Length; i++)
        {
            if (icons[i] != null)
                icons[i].enabled = i < count;
        }
    }

    private static bool HasAssignedIcons(Image[] icons)
    {
        if (icons == null) return false;
        for (int i = 0; i < icons.Length; i++)
        {
            if (icons[i] != null) return true;
        }
        return false;
    }

    private static Image[] CreateIconRow(Transform parent, string name, Sprite sprite, float startX, float y)
    {
        var icons = new Image[3];
        for (int i = 0; i < icons.Length; i++)
        {
            var iconObject = new GameObject($"{name} Icon {i + 1}", typeof(RectTransform), typeof(Image));
            iconObject.transform.SetParent(parent, false);
            var rect = iconObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(startX + i * 48f, y);
            rect.sizeDelta = new Vector2(42f, 42f);
            var image = iconObject.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            icons[i] = image;
        }
        return icons;
    }

    private static Text CreateStatusText(Transform parent, string text, Vector2 position)
    {
        var textObject = new GameObject(text, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);
        var rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(280f, 40f);
        var label = textObject.GetComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 20;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        return label;
    }

    private static void HideLegacyResourceLabels(Transform canvas)
    {
        foreach (var label in canvas.GetComponentsInChildren<Text>(true))
        {
            if (label.gameObject.name.StartsWith("Lives:") || label.gameObject.name.StartsWith("Bombs:"))
                label.gameObject.SetActive(false);
        }
    }
}
