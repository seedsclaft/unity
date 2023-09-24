﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ViewEvent
{
    public Base.CommandType commandType;
    public object templete;

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
    [SerializeField] private GameObject cursor;
    public GameObject Cursor { get {return cursor;}}
    [SerializeField] private GameObject disable;
    public GameObject Disable{ get {return disable;}}
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
		ContentEnterListener enterListener = clickButton.gameObject.AddComponent<ContentEnterListener> ();
        enterListener.SetEnterEvent(() => 
        {
            if (disable == null || disable.activeSelf == false)
            {
                handler(_index);
            }
        });
    }
}