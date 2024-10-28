using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;


public class UIManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject inventoryPanel;
    public GameObject craftingPanel;
    public GameObject minimapPanel;
    public GameObject itemBarPanel;
    public GameObject foodBarPanel;
    public TextMeshProUGUI messageText;
    public GameObject characterPanel;
    public Texture2D selectingCursor;
    public Texture2D defaultCursor;
    private PlayerInventory playerInventory;
    private MessageManager messageManager;
    private Character characterScript;
    private BuildingSystem buildingSystem;  
    private bool isDragging = false;
    private GameObject draggedItem;
    private string draggedItemName;
    private string dragSourceContainer;
    

    void Start()
    {
        playerInventory = FindObjectOfType<PlayerInventory>();
        messageManager = FindObjectOfType<MessageManager>();
        characterScript = FindObjectOfType<Character>();
        buildingSystem = FindObjectOfType<BuildingSystem>();

        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (craftingPanel != null) craftingPanel.SetActive(false);
        if (minimapPanel != null) minimapPanel.SetActive(true);
        if (itemBarPanel != null) itemBarPanel.SetActive(true);
        if (foodBarPanel != null) foodBarPanel.SetActive(true);
        if (characterPanel != null) characterPanel.SetActive(false);

        SetupContainerDragHandlers();
        SetupInventoryItemHover();
    }

private void OnDragEnd(PointerEventData eventData)
{
    if (draggedItem != null && !string.IsNullOrEmpty(draggedItemName))
    {
        GameObject droppedObject = eventData.pointerCurrentRaycast.gameObject;
        string targetContainer = GetContainerName(droppedObject);

        if (string.IsNullOrEmpty(targetContainer))
        {
            // Item was dropped outside any container (on the ground)
            if (IsBuildingItem(draggedItemName))
            {
                buildingSystem.InitiateBuildingPlacement(draggedItemName);
                playerInventory.RemoveItems(draggedItemName, 1);
            }
            else
            {
                ShowMessage("Can't place this item in the world");
            }
        }
        else if (targetContainer != dragSourceContainer)
        {
            // Handle dropping items within containers (existing code)
            // ...
        }

        // Clean up
        Destroy(draggedItem);
        draggedItem = null;
        draggedItemName = null;
        dragSourceContainer = null;
        isDragging = false;

        // Update all inventory displays
        playerInventory.UpdateInventoryDisplay();
        playerInventory.UpdateItemBar();
        playerInventory.UpdateFoodBar();
    }
}

private bool IsBuildingItem(string itemName)
{
    return itemName == "Smelter" || itemName == "Cooking Station" || itemName == "Processor" || itemName == "Drill" || itemName == "Conveyor";
}

private void SetupInventoryItemHover()
{
    SetupContainerItemHover(inventoryPanel);
    SetupContainerItemHover(itemBarPanel);
    SetupContainerItemHover(foodBarPanel);
    SetupContainerItemHover(characterPanel);
    SetupContainerItemHover(craftingPanel);
}

private void SetupContainerItemHover(GameObject container)
{
    if (container == null) return;

    Transform contentHolder = container.transform.Find("Content") ?? container.transform;

    foreach (Transform child in contentHolder)
    {
        if (child.GetComponent<Image>() != null)
        {
            SetupItemHover(child.gameObject);
        }
        else
        {
            foreach (Transform grandchild in child)
            {
                if (grandchild.GetComponent<Image>() != null)
                {
                    SetupItemHover(grandchild.gameObject);
                }
            }
        }
    }
}

private void SetupItemHover(GameObject item)
{
    EventTrigger trigger = item.GetComponent<EventTrigger>() ?? item.AddComponent<EventTrigger>();

    EventTrigger.Entry enterEntry = new EventTrigger.Entry();
    enterEntry.eventID = EventTriggerType.PointerEnter;
    enterEntry.callback.AddListener((data) => { OnPointerEnterItem((PointerEventData)data); });
    trigger.triggers.Add(enterEntry);

    EventTrigger.Entry exitEntry = new EventTrigger.Entry();
    exitEntry.eventID = EventTriggerType.PointerExit;
    exitEntry.callback.AddListener((data) => { OnPointerExitItem((PointerEventData)data); });
    trigger.triggers.Add(exitEntry);
}

