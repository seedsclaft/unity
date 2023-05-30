using System.Collections;
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
        bool isAbort = false;
        var stageEvents = _model.StageEvents(eventTiming);
        if (stageEvents.Count > 0)
        {
            for (int i = 0;i < stageEvents.Count;i++)
            {
                if (stageEvents[i].Type == StageEventType.AdvStart)
                {
                    AdvCallInfo advInfo = new AdvCallInfo();
                    advInfo.SetLabel(_model.GetAdvFile(stageEvents[i].Param));
                    advInfo.SetCallEvent(() => {                
                        if (endCall != null) endCall();
                    });
                    _view.CommandCallAdv(advInfo);
                    _model.AddEventReadFlag(stageEvents[i]);
                    isAbort = true;
                    break;
                }
                if (stageEvents[i].Type == StageEventType.SelectActorAdvStart)
                {
                    AdvCallInfo advInfo = new AdvCallInfo();
                    advInfo.SetLabel(_model.GetAdvFile(stageEvents[i].Param + selectActorId));
                    advInfo.SetCallEvent(() => {                
                        if (endCall != null) endCall();
                    });
                    _view.CommandCallAdv(advInfo);
                    _model.AddEventReadFlag(stageEvents[i]);
                    isAbort = true;
                    break;
                }
                if (stageEvents[i].Type == StageEventType.RouteSelectEvent)
                {
                    AdvCallInfo advInfo = new AdvCallInfo();
                    advInfo.SetLabel(_model.GetAdvFile(stageEvents[i].Param + GameSystem.CurrentData.CurrentStage.RouteSelect));
                    advInfo.SetCallEvent(() => {                
                        if (endCall != null) endCall();
                    });
                    _view.CommandCallAdv(advInfo);
                    _model.AddEventReadFlag(stageEvents[i]);
                    isAbort = true;
                    break;
                }
            }
        }
        return isAbort;
    }
}
