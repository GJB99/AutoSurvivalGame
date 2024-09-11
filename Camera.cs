using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; // The player's transform
    public float smoothSpeed = 0.125f; // Smoothing factor for camera movement
    public Vector3 offset = new Vector3(0, 0, -10); // Offset from the player

    public float edgeScrollSpeed = 10f; // Speed of edge scrolling
    public float edgeScrollThreshold = 10f; // Distance from screen edge to start scrolling

    public float minX, maxX, minY, maxY; // Camera boundaries
    public bool isEdgeScrolling = true; // Toggle for edge scrolling

    private Vector3 desiredPosition;
    private bool isFollowingPlayer = true;

    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            isFollowingPlayer = !isFollowingPlayer;
        }

        if (isFollowingPlayer)
        {
            FollowPlayer();
        }
        else if (isEdgeScrolling)
        {
            EdgeScroll();
        }

        // Clamp the camera position within the defined boundaries
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, minX, maxX),
            Mathf.Clamp(transform.position.y, minY, maxY),
            transform.position.z
        );
    }

    void FollowPlayer()
    {
        if (target != null)
        {
            desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }
    }

    void EdgeScroll()
    {
        Vector3 pos = transform.position;

        if (Input.mousePosition.x >= Screen.width - edgeScrollThreshold)
        {
            pos.x += edgeScrollSpeed * Time.deltaTime;
        }
        else if (Input.mousePosition.x <= edgeScrollThreshold)
        {
            pos.x -= edgeScrollSpeed * Time.deltaTime;
        }

        if (Input.mousePosition.y >= Screen.height - edgeScrollThreshold)
        {
            pos.y += edgeScrollSpeed * Time.deltaTime;
        }
        else if (Input.mousePosition.y <= edgeScrollThreshold)
        {
            pos.y -= edgeScrollSpeed * Time.deltaTime;
        }

        transform.position = Vector3.Lerp(transform.position, pos, smoothSpeed);
    }
}