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
    [SerializeField] private TextMeshProUGUI remainNuminous;
    [SerializeField] private TacticsCommandList tacticsCommandList;
    private System.Action<TacticsComandType> _confirmEvent = null;
    private ActorInfo _actorInfo = null;
    private List<StatusStrength> _statusStrengths = new List<StatusStrength>();

    public void Initialize(System.Action<int> plusEvent,System.Action<int> minusEvent,System.Action resetEvent)
    {
        _statusStrengths.Clear();
        InitializeListView(rows);
        for (int i = 0; i < rows;i++)
        {
            StatusStrength statusStrength = ObjectList[i].GetComponent<StatusStrength>();
            statusStrength.SetPlusHandler(plusEvent);
            statusStrength.SetMinusHandler(minusEvent);
            statusStrength.SetSelectHandler((data) => UpdateSelectIndex(data));
            _statusStrengths.Add(statusStrength);
        }
        SetInputHandler((a) => CallInputHandler(a,plusEvent,minusEvent,resetEvent));
        UpdateSelectIndex(-1);
    }

    public void Refresh(ActorInfo actorInfo)
    {
        _actorInfo = actorInfo;
        for (int i = 0; i < ObjectList.Count;i++)
        {
            StatusStrength statusStrength = ObjectList[i].GetComponent<StatusStrength>();
            statusStrength.SetData(actorInfo,i);
        }
        UpdateAllItems();
        if (Index == -1) UpdateSelectIndex(0);
    }

    public void InitializeConfirm(List<SystemData.MenuCommandData> confirmCommands ,System.Action<TacticsComandType> callEvent)
    {
        tacticsCommandList.Initialize(callEvent);
        tacticsCommandList.Refresh(confirmCommands);
        tacticsCommandList.UpdateSelectIndex(-1);
        _confirmEvent = callEvent;
    }

    public void RefreshCostInfo(int sp,int numinous)
    {
        remainSp.text = sp.ToString();
        remainNuminous.text = numinous.ToString();
        UpdateAllItems();
    }

    public override void UpdateHelpWindow(){
        if (_helpWindow != null)
        {
            //_helpWindow.SetHelpText(_data[Index].Help);
        }
    }
    
    private void CallInputHandler(InputKeyType keyType,System.Action<int> plusEvent,System.Action<int> minusEvent,System.Action resetEvent)
    {
        if (keyType == InputKeyType.Decide)
        {
            if (Index == -1)
            {
                _confirmEvent((TacticsComandType)tacticsCommandList.Index);
            }
        }
        if (keyType == InputKeyType.Cancel)
        {
            _confirmEvent(TacticsComandType.Train);
        }
        if (keyType == InputKeyType.Down)
        {
            if (Index == 0)
            {
                UpdateSelectIndex(-1);
                tacticsCommandList.UpdateSelectIndex(0);
            }
        }
        if (keyType == InputKeyType.Up)
        {
            if (Index == _statusStrengths.Count-1)
            {
                UpdateSelectIndex(_statusStrengths.Count-1);
                tacticsCommandList.UpdateSelectIndex(-1);
            }
        }
        if (keyType == InputKeyType.Right)
        {
            if (Index == -1)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Cursor);
                tacticsCommandList.UpdateSelectIndex(1);
            } else{
                plusEvent(_statusStrengths[Index].listIndex());
            }
        }
        if (keyType == InputKeyType.Left)
        {
            if (Index == -1)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Cursor);
                tacticsCommandList.UpdateSelectIndex(0);
            } else{
                minusEvent(_statusStrengths[Index].listIndex());
            }
        }
        if (keyType == InputKeyType.Option1)
        {
            resetEvent();
        }
    }
}
