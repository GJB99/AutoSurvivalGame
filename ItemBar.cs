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

void Update()
{
    bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    
    // Only process number keys if shift is NOT held
    if (!shiftHeld)
    {
        for (int i = 0; i < numberOfSlots; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                itemUseSystem.selectedSlot = i;  // Use local selectedSlot instead
                UpdateItemBar();
            }
        }
    }
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
            keyBindText.fontSize = 20;
            keyBindText.alignment = TextAlignmentOptions.Center;
        }

        // Update slot background color based on selection
        if (outlineImage != null)
        {
            if (i == itemUseSystem.selectedSlot)
            {
                outlineImage.color = new Color(1f, 0.95f, 0.2f, 1f); // Bright yellow outline for selected slot
            }
            else
            {
                outlineImage.color = new Color(0f, 0f, 0f, 1f); // Black outline for unselected slots
            }
        }

        // Keep slot background colors consistent
        if (slotImage != null)
        {
            if (i < itemBarItems.Count)
            {
                slotImage.color = new Color(0.7f, 0.7f, 0.7f, 1f); // Medium gray for filled slots
            }
            else
            {
                slotImage.color = new Color(0.5f, 0.5f, 0.5f, 1f); // Darker gray for empty slots
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