using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 8f;
    public float xBound = 7.5f;

    void Update()
    {
        // Stop movement if the game is over
        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
            return;

        float move = GetHorizontalInput();

        transform.Translate(Vector2.right * move * speed * Time.deltaTime);

        // Keep the player inside the screen boundaries
        float clampedX = Mathf.Clamp(transform.position.x, -xBound, xBound);
        transform.position = new Vector2(clampedX, transform.position.y);
    }

    private float GetHorizontalInput()
    {
        float move = 0f;

#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)
                move -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed)
                move += 1f;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (move == 0f)
        {
            try
            {
                move = Input.GetAxisRaw("Horizontal");
            }
            catch (System.InvalidOperationException)
            {
            }
        }
#endif

        return move;
    }
}
