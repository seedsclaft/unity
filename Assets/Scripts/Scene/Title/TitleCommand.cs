using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TitleCommand : ListItem ,IListViewItem 
{
    [SerializeField] private TextMeshProUGUI commandName;

    private SystemData.MenuCommandData _data; 
    public void SetData(SystemData.MenuCommandData data,int index){
        _data = data;
        SetIndex(index);
    }

    public void SetCallHandler(System.Action<TitleComandType> handler)
    {
        if (_data == null) return;
        clickButton.onClick.AddListener(() => handler((TitleComandType)Index));
    }

    public void UpdateViewItem()
    {
        if (_data == null) return;
        commandName.text = _data.Name;
    }
}
