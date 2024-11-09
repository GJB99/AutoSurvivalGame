using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using YourGameNamespace;
using System;
using static Character;


public class UIManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI Panels")]
    public GameObject inventoryPanel;
    public GameObject craftingPanel;
    public GameObject minimapPanel;
    public GameObject itemBarPanel;
    public GameObject foodBarPanel;
    public GameObject characterPanel;
    public GameObject mapPanel;
    public Canvas canvas; 

    public Texture2D selectingCursor;
    public Texture2D defaultCursor;
    private PlayerInventory playerInventory;
    private MessageManager messageManager;
    private Character characterScript;

    private GridPlacementSystem gridSystem;
    private BuildingSystem buildingSystem;  
    private bool isDragging = false;
    private GameObject draggedItem;
    public string draggedItemName;
    public string dragSourceContainer;
    public Character.GearSlot dragSourceSlot;

    public TextMeshProUGUI upperMessageText;  // For first time information/requirement errors
    public TextMeshProUGUI lowerMessageText;  // For item notifications

    public GameObject messagePanel;
    public TextMeshProUGUI messageText;
    private float messageDisplayTime = 2f;
    private Coroutine currentMessageCoroutine;
    private Dictionary<string, int> resourceGainCounts = new Dictionary<string, int>();
    private Coroutine resourceMessageCoroutine;
    private float lastResourceTime;

    private LoggingSystem loggingSystem;
    
private void ShowMessage(string message)
{
    if (messagePanel != null && lowerMessageText != null)
    {
        messagePanel.SetActive(true);
        lowerMessageText.text = message;
        
        if (currentMessageCoroutine != null)
        {
            StopCoroutine(currentMessageCoroutine);
        }
        currentMessageCoroutine = StartCoroutine(HideMessageAfterDelay());
    }
}

    public void ShowUpperMessage(string message)
    {
        if (upperMessageText != null)
        {
            upperMessageText.gameObject.SetActive(true);
            upperMessageText.text = message;
            
            if (loggingSystem != null)
            {
                loggingSystem.AddMessage(message, false);
            }

            if (currentMessageCoroutine != null)
                StopCoroutine(currentMessageCoroutine);
            currentMessageCoroutine = StartCoroutine(HideMessageAfterDelay(upperMessageText, messageDisplayTime));
        }
    }

public void ShowLowerMessage(string message)
{
    ShowMessage(message);
    if (loggingSystem != null)
    {
        loggingSystem.AddMessage(message, true);  // true for inventory messages
    }
}

private System.Collections.IEnumerator HideMessageAfterDelay()
{
    yield return new WaitForSeconds(messageDisplayTime);
    messagePanel.SetActive(false);
    currentMessageCoroutine = null;
}

private System.Collections.IEnumerator HideMessageAfterDelay(TextMeshProUGUI messageText, float delay)
{
    yield return new WaitForSeconds(delay);
    if (messageText != null)
    {
        messageText.gameObject.SetActive(false);
    }
}

public void ShowResourceGainMessage(string resourceName, int amount)
{
    if (lowerMessageText == null) return;

    lastResourceTime = Time.time;

    if (!resourceGainCounts.ContainsKey(resourceName))
    {
        resourceGainCounts[resourceName] = amount;
    }
    else
    {
        resourceGainCounts[resourceName] += amount;
    }

    UpdateResourceGainMessage();

    if (resourceMessageCoroutine != null)
    {
        StopCoroutine(resourceMessageCoroutine);
    }
    resourceMessageCoroutine = StartCoroutine(ClearResourceGainMessageAfterDelay());
}

private void CloseAllPanels()
{
    if (inventoryPanel != null) inventoryPanel.SetActive(false);
    if (craftingPanel != null) craftingPanel.SetActive(false);
    if (characterPanel != null) characterPanel.SetActive(false);
    if (loggingSystem != null) loggingSystem.ClosePanel();
}

public void CreateDragVisual(Sprite itemSprite, PointerEventData eventData)
{
    if (canvas == null)
    {
        canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in the scene!");
            return;
        }
    }

    // Create a new drag visual if we don't have one
    if (draggedItem == null)
    {
        draggedItem = new GameObject("DragVisual");
        draggedItem.transform.SetParent(canvas.transform);
        draggedItem.transform.SetAsLastSibling();

        // Add required components
        RectTransform rt = draggedItem.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(50, 50); // Set appropriate size
        
        Image image = draggedItem.AddComponent<Image>();
        image.sprite = itemSprite;
        image.raycastTarget = false;
    }

    // Position the drag visual at the mouse position
    draggedItem.transform.position = eventData.position;
    isDragging = true;
}

