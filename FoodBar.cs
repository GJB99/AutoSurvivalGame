using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;

public class FoodBar : MonoBehaviour
{
    public GameObject foodSlotPrefab;
    public int numberOfSlots = 3;
    public PlayerInventory playerInventory;
    public float spacing = 10f;
    public float scaleCoefficient = 1f;

    private List<GameObject> foodSlots = new List<GameObject>();

    void Start()
    {
        CreateFoodSlots();
        UpdateFoodBar();
    }

    void CreateFoodSlots()
    {
        RectTransform prefabRectTransform = foodSlotPrefab.GetComponent<RectTransform>();
        float slotWidth = prefabRectTransform.rect.width * scaleCoefficient;
        float slotHeight = prefabRectTransform.rect.height * scaleCoefficient;
        float posY = prefabRectTransform.anchoredPosition.y;

        float totalWidth = (slotWidth * numberOfSlots) + (spacing * (numberOfSlots - 1));
        float startX = -totalWidth / 2 + slotWidth / 2;

        for (int i = 0; i < numberOfSlots; i++)
        {
            GameObject slot = Instantiate(foodSlotPrefab, transform);
            RectTransform slotRectTransform = slot.GetComponent<RectTransform>();
            slotRectTransform.anchoredPosition = new Vector2(startX + i * (slotWidth + spacing), posY);
            slotRectTransform.sizeDelta = new Vector2(slotWidth, slotHeight);
            foodSlots.Add(slot);
        }
    }

private void UpdateSlotKeyBindings(GameObject slot, int index)
{
    TextMeshProUGUI keyBindText = slot.transform.Find("KeyBindText")?.GetComponent<TextMeshProUGUI>();
    if (keyBindText != null)
    {
        keyBindText.text = $"Shift+{index + 1}";
        keyBindText.fontSize = 10;
        keyBindText.alignment = TextAlignmentOptions.Center;
    }
}

public void UpdateFoodBar()
{
    var foodItems = playerInventory.GetFoodBarItems();

    // Clear existing items and update each slot
    for (int i = 0; i < foodSlots.Count; i++)
    {
        GameObject slot = foodSlots[i];
        
        // Update key binding text
        TextMeshProUGUI keyBindText = slot.transform.Find("KeyBindText")?.GetComponent<TextMeshProUGUI>();
        if (keyBindText != null)
        {
            keyBindText.text = $"Shift+{i + 1}";
            keyBindText.fontSize = 10;
            keyBindText.alignment = TextAlignmentOptions.Center;
        }

        // Update item display
        Transform itemIconTransform = slot.transform.Find("ItemIcon");
        Image itemIconImage = itemIconTransform != null ? itemIconTransform.GetComponent<Image>() : null;
        TextMeshProUGUI quantityText = slot.GetComponentInChildren<TextMeshProUGUI>();

        if (i < foodItems.Count)
        {
            string itemName = foodItems[i].Key;
            string baseItemName = itemName.Split('_')[0];
            if (itemIconImage != null)
            {
                itemIconImage.sprite = playerInventory.LoadItemSprite(baseItemName);
                itemIconImage.color = Color.white;
            }
            if (quantityText != null)
            {
                quantityText.text = foodItems[i].Value.ToString();
            }
        }
        else
        {
            if (itemIconImage != null)
            {
                itemIconImage.sprite = null;
                itemIconImage.color = new Color(1f, 1f, 1f, 0f); // Completely transparent
            }
            if (quantityText != null)
            {
                quantityText.text = "";
            }
        }
    }
}

private bool IsFoodItem(string itemName)
{
    // Strip any modifiers from the item name
    string baseItemName = itemName.Split('_')[0];
    
    // Add your food item names here
    string[] foodItems = { 
        "Apple", "Carrot", "Wheat", "Herb", "Bread", "Meat", "Fish",
        "Herby Carrots" // Add processed food items
    };
    return System.Array.Exists(foodItems, food => 
        food.Equals(baseItemName, System.StringComparison.OrdinalIgnoreCase));
}
}