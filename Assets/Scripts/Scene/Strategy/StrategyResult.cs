using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StrategyResult : ListItem ,IListViewItem  
{   
    [SerializeField] private ActorInfoComponent component;
    private GetItemInfo _data; 


    public void SetData(GetItemInfo data){
        _data = data;
    }

    public void SetCallHandler(System.Action<GetItemInfo> handler)
    {
        if (_data == null) return;
        //clickButton.onClick.AddListener(() => handler(_data));
    }

    public void UpdateViewItem()
    {
        if (_data == null) return;
        //component.UpdateInfo(_data);
    }
}
