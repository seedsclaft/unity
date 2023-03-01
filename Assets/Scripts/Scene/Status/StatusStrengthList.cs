using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;

public class StatusStrengthList : ListWindow , IInputHandlerEvent
{
    [SerializeField] private int rows = 0;
    [SerializeField] private int cols = 0;

    [SerializeField] private TextMeshProUGUI remainSp;
    [SerializeField] private TacticsCommandList tacticsCommandList;

    public int selectIndex{
        get {return Index;}
    }

    public void Initialize(ActorInfo actorInfo ,System.Action<int> plusEvent,System.Action<int> minusEvent)
    {
        InitializeListView(rows);
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var statusStrength = ObjectList[i].GetComponent<StatusStrength>();
            statusStrength.SetData(actorInfo,i);
            //statusStrength.SetCallHandler(callEvent);
            statusStrength.SetPlusHandler(plusEvent);
            statusStrength.SetMinusHandler(minusEvent);
            statusStrength.SetSelectHandler((data) => UpdateSelectIndex(data));
        }
        UpdateAllItems();
        UpdateSelectIndex(0);
    }

    public void InitializeConfirm(List<SystemData.MenuCommandData> confirmCommands ,System.Action<TacticsComandType> callEvent)
    {
        tacticsCommandList.Initialize(confirmCommands,callEvent);
    }

    public void Refresh(int sp)
    {
        remainSp.text = sp.ToString();
        UpdateAllItems();
    }

    public override void UpdateHelpWindow(){
        if (_helpWindow != null)
        {
            //_helpWindow.SetHelpText(_data[Index].Help);
        }
    }
}
