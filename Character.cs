using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using System.Text;
using System.Linq;
public class Character : MonoBehaviour, IDropHandler
{
    [System.Serializable]
    public class GearSlot
    {
        public string name;
        public GameObject slotPrefab;        // Assign in Inspector
        public Sprite silhouetteSprite;      // Assign in Inspector
        public Image slotImage;              // Set at runtime
        public Image itemImage;              // Set at runtime
        public GearStats currentStats;
        public string currentItemName;
    }

    [Header("UI Elements")]
    public GameObject characterContent;
    public GameObject skillsContent;
    public GameObject powersContent;
    public GameObject minionContent;
    public TextMeshProUGUI statsTitle;
    public ScrollRect defenseStatsScrollRect;
    public ScrollRect offenseStatsScrollRect;
    public ScrollRect utilityStatsScrollRect;

    [Header("Tab Buttons")]
    public Button characterTabButton;
    public Button skillsTabButton;
    public Button powersTabButton;
    public Button minionTabButton;

    [Header("Cursor")]
    public Texture2D handCursor;
    public Texture2D defaultCursor;
    [Header("Gear Slots")]
    public List<GearSlot> gearSlots;

    [Header("Character Stats")]
    //defense
    public int health = 100;
    public int physicalResistance = 0;
    public int heatResistance = 0;
    public int poisonResistance = 0;
    public int radiationResistance = 0;
    //offense
    //public int projectile = 10;
    //public int explosive = 10;
    public int physicalDamage = 0;
    public int meleeAttackSpeed = 10;
    public int rangedAttackSpeed = 10;
    public int critChance = 0;
    public double critMultiplier = 1.5;
    //utility
    public int mvs = 10;

    [Header("References")]
    private PlayerInventory playerInventory;
    private PlayerStats playerStats;

    private Button currentSelectedButton;
    private Color normalColor = new Color(0.7f, 0.7f, 0.7f);
    private Color selectedColor = new Color(0.5f, 0.5f, 0.5f);

private void Start()
{
    playerInventory = FindObjectOfType<PlayerInventory>();
    if (playerInventory == null)
    {
        Debug.LogError("PlayerInventory not found!");
    }
    playerStats = GetComponent<PlayerStats>();
    if (playerStats == null)
    {
        Debug.LogError("PlayerStats component not found!");
    }
    SetupTabButtons();
    UpdateStatsDisplay();
    SetupButtonHoverEffects();
    SetupGearSlots();
    ShowTab(characterContent);
}

private void SetupGearSlots()
{
    if (characterContent == null)
    {
        Debug.LogError("Character Content is null!");
        return;
    }

    // Define slot positions relative to the character panel
    Dictionary<string, Vector2> slotPositions = new Dictionary<string, Vector2>
    {
        { "Helmet", new Vector2(-140, 150) },
        { "Chest", new Vector2(-140, -100) },
        { "Legs", new Vector2(140, -100) },
        { "Ring 1", new Vector2(47, 150) },
        { "Ring 2", new Vector2(140, 150) },
        { "Belt", new Vector2(140, 25) },
        { "Neck", new Vector2(-46, 150) },
        { "Bag", new Vector2(-140, 25) }
    };

    foreach (GearSlot slot in gearSlots)
    {
        if (slot.slotPrefab == null)
        {
            Debug.LogError($"Slot prefab is null for {slot.name}!");
            continue;
        }

        // Instantiate the gear slot prefab
        GameObject slotObj = Instantiate(slot.slotPrefab, characterContent.transform);
        RectTransform rt = slotObj.GetComponent<RectTransform>();
        
        // Position the slot
        if (slotPositions.TryGetValue(slot.name, out Vector2 position))
        {
            rt.anchoredPosition = position;
        }
        
        // Set up the slot image reference
        slot.slotImage = slotObj.transform.Find("MainImage")?.GetComponent<Image>();
        if (slot.slotImage == null)
        {
            Debug.LogError($"MainImage not found in prefab for {slot.name}!");
            continue;
        }
        
        // Set up the item image reference
        slot.itemImage = slotObj.transform.Find("ItemIcon")?.GetComponent<Image>();
        if (slot.itemImage == null)
        {
            Debug.LogError($"ItemIcon not found in prefab for {slot.name}!");
            continue;
        }
        
        // Set the silhouette sprite if available
        if (slot.silhouetteSprite != null)
        {
            slot.slotImage.sprite = slot.silhouetteSprite;
        }
        
        // Configure the images
        slot.slotImage.raycastTarget = true;
        slot.itemImage.enabled = false;
        
        // Add EventTrigger for all interactions
        EventTrigger trigger = slotObj.GetComponent<EventTrigger>() ?? slotObj.AddComponent<EventTrigger>();
        
        // Add drop handler
        EventTrigger.Entry dropEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.Drop
        };
        dropEntry.callback.AddListener((data) => OnGearSlotDrop(slot, (PointerEventData)data));
        trigger.triggers.Add(dropEntry);

        // Add begin drag handler
        EventTrigger.Entry beginDragEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.BeginDrag
        };
        beginDragEntry.callback.AddListener((data) => OnBeginDragFromGearSlot(slot, (PointerEventData)data));
        trigger.triggers.Add(beginDragEntry);

