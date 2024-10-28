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
    // Implement your logic to determine if an item is food
    // For example, you could have a list of food items or a property on the item itself
    List<string> foodItems = new List<string> { "Apple", "Bread", "Fish" }; // Add your food items here
    return foodItems.Contains(itemName);
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
        UpdateInventoryDisplay();
        UpdateBuildingMenuDisplay();
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
        if (!IsFood(itemName) || foodBarItems.Count >= foodBarCapacity)
        {
            return false;
        }
    }

    // Validate item bar moves
    if (toContainer == "ItemBar" && itemBarItems.Count >= itemBarCapacity)
    {
        return false;
    }

    // Remove from source
    sourceContainer.Remove(itemName);

    // Add to target with stack size limit
    if (targetContainer.ContainsKey(itemName))
    {
        int currentAmount = targetContainer[itemName];
        int spaceInStack = MAX_STACK_SIZE - currentAmount;
        int amountToAdd = Mathf.Min(amount, spaceInStack);

        targetContainer[itemName] += amountToAdd;
        amount -= amountToAdd;

        // If there are remaining items, create a new stack
        if (amount > 0)
        {
            // For main inventory, we can create multiple stacks
            if (toContainer == "MainInventory")
            {
                while (amount > 0)
                {
                    int stackAmount = Mathf.Min(amount, MAX_STACK_SIZE);
                    targetContainer.Add(itemName + "_" + System.Guid.NewGuid().ToString(), stackAmount);
                    amount -= stackAmount;
                }
            }
            else
            {
                // For item bar and food bar, if we can't fit all items, move remainder to main inventory
                AddToMainInventory(itemName, amount);
            }
        }
    }
    else
    {
        // Creating a new stack
        int stackAmount = Mathf.Min(amount, MAX_STACK_SIZE);
        targetContainer[itemName] = stackAmount;
        
        // If there are remaining items, handle them
        if (amount > stackAmount)
        {
            AddToMainInventory(itemName, amount - stackAmount);
        }
    }

    OnInventoryChanged?.Invoke();
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
    // First try to add to any existing stacks in any container
    bool addedToExisting = false;

    // Check main inventory first for existing stacks
    foreach (var stack in mainInventory.ToList())
    {
        if (stack.Key.StartsWith(itemName) && stack.Value < MAX_STACK_SIZE)
        {
            int spaceInStack = MAX_STACK_SIZE - stack.Value;
            int amountToAdd = Mathf.Min(amount, spaceInStack);
            mainInventory[stack.Key] += amountToAdd;
            amount -= amountToAdd;
            addedToExisting = true;
            
            if (amount <= 0) break;
        }
    }

    // If we didn't add to an existing stack or still have remaining amount
    if (!addedToExisting || amount > 0)
    {
        // For food items, try food bar first
        if (IsFood(itemName))
        {
            if (foodBarItems.ContainsKey(itemName) && foodBarItems[itemName] < MAX_STACK_SIZE)
            {
                int spaceInStack = MAX_STACK_SIZE - foodBarItems[itemName];
                int amountToAdd = Mathf.Min(amount, spaceInStack);
                foodBarItems[itemName] += amountToAdd;
                amount -= amountToAdd;
            }
            else if (foodBarItems.Count < foodBarCapacity && amount > 0)
            {
                int stackAmount = Mathf.Min(amount, MAX_STACK_SIZE);
                foodBarItems[itemName] = stackAmount;
                amount -= stackAmount;
            }
        }

        // Try item bar if no existing stacks were found
        if (!addedToExisting && amount > 0)
        {
            if (itemBarItems.ContainsKey(itemName) && itemBarItems[itemName] < MAX_STACK_SIZE)
            {
                int spaceInStack = MAX_STACK_SIZE - itemBarItems[itemName];
                int amountToAdd = Mathf.Min(amount, spaceInStack);
                itemBarItems[itemName] += amountToAdd;
                amount -= amountToAdd;
            }
            else if (itemBarItems.Count < itemBarCapacity && amount > 0)
            {
                int stackAmount = Mathf.Min(amount, MAX_STACK_SIZE);
                itemBarItems[itemName] = stackAmount;
                amount -= stackAmount;
            }
        }

        // If there's still remaining amount, add to main inventory
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
        // Find an existing stack with room
        KeyValuePair<string, int>? availableStack = null;
        foreach (var stack in mainInventory)
        {
            if (stack.Key.StartsWith(itemName) && stack.Value < MAX_STACK_SIZE)
            {
                availableStack = stack;
                break;
            }
        }

        if (availableStack.HasValue)
        {
            // Add to existing stack
            int spaceInStack = MAX_STACK_SIZE - availableStack.Value.Value;
            int amountToAdd = Mathf.Min(quantity, spaceInStack);
            mainInventory[availableStack.Value.Key] += amountToAdd;
            quantity -= amountToAdd;
        }
        else
        {
            // Create new stack
            int stackAmount = Mathf.Min(quantity, MAX_STACK_SIZE);
            string stackKey = quantity <= MAX_STACK_SIZE ? itemName : itemName + "_" + System.Guid.NewGuid().ToString();
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
            Image itemImage = newItem.transform.Find("ItemIcon").GetComponent<Image>();
            TextMeshProUGUI quantityText = newItem.GetComponentInChildren<TextMeshProUGUI>();

            if (i < inventoryItems.Count)
            {
                var item = inventoryItems[i];
                string formattedName = item.Key.Replace(" ", "_");
                Sprite itemSprite = Resources.Load<Sprite>("Images/" + formattedName);

                if (itemSprite == null)
                {
                    itemSprite = Resources.Load<Sprite>("Images/" + item.Key.Replace(" ", ""));
                }

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