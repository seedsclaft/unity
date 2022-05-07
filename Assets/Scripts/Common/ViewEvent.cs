using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ViewEvent
{
    public MainMenu.CommandType commandType;
    public object templete;

    public ViewEvent(MainMenu.CommandType type)
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
    void InputHandler(InputKeyType keyType);
}

abstract public class ListItem : MonoBehaviour
{    
    public Button clickButton;
    private Color _normalColor;
    private Color _selectedColor;
    private Color _disableColor;
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
        if (clickButton == null) return;
        ColorBlock cb = clickButton.colors;
        cb.normalColor = _selectedColor;
        clickButton.colors = cb;
    }
    
    public void SetUnSelect()
    {
        if (clickButton == null) return;
        ColorBlock cb = clickButton.colors;
        cb.normalColor = _normalColor;
        clickButton.colors = cb;
    }
}