using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ItemBar : MonoBehaviour
{
    public GameObject itemSlotPrefab;
    public int numberOfSlots = 8;
    public PlayerInventory playerInventory;
    public float spacing = 10f;

    private List<GameObject> itemSlots = new List<GameObject>();

    void Start()
    {
        CreateItemSlots();
        UpdateItemBar();
    }

    void CreateItemSlots()
    {
        RectTransform prefabRectTransform = itemSlotPrefab.GetComponent<RectTransform>();
        float slotWidth = prefabRectTransform.rect.width;
        float slotHeight = prefabRectTransform.rect.height;
        float posY = prefabRectTransform.anchoredPosition.y;

        float totalWidth = (slotWidth * numberOfSlots) + (spacing * (numberOfSlots - 1));
        float startX = -totalWidth / 2 + slotWidth / 2;

        for (int i = 0; i < numberOfSlots; i++)
        {
            GameObject slot = Instantiate(itemSlotPrefab, transform);
            RectTransform slotRectTransform = slot.GetComponent<RectTransform>();
            slotRectTransform.anchoredPosition = new Vector2(startX + i * (slotWidth + spacing), posY);
            slotRectTransform.sizeDelta = new Vector2(slotWidth, slotHeight);
            itemSlots.Add(slot);
        }
    }

    public void UpdateItemBar()
    {
        List<KeyValuePair<string, int>> inventoryItems = playerInventory.GetInventoryItems();

        for (int i = 0; i < itemSlots.Count; i++)
        {
            Image slotImage = itemSlots[i].GetComponent<Image>();
            TextMeshProUGUI quantityText = itemSlots[i].GetComponentInChildren<TextMeshProUGUI>();

            if (i < inventoryItems.Count)
            {
                string itemName = inventoryItems[i].Key;
                string formattedName = itemName.Replace(" ", "_");
                Sprite itemSprite = Resources.Load<Sprite>("Images/" + formattedName);
                
                if (itemSprite == null)
                {
                    itemSprite = Resources.Load<Sprite>("Images/" + itemName.Replace(" ", ""));
                }

                if (itemSprite != null)
                {
                    Debug.Log($"Loaded sprite: {itemName}");
                    slotImage.sprite = itemSprite;
                    slotImage.color = Color.white;
                }
                else
                {
                    Debug.Log($"Failed to load sprite: {itemName}");
                    slotImage.sprite = null;
                    slotImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                }
                quantityText.text = inventoryItems[i].Value.ToString();
            }
            else
            {
                slotImage.sprite = null;
                slotImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                quantityText.text = "";
            }
        }
    }
}