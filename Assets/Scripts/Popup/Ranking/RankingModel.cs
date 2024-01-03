using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;

public class RankingModel : BaseModel
{
    private int _stageId = 0;
    public async void RankingInfos(int stageId,Action<List<ListData>> endEvent)
    {
        _stageId = stageId;
#if (UNITY_WEBGL || UNITY_ANDROID) //&& !UNITY_EDITOR
        if (TempData.TempRankingData.ContainsKey(stageId) == false)
        {
            FirebaseController.Instance.ReadRankingData(stageId,RankingTypeText(DataSystem.FindStage(stageId).RankingStage));
            await UniTask.WaitUntil(() => FirebaseController.IsBusy == false);
            TempData.SetRankingInfo(stageId,FirebaseController.RankingInfos);
        }
        if (endEvent != null) 
        {
            endEvent(MakeListData(TempData.TempRankingData[stageId]));
        }
#endif
    }

    public void MakeDetailPartyInfo(int listIndex)
    {
        var rankingInfo = TempData.TempRankingData[_stageId][listIndex];
        CurrentStageData.ClearActors();
        PartyInfo.InitActors();
        foreach (var actorInfo in rankingInfo.ActorInfos)
        {
            PartyInfo.AddActor(actorInfo.ActorId);
            CurrentStageData.AddActor(actorInfo);
        }
    }
}
