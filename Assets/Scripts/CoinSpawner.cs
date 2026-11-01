using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    public GameObject coinPrefab;
    public GameObject bombPrefab;

    public float spawnInterval = 1.2f;
    public float minX = -7f;
    public float maxX = 7f;
    public float spawnY = 6f;

    [Range(0f, 1f)]
    public float bombChance = 0.25f; // 25% chance for bomb, 75% for coin

    private float timer = 0f;

    void Update()
    {
        // Stop spawning when the game is over
        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
            return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnItem();
        }
    }

    void SpawnItem()
    {
        float randomX = Random.Range(minX, maxX);
        Vector2 spawnPosition = new Vector2(randomX, spawnY);

        GameObject prefabToSpawn = coinPrefab;
        if (Random.value < bombChance && bombPrefab != null)
        {
            prefabToSpawn = bombPrefab;
        }

        if (prefabToSpawn != null)
        {
            Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        }
    }
}
