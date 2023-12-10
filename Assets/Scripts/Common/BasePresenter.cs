﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePresenter 
{
    private BaseView _view = null;
    public void SetView(BaseView view)
    {
        _view = view;
    }

    private BaseModel _model = null;
    public void SetModel(BaseModel model)
    {
        _model = model;
    }

    public bool CheckAdvStageEvent(EventTiming eventTiming,System.Action endCall,int selectActorId = 0)
    {
        var isAbort = false;
        var advId = -1;
        var stageEvents = _model.StageEvents(eventTiming);
        foreach (var stageEvent in stageEvents)
        {
            if (stageEvent.Type == StageEventType.AdvStart)
            {
                advId = stageEvent.Param;
                _model.AddEventReadFlag(stageEvent);
                isAbort = true;
                break;
            }
            if (stageEvent.Type == StageEventType.SelectActorAdvStart)
            {
                advId = stageEvent.Param + selectActorId;
                _model.AddEventReadFlag(stageEvent);
                isAbort = true;
                break;
            }
            if (stageEvent.Type == StageEventType.RouteSelectEvent)
            {
                int route = GameSystem.CurrentData.CurrentStage.RouteSelect;
                advId = stageEvent.Param + route;
                _model.AddEventReadFlag(stageEvent);
                isAbort = true;
                break;
            }
        }
        if (isAbort)
        {
            var advInfo = new AdvCallInfo();
            advInfo.SetLabel(_model.GetAdvFile(advId));
            advInfo.SetCallEvent(() => {                
                if (endCall != null) endCall();
            });
            _view.CommandCallAdv(advInfo);
        }
        return isAbort;
    }

    public bool CheckRebornEvent(EventTiming eventTiming,System.Action endCall)
    {
        var isReborn = false;
        var stageEvents = _model.StageEvents(eventTiming);
        foreach (var stageEvent in stageEvents)
        {
            if (stageEvent.Type == StageEventType.RebornSkillEffect)
            {
                _model.AddEventReadFlag(stageEvent);
                isReborn = true;
                break;
            }
        }
        if (isReborn)
        {
            if (endCall != null) endCall();
        }
        return isReborn;
    }
}
