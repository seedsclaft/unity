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
}
