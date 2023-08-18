using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;

public class StrategyStrengthList : ListWindow , IInputHandlerEvent
{
    [SerializeField] private int rows = 0;

    private ActorInfo _actorInfo = null;
    private List<StrategyStrength> _statusStrengths = new List<StrategyStrength>();

    public void Initialize(System.Action callEvent)
    {
        _statusStrengths.Clear();
        InitializeListView(rows);
        for (int i = 0; i < rows;i++)
        {
            StrategyStrength statusStrength = ObjectList[i].GetComponent<StrategyStrength>();
            _statusStrengths.Add(statusStrength);
        }
        SetInputCallHandler((a) => CallInputHandler(a,callEvent));
        UpdateSelectIndex(-1);
    }

    public void Refresh(ActorInfo actorInfo)
    {
        _actorInfo = actorInfo;
        for (int i = 0; i < ObjectList.Count;i++)
        {
            StrategyStrength statusStrength = ObjectList[i].GetComponent<StrategyStrength>();
            statusStrength.SetData(actorInfo,i);
        }
        UpdateAllItems();
    }



    public override void UpdateHelpWindow(){
        if (_helpWindow != null)
        {
            //_helpWindow.SetHelpText(_data[Index].Help);
        }
    }
    
    private void CallInputHandler(InputKeyType keyType,System.Action callEvent)
    {
        if (keyType == InputKeyType.Decide)
        {
            callEvent();
        }
    }
}
