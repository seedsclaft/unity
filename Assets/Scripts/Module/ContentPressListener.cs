using UnityEngine;
using UnityEngine.EventSystems;

public class ContentPressListener : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private System.Action _pressAction = null;
    private bool _pressed = false;
    private int _duration = 0;
    
    public void OnPointerDown(PointerEventData eventData) 
    {
        _pressed = true;
    }

    public void OnPointerUp(PointerEventData eventData) 
    {
        _duration = 0;
        _pressed = false;
    }

    public void SetPressEvent(System.Action pressAction)
    {
        _pressAction = pressAction;
    }
    
    void Update() {
        if (_pressed == true) 
        {
            _duration += 1;
        }

        //長押しを判定
        if (_duration > 30) 
        {
            _pressed = false;
            _pressAction?.Invoke();
        }
    }
}
