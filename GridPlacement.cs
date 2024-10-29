using UnityEngine;

public class GridPlacementSystem : MonoBehaviour
{
    public float gridSize = 0.5f;
    private Vector2 gridOffset = Vector2.zero;
    
    private GameObject gridPreview;
    public GameObject gridCellPrefab; // Assign this in inspector
    private int gridPreviewSize = 1; // Size of the preview grid (1x1)

    void Start()
    {
        CreateGridPreview();
        HideGridPreview();
    }

    private void CreateGridPreview()
    {
        gridPreview = new GameObject("GridPreview");
        gridPreview.transform.parent = transform;
        
        // Set the grid preview to be on top of other objects
        gridPreview.transform.position = new Vector3(0, 0, -1);

        float cellSize = gridSize * 0.9f; // Slightly smaller than grid size
        
        for (int x = -gridPreviewSize/2; x <= gridPreviewSize/2; x++)
        {
            for (int y = -gridPreviewSize/2; y <= gridPreviewSize/2; y++)
            {
                GameObject cell = Instantiate(gridCellPrefab, gridPreview.transform);
                cell.transform.localPosition = new Vector3(x * gridSize, y * gridSize, -1);
                cell.transform.localScale = new Vector3(cellSize, cellSize, 1f);
                
                SpriteRenderer cellRenderer = cell.GetComponent<SpriteRenderer>();
                if (cellRenderer != null)
                {
                    cellRenderer.sortingOrder = 100;
                    cellRenderer.color = new Color(1f, 1f, 1f, 0.3f);
                }
            }
        }
    }

    public Vector2 SnapToGrid(Vector2 position)
    {
        float x = Mathf.Round((position.x - gridOffset.x) / gridSize) * gridSize + gridOffset.x;
        float y = Mathf.Round((position.y - gridOffset.y) / gridSize) * gridSize + gridOffset.y;
        return new Vector2(x, y);
    }

public bool IsPositionOccupied(Vector2 position)
{
    Vector2 snappedPos = SnapToGrid(position);
    
    // Create a layerMask that only includes the layers we want to check
    // Assuming resources are on layer 8 and buildings on layer 9
    int layerMask = (1 << LayerMask.NameToLayer("Resource")) | 
                    (1 << LayerMask.NameToLayer("Building"));
    
    // Use OverlapBox instead of BoxCastAll for simpler collision check
    Collider2D[] colliders = Physics2D.OverlapBoxAll(
        snappedPos,
        new Vector2(gridSize * 0.4f, gridSize * 0.4f),
        0f,
        layerMask
    );
    
    foreach (Collider2D collider in colliders)
    {
        // Check if the collider belongs to a resource or building
        if (!collider.isTrigger && 
            (collider.CompareTag("Resource") || collider.CompareTag("Building")))
        {
            Debug.Log($"Collision detected with: {collider.gameObject.name} at {snappedPos}");
            return true;
        }
    }
    
    Debug.Log("No collisions detected - position is valid");
    return false;
}

    public void ShowGridPreview(Vector2 position)
    {
        if (gridPreview != null)
        {
            gridPreview.SetActive(true);
            gridPreview.transform.position = SnapToGrid(position);
        }
    }

    public void HideGridPreview()
    {
        if (gridPreview != null)
        {
            gridPreview.SetActive(false);
        }
    }
}