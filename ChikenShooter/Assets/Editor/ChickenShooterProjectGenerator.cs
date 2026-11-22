using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor.Events;

public static class ChickenShooterProjectGenerator
{
    [MenuItem("Chicken Shooter/Build Windows x86_64")]
    public static void BuildWindows()
    {
        var buildPath = "Build/ChickenShooter.exe";
        var report = UnityEditor.BuildPipeline.BuildPlayer(
            EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray(),
            buildPath,
            BuildTarget.StandaloneWindows64,
            BuildOptions.None);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            Debug.Log($"Build succeeded: {buildPath}");
        else
            throw new System.Exception($"Build failed: {report.summary.result}");
    }

    [MenuItem("Chicken Shooter/Generate Complete Project")]
    public static void Generate()
    {
        EnsureFolders();
        ConfigureInputActions();
        CreateDataAssets();
        CreatePrefabs();
        CreateScenes();
        ConfigureBuildSettings();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Chicken Shooter", "The project scenes, prefabs, data assets, UI, and build settings were generated.", "OK");
    }

    private static void EnsureFolders()
    {
        string[] folders = {
            "Assets/Data", "Assets/Data/Levels", "Assets/Data/Waves",
            "Assets/Prefabs", "Assets/Scenes", "Assets/Sprites",
            "Assets/Scripts/Core", "Assets/Scripts/Gameplay",
            "Assets/Scripts/UI", "Assets/Editor", "Documentation"
        };
        string project = Application.dataPath.Replace("/Assets", "");
        foreach (var folder in folders)
        {
            var full = Path.Combine(project, folder.Replace('/', '\\'));
            if (!Directory.Exists(full)) Directory.CreateDirectory(full);
        }
    }

    private static Sprite CreateSprite(string name, Color color)
    {
        var path = $"Assets/Sprites/{name}.png";
        var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (existing != null) return existing;

        const int size = 32;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color[size * size];
        var center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float d = Vector2.Distance(new Vector2(x, y), center);
            pixels[y * size + x] = d <= size * 0.45f ? color : new Color(0f, 0f, 0f, 0f);
        }
        texture.SetPixels(pixels);
        texture.Apply();
        File.WriteAllBytes(Application.dataPath + $"/Sprites/{name}.png", texture.EncodeToPNG());
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = size;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static void ConfigureInputActions()
    {
        var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
        if (asset == null) return;

        asset.Disable();
        while (asset.actionMaps.Count > 0)
            asset.RemoveActionMap(asset.actionMaps[0]);

        var map = asset.AddActionMap("Player");
        var move = map.AddAction("Move", InputActionType.Value, null);
        move.expectedControlType = "Vector2";
        move.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        move.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");

        var attack = map.AddAction("Attack", InputActionType.Button);
        attack.AddBinding("<Keyboard>/space");
        attack.AddBinding("<Mouse>/leftButton");

        var bomb = map.AddAction("Bomb", InputActionType.Button);
        bomb.AddBinding("<Keyboard>/x");

        var pause = map.AddAction("Pause", InputActionType.Button);
        pause.AddBinding("<Keyboard>/escape");
        pause.AddBinding("<Keyboard>/p");

        EditorUtility.SetDirty(asset);
    }

