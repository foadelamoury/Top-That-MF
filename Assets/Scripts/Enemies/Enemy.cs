using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Enemy : MonoBehaviour {
    [Header("Ball Settings")]
    [SerializeField] float speed = 10f;            // initial launch speed
    [SerializeField] float maxSpeed = 20f;         // hard cap
    [SerializeField] float speedIncrement = 0.5f;  // added every N ground hits
    [SerializeField] int hitsBeforeSpeedIncrease = 1;

    [Header("Post-Bounce Floor")]
    [Tooltip("Ensures the ball never gets too slow after a collision.")]
    [SerializeField] float minSpeedAfterBounce = 8f;

    [Header("UI Animation")]
    [SerializeField] TextMeshProUGUI LoseText;
    [SerializeField] UIAutoAnimation uiAnimation;

    [Header("Scene Reload Settings")]
    [SerializeField] float delayBeforeReload = 2f;

    Rigidbody2D rb;
    Vector2 lastVelocity;   // cached each FixedUpdate
    int hitCount = 0;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        if (uiAnimation == null) uiAnimation = FindAnyObjectByType<UIAutoAnimation>();
        LaunchBall();
    }

    void FixedUpdate() {
        var v = rb.linearVelocity;

        // Clamp max speed
        float mag = v.magnitude;
        if (mag > maxSpeed) {
            v = v.normalized * maxSpeed;
            mag = maxSpeed;
        }

        // Keep trajectory from being too vertical, but PRESERVE magnitude
        // (prevents hidden energy loss when we bend the path)
        if (Mathf.Abs(v.y) > Mathf.Abs(v.x) * 3f) {
            float newY = Mathf.Sign(v.y) * Mathf.Abs(v.x) * 2f;
            v = new Vector2(v.x, newY).normalized * mag;
        }

        rb.linearVelocity = v;
        lastVelocity = v; // cache for collisions in physics time
    }

    void OnCollisionEnter2D(Collision2D collision) {
        // Player -> Game Over
        if (collision.gameObject.CompareTag("Player")) {
            StartCoroutine(HandleGameOver());
            Destroy(collision.gameObject);
            return;
        }

        // Use physics-cached velocity to reflect with stable magnitude
        Vector2 n = collision.contacts[0].normal;
        float preSpeed = lastVelocity.magnitude;
        Vector2 dir = Vector2.Reflect(lastVelocity.normalized, n);
        float postSpeed = Mathf.Clamp(Mathf.Max(preSpeed, minSpeedAfterBounce), 0f, maxSpeed);
        rb.linearVelocity = dir * postSpeed;

        // Count ground hits to ramp speed gradually (optional)
        if (collision.gameObject.CompareTag("Ground")) {
            hitCount++;
            if (hitCount >= hitsBeforeSpeedIncrease) {
                IncreaseSpeed();   // preserves direction, clamps to max
                hitCount = 0;
            }
        }
    }

    void IncreaseSpeed() {
        float current = rb.linearVelocity.magnitude;
        float next = Mathf.Min(current + speedIncrement, maxSpeed);
        rb.linearVelocity = rb.linearVelocity.sqrMagnitude > 0.0001f
            ? rb.linearVelocity.normalized * next
            : Random.insideUnitCircle.normalized * next;

        Debug.Log($"[Enemy] Speed increased to: {next:0.00}");
    }

    IEnumerator HandleGameOver() {
        rb.linearVelocity = Vector2.zero;

        if (uiAnimation != null) {
            if (LoseText) LoseText.gameObject.SetActive(true);
            uiAnimation.EntranceAnimation();
        }

        yield return new WaitForSeconds(delayBeforeReload);

        // Reload current scene (simple, avoids hitches)
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);

        // If you really need to stack an additive scene, uncomment next line:
        SceneManager.LoadScene("Timing", LoadSceneMode.Additive);
    }

    void LaunchBall() {
        float x = Random.Range(0, 2) == 0 ? -1 : 1;
        float y = Random.Range(-1f, 1f);
        rb.linearVelocity = new Vector2(x, y).normalized * speed;
    }

    // ---- Optional helpers (safe) ----
    public void SetDirection(Vector2 direction) {
        // Keep current magnitude (or initial speed if stopped), clamp to max
        float mag = Mathf.Max(rb.linearVelocity.magnitude, speed);
        rb.linearVelocity = direction.sqrMagnitude > 0.0001f
            ? direction.normalized * Mathf.Min(mag, maxSpeed)
            : rb.linearVelocity;
    }

    public float GetCurrentSpeed() => rb.linearVelocity.magnitude;

    public void TriggerEntranceAnimation() { if (uiAnimation != null) uiAnimation.EntranceAnimation(); }
    public void TriggerExitAnimation() { if (uiAnimation != null) uiAnimation.ExitAnimation(); }
}
