using UnityEngine;
using TMPro;

public class Coin : MonoBehaviour
{
    public float fallSpeed = 5f;
    public float bottomBound = -6f;

    void Update()
    {
        // Stop falling when the game is over
        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
            return;

        transform.Translate(Vector2.down * fallSpeed * Time.deltaTime);

        // Auto cleanup if fallen below screen
        if (transform.position.y < bottomBound)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
            return;

        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(1);
            }
            else
            {
                // Fallback direct UI update if GameManager is not present
                GameObject scoreObject = GameObject.Find("ScoreText");
                if (scoreObject != null)
                {
                    TMP_Text scoreText = scoreObject.GetComponent<TMP_Text>();
                    if (scoreText != null)
                    {
                        scoreText.text = "Score: " + (int.Parse(scoreText.text.Replace("Score: ", "")) + 1);
                    }
                }
            }

            Destroy(gameObject);
        }
    }
}
