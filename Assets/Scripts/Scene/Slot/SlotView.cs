using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Slot;

public class SlotView : BaseView
{
    [SerializeField] private BaseList slotInfoList = null;
    private new System.Action<SlotViewEvent> _commandData = null;

    public override void Initialize() 
    {
        base.Initialize();
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

    public void SetSlotInfo(Dictionary<int,SlotInfo> slotInfo) 
    {
        slotInfoList.Initialize();
        SetInputHandler(slotInfoList.GetComponent<IInputHandlerEvent>());
        slotInfoList.SetInputHandler(InputKeyType.Decide,() => CallSlotInfoLock());
        slotInfoList.SetInputHandler(InputKeyType.Cancel,() => BackEvent());
        slotInfoList.SetInputHandler(InputKeyType.Option1,() => CallSlotStatus());
        slotInfoList.Refresh();
        slotInfoList.Activate();
    }

    public void CommandRefresh(List<ListData> slotInfo)
    {
        slotInfoList.SetData(slotInfo);
    }

    private void CallSlotInfoLock()
    {
        var eventData = new SlotViewEvent(CommandType.Lock);
        eventData.template = slotInfoList.Index;
        _commandData(eventData); 
    }
    
    private void CallSlotStatus()
    {
        var eventData = new SlotViewEvent(CommandType.Status);
        eventData.template = slotInfoList.Index;
        _commandData(eventData); 
    }

}

namespace Slot
{
    public enum CommandType
    {
        None = 0,
        Lock,
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