        // Add drag handler
        EventTrigger.Entry dragEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.Drag
        };
        dragEntry.callback.AddListener((data) => FindObjectOfType<UIManager>().OnDrag((PointerEventData)data));
        trigger.triggers.Add(dragEntry);

        // Add end drag handler
        EventTrigger.Entry endDragEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.EndDrag
        };
        endDragEntry.callback.AddListener((data) => FindObjectOfType<UIManager>().OnEndDrag((PointerEventData)data));
        trigger.triggers.Add(endDragEntry);
        
        Debug.Log($"Set up gear slot: {slot.name} with all handlers");
    }
}

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop called in Character"); // Debug log
        
        // Find which gear slot was dropped on
        foreach (GearSlot slot in gearSlots)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(
                slot.slotImage.rectTransform, 
                eventData.position))
            {
                OnGearSlotDrop(slot, eventData);
                break;
            }
        }
    }

private void OnGearSlotShiftRightClick(GearSlot slot)
{
    // Get the currently selected item from inventory
    PlayerInventory inventory = FindObjectOfType<PlayerInventory>();
    if (inventory == null) return;

    string selectedItemName = inventory.GetSelectedItemName();
    if (string.IsNullOrEmpty(selectedItemName)) return;

    if (CanEquipInSlot(selectedItemName, slot.name))
    {
        // Remove old stats if there was an item
        if (slot.currentStats != null)
        {
            RemoveGearStats(slot.currentStats);
        }

        // Get new item stats and add them
        GearStats itemStats = GetItemStats(selectedItemName);
        AddGearStats(itemStats);

        // Update slot visuals
        slot.itemImage.sprite = inventory.GetSelectedItemSprite();
        slot.itemImage.enabled = true;
        slot.currentStats = itemStats;
        slot.currentItemName = selectedItemName;

        // Update overall stats display
        UpdateStatsDisplay();
    }
}

public GearStats GetItemStats(string itemName)
{
    return GearStats.GetGearStats(itemName);
}