    private static void CreateDataAssets()
    {
        var weapon = ScriptableObject.CreateInstance<WeaponDataSO>();
        weapon.weaponName = "Laser";
        weapon.damage = 1f;
        weapon.projectileSpeed = 12f;
        weapon.fireRate = 0.18f;
        EnsureAsset(weapon, "Assets/Data/WeaponData.asset");

        var levelNames = new[] { "Planetary Orbit", "Asteroid Belt", "Cosmic Abyss" };
        var roundCounts = new[] { 5, 7, 10 };
        var sceneNames = new[] { "Level1", "Level2", "Level3" };

        for (int level = 0; level < 3; level++)
        {
            var waves = new WaveDataSO[roundCounts[level]];
            for (int round = 0; round < roundCounts[level]; round++)
            {
                var wave = ScriptableObject.CreateInstance<WaveDataSO>();
                wave.enemyType = level == 0 ? EnemyArchetype.Standard :
                                 level == 1 ? EnemyArchetype.Armored : EnemyArchetype.Cyber;
                wave.enemyCount = Mathf.Min(18, 8 + round * 2);
                wave.movementSpeed = 1.5f + level * 0.6f + round * 0.12f;
                wave.eggDropRate = Mathf.Max(0.45f, 2f - level * 0.35f - round * 0.08f);
                wave.eggDropRate = Mathf.Max(0.45f, wave.eggDropRate);
                wave.enemyHealth = level == 0 ? 1 : level == 1 ? 2 : 3;
                wave.formation = (FormationPattern)((round + level) % 4);
                wave.isAsteroidRound = (level == 0 && round == 2) ||
                                       (level == 1 && (round == 2 || round == 5)) ||
                                       (level == 2 && (round == 3 || round == 6));
                wave.isBossRound = round == roundCounts[level] - 1;
                wave.bossHealth = 300f + level * 180f + round * 10f;
                EnsureAsset(wave, $"Assets/Data/Waves/Level{level + 1}_Round{round + 1}.asset");
                waves[round] = wave;
            }

            var levelAsset = ScriptableObject.CreateInstance<LevelDataSO>();
            levelAsset.levelIndex = level + 1;
            levelAsset.sceneName = sceneNames[level];
            levelAsset.stageTheme = levelNames[level];
            levelAsset.backgroundColor = level == 0 ? new Color(0.02f, 0.05f, 0.18f) :
                                         level == 1 ? new Color(0.12f, 0.07f, 0.18f) :
                                         new Color(0.03f, 0.03f, 0.12f);
            levelAsset.waves = waves;
            EnsureAsset(levelAsset, $"Assets/Data/Levels/Level{level + 1}Data.asset");
        }
    }

    private static void EnsureAsset(ScriptableObject asset, string path)
    {
        var existing = AssetDatabase.LoadAssetAtPath<Object>(path);
        if (existing != null)
        {
            if (existing is ScriptableObject existingData)
            {
                EditorUtility.CopySerialized(asset, existingData);
                EditorUtility.SetDirty(existingData);
            }
            return;
        }
        AssetDatabase.CreateAsset(asset, path);
    }

    private static void CreatePrefabs()
    {
        CreateProjectilePrefab();
        CreateEggPrefab();
        CreateFoodPrefab();
        CreateGiftPrefab();
        CreateEnemyPrefabs();
        CreateBossPrefab();
        CreatePlayerPrefab();
    }

    private static GameObject CreateCirclePrefab(string name, string path, Color color, float scale)
    {
        var go = new GameObject(name);
        go.transform.localScale = Vector3.one * scale;
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateSprite(name.Replace(" ", ""), color);
        renderer.sortingOrder = 5;
        return SavePrefab(go, path);
    }

    private static GameObject SavePrefab(GameObject go, string path)
    {
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null)
        {
            Object.DestroyImmediate(go);
            return existing;
        }
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static void CreateProjectilePrefab()
    {
        var go = CreateCirclePrefab("Laser Projectile", "Assets/Prefabs/LaserProjectile.prefab", Color.cyan, 0.18f);
        var collider = go.GetComponent<Collider2D>();
        if (collider == null) go.AddComponent<CircleCollider2D>().isTrigger = true;
        if (go.GetComponent<Projectile>() == null) go.AddComponent<Projectile>();
        var weapon = AssetDatabase.LoadAssetAtPath<WeaponDataSO>("Assets/Data/WeaponData.asset");
        if (weapon != null) weapon.projectilePrefab = go.GetComponent<Projectile>();
    }

    private static void CreateEggPrefab()
    {
        var go = CreateCirclePrefab("Enemy Egg", "Assets/Prefabs/EnemyEgg.prefab", new Color(0.95f, 0.95f, 0.75f), 0.22f);
        go.AddComponent<CircleCollider2D>().isTrigger = true;
        go.AddComponent<EnemyEgg>();
    }

    private static void CreateFoodPrefab()
    {
        var go = CreateCirclePrefab("Food Drop", "Assets/Prefabs/FoodDrop.prefab", new Color(1f, 0.35f, 0.25f), 0.28f);
        go.AddComponent<CircleCollider2D>().isTrigger = true;
        var drop = go.AddComponent<Drop>();
    }

