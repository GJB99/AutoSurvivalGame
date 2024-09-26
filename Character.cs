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
    public int health = 100;
    public int armor = 0;
    public int magicResistance = 0;
    public int fireResistance = 0;
    public int coldResistance = 0;
    public int mana = 100;
    public int strength = 10;
    public int agility = 10;
    public int intelligence = 10; 
    //public int projectile = 10;
    //public int explosive = 10;
    public int mvs = 10;
    public int critChance = 0;
    public double critX = 1.5;

    private void Start()
    {
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
        defenseStats.AppendLine($"HP: {health}");
        defenseStats.AppendLine($"Armor: {armor}");
        defenseStats.AppendLine($"Magic: {magicResistance}");
        defenseStats.AppendLine($"Fire: {fireResistance}");
        defenseStats.AppendLine($"Cold: {coldResistance}");
        defenseStats.AppendLine($"Light: {lightResistance}");
        defenseStats.AppendLine($"Necrotic: {necroticResistance}");
        return defenseStats.ToString();
    }

    private string BuildOffenseStats()
    {
        StringBuilder offenseStats = new StringBuilder();
        offenseStats.AppendLine($"Strength: {strength}");
        offenseStats.AppendLine($"Agility: {agility}");
        //offenseStats.AppendLine($"Projectile: {projectile}");
        //offenseStats.AppendLine($"Explosive: {explosive}");
        offenseStats.AppendLine($"Magic: {intelligence}");
        offenseStats.AppendLine($"Crit chance: {critChance}");
        offenseStats.AppendLine($"Crit X: {critX}");
        return offenseStats.ToString();
    }

    private string BuildUtilityStats()
    {
        StringBuilder utilityStats = new StringBuilder();
        utilityStats.AppendLine($"Mana: {mana}");
        utilityStats.AppendLine($"Move Speed: {mvs}");
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