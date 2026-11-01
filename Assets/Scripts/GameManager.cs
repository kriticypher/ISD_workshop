using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int score = 0;
    public int bombCount = 0;
    public bool isGameOver = false;

    public TMP_Text scoreText;
    public TMP_Text bombText;
    public TMP_Text gameOverText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Auto-find references if not assigned in Inspector
        if (scoreText == null)
        {
            GameObject obj = GameObject.Find("ScoreText");
            if (obj != null) scoreText = obj.GetComponent<TMP_Text>();
        }

        if (bombText == null)
        {
            GameObject obj = GameObject.Find("BombText");
            if (obj != null) bombText = obj.GetComponent<TMP_Text>();
        }

        if (gameOverText == null)
        {
            GameObject obj = GameObject.Find("GameOverText");
            if (obj != null)
            {
                gameOverText = obj.GetComponent<TMP_Text>();
            }
            else
            {
                GameObject canvas = GameObject.Find("Canvas");
                if (canvas != null)
                {
                    Transform t = canvas.transform.Find("GameOverText");
                    if (t != null) gameOverText = t.GetComponent<TMP_Text>();
                }
            }
        }
    }

    private void Start()
    {
        score = 0;
        bombCount = 0;
        isGameOver = false;

        UpdateUI();

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (isGameOver)
        {
            if (GetRestartInput())
            {
                RestartGame();
            }
        }
    }

    private bool GetRestartInput()
    {
        bool pressed = false;

#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Keyboard.current != null &&
            UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame)
        {
            pressed = true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (!pressed)
        {
            try
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    pressed = true;
                }
            }
            catch (System.InvalidOperationException)
            {
            }
        }
#endif

        return pressed;
    }

    public void AddScore(int amount = 1)
    {
        if (isGameOver) return;

        score += amount;
        UpdateUI();
    }

    public void AddBomb(int amount = 1)
    {
        if (isGameOver) return;

        bombCount += amount;
        UpdateUI();

        if (bombCount >= 3)
        {
            GameOver();
        }
    }

    public void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }

        if (bombText != null)
        {
            bombText.text = "Bombs: " + bombCount;
        }
    }

    private void GameOver()
    {
        isGameOver = true;

        if (gameOverText != null)
        {
            gameOverText.text = "GAME OVER\nScore: " + score + "\nBombs: " + bombCount + "\nPress R to Restart";
            gameOverText.gameObject.SetActive(true);
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
