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
        actorInfoList.Initialize(actorInfos,disableIndexs);
        actorInfoList.SetInputHandler(InputKeyType.Decide,() => CallDecideActor(disableIndexs));
        actorInfoList.SetInputHandler(InputKeyType.Cancel,() => CallCancelActor());
        actorInfoList.SetInputHandler(InputKeyType.Down,() => CallUpdate());
        actorInfoList.SetInputHandler(InputKeyType.Up,() => CallUpdate());
        actorInfoList.Refresh();
        SetInputHandler(actorInfoList.GetComponent<IInputHandlerEvent>());
        actorInfoList.Activate();
    }

    private void CallDecideActor(List<int> disableIndexs)
    {
        var eventData = new RebornViewEvent(CommandType.DecideActor);
        if (actorInfoList.Index > -1 &&!disableIndexs.Contains(actorInfoList.Index))
        {
            eventData.templete = actorInfoList.Index;
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

    public void UpdateActor(ActorInfo actorInfo)
    {
        actorInfoComponent.Clear();
        actorInfoComponent.UpdateInfo(actorInfo,null);
        rebornSkillList.Initialize(actorInfo.RebornSkillInfos);
        rebornSkillList.SetInputHandler(InputKeyType.SideLeft1,() => OnPageUpRebornSkill());
        rebornSkillList.SetInputHandler(InputKeyType.SideRight1,() => OnPageDownRebornSkill());
        SetInputHandler(rebornSkillList.GetComponent<IInputHandlerEvent>());
        rebornSkillList.Refresh();
    }
    
    private void OnPageUpRebornSkill()
    {
        if (rebornSkillList.Data != null && rebornSkillList.Data.Count < 4) return;
        var margin = 1.0f / (rebornSkillList.Data.Count - 4);

        var value = rebornSkillList.ScrollRect.normalizedPosition.y - margin;
        if ((rebornSkillList.Data.Count - 4) == 0)
        {
            value = 1;
        }
        rebornSkillList.ScrollRect.normalizedPosition = new Vector2(0,value);
        if (rebornSkillList.ScrollRect.normalizedPosition.y < 0)
        {
            rebornSkillList.ScrollRect.normalizedPosition = new Vector2(0,0);
        }
    }

    private void OnPageDownRebornSkill()
    {
        if (rebornSkillList.Data != null && rebornSkillList.Data.Count < 4) return;
        var margin = 1.0f / (rebornSkillList.Data.Count - 4);

        var value = rebornSkillList.ScrollRect.normalizedPosition.y + margin;
        if ((rebornSkillList.Data.Count - 4) == 0)
        {
            value = 0;
        }
        rebornSkillList.ScrollRect.normalizedPosition = new Vector2(0,value);
        if (rebornSkillList.ScrollRect.normalizedPosition.y > 1)
        {
            rebornSkillList.ScrollRect.normalizedPosition = new Vector2(0,1);
        }
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
