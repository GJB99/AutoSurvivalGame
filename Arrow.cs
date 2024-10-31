using UnityEngine;

public class Arrow : MonoBehaviour
{
    public int damage = 10;
    private float distanceTraveled = 0f;
    private float maxDistance = 10f;
    private Vector2 lastPosition;
    
    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        // Calculate distance traveled
        float deltaDistance = Vector2.Distance(lastPosition, transform.position);
        distanceTraveled += deltaDistance;
        lastPosition = transform.position;

        // Destroy if traveled too far
        if (distanceTraveled >= maxDistance)
        {
            Destroy(gameObject);
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Only check for Enemy tag if the object has any tag
        if (other.gameObject.tag != "Untagged" && other.CompareTag("Enemy"))
        {
            // Deal damage to enemy
            Destroy(gameObject);
        }
        else if (!other.CompareTag("Player") && !other.CompareTag("UI") && !other.CompareTag("Arrow") && !other.CompareTag("Ground"))
        {
            // Destroy arrow when hitting anything except the player, other arrows, or ground
            Destroy(gameObject);
        }
    }
}