private void UpdateResourceGainMessage()
{
    if (resourceGainCounts.Count == 0) return;

    string message = "";
    foreach (var kvp in resourceGainCounts)
    {
        if (message.Length > 0)
            message += "\n";
            
        // Adjusted sprite size and added vertical offset
        message += $"<color=#FFFFFF>+{kvp.Value}<space=10>{kvp.Key}<space=20></color><voffset=10><size=40><sprite name={kvp.Key}></size></voffset>";
    }
    
    lowerMessageText.text = message;
}

private System.Collections.IEnumerator ClearResourceGainMessageAfterDelay()
{
    while (true)
    {
        yield return new WaitForSeconds(2f);
        
        // Check if we've received any new resources in the last 2 seconds
        if (Time.time - lastResourceTime > 2f)
        {
            // Log the accumulated resources before clearing
            if (loggingSystem != null)
            {
                foreach (var resource in resourceGainCounts)
                {
                    loggingSystem.AddMessage($"+{resource.Value} {resource.Key}", true);
                }
            }
            
            resourceGainCounts.Clear();
            lowerMessageText.text = "";
            resourceMessageCoroutine = null;
            yield break;
        }
    }
}

    void Start()
    {
        playerInventory = FindObjectOfType<PlayerInventory>();
        messageManager = FindObjectOfType<MessageManager>();
        characterScript = FindObjectOfType<Character>();
        buildingSystem = FindObjectOfType<BuildingSystem>();
        gridSystem = FindObjectOfType<GridPlacementSystem>();
        loggingSystem = FindObjectOfType<LoggingSystem>();

            if (gridSystem == null)
        {
            Debug.LogError("GridPlacementSystem not found in the scene!");
        }

        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (craftingPanel != null) craftingPanel.SetActive(false);
        if (minimapPanel != null) minimapPanel.SetActive(true);
        if (itemBarPanel != null) itemBarPanel.SetActive(true);
        if (foodBarPanel != null) foodBarPanel.SetActive(true);
        if (characterPanel != null) characterPanel.SetActive(false);

        SetupContainerDragHandlers();
        SetupInventoryItemHover();
    }

