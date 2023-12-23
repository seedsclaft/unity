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
        var list = new List<ListData>();
        foreach (var slotDict in CurrentData.PlayerInfo.GetSaveSlotDict())
        {
            var listData = new ListData(slotDict);
            list.Add(listData);
        }
        return list;
    }

    public Dictionary<int, SlotInfo> SlotInfos()
    {
        return CurrentData.PlayerInfo.GetSaveSlotDict();
    }

    public void SlotLock(int index)
    {
        SlotInfos()[index].ChangeLock();
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
        var actorInfos = SlotInfos()[index].ActorInfos;
        foreach (var actorInfo in actorInfos)
        {
            CurrentStageData.AddActor(actorInfo);
        }
    }

}
