using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleActorListItem : ListItem ,IListViewItem
{
    [SerializeField] private BattlerInfoComp component;
    private BattlerInfo _data; 


    public void SetData(BattlerInfo data){
        _data = data;
    }

    public void SetCallHandler(System.Action<BattlerInfo> handler)
    {
        if (_data == null) return;
        clickButton.onClick.AddListener(() => handler(_data));
    }

    public void UpdateViewItem()
    {
        if (_data == null) return;
        component.UpdateInfo(_data);
    }
}
