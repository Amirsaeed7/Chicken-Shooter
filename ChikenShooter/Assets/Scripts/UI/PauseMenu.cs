using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject panel;

    private bool paused;

    public void Toggle()
    {
        paused = !paused;
        if (panel != null) panel.SetActive(paused);
        Time.timeScale = paused ? 0f : 1f;
    }

    public void Resume()
    {
        paused = false;
        if (panel != null) panel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;
        GameManager.Instance?.SaveDataToDisk();
        SceneManager.LoadScene("MainMenu");
    }
}
