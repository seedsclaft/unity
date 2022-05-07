using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuCommand : ListItem ,IListViewItem ,IClickHandlerEvent 
{
    [SerializeField] private Text commandName;

    private SystemData.MenuCommandData _data; 

    public void SetData(SystemData.MenuCommandData data){
        _data = data;
    }

    public void SetCallHandler(System.Action<MenuComandType> handler)
    {
        if (_data == null) return;
        clickButton.onClick.AddListener(() => handler((MenuComandType)_data.Id));
    }

    public void UpdateViewItem()
    {
        if (_data == null) return;
        commandName.text = _data.NameTextId.ToString();
    }

    public void ClickHandler()
    {
    }
}
