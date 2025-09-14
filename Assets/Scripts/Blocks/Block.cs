using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour
{
    public int numberOfFlashes = 3; // Number of times to flash
    public float flashDuration = 0.1f; // Duration of each flash
    public string noCollisionLayer = "NoEnemyCollision"; // Layer name to switch to after collision

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private int originalLayer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // Store the original layer
        originalLayer = gameObject.layer;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Change to a layer that doesn't collide with enemies
            gameObject.layer = LayerMask.NameToLayer(noCollisionLayer);

            StartCoroutine(FlashAndDestroy());
        }
    }

    private IEnumerator FlashAndDestroy()
    {
        for (int i = 0; i < numberOfFlashes; i++)
        {
            // Flash red
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.red;
            }
            yield return new WaitForSeconds(flashDuration);

            // Flash white
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.white;
            }
            yield return new WaitForSeconds(flashDuration);
        }

        // Restore original color (optional)
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        Destroy(gameObject);
    }
}