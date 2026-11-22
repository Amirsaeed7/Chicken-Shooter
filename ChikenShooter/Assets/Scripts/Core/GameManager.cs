using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public SaveData Data { get; private set; } = new SaveData();
    public event Action<SaveData> StateChanged;

    private const string SaveFileName = "savegame.json";
    private string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance == null)
        {
            var existing = FindFirstObjectByType<GameManager>();
            if (existing == null)
            {
                var go = new GameObject("GameManager");
                go.AddComponent<GameManager>();
            }
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Data = LoadData();
    }

    public bool HasSave() => File.Exists(SavePath);

    public SaveData LoadData()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                var loaded = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
                if (loaded != null)
                {
                    Data = loaded;
                    StateChanged?.Invoke(Data);
                    return Data;
                }
            }
        }
        catch (Exception exception)
        {
            Debug.LogError($"Could not load save data: {exception.Message}");
        }

        Data = new SaveData();
        StateChanged?.Invoke(Data);
        return Data;
    }

    public void SaveDataToDisk()
    {
        try
        {
            Directory.CreateDirectory(Application.persistentDataPath);
            File.WriteAllText(SavePath, JsonUtility.ToJson(Data, true));
        }
        catch (Exception exception)
        {
            Debug.LogError($"Could not save data: {exception.Message}");
        }
    }

    public void ResetSave()
    {
        try
        {
            if (File.Exists(SavePath)) File.Delete(SavePath);
        }
        catch (Exception exception)
        {
            Debug.LogError($"Could not reset save data: {exception.Message}");
        }

        Data = new SaveData();
        StateChanged?.Invoke(Data);
    }

    public void NewGame()
    {
        ResetSave();
        Data.levelIndex = 1;
        Data.roundIndex = 1;
        SceneManager.LoadScene("Level1");
    }

    public void ContinueGame()
    {
        if (!HasSave()) return;
        LoadData();
        SceneManager.LoadScene($"Level{Mathf.Clamp(Data.levelIndex, 1, 3)}");
    }

    public void CompleteRound(int nextRoundIndex)
    {
        Data.roundIndex = nextRoundIndex;
        SaveDataToDisk();
    }

    public void CompleteLevel(int completedLevelIndex)
    {
        if (completedLevelIndex >= 3)
        {
            ResetSave();
            SceneManager.LoadScene("Victory");
            return;
        }

        Data.levelIndex = completedLevelIndex + 1;
        Data.roundIndex = 1;
        SaveDataToDisk();
        SceneManager.LoadScene($"Level{Data.levelIndex}");
    }

    public void GameOver()
    {
        Data.levelIndex = Mathf.Clamp(Data.levelIndex, 1, 3);
        Data.roundIndex = 1;
        Data.lives = 3;
        Data.powerLevel = 1;
        Data.bombs = 2;
        Data.foodProgress = 0f;
        SaveDataToDisk();
        SceneManager.LoadScene("GameOver");
    }

    public void NotifyStateChanged() => StateChanged?.Invoke(Data);
}