    private static void CreateGiftPrefab()
    {
        var go = CreateCirclePrefab("Weapon Gift", "Assets/Prefabs/WeaponGift.prefab", Color.yellow, 0.32f);
        go.AddComponent<CircleCollider2D>().isTrigger = true;
        go.AddComponent<Drop>();
        var so = new SerializedObject(go.GetComponent<Drop>());
        so.FindProperty("weaponGift").boolValue = true;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateEnemyPrefabs()
    {
        CreateEnemyPrefab("StandardChicken", new Color(1f, 0.82f, 0.35f), 0.75f);
        CreateEnemyPrefab("ArmoredChicken", new Color(0.55f, 0.75f, 1f), 0.85f);
        CreateEnemyPrefab("CyberChicken", new Color(0.75f, 0.25f, 1f), 0.9f);
        CreateEnemyPrefab("Asteroid", Color.gray, 0.65f);
    }

    private static void CreateEnemyPrefab(string name, Color color, float scale)
    {
        var go = CreateCirclePrefab(name, $"Assets/Prefabs/{name}.prefab", color, scale);
        go.AddComponent<CircleCollider2D>().isTrigger = true;
        go.AddComponent<Enemy>();
    }

    private static void CreateBossPrefab()
    {
        var go = CreateCirclePrefab("Stage Boss", "Assets/Prefabs/StageBoss.prefab", new Color(0.95f, 0.25f, 0.2f), 2.1f);
        go.AddComponent<CircleCollider2D>().isTrigger = true;
        go.AddComponent<BossController>();
    }

    private static void CreatePlayerPrefab()
    {
        var go = CreateCirclePrefab("Player Ship", "Assets/Prefabs/PlayerShip.prefab", Color.cyan, 0.65f);
        go.tag = "Player";
        go.AddComponent<CircleCollider2D>().isTrigger = true;
        go.AddComponent<PlayerInput>();
        go.AddComponent<PlayerController>();
    }

    private static void CreateScenes()
    {
        CreateMenuScene();
        for (int i = 1; i <= 3; i++) CreateLevelScene(i);
        CreateGameOverScene("GameOver", false);
        CreateGameOverScene("Victory", true);
    }

    private static void CreateMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        AddCameraAndCanvas();
        CreateStarfield(0);
        var menu = new GameObject("Main Menu");
        var controller = menu.AddComponent<MainMenuController>();
        var canvas = Object.FindFirstObjectByType<Canvas>();

        var title = CreateText(canvas.transform, "CHICKEN SHOOTER", 36, new Vector2(0, 180));
        var newButton = CreateButton(canvas.transform, "New Game", new Vector2(0, 20), controller, "NewGame");
        var continueButton = CreateButton(canvas.transform, "Continue", new Vector2(0, -35), controller, "ContinueGame");
        CreateButton(canvas.transform, "Quit", new Vector2(0, -90), controller, "QuitGame");

        var so = new SerializedObject(controller);
        so.FindProperty("continueButton").objectReferenceValue = continueButton.GetComponent<Button>();
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
    }

    private static void CreateLevelScene(int levelIndex)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        AddCameraAndCanvas();
        var camera = Camera.main;
        camera.backgroundColor = levelIndex == 1 ? new Color(0.02f, 0.05f, 0.18f) :
                                 levelIndex == 2 ? new Color(0.12f, 0.07f, 0.18f) :
                                                   new Color(0.03f, 0.03f, 0.12f);

        CreateStarfield(levelIndex);
        CreatePlayer(levelIndex);

        var roundManagerObject = new GameObject("RoundManager");
        var roundManager = roundManagerObject.AddComponent<RoundManager>();
        var egg = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/EnemyEgg.prefab");
        var food = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/FoodDrop.prefab");
        var gift = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/WeaponGift.prefab");
        var boss = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/StageBoss.prefab");

        var levelData = AssetDatabase.LoadAssetAtPath<LevelDataSO>($"Assets/Data/Levels/Level{levelIndex}Data.asset");
        var roundSo = new SerializedObject(roundManager);
        roundSo.FindProperty("levelData").objectReferenceValue = levelData;
        roundSo.FindProperty("eggPrefab").objectReferenceValue = egg != null ? egg.GetComponent<EnemyEgg>() : null;
        roundSo.FindProperty("foodPrefab").objectReferenceValue = food != null ? food.GetComponent<Drop>() : null;
        roundSo.FindProperty("giftPrefab").objectReferenceValue = gift != null ? gift.GetComponent<Drop>() : null;
        roundSo.FindProperty("bossPrefab").objectReferenceValue = boss != null ? boss.GetComponent<BossController>() : null;
        roundSo.ApplyModifiedPropertiesWithoutUndo();

        CreateHud();
        CreatePauseMenu();
        new GameObject("ObjectPool").AddComponent<ObjectPool>();

