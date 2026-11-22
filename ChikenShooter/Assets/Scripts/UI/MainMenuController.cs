using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button continueButton;

    private void Start()
    {
        if (continueButton != null)
            continueButton.interactable = GameManager.Instance != null && GameManager.Instance.HasSave();
    }

    public void NewGame()
    {
        GameManager.Instance?.NewGame();
        if (GameManager.Instance == null) SceneManager.LoadScene("Level1");
    }

    public void ContinueGame()
    {
        if (GameManager.Instance != null) GameManager.Instance.ContinueGame();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
