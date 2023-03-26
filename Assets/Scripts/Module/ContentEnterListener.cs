using UnityEngine;
using UnityEngine.EventSystems;

public class ContentEnterListener : MonoBehaviour, IPointerEnterHandler 
{
    private System.Action _enterAction = null;
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_enterAction != null) _enterAction();
    }

    public void SetEnterEvent(System.Action enterAction)
    {
        _enterAction = enterAction;
    }
}