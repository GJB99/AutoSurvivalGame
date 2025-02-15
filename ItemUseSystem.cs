using UnityEngine;

public class ItemUseSystem : MonoBehaviour
{
    private PlayerInventory inventory;
    public int selectedSlot = -1;
    public GameObject arrowPrefab;
    public float arrowSpeed = 20f;
    public float bowCooldown = 0.5f;
    private float lastShotTime;
    private Vector2 lastMovementDirection = Vector2.right;
    private ItemBar itemBar;

    void Start()
    {
        inventory = GetComponent<PlayerInventory>();
        itemBar = FindObjectOfType<ItemBar>();
    }

void Update()
{
    float horizontalInput = Input.GetAxisRaw("Horizontal");
    float verticalInput = Input.GetAxisRaw("Vertical");
    
    if (horizontalInput != 0 || verticalInput != 0)
    {
        lastMovementDirection = new Vector2(horizontalInput, verticalInput).normalized;
    }

    bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

    // Handle slot selection (1-5 keys) only when shift is NOT held
    if (!shiftHeld)
    {
        for (int i = 0; i < 5; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                selectedSlot = i;
                // Update the item bar visuals when slot changes
                if (itemBar != null)
                {
                    itemBar.UpdateItemBar();
                }
            }
        }
    }

    // Handle item use
    if (Input.GetKeyDown(KeyCode.Space) && selectedSlot != -1)
    {
        UseSelectedItem();
    }
}

void UseSelectedItem()
{
    var itemBarItems = inventory.GetItemBarItems();
    if (selectedSlot >= itemBarItems.Count) return;

    string itemName = itemBarItems[selectedSlot].Key;
    if (itemName == "Bow" && Time.time >= lastShotTime + bowCooldown)
    {
        // Check if player has arrows in inventory
        int totalArrows = inventory.GetTotalItemCount("Arrow");
        if (totalArrows > 0)
        {
            ShootArrow();
            // Remove one arrow from inventory
            inventory.RemoveItems("Arrow", 1);
            lastShotTime = Time.time;
        }
        else
        {
            // No arrows available
            FindObjectOfType<UIManager>().ShowUpperMessage("You need arrows to shoot the bow!");
        }
    }
}

void ShootArrow()
{
    // Get mouse position in world coordinates
    Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    
    // Calculate direction from player to mouse
    Vector2 direction = (mousePosition - (Vector2)transform.position).normalized;

    // Create arrow
    GameObject arrow = Instantiate(arrowPrefab, transform.position, Quaternion.identity);
    
    // Calculate the angle from the direction and add 225 degrees to align the sprite
    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 225f;
    
    // Set the arrow's rotation to point in the direction of travel
    arrow.transform.rotation = Quaternion.Euler(0, 0, angle);
    
    // Initialize arrow direction using the Arrow component
    Arrow arrowComponent = arrow.GetComponent<Arrow>();
    if (arrowComponent != null)
    {
        arrowComponent.Initialize(direction);
    }
    else
    {
        Debug.LogError("Arrow component not found on arrow prefab!");
    }
    
    // Destroy arrow after 5 seconds as a fallback
    Destroy(arrow, 5f);
}
}