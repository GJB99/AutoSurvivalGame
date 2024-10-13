using UnityEngine;

public class BuildingPlacer : MonoBehaviour
{
    private GameObject buildingPrefab;
    private BuildingSystem buildingSystem;
    private SpriteRenderer spriteRenderer;

    public void Initialize(GameObject prefab, BuildingSystem system)
    {
        buildingPrefab = prefab;
        buildingSystem = system;
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = buildingPrefab.GetComponent<SpriteRenderer>().sprite;
        spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
    }

    void Update()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        transform.position = mousePosition;

        if (Input.GetMouseButtonDown(0))
        {
            PlaceBuilding();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            CancelPlacement();
        }
    }

    void PlaceBuilding()
    {
        if (CanPlaceBuilding())
        {
            Instantiate(buildingPrefab, transform.position, Quaternion.identity);
            buildingSystem.OnBuildingPlaced();
            Destroy(gameObject);
        }
    }

    bool CanPlaceBuilding()
    {
        // Implement logic to check if the building can be placed here
        // For example, check for collisions with other objects
        return true;
    }

    void CancelPlacement()
    {
        buildingSystem.OnBuildingPlacementCancelled();
        Destroy(gameObject);
    }
}