using UnityEngine;

public class ThrowableWeapon : MonoBehaviour {
    public Vector2 direction;
    public float speed = 10f;

    private bool hasHit = false;
    private Rigidbody2D rb;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    void FixedUpdate() {
        if (!hasHit && rb != null)
            rb.linearVelocity = direction * speed;
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (hasHit) return; // prevent double-processing on rapid contacts
        hasHit = true;

        if (collision.gameObject.CompareTag("Enemy")) {
            collision.gameObject.SendMessage("ApplyDamage", Mathf.Sign(direction.x) * 2f);
            Destroy(gameObject);
            return;
        }

        if (!collision.gameObject.CompareTag("Player")) {
            Destroy(gameObject);
        }
    }
}
