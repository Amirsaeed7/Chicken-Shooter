using UnityEngine;
using UnityEngine.UI;

public class HudController : MonoBehaviour
{
    [SerializeField] private Text levelRoundText;
    [SerializeField] private Text livesText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text bombsText;
    [SerializeField] private Slider foodProgress;
    [SerializeField] private Slider bossHealth;

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
        if (livesText != null) livesText.text = $"Lives: {data.lives}";
        if (scoreText != null) scoreText.text = $"Score: {data.score}";
        if (bombsText != null) bombsText.text = $"Bombs: {data.bombs}";
        if (foodProgress != null) foodProgress.value = data.foodProgress / 100f;
    }
}