        EditorSceneManager.SaveScene(scene, $"Assets/Scenes/Level{levelIndex}.unity");
    }

    private static void AddCameraAndCanvas()
    {
        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        var camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5.5f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.015f, 0.025f, 0.09f);
        cameraObject.AddComponent<AudioListener>();
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);

        var canvasObject = new GameObject("Canvas");
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();
        var eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
    }

    private static void CreateStarfield(int levelIndex)
    {
        var stars = new GameObject("Starfield");
        for (int i = 0; i < 70; i++)
        {
            var star = new GameObject($"Star {i}");
            star.transform.SetParent(stars.transform);
            star.transform.position = new Vector3(
                Random.Range(-8f, 8f),
                Random.Range(-5f, 6f),
                2f);
            var renderer = star.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateSprite($"Star{levelIndex}_{i}", Color.Lerp(Color.white, new Color(0.5f, 0.7f, 1f), Random.value));
            renderer.sortingOrder = -10;
            star.transform.localScale = Vector3.one * Random.Range(0.05f, 0.12f);
        }
    }

    private static void CreatePlayer(int levelIndex)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PlayerShip.prefab");
        var player = prefab != null ? PrefabUtility.InstantiatePrefab(prefab) as GameObject : new GameObject("Player");
        if (player == null) return;
        player.name = "Player";
        player.transform.position = new Vector3(0f, -3.7f, 0f);
        if (player.GetComponent<Collider2D>() == null)
            player.AddComponent<CircleCollider2D>().isTrigger = true;
        if (player.GetComponent<PlayerInput>() == null)
            player.AddComponent<PlayerInput>();
        if (player.GetComponent<PlayerController>() == null)
            player.AddComponent<PlayerController>();
        var weapon = AssetDatabase.LoadAssetAtPath<WeaponDataSO>("Assets/Data/WeaponData.asset");
        var playerSo = new SerializedObject(player.GetComponent<PlayerController>());
        if (playerSo == null) return;
        playerSo.FindProperty("weaponData").objectReferenceValue = weapon;
        var input = player.GetComponent<PlayerInput>();
        input.actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
        input.defaultActionMap = "Player";
        input.defaultControlScheme = null;
        playerSo.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateHud()
    {
        var canvas = Object.FindFirstObjectByType<Canvas>();
        var hudObject = new GameObject("HUD");
        hudObject.transform.SetParent(canvas.transform, false);
        var hud = hudObject.AddComponent<HudController>();

        var levelRound = CreateText(canvas.transform, "Level 1 - Round 1", 20, new Vector2(-430, 260));
        var lives = CreateText(canvas.transform, "Lives: 3", 20, new Vector2(-430, 220));
        var score = CreateText(canvas.transform, "Score: 0", 20, new Vector2(-430, 180));
        var bombs = CreateText(canvas.transform, "Bombs: 2", 20, new Vector2(430, 260));
        var food = CreateSlider(canvas.transform, "Food Progress", new Vector2(-430, -260), new Vector2(200, 20));
        var boss = CreateSlider(canvas.transform, "Boss Health", new Vector2(0, 300), new Vector2(500, 22));

        var so = new SerializedObject(hud);
        so.FindProperty("levelRoundText").objectReferenceValue = levelRound;
        so.FindProperty("livesText").objectReferenceValue = lives;
        so.FindProperty("scoreText").objectReferenceValue = score;
        so.FindProperty("bombsText").objectReferenceValue = bombs;
        so.FindProperty("foodProgress").objectReferenceValue = food;
        so.FindProperty("bossHealth").objectReferenceValue = boss;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreatePauseMenu()
    {
        var canvas = Object.FindFirstObjectByType<Canvas>();
        var pauseController = new GameObject("Pause Menu");
        pauseController.transform.SetParent(canvas.transform, false);
        var pause = pauseController.AddComponent<PauseMenu>();

        var panel = new GameObject("Pause Panel");
        panel.transform.SetParent(pauseController.transform, false);
        var rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        var image = panel.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.82f);
        panel.SetActive(false);

        CreateButton(panel.transform, "Resume", new Vector2(0, 20), pause, "Resume");
        CreateButton(panel.transform, "Exit to Main Menu", new Vector2(0, -35), pause, "ExitToMainMenu");
        var so = new SerializedObject(pause);
        so.FindProperty("panel").objectReferenceValue = panel;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateGameOverScene(string sceneName, bool victory)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        AddCameraAndCanvas();
        var canvas = Object.FindFirstObjectByType<Canvas>();
        var controllerObject = new GameObject(victory ? "Victory Screen" : "Game Over");
        var controller = controllerObject.AddComponent<EndScreenController>();
        CreateText(canvas.transform, victory ? "VICTORY" : "GAME OVER", 42, new Vector2(0, 120));

        if (victory)
            CreateButton(canvas.transform, "New Game", new Vector2(0, 0), controller, "NewGame");
        else
            CreateButton(canvas.transform, "Retry", new Vector2(0, 20), controller, "Retry");

        CreateButton(canvas.transform, "Main Menu", new Vector2(0, -40), controller, "MainMenu");
        if (victory && GameManager.Instance != null)
            GameManager.Instance.ResetSave();
        EditorSceneManager.SaveScene(scene, $"Assets/Scenes/{sceneName}.unity");
    }

    private static Text CreateText(Transform parent, string text, int size, Vector2 position)
    {
        var go = new GameObject(text);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(500, 70);
        var label = go.AddComponent<Text>();
        label.font = LoadBuiltinFont();
        label.text = text;
        label.fontSize = size;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        return label;
    }

    private static Button CreateButton(Transform parent, string text, Vector2 position, Object target, string method)
    {
        var go = new GameObject(text);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(220, 42);
        var image = go.AddComponent<Image>();
        image.color = new Color(0.15f, 0.28f, 0.55f, 0.95f);
        var button = go.AddComponent<Button>();

        var label = new GameObject("Label");
        label.transform.SetParent(go.transform, false);
        var labelRect = label.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        var labelText = label.AddComponent<Text>();
        labelText.font = LoadBuiltinFont();
        labelText.text = text;
        labelText.fontSize = 22;
        labelText.fontStyle = FontStyle.Bold;
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.color = Color.white;

        if (target != null)
        {
            if (target is MainMenuController mainMenu)
            {
                if (method == nameof(MainMenuController.NewGame))
                    UnityEventTools.AddPersistentListener(button.onClick, mainMenu.NewGame);
                else if (method == nameof(MainMenuController.ContinueGame))
                    UnityEventTools.AddPersistentListener(button.onClick, mainMenu.ContinueGame);
                else if (method == nameof(MainMenuController.QuitGame))
                    UnityEventTools.AddPersistentListener(button.onClick, mainMenu.QuitGame);
            }
            else if (target is PauseMenu pauseMenu)
            {
                if (method == nameof(PauseMenu.Resume))
                    UnityEventTools.AddPersistentListener(button.onClick, pauseMenu.Resume);
                else if (method == nameof(PauseMenu.ExitToMainMenu))
                    UnityEventTools.AddPersistentListener(button.onClick, pauseMenu.ExitToMainMenu);
            }
            else if (target is EndScreenController endScreen)
            {
                if (method == nameof(EndScreenController.Retry))
                    UnityEventTools.AddPersistentListener(button.onClick, endScreen.Retry);
                else if (method == nameof(EndScreenController.MainMenu))
                    UnityEventTools.AddPersistentListener(button.onClick, endScreen.MainMenu);
                else if (method == nameof(EndScreenController.NewGame))
                    UnityEventTools.AddPersistentListener(button.onClick, endScreen.NewGame);
            }
        }
        return button;
    }

    private static Slider CreateSlider(Transform parent, string name, Vector2 position, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        var slider = go.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;

        var background = new GameObject("Background");
        background.transform.SetParent(go.transform, false);
        var bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        var bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.12f, 0.12f, 0.12f, 0.9f);

        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(go.transform, false);
        var fillRect = fillArea.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(5f, 5f);
        fillRect.offsetMax = new Vector2(-5f, -5f);
        var fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        var fillImage = fill.AddComponent<Image>();
        fillImage.color = name.Contains("Boss") ? Color.red : Color.yellow;
        var fillImageRect = fill.GetComponent<RectTransform>();
        fillImageRect.anchorMin = Vector2.zero;
        fillImageRect.anchorMax = Vector2.one;
        fillImageRect.offsetMin = Vector2.zero;
        fillImageRect.offsetMax = Vector2.zero;
        slider.fillRect = fillImageRect;
        slider.targetGraphic = fillImage;
        return slider;
    }

    private static Font LoadBuiltinFont()
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    private static void ConfigureBuildSettings()
    {
        var scenes = new[]
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/Level1.unity",
            "Assets/Scenes/Level2.unity",
            "Assets/Scenes/Level3.unity",
            "Assets/Scenes/GameOver.unity",
            "Assets/Scenes/Victory.unity"
        };
        EditorBuildSettings.scenes = scenes
            .Select(path => new EditorBuildSettingsScene(path, AssetDatabase.LoadAssetAtPath<Object>(path) != null))
            .ToArray();
    }
}
