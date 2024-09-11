using UnityEngine;
using UnityEngine.EventSystems;

public class ItemHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string itemName;
    public MessageManager messageManager;

    public void OnPointerEnter(PointerEventData eventData)
    {
        messageManager.ShowMessage(itemName);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        messageManager.HideMessage();
    }
}