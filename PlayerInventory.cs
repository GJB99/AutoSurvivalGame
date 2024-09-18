using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{
    private Dictionary<string, int> inventory = new Dictionary<string, int>();
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

    void Start()
    {
        if (inventoryUI != null) inventoryUI.SetActive(false);
        if (buildingMenuUI != null) buildingMenuUI.SetActive(false);
        itemBar = FindObjectOfType<ItemBar>();
        UpdateInventoryDisplay();
        UpdateBuildingMenuDisplay();
    }

    public bool HasStonePickaxe()
    {
        return inventory.ContainsKey("Stone Pickaxe") && inventory["Stone Pickaxe"] > 0;
    }

    public bool HasIronPickaxe()
    {
        return inventory.ContainsKey("Iron Pickaxe") && inventory["Iron Pickaxe"] > 0;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleInventory();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleBuildingMenu();
        }
    }

    public List<KeyValuePair<string, int>> GetInventoryItems()
    {
        return inventory.ToList();
    }

    public void AddItem(string itemName, int quantity)
    {
        if (inventory.ContainsKey(itemName))
        {
            inventory[itemName] += quantity;
        }
        else
        {
            inventory[itemName] = quantity;
        }
        UpdateInventoryDisplay();
        if (itemBar != null) itemBar.UpdateItemBar(); // Add this line
    }

    private void UpdateInventoryDisplay()
    {
        if (inventoryItemsContainer != null)
        {
            // Clear existing inventory items
            foreach (GameObject item in inventoryItemObjects)
            {
                Destroy(item);
            }
            inventoryItemObjects.Clear();

            // Create new inventory items
            foreach (var item in inventory)
            {
                GameObject newItem = Instantiate(inventoryItemPrefab, inventoryItemsContainer);
                Image itemImage = newItem.GetComponent<Image>();
                TextMeshProUGUI quantityText = newItem.GetComponentInChildren<TextMeshProUGUI>();

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
                    Debug.Log($"Failed to load sprite: {item.Key}");
                    itemImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                }

                quantityText.text = item.Value.ToString();
                inventoryItemObjects.Add(newItem);
            }
        }
    }

    public int GetItemCount(string itemName)
    {
        return inventory.ContainsKey(itemName) ? inventory[itemName] : 0;
    }

    public bool IsBuildingMenuVisible()
    {
        return isBuildingMenuVisible;
    }

    private void ToggleInventory()
    {
        isInventoryVisible = !isInventoryVisible;
        if (inventoryUI != null) inventoryUI.SetActive(isInventoryVisible);
        if (isInventoryVisible) UpdateInventoryDisplay();
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
        return inventory.ContainsKey(itemName) && inventory[itemName] >= cost;
    }

    public void RemoveItems(string itemName, int amount)
    {
        if (inventory.ContainsKey(itemName))
        {
            inventory[itemName] -= amount;
            if (inventory[itemName] <= 0)
            {
                inventory.Remove(itemName);
            }
            UpdateInventoryDisplay();
        }
    }
}