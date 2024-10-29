using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class BuildingSystem : MonoBehaviour
{
    public GameObject chestPrefab;
    public GameObject smelterPrefab;
    public GameObject drillPrefab;
    public GameObject conveyorBeltPrefab;
    public GameObject cookingStationPrefab;
    public GameObject processorPrefab;

    private PlayerInventory playerInventory;
    public TextMeshProUGUI messageText;
    public float messageDisplayTime = 2f;
    public float fabricationTime = 1f; // Time to fabricate an item

    [Header("UI Elements")]
    public GameObject buildingMenuPanel;
    public Button gearWeaponsButton;
    public Button factoriesAutomationButton;
    public Button componentsButton;
    public Button transportButton;
    public Button baseStationsButton;
    public Button foodButton;

    public GameObject gearPanel;
    public GameObject foodPanel;
    public GameObject componentsPanel;
    public GameObject basePanel;
    public GameObject autoPanel;

    public GameObject itemPrefab; // Prefab for displaying items in the UI
    public float itemSizeCoefficient = 1f; // Adjust this in the Unity Inspector to change item sizes
    
    public GameObject itemDetailsPanel;
    public TextMeshProUGUI itemDetailsText;
    public Button buildButton;

    public GameObject buildingPlacerPrefab;

    private BuildingPlacer activeBuildingPlacer;

    private string lastSelectedItemName;
    private int lastSelectedCost1, lastSelectedCost2, lastSelectedCost3;
    private string lastSelectedResource1, lastSelectedResource2, lastSelectedResource3;
    private bool lastSelectedRequiresStation;
    private string lastSelectedStationName;

    private UIManager uiManager;
    private bool hasShownBuildingMenuMessage = false;
    

    private void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
        playerInventory = GetComponent<PlayerInventory>();
        playerInventory.OnInventoryChanged += UpdateUIOnInventoryChange;
        SetupButtonListeners();
        PopulateBuildingMenu();
    }

    private void OnDestroy()
    {
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged -= UpdateUIOnInventoryChange;
        }
    }

public void InitiateBuildingPlacement(string buildingName)
{
    GameObject buildingPrefab = GetBuildingPrefab(buildingName);
    if (buildingPrefab != null)
    {
        if (activeBuildingPlacer != null)
        {
            Destroy(activeBuildingPlacer.gameObject);
        }
        GameObject placerObject = Instantiate(buildingPlacerPrefab);
        activeBuildingPlacer = placerObject.GetComponent<BuildingPlacer>();
        activeBuildingPlacer.Initialize(buildingPrefab, this);
    }
}

private GameObject GetBuildingPrefab(string buildingName)
{
    switch (buildingName)
    {
        case "Smelter":
            return smelterPrefab;
        case "Cooking Station":
            return cookingStationPrefab;
        case "Processor":
            return processorPrefab;
        case "Drill":
            return drillPrefab;
        case "Conveyor":
            return conveyorBeltPrefab;
        default:
            Debug.LogWarning($"Unknown building type: {buildingName}");
            return null;
    }
}

public void OnBuildingPlaced()
{
    Debug.Log("Building system notified of placement");
    
    // Remove one instance of the building item from inventory
    if (!string.IsNullOrEmpty(lastSelectedItemName))
    {
        playerInventory.RemoveItems(lastSelectedItemName, 1);
        Debug.Log($"Removed 1 {lastSelectedItemName} from inventory");
    }
    else
    {
        Debug.LogWarning("No item name stored for removal from inventory");
    }
    
    activeBuildingPlacer = null;
}

public void OnBuildingPlacementCancelled()
{
    // Handle cancelled building placement
    activeBuildingPlacer = null;
}

private void UpdateUIOnInventoryChange()
{
    if (buildingMenuPanel.activeSelf)
    {
        RefreshAllItemDetails();
        if (!string.IsNullOrEmpty(lastSelectedItemName))
        {
            ShowItemDetails(lastSelectedItemName, lastSelectedCost1, lastSelectedResource1, 
                            lastSelectedCost2, lastSelectedResource2, lastSelectedCost3, 
                            lastSelectedResource3, lastSelectedRequiresStation, lastSelectedStationName);
        }
    }
}

