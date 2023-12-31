using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SlotSave;

public class SlotSaveView : BaseView
{
    [SerializeField] private BaseList slotInfoList = null;
    [SerializeField] private SlotInfoComponent slotInfoComponent = null;
    private new System.Action<SlotSaveViewEvent> _commandData = null;

    public int SlotListIndex => slotInfoList.Index;

    public override void Initialize() 
    {
        base.Initialize();
        InitSlotList();
        new SlotSavePresenter(this);
    }

    public void InitSlotList() 
    {
        slotInfoList.Initialize();
        SetInputHandler(slotInfoList.GetComponent<IInputHandlerEvent>());
        slotInfoList.SetInputHandler(InputKeyType.Decide,() => CallSlotSave());
        slotInfoList.SetInputHandler(InputKeyType.Cancel,() => BackEvent());
        slotInfoList.SetInputHandler(InputKeyType.Option1,() => CallSlotStatus());
        for (var i = 0;i < slotInfoList.ObjectList.Count;i++)
        {
            var slotParty = slotInfoList.ObjectList[i].GetComponent<SlotParty>();
            slotParty.SetCallInfoHandler((a) => {
                CallSlotStatus(a);
            });
        }
    }

    public void SetEvent(System.Action<SlotSaveViewEvent> commandData)
    {
        _commandData = commandData;
    }
    
    public void SetBackEvent(System.Action backEvent)
    {
        SetBackCommand(() => 
        {    
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            if (backEvent != null) backEvent();
        });
        ChangeBackCommandActive(true);
    }

    public void SetSlotSaveViewInfo(SlotSaveViewInfo slotSaveViewInfo)
    {
        var eventData = new SlotSaveViewEvent(CommandType.SlotSaveOpen);
        eventData.template = slotSaveViewInfo.SlotInfo;
        _commandData(eventData);
    }

    public void SetCurrentSlotInfo(SlotInfo current = null)
    {
        slotInfoComponent.gameObject.SetActive(current != null);
        if (current != null)
        {
            slotInfoComponent.UpdateInfo(current);
        }
    }

    public void CommandRefresh(List<ListData> slotInfos)
    {
        slotInfoList.SetData(slotInfos);
        slotInfoList.Activate();
    }

    private void CallSlotSave()
    {
        var listData = slotInfoList.ListData;
        if (listData != null)
        {
            var data = (SlotInfo)listData.Data;
            var eventData = new SlotSaveViewEvent(CommandType.SlotSave);
            eventData.template = data;
            _commandData(eventData); 
        }
    }

    private void CallSlotStatus(int selectIndex = -1)
    {
        var eventData = new SlotSaveViewEvent(CommandType.Status);
        eventData.template = slotInfoList.Index;
        if (selectIndex != -1)
        {
            eventData.template = selectIndex;
        }
        _commandData(eventData); 
    }
}

namespace SlotSave
{
    public enum CommandType
    {
        None = 0,
        Status = 1,
        SlotSave = 2,
        SlotSaveOpen = 3,
    }
}
public class SlotSaveViewEvent
{
    public SlotSave.CommandType commandType;
    public object template;

    public SlotSaveViewEvent(SlotSave.CommandType type)
    {
        commandType = type;
    }
}


public class SlotSaveViewInfo{
    public SlotInfo SlotInfo;
    public System.Action EndEvent;
}