using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ViewEvent
{
    public Base.CommandType commandType;
    public object template;

    public ViewEvent(Base.CommandType type)
    {
        commandType = type;
    }
    
}


public interface IClickHandlerEvent
{
    void ClickHandler();
}

public interface IListViewItem
{
    void UpdateViewItem();
}

public interface IInputHandlerEvent
{
    void InputHandler(InputKeyType keyType,bool pressed);
    void MouseCancelHandler();
}

abstract public class ListItem : MonoBehaviour
{    
    public Button clickButton;
    private Color _normalColor;
    private Color _selectedColor;
    private Color _disableColor;
    private int _index;
    public int Index{get {return _index;}}
    private ListData _listData;
    public ListData ListData => _listData;
    public void SetListData(ListData listData,int index)
    {
        _listData = listData;
        _index = index;
    }
    [SerializeField] private GameObject cursor;
    public GameObject Cursor { get {return cursor;}}
    [SerializeField] private GameObject disable;
    public GameObject Disable{ get {return disable;}}

    private bool _addListenHandler = false;
    public void SetAddListenHandler()
    {
        _addListenHandler = true;
    }
    public void Awake()
    {
        InitButtonColors();
    }

    public void InitButtonColors()
    {
        if (clickButton == null) return;
        ColorBlock cb = clickButton.colors;
        _normalColor = new Color(cb.normalColor.r,cb.normalColor.g,cb.normalColor.b,cb.normalColor.a);
        _selectedColor = new Color(cb.selectedColor.r,cb.selectedColor.g,cb.selectedColor.b,cb.selectedColor.a);
        _disableColor = new Color(cb.disabledColor.r,cb.disabledColor.g,cb.disabledColor.b,cb.disabledColor.a);
    }

    public void SetSelect()
    {
        if (cursor == null) return;
        if (disable != null && disable.activeSelf) return;
        cursor.SetActive(true);
    }
    
    public void SetUnSelect()
    {
        if (cursor == null) return;
        cursor.SetActive(false);
    }

    public void SetIndex(int index)
    {
        _index = index;
    }
    
    public void SetSelectHandler(System.Action<int> handler){
        if (clickButton == null)
        {
            return;
        }
        if (_addListenHandler)
        {
            return;
        }
		var enterListener = clickButton.gameObject.AddComponent<ContentEnterListener>();
        enterListener.SetEnterEvent(() => 
        {
            //if (disable == null || disable.activeSelf == false)
            //{
                handler(_index);
            //}
        });
    }

    public void SetCallHandler(System.Action handler)
    {        
        if (clickButton == null)
        {
            return;
        }
        if (_addListenHandler)
        {
            return;
        }
        clickButton.onClick.AddListener(() => handler());
    }
}