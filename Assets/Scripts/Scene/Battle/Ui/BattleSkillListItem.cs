using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSkillListItem : ListItem ,IListViewItem
{
    [SerializeField] private SkillInfoComp component;
    private SkillInfo _data; 


    public void SetData(SkillInfo data){
        _data = data;
    }

    public void SetCallHandler(System.Action<SkillInfo> handler)
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
