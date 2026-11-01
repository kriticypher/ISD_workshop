using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public static class SetupGame
{
    [MenuItem("Game/Run Full Setup")]
    public static void RunFullSetup()
    {
        Debug.Log("Starting Full Game Setup...");

        // 1. Ensure Tags exist
        EnsureTag("Coin");
        EnsureTag("Bomb");

        // 2. Import TMP Essential Resources if not present
        EnsureTMPEssentials();

        // 3. Ensure Prefabs directory exists
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        // 4. Create Coin Prefab
        GameObject coinPrefab = CreateCoinPrefab();

        // 5. Create Bomb Prefab
        GameObject bombPrefab = CreateBombPrefab();

        // 6. Setup SampleScene
        SetupScene(coinPrefab, bombPrefab);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Full Game Setup Completed Successfully!");
    }

    public static void EnsureTMPEssentials()
    {
        string tmpFontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
        if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(tmpFontPath) == null)
        {
            Debug.Log("Importing TMP Essential Resources...");
            string[] packages = Directory.GetFiles("Library/PackageCache", "TMP Essential Resources.unitypackage", SearchOption.AllDirectories);
            if (packages.Length > 0)
            {
                AssetDatabase.ImportPackage(packages[0], false);
                AssetDatabase.Refresh();
            }
        }
    }

    private static void EnsureTag(string tag)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        bool found = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue.Equals(tag))
            {
                found = true;
                break;
            }
        }

        if (!found)
        {
            tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
            tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
            tagManager.ApplyModifiedProperties();
            Debug.Log($"Added tag: {tag}");
        }
    }

    private static Sprite GetCircleSprite()
    {
        string[] paths = new string[]
        {
            "Packages/com.unity.2d.sprite/Editor/ObjectMenuCreation/DefaultAssets/Textures/v2/Circle.png",
            "Packages/com.unity.2d.sprite/Editor/ObjectMenuCreation/DefaultAssets/Textures/2D/Circle.png"
        };

        foreach (string path in paths)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var a in assets)
            {
                if (a is Sprite s) return s;
            }
        }

        // Fallback to built-in knob/circle if package path fails
        Sprite knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        if (knob != null) return knob;

        // If all else fails, create a clean procedural circle sprite
        string localCirclePath = "Assets/Sprites/Circle.png";
        if (!AssetDatabase.IsValidFolder("Assets/Sprites"))
        {
            AssetDatabase.CreateFolder("Assets", "Sprites");
        }

        if (File.Exists(localCirclePath))
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(localCirclePath);
        }

        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = size / 2f;
        float radius = (size / 2f) - 2f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (dist <= radius)
                {
                    tex.SetPixel(x, y, Color.white);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        tex.Apply();
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(localCirclePath, bytes);
        AssetDatabase.ImportAsset(localCirclePath, ImportAssetOptions.ForceSynchronousImport);

        TextureImporter importer = AssetImporter.GetAtPath(localCirclePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 100;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(localCirclePath);
    }

    private static GameObject CreateCoinPrefab()
    {
        string prefabPath = "Assets/Prefabs/Coin.prefab";

        GameObject go = new GameObject("Coin");
        go.tag = "Coin";

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GetCircleSprite();
        sr.color = Color.white;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        Coin coinScript = go.AddComponent<Coin>();
        coinScript.fallSpeed = 5f;
        coinScript.bottomBound = -6f;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        Object.DestroyImmediate(go);
        Debug.Log("Created Coin Prefab at " + prefabPath);
        return prefab;
    }

    private static GameObject CreateBombPrefab()
    {
        string prefabPath = "Assets/Prefabs/Bomb.prefab";

        GameObject go = new GameObject("Bomb");
        go.tag = "Bomb";

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GetCircleSprite();
        sr.color = Color.black; // Bomb visually distinguishable by black color

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        Bomb bombScript = go.AddComponent<Bomb>();
        bombScript.fallSpeed = 5f;
        bombScript.bottomBound = -6f;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        Object.DestroyImmediate(go);
        Debug.Log("Created Bomb Prefab at " + prefabPath);
        return prefab;
    }

    private static void SetupScene(GameObject coinPrefab, GameObject bombPrefab)
    {
        string scenePath = "Assets/Scenes/SampleScene.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // 1. Setup Player (existing catcher object)
        GameObject playerObj = GameObject.Find("catcher");
        if (playerObj == null)
        {
            playerObj = GameObject.Find("Player");
        }

        if (playerObj == null)
        {
            playerObj = new GameObject("Player");
            playerObj.transform.position = new Vector3(0, -4, 0);
            playerObj.transform.localScale = new Vector3(2, 0.5f, 1);
            SpriteRenderer sr = playerObj.AddComponent<SpriteRenderer>();
            sr.color = Color.white;
            BoxCollider2D col = playerObj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1, 1);
            Rigidbody2D rb = playerObj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        playerObj.name = "Player";
        playerObj.tag = "Player";

        // Remove old CatcherMovement if present
        CatcherMovement oldCatcher = playerObj.GetComponent<CatcherMovement>();
        if (oldCatcher != null)
        {
            Object.DestroyImmediate(oldCatcher);
        }

        PlayerMovement pm = playerObj.GetComponent<PlayerMovement>();
        if (pm == null)
        {
            pm = playerObj.AddComponent<PlayerMovement>();
        }
        pm.speed = 8f;
        pm.xBound = 7.5f;

        // 2. Setup GameManager
        GameObject gmObj = GameObject.Find("GameManager");
        if (gmObj == null)
        {
            gmObj = new GameObject("GameManager");
        }
        GameManager gm = gmObj.GetComponent<GameManager>();
        if (gm == null)
        {
            gm = gmObj.AddComponent<GameManager>();
        }

        // 3. Setup Spawner
        GameObject spawnerObj = GameObject.Find("CoinSpawner");
        if (spawnerObj == null)
        {
            spawnerObj = new GameObject("CoinSpawner");
        }
        spawnerObj.transform.position = new Vector3(0, 6, 0);
        CoinSpawner spawner = spawnerObj.GetComponent<CoinSpawner>();
        if (spawner == null)
        {
            spawner = spawnerObj.AddComponent<CoinSpawner>();
        }
        spawner.coinPrefab = coinPrefab;
        spawner.bombPrefab = bombPrefab;
        spawner.spawnInterval = 1.2f;
        spawner.minX = -7f;
        spawner.maxX = 7f;
        spawner.spawnY = 6f;
        spawner.bombChance = 0.25f;

        // 4. Setup Canvas and UI
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj == null)
        {
            canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Ensure EventSystem exists
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");

        // ScoreText (Top-Left)
        GameObject scoreObj = GameObject.Find("ScoreText");
        if (scoreObj == null)
        {
            scoreObj = new GameObject("ScoreText");
            scoreObj.transform.SetParent(canvasObj.transform, false);
        }
        RectTransform scoreRect = scoreObj.GetComponent<RectTransform>();
        if (scoreRect == null) scoreRect = scoreObj.AddComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0, 1);
        scoreRect.anchorMax = new Vector2(0, 1);
        scoreRect.pivot = new Vector2(0, 1);
        scoreRect.anchoredPosition = new Vector2(30, -30);
        scoreRect.sizeDelta = new Vector2(400, 50);

        TextMeshProUGUI scoreText = scoreObj.GetComponent<TextMeshProUGUI>();
        if (scoreText == null) scoreText = scoreObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) scoreText.font = fontAsset;
        scoreText.text = "Score: 0";
        scoreText.fontSize = 32;
        scoreText.alignment = TextAlignmentOptions.TopLeft;
        scoreText.color = Color.white;

        // BombText (Top-Left, below ScoreText)
        GameObject bombObj = GameObject.Find("BombText");
        if (bombObj == null)
        {
            bombObj = new GameObject("BombText");
            bombObj.transform.SetParent(canvasObj.transform, false);
        }
        RectTransform bombRect = bombObj.GetComponent<RectTransform>();
        if (bombRect == null) bombRect = bombObj.AddComponent<RectTransform>();
        bombRect.anchorMin = new Vector2(0, 1);
        bombRect.anchorMax = new Vector2(0, 1);
        bombRect.pivot = new Vector2(0, 1);
        bombRect.anchoredPosition = new Vector2(30, -80);
        bombRect.sizeDelta = new Vector2(400, 50);

        TextMeshProUGUI bombText = bombObj.GetComponent<TextMeshProUGUI>();
        if (bombText == null) bombText = bombObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) bombText.font = fontAsset;
        bombText.text = "Bombs: 0";
        bombText.fontSize = 32;
        bombText.alignment = TextAlignmentOptions.TopLeft;
        bombText.color = Color.white;

        // GameOverText (Middle-Center)
        GameObject goObj = GameObject.Find("GameOverText");
        if (goObj == null)
        {
            goObj = new GameObject("GameOverText");
            goObj.transform.SetParent(canvasObj.transform, false);
        }
        RectTransform goRect = goObj.GetComponent<RectTransform>();
        if (goRect == null) goRect = goObj.AddComponent<RectTransform>();
        goRect.anchorMin = new Vector2(0.5f, 0.5f);
        goRect.anchorMax = new Vector2(0.5f, 0.5f);
        goRect.pivot = new Vector2(0.5f, 0.5f);
        goRect.anchoredPosition = Vector2.zero;
        goRect.sizeDelta = new Vector2(800, 300);

        TextMeshProUGUI goText = goObj.GetComponent<TextMeshProUGUI>();
        if (goText == null) goText = goObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) goText.font = fontAsset;
        goText.text = "GAME OVER\nScore: 0\nBombs: 3\nPress R to Restart";
        goText.fontSize = 44;
        goText.alignment = TextAlignmentOptions.Center;
        goText.color = Color.white;
        goObj.SetActive(false); // Hidden during gameplay

        // Assign UI references to GameManager
        gm.scoreText = scoreText;
        gm.bombText = bombText;
        gm.gameOverText = goText;

        EditorUtility.SetDirty(gm);
        EditorUtility.SetDirty(spawner);
        EditorUtility.SetDirty(playerObj);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Scene configured and saved successfully: " + scenePath);
    }
}
