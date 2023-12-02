using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class SlotModel : BaseModel
{
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
        CurrentData.InitActors();
        CurrentData.InitPlayer();
    }

    public void SetActorsData(int index)
    {
        CurrentData.InitActors();
        PartyInfo.InitActors();
        var actorInfos = SlotInfos()[index].ActorInfos;
        foreach (var actorInfo in actorInfos)
        {
            CurrentData.AddActor(actorInfo);
        }
    }

}
