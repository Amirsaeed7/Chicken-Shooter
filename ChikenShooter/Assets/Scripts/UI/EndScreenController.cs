using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreenController : MonoBehaviour
{
    public void Retry()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.Data.lives = 3;
            GameManager.Instance.Data.powerLevel = 1;
            GameManager.Instance.Data.bombs = 2;
            GameManager.Instance.Data.roundIndex = 1;
            GameManager.Instance.SaveDataToDisk();
            SceneManager.LoadScene($"Level{GameManager.Instance.Data.levelIndex}");
        }
        else
        {
            SceneManager.LoadScene("Level1");
        }
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void NewGame()
    {
        if (GameManager.Instance != null) GameManager.Instance.NewGame();
        else SceneManager.LoadScene("Level1");
    }
}
