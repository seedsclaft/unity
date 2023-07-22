using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePresenter 
{
    private bool _busy = true;
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
        bool isAbort = false;
        int advId = -1;
        var stageEvents = _model.StageEvents(eventTiming);
        if (stageEvents.Count > 0)
        {
            for (int i = 0;i < stageEvents.Count;i++)
            {
                if (stageEvents[i].Type == StageEventType.AdvStart)
                {
                    advId = stageEvents[i].Param;
                    _model.AddEventReadFlag(stageEvents[i]);
                    isAbort = true;
                    break;
                }
                if (stageEvents[i].Type == StageEventType.SelectActorAdvStart)
                {
                    advId = stageEvents[i].Param + selectActorId;
                    _model.AddEventReadFlag(stageEvents[i]);
                    isAbort = true;
                    break;
                }
                if (stageEvents[i].Type == StageEventType.RouteSelectEvent)
                {
                    int route = GameSystem.CurrentData.CurrentStage.RouteSelect;
                    advId = stageEvents[i].Param + route;
                    _model.AddEventReadFlag(stageEvents[i]);
                    isAbort = true;
                    break;
                }
            }
            if (isAbort)
            {
                AdvCallInfo advInfo = new AdvCallInfo();
                advInfo.SetLabel(_model.GetAdvFile(advId));
                advInfo.SetCallEvent(() => {                
                    if (endCall != null) endCall();
                });
                _view.CommandCallAdv(advInfo);
            }
        }
        return isAbort;
    }
    
    public void CommandOption()
    {
        _busy = true;
        _view.CommandCallOption(() => {
            _busy = false;
        });
    }
}
