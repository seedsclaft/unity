using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Slot;

public class SlotView : BaseView
{

    [SerializeField] private SlotInfoList slotInfoList = null;
    private new System.Action<SlotViewEvent> _commandData = null;

    public override void Initialize() 
    {
        base.Initialize();
        InitializeInput();
        new SlotPresenter(this);
        SetInputHandler(gameObject.GetComponent<IInputHandlerEvent>());
    }


    public void SetEvent(System.Action<SlotViewEvent> commandData)
    {
        _commandData = commandData;
    }
    
    public void SetBackEvent()
    {
        CreateBackCommand(() => 
        {
            var eventData = new SlotViewEvent(CommandType.Back);
            _commandData(eventData); 
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        });
        SetActiveBack(true);
    }

    public void SetSlotInfo(Dictionary<int,SlotInfo> slotInfo) 
    {
        slotInfoList.Initialize(slotInfo,null);
        SetInputHandler(slotInfoList.GetComponent<IInputHandlerEvent>());
        slotInfoList.SetInputHandler(InputKeyType.Decide,() => CallSlotInfoLock());
        slotInfoList.SetInputHandler(InputKeyType.Cancel,() => BackEvent());
        slotInfoList.SetInputHandler(InputKeyType.Option1,() => CallSlotStatus());
        slotInfoList.Refresh();
        slotInfoList.Activate();
    }

    public void CommandRefresh(Dictionary<int,SlotInfo> slotInfo)
    {
        slotInfoList.SetData(slotInfo);
        slotInfoList.Refresh();
    }

    private void CallSlotInfoLock()
    {
        var eventData = new SlotViewEvent(CommandType.Lock);
        eventData.templete = slotInfoList.Index;
        _commandData(eventData); 
    }
    
    private void CallSlotStatus()
    {
        var eventData = new SlotViewEvent(CommandType.Status);
        eventData.templete = slotInfoList.Index;
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
    public object templete;

    public SlotViewEvent(Slot.CommandType type)
    {
        commandType = type;
    }
}
