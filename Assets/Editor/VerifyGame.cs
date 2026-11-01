using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public static class VerifyGame
{
    [MenuItem("Game/Run Verification Tests")]
    public static void RunAllTests()
    {
        Debug.Log("======================================");
        Debug.Log("RUNNING GAME VERIFICATION TESTS...");
        Debug.Log("======================================");

        int passed = 0;
        int failed = 0;

        void Assert(bool condition, string testName)
        {
            if (condition)
            {
                Debug.Log($"[PASS] {testName}");
                passed++;
            }
            else
            {
                Debug.LogError($"[FAIL] {testName}");
                failed++;
            }
        }

        // Test 1: Tags
        Assert(UnityEditorInternal.InternalEditorUtility.tags.Contains("Coin"), "Tag 'Coin' exists in TagManager");
        Assert(UnityEditorInternal.InternalEditorUtility.tags.Contains("Bomb"), "Tag 'Bomb' exists in TagManager");

        // Test 2: Coin Prefab
        GameObject coinPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Coin.prefab");
        Assert(coinPrefab != null, "Coin prefab exists at Assets/Prefabs/Coin.prefab");
        if (coinPrefab != null)
        {
            Assert(coinPrefab.CompareTag("Coin"), "Coin prefab has tag 'Coin'");
            CircleCollider2D col = coinPrefab.GetComponent<CircleCollider2D>();
            Assert(col != null && col.isTrigger, "Coin prefab has CircleCollider2D with isTrigger = true");
            Rigidbody2D rb = coinPrefab.GetComponent<Rigidbody2D>();
            Assert(rb != null && rb.bodyType == RigidbodyType2D.Kinematic, "Coin prefab has Rigidbody2D (Kinematic)");
            Coin coinScript = coinPrefab.GetComponent<Coin>();
            Assert(coinScript != null, "Coin prefab has Coin script attached");
            SpriteRenderer sr = coinPrefab.GetComponent<SpriteRenderer>();
            Assert(sr != null && sr.sprite != null && sr.color == Color.white, "Coin prefab has SpriteRenderer (White Circle)");
        }

        // Test 3: Bomb Prefab
        GameObject bombPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Bomb.prefab");
        Assert(bombPrefab != null, "Bomb prefab exists at Assets/Prefabs/Bomb.prefab");
        if (bombPrefab != null)
        {
            Assert(bombPrefab.CompareTag("Bomb"), "Bomb prefab has tag 'Bomb'");
            CircleCollider2D col = bombPrefab.GetComponent<CircleCollider2D>();
            Assert(col != null && col.isTrigger, "Bomb prefab has CircleCollider2D with isTrigger = true");
            Rigidbody2D rb = bombPrefab.GetComponent<Rigidbody2D>();
            Assert(rb != null && rb.bodyType == RigidbodyType2D.Kinematic, "Bomb prefab has Rigidbody2D (Kinematic)");
            Bomb bombScript = bombPrefab.GetComponent<Bomb>();
            Assert(bombScript != null, "Bomb prefab has Bomb script attached");
            SpriteRenderer sr = bombPrefab.GetComponent<SpriteRenderer>();
            Assert(sr != null && sr.sprite != null && sr.color == Color.black, "Bomb prefab has SpriteRenderer (Black Circle)");
        }

        // Test 4: Scene Setup
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Single);
        Assert(scene.IsValid(), "SampleScene opens and is valid");

        // Check Player
        GameObject player = GameObject.FindWithTag("Player");
        Assert(player != null, "Player GameObject with 'Player' tag found in scene");
        if (player != null)
        {
            Assert(player.GetComponent<Collider2D>() != null, "Player has 2D Collider");
            Assert(player.GetComponent<Rigidbody2D>() != null, "Player has Rigidbody2D");
            Assert(player.GetComponent<PlayerMovement>() != null, "Player has PlayerMovement script attached");
        }

        // Check GameManager
        GameObject gmObj = GameObject.Find("GameManager");
        Assert(gmObj != null, "GameManager GameObject found in scene");
        GameManager gm = gmObj != null ? gmObj.GetComponent<GameManager>() : null;
        Assert(gm != null, "GameManager component attached");
        if (gm != null)
        {
            Assert(gm.scoreText != null, "GameManager has scoreText assigned");
            Assert(gm.bombText != null, "GameManager has bombText assigned");
            Assert(gm.gameOverText != null, "GameManager has gameOverText assigned");
        }

        // Check Spawner
        GameObject spawnerObj = GameObject.Find("CoinSpawner");
        Assert(spawnerObj != null, "CoinSpawner GameObject found in scene");
        CoinSpawner spawner = spawnerObj != null ? spawnerObj.GetComponent<CoinSpawner>() : null;
        Assert(spawner != null, "CoinSpawner component attached");
        if (spawner != null)
        {
            Assert(spawner.coinPrefab != null, "CoinSpawner has coinPrefab assigned");
            Assert(spawner.bombPrefab != null, "CoinSpawner has bombPrefab assigned");
        }

        // Check UI texts
        GameObject scoreObj = GameObject.Find("ScoreText");
        Assert(scoreObj != null && scoreObj.GetComponent<TMP_Text>() != null, "ScoreText GameObject exists with TMP_Text");
        GameObject bombObj = GameObject.Find("BombText");
        Assert(bombObj != null && bombObj.GetComponent<TMP_Text>() != null, "BombText GameObject exists with TMP_Text");
        
        GameObject goObj = gm != null && gm.gameOverText != null ? gm.gameOverText.gameObject : null;
        if (goObj == null)
        {
            var canvas = GameObject.Find("Canvas");
            if (canvas != null)
            {
                var t = canvas.transform.Find("GameOverText");
                if (t != null) goObj = t.gameObject;
            }
        }
        Assert(goObj != null && goObj.GetComponent<TMP_Text>() != null, "GameOverText GameObject exists with TMP_Text");

        // Test 5: Logic Simulation
        if (gm != null)
        {
            // Initial state
            Assert(gm.score == 0, "Initial score is 0");
            Assert(gm.bombCount == 0, "Initial bombCount is 0");
            Assert(!gm.isGameOver, "Initial isGameOver is false");

            // Catch 2 coins
            gm.AddScore(1);
            gm.AddScore(1);
            Assert(gm.score == 2, "Score increments to 2");
            Assert(gm.scoreText.text == "Score: 2", "ScoreText updates to 'Score: 2'");

            // Catch 2 bombs (no game over yet)
            gm.AddBomb(1);
            gm.AddBomb(1);
            Assert(gm.bombCount == 2, "Bomb count increments to 2");
            Assert(gm.bombText.text == "Bombs: 2", "BombText updates to 'Bombs: 2'");
            Assert(!gm.isGameOver, "Game over NOT triggered at 2 bombs");

            // Catch 3rd bomb (triggers game over)
            gm.AddBomb(1);
            Assert(gm.bombCount == 3, "Bomb count increments to 3");
            Assert(gm.isGameOver, "Game over triggers at exactly 3 bombs");
            Assert(gm.gameOverText.gameObject.activeSelf, "GameOverText becomes active on Game Over");
            Assert(gm.gameOverText.text.Contains("GAME OVER") &&
                   gm.gameOverText.text.Contains("Score: 2") &&
                   gm.gameOverText.text.Contains("Bombs: 3") &&
                   gm.gameOverText.text.Contains("Press R to Restart"),
                   "GameOverText contains GAME OVER, Score, Bombs: 3, and Press R to Restart");

            // Post-game-over safety check: should not change score or bomb count
            gm.AddScore(1);
            Assert(gm.score == 2, "Score does not increment after Game Over");
            gm.AddBomb(1);
            Assert(gm.bombCount == 3, "Bomb count does not increment after Game Over");
        }

        Debug.Log("======================================");
        Debug.Log($"VERIFICATION RESULTS: {passed} PASSED, {failed} FAILED");
        Debug.Log("======================================");

        if (failed > 0)
        {
            EditorApplication.Exit(1);
        }
    }
}
