using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic; 

public class PlayerStats : MonoBehaviour
{
    [System.Serializable]
    public class FoodEffect
    {
        public string name;
        public float duration;
        public float maxHealthBonus;
        public GameObject buffIcon;
    }

    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth = 50f;
    public float maxSatiety = 100f;
    public float currentSatiety = 50f;
    
    [Header("Regeneration Settings")]
    public float healthRegenRate = 1f;
    public float healthRegenInterval = 1f;
    public float satietyDecreaseRate = 1f;
    public float satietyDecreaseInterval = 10f;
    
    [Header("Movement Modifiers")]
    public float lowSatietyThreshold = 25f;
    public float highSatietyThreshold = 75f;
    public float lowSatietySpeedMultiplier = 0.5f;
    public float highSatietySpeedMultiplier = 1.25f;
    
    [Header("UI References")]
    public Image healthBarFill;
    public Image satietyBarFill;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI satietyText;

    [Header("Food Effects")]
    public List<FoodEffect> activeEffects = new List<FoodEffect>();
    public List<FoodEffect> activeBuffs = new List<FoodEffect>();  // New list for actual buffs
    public GameObject buffIconPrefab;
    public Transform buffContainer;
    public int maxActiveEffects = 3;

    [Header("Buff Icons")]
    public GameObject foodBuffPrefab;    // Assign the FoodBuff prefab
    public GameObject foodDebuffPrefab;  // Assign the FoodDebuff prefab
    private GameObject currentSatietyBuff;  // Track current buff/debuff icon
    
    private float nextHealthRegenTime;
    private float nextSatietyDecrease;
    private PlayerMovement playerMovement;
    private PlayerInventory playerInventory;

    [Header("UI References")]
    public Transform foodBuffContainer;    // New container just for food buffs
    public Transform passiveBuffContainer; // New container for passive buffs like satiety

    private Character characterUI;

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerInventory = GetComponent<PlayerInventory>();
        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement component not found!");
        }
        UpdateUI();
        characterUI = GetComponent<Character>();
    }

    private void Update()
    {
        // Health regeneration
        if (Time.time >= nextHealthRegenTime)
        {
            currentHealth = Mathf.Min(currentHealth + healthRegenRate, maxHealth);
            nextHealthRegenTime = Time.time + healthRegenInterval;
        }

        // Satiety decrease
        if (Time.time >= nextSatietyDecrease)
        {
            currentSatiety = Mathf.Max(currentSatiety - satietyDecreaseRate, 0);
            nextSatietyDecrease = Time.time + satietyDecreaseInterval;
        }

        // Update movement speed based on satiety
        UpdateMovementSpeed();
        
        // Update UI
        UpdateUI();

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            for (int i = 0; i < 3; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    TryEatFood(i);
                }
            }
        }
    }

    private void TryEatFood(int slotIndex)
    {
        var foodItems = playerInventory.GetFoodBarItems();
        if (slotIndex < foodItems.Count)
        {
            string foodName = foodItems[slotIndex].Key.Split('_')[0];
            if (IsEdibleFood(foodName))
            {
                ApplyFoodEffect(foodName);
                playerInventory.RemoveItems(foodItems[slotIndex].Key, 1);
            }
        }
    }

    private void ApplyFoodEffect(string foodName)
    {
        float satietyAmount = 0f;
        switch (foodName)
        {
            case "Carrot":
                satietyAmount = 5f;
                break;
            case "Herby Carrots":
                satietyAmount = 10f;
                break;
        }

        // Add satiety
        currentSatiety = Mathf.Min(currentSatiety + satietyAmount, maxSatiety);
        
        // Add buff effect with duration based on satiety amount
        AddFoodEffect(foodName, satietyAmount);
        
        UpdateUI();
    }

    private bool IsEdibleFood(string foodName)
    {
        return foodName == "Carrot" || foodName == "Herby Carrots";
    }

