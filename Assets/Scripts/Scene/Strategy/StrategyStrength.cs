using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StrategyStrength : ListItem ,IListViewItem 
{
    [SerializeField] private StrengthComponent strengthComponent;

    private ActorInfo _data;
    public void SetData(ActorInfo data,int index){
        _data = data;
        SetIndex(index);
    }

    public void UpdateViewItem()
    {
        if (_data == null) return;
        strengthComponent.UpdateInfo(_data,Index);
    }
}
