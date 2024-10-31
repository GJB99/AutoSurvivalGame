using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace YourGameNamespace
{
    public class LoggingSystem : MonoBehaviour
    {
        [Header("Panel References")]
        public GameObject loggingPanel;
        public GameObject messagesContent;
        public GameObject inventoryContent;
        public GameObject tipsContent;  // Add this
        public Button messagesTabButton;
        public Button inventoryTabButton;
        public Button tipsTabButton;    // Add this
        public TextMeshProUGUI messagesText;
        public TextMeshProUGUI inventoryText;

        private List<string> messagesList = new List<string>();
        private List<string> inventoryList = new List<string>();
        private const int MAX_MESSAGES = 10000;
        private ScrollRect messagesScrollRect;
        private ScrollRect inventoryScrollRect;
        private float savedMessagesScrollPosition = 1f;
        private float savedInventoryScrollPosition = 1f;

        private Button currentSelectedButton;
        private Color normalColor = new Color(0.7f, 0.7f, 0.7f);
        private Color selectedColor = new Color(0.5f, 0.5f, 0.5f);

        void Start()
        {
            loggingPanel.SetActive(false);
            messagesTabButton.onClick.AddListener(() => ShowTab(messagesContent));
            inventoryTabButton.onClick.AddListener(() => ShowTab(inventoryContent));
            tipsTabButton.onClick.AddListener(() => ShowTab(tipsContent));  // Add this
            
            messagesScrollRect = messagesContent.GetComponentInParent<ScrollRect>();
            inventoryScrollRect = inventoryContent.GetComponentInParent<ScrollRect>();
            
            ShowTab(messagesContent);
        }

        public void ToggleLoggingPanel()
        {
            UIManager uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null && uiManager.IsCharacterPanelActive())
            {
                uiManager.ToggleCharacter();
            }

            loggingPanel.SetActive(!loggingPanel.activeSelf);
            if (loggingPanel.activeSelf)
            {
                UpdateDisplays();
            }
        }

        public void ClosePanel()
        {
            loggingPanel.SetActive(false);
        }

        public void AddMessage(string message, bool isInventoryMessage = false)
        {
            List<string> targetList = isInventoryMessage ? inventoryList : messagesList;
            targetList.Insert(0, $"[{System.DateTime.Now:HH:mm:ss}] {message}");
            
            if (targetList.Count > MAX_MESSAGES)
            {
                targetList.RemoveAt(targetList.Count - 1);
            }

            if (loggingPanel.activeSelf)
            {
                UpdateDisplays();
            }
        }

    private void ShowTab(GameObject tabContent)
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
        if (tabContent == messagesContent)
            selectedButton = messagesTabButton;
        else if (tabContent == inventoryContent)
            selectedButton = inventoryTabButton;
        else if (tabContent == tipsContent)  // Add this
            selectedButton = tipsTabButton;

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

        messagesContent.SetActive(false);
        inventoryContent.SetActive(false);
        tipsContent.SetActive(false);  // Add this
        tabContent.SetActive(true);
        UpdateDisplays();
    }

        private System.Collections.IEnumerator RestoreScrollPosition()
        {
            yield return null; // Wait for next frame after content is updated
            
            if (messagesContent.activeSelf && messagesScrollRect != null)
                messagesScrollRect.verticalNormalizedPosition = savedMessagesScrollPosition;
            if (inventoryContent.activeSelf && inventoryScrollRect != null)
                inventoryScrollRect.verticalNormalizedPosition = savedInventoryScrollPosition;
        }


        private void UpdateDisplays()
        {
            // Save current scroll positions
            if (messagesContent.activeSelf && messagesScrollRect != null)
                savedMessagesScrollPosition = messagesScrollRect.verticalNormalizedPosition;
            if (inventoryContent.activeSelf && inventoryScrollRect != null)
                savedInventoryScrollPosition = inventoryScrollRect.verticalNormalizedPosition;

            // Update the text
            messagesText.text = string.Join("\n", messagesList);
            inventoryText.text = string.Join("\n", inventoryList);

            // Restore scroll positions
            StartCoroutine(RestoreScrollPosition());
        }
    }
}