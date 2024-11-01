using UnityEngine;
using TMPro;
using System.Collections;
using System.Text.RegularExpressions;
using static Resource;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float baseSpeed = 5f;
    public float miningRate = 1f;
    public float cellSize = 1f;
    public TextMeshProUGUI miningInfoText;
    public TextMeshProUGUI messageText;
    public float messageDisplayTime = 2f;
    public Texture2D miningCursor;
    public Texture2D handCursor;
    public Texture2D defaultCursor;
    public Texture2D hoeCursor;
    public Texture2D selectingCursor;

    private Vector3 targetPosition;
    private bool isMoving = false;
    private Resource currentResource;
    private float miningTimer;
    private bool isMining = false;
    private PlayerInventory inventory;
    private MessageManager messageManager;
    private bool isHoldingRightClick = false;
    private UIManager uiManager;
    private Camera mainCamera;

void Start()
{
    baseSpeed = moveSpeed;
    messageManager = FindObjectOfType<MessageManager>();
    inventory = GetComponent<PlayerInventory>();
    uiManager = FindObjectOfType<UIManager>();
    mainCamera = Camera.main;
    
    if (mainCamera == null)
    {
        Debug.LogError("Main Camera not found! Make sure there's a camera tagged as 'MainCamera' in the scene.");
        return;
    }
    
    if (inventory == null)
    {
        Debug.LogError("PlayerInventory component not found on the player!");
    }
    if (uiManager == null)
    {
        Debug.LogError("UIManager not found in the scene!");
    }

    Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
}

void Update()
{
    // Handle WASD movement
    float horizontalInput = Input.GetAxisRaw("Horizontal");
    float verticalInput = Input.GetAxisRaw("Vertical");
    
    if (horizontalInput != 0 || verticalInput != 0)
    {
        Vector3 movement = new Vector3(horizontalInput, verticalInput, 0).normalized;
        transform.position += movement * moveSpeed * Time.deltaTime;
        isMoving = true;
    }
    else
    {
        isMoving = false;
    }

    // Check for resources in range when not moving
    if (!isMoving)
    {
        CheckForResourcesInRange();
    }

    // Handle mining and other interactions
    if (isMoving)
    {
        isMining = false;
        currentResource = null;
    }
    else if (isMining && currentResource != null)
    {
        if (currentResource.IsInMiningRange(transform.position))
        {
            MineResource();
        }
        else
        {
            isMining = false;
            currentResource = null;
            ShowMessage("Out of range for mining.");
        }
    }

    // Handle mouse input for resource info
    HandleMouseInput();
    UpdateCursor();
}

private void UpdateCursor()
{
    if (mainCamera == null) return;
    
    Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
    RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

    if (hit.collider != null)
    {
        Resource resource = hit.collider.GetComponent<Resource>();
        if (resource != null)
        {
            if (resource.resourceType == ResourceType.Food)
            {
                Cursor.SetCursor(hoeCursor, Vector2.zero, CursorMode.Auto);
            }
            else if (resource.isMineableResource())
            {
                Cursor.SetCursor(miningCursor, Vector2.zero, CursorMode.Auto);
            }
            else
            {
                Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
            }
        }
        else if (hit.collider.CompareTag("Water"))
        {
            Cursor.SetCursor(selectingCursor, Vector2.zero, CursorMode.Auto);
        }
        else if (IsClickableObject(hit.collider.gameObject))
        {
            Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
        }
    }
    else
    {
        Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
    }
}

private void CheckForResourcesInRange()
{
    if (Input.GetMouseButtonDown(1)) // Left click to start mining
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        if (hit.collider != null)
        {
            Resource resource = hit.collider.GetComponent<Resource>();
            if (resource != null)
            {
                if (resource.IsInMiningRange(transform.position))
                {
                    StartMining(resource);
                }
                else
                {
                    ShowMessage("Move closer to mine this resource!");
                }
            }
        }
    }
}

