using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Enemy : MonoBehaviour {
    [Header("Ball Settings")]
    [SerializeField] float speed = 10f;
    [SerializeField] float maxSpeed = 20f;
    [SerializeField] float speedIncrement = 0.5f;
    [SerializeField] int hitsBeforeSpeedIncrease = 1;

    [Header("UI Animation")]
    [SerializeField] TextMeshProUGUI LoseText;
    [SerializeField] UIAutoAnimation uiAnimation;

    [Header("Scene Reload Settings")]
    [SerializeField] float delayBeforeReload = 2f;

    Rigidbody2D rb;
    Vector2 lastVelocity;
    int hitCount = 0;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        if (uiAnimation == null) uiAnimation = FindAnyObjectByType<UIAutoAnimation>();
        LaunchBall();
    }

    void Update() {
        lastVelocity = rb.linearVelocity;

        if (rb.linearVelocity.magnitude > maxSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;

        // keep trajectory from being too vertical
        if (Mathf.Abs(rb.linearVelocity.y) > Mathf.Abs(rb.linearVelocity.x) * 3f) {
            float newY = Mathf.Sign(rb.linearVelocity.y) * Mathf.Abs(rb.linearVelocity.x) * 2f;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, newY);
        }
    }

    void OnCollisionEnter2D(Collision2D collision) {
        Vector2 direction = Vector2.Reflect(lastVelocity.normalized, collision.contacts[0].normal);

        if (collision.gameObject.CompareTag("Player")) {
            StartCoroutine(HandleGameOver());
            Destroy(collision.gameObject);
            return;
        }

        if (collision.gameObject.CompareTag("Ground")) {
            hitCount++;
            if (hitCount >= hitsBeforeSpeedIncrease) {
                IncreaseSpeed();
                hitCount = 0;
            }
        }

        if (collision.gameObject.CompareTag("Wall"))
            HandleWallCollision(direction);
    }

    void IncreaseSpeed() {
        float current = rb.linearVelocity.magnitude;
        float next = Mathf.Min(current + speedIncrement, maxSpeed);
        rb.linearVelocity = rb.linearVelocity.normalized * next;
        Debug.Log($"Speed increased to: {next}");
    }

    IEnumerator HandleGameOver() {
        rb.linearVelocity = Vector2.zero;

        if (uiAnimation != null) {
            LoseText.gameObject.SetActive(true);
            uiAnimation.EntranceAnimation();
        }

        yield return new WaitForSeconds(delayBeforeReload);
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }

    void HandleWallCollision(Vector2 direction)
        => rb.linearVelocity = direction * lastVelocity.magnitude;

    void LaunchBall() {
        float x = Random.Range(0, 2) == 0 ? -1 : 1;
        float y = Random.Range(-1f, 1f);
        rb.linearVelocity = new Vector2(x, y).normalized * speed;
    }

    public void SetDirection(Vector2 direction) => rb.linearVelocity = direction.normalized * speed;
    public float GetCurrentSpeed() => rb.linearVelocity.magnitude;

    public void TriggerEntranceAnimation() { if (uiAnimation != null) uiAnimation.EntranceAnimation(); }
    public void TriggerExitAnimation() { if (uiAnimation != null) uiAnimation.ExitAnimation(); }
}