public void OnDragEnd(PointerEventData eventData)
{
    if (draggedItem != null && !string.IsNullOrEmpty(draggedItemName))
    {
        GameObject droppedObject = eventData.pointerCurrentRaycast.gameObject;
        string targetContainer = GetContainerName(droppedObject);

        if (string.IsNullOrEmpty(targetContainer))
        {
            // Handle dropping outside containers
            if (IsBuildingItem(draggedItemName))
            {
                buildingSystem.InitiateBuildingPlacement(draggedItemName);
                playerInventory.RemoveItems(draggedItemName, 1);
            }
        }
        else
        {
            // Let PlayerInventory handle the stacking logic
            string baseItemName = draggedItemName.Split('_')[0];
            int amount = playerInventory.GetItemCount(draggedItemName, dragSourceContainer);
            playerInventory.RemoveItems(draggedItemName, amount);
            playerInventory.AddItem(baseItemName, amount);
        }

        CleanUpDragOperation();
        UpdateAllInventoryDisplays();
    }
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
    SetupDragHandlersForContainer(characterPanel);
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
    // Skip if this is a UI element or non-item object
    if (!IsValidDraggableItem(item)) return;

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

private bool IsValidDraggableItem(GameObject item)
{
    // Check if it's an inventory/itembar/foodbar item
    Transform itemIcon = item.transform.Find("ItemIcon");
    if (itemIcon != null && itemIcon.GetComponent<Image>() != null)
    {
        Transform parent = item.transform.parent;
        while (parent != null)
        {
            if (parent.gameObject == inventoryPanel || 
                parent.gameObject == itemBarPanel ||
                parent.gameObject == foodBarPanel)
            {
                return true;
            }
            parent = parent.parent;
        }
    }

    // Check if it's a gear slot item with an equipped item
    if (item.transform.parent?.gameObject == characterPanel)
    {
        Image itemImage = item.GetComponentInChildren<Image>();
        return itemImage != null && itemImage.enabled && itemImage.sprite != null;
    }

    return false;
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

public void CleanUpDragOperation()
{
    if (draggedItem != null)
    {
        Destroy(draggedItem);
    }
    
    // Restore the original slot's appearance if it was from character panel
    if (dragSourceSlot != null)
    {
        dragSourceSlot.itemImage.color = Color.white;
    }
    
    draggedItem = null;
    draggedItemName = null;
    dragSourceContainer = null;
    dragSourceSlot = null;
    isDragging = false;
}

public void OnEndDrag(PointerEventData eventData)
{
    if (isDragging && draggedItem != null && !string.IsNullOrEmpty(draggedItemName))
    {
        GameObject droppedObject = eventData.pointerCurrentRaycast.gameObject;
        string targetContainer = GetContainerName(droppedObject);
        bool validDrop = false;
        bool itemReturned = false;

        // Handle gear items
        if (dragSourceContainer == "Character")
        {
            // Only allow dropping in inventory or item bar
            if (targetContainer == "MainInventory" || targetContainer == "ItemBar")
            {
                validDrop = true;
                playerInventory.AddItem(draggedItemName, 1);
                UpdateAllInventoryDisplays();
            }
            else
            {
                // If trying to drop on character panel or anywhere else, 
                // force return to original slot
                ReturnItemToOriginalSlot(eventData);
                itemReturned = true;
            }
        }
        // Handle inventory items
        else if (!string.IsNullOrEmpty(targetContainer))
        {
            if (targetContainer == "Character")
            {
                // Check if it can be equipped in the target slot
                Transform slotTransform = droppedObject.transform;
                while (slotTransform != null && slotTransform.parent != characterPanel.transform)
                {
                    slotTransform = slotTransform.parent;
                }

                if (slotTransform != null)
                {
                    Character character = FindObjectOfType<Character>();
                    if (character != null && character.CanEquipInSlot(draggedItemName, slotTransform.name))
                    {
                        validDrop = true;
                    }
                    else
                    {
                        // Don't return the item to inventory - it's already there
                        itemReturned = true;
                    }
                }
            }
            else if (targetContainer != dragSourceContainer)
            {
                string baseItemName = draggedItemName.Split('_')[0];
                validDrop = playerInventory.MoveItem(baseItemName, 1, dragSourceContainer, targetContainer);
                
                if (validDrop)
                {
                    UpdateAllInventoryDisplays();
                }
            }
        }

        if (!validDrop && !itemReturned)
        {
            ResetDraggedItemAppearance(eventData);
        }

        CleanUpDragOperation();
    }
}

    private bool IsBuildingItem(string itemName)
    {
        return itemName == "Smelter" || 
               itemName == "Processor" || 
               itemName == "Cooking Station" || 
               itemName == "Drill" || 
               itemName == "Oven" ||
               itemName == "Conveyor" ||
               itemName == "Chest"; 
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

private string GetOriginalSlotType(string itemName)
{
    if (itemName.Contains("Helmet") || itemName.Contains("Mask"))
        return "Helmet";
    if (itemName.Contains("Armor") || itemName.Contains("Robe"))
        return "Chest";
    if (itemName.Contains("Ring"))
        return "Ring 1"; // or "Ring 2"
    if (itemName.Contains("Belt") || itemName.Contains("Sash"))
        return "Belt";
    if (itemName.Contains("Pants") || itemName.Contains("Leggings"))
        return "Legs";
    if (itemName.Contains("Amulet") || itemName.Contains("Necklace"))
        return "Neck";
    if (itemName.Contains("Bag") || itemName.Contains("Backpack") || itemName.Contains("Satchel"))
        return "Bag";
    return "";
}

private void ReturnItemToOriginalSlot(PointerEventData eventData)
{
    GameObject originalSlot = eventData.pointerDrag;
    if (originalSlot != null)
    {
        Image itemImage = originalSlot.GetComponentInChildren<Image>();
        if (itemImage != null)
        {
            itemImage.enabled = true;
            itemImage.color = Color.white;
        }

        Character character = FindObjectOfType<Character>();
        if (character != null)
        {
            character.UpdateStatsDisplay();
        }
    }
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
        else if (current.gameObject == characterPanel)
            return "Character";

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
        ToggleBuildingMenu();
    }
    if (Input.GetKeyDown(KeyCode.C))
    {
        ToggleCharacter();
    }
    if (Input.GetKeyDown(KeyCode.M))
    {
        ToggleMap();
    }
    if (Input.GetKeyDown(KeyCode.L))
    {
        loggingSystem.ToggleLoggingPanel();
    }
}

public void ToggleMap()
{
    Minimap minimap = FindObjectOfType<Minimap>();
    if (minimap != null)
    {
        CloseAllPanels();
        minimap.ToggleFullMap();
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
        if (loggingSystem != null) loggingSystem.ClosePanel();
        
        bool isActive = !characterPanel.activeSelf;
        characterPanel.SetActive(isActive);
        if (isActive && characterScript != null)
        {
            characterScript.UpdateStatsDisplay();
        }
    }
}

public bool IsCharacterPanelActive()
{
    return characterPanel != null && characterPanel.activeSelf;
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