using UnityEngine;
using System.Collections;

public class BuildingPickup : MonoBehaviour
{
    private float holdTime = 1f;
    private float holdTimer = 0f;
    private bool isHolding = false;
    private PlayerInventory playerInventory;
    private UIManager uiManager;

    void Start()
    {
        playerInventory = FindObjectOfType<PlayerInventory>();
        uiManager = FindObjectOfType<UIManager>();
    }

void Update()
{
    Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    Collider2D hitCollider = Physics2D.OverlapPoint(mousePosition);

    // Add debug logging
    if (hitCollider != null)
    {
        Debug.Log($"Hit object: {hitCollider.gameObject.name}, Tag: {hitCollider.gameObject.tag}");
    }

    if (hitCollider != null && hitCollider.gameObject == gameObject && gameObject.CompareTag("UsableBuilding"))
    {
        if (Input.GetMouseButton(1)) // Right mouse button
        {
            Debug.Log("Right mouse button held on building"); // Debug log
            if (!isHolding)
            {
                isHolding = true;
                holdTimer = 0f;
                StartCoroutine(ShowPickupProgress());
            }
            
            holdTimer += Time.deltaTime;
            Debug.Log($"Hold timer: {holdTimer}"); // Debug timer
            
            if (holdTimer >= holdTime)
            {
                PickupBuilding();
            }
        }
        else
        {
            isHolding = false;
            holdTimer = 0f;
        }
    }
}

    private IEnumerator ShowPickupProgress()
    {
        while (isHolding && holdTimer < holdTime)
        {
            // Optional: Show pickup progress
            yield return null;
        }
    }

    private void PickupBuilding()
    {
        string buildingType = GetBuildingType();
        if (!string.IsNullOrEmpty(buildingType))
        {
            playerInventory.AddItem(buildingType, 1);
            uiManager.ShowResourceGainMessage(buildingType, 1);
            Destroy(gameObject);
        }
    }

private string GetBuildingType()
{
    string name = gameObject.name.Replace("(Clone)", "").Trim();
    
    // Match the names used in BuildingSystem.GetBuildingPrefab
    switch (name)
    {
        case "Smelter":
        case "Chest":
        case "Cooking Station":
        case "Processor":
        case "Drill":
        case "Oven":
        case "Conveyor":
            return name;
        default:
            Debug.LogWarning($"Unknown building type: {name}");
            return name;
    }
}
}