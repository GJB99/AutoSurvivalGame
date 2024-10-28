using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerInventory : MonoBehaviour
{
    private Dictionary<string, int> inventory = new Dictionary<string, int>();
    private Dictionary<string, int> foodBarItems = new Dictionary<string, int>();
    private Dictionary<string, int> itemBarItems = new Dictionary<string, int>();
    private Dictionary<string, int> mainInventory = new Dictionary<string, int>();
    private const int MAX_STACK_SIZE = 100;
    public GameObject inventoryUI;
    public GameObject buildingMenuUI;
    public TextMeshProUGUI inventoryText;
    public TextMeshProUGUI buildingMenuText;
    private bool isInventoryVisible = false;
    private bool isBuildingMenuVisible = false;
    public ItemBar itemBar;
    private UIManager uiManager;
    public GameObject inventoryItemPrefab;
    public Transform inventoryItemsContainer;
    private List<GameObject> inventoryItemObjects = new List<GameObject>();
    public FoodBar foodBar;
    public int itemBarCapacity = 5;
    public int foodBarCapacity = 3;
    public delegate void InventoryChangedHandler();
    public event InventoryChangedHandler OnInventoryChanged;

public bool IsFood(string itemName)
{
    // Add your food item names here
    string[] foodItems = { "Apple", "Herb", "Carrot", "Bread", "Wheat", "Fish" };
    return System.Array.Exists(foodItems, food => food.Equals(itemName, System.StringComparison.OrdinalIgnoreCase));
}

    public void UpdateItemBar()
    {
        itemBar.UpdateItemBar();
    }

    public void UpdateFoodBar()
    {
        foodBar.UpdateFoodBar();
    }

void Start()
{
    if (inventoryUI != null) inventoryUI.SetActive(false);
    if (buildingMenuUI != null) buildingMenuUI.SetActive(false);
    itemBar = FindObjectOfType<ItemBar>();
    foodBar = FindObjectOfType<FoodBar>();
    uiManager = FindObjectOfType<UIManager>();
    UpdateInventoryDisplay();
}

public bool MoveItem(string itemName, int amount, string fromContainer, string toContainer)
{
    if (fromContainer == toContainer) return false;

    Dictionary<string, int> sourceContainer = GetContainer(fromContainer);
    Dictionary<string, int> targetContainer = GetContainer(toContainer);

    if (sourceContainer == null || targetContainer == null)
    {
        Debug.LogError($"Invalid container: {fromContainer} or {toContainer}");
        return false;
    }

    if (!sourceContainer.ContainsKey(itemName))
    {
        return false;
    }

    // Get the full stack amount if amount parameter is 1
    int totalAmount = sourceContainer[itemName];
    if (amount == 1)
    {
        amount = totalAmount;
    }

    // Validate food bar moves
    if (toContainer == "FoodBar")
    {
        if (!IsFood(itemName))
        {
            return false;
        }
        if (foodBarItems.Count >= foodBarCapacity && !targetContainer.ContainsKey(itemName))
        {
            return false;
        }
    }

    // Validate item bar moves
    if (toContainer == "ItemBar")
    {
        if (itemBarItems.Count >= itemBarCapacity && !targetContainer.ContainsKey(itemName))
        {
            return false;
        }
    }

    // Check stack size limits
    if (targetContainer.ContainsKey(itemName))
    {
        if (targetContainer[itemName] >= MAX_STACK_SIZE)
        {
            return false;
        }
        int spaceInStack = MAX_STACK_SIZE - targetContainer[itemName];
        amount = Mathf.Min(amount, spaceInStack);
    }

    // Remove from source
    if (amount >= totalAmount)
    {
        sourceContainer.Remove(itemName);
    }
    else
    {
        sourceContainer[itemName] -= amount;
    }

    // Add to target
    if (targetContainer.ContainsKey(itemName))
    {
        targetContainer[itemName] += amount;
    }
    else
    {
        targetContainer[itemName] = amount;
    }

    return true;
}

public int GetItemCount(string itemName, string containerName)
{
    if (string.IsNullOrEmpty(itemName) || string.IsNullOrEmpty(containerName))
    {
        return 0;
    }

    Dictionary<string, int> container = GetContainer(containerName);
    if (container != null && container.TryGetValue(itemName, out int count))
    {
        return count;
    }
    return 0;
} 

    private Dictionary<string, int> GetContainer(string containerName)
    {
        switch (containerName)
        {
            case "FoodBar":
                return foodBarItems;
            case "ItemBar":
                return itemBarItems;
            case "MainInventory":
                return mainInventory;
            default:
                return null;
        }
    }

    private bool IsFoodItem(string itemName)
    {
        // Add your food item names here
        string[] foodItems = { "Apple", "Herb", "Carrot", "Bread", "Wheat", "Fish", "Herb" };
        return System.Array.Exists(foodItems, food => food.Equals(itemName, System.StringComparison.OrdinalIgnoreCase));
    }

public bool HasStonePickaxe()
{
    return GetItemCount("Stone Pickaxe", "MainInventory") > 0 || GetItemCount("Stone Pickaxe", "ItemBar") > 0;
}

public bool HasIronPickaxe()
{
    return GetItemCount("Iron Pickaxe", "MainInventory") > 0 || GetItemCount("Iron Pickaxe", "ItemBar") > 0;
}

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleBuildingMenu();
        }
    }

    public List<KeyValuePair<string, int>> GetFoodBarItems()
    {
        return foodBarItems.ToList();
    }

    public List<KeyValuePair<string, int>> GetItemBarItems()
    {
        return itemBarItems.ToList();
    }

    public List<KeyValuePair<string, int>> GetInventoryItems()
    {
        return mainInventory.ToList();
    }

