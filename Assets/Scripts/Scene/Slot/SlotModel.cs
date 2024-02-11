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
    }

    public void ResetActors()
    {
        foreach (var actorInfo in Actors())
        {
            actorInfo.ResetData();
        }
    }
}