private void OnPointerEnterItem(PointerEventData eventData)
{
    if (eventData.pointerEnter != null)
    {
        Image image = eventData.pointerEnter.GetComponent<Image>();
        if (image != null && image.sprite != null)
        {
            Cursor.SetCursor(selectingCursor, Vector2.zero, CursorMode.Auto);
        }
    }
}

private void OnPointerExitItem(PointerEventData eventData)
{
    Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
}

private void SetupContainerDragHandlers()
{
    SetupDragHandlersForContainer(inventoryPanel);
    SetupDragHandlersForContainer(itemBarPanel);
    SetupDragHandlersForContainer(foodBarPanel);
}

private void SetupDragHandlersForContainer(GameObject container)
{
    if (container == null) return;

    // Find the content holder (might be the container itself or a child named "Content")
    Transform contentHolder = container.transform.Find("Content") ?? container.transform;

    foreach (Transform child in contentHolder)
    {
        if (child.GetComponent<Image>() != null)
        {
            SetupDragHandler(child.gameObject);
        }
        else
        {
            foreach (Transform grandchild in child)
            {
                if (grandchild.GetComponent<Image>() != null)
                {
                    SetupDragHandler(grandchild.gameObject);
                }
            }
        }
    }
}

private void SetupDragHandler(GameObject item)
{
    EventTrigger trigger = item.GetComponent<EventTrigger>() ?? item.AddComponent<EventTrigger>();

    // Begin Drag
    EventTrigger.Entry beginDragEntry = new EventTrigger.Entry();
    beginDragEntry.eventID = EventTriggerType.BeginDrag;
    beginDragEntry.callback.AddListener((data) => { OnBeginDrag((PointerEventData)data); });
    trigger.triggers.Add(beginDragEntry);

    // Drag
    EventTrigger.Entry dragEntry = new EventTrigger.Entry();
    dragEntry.eventID = EventTriggerType.Drag;
    dragEntry.callback.AddListener((data) => { OnDrag((PointerEventData)data); });
    trigger.triggers.Add(dragEntry);

    // End Drag
    EventTrigger.Entry endDragEntry = new EventTrigger.Entry();
    endDragEntry.eventID = EventTriggerType.EndDrag;
    endDragEntry.callback.AddListener((data) => { OnEndDrag((PointerEventData)data); });
    trigger.triggers.Add(endDragEntry);
}

    private void AddEventTrigger(EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventType;
        entry.callback.AddListener(action);
        trigger.triggers.Add(entry);
    }

public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        GameObject draggedObject = eventData.pointerDrag;
        Transform itemIconTransform = draggedObject.transform.Find("ItemIcon");
        
        if (itemIconTransform == null) return;

        Image itemImage = itemIconTransform.GetComponent<Image>();
        if (itemImage == null || itemImage.sprite == null) return;

        string containerName = GetContainerName(draggedObject);
        if (string.IsNullOrEmpty(containerName)) return;

        draggedItemName = itemImage.sprite.name;
        dragSourceContainer = containerName;

        // Create drag visual
        draggedItem = new GameObject("DraggedItem");
        draggedItem.transform.SetParent(transform, false);
        
        RectTransform rt = draggedItem.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(50, 50); // Adjust size as needed

        Image draggedItemImage = draggedItem.AddComponent<Image>();
        draggedItemImage.sprite = itemImage.sprite;
        draggedItemImage.raycastTarget = false;

        // Fade the original item
        itemImage.color = new Color(1, 1, 1, 0.5f);

        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedItem != null)
        {
            draggedItem.transform.position = eventData.position;
        }
    }

public void OnEndDrag(PointerEventData eventData)
{
    if (isDragging && draggedItem != null && !string.IsNullOrEmpty(draggedItemName))
    {
        GameObject hitObject = eventData.pointerCurrentRaycast.gameObject;
        string targetContainer = GetContainerName(hitObject);

        // Reset the original item's appearance
        if (eventData.pointerDrag != null)
        {
            Image originalItemImage = GetItemImage(eventData.pointerDrag);
            if (originalItemImage != null)
            {
                originalItemImage.color = Color.white;
            }
        }

        if (!string.IsNullOrEmpty(targetContainer) && targetContainer != dragSourceContainer)
        {
            // Move the entire stack (passing 1 will move all)
            bool moved = playerInventory.MoveItem(draggedItemName, 1, dragSourceContainer, targetContainer);
            
            if (moved)
            {
                // Update all displays
                playerInventory.UpdateInventoryDisplay();
                playerInventory.UpdateItemBar();
                playerInventory.UpdateFoodBar();
            }
        }

        CleanUpDragOperation();
    }
}