public void AddItem(string itemName, int amount)
{
    // First check for existing stacks in ANY container
    if (mainInventory.ContainsKey(itemName) && mainInventory[itemName] < MAX_STACK_SIZE)
    {
        int spaceInStack = MAX_STACK_SIZE - mainInventory[itemName];
        int amountToAdd = Mathf.Min(amount, spaceInStack);
        mainInventory[itemName] += amountToAdd;
        amount -= amountToAdd;
    }
    else if (foodBarItems.ContainsKey(itemName) && foodBarItems[itemName] < MAX_STACK_SIZE)
    {
        int spaceInStack = MAX_STACK_SIZE - foodBarItems[itemName];
        int amountToAdd = Mathf.Min(amount, spaceInStack);
        foodBarItems[itemName] += amountToAdd;
        amount -= amountToAdd;
    }
    else if (itemBarItems.ContainsKey(itemName) && itemBarItems[itemName] < MAX_STACK_SIZE)
    {
        int spaceInStack = MAX_STACK_SIZE - itemBarItems[itemName];
        int amountToAdd = Mathf.Min(amount, spaceInStack);
        itemBarItems[itemName] += amountToAdd;
        amount -= amountToAdd;
    }

    // If we still have items to add
    if (amount > 0)
    {
        // For food items, try food bar first if empty slot available
        if (IsFood(itemName) && foodBarItems.Count < foodBarCapacity)
        {
            int stackAmount = Mathf.Min(amount, MAX_STACK_SIZE);
            foodBarItems[itemName] = stackAmount;
            amount -= stackAmount;
        }
        else if (!IsFood(itemName) && itemBarItems.Count < itemBarCapacity)
        {
            int stackAmount = Mathf.Min(amount, MAX_STACK_SIZE);
            itemBarItems[itemName] = stackAmount;
            amount -= stackAmount;
        }
        
        // Any remaining amount goes to main inventory
        if (amount > 0)
        {
            AddToMainInventory(itemName, amount);
        }
    }

    OnInventoryChanged?.Invoke();
    UpdateInventoryDisplay();
    UpdateItemBar();
    UpdateFoodBar();
}

public Sprite LoadItemSprite(string itemKey)
{
    // Strip any stack suffix (e.g., "Wood_1" becomes "Wood")
    string baseItemName = itemKey.Split('_')[0];
    
    // Try loading the sprite with different formats
    Sprite itemSprite = Resources.Load<Sprite>($"Images/{baseItemName}");
    if (itemSprite == null)
    {
        itemSprite = Resources.Load<Sprite>($"Images/{baseItemName.Replace(" ", "")}");
        if (itemSprite == null)
        {
            itemSprite = Resources.Load<Sprite>($"Images/{baseItemName.Replace(" ", "_")}");
        }
    }
    return itemSprite;
}

private bool TryAddToExistingStack(string itemName, int amount)
{
    // For food items, prioritize food bar
    if (IsFood(itemName))
    {
        if (foodBarItems.ContainsKey(itemName) && foodBarItems[itemName] < MAX_STACK_SIZE)
        {
            int spaceInStack = MAX_STACK_SIZE - foodBarItems[itemName];
            int amountToAdd = Mathf.Min(amount, spaceInStack);
            foodBarItems[itemName] += amountToAdd;
            
            if (amountToAdd < amount)
            {
                AddItem(itemName, amount - amountToAdd); // Recursively handle remaining amount
            }
            return true;
        }
        else if (foodBarItems.Count < foodBarCapacity)
        {
            AddItem(itemName, amount); // This will handle adding to food bar
            return true;
        }
    }

    // Then try item bar
    if (itemBarItems.ContainsKey(itemName) && itemBarItems[itemName] < MAX_STACK_SIZE)
    {
        int spaceInStack = MAX_STACK_SIZE - itemBarItems[itemName];
        int amountToAdd = Mathf.Min(amount, spaceInStack);
        itemBarItems[itemName] += amountToAdd;
        
        if (amountToAdd < amount)
        {
            AddItem(itemName, amount - amountToAdd); // Recursively handle remaining amount
        }
        return true;
    }
    else if (itemBarItems.Count < itemBarCapacity)
    {
        AddItem(itemName, amount); // This will handle adding to item bar
        return true;
    }

    return false;
}

    private void AddToFoodBar(string itemName, int quantity)
    {
        if (foodBarItems.ContainsKey(itemName))
        {
            foodBarItems[itemName] += quantity;
        }
        else
        {
            foodBarItems[itemName] = quantity;
        }
    }

    private void AddToItemBar(string itemName, int quantity)
    {
        if (itemBarItems.ContainsKey(itemName))
        {
            itemBarItems[itemName] += quantity;
        }
        else
        {
            itemBarItems[itemName] = quantity;
        }
    }

