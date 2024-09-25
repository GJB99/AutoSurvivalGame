using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public GameObject inventoryPanel;
    public GameObject craftingPanel;
    public GameObject minimapPanel;
    public GameObject itemBarPanel;
    public GameObject foodBarPanel;
    public TextMeshProUGUI messageText;

    private PlayerInventory playerInventory;
    private MessageManager messageManager;

    void Start()
    {
        playerInventory = FindObjectOfType<PlayerInventory>();
        messageManager = FindObjectOfType<MessageManager>();

        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (craftingPanel != null) craftingPanel.SetActive(false);
        if (minimapPanel != null) minimapPanel.SetActive(true);
        if (itemBarPanel != null) itemBarPanel.SetActive(true);
        if (foodBarPanel != null) foodBarPanel.SetActive(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleCrafting();
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

    public void ShowMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
            CancelInvoke("ClearMessage");
            Invoke("ClearMessage", 2f);
        }
    }

    void ClearMessage()
    {
        if (messageText != null)
        {
            messageText.text = "";
        }
    }
}