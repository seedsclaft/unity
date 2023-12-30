using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class SlotModel : BaseModel
{
    public List<ListData> SlotList()
    {
        return MakeListData(CurrentData.PlayerInfo.SlotSaveList);
    }

    public void SlotLock(int index)
    {
    }

    public void ClearActorsData()
    {
        CurrentStageData.ClearActors();
        CurrentStageData.InitAllActorMembers();
    }

    public void SetActorsData(int index)
    {
        CurrentStageData.ClearActors();
        PartyInfo.InitActors();
        var slotData = (SlotInfo)SlotList()[index].Data;
        var actorInfos = slotData.ActorInfos;
        foreach (var actorInfo in actorInfos)
        {
            CurrentStageData.AddActor(actorInfo);
        }
    }
}