private void AddToMainInventory(string itemName, int quantity)
{
    while (quantity > 0)
    {
        // First try to fill existing stacks
        bool addedToExisting = false;
        foreach (var pair in mainInventory.ToList())
        {
            string baseItemName = pair.Key.Split('_')[0];
            if (baseItemName == itemName && pair.Value < MAX_STACK_SIZE)
            {
                int spaceInStack = MAX_STACK_SIZE - pair.Value;
                int amountToAdd = Mathf.Min(quantity, spaceInStack);
                mainInventory[pair.Key] += amountToAdd;
                quantity -= amountToAdd;
                addedToExisting = true;
                
                if (quantity <= 0) break;
            }
        }

        // If we couldn't add to existing stacks or still have remaining quantity
        if (!addedToExisting || quantity > 0)
        {
            // Create new stack with unique key
            int stackAmount = Mathf.Min(quantity, MAX_STACK_SIZE);
            string stackKey = itemName;
            int suffix = 1;
            
            // Find an available key
            while (mainInventory.ContainsKey(stackKey))
            {
                stackKey = $"{itemName}_{suffix}";
                suffix++;
            }
            
            mainInventory.Add(stackKey, stackAmount);
            quantity -= stackAmount;
        }
    }

    UpdateInventoryDisplay();
    itemBar.UpdateItemBar();
    foodBar.UpdateFoodBar();
    OnInventoryChanged?.Invoke();
}

