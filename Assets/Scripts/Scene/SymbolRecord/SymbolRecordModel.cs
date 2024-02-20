using System.Collections.Generic;

public class SymbolRecordModel : BaseModel
{

    public bool CanParallel()
    {
        return PartyInfo.Currency >= PartyInfo.ParallelCost();
    }

    public List<ListData> ParallelCommand()
    {
        return MakeListData(BaseConfirmCommand(23050,23040));
    }

    public List<ListData> StageSymbolInfos(int seek)
    {
        var list = new List<SymbolInfo>();
        var symbolInfos = PartyInfo.SymbolRecordList.FindAll(a => a.StageId == CurrentStage.Id && a.Seek == (seek+1));
        symbolInfos.Sort((a,b) => a.SeekIndex > b.SeekIndex ? 1 : -1);
        var symbolRecords = PartyInfo.SymbolRecordList.FindAll(a => a.StageId == CurrentStage.Id && a.Selected == true);
        for (int i = 0;i < symbolInfos.Count;i++)
        {
            var symbolInfo = new SymbolInfo();
            symbolInfo.CopyData(symbolInfos[i].SymbolInfo);
            var saveRecord = symbolRecords.Find(a => a.IsSameSymbol(CurrentStage.Id,seek+1,i));
            symbolInfo.SetSelected(saveRecord != null);
            symbolInfo.SetCleared(symbolInfos[i].Cleared);
            MakePrizeData(saveRecord,symbolInfo.GetItemInfos);
            list.Add(symbolInfo);
        }
        return MakeListData(list);
    }

    public List<ListData> SymbolRecords()
    {
        var symbolRecords = PartyInfo.SymbolRecordList.FindAll(a => a.StageId == CurrentStage.Id && a.Selected == true);
        var symbolInfos = new List<SymbolInfo>();
        
        var stageData = DataSystem.FindStage(CurrentStage.Id);
        foreach (var symbolRecord in symbolRecords)
        {
            var symbolInfo = new SymbolInfo();
            symbolInfo.CopyData(symbolRecord.SymbolInfo);
            MakePrizeData(symbolRecord,symbolInfo.GetItemInfos);
            if (symbolInfos.Find(a => a.StageSymbolData.Seek == symbolRecord.Seek) == null)
            {
                symbolInfos.Add(symbolInfo);
            }
        }
        return MakeListData(symbolInfos);
    }
    
}
