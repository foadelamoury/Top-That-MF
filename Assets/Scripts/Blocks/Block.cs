using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour {
    public int numberOfFlashes = 3;
    public float flashDuration = 0.1f;
    public string noCollisionLayer = "NoEnemyCollision";

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (!collision.gameObject.CompareTag("Enemy")) return;

        gameObject.layer = LayerMask.NameToLayer(noCollisionLayer);
        StartCoroutine(FlashAndDestroy());
    }

    private IEnumerator FlashAndDestroy() {
        for (int i = 0; i < numberOfFlashes; i++) {
            if (spriteRenderer != null) spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(flashDuration);
            if (spriteRenderer != null) spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(flashDuration);
        }
        Destroy(gameObject);
    }
}