private void ResetDraggedItemAppearance(PointerEventData eventData)
{
    if (eventData.pointerDrag != null)
    {
        Image originalItemImage = GetItemImage(eventData.pointerDrag);
        if (originalItemImage != null)
        {
            originalItemImage.color = Color.white;
        }
    }
}

private void UpdateAllInventoryDisplays()
{
    playerInventory.UpdateInventoryDisplay();
    playerInventory.UpdateItemBar();
    playerInventory.UpdateFoodBar();
}

private void CleanUpDragOperation()
{
    if (draggedItem != null)
    {
        Destroy(draggedItem);
    }
    draggedItem = null;
    draggedItemName = null;
    dragSourceContainer = null;
    isDragging = false;
}

private void ShowMessage(string message)
{
    if (messageText != null)
    {
        messageText.text = message;
        messageText.gameObject.SetActive(true);
        StartCoroutine(HideMessageAfterDelay(3f));
    }
}

private System.Collections.IEnumerator HideMessageAfterDelay(float delay)
{
    yield return new WaitForSeconds(delay);
    if (messageText != null)
    {
        messageText.gameObject.SetActive(false);
    }
}

private void SwapSlotContents(Transform sourceSlot, Transform targetSlot)
{
    // Swap the ItemIcon contents
    Transform sourceItemIcon = sourceSlot.Find("ItemIcon");
    Transform targetItemIcon = targetSlot.Find("ItemIcon");

    if (sourceItemIcon != null && targetItemIcon != null)
    {
        Image sourceImage = sourceItemIcon.GetComponent<Image>();
        Image targetImage = targetItemIcon.GetComponent<Image>();

        Sprite tempSprite = sourceImage.sprite;
        sourceImage.sprite = targetImage.sprite;
        targetImage.sprite = tempSprite;

        // Swap the quantity text
        TextMeshProUGUI sourceText = sourceSlot.GetComponentInChildren<TextMeshProUGUI>();
        TextMeshProUGUI targetText = targetSlot.GetComponentInChildren<TextMeshProUGUI>();

        if (sourceText != null && targetText != null)
        {
            string tempText = sourceText.text;
            sourceText.text = targetText.text;
            targetText.text = tempText;
        }
    }
}

private Image GetItemImage(GameObject obj)
{
    Transform itemIconTransform = obj.transform.Find("ItemIcon");
    if (itemIconTransform != null)
    {
        return itemIconTransform.GetComponent<Image>();
    }
    return null;
}

private string GetContainerName(GameObject obj)
{
    if (obj == null) return string.Empty;

    Transform current = obj.transform;
    while (current != null)
    {
        if (current.gameObject == inventoryPanel)
            return "MainInventory";
        else if (current.gameObject == itemBarPanel)
            return "ItemBar";
        else if (current.gameObject == foodBarPanel)
            return "FoodBar";

        current = current.parent;
    }

    return string.Empty;
}

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleBuildingMenu();  // Changed from ToggleCrafting
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleCharacter();
        }
    }

    public void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            bool isActive = !inventoryPanel.activeSelf;
            inventoryPanel.SetActive(isActive);
            if (isActive)
            {
                playerInventory.UpdateInventoryDisplay();
            }
        }
    }

    public void ToggleCrafting()
    {
        if (craftingPanel != null)
        {
            craftingPanel.SetActive(!craftingPanel.activeSelf);
            if (craftingPanel.activeSelf)
            {
                UpdateCraftingDisplay();
            }
        }
    }

    public void UpdateCraftingDisplay()
    {
        // Update crafting UI with available recipes
    }

    public void ToggleCharacter()
    {
        if (characterPanel != null)
        {
            bool isActive = !characterPanel.activeSelf;
            characterPanel.SetActive(isActive);
            if (isActive && characterScript != null)
            {
                characterScript.UpdateStatsDisplay();
            }
        }
    }

    public void ToggleBuildingMenu()
    {
        if (buildingSystem != null)
        {
            buildingSystem.ToggleBuildingMenu();
        }
    }

    private void UpdateCharacterDisplay()
    {
        // Update character UI with current gear, skills, powers, minions, and stats
        // You'll need to implement this method based on your game's specific requirements
    }

    void ClearMessage()
    {
        if (messageText != null)
        {
            messageText.text = "";
        }
    }
}