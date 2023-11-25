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

    public void SetRankingInfo(List<ListData> rankingInfo) 
    {
        rankingInfoList.SetData(rankingInfo);
    }
}

namespace Ranking
{
    public enum CommandType
    {
        None = 0,
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
