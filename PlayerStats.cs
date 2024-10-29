using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStats : MonoBehaviour
{
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
    
    private float nextHealthRegenTime;
    private float nextSatietyDecrease;
    private PlayerMovement playerMovement;

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement component not found!");
        }
        UpdateUI();
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
    }

    private void UpdateMovementSpeed()
    {
        float satietyPercentage = (currentSatiety / maxSatiety) * 100f;
        float speedMultiplier = 1f;
        
        if (satietyPercentage < lowSatietyThreshold)
        {
            speedMultiplier = lowSatietySpeedMultiplier;
        }
        else if (satietyPercentage > highSatietyThreshold)
        {
            speedMultiplier = highSatietySpeedMultiplier;
        }

        playerMovement.moveSpeed = playerMovement.moveSpeed * speedMultiplier;
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