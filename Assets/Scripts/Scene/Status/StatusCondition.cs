using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusCondition : ListItem ,IListViewItem 
{
    [SerializeField] private StateInfoComponent stateInfoComponent;

    private StateInfo _stateInfo; 
    public void SetData(StateInfo stateInfo,int index){
        _stateInfo = stateInfo;
        SetIndex(index);
    }

    public void SetCallHandler(System.Action<int> handler)
    {
        if (_stateInfo == null) return;
        clickButton.onClick.AddListener(() => handler(Index));
    }

    public void UpdateViewItem()
    {
        if (_stateInfo == null) return;
        stateInfoComponent.UpdateInfo(_stateInfo);
    }
}