public void PlaceBuilding(string buildingName, Vector2 position)
{
    GameObject buildingPrefab = GetBuildingPrefab(buildingName);
    if (buildingPrefab != null)
    {
        GameObject building = Instantiate(buildingPrefab, position, Quaternion.identity);
        
        // Set proper scale
        building.transform.localScale = new Vector3(0.2f, 0.2f, 1f);
        
        // Adjust collider
        BoxCollider2D collider = building.GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.size = new Vector2(0.4f, 0.4f);
            collider.offset = Vector2.zero;
            collider.isTrigger = false;
        }
        
        // Set sprite properties
        SpriteRenderer renderer = building.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.sortingOrder = 5;
        }

        // Add BuildingPickup component
        building.AddComponent<BuildingPickup>();
    }
}

    private void SetupButtonListeners()
    {
        if (gearWeaponsButton != null)
            gearWeaponsButton.onClick.AddListener(() => ShowBuildingCategory("Gear"));
        if (factoriesAutomationButton != null)
            factoriesAutomationButton.onClick.AddListener(() => ShowBuildingCategory("Auto"));
        if (componentsButton != null)
            componentsButton.onClick.AddListener(() => ShowBuildingCategory("Comps"));
        if (transportButton != null)
            transportButton.onClick.AddListener(() => ShowBuildingCategory("Transport"));
        if (baseStationsButton != null)
            baseStationsButton.onClick.AddListener(() => ShowBuildingCategory("Base"));
        if (foodButton != null)
            foodButton.onClick.AddListener(() => ShowBuildingCategory("Food"));
    }

    private void ShowBuildingCategory(string category)
    {
        // Deactivate all panels first
        gearPanel.SetActive(false);
        foodPanel.SetActive(false);
        componentsPanel.SetActive(false);
        basePanel.SetActive(false);
        autoPanel.SetActive(false);

        // Activate the selected panel
        GameObject activePanel = GetActivePanelForCategory(category);
        if (activePanel != null)
        {
            activePanel.SetActive(true);
            RectTransform contentRect = EnsureScrollRect(activePanel);
            if (contentRect.childCount == 0)
            {
                switch (category)
                {
                    case "Gear":
                        PopulateGearPanel();
                        break;
                    case "Food":
                        PopulateFoodPanel();
                        break;
                    case "Comps":
                        PopulateComponentsPanel();
                        break;
                    case "Base":
                        PopulateBasePanel();
                        break;
                    case "Auto":
                        PopulateAutoPanel();
                        break;
                }
            }
            RefreshCategoryItems(activePanel);
        }
        
        Debug.Log($"Showing category: {category}");
    }

private void RefreshAllItemDetails()
{
    if (buildingMenuPanel == null) return;

    foreach (Transform child in buildingMenuPanel.transform)
    {
        Button itemButton = child.GetComponent<Button>();
        if (itemButton == null) continue;

        TextMeshProUGUI buildInfo = itemButton.transform.Find("BuildInfo")?.GetComponent<TextMeshProUGUI>();
        if (buildInfo == null) continue;

        string itemName = buildInfo.text;
        int cost1 = 0, cost2 = 0, cost3 = 0;
        string resource1 = null, resource2 = null, resource3 = null;
        bool requiresStation = false;
        string stationName = null;

        ParseCost(itemButton.transform.Find("ItemCost1"), out cost1, out resource1);
        ParseCost(itemButton.transform.Find("ItemCost2"), out cost2, out resource2);
        ParseCost(itemButton.transform.Find("ItemCost3"), out cost3, out resource3);

        TextMeshProUGUI stationRequirement = itemButton.transform.Find("StationRequirement")?.GetComponent<TextMeshProUGUI>();
        if (stationRequirement != null && stationRequirement.gameObject.activeSelf)
        {
            requiresStation = true;
            stationName = stationRequirement.text.Replace("Requires: ", "");
        }

        ShowItemDetails(itemName, cost1, resource1, cost2, resource2, cost3, resource3, requiresStation, stationName);
    }
}

    private void RefreshCategoryItems(GameObject panel)
    {
        if (panel.activeSelf)
        {
            foreach (Transform child in panel.GetComponent<ScrollRect>().content)
            {
                Button itemButton = child.GetComponent<Button>();
                if (itemButton != null)
                {
                    UpdateItemDetails(itemButton);
                }
            }
        }
    }

    private void UpdateItemDetails(Button itemButton)
    {
        // Retrieve the item details from the button
        string itemName = itemButton.transform.Find("BuildInfo").GetComponent<TextMeshProUGUI>().text;
        int cost1 = 0, cost2 = 0, cost3 = 0;
        string resource1 = null, resource2 = null, resource3 = null;
        bool requiresStation = false;
        string stationName = null;

        // Parse the costs from the button's child objects
        ParseCost(itemButton.transform.Find("ItemCost1"), out cost1, out resource1);
        ParseCost(itemButton.transform.Find("ItemCost2"), out cost2, out resource2);
        ParseCost(itemButton.transform.Find("ItemCost3"), out cost3, out resource3);

        // Check if the item requires a station
        TextMeshProUGUI stationRequirement = itemButton.transform.Find("StationRequirement").GetComponent<TextMeshProUGUI>();
        if (stationRequirement.gameObject.activeSelf)
        {
            requiresStation = true;
            stationName = stationRequirement.text.Replace("Requires: ", "");
        }

        // Update the item details if it's the currently selected item
        if (itemName == lastSelectedItemName)
        {
            ShowItemDetails(itemName, cost1, resource1, cost2, resource2, cost3, resource3, requiresStation, stationName);
        }
    }

