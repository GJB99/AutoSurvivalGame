using UnityEngine;

public class Arrow : MonoBehaviour
{
    public int damage = 10;
    public float speed = 20f;
    private float distanceTraveled = 0f;
    private float maxDistance = 10f;
    private float hitDistance = 0.5f;
    private Vector2 lastPosition;
    private Vector2 direction;
    private bool isInitialized = false;
    
    public void Initialize(Vector2 shootDirection)
    {
        direction = shootDirection;
        lastPosition = transform.position;
        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized) return;

        // Move arrow in its direction
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
        
        // Calculate distance traveled
        float deltaDistance = Vector2.Distance(lastPosition, transform.position);
        distanceTraveled += deltaDistance;
        lastPosition = transform.position;

        // Check for nearby objects
        Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(transform.position, hitDistance);
        foreach (Collider2D collider in nearbyObjects)
        {
            if (!collider.CompareTag("Player") && 
                !collider.CompareTag("Water") && 
                !collider.CompareTag("Resource") && 
                !collider.CompareTag("UI") && 
                !collider.CompareTag("Arrow") && 
                !collider.CompareTag("Ground"))
            {
                Destroy(gameObject);
                return;
            }
        }

        // Destroy if traveled too far
        if (distanceTraveled >= maxDistance)
        {
            Destroy(gameObject);
        }
    }
}