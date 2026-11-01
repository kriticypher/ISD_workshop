using UnityEngine;
using TMPro;

public class Bomb : MonoBehaviour
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
                GameManager.Instance.AddBomb(1);
            }
            else
            {
                // Fallback direct UI update if GameManager is not present
                GameObject bombObject = GameObject.Find("BombText");
                if (bombObject != null)
                {
                    TMP_Text bombText = bombObject.GetComponent<TMP_Text>();
                    if (bombText != null)
                    {
                        bombText.text = "Bombs: " + (int.Parse(bombText.text.Replace("Bombs: ", "")) + 1);
                    }
                }
            }

            Destroy(gameObject);
        }
    }
}
