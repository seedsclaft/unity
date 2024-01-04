using System;
using System.Collections;
using System.Collections.Generic;

public class SlotModel : BaseModel
{
    public List<ListData> SlotList()
    {
        return MakeListData(CurrentData.PlayerInfo.SlotSaveList);
    }

    public void SetSelectActorIds()
    {
        foreach (var actorId in PartyInfo.ActorIdList)
        {
            CurrentStage.AddSelectActorId(actorId);
        }
    }

    public void ResetActors()
    {
        foreach (var actorInfo in Actors())
        {
            actorInfo.ChangeLost(false);
            actorInfo.ClearTacticsCommand();
            actorInfo.ChangeHp(9999);
            actorInfo.ChangeMp(9999);
        }
    }
}
