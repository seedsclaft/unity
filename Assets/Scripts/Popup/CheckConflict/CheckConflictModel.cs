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
            var mainRecords = PartyInfo.SymbolRecordList.FindAll(a => a.StageId > 0);
            var brunchRecords = PartyInfo.SymbolRecordList.FindAll(a => a.StageId > 0);
            // 始点と終点を作る
            var brunchSymbol = PartyInfo.BrunchBaseSymbol;
            var returnSymbol = PartyInfo.ReturnSymbol;
            mainRecords = mainRecords.FindAll(a => a.IsBeforeStageSeek(returnSymbol.StageId,returnSymbol.Seek,WorldType.Main) && a.IsAfterStageSeek(brunchSymbol.StageId,brunchSymbol.Seek,WorldType.Main));
            brunchRecords = brunchRecords.FindAll(a => a.IsBeforeStageSeek(returnSymbol.StageId,returnSymbol.Seek,WorldType.Brunch) && a.IsAfterStageSeek(brunchSymbol.StageId,brunchSymbol.Seek,WorldType.Brunch));

            foreach (var selectRecord in brunchRecords)
            {
                var stageKey = (selectRecord.StageId-1)*100 + selectRecord.Seek;
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
                var stageKey = (mainRecord.StageId-1)*100 + mainRecord.Seek;
                if (recordList.ContainsKey(stageKey) && mainRecord.Selected)
                {
                    var list = new List<SymbolResultInfo>();
                    var brunchRecord = brunchRecords.Find(a => a.Selected && a.IsSameStageSeek(mainRecord.StageId,mainRecord.Seek,WorldType.Brunch));
                    if (brunchRecord != null)
                    {
                        list.Add(mainRecord);
                        list.Add(brunchRecord);
                        recordList[stageKey] = list;
                    }
                }
            }
    
            var resultList = new List<List<SymbolResultInfo>>();
            var result = recordList.OrderBy(a => a.Key).ToList();
            foreach (var resultData in result)
            {
                if (resultData.Value.Count == 2)
                {
                    resultList.Add(resultData.Value);
                }
            }
            return resultList;
        }

        
        public List<ActorInfo> MainActorInfos()
        {
            var returnSymbol = PartyInfo.ReturnSymbol;
            var actorInfos = PartyInfo.CurrentActorInfos(returnSymbol.StageId,returnSymbol.Seek,WorldType.Main);
            foreach (var actorInfo in actorInfos)
            {
                actorInfo.SetStageSeek(returnSymbol.StageId,returnSymbol.Seek,WorldType.Main);
            }
            return actorInfos;
        }

        public List<ActorInfo> BrunchActorInfos()
        {
            var brunchSymbol = PartyInfo.BrunchSymbol;
            var actorInfos = PartyInfo.CurrentActorInfos(brunchSymbol.StageId,brunchSymbol.Seek,WorldType.Brunch);
            foreach (var actorInfo in actorInfos)
            {
                actorInfo.SetStageSeek(brunchSymbol.StageId,brunchSymbol.Seek,WorldType.Brunch);
            }
            return actorInfos;
        }
    }
}