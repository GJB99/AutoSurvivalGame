using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInventory : MonoBehaviour
{
    private Dictionary<string, int> inventory = new Dictionary<string, int>();
    public GameObject inventoryUI;
    public GameObject buildingMenuUI;
    public TextMeshProUGUI inventoryText;
    public TextMeshProUGUI buildingMenuText;
    private bool isInventoryVisible = false;
    private bool isBuildingMenuVisible = false;

    void Start()
    {
        if (inventoryUI != null) inventoryUI.SetActive(false);
        if (buildingMenuUI != null) buildingMenuUI.SetActive(false);
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
    }

    private void UpdateInventoryDisplay()
    {
        if (inventoryText != null)
        {
            string displayText = "Inventory:\n";
            foreach (var item in inventory)
            {
                displayText += $"{item.Key}: {item.Value}\n";
            }
            inventoryText.text = displayText;
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