public void UpdateInventoryDisplay()
{
    Debug.Log("=== UpdateInventoryDisplay Start ===");
    Debug.Log("Main Inventory Contents:");
    foreach (var item in mainInventory)
    {
        Debug.Log($"{item.Key}: {item.Value}");
    }

    if (inventoryItemsContainer == null)
    {
        Debug.LogError("inventoryItemsContainer is null");
        return;
    }

    Debug.Log($"inventoryUI active: {inventoryUI.activeSelf}");
    Debug.Log($"inventoryItemsContainer active: {inventoryItemsContainer.gameObject.activeSelf}");

    // Clear existing inventory items
    foreach (GameObject item in inventoryItemObjects)
    {
        Destroy(item);
    }
    inventoryItemObjects.Clear();

    // Set up GridLayoutGroup
    GridLayoutGroup gridLayout = inventoryItemsContainer.GetComponent<GridLayoutGroup>();
    if (gridLayout == null)
    {
        gridLayout = inventoryItemsContainer.gameObject.AddComponent<GridLayoutGroup>();
    }

    int columns = 8;
    int rows = 3;
    float spacing = 10f;
    float slotSize = 75f;

    gridLayout.cellSize = new Vector2(slotSize, slotSize);
    gridLayout.spacing = new Vector2(spacing, spacing);
    gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
    gridLayout.constraintCount = columns;

    List<KeyValuePair<string, int>> inventoryItems = mainInventory.ToList();

    for (int i = 0; i < rows * columns; i++)
    {
        GameObject newItem = Instantiate(inventoryItemPrefab, inventoryItemsContainer);
        
        // Add EventTrigger component to the slot
        EventTrigger trigger = newItem.AddComponent<EventTrigger>();
        trigger.triggers = new List<EventTrigger.Entry>();

        // Begin Drag
        EventTrigger.Entry beginDragEntry = new EventTrigger.Entry();
        beginDragEntry.eventID = EventTriggerType.BeginDrag;
        beginDragEntry.callback.AddListener((data) => { uiManager.OnBeginDrag((PointerEventData)data); });
        trigger.triggers.Add(beginDragEntry);

        // Drag
        EventTrigger.Entry dragEntry = new EventTrigger.Entry();
        dragEntry.eventID = EventTriggerType.Drag;
        dragEntry.callback.AddListener((data) => { uiManager.OnDrag((PointerEventData)data); });
        trigger.triggers.Add(dragEntry);

        // End Drag
        EventTrigger.Entry endDragEntry = new EventTrigger.Entry();
        endDragEntry.eventID = EventTriggerType.EndDrag;
        dragEntry.callback.AddListener((data) => { uiManager.OnDragEnd((PointerEventData)data); });
        trigger.triggers.Add(endDragEntry);

        Image itemImage = newItem.transform.Find("ItemIcon").GetComponent<Image>();
        TextMeshProUGUI quantityText = newItem.GetComponentInChildren<TextMeshProUGUI>();

        if (i < inventoryItems.Count)
        {
            var item = inventoryItems[i];
            Sprite itemSprite = LoadItemSprite(item.Key);

            if (itemSprite != null)
            {
                itemImage.sprite = itemSprite;
                itemImage.color = Color.white;
            }
            else
            {
                Debug.LogWarning($"Failed to load sprite: {item.Key}");
                itemImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            }

            quantityText.text = item.Value.ToString();
            Debug.Log($"Added item to inventory display: {item.Key} x{item.Value}");
        }
        else
        {
            itemImage.sprite = null;
            itemImage.color = new Color(0.5f, 0.5f, 0.5f, 0.1f);
            quantityText.text = "";
        }

        inventoryItemObjects.Add(newItem);
    }

    Debug.Log($"Inventory display updated. Total slots: {inventoryItemObjects.Count}");
    LogInventoryContents();
    Debug.Log("=== UpdateInventoryDisplay End ===");
}

    public void AddItemToMainInventoryForTesting(string itemName, int quantity)
    {
        AddToMainInventory(itemName, quantity);
        Debug.Log($"Added {quantity} {itemName} to main inventory for testing.");
        UpdateInventoryDisplay();
    }

    public bool IsBuildingMenuVisible()
    {
        return isBuildingMenuVisible;
    }

    private void ToggleInventory()
    {
        isInventoryVisible = !isInventoryVisible;
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(isInventoryVisible);
            if (isInventoryVisible)
            {
                // Position the inventory panel
                RectTransform rectTransform = inventoryUI.GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = Vector2.zero;
                
                UpdateInventoryDisplay();
            }
        }
    }

    private void ToggleBuildingMenu()
    {
        isBuildingMenuVisible = !isBuildingMenuVisible;
        if (buildingMenuUI != null) buildingMenuUI.SetActive(isBuildingMenuVisible);
        if (isBuildingMenuVisible) UpdateBuildingMenuDisplay();
    }

    private void UpdateBuildingMenuDisplay()
    {
        if (buildingMenuText != null)
        {
            string displayText = "Building Menu:\n";
            displayText += "Click on items to view details and build";
            buildingMenuText.text = displayText;
        }
    }

    public bool CanBuild(string itemName, int cost)
    {
        int totalItemCount = GetItemCount(itemName, "MainInventory") + 
                            GetItemCount(itemName, "ItemBar") + 
                            GetItemCount(itemName, "FoodBar");
        return totalItemCount >= cost;
    }

    public void RemoveItems(string itemName, int amount)
    {
        int amountToRemove = amount;

        // Remove from FoodBar first
        if (foodBarItems.ContainsKey(itemName))
        {
            int foodBarAmount = foodBarItems[itemName];
            int removeAmount = Mathf.Min(amountToRemove, foodBarAmount);
            foodBarItems[itemName] -= removeAmount;
            if (foodBarItems[itemName] <= 0)
            {
                foodBarItems.Remove(itemName);
            }
            amountToRemove -= removeAmount;
        }

        // Then remove from ItemBar
        if (amountToRemove > 0 && itemBarItems.ContainsKey(itemName))
        {
            int itemBarAmount = itemBarItems[itemName];
            int removeAmount = Mathf.Min(amountToRemove, itemBarAmount);
            itemBarItems[itemName] -= removeAmount;
            if (itemBarItems[itemName] <= 0)
            {
                itemBarItems.Remove(itemName);
            }
            amountToRemove -= removeAmount;
        }

        // Finally, remove from Main Inventory
        if (amountToRemove > 0 && mainInventory.ContainsKey(itemName))
        {
            mainInventory[itemName] -= amountToRemove;
            if (mainInventory[itemName] <= 0)
            {
                mainInventory.Remove(itemName);
            }
        }

        UpdateInventoryDisplay();
        itemBar.UpdateItemBar();
        foodBar.UpdateFoodBar();
        OnInventoryChanged?.Invoke();
    }

    private void LogInventoryContents()
    {
        Debug.Log("=== Inventory Contents ===");
        foreach (var item in mainInventory)
        {
            Debug.Log($"{item.Key}: {item.Value}");
        }
        Debug.Log("==========================");
    }

    public int GetTotalItemCount(string itemName)
    {
        return GetItemCount(itemName, "MainInventory") +
               GetItemCount(itemName, "ItemBar") +
               GetItemCount(itemName, "FoodBar");
    }

}