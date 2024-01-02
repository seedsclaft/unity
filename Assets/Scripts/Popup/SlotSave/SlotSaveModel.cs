using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;

public class SlotSaveModel : BaseModel
{
    public SlotInfo CurrentSlotInfo = null;
    private SlotInfo _baseSlotInfo = null;
    public SlotInfo BaseSlotInfo => _baseSlotInfo;
    public List<ListData> SlotList()
    {
        return MakeListData(CurrentData.PlayerInfo.SlotSaveList);
    }

    public void SetBaseSlotSaveInfo(SlotInfo slotInfo)
    {
        _baseSlotInfo = slotInfo;
    }

    public void SaveSlotInfo(int slotId)
    {
        CurrentData.PlayerInfo.SaveSlotData(slotId,CurrentSlotInfo);
    }
}
