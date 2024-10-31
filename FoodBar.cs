using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;

public class FoodBar : MonoBehaviour
{
    public GameObject foodSlotPrefab;
    public int numberOfSlots = 3;
    public PlayerInventory playerInventory;
    public float scaleCoefficient = 1f;
    public float spacing = 12.5f;

    private List<GameObject> foodSlots = new List<GameObject>();

private Dictionary<int, Color> originalSlotColors = new Dictionary<int, Color>();


    public void SetSlotBuffActive(int slotIndex, float duration)
    {
        if (slotIndex >= 0 && slotIndex < foodSlots.Count)
        {
            GameObject slot = foodSlots[slotIndex];
            Image outlineImage = slot.transform.Find("Background")?.GetComponent<Image>();
            
            if (outlineImage != null)
            {
                // Store original color if not stored
                if (!originalSlotColors.ContainsKey(slotIndex))
                {
                    originalSlotColors[slotIndex] = outlineImage.color;
                }
                
                // Set buff color
                outlineImage.color = new Color(1f, 0.95f, 0.2f, 1f); // Same yellow as item bar
                
                // Start coroutine to reset color after duration
                StartCoroutine(ResetSlotColorAfterDuration(slotIndex, duration));
            }
        }
    }

    private IEnumerator ResetSlotColorAfterDuration(int slotIndex, float duration)
    {
        yield return new WaitForSeconds(duration);
        
        if (slotIndex >= 0 && slotIndex < foodSlots.Count)
        {
            GameObject slot = foodSlots[slotIndex];
            Image outlineImage = slot.transform.Find("Background")?.GetComponent<Image>();
            
            if (outlineImage != null && originalSlotColors.ContainsKey(slotIndex))
            {
                outlineImage.color = originalSlotColors[slotIndex];
            }
        }
    }

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
    PlayerStats playerStats = FindObjectOfType<PlayerStats>();

    // Clear existing items and update each slot
    for (int i = 0; i < foodSlots.Count; i++)
    {
        GameObject slot = foodSlots[i];
        
        // Reset background color if slot becomes empty
        if (i >= foodItems.Count)
        {
            Image outlineImage = slot.transform.Find("Background")?.GetComponent<Image>();
            if (outlineImage != null && originalSlotColors.ContainsKey(i))
            {
                outlineImage.color = originalSlotColors[i];
                originalSlotColors.Remove(i);
            }
        }
        else
        {
            // Check if this slot has a buff-giving food
            string itemName = foodItems[i].Key;
            string baseFoodName = itemName.Split('_')[0];
            if (baseFoodName == "Herby Carrots" || baseFoodName == "Bread")
            {
                float remainingDuration = playerStats.GetRemainingBuffDuration(baseFoodName);
                if (remainingDuration > 0)
                {
                    SetSlotBuffActive(i, remainingDuration);
                }
            }
        }

        // Update key binding text
        TextMeshProUGUI keyBindText = slot.transform.Find("KeyBindText")?.GetComponent<TextMeshProUGUI>();
        if (keyBindText != null)
        {
            keyBindText.text = $"Shift+{i + 1}";
            keyBindText.fontSize = 20;
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