using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Slot;

public class SlotView : BaseView
{
    [SerializeField] private BaseList slotInfoList = null;
    private new System.Action<SlotViewEvent> _commandData = null;

    public int SlotListIndex => slotInfoList.Index;
    public override void Initialize() 
    {
        base.Initialize();
        InitSlotList();
        new SlotPresenter(this);
    }


    public void SetEvent(System.Action<SlotViewEvent> commandData)
    {
        _commandData = commandData;
    }
    
    public void SetBackEvent()
    {
        SetBackCommand(() => 
        {
            var eventData = new SlotViewEvent(CommandType.Back);
            _commandData(eventData); 
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        });
        ChangeBackCommandActive(true);
    }

    public void InitSlotList() 
    {
        slotInfoList.Initialize();
        SetInputHandler(slotInfoList.GetComponent<IInputHandlerEvent>());
        slotInfoList.SetInputHandler(InputKeyType.Decide,() => CallSlotDecide());
        slotInfoList.SetInputHandler(InputKeyType.Cancel,() => BackEvent());
        slotInfoList.SetInputHandler(InputKeyType.Option1,() => CallSlotStatus());
    }

    public void CommandRefresh(List<ListData> slotInfos)
    {
        slotInfoList.SetData(slotInfos);
        slotInfoList.Activate();
        for (var i = 0;i < slotInfoList.ObjectList.Count;i++)
        {
            var slotParty = slotInfoList.ObjectList[i].GetComponent<SlotParty>();
            slotParty.SetCallInfoHandler((a) => {
                CallSlotStatus(a);
            });
        }
    }
    private void CallSlotDecide()
    {
        var eventData = new SlotViewEvent(CommandType.Decide);
        eventData.template = slotInfoList.Index;
        _commandData(eventData); 
    }

    private void CallSlotStatus(int selectIndex = -1)
    {
        var eventData = new SlotViewEvent(CommandType.Status);
        eventData.template = slotInfoList.Index;
        if (selectIndex != -1)
        {
            eventData.template = selectIndex;
        }
        _commandData(eventData); 
    }
}

namespace Slot
{
    public enum CommandType
    {
        None = 0,
        Decide,
        Status,
        Back,
    }
}
public class SlotViewEvent
{
    public Slot.CommandType commandType;
    public object template;

    public SlotViewEvent(Slot.CommandType type)
    {
        commandType = type;
    }
}
