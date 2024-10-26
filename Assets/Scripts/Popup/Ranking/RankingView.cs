using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ranking;

namespace Ryneus
{
    public class RankingView : BaseView
    {
        [SerializeField] private BaseList rankingInfoList = null;
        private new System.Action<RankingViewEvent> _commandData = null;

        public override void Initialize() 
        {
            base.Initialize();
            rankingInfoList.Initialize();
            new RankingPresenter(this);
            rankingInfoList.SetInputHandler(InputKeyType.Cancel,() => BackEvent());
            SetInputHandler(rankingInfoList.GetComponent<IInputHandlerEvent>());
        }

        public void SetEvent(System.Action<RankingViewEvent> commandData)
        {
            _commandData = commandData;
        }

        public void SetRankingViewInfo(RankingViewInfo rankingViewInfo)
        {
            var eventData = new RankingViewEvent(CommandType.RankingOpen)
            {
                template = rankingViewInfo.StageId
            };
            _commandData(eventData);
        }

        public void SetRankingInfo(List<ListData> rankingInfo) 
        {
            rankingInfoList.SetData(rankingInfo,true,() => 
            {
                for (int i = 0; i < rankingInfoList.ItemPrefabList.Count;i++)
                {
                    var rankingInfoComponent = rankingInfoList.ItemPrefabList[i].GetComponent<RankingInfoComponent>();
                    rankingInfoComponent.SetDetailActor((a) => 
                    {
                        CallDetail(a);
                    });
                }
            });
        }

        private void CallDetail(List<ActorInfo> actorInfos)
        {
            var eventData = new RankingViewEvent(CommandType.Detail)
            {
                template = actorInfos
            };
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
    public CommandType commandType;
    public object template;

    public RankingViewEvent(CommandType type)
    {
        commandType = type;
    }
}

public class RankingViewInfo
{
    public int StageId;
    public System.Action EndEvent;
}