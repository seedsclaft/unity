using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;

public class RuleHelp : ListItem ,IListViewItem 
{
    [SerializeField] private TextMeshProUGUI commandName;

    private string _data; 
    public void SetData(string data,int index){
        _data = data;
        SetIndex(index);
        UpdateViewItem();
    }

    public void UpdateViewItem()
    {
        if (_data == null) return;
        commandName.text = _data;
    }
}
