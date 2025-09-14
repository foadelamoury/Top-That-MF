using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Enemy : MonoBehaviour
{
    [Header("Ball Settings")]
    [SerializeField] float speed = 10f;
    [SerializeField] float maxSpeed = 20f;
    [SerializeField] float speedIncrement = 0.5f;
    [SerializeField] int hitsBeforeSpeedIncrease = 1; // How many block hits before increasing speed

    [Header("UI Animation")]
    [SerializeField] TextMeshProUGUI LoseText;
    [SerializeField] UIAutoAnimation uiAnimation; // Reference to UIAutoAnimation component

    [Header("Scene Reload Settings")]
    [SerializeField] float delayBeforeReload = 2f; // Time to wait before reloading scene

    private Rigidbody2D rb;
    private Vector2 lastVelocity;
    private int hitCount = 0; // Counter for block hits

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // If uiAnimation is not assigned in inspector, try to find it
        if (uiAnimation == null)
        {
            uiAnimation = FindAnyObjectByType<UIAutoAnimation>();
        }
        LaunchBall();
    }

    void Update()
    {
        // Store the velocity for collision calculations
        lastVelocity = rb.linearVelocity;
        // Clamp the speed to prevent it from getting too fast
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
        // Prevent the ball from getting stuck moving too vertically
        if (Mathf.Abs(rb.linearVelocity.y) > Mathf.Abs(rb.linearVelocity.x) * 3f)
        {
            float newY = Mathf.Sign(rb.linearVelocity.y) * Mathf.Abs(rb.linearVelocity.x) * 2f;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, newY);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Calculate reflection
        Vector2 direction = Vector2.Reflect(lastVelocity.normalized, collision.contacts[0].normal);

        if (collision.gameObject.CompareTag("Player"))
        {
            // Start the game over coroutine
            StartCoroutine(HandleGameOver());

            // Destroy the player object
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("Ground"))
        {
            // Increment hit count when colliding with a block
            hitCount++;

            // Increase speed after specified number of hits
            if (hitCount >= hitsBeforeSpeedIncrease)
            {
                IncreaseSpeed();
                hitCount = 0; // Reset counter
            }
        }

        // Handle wall collisions
        if (collision.gameObject.CompareTag("Wall"))
        {
            HandleWallCollision(direction);
        }
    }

    // Method to increase the ball's speed
    private void IncreaseSpeed()
    {
        float currentSpeed = rb.linearVelocity.magnitude;
        float newSpeed = Mathf.Min(currentSpeed + speedIncrement, maxSpeed);

        // Apply the new speed while maintaining direction
        rb.linearVelocity = rb.linearVelocity.normalized * newSpeed;

        Debug.Log($"Speed increased to: {newSpeed}");
    }

    // Coroutine to handle game over sequence
    IEnumerator HandleGameOver()
    {
        // Stop the ball movement
        rb.linearVelocity = Vector2.zero;

        // Show the lose text and trigger UI animation
        if (uiAnimation != null)
        {
            LoseText.gameObject.SetActive(true); // Show the lose text
            uiAnimation.EntranceAnimation(); // This will trigger the entrance animation
        }

        // Wait for the specified delay (allows UI animation to complete)
        yield return new WaitForSeconds(delayBeforeReload);

        // Reload the current scene
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }

    void HandleWallCollision(Vector2 direction)
    {
        // Simple reflection for walls
        rb.linearVelocity = direction * lastVelocity.magnitude;
    }

    void LaunchBall()
    {
        // Random direction (left or right)
        float x = Random.Range(0, 2) == 0 ? -1 : 1;
        float y = Random.Range(-1f, 1f);
        Vector2 direction = new Vector2(x, y).normalized;
        rb.linearVelocity = direction * speed;
    }

    // Public method to manually set ball direction (useful for testing)
    public void SetDirection(Vector2 direction)
    {
        rb.linearVelocity = direction.normalized * speed;
    }

    // Public method to get current speed
    public float GetCurrentSpeed()
    {
        return rb.linearVelocity.magnitude;
    }

    // Public methods to trigger UI animations
    public void TriggerEntranceAnimation()
    {
        if (uiAnimation != null)
        {
            uiAnimation.EntranceAnimation();
        }
    }

    public void TriggerExitAnimation()
    {
        if (uiAnimation != null)
        {
            uiAnimation.ExitAnimation();
        }
    }
}