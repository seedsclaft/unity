using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Slot;

public class SlotView : BaseView
{
    [SerializeField] private BaseList slotInfoList = null;
    [SerializeField] private SlotInfoComponent slotInfoComponent = null;
    private new System.Action<SlotViewEvent> _commandData = null;

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
        slotInfoList.SetInputHandler(InputKeyType.Cancel,() => BackEvent());
        slotInfoList.SetInputHandler(InputKeyType.Option1,() => CallSlotStatus());
    }

    public void CommandRefresh(List<ListData> slotInfos,SlotInfo current = null)
    {
        slotInfoList.SetData(slotInfos);
        slotInfoList.Activate();
        slotInfoComponent.gameObject.SetActive(current != null);
        if (current != null)
        {
            slotInfoComponent.UpdateInfo(current);
        }
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
