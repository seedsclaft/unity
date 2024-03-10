using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class SymbolUtility
    {
        public static SaveStageInfo CurrentSaveData => GameSystem.CurrentStageData;
        public static StageInfo CurrentStage => CurrentSaveData.CurrentStage;
        public static PartyInfo PartyInfo => CurrentSaveData.Party;
        public static List<SymbolInfo> StageSymbolInfos(List<StageSymbolData> stageSymbolDates)
        {
            var symbolInfos = new List<SymbolInfo>();
            var symbols = stageSymbolDates.FindAll(a => a.Seek > 0);
            foreach (var symbol in symbols)
            {
                var symbolInfo = new SymbolInfo(symbol);
                // グループ指定
                if (symbol.SymbolType > SymbolType.Rebirth){
                    var groupId = (int)symbol.SymbolType;
                    var groupDates = DataSystem.SymbolGroups.FindAll(a => a.GroupId == groupId);
                    var data = PickUpSymbolData(groupDates);
                    data.StageId = symbol.StageId;
                    data.Seek = symbol.Seek;
                    data.SeekIndex = symbol.SeekIndex;
                    symbolInfo = new SymbolInfo(data);
                }
                if (symbol.SymbolType == SymbolType.Random || symbolInfo.StageSymbolData.SymbolType == SymbolType.Random){
                    var data = RandomSymbolData(stageSymbolDates);
                    data.StageId = symbol.StageId;
                    data.Seek = symbol.Seek;
                    data.SeekIndex = symbol.SeekIndex;
                    symbolInfo = new SymbolInfo(data);
                }
                var getItemInfos = new List<GetItemInfo>();
                if (symbolInfo.SymbolType == SymbolType.Battle || symbolInfo.SymbolType == SymbolType.Boss){
                    if (symbolInfo.StageSymbolData.Param1 > 0 || symbolInfo.StageSymbolData.Param1 == -1)
                    {
                        symbolInfo.SetTroopInfo(BattleTroop(symbolInfo.StageSymbolData));
                    }
                    
                    if (symbolInfo.TroopInfo != null && symbolInfo.TroopInfo.GetItemInfos.Count > 0)
                    {
                        getItemInfos.AddRange(symbolInfo.TroopInfo.GetItemInfos);
                    }
                }
                // アルカナランダムで報酬設定
                if (symbolInfo.SymbolType == SymbolType.Alcana)
                {
                    if (symbolInfo.StageSymbolData.Param1 == -1)
                    {
                        var alcanaRank = symbolInfo.StageSymbolData.Param2;
                        var alcanaSkills = DataSystem.Skills.FindAll(a => a.Rank == alcanaRank);
                        var rand = Random.Range(0,alcanaSkills.Count);
                        if (alcanaSkills.Count > 0)
                        {
                            var getItemData = new GetItemData();
                            getItemData.Type = GetItemType.Skill;
                            getItemData.Param1 = alcanaSkills[rand].Id;
                            var getItemInfo = new GetItemInfo(getItemData);
                            symbolInfo.SetGetItemInfos(new List<GetItemInfo>(){getItemInfo});
                            getItemInfos.Add(getItemInfo);
                        }

                    }
                }
                if (symbolInfo.StageSymbolData.PrizeSetId > 0)
                {
                    var prizeSets = DataSystem.PrizeSets.FindAll(a => a.Id == symbolInfo.StageSymbolData.PrizeSetId);
                    foreach (var prizeSet in prizeSets)
                    {
                        var getItemInfo = new GetItemInfo(prizeSet.GetItem);
                        getItemInfos.Add(getItemInfo);
                    }
                }
                symbolInfo.SetGetItemInfos(getItemInfos);
                symbolInfos.Add(symbolInfo);
            }
            return symbolInfos;
        }
        
        public static StageSymbolData PickUpSymbolData(List<SymbolGroupData> groupDates)
        {
            int targetRand = 0;
            for (int i = 0;i < groupDates.Count;i++)
            {
                targetRand += groupDates[i].Rate;
            }
            targetRand = Random.Range (0,targetRand);
            int targetIndex = -1;
            for (int i = 0;i < groupDates.Count;i++)
            {
                targetRand -= groupDates[i].Rate;
                if (targetRand <= 0 && targetIndex == -1)
                {
                    targetIndex = i;
                }
            }
            var StageSymbolData = new StageSymbolData();
            StageSymbolData.ConvertSymbolGroupData(groupDates[targetIndex]);
            return StageSymbolData;
        }

        public static StageSymbolData RandomSymbolData(List<StageSymbolData> stageSymbolDates)
        {
            // 候補を生成
            var stageSymbolList = stageSymbolDates.FindAll(a => a.Seek == 0);
            var stageSymbolData = new StageSymbolData();
            stageSymbolData.SymbolType = SymbolType.None;
            int targetRand = 0;
            foreach (var stageSymbol in stageSymbolList)
            {
                targetRand += stageSymbol.Rate;
            }
            targetRand = Random.Range(0,targetRand);
            while (stageSymbolData.SymbolType == SymbolType.None)
            {
                int targetIndex = -1;
                for (int i = 0;i < stageSymbolList.Count;i++)
                {
                    targetRand -= stageSymbolList[i].Rate;
                    if (targetRand <= 0 && targetIndex == -1)
                    {
                        targetIndex = i;
                    }
                }
                var symbol = stageSymbolList[targetIndex];
                switch (symbol.SymbolType)
                {
                    case SymbolType.Battle:
                        stageSymbolData.CopyData(symbol);
                        stageSymbolData.Param1 = -1;
                        stageSymbolData.PrizeSetId = 0;
                        break;
                    case SymbolType.Actor:
                        if (!PartyInfo.CurrentActorIdList(CurrentStage.Id,CurrentStage.CurrentTurn).Contains(symbol.PrizeSetId - 500))
                        {
                            stageSymbolData.CopyData(symbol);
                        }
                        break;
                    case SymbolType.Alcana:
                        if (!PartyInfo.CurrentAlchemyIdList(CurrentStage.Id,CurrentStage.CurrentTurn).Contains(symbol.Param1))
                        {
                            stageSymbolData.CopyData(symbol);
                        }
                        break;
                }
            }
            return stageSymbolData;
        }
        
        public static TroopInfo BattleTroop(StageSymbolData stageSymbolData)
        {
            var troopId = stageSymbolData.Param1;
            var enemyCount = stageSymbolData.Param2;
            var troopInfo = new TroopInfo(troopId,false);
            // ランダム生成
            if (troopId == -1)
            {
                troopInfo.MakeEnemyRandomTroopDates(PartyInfo.ClearTroopCount + stageSymbolData.Seek + CurrentStage.Master.StageLv);
                for (int i = 0;i < enemyCount;i++)
                {
                    int rand = new System.Random().Next(1, CurrentStage.Master.RandomTroopCount);
                    var enemyData = DataSystem.Enemies.Find(a => a.Id == rand);
                    var enemy = new BattlerInfo(enemyData,PartyInfo.ClearTroopCount + 1,i,0,false);
                    troopInfo.AddEnemy(enemy);
                }
                return troopInfo;
            }
            if (troopInfo.Master == null)
            {
                Debug.LogError("troopId" + troopId + "のデータが不足");
            } else
            {
                troopInfo.MakeEnemyTroopDates(PartyInfo.ClearTroopCount);
            }
            return troopInfo;
        }

    }
}
