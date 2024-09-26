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
    public float scaleCoefficient = 1f;

    private List<GameObject> itemSlots = new List<GameObject>();

    void Start()
    {
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
            Image slotBackgroundImage = itemSlots[i].GetComponent<Image>();
            Transform itemIconTransform = itemSlots[i].transform.Find("ItemIcon");
            Image itemIconImage = itemIconTransform != null ? itemIconTransform.GetComponent<Image>() : null;
            TextMeshProUGUI quantityText = itemSlots[i].GetComponentInChildren<TextMeshProUGUI>();

            if (itemIconImage == null)
            {
                Debug.LogError("ItemIcon Image component not found in ItemSlot prefab.");
                continue;
            }

            if (i < itemBarItems.Count)
            {
                string itemName = itemBarItems[i].Key;
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
                    itemIconImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                }
                quantityText.text = itemBarItems[i].Value.ToString();
            }
            else
            {
                itemIconImage.sprite = null;
                itemIconImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                quantityText.text = "";
            }
        }
    }

}