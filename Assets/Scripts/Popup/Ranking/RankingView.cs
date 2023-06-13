using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ranking;

public class RankingView : BaseView
{

    [SerializeField] private RankingInfoList rankingInfoList = null;
    private new System.Action<RankingViewEvent> _commandData = null;

    public override void Initialize() 
    {
        base.Initialize();
        InitializeInput();
        new RankingPresenter(this);
    }


    public void SetEvent(System.Action<RankingViewEvent> commandData)
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

    public void SetRankingInfo(List<RankingInfo> rankingInfo) 
    {
        rankingInfoList.Initialize(rankingInfo,CallCancel);
    }

    private void CallCancel()
    {

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
    public object templete;

    public RankingViewEvent(Ranking.CommandType type)
    {
        commandType = type;
    }
}
