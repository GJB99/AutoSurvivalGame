using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

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

    public void UpdateFoodBar()
    {
        List<KeyValuePair<string, int>> foodItems = playerInventory.GetFoodBarItems();

        for (int i = 0; i < foodSlots.Count; i++)
        {
            Image slotBackgroundImage = foodSlots[i].GetComponent<Image>();
            Transform itemIconTransform = foodSlots[i].transform.Find("ItemIcon");
            Image itemIconImage = itemIconTransform != null ? itemIconTransform.GetComponent<Image>() : null;
            TextMeshProUGUI quantityText = foodSlots[i].GetComponentInChildren<TextMeshProUGUI>();

            if (itemIconImage == null)
            {
                Debug.LogError("ItemIcon Image component not found in FoodSlot prefab.");
                continue;
            }

            if (i < foodItems.Count)
            {
                string itemName = foodItems[i].Key;
                string formattedName = itemName.Replace(" ", "_");
                Sprite itemSprite = Resources.Load<Sprite>("Images/" + formattedName);

                if (itemSprite == null)
                {
                    itemSprite = Resources.Load<Sprite>("Images/" + itemName.Replace(" ", ""));
                }

                if (itemSprite != null)
                {
                    itemIconImage.sprite = itemSprite;
                    itemIconImage.color = Color.white;
                }
                else
                {
                    Debug.Log($"Failed to load sprite: {itemName}");
                    itemIconImage.sprite = null;
                    itemIconImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                }
                quantityText.text = foodItems[i].Value.ToString();
            }
            else
            {
                itemIconImage.sprite = null;
                itemIconImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                quantityText.text = "";
            }
        }
    }

    private bool IsFoodItem(string itemName)
    {
        // Add your food item names here
        string[] foodItems = { "Apple", "Carrot", "Wheat", "Herb", "Bread", "Meat", "Fish" };
        return System.Array.Exists(foodItems, food => food.Equals(itemName, System.StringComparison.OrdinalIgnoreCase));
    }
}