public void OnGearSlotDrop(GearSlot slot, PointerEventData data)
{
    GameObject draggedItem = data.pointerDrag;
    if (draggedItem == null) return;

    UIManager uiManager = FindObjectOfType<UIManager>();
    if (uiManager == null || string.IsNullOrEmpty(uiManager.draggedItemName)) return;

    string itemName = uiManager.draggedItemName.Split('_')[0];
    
    Debug.Log($"Attempting to equip {itemName} in {slot.name}");
    
    if (CanEquipInSlot(itemName, slot.name))
    {
        // Store the old item if there was one
        string oldItemName = slot.currentItemName;
        GearStats oldStats = slot.currentStats;

        // Remove old stats and return old item to inventory if it exists
        if (oldStats != null)
        {
            RemoveGearStats(oldStats);
            if (!string.IsNullOrEmpty(oldItemName))
            {
                playerInventory.AddItem(oldItemName, 1);
            }
        }

        // Remove the new item from the source inventory if it's not coming from another gear slot
        if (uiManager.dragSourceContainer != "Character")
        {
            playerInventory.RemoveItems(itemName, 1);
            playerInventory.UpdateInventoryDisplay();
        }
        else
        {
            // If it's coming from another gear slot, find and clear that slot
            foreach (GearSlot otherSlot in gearSlots)
            {
                if (otherSlot.currentItemName == itemName && otherSlot != slot)
                {
                    otherSlot.itemImage.enabled = false;
                    otherSlot.currentItemName = null;
                    otherSlot.currentStats = null;
                    if (otherSlot.silhouetteSprite != null)
                    {
                        otherSlot.slotImage.sprite = otherSlot.silhouetteSprite;
                    }
                    break;
                }
            }
        }

        // Get new item stats and add them
        GearStats itemStats = GetItemStats(itemName);
        AddGearStats(itemStats);

        // Load and set the proper item sprite
        Sprite properItemSprite = playerInventory.LoadItemSprite(itemName);
        if (properItemSprite != null)
        {
            slot.itemImage.sprite = properItemSprite;
            slot.itemImage.enabled = true;
            slot.currentStats = itemStats;
            slot.currentItemName = itemName;
            
            // Force cleanup of the drag operation
            uiManager.CleanUpDragOperation();
        }
        else
        {
            Debug.LogError($"Failed to load sprite for item: {itemName}");
        }

        UpdateStatsDisplay();
        playerInventory.UpdateInventoryDisplay();
    }
    else
    {
        Debug.Log($"Cannot equip {itemName} in {slot.name} slot");
    }
}

public void OnBeginDragFromGearSlot(GearSlot slot, PointerEventData eventData)
{
    if (string.IsNullOrEmpty(slot.currentItemName)) return;

    UIManager uiManager = FindObjectOfType<UIManager>();
    if (uiManager != null)
    {
        // Set up drag data first
        uiManager.draggedItemName = slot.currentItemName;
        uiManager.dragSourceContainer = "Character";
        uiManager.dragSourceSlot = slot;  // Store the original slot reference

        // Create drag visual without clearing the original slot
        Sprite itemSprite = slot.itemImage.sprite;
        if (itemSprite != null)
        {
            uiManager.CreateDragVisual(itemSprite, eventData);
        }

        // Just fade the original item instead of clearing it
        slot.itemImage.color = new Color(1, 1, 1, 0.5f);
    }
}

public bool CanEquipInSlot(string itemName, string slotName)
{
    Debug.Log($"Checking if {itemName} can be equipped in {slotName}");
    switch (slotName)
    {
        case "Helmet":
            return itemName.Contains("Helmet") || itemName.Contains("Mask");
        case "Chest":
            return itemName.Contains("Armor") || itemName.Contains("Robe");
        case "Ring 1":
        case "Ring 2":
            return itemName.Contains("Ring");
        case "Belt":
            return itemName.Contains("Belt") || itemName.Contains("Sash");
        case "Legs":
            return itemName.Contains("Pants") || itemName.Contains("Leggings");
        case "Neck":
            return itemName.Contains("Amulet") || itemName.Contains("Necklace");
        case "Bag":
            return itemName.Contains("Bag") || itemName.Contains("Backpack") || itemName.Contains("Satchel");
        default:
            return false;
    }
}

public GearSlot GetGearSlotByName(string slotName)
{
    return gearSlots.FirstOrDefault(slot => slot.name == slotName);
}

public void AddGearStats(GearStats stats)
{
    physicalResistance += stats.physicalResistance;
    heatResistance += stats.heatResistance;
    poisonResistance += stats.poisonResistance;
    radiationResistance += stats.radiationResistance;
    physicalDamage += stats.physicalDamage;
    meleeAttackSpeed += stats.meleeAttackSpeed;
    rangedAttackSpeed += stats.rangedAttackSpeed;
    critChance += stats.critChance;
    critMultiplier += stats.critMultiplier;
    mvs += stats.moveSpeed;
}

