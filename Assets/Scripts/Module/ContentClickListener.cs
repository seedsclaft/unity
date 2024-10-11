using UnityEngine;
using UnityEngine.EventSystems;

public class ContentClickListener : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private System.Action _clickAction = null;
    private bool _clicked = false;
    
    public void OnPointerDown(PointerEventData eventData) 
    {
        _clicked = true;
        _clickAction?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData) 
    {
        _clicked = false;
    }

    public void SetClickEvent(System.Action clickAction)
    {
        _clickAction = clickAction;
    }
}
