using UnityEngine;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float miningRate = 1f;
    public float cellSize = 1f;
    public TextMeshProUGUI miningInfoText;
    public TextMeshProUGUI messageText;
    public float messageDisplayTime = 2f;
    public Texture2D miningCursor;

    private Vector3 targetPosition;
    private bool isMoving = false;
    private Resource currentResource;
    private float miningTimer;
    private bool isMining = false;
    private PlayerInventory inventory;
    private MessageManager messageManager;

    void Start()
    {
        messageManager = FindObjectOfType<MessageManager>();
        inventory = GetComponent<PlayerInventory>();
        if (inventory == null)
        {
            Debug.LogError("PlayerInventory component not found on the player!");
        }
    }

    void Update()
    {
        HandleMouseInput();

        // If the player is moving, stop mining
        if (isMoving)
        {
            isMining = false;
        }
        else
        {
            // If player is not moving and trying to mine a resource
            if (isMining && currentResource != null)
            {
                if (currentResource.IsInMiningRange(transform.position))
                {
                    MineResource();
                }
                else
                {
                    // Player is out of range, stop mining
                    isMining = false;
                    currentResource = null;
                    Debug.Log("Out of range for mining.");
                }
            }
        }

        if (isMoving)
        {
            MovePlayer();
        }
    }


    void InteractWithSmelter(Smelter smelter)
    {
    if (Input.GetMouseButtonDown(1)) // Right-click
    {
        ShowMessage(smelter.GetSmelterInfo());
    }
    }

    void HandleMouseInput()
    {
        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPosition, Vector2.zero);

        if (hit.collider != null)
        {
            Smelter clickedSmelter = hit.collider.GetComponent<Smelter>();
            if (clickedSmelter != null)
            {
                InteractWithSmelter(clickedSmelter);
                return;
            }

            Resource clickedResource = hit.collider.GetComponent<Resource>();
            if (clickedResource != null)
            {
                // Change cursor to mining cursor when hovering over a resource
                Cursor.SetCursor(miningCursor, Vector2.zero, CursorMode.Auto);

                if (Input.GetMouseButtonDown(0)) // Left click
                {
                    ShowResourceInfo(clickedResource);
                }
                else if (Input.GetMouseButtonDown(1)) // Right click
                {
                    SetTargetResource(clickedResource, hit.point);
                }
            }
            else
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                if (Input.GetMouseButtonDown(1)) // Right click
                {
                    SetTargetPosition(hit.point);
                }
            }
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            if (Input.GetMouseButtonDown(1)) // Right click
            {
                SetTargetPosition(mouseWorldPosition);
            }
        }
    }

    void HandleRightClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null)
        {
            Resource clickedResource = hit.collider.GetComponent<Resource>();
            if (clickedResource != null)
            {
                if (Vector2.Distance(transform.position, hit.point) <= clickedResource.miningRange + cellSize / 2f)
                {
                    currentResource = clickedResource;
                    isMining = true;
                    isMoving = false;
                    Debug.Log($"Started mining {currentResource.resourceName}");
                }
                else
                {
                    SetTargetPosition(hit.point);
                    isMining = false;
                    Debug.Log($"Moving towards {clickedResource.resourceName}");
                }
            }
            else
            {
                SetTargetPosition(hit.point);
                isMining = false;
            }
        }
        else
        {
            SetTargetPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            isMining = false;
        }
    }

    void ShowResourceInfo(Resource resource)
    {
        string info = $"{resource.resourceName}\nQuantity: {resource.currentQuantity}/{resource.initialQuantity}";
        ShowMessage(info);
    }

    void SetTargetResource(Resource resource, Vector2 position)
    {
        currentResource = resource;
        // Set the target position to the center of the resource's cell
        Vector3 resourceCenter = resource.transform.position + new Vector3(resource.cellSize / 2, resource.cellSize / 2, 0);
        SetTargetPosition(resourceCenter);
        isMining = false; // Will start mining when in range
    }

    void SetTargetPosition(Vector3 target)
    {
        targetPosition = target;
        targetPosition.z = 0f; // Keep the player on the same z plane
        isMoving = true;
    }

    void MovePlayer()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            isMoving = false;
            
            if (currentResource != null && !isMining)
            {
                if (currentResource.IsInMiningRange(transform.position))
                {
                    isMining = true;
                    Debug.Log($"In range, started mining {currentResource.resourceName}");
                }
                else
                {
                    Debug.Log($"Not in range to mine {currentResource.resourceName}");
                }
            }
        }
    }

    void ShowMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
            CancelInvoke("ClearMessage");
            Invoke("ClearMessage", messageDisplayTime);
        }
        Debug.Log(message);
    }

    void ClearMessage()
    {
        if (messageText != null)
        {
            messageText.text = "";
        }
    }

    void MineResource()
    {
        if (currentResource != null && currentResource.IsInMiningRange(transform.position))
        {
            bool canMine = false;
            if (currentResource.resourceName == "Rock")
            {
                canMine = true;
            }
            else if (currentResource.resourceName == "Iron" && (inventory.HasStonePickaxe() || inventory.HasIronPickaxe()))
            {
                canMine = true;
            }
            else if (currentResource.resourceName == "Copper" && inventory.HasIronPickaxe())
            {
                canMine = true;
            }

            if (!canMine)
            {
                ShowMessage($"You need a better pickaxe to mine {currentResource.resourceName}!");
                isMining = false;
                currentResource = null;
                return;
            }

            miningTimer += Time.deltaTime;
            if (miningTimer >= miningRate)
            {
                miningTimer = 0f;
                currentResource.Mine();
                inventory.AddItem(currentResource.resourceName, 1);
                Debug.Log($"Mined {currentResource.resourceName}. Remaining: {currentResource.currentQuantity}");
                
                if (currentResource.IsDepletedResource())
                {
                    Debug.Log($"{currentResource.resourceName} depleted!");
                    isMining = false;
                    currentResource = null;
                }
            }
        }
        else
        {
            isMining = false;
            currentResource = null;
            Debug.Log("No resource to mine");
        }
    }
}