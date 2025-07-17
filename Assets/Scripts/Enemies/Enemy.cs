using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour {

    [Header("Ball Settings")]
    [SerializeField] float speed = 10f;
    [SerializeField] float maxSpeed = 20f;
    [SerializeField] float speedIncrement = 0.5f;

  

    private Rigidbody2D rb;
    private Vector2 lastVelocity;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

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

       
         if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Ground"))
        {
            HandleWallCollision(direction);
        }
         if(collision.gameObject.CompareTag("Player"))
        {
            Destroy(collision.gameObject);
        }
      
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
}
