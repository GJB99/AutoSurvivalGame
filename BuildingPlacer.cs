using UnityEngine;

public class BuildingPlacer : MonoBehaviour
{
    private GameObject buildingPrefab;
    private BuildingSystem buildingSystem;
    private SpriteRenderer spriteRenderer;
    private GridPlacementSystem gridSystem;
    private bool isValidPlacement = true;

public void Initialize(GameObject prefab, BuildingSystem system)
{
    buildingPrefab = prefab;
    buildingSystem = system;
    gridSystem = FindObjectOfType<GridPlacementSystem>();
    spriteRenderer = GetComponent<SpriteRenderer>();
    spriteRenderer.sprite = buildingPrefab.GetComponent<SpriteRenderer>().sprite;
    
    // Set proper scale for the preview (adjust these values based on your sprite size)
    transform.localScale = new Vector3(0.2f, 0.2f, 1f);
    
    // Set sprite properties
    spriteRenderer.drawMode = SpriteDrawMode.Simple;
    spriteRenderer.sortingOrder = 150;
    
    UpdatePreviewColor(true);
}

void Update()
{
    Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    mousePosition.z = 0;
    
    // Snap to grid
    Vector2 snappedPos = gridSystem.SnapToGrid(mousePosition);
    transform.position = snappedPos;

    // Check if position is valid
    isValidPlacement = CanPlaceBuilding();
    UpdatePreviewColor(isValidPlacement);

    // Show grid preview
    gridSystem.ShowGridPreview(snappedPos);

    // Debug log for input detection
    if (Input.GetMouseButtonDown(0))
    {
        Debug.Log("Left mouse button clicked");
    }

    if (Input.GetMouseButtonDown(0) && isValidPlacement)
    {
        Debug.Log("Attempting to place building..."); // Debug log
        PlaceBuilding();
    }
    else if (Input.GetMouseButtonDown(1))
    {
        Debug.Log("Cancelling placement..."); // Debug log
        CancelPlacement();
    }
}

    void UpdatePreviewColor(bool isValid)
    {
        Color color = isValid ? new Color(1f, 1f, 1f, 0.5f) : new Color(1f, 0f, 0f, 0.5f);
        spriteRenderer.color = color;
    }

void PlaceBuilding()
{
    if (CanPlaceBuilding())
    {
        Debug.Log("Attempting to place building at: " + transform.position); // Debug log
        
        // Create the actual building
        GameObject placedBuilding = Instantiate(buildingPrefab, transform.position, Quaternion.identity);
        
        // Set the proper scale and components
        placedBuilding.transform.localScale = new Vector3(0.2f, 0.2f, 1f);
        
        // Adjust collider
        BoxCollider2D collider = placedBuilding.GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.size = new Vector2(0.4f, 0.4f);
            collider.offset = Vector2.zero;
            collider.isTrigger = false;
        }
        
        // Set sprite properties
        SpriteRenderer renderer = placedBuilding.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.sortingOrder = 5;
        }
        
        // Notify the building system
        buildingSystem.OnBuildingPlaced();
        gridSystem.HideGridPreview();
        
        // Clean up the placer
        Destroy(gameObject);
        
        Debug.Log("Building placed successfully"); // Debug log
    }
    else
    {
        Debug.Log("Cannot place building - invalid position"); // Debug log
    }
}

bool CanPlaceBuilding()
{
    Debug.Log("Checking if can place building at: " + transform.position);
    bool canPlace = !gridSystem.IsPositionOccupied(transform.position);
    Debug.Log("Can place result: " + canPlace);
    return canPlace;
}

    void CancelPlacement()
    {
        buildingSystem.OnBuildingPlacementCancelled();
        gridSystem.HideGridPreview();
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (gridSystem != null)
        {
            gridSystem.HideGridPreview();
        }
    }
}