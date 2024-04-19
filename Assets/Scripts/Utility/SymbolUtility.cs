using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
                if (symbol.IsRandomSymbol()){
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
                switch (symbolInfo.SymbolType)
                {
                    case SymbolType.Battle:
                    case SymbolType.Boss:
                        if (symbolInfo.StageSymbolData.Param1 > 0 || symbolInfo.StageSymbolData.Param1 == -1)
                        {
                            symbolInfo.SetTroopInfo(BattleTroop(symbolInfo.StageSymbolData));
                        }
                        
                        if (symbolInfo.TroopInfo != null && symbolInfo.TroopInfo.GetItemInfos.Count > 0)
                        {
                            getItemInfos.AddRange(symbolInfo.TroopInfo.GetItemInfos);
                        }
                        break;
                    case SymbolType.Alcana:
                        // アルカナランダムで報酬設定
                        if (symbolInfo.StageSymbolData.Param1 == -1)
                        {
                            var alcanaRank = symbolInfo.StageSymbolData.Param2;
                            var alcanaIds = PartyInfo.CurrentAlcanaIdList(CurrentStage.Id,CurrentStage.CurrentTurn);
                            var alcanaSkills = DataSystem.Skills.Where(a => a.Value.Rank == alcanaRank && !alcanaIds.Contains(a.Value.Id)).ToList();
                            var count = 2;
                            if (alcanaSkills.Count < 2)
                            {
                                count = alcanaSkills.Count;
                            }
                            if (count == 0)
                            {
                                // 報酬設定
                                var resourceData2 = new GetItemData
                                {
                                    Type = GetItemType.Numinous,
                                    Param1 = 20
                                };
                                getItemInfos.Add(new GetItemInfo(resourceData2));
                            } else
                            {
                                while (getItemInfos.Count <= count)
                                {
                                    var rand = Random.Range(0,alcanaSkills.Count);
                                    // 報酬設定
                                    var alcanaData = new GetItemData
                                    {
                                        Type = GetItemType.Skill,
                                        Param1 = alcanaSkills[rand].Value.Id
                                    };
                                    var getItemInfo = new GetItemInfo(alcanaData);
                                    symbolInfo.SetGetItemInfos(new List<GetItemInfo>(){new GetItemInfo(alcanaData)});
                                    getItemInfos.Add(getItemInfo);
                                }
                            }
                        }
                        break;
                    case SymbolType.Resource:
                        // 報酬設定
                        var resourceData = new GetItemData
                        {
                            Type = GetItemType.Numinous,
                            Param1 = symbolInfo.StageSymbolData.Param1
                        };
                        getItemInfos.Add(new GetItemInfo(resourceData));
                        break;
                    case SymbolType.Actor:
                        // 表示用に報酬設定
                        var actorData = new GetItemData
                        {
                            Type = GetItemType.AddActor,
                            Param1 = symbolInfo.StageSymbolData.Param1
                        };
                        getItemInfos.Add(new GetItemInfo(actorData));
                        break;
                    case SymbolType.SelectActor:
                        // 表示用に報酬設定
                        if (symbolInfo.StageSymbolData.Param2 == 0)
                        {
                            var actorData2 = new GetItemInfo(new GetItemData());
                            actorData2.MakeSelectActorSymbolResult();
                            getItemInfos.Add(actorData2);
                        } else
                        {
                            // 選択できるアクターが3人まで
                            var pastActorIdList = PartyInfo.PastActorIdList(CurrentStage.Id,CurrentStage.CurrentTurn);
                            var actorInfos = PartyInfo.ActorInfos.FindAll(a => !pastActorIdList.Contains(a.ActorId));
                            var count = 2;
                            if (actorInfos.Count < 2)
                            {
                                count = actorInfos.Count;
                            }
                            if (count == 0)
                            {
                                // 報酬設定
                                var resourceData2 = new GetItemData
                                {
                                    Type = GetItemType.Numinous,
                                    Param1 = 20
                                };
                                getItemInfos.Add(new GetItemInfo(resourceData2));
                            } else
                            {
                                while (getItemInfos.Count <= count)
                                {
                                    var rand = Random.Range(0,actorInfos.Count);
                                    var actorData2 = new GetItemData
                                    {
                                        Type = GetItemType.AddActor,
                                        Param1 = actorInfos[rand].ActorId
                                    };
                                    getItemInfos.Add(new GetItemInfo(actorData2));
                                }
                            }
                        }
                        break;
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
            int targetRand = Random.Range(0,groupDates.Sum(a => a.Rate));
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
            int targetRand = Random.Range(0,stageSymbolList.Sum(a => a.Rate));
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
                        // 今まで遭遇していないアクターを選定
                        var list = new List<int>();
                        foreach (var actorInfo in PartyInfo.ActorInfos)
                        {
                            if (!PartyInfo.PastActorIdList(CurrentStage.Id,CurrentStage.CurrentTurn).Contains(actorInfo.ActorId))
                            {
                                list.Add(actorInfo.ActorId);
                            }
                        }
                        targetRand = Random.Range(0,list.Count);
                        stageSymbolData.CopyData(symbol);
                        stageSymbolData.Param1 = list[targetRand];
                        stageSymbolData.Param2 = 0;
                        break;
                    case SymbolType.Alcana:
                        if (!PartyInfo.CurrentAlchemyIdList(CurrentStage.Id,CurrentStage.CurrentTurn).Contains(symbol.Param1))
                        {
                            stageSymbolData.CopyData(symbol);
                        }
                        break;
                    default:
                        if (symbol.IsRandomSymbol())
                        {
                            var groupId = (int)symbol.SymbolType;
                            var groupDates = DataSystem.SymbolGroups.FindAll(a => a.GroupId == groupId);
                            stageSymbolData = PickUpSymbolData(groupDates);
                            // GroupのBattleはParam2をコンバート前にする
                            if (stageSymbolData.SymbolType == SymbolType.Battle)
                            {
                                stageSymbolData.Param2 = symbol.Param2;
                            }
                            
                        } else
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
            var plusLevel = stageSymbolData.Param2;
            var troopInfo = new TroopInfo(troopId,false);
            // ランダム生成
            if (troopId == -1)
            {
                troopInfo.MakeEnemyRandomTroopDates(stageSymbolData.Seek + CurrentStage.Master.StageLv + plusLevel);
                /*
                for (int i = 0;i < enemyCount;i++)
                {
                    int rand = new System.Random().Next(1, CurrentStage.Master.RandomTroopCount);
                    var enemyData = DataSystem.Enemies.Find(a => a.Id == rand);
                    var enemy = new BattlerInfo(enemyData,PartyInfo.ClearTroopCount + 1,i,0,false);
                    troopInfo.AddEnemy(enemy);
                }
                */
                return troopInfo;
            }
            if (troopInfo.TroopMaster == null)
            {
                Debug.LogError("troopId" + troopId + "のデータが不足");
            } else
            {
                troopInfo.MakeEnemyTroopDates(plusLevel);
            }
            return troopInfo;
        }

    }
}
