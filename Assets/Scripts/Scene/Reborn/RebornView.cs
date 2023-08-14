using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Reborn;

public class RebornView : BaseView
{
    [SerializeField] private RebornActorList actorInfoList = null;
    public int ActorInfoListIndex => actorInfoList.Index;

    [SerializeField] private RebornSkillList rebornSkillList = null;    
    [SerializeField] private ActorInfoComponent actorInfoComponent = null; 
    private new System.Action<RebornViewEvent> _commandData = null;

    public override void Initialize() 
    {
        base.Initialize();
        InitializeInput();
        new RebornPresenter(this);
    }


    public void SetEvent(System.Action<RebornViewEvent> commandData)
    {
        _commandData = commandData;
    }
    
    public void SetBackEvent(System.Action backEvent)
    {
        CreateBackCommand(() => 
        {    
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            if (backEvent != null) backEvent();
        });
        SetActiveBack(true);
    }

    public void SetActorList(List<ActorInfo> actorInfos,List<int> disableIndexs) 
    {
        actorInfoList.Initialize(actorInfos,disableIndexs,(a) => CallDecideActor(a),() => CallCancelActor(),() => CallUpdate());
        actorInfoList.Refresh();
        SetInputHandler(actorInfoList.GetComponent<IInputHandlerEvent>());
        actorInfoList.Activate();
    }

    private void CallDecideActor(int index)
    {
        var eventData = new RebornViewEvent(CommandType.DecideActor);
        eventData.templete = index;
        _commandData(eventData);
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

    public void UpdateActor(ActorInfo actorInfo)
    {
        actorInfoComponent.Clear();
        actorInfoComponent.UpdateInfo(actorInfo,null);
        rebornSkillList.Initialize(actorInfo.RebornSkillInfos);
        rebornSkillList.Refresh();
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
        Back = 4
    }
}
public class RebornViewEvent
{
    public Reborn.CommandType commandType;
    public object templete;

    public RebornViewEvent(Reborn.CommandType type)
    {
        commandType = type;
    }
}