private void ParseCost(Transform costTransform, out int cost, out string resource)
{
    cost = 0;
    resource = null;
    if (costTransform == null) return;

    TextMeshProUGUI costText = costTransform.GetComponent<TextMeshProUGUI>();
    if (costText == null || string.IsNullOrEmpty(costText.text)) return;

    string[] parts = costText.text.Split(' ');
    if (parts.Length >= 2 && int.TryParse(parts[0], out cost))
    {
        resource = string.Join(" ", parts, 1, parts.Length - 1);
    }
}

    private GameObject GetActivePanelForCategory(string category)
    {
        switch (category)
        {
            case "Gear": return gearPanel;
            case "Food": return foodPanel;
            case "Comps": return componentsPanel;
            case "Base": return basePanel;
            case "Auto": return autoPanel;
            default: return null;
        }
    }

    private void PopulateBuildingMenu()
    {
        PopulateGearPanel();
        // Initially show only the Gear panel
        ShowBuildingCategory("Gear");
    }

    private void PopulateGearPanel()
    {
        AddItemToPanel(gearPanel, "Stone Pickaxe", "Stone Pickaxe", 5, "Rock", 5, "Wood");
        AddItemToPanel(gearPanel, "Iron Pickaxe", "Iron Pickaxe", 1, "Stone Pickaxe", 10, "Iron");
        AddItemToPanel(gearPanel, "Wood Helmet", "Wood Helmet", 10, "Wood", 3, "String");
        AddItemToPanel(gearPanel, "Bow", "Bow", 10, "Wood", 2, "String");
    }

    private void PopulateFoodPanel()
    {
        AddItemToPanel(foodPanel, "Roasted Carrots with Herbs", "Roasted_Carrots", 1, "Carrot", 1, "Herb", true, "Cooking Station");
    }

    private void PopulateComponentsPanel()
    {
        AddItemToPanel(componentsPanel, "String", "String", 1, "PlantFiber");
        AddItemToPanel(componentsPanel, "Iron Ingot", "Iron Ingot", 1, "Iron", 0, null, true, "Smelter");
        AddItemToPanel(componentsPanel, "Copper Ingot", "Copper Ingot", 1, "Copper", 0, null, true, "Smelter");
    }

    private void PopulateBasePanel()
    {
        AddItemToPanel(basePanel, "Smelter", "Smelter", 5, "Copper", 5, "Iron", false, null, 10, "Rock");
        AddItemToPanel(basePanel, "Processor", "Processor", 10, "Iron Ingot", 10, "Copper Ingot");
        AddItemToPanel(basePanel, "Cooking Station", "Cooking Station", 10, "Wood", 5, "Iron");
    }

    private void PopulateAutoPanel()
    {
        AddItemToPanel(autoPanel, "Drill", "Drill", 10, "Iron Ingot", 5, "Copper Ingot");
        AddItemToPanel(autoPanel, "Conveyor", "Conveyor", 5, "Iron Ingot", 5, "Copper Ingot");
    }

    private void AddItemToPanel(GameObject panel, string itemName, string imageName, int cost1, string resource1, int cost2 = 0, string resource2 = null, bool requiresStation = false, string stationName = null, int cost3 = 0, string resource3 = null)
    {
        RectTransform contentRect = EnsureScrollRect(panel);

        GameObject item = Instantiate(itemPrefab, contentRect);
        RectTransform rectTransform = item.GetComponent<RectTransform>();

        // Set the item's size
        float itemSize = 100f * itemSizeCoefficient; // Base size of 100 units
        rectTransform.sizeDelta = new Vector2(itemSize, itemSize);

        // Calculate the position
        int itemCount = contentRect.childCount - 1;
        int row = itemCount / 5;
        int col = itemCount % 5;
        float xPosition = col * (itemSize + 10); // 10 units of padding
        float yPosition = -row * (itemSize + 10);

        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.anchoredPosition = new Vector2(xPosition, yPosition);

        // Adjust content size
        float contentWidth = 5 * (itemSize + 10) - 10; // 5 items per row, minus last padding
        float contentHeight = (row + 1) * (itemSize + 10);
        contentRect.sizeDelta = new Vector2(contentWidth, contentHeight);

        // Set up item components
        item.transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = itemName;

        Sprite sprite = Resources.Load<Sprite>($"Images/{imageName}");
        if (sprite == null)
        {
            Debug.LogError($"Failed to load sprite: {imageName}. Make sure the file exists in Assets/Resources/Images/ and has the correct name.");
        }
        else
        {
            Image itemImage = item.transform.Find("ItemImage").GetComponent<Image>();
            itemImage.sprite = sprite;
            itemImage.preserveAspect = true;
        }

        SetCostText(item, "ItemCost1", cost1, resource1);
        SetCostText(item, "ItemCost2", cost2, resource2);
        SetCostText(item, "ItemCost3", cost3, resource3);

        if (requiresStation && stationName != null)
        {
            item.transform.Find("StationRequirement").GetComponent<TextMeshProUGUI>().text = $"Requires: {stationName}";
        }
        else
        {
            item.transform.Find("StationRequirement").gameObject.SetActive(false);
        }

        string buildInfo = $"{itemName}";
        item.transform.Find("BuildInfo").GetComponent<TextMeshProUGUI>().text = buildInfo;
    
        // Add click listener to the item
        Button itemButton = item.GetComponent<Button>();
        if (itemButton == null)
        {
            itemButton = item.AddComponent<Button>();
        }
        itemButton.onClick.AddListener(() => ShowItemDetails(itemName, cost1, resource1, cost2, resource2, cost3, resource3, requiresStation, stationName));
    }