public void RemoveGearStats(GearStats stats)
{
    physicalResistance -= stats.physicalResistance;
    heatResistance -= stats.heatResistance;
    poisonResistance -= stats.poisonResistance;
    radiationResistance -= stats.radiationResistance;
    physicalDamage -= stats.physicalDamage;
    meleeAttackSpeed -= stats.meleeAttackSpeed;
    rangedAttackSpeed -= stats.rangedAttackSpeed;
    critChance -= stats.critChance;
    critMultiplier -= stats.critMultiplier;
    mvs -= stats.moveSpeed;
}

    private void SetupButtonHoverEffects()
    {
        SetupButtonHover(characterTabButton);
        SetupButtonHover(skillsTabButton);
        SetupButtonHover(powersTabButton);
        SetupButtonHover(minionTabButton);
    }

    private void SetupButtonHover(Button button)
    {
        if (button != null)
        {
            EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = button.gameObject.AddComponent<EventTrigger>();
            }

            EventTrigger.Entry enterEntry = new EventTrigger.Entry();
            enterEntry.eventID = EventTriggerType.PointerEnter;
            enterEntry.callback.AddListener((data) => { OnButtonHover(true); });
            trigger.triggers.Add(enterEntry);

            EventTrigger.Entry exitEntry = new EventTrigger.Entry();
            exitEntry.eventID = EventTriggerType.PointerExit;
            exitEntry.callback.AddListener((data) => { OnButtonHover(false); });
            trigger.triggers.Add(exitEntry);
        }
    }

    private void OnButtonHover(bool isHovering)
    {
        Cursor.SetCursor(isHovering ? handCursor : null, Vector2.zero, CursorMode.Auto);
    }

    private void SetupTabButtons()
    {
        characterTabButton.onClick.AddListener(() => ShowTab(characterContent));
        skillsTabButton.onClick.AddListener(() => ShowTab(skillsContent));
        powersTabButton.onClick.AddListener(() => ShowTab(powersContent));
        minionTabButton.onClick.AddListener(() => ShowTab(minionContent));

        // Show character tab by default
        ShowTab(characterContent);
    }

public void ShowTab(GameObject tabContent)
{
    // Reset previous button color if exists
    if (currentSelectedButton != null)
    {
        Image buttonImage = currentSelectedButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            Color color = normalColor;
            color.a = buttonImage.color.a;
            buttonImage.color = color;
        }
    }

    // Set new button color based on tab
    Button selectedButton = null;
    if (tabContent == characterContent)
        selectedButton = characterTabButton;
    else if (tabContent == skillsContent)
        selectedButton = skillsTabButton;
    else if (tabContent == powersContent)
        selectedButton = powersTabButton;
    else if (tabContent == minionContent)
        selectedButton = minionTabButton;

    if (selectedButton != null)
    {
        Image buttonImage = selectedButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            Color color = selectedColor;
            color.a = buttonImage.color.a;
            buttonImage.color = color;
        }
        currentSelectedButton = selectedButton;
    }

    characterContent.SetActive(false);
    skillsContent.SetActive(false);
    powersContent.SetActive(false);
    minionContent.SetActive(false);

    tabContent.SetActive(true);
}

    public void UpdateStatsDisplay()
    {
        statsTitle.text = "Stats";

        UpdateScrollableStats(defenseStatsScrollRect, BuildDefenseStats());
        UpdateScrollableStats(offenseStatsScrollRect, BuildOffenseStats());
        UpdateScrollableStats(utilityStatsScrollRect, BuildUtilityStats());
    }

    private void UpdateScrollableStats(ScrollRect scrollRect, string stats)
    {
        TextMeshProUGUI statsText = scrollRect.content.GetComponent<TextMeshProUGUI>();
        if (statsText != null)
        {
            statsText.text = stats;
        }
    }

