using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Reborn;

public class RebornView : BaseView
{
    [SerializeField] private BaseList actorInfoList = null;
    public int ActorInfoListIndex => actorInfoList.Index;

    [SerializeField] private BattleSelectCharacter selectCharacter = null; 
    private new System.Action<RebornViewEvent> _commandData = null;

    public override void Initialize() 
    {
        base.Initialize();
        actorInfoList.Initialize();
        selectCharacter.Initialize();
        SetInputHandler(selectCharacter.GetComponent<IInputHandlerEvent>());
        InitializeSelectCharacter();
        SetUIButton();
        new RebornPresenter(this);
    }

    private void InitializeSelectCharacter()
    {
        selectCharacter.SetInputHandlerAction(InputKeyType.Decide,() => {});
        selectCharacter.SetInputHandlerAction(InputKeyType.Cancel,() => OnClickBack());
        selectCharacter.SetInputHandlerAction(InputKeyType.SideLeft1,() => OnClickLeft());
        selectCharacter.SetInputHandlerAction(InputKeyType.SideRight1,() => OnClickRight());

        SetInputHandler(selectCharacter.MagicList.GetComponent<IInputHandlerEvent>());
        selectCharacter.HideActionList();
        selectCharacter.HideStatus();
    }

    private void SetUIButton()
    {
    }

    public void SetEvent(System.Action<RebornViewEvent> commandData)
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

    public void SetActorList(List<ListData> actorInfos) 
    {
        actorInfoList.SetData(actorInfos);
        actorInfoList.SetInputHandler(InputKeyType.Decide,() => CallDecideActor());
        actorInfoList.SetInputHandler(InputKeyType.Cancel,() => CallCancelActor());
        actorInfoList.SetInputHandler(InputKeyType.Down,() => CallUpdate());
        actorInfoList.SetInputHandler(InputKeyType.Up,() => CallUpdate());
        actorInfoList.SetSelectedHandler(() => CallUpdate());
        SetInputHandler(actorInfoList.GetComponent<IInputHandlerEvent>());
    }

    private void CallDecideActor()
    {
        var listData = actorInfoList.ListData;
        if (listData != null && listData.Enable)
        {
            var data = (ActorInfo)listData.Data;
            var eventData = new RebornViewEvent(CommandType.DecideActor);
            eventData.template = actorInfoList.Index;
            _commandData(eventData);
        }
    }
    
    private void CallCancelActor()
    {
        var eventData = new RebornViewEvent(CommandType.CancelActor);
        _commandData(eventData);
    }

    private void CallUpdate()
    {
        var eventData = new RebornViewEvent(CommandType.UpdateActor);
        _commandData(eventData);
    }

    private void OnClickBack()
    {
        var eventData = new RebornViewEvent(CommandType.Back);
        _commandData(eventData);
    }

    private void OnClickLeft()
    {
        var eventData = new RebornViewEvent(CommandType.LeftActor);
        _commandData(eventData);
    }

    private void OnClickRight()
    {
        var eventData = new RebornViewEvent(CommandType.RightActor);
        _commandData(eventData);
    }

    public void CommandRefreshStatus(List<ListData> skillInfos,ActorInfo actorInfo,List<ActorInfo> party,int lastSelectIndex)
    {
        selectCharacter.SetActiveTab(SelectCharacterTabType.Magic,false);
        selectCharacter.SetActiveTab(SelectCharacterTabType.Condition,false);
        selectCharacter.SetActiveTab(SelectCharacterTabType.Detail,false);
        selectCharacter.ShowActionList();
        selectCharacter.MagicList.Activate();
        //selectCharacter.SetActorThumbOnly(actorInfo);
        selectCharacter.SetActorInfo(actorInfo,party);
        selectCharacter.SetSkillInfos(skillInfos);
        selectCharacter.RefreshAction(lastSelectIndex);
    }
}

namespace Reborn
{
    public enum CommandType
    {
        None = 0,
        DecideActor = 1,
        CancelActor = 2,
        UpdateActor = 3,
        Back = 4,
        LeftActor = 5,
        RightActor = 6,
    }
}
public class RebornViewEvent
{
    public Reborn.CommandType commandType;
    public object template;

    public RebornViewEvent(Reborn.CommandType type)
    {
        commandType = type;
    }
}