private void HandleMouseInput()
{
    if (mainCamera == null) return;
    
    Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
    RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

    if (hit.collider != null)
    {
        if (IsClickableObject(hit.collider.gameObject))
        {
            if (Input.GetMouseButtonDown(1))
            {
                InteractWithClickable(hit.collider.gameObject);
            }
        }
        else if (hit.collider.GetComponent<Resource>() != null && Input.GetMouseButtonDown(0))
        {
            ShowResourceInfo(hit.collider.GetComponent<Resource>());
        }
    }
}

bool IsClickableObject(GameObject obj)
{
    return obj.GetComponent<Smelter>() != null || obj.CompareTag("Water");
}

void InteractWithClickable(GameObject clickableObject)
{
    if (clickableObject.TryGetComponent<Smelter>(out var smelter))
    {
        InteractWithSmelter(smelter);
    }
    else if (clickableObject.CompareTag("Water"))
    {
        InteractWithWater();
    }
}

void InteractWithWater()
{
    if (inventory.IsItemSelected("Bucket"))
    {
        inventory.RemoveItems("Bucket", 1);
        inventory.AddItem("Bucket with Water", 1);
        ShowMessage("Filled bucket with water");
    }
    else
    {
        ShowMessage("You need to select a Bucket to collect water!");
    }
}

    void HandleRightClick(Vector2 mouseWorldPosition, RaycastHit2D hit)
    {
        if (hit.collider != null)
        {
            Resource clickedResource = hit.collider.GetComponent<Resource>();
            if (clickedResource != null)
            {
                if (Vector2.Distance(transform.position, hit.point) <= clickedResource.miningRange + cellSize / 2f)
                {
                    StartMining(clickedResource);
                }
                else
                {
                    MoveToResourceAndMine(clickedResource, hit.point);
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
            SetTargetPosition(mouseWorldPosition);
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
    if (uiManager != null)
    {
        uiManager.ShowUpperMessage(message);
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
        bool canMine = currentResource.isFood || CanMineWithCurrentTools(currentResource.resourceName);

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
            
            // Show resource gain message
            uiManager.ShowResourceGainMessage(currentResource.resourceName, 1);
            
            if (currentResource.IsDepletedResource())
            {
                isMining = false;
                currentResource = null;
            }
        }
    }
    else
    {
        isMining = false;
        currentResource = null;
    }
}

bool CanMineWithCurrentTools(string resourceName)
{
    if (resourceName == "Rock" || resourceName == "Wood" || resourceName == "Plant Fiber" || resourceName == "Clay" || resourceName == "Tin" || resourceName == "Uranium" || resourceName == "Scrap" || resourceName == "Oil" || resourceName == "Titanium")
        return true;
    else if (resourceName == "Iron" && (inventory.HasStonePickaxe() || inventory.HasIronPickaxe()))
        return true;
    else if (resourceName == "Copper" && inventory.HasIronPickaxe())
        return true;
    return false;
}

    void MoveToResourceAndMine(Resource resource, Vector2 targetPoint)
    {
        currentResource = resource;
        SetTargetPosition(targetPoint);
        StartCoroutine(WaitForMoveAndStartMining());
    }

    IEnumerator WaitForMoveAndStartMining()
    {
        while (isMoving)
        {
            yield return null;
        }

        if (currentResource != null && currentResource.IsInMiningRange(transform.position))
        {
            StartMining(currentResource);
        }
        else
        {
            Debug.Log("Failed to move into mining range.");
        }
    }

    void StartMining(Resource resource)
    {
        currentResource = resource;
        isMining = true;
        isMoving = false;
        Debug.Log($"Started mining {currentResource.resourceName}");
    }

    void InteractWithSmelter(Smelter smelter)
    {
    if (Input.GetMouseButtonDown(1)) // Right-click
    {
        ShowMessage(smelter.GetSmelterInfo());
    }
    }
}