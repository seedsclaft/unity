using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ranking;

public class RankingView : BaseView
{
    [SerializeField] private BaseList rankingInfoList = null;
    private new System.Action<RankingViewEvent> _commandData = null;

    public override void Initialize() 
    {
        base.Initialize();
        new RankingPresenter(this);
        rankingInfoList.SetInputHandler(InputKeyType.Cancel,() => BackEvent());
        SetInputHandler(rankingInfoList.GetComponent<IInputHandlerEvent>());
    }


    public void SetEvent(System.Action<RankingViewEvent> commandData)
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

    public void SetRankingViewInfo(RankingViewInfo rankingViewInfo)
    {
        var eventData = new RankingViewEvent(CommandType.RankingOpen);
        eventData.template = rankingViewInfo.StageId;
        _commandData(eventData);
    }

    public void SetRankingInfo(List<ListData> rankingInfo) 
    {
        foreach (var listDate in rankingInfo)
        {
            var data = (RankingInfo)listDate.Data;
            data.DetailEvent = CallDetail;
        }
        rankingInfoList.SetData(rankingInfo);
    }

    private void CallDetail(int index)
    {
        var listData = rankingInfoList.ListData;
        if (listData != null && listData.Enable)
        {
            var eventData = new RankingViewEvent(CommandType.Detail);
            eventData.template = index;
            _commandData(eventData);
        }
    }
}

namespace Ranking
{
    public enum CommandType
    {
        None = 0,
        RankingOpen = 1,
        Detail = 2,
    }
}
public class RankingViewEvent
{
    public Ranking.CommandType commandType;
    public object template;

    public RankingViewEvent(Ranking.CommandType type)
    {
        commandType = type;
    }
}

public class RankingViewInfo{
    public int StageId;
    public System.Action EndEvent;
}