using UnityEngine;
using UnityEngine.UI;

public class MessageManager : MonoBehaviour
{
    public Canvas messageCanvas;
    public Text messageText;
    private float messageTimer = 0f;
    public float messageDuration = 3f;

    void Start()
    {
        // Ensure the canvas is hidden at start
        messageCanvas.enabled = false;
    }

    void Update()
    {
        if (messageTimer > 0)
        {
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0)
            {
                HideMessage();
            }
        }
    }

    public void ShowMessage(string message)
    {
        messageText.text = message;
        messageCanvas.enabled = true;
        messageTimer = messageDuration;
    }

    public void HideMessage()
    {
        messageCanvas.enabled = false;
        messageTimer = 0f;
    }
}