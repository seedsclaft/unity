using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ContentDragListener : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{  
    private Vector2 _startPos;
    private System.Action<int,int> _dragMoveAction;
    private System.Action _dragEndAction;
    private bool _endDrag;
    public void OnBeginDrag(PointerEventData eventData) 
    {
        _endDrag = false;
        _startPos = eventData.position;
    }

    public void OnDrag(PointerEventData eventData) 
    {
        SendDragPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData) 
    {
        if (_dragEndAction != null)
        {
            _dragEndAction();
        }
    }

    private void SendDragPosition(PointerEventData eventData)
    {
        if (_endDrag)
        {
            return;
        }
        if (_dragMoveAction != null)
        {
            var endPosition = eventData.position;
            var posx = (float)(_startPos.x - endPosition.x) / (float)Screen.width;
            var posy = (float)(_startPos.y - endPosition.y) / (float)Screen.height;
            _dragMoveAction((int)(posx * 100),(int)(posy * 100));
        }
    }

    public void SetDragMoveEvent(System.Action<int,int> dragMoveAction)
    {
        _dragMoveAction = dragMoveAction;
    }
    
    public void SetDragEndEvent(System.Action dragEndAction)
    {
        _dragEndAction = dragEndAction;
    }

    public void OnEndDrag()
    {
        _endDrag = true;
    }
}
