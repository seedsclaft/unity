using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ryneus
{
    public class StageSymbolModel : BaseModel
    {
        public List<ListData> StageSymbolInfos(int seek)
        {
            var list = new List<SymbolInfo>();
            var symbolInfos = CurrentStage.StageSymbolInfos.FindAll(a => a.StageSymbolData.Seek == seek);
            var selectRecords = CurrentStage.SymbolRecordList.FindAll(a => a.StageId == CurrentStage.Id && a.Selected == true);
            for (int i = 0;i < symbolInfos.Count;i++)
            {
                var symbolInfo = new SymbolInfo();
                symbolInfo.CopyData(symbolInfos[i]);
                var saveRecord = selectRecords.Find(a => a.IsSameSymbol(CurrentStage.Id,seek+1,i));
                symbolInfo.SetSelected(saveRecord != null);
                symbolInfo.SetCleared(symbolInfos[i].Cleared);
                MakePrizeData(saveRecord,symbolInfo.GetItemInfos);
                list.Add(symbolInfo);
            }
            return MakeListData(list);
        }

        public List<ListData> SymbolRecords()
        {
            var symbolInfos = new List<SymbolInfo>();
            var symbolInfoList = new List<List<SymbolInfo>>();
            
            var stageSeekList = new List<int>();
            foreach (var symbolInfo in CurrentStage.StageSymbolInfos)
            {
                if (!stageSeekList.Contains(symbolInfo.StageSymbolData.Seek))
                {
                    stageSeekList.Add(symbolInfo.StageSymbolData.Seek);
                }
            }
            foreach (var stageSeek in stageSeekList)
            {
                var list = new List<SymbolInfo>();
                symbolInfoList.Add(list);
            }
            var selectRecords = CurrentStage.SymbolRecordList.FindAll(a => a.StageId == CurrentStage.Id && a.Selected == true);
            var lastSelectSeek = selectRecords.Count > 0 ? selectRecords.Select(a => a.Seek).Max() : -1;
            foreach (var stageSymbolInfo in CurrentStage.StageSymbolInfos)
            {
                var symbolInfo = new SymbolInfo();
                symbolInfo.CopyData(stageSymbolInfo);
                var saveRecord = selectRecords.Find(a => a.IsSameSymbol(stageSymbolInfo.StageSymbolData.StageId,stageSymbolInfo.StageSymbolData.Seek,stageSymbolInfo.StageSymbolData.SeekIndex));
                symbolInfo.SetSelected(saveRecord != null);
                symbolInfo.SetLastSelected(saveRecord != null && lastSelectSeek == symbolInfo.StageSymbolData.Seek);
                symbolInfo.SetPast(saveRecord == null && stageSymbolInfo.StageSymbolData.Seek <= lastSelectSeek);
                if (saveRecord != null)
                {
                    MakePrizeData(saveRecord,symbolInfo.GetItemInfos);
                }
                symbolInfoList[symbolInfo.StageSymbolData.Seek-1].Add(symbolInfo);
                /*
                if (symbolInfos.Find(a => a.StageSymbolData.Seek == symbolRecord.Seek) == null)
                {
                    symbolInfos.Add(symbolInfo);
                }
                */
            }
            return MakeListData(symbolInfoList);
        }
        
        public void SetTempAddActorStatusInfos(int actorId)
        {
            var actorInfos = PartyInfo.ActorInfos.FindAll(a => a.ActorId == actorId);
            TempInfo.SetTempStatusActorInfos(actorInfos);
        }
    }
}