private void ShowItemDetails(string itemName, int cost1, string resource1, int cost2, string resource2, int cost3, string resource3, bool requiresStation, string stationName)
{
    if (itemDetailsPanel != null && itemDetailsText != null)
    {
        itemDetailsPanel.SetActive(true);
        string details = $"<b>{itemName}</b>\n";
        
        List<string> requirements = new List<string>();
        
        requirements.Add(FormatRequirement(cost1, resource1));
        if (cost2 > 0 && !string.IsNullOrEmpty(resource2))
            requirements.Add(FormatRequirement(cost2, resource2));
        if (cost3 > 0 && !string.IsNullOrEmpty(resource3))
            requirements.Add(FormatRequirement(cost3, resource3));
        
        details += string.Join(", ", requirements);
        
        if (requiresStation)
            details += $"\n<color=#FFD700>Requires: {stationName}</color>";

        itemDetailsText.text = details;

        bool canBuild = CanBuildItem(itemName, cost1, resource1, cost2, resource2, cost3, resource3);
        buildButton.interactable = canBuild;
        buildButton.onClick.RemoveAllListeners();
        buildButton.onClick.AddListener(() => BuildItem(itemName, cost1, resource1, cost2, resource2, cost3, resource3));

        // Update last selected item details
        lastSelectedItemName = itemName;
        lastSelectedCost1 = cost1;
        lastSelectedResource1 = resource1;
        lastSelectedCost2 = cost2;
        lastSelectedResource2 = resource2;
        lastSelectedCost3 = cost3;
        lastSelectedResource3 = resource3;
        lastSelectedRequiresStation = requiresStation;
        lastSelectedStationName = stationName;
    }
}

