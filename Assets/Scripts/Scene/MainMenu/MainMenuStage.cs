using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuStage : ListItem ,IListViewItem  
{   
    [SerializeField] private StageInfoComponent component;
    private StageInfo _data;
    public void SetData(StageInfo data,int index){
        _data = data;
        SetIndex(index);
    }

    public void SetCallHandler(System.Action<StageInfo> handler)
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
