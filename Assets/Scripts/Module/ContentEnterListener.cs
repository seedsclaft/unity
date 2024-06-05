using UnityEngine;
using UnityEngine.EventSystems;

public class ContentEnterListener : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private System.Action _enterAction = null;
    private System.Action _exitAction = null;
    public void OnPointerEnter(PointerEventData eventData)
    {
        _enterAction?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _exitAction?.Invoke();
    }

    public void SetEnterEvent(System.Action enterAction)
    {
        _enterAction = enterAction;
    }

    public void SetExitEvent(System.Action enterAction)
    {
        _exitAction = enterAction;
    }
}