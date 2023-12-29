using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlcanaList;

public class AlcanaListView : BaseView
{
    [SerializeField] private BaseList alcanaList = null;
    [SerializeField] private SkillInfoComponent skillInfoComponent = null;
    private new System.Action<AlcanaListViewEvent> _commandData = null;
    private System.Action<int> _callEvent = null;
    
    public override void Initialize() 
    {
        base.Initialize();
        new AlcanaListPresenter(this);
        alcanaList.SetInputHandler(InputKeyType.Cancel,() => BackEvent());
        alcanaList.SetInputHandler(InputKeyType.Decide,() => {});
        alcanaList.SetSelectedHandler(() => {
            if (alcanaList.ListData != null)
            {
                var data = (SkillInfo)alcanaList.ListData.Data;
                skillInfoComponent.UpdateSkillInfo(data);
            }
        });
        SetInputHandler(alcanaList.GetComponent<IInputHandlerEvent>());
    }
    
    public void SetEvent(System.Action<AlcanaListViewEvent> commandData)
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

    public void SetAlcanaList(List<ListData> alcanaLists)
    {
        alcanaList.SetData(alcanaLists);
        alcanaList.Activate();
    }
}

namespace AlcanaList
{
    public enum CommandType
    {
        None = 0,
    }
}

public class AlcanaListViewEvent
{
    public AlcanaList.CommandType commandType;
    public object template;

    public AlcanaListViewEvent(AlcanaList.CommandType type)
    {
        commandType = type;
    }
}