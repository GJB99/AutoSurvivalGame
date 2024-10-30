using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using System.Text;
public class Character : MonoBehaviour
{
    [System.Serializable]
    public class GearSlot
    {
        public string name;
        public Image slotImage;
        public Image itemImage;
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

    private PlayerStats playerStats;

    private void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats component not found!");
        }
        SetupTabButtons();
        UpdateStatsDisplay();
        SetupButtonHoverEffects();
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
        characterContent.SetActive(false);
        skillsContent.SetActive(false);
        powersContent.SetActive(false);
        minionContent.SetActive(false);

        tabContent.SetActive(true);

        // Debug log to check if the method is being called
        Debug.Log("Showing tab: " + tabContent.name);
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

    public void EquipItem(string slotName, Sprite itemSprite)
    {
        GearSlot slot = gearSlots.Find(s => s.name == slotName);
        if (slot != null)
        {
            slot.itemImage.sprite = itemSprite;
            slot.itemImage.enabled = true;
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