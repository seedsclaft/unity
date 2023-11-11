using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StrategyStrength : ListItem ,IListViewItem 
{
    [SerializeField] private StrengthComponent strengthComponent;

    public void UpdateViewItem()
    {
        if (ListData == null) return;
        var data = (ActorInfo)ListData.Data;
        strengthComponent.UpdateInfo(data,(StatusParamType)Index);
    }
}
