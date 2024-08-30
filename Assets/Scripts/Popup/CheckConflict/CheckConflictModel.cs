using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ryneus
{
    public class CheckConflictModel : BaseModel
    {
        public List<List<SymbolResultInfo>> ResultInfos()
        {
            var symbolInfos = new List<SymbolInfo>();
            var recordList = new Dictionary<int,List<SymbolResultInfo>>();
            
            var stageSeekList = new List<int>();
            var mainRecords = PartyInfo.SymbolRecordList.FindAll(a => a.EndFlag == false && a.Seek > 0 || a.EndFlag == false && PartyInfo.EnableMultiverse() && a.Seek == 0);
            var brunchRecords = PartyInfo.SymbolRecordList.FindAll(a => a.EndFlag == false && a.Seek > 0 || a.EndFlag == false && PartyInfo.EnableMultiverse() && a.Seek == 0);
            // 始点と終点を作る
            var brunchSymbol = PartyInfo.BrunchBaseSymbol;
            var returnSymbol = PartyInfo.ReturnSymbol;
            mainRecords = mainRecords.FindAll(a => a.IsBeforeStageSeek(returnSymbol.StageId,returnSymbol.Seek,WorldType.Main) && a.IsAfterStageSeek(brunchSymbol.StageId,brunchSymbol.Seek,WorldType.Main));
            brunchRecords = brunchRecords.FindAll(a => a.IsBeforeStageSeek(returnSymbol.StageId,returnSymbol.Seek,WorldType.Brunch) && a.IsAfterStageSeek(brunchSymbol.StageId,brunchSymbol.Seek,WorldType.Brunch));

            foreach (var selectRecord in brunchRecords)
            {
                var stageKey = (selectRecord.StageSymbolData.StageId-1)*100 + selectRecord.StageSymbolData.Seek;
                if (!stageSeekList.Contains(stageKey))
                {
                    stageSeekList.Add(stageKey);
                }
            }    
            stageSeekList.Sort((a,b) => a - b > 0 ? 1 : -1);
            
            foreach (var stageSeek in stageSeekList)
            {
                recordList[stageSeek] = new List<SymbolResultInfo>();
            }
            var lastSelectSeek = brunchRecords.Count > 0 ? brunchRecords.Select(a => a.Seek).Max() : -1;
            foreach (var mainRecord in mainRecords)
            {
                var stageKey = (mainRecord.StageSymbolData.StageId-1)*100 + mainRecord.StageSymbolData.Seek;
                if (recordList.ContainsKey(stageKey))
                {
                    var list = new List<SymbolResultInfo>();
                    list.Add(mainRecord);
                    var brunchRecord = brunchRecords.Find(a => a.IsSameStageSeek(mainRecord.StageId,mainRecord.Seek,WorldType.Brunch));
                    list.Add(brunchRecord);
                    recordList[stageKey] = list;
                }
            }
    
            var resultList = new List<List<SymbolResultInfo>>();
            var result = recordList.OrderBy(a => a.Key).ToList();
            foreach (var resultData in result)
            {
                resultList.Add(resultData.Value);
            }
            return resultList;
        }

        
        public List<ActorInfo> MainActorInfos()
        {
            var returnSymbol = PartyInfo.ReturnSymbol;
            return PartyInfo.CurrentActorInfos(returnSymbol.StageId,returnSymbol.Seek,WorldType.Main);
        }

        public List<ActorInfo> BrunchActorInfos()
        {
            var brunchSymbol = PartyInfo.BrunchSymbol;
            return PartyInfo.CurrentActorInfos(brunchSymbol.StageId,brunchSymbol.Seek,WorldType.Main);
        }
    }
}