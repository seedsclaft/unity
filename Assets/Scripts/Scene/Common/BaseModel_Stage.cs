using System.Collections.Generic;
using System.Diagnostics;

namespace Ryneus
{
    public partial class BaseModel
    {
        public void MakeStageInfo(int stageId)
        {
            var stageInfo = new StageInfo(stageId);
            stageInfo.SetSymbolInfos(GetStageSymbolInfos(stageId));
            CurrentGameInfo.SetStageInfo(stageInfo);
            PartyInfo.SetStageId(stageId);
            PartyInfo.SetSeek(1);
            PartyInfo.SetSeekIndex(0);
        }
        
        public List<SymbolInfo> GetStageSymbolInfos(int stageId)
        {
            return StageSymbolInfos(DataSystem.FindStage(stageId).StageSymbols);
        }

        /// <summary>
        /// 表示するステージデータ
        /// </summary>
        /// <returns></returns>
        public List<ListData> StageSymbolInfos()
        {
            var symbolListDict = new Dictionary<int,List<SymbolInfo>>();
            var symbolInfos = CurrentStage.SymbolInfos;

            foreach (var symbolInfo in symbolInfos)
            {
                if (!symbolListDict.ContainsKey(symbolInfo.Master.Seek))
                {
                    symbolListDict[symbolInfo.Master.Seek] = new List<SymbolInfo>();
                }
                symbolListDict[symbolInfo.Master.Seek].Add(symbolInfo);
            }

            var listData = new List<ListData>();
            foreach (var symbolList in symbolListDict)
            {
                var list = new ListData(symbolList.Value);
                listData.Add(list);
            }
            return listData;
        }

        public void EndSymbolInfo(SymbolInfo symbolInfo)
        {
            foreach (var getItemInfo in symbolInfo.GetItemInfos)
            {   
                getItemInfo.SetGetFlag(true);
                switch (getItemInfo.GetItemType)
                {
                    case GetItemType.Currency:
                        PartyInfo.AddCurrency(getItemInfo.Param1);
                        break;
                    default:
                        PartyInfo.AddGetItemInfo(getItemInfo);
                        break;
                }
            }
        }

        public void SeekNext()
        {
            foreach (var actorInfo in PartyInfo.ActorInfos)
            {
                actorInfo.ChangeHp(actorInfo.MaxHp);
            }
            PartyInfo.SetSeek(PartyInfo.Seek + 1);
            PartyInfo.SetSeekIndex(0);
        }
    }
}
