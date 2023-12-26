using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;

public class RankingModel : BaseModel
{
    private List<RankingInfo> _rakingInfos = null;
    public async void RankingInfos(Action<List<ListData>> endEvent)
    {
#if (UNITY_WEBGL || UNITY_ANDROID) //&& !UNITY_EDITOR
        if (_rakingInfos == null)
        {
            FirebaseController.Instance.ReadRankingData();
            await UniTask.WaitUntil(() => FirebaseController.IsBusy == false);
            _rakingInfos = FirebaseController.RankingInfos;
        }
        if (endEvent != null) 
        {
            endEvent(MakeListData(_rakingInfos));
        }
#endif
    }

    public void MakeDetailPartyInfo(int listIndex)
    {
        var rankingInfo = _rakingInfos[listIndex];
        CurrentStageData.ClearActors();
        PartyInfo.InitActors();
        foreach (var actorInfo in rankingInfo.ActorInfos)
        {
            PartyInfo.AddActor(actorInfo.ActorId);
            CurrentStageData.AddActor(actorInfo);
        }
    }
}