private void AddFoodEffect(string foodName, float satietyAmount)
{
    // Add to general food effects list
    FoodEffect effect = new FoodEffect
    {
        name = foodName,
        duration = satietyAmount * 60f
    };
    
    // Check if this food provides a buff
    bool hasBuff = false;
    
    if (foodName == "Herby Carrots")
    {
        hasBuff = true;
        effect.maxHealthBonus = 10f;
        maxHealth += effect.maxHealthBonus;
        
        GameObject buffObj = Instantiate(buffIconPrefab, foodBuffContainer);
        buffObj.GetComponentInChildren<TextMeshProUGUI>().text = "+10 HP";
        
        GameObject countdownObj = new GameObject("Countdown");
        countdownObj.transform.SetParent(buffObj.transform, false);
        TextMeshProUGUI countdownText = countdownObj.AddComponent<TextMeshProUGUI>();
        RectTransform rectTransform = countdownObj.GetComponent<RectTransform>();
        
        rectTransform.localPosition = new Vector3(0, -25, 0);
        rectTransform.sizeDelta = new Vector2(100, 20);
        rectTransform.localScale = Vector3.one;
        
        countdownText.fontSize = 12;
        countdownText.alignment = TextAlignmentOptions.Center;
        
        StartCoroutine(UpdateBuffDuration(countdownText, effect.duration));
        effect.buffIcon = buffObj;
    }

    // Manage lists separately
    if (hasBuff)
    {
        if (activeBuffs.Count >= maxActiveEffects)
        {
            RemoveOldestBuff();
        }
        activeBuffs.Add(effect);
    }
    else
    {
        if (activeEffects.Count >= maxActiveEffects)
        {
            RemoveOldestEffect();
        }
        activeEffects.Add(effect);
    }
}

private void RemoveOldestBuff()
{
    if (activeBuffs.Count > 0)
    {
        FoodEffect oldBuff = activeBuffs[0];
        maxHealth -= oldBuff.maxHealthBonus;
        if (oldBuff.buffIcon != null)
        {
            Destroy(oldBuff.buffIcon);
        }
        activeBuffs.RemoveAt(0);
        UpdateUI();
    }
}

private IEnumerator UpdateBuffDuration(TextMeshProUGUI countdownText, float duration)
{
    float remainingTime = duration;
    
    while (remainingTime > 0)
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        countdownText.text = $"{minutes:00}:{seconds:00}";
        
        yield return new WaitForSeconds(1f);
        remainingTime -= 1f;
    }
    
    // Effect has expired, remove it
    for (int i = 0; i < activeEffects.Count; i++)
    {
        if (activeEffects[i].buffIcon != null && 
            activeEffects[i].buffIcon.GetComponentInChildren<TextMeshProUGUI>() == countdownText)
        {
            RemoveEffect(i);
            break;
        }
    }
}

private void RemoveEffect(int index)
{
    FoodEffect effect = activeEffects[index];
    maxHealth -= effect.maxHealthBonus;
    if (effect.buffIcon != null)
    {
        Destroy(effect.buffIcon);
    }
    activeEffects.RemoveAt(index);
    UpdateUI();
}

    private void RemoveOldestEffect()
    {
        if (activeEffects.Count > 0)
        {
            FoodEffect oldEffect = activeEffects[0];
            maxHealth -= oldEffect.maxHealthBonus;
            if (oldEffect.buffIcon != null)
            {
                Destroy(oldEffect.buffIcon);
            }
            activeEffects.RemoveAt(0);
            UpdateUI();
        }
    }

private void UpdateMovementSpeed()
{
    if (playerMovement == null) return;
    
    float satietyPercentage = (currentSatiety / maxSatiety) * 100f;
    float speedMultiplier = 1f;
    
    // Remove existing buff/debuff if any
    if (currentSatietyBuff != null)
    {
        Destroy(currentSatietyBuff);
        currentSatietyBuff = null;
    }
    
    if (satietyPercentage < lowSatietyThreshold)
    {
        speedMultiplier = lowSatietySpeedMultiplier;
        currentSatietyBuff = Instantiate(foodDebuffPrefab, passiveBuffContainer);
        currentSatietyBuff.GetComponentInChildren<TextMeshProUGUI>().text = "-50% SPD";
    }
    else if (satietyPercentage > highSatietyThreshold)
    {
        speedMultiplier = highSatietySpeedMultiplier;
        currentSatietyBuff = Instantiate(foodBuffPrefab, passiveBuffContainer);
        currentSatietyBuff.GetComponentInChildren<TextMeshProUGUI>().text = "+25% SPD";
    }

    playerMovement.moveSpeed = playerMovement.baseSpeed * speedMultiplier;
}

private void UpdateUI()
{
    // Update health bar
    float healthPercentage = currentHealth / maxHealth;
    healthBarFill.fillAmount = healthPercentage;
    healthText.text = $"HP: {Mathf.Round(currentHealth)}/{maxHealth}";

    // Update satiety bar
    float satietyPercentage = currentSatiety / maxSatiety;
    satietyBarFill.fillAmount = satietyPercentage;
    satietyText.text = $"Satiety: {Mathf.Round(currentSatiety)}/{maxSatiety}";

    if (characterUI != null)
    {
        characterUI.OnStatsChanged();
    }
}

    public void AddHealth(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateUI();
    }

    public void AddSatiety(float amount)
    {
        currentSatiety = Mathf.Min(currentSatiety + amount, maxSatiety);
        UpdateUI();
    }
}