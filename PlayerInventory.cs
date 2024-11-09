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
    // Strip any modifiers from the item name
    string baseItemName = itemName.Split('_')[0];
    
    // Add your food item names here
    string[] foodItems = { 
        "Apple", "Salt", "Sugar", "Potato", "Sugar Cane", "Herb", "Carrot", "Bread", "Wheat", "Fish",
        "Herby Carrots" // Add processed food items
    };
    return System.Array.Exists(foodItems, food => 
        food.Equals(baseItemName, System.StringComparison.OrdinalIgnoreCase));
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
    if (container == null) return 0;

    // Sum up all stacks that match the base item name
    return container.Where(x => x.Key.Split('_')[0] == itemName)
                   .Sum(x => x.Value);
} 

private Dictionary<string, int> GetContainer(string containerName)
{
    switch (containerName)
    {
        case "MainInventory":
            return mainInventory;
        case "ItemBar":
            return itemBarItems;
        case "FoodBar":
            return foodBarItems;
        case "Character":
            // Character panel doesn't need a dictionary since items are stored in gear slots
            return new Dictionary<string, int>();
        default:
            Debug.LogError($"Unknown container: {containerName}");
            return null;
    }
}

    private bool IsFoodItem(string itemName)
    {
        string[] foodItems = { "Apple", "Herb", "Carrot", "Herby Carrots", "Bread", "Wheat", "Fish", "Herb" };
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
    // First try to add to existing stacks in their current containers
    bool addedToExisting = false;

    // Check if item exists in any container
    bool existsInMainInventory = mainInventory.Any(x => x.Key.Split('_')[0] == itemName);
    bool existsInItemBar = itemBarItems.Any(x => x.Key.Split('_')[0] == itemName);
    bool existsInFoodBar = foodBarItems.Any(x => x.Key.Split('_')[0] == itemName);

    // Try to add to existing stacks first
    if (existsInMainInventory)
    {
        addedToExisting = TryAddToExistingStacksInContainer(mainInventory, itemName, ref amount);
    }
    else if (existsInItemBar)
    {
        addedToExisting = TryAddToExistingStacksInContainer(itemBarItems, itemName, ref amount);
    }
    else if (existsInFoodBar)
    {
        addedToExisting = TryAddToExistingStacksInContainer(foodBarItems, itemName, ref amount);
    }

    // If we still have items to add
    if (amount > 0)
    {
        if (existsInMainInventory)
        {
            AddToMainInventory(itemName, amount);
        }
        else if (IsFood(itemName) && foodBarItems.Count < foodBarCapacity)
        {
            AddToFoodBarWithStacks(itemName, amount);
        }
        else if (!IsFood(itemName) && itemBarItems.Count < itemBarCapacity)
        {
            AddToItemBarWithStacks(itemName, amount);
        }
        else
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

private bool TryAddToExistingStacksInContainer(Dictionary<string, int> container, string itemName, ref int amount)
{
    bool added = false;
    foreach (var pair in container.ToList())
    {
        if (pair.Key.Split('_')[0] == itemName && pair.Value < MAX_STACK_SIZE)
        {
            int spaceInStack = MAX_STACK_SIZE - pair.Value;
            int amountToAdd = Mathf.Min(amount, spaceInStack);
            container[pair.Key] += amountToAdd;
            amount -= amountToAdd;
            added = true;
            if (amount <= 0) break;
        }
    }
    return added;
}

private void AddToItemBarWithStacks(string itemName, int amount)
{
    while (amount > 0 && itemBarItems.Count < itemBarCapacity)
    {
        string stackKey = itemName;
        int suffix = 1;
        while (itemBarItems.ContainsKey(stackKey))
        {
            stackKey = $"{itemName}_{suffix}";
            suffix++;
        }

        int stackAmount = Mathf.Min(amount, MAX_STACK_SIZE);
        itemBarItems.Add(stackKey, stackAmount);
        amount -= stackAmount;
    }

    if (amount > 0)
    {
        AddToMainInventory(itemName, amount);
    }
}

private void AddToFoodBarWithStacks(string itemName, int amount)
{
    while (amount > 0 && foodBarItems.Count < foodBarCapacity)
    {
        string stackKey = itemName;
        int suffix = 1;
        while (foodBarItems.ContainsKey(stackKey))
        {
            stackKey = $"{itemName}_{suffix}";
            suffix++;
        }

        int stackAmount = Mathf.Min(amount, MAX_STACK_SIZE);
        foodBarItems.Add(stackKey, stackAmount);
        amount -= stackAmount;
    }

    if (amount > 0)
    {
        AddToMainInventory(itemName, amount);
    }
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
        endDragEntry.callback.AddListener((data) => { uiManager.OnEndDrag((PointerEventData)data); }); // Changed from OnDragEnd to OnEndDrag
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

    // Try to remove from food bar first
    if (foodBarItems.ContainsKey(itemName))
    {
        RemoveFromContainer(amountToRemove, foodBarItems, itemName);
        amountToRemove = 0;  // Add this line to prevent further removals
    }
    
    // If we still need to remove more, try item bar
    if (amountToRemove > 0 && itemBarItems.ContainsKey(itemName))
    {
        RemoveFromContainer(amountToRemove, itemBarItems, itemName);
        amountToRemove = 0;
    }
    
    // If we still need to remove more, try main inventory
    if (amountToRemove > 0 && mainInventory.ContainsKey(itemName))
    {
        RemoveFromContainer(amountToRemove, mainInventory, itemName);
    }

    // After removal, consolidate stacks
    ConsolidateStacks(itemName);

    UpdateInventoryDisplay();
    itemBar.UpdateItemBar();
    foodBar.UpdateFoodBar();
    OnInventoryChanged?.Invoke();
}

public void RemoveFromContainer(int amount, Dictionary<string, int> container, string itemName)
{
    if (container.ContainsKey(itemName))
    {
        container[itemName] -= amount;
        if (container[itemName] <= 0)
        {
            container.Remove(itemName);
        }
        //UpdateInventoryDisplay();
        //UpdateItemBar();
        //UpdateFoodBar();
        //OnInventoryChanged?.Invoke();
    }
}

public void RemoveFromFoodBar(string itemName, int amount)
{
    if (foodBarItems.ContainsKey(itemName))
    {
        foodBarItems[itemName] -= amount;
        if (foodBarItems[itemName] <= 0)
        {
            foodBarItems.Remove(itemName);
        }
        UpdateFoodBar();
        OnInventoryChanged?.Invoke();
    }
}



public string GetSelectedItemName()
{
    // First check if there's a dragged item from UIManager
    UIManager uiManager = FindObjectOfType<UIManager>();
    if (uiManager != null && !string.IsNullOrEmpty(uiManager.draggedItemName))
    {
        return uiManager.draggedItemName.Split('_')[0];
    }

    // If no dragged item, check item bar
    foreach (var item in itemBarItems)
    {
        string baseItemName = item.Key.Split('_')[0];
        if (item.Value > 0)
        {
            return baseItemName;
        }
    }
    return null;
}

public Sprite GetSelectedItemSprite()
{
    // First check if there's a dragged item from UIManager
    UIManager uiManager = FindObjectOfType<UIManager>();
    if (uiManager != null && !string.IsNullOrEmpty(uiManager.draggedItemName))
    {
        return LoadItemSprite(uiManager.draggedItemName.Split('_')[0]);
    }

    // If no dragged item, check item bar
    foreach (var item in itemBarItems)
    {
        if (item.Value > 0)
        {
            return LoadItemSprite(item.Key);
        }
    }
    return null;
}

public bool HasItem(string itemName, string containerName)
{
    Dictionary<string, int> container = GetContainer(containerName);
    return container.Any(x => x.Key.Split('_')[0] == itemName && x.Value > 0);
}

public bool IsItemSelected(string itemName)
{
    // Get all items in the item bar
    var itemBarList = GetItemBarItems();
    
    // Check if the item exists in the selected slot
    return itemBarList.Any(x => x.Key.Split('_')[0] == itemName);
}

private void ConsolidateStacks(string itemName)
{
    // Consolidate main inventory stacks
    ConsolidateContainerStacks(mainInventory, itemName);
    ConsolidateContainerStacks(itemBarItems, itemName);
    ConsolidateContainerStacks(foodBarItems, itemName);
}

private void ConsolidateContainerStacks(Dictionary<string, int> container, string itemName)
{
    var stacksToConsolidate = container.Where(x => x.Key.Split('_')[0] == itemName && x.Value < MAX_STACK_SIZE)
                                     .OrderBy(x => x.Key)
                                     .ToList();
    
    while (stacksToConsolidate.Count > 1)
    {
        var firstStack = stacksToConsolidate[0];
        var secondStack = stacksToConsolidate[1];
        
        int spaceInFirstStack = MAX_STACK_SIZE - firstStack.Value;
        int amountToMove = Mathf.Min(spaceInFirstStack, secondStack.Value);
        
        container[firstStack.Key] += amountToMove;
        container[secondStack.Key] -= amountToMove;
        
        if (container[secondStack.Key] <= 0)
        {
            container.Remove(secondStack.Key);
            stacksToConsolidate.RemoveAt(1);
        }
        else if (container[firstStack.Key] >= MAX_STACK_SIZE)
        {
            stacksToConsolidate.RemoveAt(0);
        }
    }
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

public Dictionary<string, int> EdibleFoodValues = new Dictionary<string, int>()
{
    {"Carrot", 5},
    {"Herby Carrots", 10},
    {"Bread", 10},
    // Add more edible foods here
};
}