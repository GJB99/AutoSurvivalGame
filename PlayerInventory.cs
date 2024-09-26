using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{
    private Dictionary<string, int> inventory = new Dictionary<string, int>();
    private Dictionary<string, int> foodBarItems = new Dictionary<string, int>();
    private Dictionary<string, int> itemBarItems = new Dictionary<string, int>();
    private Dictionary<string, int> mainInventory = new Dictionary<string, int>();
    public GameObject inventoryUI;
    public GameObject buildingMenuUI;
    public TextMeshProUGUI inventoryText;
    public TextMeshProUGUI buildingMenuText;
    private bool isInventoryVisible = false;
    private bool isBuildingMenuVisible = false;
    private ItemBar itemBar;
    public GameObject inventoryItemPrefab;
    public Transform inventoryItemsContainer;
    private List<GameObject> inventoryItemObjects = new List<GameObject>();
    private FoodBar foodBar;
    public int itemBarCapacity = 5;
    public int foodBarCapacity = 3;

    void Start()
    {
        if (inventoryUI != null) inventoryUI.SetActive(false);
        if (buildingMenuUI != null) buildingMenuUI.SetActive(false);
        itemBar = FindObjectOfType<ItemBar>();
        foodBar = FindObjectOfType<FoodBar>();
        UpdateInventoryDisplay();
        UpdateBuildingMenuDisplay();
    }

    private bool IsFoodItem(string itemName)
    {
        // Add your food item names here
        string[] foodItems = { "Apple", "Herb", "Carrot", "Bread", "Wheat", "Fish", "Herb" };
        return System.Array.Exists(foodItems, food => food.Equals(itemName, System.StringComparison.OrdinalIgnoreCase));
    }

    public bool HasStonePickaxe()
    {
        return GetItemCount("Stone Pickaxe") > 0;
    }

    public bool HasIronPickaxe()
    {
        return GetItemCount("Iron Pickaxe") > 0;
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

    public void AddItem(string itemName, int quantity)
    {
        if (IsFoodItem(itemName))
        {
            int foodBarSpace = foodBarCapacity - foodBarItems.Count;
            int amountToAdd = Mathf.Min(quantity, foodBarSpace);
            
            if (amountToAdd > 0)
            {
                AddToFoodBar(itemName, amountToAdd);
                quantity -= amountToAdd;
            }
        }

        if (quantity > 0)
        {
            int itemBarSpace = itemBarCapacity - itemBarItems.Count;
            
            if (itemBarSpace > 0 || itemBarItems.ContainsKey(itemName))
            {
                if (itemBarItems.ContainsKey(itemName))
                {
                    AddToItemBar(itemName, quantity);
                    quantity = 0;
                }
                else
                {
                    AddToItemBar(itemName, 1);
                    quantity--;
                }
            }
        }

        if (quantity > 0)
        {
            AddToMainInventory(itemName, quantity);
        }

        UpdateInventoryDisplay();
        itemBar.UpdateItemBar();
        foodBar.UpdateFoodBar();
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
        if (mainInventory.ContainsKey(itemName))
        {
            mainInventory[itemName] += quantity;
        }
        else
        {
            mainInventory[itemName] = quantity;
        }    
        Debug.Log($"Added {quantity} {itemName} to main inventory. Total: {mainInventory[itemName]}");
    }

    public void UpdateInventoryDisplay()
    {
        Debug.Log("=== UpdateInventoryDisplay Start ===");

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

        // Resize the inventory container
        RectTransform containerRect = inventoryItemsContainer.GetComponent<RectTransform>();
        float containerWidth = columns * (slotSize + spacing) - spacing;
        float containerHeight = rows * (slotSize + spacing) - spacing;
        containerRect.sizeDelta = new Vector2(containerWidth, containerHeight);

        // Center the container in the inventoryUI
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = Vector2.zero;

        Debug.Log($"inventoryItemsContainer position: {containerRect.anchoredPosition}");
        Debug.Log($"inventoryItemsContainer size: {containerRect.sizeDelta}");

        List<KeyValuePair<string, int>> inventoryItems = mainInventory.ToList();

        Debug.Log($"Main inventory items: {inventoryItems.Count}");

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

    public int GetItemCount(string itemName)
    {
        int count = 0;

        if (foodBarItems.ContainsKey(itemName))
            count += foodBarItems[itemName];

        if (itemBarItems.ContainsKey(itemName))
            count += itemBarItems[itemName];

        if (mainInventory.ContainsKey(itemName))
            count += mainInventory[itemName];

        return count;
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
            displayText += "1. Stone Pickaxe (5 Rock)\n";
            displayText += "2. Iron Pickaxe (1 Stone Pickaxe, 10 Iron)\n";
            displayText += "3. Smelter (5 Copper, 5 Iron, 10 Rock)\n";
            displayText += "4. Processor (10 Iron Ingot, 10 Copper Ingot)\n";
            displayText += "\nPress 1, 2, 3 or 4 to build";
            buildingMenuText.text = displayText;
        }
    }

    public bool CanBuild(string itemName, int cost)
    {
        int totalItemCount = GetItemCount(itemName);
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

}