private string FormatRequirement(int cost, string resource)
{
    if (string.IsNullOrEmpty(resource) || cost <= 0)
    {
        return string.Empty;
    }

    bool hasEnough = playerInventory.GetTotalItemCount(resource) >= cost;
    string color = hasEnough ? "white" : "red";
    return $"<color={color}>{cost} {resource}</color>";
}

private bool CanBuildItem(string itemName, int cost1, string resource1, int cost2, string resource2, int cost3, string resource3)
{
    bool canBuild = playerInventory.GetTotalItemCount(resource1) >= cost1;
    if (cost2 > 0 && !string.IsNullOrEmpty(resource2))
        canBuild &= playerInventory.GetTotalItemCount(resource2) >= cost2;
    if (cost3 > 0 && !string.IsNullOrEmpty(resource3))
        canBuild &= playerInventory.GetTotalItemCount(resource3) >= cost3;
    return canBuild;
}

private void BuildItem(string itemName, int cost1, string resource1, int cost2, string resource2, int cost3, string resource3)
{
    if (CanBuildItem(itemName, cost1, resource1, cost2, resource2, cost3, resource3))
    {
        playerInventory.RemoveItems(resource1, cost1);
        if (cost2 > 0 && resource2 != null)
            playerInventory.RemoveItems(resource2, cost2);
        if (cost3 > 0 && resource3 != null)
            playerInventory.RemoveItems(resource3, cost3);

        playerInventory.AddItem(itemName, 1);
        // Replace direct message with resource gain message
        uiManager.ShowResourceGainMessage(itemName, 1);
    }
    else
    {
        uiManager.ShowUpperMessage("Not enough resources to build this item");
    }
}

    private RectTransform EnsureScrollRect(GameObject panel)
    {
        ScrollRect scrollRect = panel.GetComponent<ScrollRect>();
        if (scrollRect == null)
        {
            scrollRect = panel.AddComponent<ScrollRect>();
            scrollRect.vertical = true;
            scrollRect.horizontal = false;

            // Create content object if it doesn't exist
            if (scrollRect.content == null)
            {
                GameObject content = new GameObject("Content");
                RectTransform contentRectTransform = content.AddComponent<RectTransform>();
                content.transform.SetParent(panel.transform, false);
                scrollRect.content = contentRectTransform;

                contentRectTransform.anchorMin = new Vector2(0, 1);
                contentRectTransform.anchorMax = new Vector2(1, 1);
                contentRectTransform.sizeDelta = new Vector2(0, 0);
                contentRectTransform.anchoredPosition = Vector2.zero;
            }

            // Add a mask to the panel
            Image mask = panel.GetComponent<Image>();
            if (mask == null)
            {
                mask = panel.AddComponent<Image>();
                mask.color = Color.clear; // Set to transparent
            }
            Mask panelMask = panel.GetComponent<Mask>();
            if (panelMask == null)
            {
                panelMask = panel.AddComponent<Mask>();
                panelMask.showMaskGraphic = false;
            }
        }
        return scrollRect.content;
    }

    private void SetCostText(GameObject item, string costObjectName, int cost, string resource)
    {
        if (cost > 0 && resource != null)
        {
            item.transform.Find(costObjectName).GetComponent<TextMeshProUGUI>().text = $"{cost} {resource}";
        }
        else
        {
            item.transform.Find(costObjectName).gameObject.SetActive(false);
        }
    }

public void ToggleBuildingMenu()
{
    if (buildingMenuPanel != null)
    {
        bool isOpening = !buildingMenuPanel.activeSelf;
        buildingMenuPanel.SetActive(isOpening);
        
        if (isOpening && !hasShownBuildingMenuMessage)
        {
            uiManager.ShowUpperMessage("Building Menu:\nClick on items to view details and build");
            hasShownBuildingMenuMessage = true;
        }
        
        RefreshAllItemDetails();
        if (!string.IsNullOrEmpty(lastSelectedItemName))
        {
            ShowItemDetails(lastSelectedItemName, lastSelectedCost1, lastSelectedResource1, 
                            lastSelectedCost2, lastSelectedResource2, lastSelectedCost3, 
                            lastSelectedResource3, lastSelectedRequiresStation, lastSelectedStationName);
        }
    }
}

    private void OpenBuildingCategory(string category)
    {
        ShowBuildingCategory(category);
        Debug.Log($"Opening building category: {category}");
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
}