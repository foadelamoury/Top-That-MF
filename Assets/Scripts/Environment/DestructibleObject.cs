using UnityEngine;

public class DestructibleObject : MonoBehaviour {
    public float life = 3;

    float shakeDuration = 0f;
    float shakeMagnitude = 0.25f;
    float dampingSpeed = 1f;

    Vector3 initialPosition;

    void Awake() => initialPosition = transform.position;

    void Update() {
        if (life <= 0) {
            Destroy(gameObject);
            return;
        }

        if (shakeDuration > 0) {
            transform.localPosition = initialPosition + Random.insideUnitSphere * shakeMagnitude;
            shakeDuration -= Time.deltaTime * dampingSpeed;
        }
        else {
            shakeDuration = 0f;
            transform.localPosition = initialPosition;
        }
    }

    public void ApplyDamage(float damage) {
        life -= 1;
        shakeDuration = 0.1f;
    }
}
