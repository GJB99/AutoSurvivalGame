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
        public Button messagesTabButton;
        public Button inventoryTabButton;
        public TextMeshProUGUI messagesText;
        public TextMeshProUGUI inventoryText;

        private List<string> messagesList = new List<string>();
        private List<string> inventoryList = new List<string>();
        private const int MAX_MESSAGES = 10000;
        private ScrollRect messagesScrollRect;
        private ScrollRect inventoryScrollRect;
        private float savedMessagesScrollPosition = 1f;
        private float savedInventoryScrollPosition = 1f;

        void Start()
        {
            loggingPanel.SetActive(false);
            messagesTabButton.onClick.AddListener(() => ShowTab(messagesContent));
            inventoryTabButton.onClick.AddListener(() => ShowTab(inventoryContent));
            
            // Get ScrollRect components
            messagesScrollRect = messagesContent.GetComponentInParent<ScrollRect>();
            inventoryScrollRect = inventoryContent.GetComponentInParent<ScrollRect>();
            
            ShowTab(messagesContent); // Default to messages tab
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
            messagesContent.SetActive(false);
            inventoryContent.SetActive(false);
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