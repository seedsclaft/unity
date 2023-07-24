using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ranking;

public class RankingView : BaseView,IInputHandlerEvent
{

    [SerializeField] private ScrollRect scrollRect = null;
    [SerializeField] private RankingInfoList rankingInfoList = null;
    private new System.Action<RankingViewEvent> _commandData = null;

    public override void Initialize() 
    {
        base.Initialize();
        InitializeInput();
        new RankingPresenter(this);
        SetInputHandler(gameObject.GetComponent<IInputHandlerEvent>());
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

    public void InputHandler(InputKeyType keyType)
    {
        if (keyType == InputKeyType.Cancel)
        {
            BackEvent();
        }
        if (keyType == InputKeyType.Down)
        {
            var value = scrollRect.normalizedPosition.y - 0.005f;
            scrollRect.normalizedPosition = new Vector2(0,value);
            if (scrollRect.normalizedPosition.y < 0)
            {
                scrollRect.normalizedPosition = new Vector2(0,0);
            }
        }
        if (keyType == InputKeyType.Up)
        {
            var value = scrollRect.normalizedPosition.y + 0.005f;
            scrollRect.normalizedPosition = new Vector2(0,value);
            if (scrollRect.normalizedPosition.y > 1)
            {
                scrollRect.normalizedPosition = new Vector2(0,1);
            }
        }
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