private string BuildDefenseStats()
{
    StringBuilder defenseStats = new StringBuilder();
    
    if (playerStats != null)
    {
        defenseStats.AppendLine($"HP: {playerStats.maxHealth}");
    }
    else
    {
        defenseStats.AppendLine($"HP: {health}");
    }
    
    defenseStats.AppendLine($"Physical: {physicalResistance}");
    defenseStats.AppendLine($"Heat: {heatResistance}");
    defenseStats.AppendLine($"Poison: {poisonResistance}");
    defenseStats.AppendLine($"Radiation: {radiationResistance}");
    //defenseStats.AppendLine($"Magic Resistance: {magicResistance}");
    //defenseStats.AppendLine($"Fire Resistance: {fireResistance}");
    //defenseStats.AppendLine($"Cold Resistance: {coldResistance}");
    //defenseStats.AppendLine($"Light Resistance: {lightResistance}");
    //defenseStats.AppendLine($"Necrotic Resistance: {necroticResistance}");
    return defenseStats.ToString();
}

    public void OnStatsChanged()
    {
        UpdateStatsDisplay();
    }

    private string BuildOffenseStats()
    {
        StringBuilder offenseStats = new StringBuilder();
        offenseStats.AppendLine($"Physical dmg: {physicalDamage}");
        offenseStats.AppendLine($"Melee as: {meleeAttackSpeed}");
        offenseStats.AppendLine($"Ranged as: {rangedAttackSpeed}");
        //offenseStats.AppendLine($"Agility: {agility}");
        //offenseStats.AppendLine($"Projectile: {projectile}");
        //offenseStats.AppendLine($"Explosive: {explosive}");
        //offenseStats.AppendLine($"Magic: {intelligence}");
        offenseStats.AppendLine($"Crit %: {critChance}");
        offenseStats.AppendLine($"Crit X: {critMultiplier}");
        return offenseStats.ToString();
    }

private string BuildUtilityStats()
{
    StringBuilder utilityStats = new StringBuilder();
    //utilityStats.AppendLine($"Mana: {mana}");
    
    if (playerStats != null)
    {
        float satietyPercentage = (playerStats.currentSatiety / playerStats.maxSatiety) * 100f;
        string speedModifier = "Normal";
        float modifiedMvs = mvs;
        
        if (satietyPercentage > playerStats.highSatietyThreshold)
        {
            speedModifier = $"+{(playerStats.highSatietySpeedMultiplier - 1) * 100}%";
            modifiedMvs = mvs * playerStats.highSatietySpeedMultiplier;
        }
        else if (satietyPercentage < playerStats.lowSatietyThreshold)
        {
            speedModifier = $"-{(1 - playerStats.lowSatietySpeedMultiplier) * 100}%";
            modifiedMvs = mvs * playerStats.lowSatietySpeedMultiplier;
        }
        
        utilityStats.AppendLine($"Move Speed: {Mathf.Round(modifiedMvs)} ({speedModifier})");
    }
    else
    {
        utilityStats.AppendLine($"Move Speed: {mvs}");
    }
    
    return utilityStats.ToString();
}

public void EquipItem(GearSlot slot, string itemName)
{
    if (CanEquipInSlot(itemName, slot.name))
    {
        // Remove old stats if there was an item
        if (slot.currentStats != null)
        {
            RemoveGearStats(slot.currentStats);
        }

        // Get new item stats and add them
        GearStats itemStats = GetItemStats(itemName);
        AddGearStats(itemStats);

        // Update slot visuals
        slot.itemImage.sprite = playerInventory.LoadItemSprite(itemName);
        slot.itemImage.enabled = true;
        slot.currentStats = itemStats;
        slot.currentItemName = itemName;

        // Update overall stats display
        UpdateStatsDisplay();
    }
}

    public void UnequipItem(string slotName)
    {
        GearSlot slot = gearSlots.Find(s => s.name == slotName);
        if (slot != null)
        {
            slot.itemImage.sprite = null;
            slot.itemImage.enabled = false;
        }
    }
}