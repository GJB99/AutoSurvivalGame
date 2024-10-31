using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;

public class ItemBar : MonoBehaviour
{
    public GameObject itemSlotPrefab;
    public int numberOfSlots = 5;
    public PlayerInventory playerInventory;
    public float spacing = 10f;
    public float scaleCoefficient = 1f;
    private ItemUseSystem itemUseSystem;
    private List<GameObject> itemSlots = new List<GameObject>();

    void Start()
    {
        itemUseSystem = FindObjectOfType<ItemUseSystem>();
        CreateItemSlots();
        UpdateItemBar();
    }

    void CreateItemSlots()
    {
        RectTransform prefabRectTransform = itemSlotPrefab.GetComponent<RectTransform>();
        float slotWidth = prefabRectTransform.rect.width * scaleCoefficient;
        float slotHeight = prefabRectTransform.rect.height * scaleCoefficient;
        float posY = prefabRectTransform.anchoredPosition.y - 60f; // Move the item bar down to make room for the food bar

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
    List<KeyValuePair<string, int>> itemBarItems = playerInventory.GetItemBarItems();

    for (int i = 0; i < itemSlots.Count; i++)
    {
        Transform itemIconTransform = itemSlots[i].transform.Find("ItemIcon");
        Image itemIconImage = itemIconTransform != null ? itemIconTransform.GetComponent<Image>() : null;
        Image outlineImage = itemSlots[i].transform.Find("Background")?.GetComponent<Image>();
        TextMeshProUGUI quantityText = itemSlots[i].GetComponentInChildren<TextMeshProUGUI>();
        TextMeshProUGUI keyBindText = itemSlots[i].transform.Find("KeyBindText")?.GetComponent<TextMeshProUGUI>();
        Image slotImage = itemSlots[i].GetComponent<Image>();

        // Update key binding text
        if (keyBindText != null)
        {
            keyBindText.text = (i + 1).ToString();
            keyBindText.fontSize = 10;
            keyBindText.alignment = TextAlignmentOptions.Center;
        }

        // Set black outline
        if (outlineImage != null)
        {
            outlineImage.color = new Color(0f, 0f, 0f, 1f);
        }

        // Update slot background color based on selection
        if (slotImage != null)
        {
            if (i == itemUseSystem.selectedSlot)
            {
                slotImage.color = new Color(1f, 1f, 0.7f, 1f); // Light yellow for selected slot
            }
            else if (i < itemBarItems.Count)
            {
                slotImage.color = new Color(0.6f, 0.6f, 0.6f, 1f); // Darker gray for filled slots
            }
            else
            {
                slotImage.color = new Color(0.8f, 0.8f, 0.8f, 1f); // Light gray for empty slots
            }
        }

        if (i < itemBarItems.Count)
        {
            string itemName = itemBarItems[i].Key;
            string baseItemName = itemName.Split('_')[0];
            Sprite itemSprite = playerInventory.LoadItemSprite(baseItemName);

            if (itemSprite != null)
            {
                itemIconImage.sprite = itemSprite;
                itemIconImage.color = Color.white;
                quantityText.text = itemBarItems[i].Value.ToString();
            }
            else
            {
                Debug.LogWarning($"Sprite not found for item: {itemName}");
                SetEmptySlotAppearance(itemIconImage, quantityText, itemSlots[i]);
            }
        }
        else
        {
            SetEmptySlotAppearance(itemIconImage, quantityText, itemSlots[i]);
        }
    }
}

private void SetEmptySlotAppearance(Image itemIconImage, TextMeshProUGUI quantityText, GameObject slot)
{
    itemIconImage.sprite = null;
    itemIconImage.color = new Color(1f, 1f, 1f, 0f); // Completely transparent
    quantityText.text = "";
    
    // Lighter background for empty slots
    Image slotImage = slot.GetComponent<Image>();
    if (slotImage != null)
    {
        slotImage.color = new Color(0.8f, 0.8f, 0.8f, 1f); // Light gray fill
    }
}

}