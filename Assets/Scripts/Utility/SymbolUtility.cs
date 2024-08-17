using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Ryneus
{
    public class SymbolUtility
    {
        public static SaveStageInfo CurrentSaveData => GameSystem.CurrentStageData;
        public static StageInfo CurrentStage => CurrentSaveData.CurrentStage;
        public static PartyInfo PartyInfo => CurrentSaveData.Party;
        public static List<SymbolResultInfo> StageResultInfos(List<StageSymbolData> stageSymbolDates)
        {
            var symbolInfos = new List<SymbolResultInfo>();
            var symbols = stageSymbolDates.FindAll(a => a.Seek > 0);
            foreach (var symbolMaster in symbols)
            {
                var symbolInfo = new SymbolInfo(symbolMaster.SymbolType);
                var stageSymbolData = new StageSymbolData();
                stageSymbolData.CopyData(symbolMaster);
                // グループ指定
                if (stageSymbolData.IsGroupSymbol())
                {
                    var groupId = (int)stageSymbolData.SymbolType;
                    var groupDates = DataSystem.SymbolGroups.FindAll(a => a.GroupId == groupId);
                    stageSymbolData = PickUpSymbolData(groupDates);
                    stageSymbolData.StageId = symbolMaster.StageId;
                    stageSymbolData.Seek = symbolMaster.Seek;
                    stageSymbolData.SeekIndex = symbolMaster.SeekIndex;
                    symbolInfo = new SymbolInfo(stageSymbolData.SymbolType);
                    if (stageSymbolData.SymbolType == SymbolType.Battle)
                    {
                        symbolInfo.SetTroopInfo(BattleTroop(stageSymbolData));
                    }
                }
                if (stageSymbolData.SymbolType == SymbolType.Random)
                {
                    stageSymbolData = RandomSymbolData(stageSymbolDates);
                    stageSymbolData.StageId = symbolMaster.StageId;
                    stageSymbolData.Seek = symbolMaster.Seek;
                    stageSymbolData.SeekIndex = symbolMaster.SeekIndex;
                    symbolInfo = new SymbolInfo(stageSymbolData.SymbolType);
                    if (stageSymbolData.SymbolType == SymbolType.Battle && stageSymbolData.Param1 == -1)
                    {
                        symbolInfo.SetTroopInfo(BattleTroop(stageSymbolData));
                    }
                }
                var getItemInfos = new List<GetItemInfo>();
                switch (stageSymbolData.SymbolType)
                {
                    case SymbolType.Battle:
                    case SymbolType.Boss:
                        if (stageSymbolData.Param1 > 0)
                        {
                            symbolInfo.SetTroopInfo(BattleTroop(stageSymbolData));
                        }
                        
                        if (symbolInfo.TroopInfo != null && symbolInfo.TroopInfo.GetItemInfos.Count > 0)
                        {
                            getItemInfos.AddRange(symbolInfo.TroopInfo.GetItemInfos);
                        }
                        break;
                    case SymbolType.Alcana:
                        // アルカナランダムで報酬設定
                        if (stageSymbolData.Param1 == -1)
                        {
                            var relicInfos = MakeSelectRelicGetItemInfos((RankType)stageSymbolData.Param2);
                            //symbolInfo.SetGetItemInfos(relicInfos);
                            getItemInfos.AddRange(relicInfos);
                        }
                        break;
                    case SymbolType.Resource:
                        // 報酬設定
                        var resourceData = new GetItemData
                        {
                            Type = GetItemType.Numinous,
                            Param1 = stageSymbolData.Param1
                        };
                        getItemInfos.Add(new GetItemInfo(resourceData));
                        break;
                    case SymbolType.Actor:
                        // 表示用に報酬設定
                        var actorData = new GetItemData
                        {
                            Type = GetItemType.AddActor,
                            Param1 = stageSymbolData.Param1
                        };
                        getItemInfos.Add(new GetItemInfo(actorData));
                        break;
                    case SymbolType.SelectActor:
                        // 表示用に報酬設定
                        if (stageSymbolData.Param2 == 0)
                        {
                            var actorData2 = new GetItemInfo(new GetItemData());
                            getItemInfos.Add(actorData2);
                        } else
                        {
                            // 選択できるアクターが3人まで
                            var pastActorIdList = PartyInfo.PastActorIdList(CurrentStage.Id,CurrentStage.CurrentSeek,CurrentStage.WorldNo);
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
                                    if (getItemInfos.Find(a => a.Param1 == actorData2.Param1) == null)
                                    {
                                        getItemInfos.Add(new GetItemInfo(actorData2));
                                    }
                                }
                            }
                        }
                        break;
                        case SymbolType.Shop:
                        // Rank1,2から1つずつランダム設定
                        if (stageSymbolData.Param1 == -1)
                        {
                            var alcanaRank = RankType.ActiveRank2;
                            var alcanaIds = PartyInfo.CurrentAlcanaIdList(CurrentStage.Id,CurrentStage.CurrentSeek,CurrentStage.WorldNo);
                            var alcanaSkills = DataSystem.Skills.Where(a => a.Value.Rank == alcanaRank && !alcanaIds.Contains(a.Value.Id)).ToList();
                            var count = 1;
                            while (getItemInfos.Count <= count)
                            {
                                var rand = Random.Range(0,alcanaSkills.Count);
                                // 報酬設定
                                if (getItemInfos.Find(a => a.Param1 == alcanaSkills[rand].Value.Id) == null)
                                {
                                    var alcanaData = new GetItemData
                                    {
                                        Type = GetItemType.Skill,
                                        Param1 = alcanaSkills[rand].Value.Id
                                    };
                                    var getItemInfo = new GetItemInfo(alcanaData);
                                    if (getItemInfos.Find(a => a.Param1 == alcanaData.Param1) == null)
                                    {
                                        symbolInfo.SetGetItemInfos(new List<GetItemInfo>(){new GetItemInfo(alcanaData)});
                                        getItemInfos.Add(getItemInfo);
                                    }
                                }
                            }
                            alcanaRank = RankType.PassiveRank1;
                            alcanaIds = PartyInfo.CurrentAlcanaIdList(CurrentStage.Id,CurrentStage.CurrentSeek,CurrentStage.WorldNo);
                            alcanaSkills = DataSystem.Skills.Where(a => a.Value.Rank == alcanaRank && !alcanaIds.Contains(a.Value.Id)).ToList();
                            count = 2;
                            while (getItemInfos.Count <= count)
                            {
                                var rand = Random.Range(0,alcanaSkills.Count);
                                // 報酬設定
                                if (getItemInfos.Find(a => a.Param1 == alcanaSkills[rand].Value.Id) == null)
                                {
                                    var alcanaData = new GetItemData
                                    {
                                        Type = GetItemType.Skill,
                                        Param1 = alcanaSkills[rand].Value.Id
                                    };
                                    var getItemInfo = new GetItemInfo(alcanaData);
                                    if (getItemInfos.Find(a => a.Param1 == alcanaData.Param1) == null)
                                    {
                                        symbolInfo.SetGetItemInfos(new List<GetItemInfo>(){new GetItemInfo(alcanaData)});
                                        getItemInfos.Add(getItemInfo);
                                    }
                                }
                            }
                            alcanaRank = RankType.PassiveRank2;
                            alcanaIds = PartyInfo.CurrentAlcanaIdList(CurrentStage.Id,CurrentStage.CurrentSeek,CurrentStage.WorldNo);
                            alcanaSkills = DataSystem.Skills.Where(a => a.Value.Rank == alcanaRank && !alcanaIds.Contains(a.Value.Id)).ToList();
                            count = 1;
                            while (getItemInfos.Count <= count)
                            {
                                var rand = Random.Range(0,alcanaSkills.Count);
                                // 報酬設定
                                if (getItemInfos.Find(a => a.Param1 == alcanaSkills[rand].Value.Id) == null)
                                {
                                    var alcanaData = new GetItemData
                                    {
                                        Type = GetItemType.Skill,
                                        Param1 = alcanaSkills[rand].Value.Id
                                    };
                                    var getItemInfo = new GetItemInfo(alcanaData);
                                    if (getItemInfos.Find(a => a.Param1 == alcanaData.Param1) == null)
                                    {
                                        symbolInfo.SetGetItemInfos(new List<GetItemInfo>(){new GetItemInfo(alcanaData)});
                                        getItemInfos.Add(getItemInfo);
                                    }
                                }
                            }
                        }
                        break;
                }
                if (stageSymbolData.PrizeSetId > 0)
                {
                    var prizeSets = DataSystem.PrizeSets.FindAll(a => a.Id == stageSymbolData.PrizeSetId);
                    foreach (var prizeSet in prizeSets)
                    {
                        var getItemInfo = new GetItemInfo(prizeSet.GetItem);
                        if (prizeSet.GetItem.Type == GetItemType.SelectRelic)
                        {
                            var relicInfos = MakeSelectRelicGetItemInfos((RankType)getItemInfo.Param2);
                            getItemInfos.AddRange(relicInfos);
                        } else
                        {
                            getItemInfos.Add(getItemInfo);
                        }
                    }
                }
                symbolInfo.SetGetItemInfos(getItemInfos);
                var record = new SymbolResultInfo(symbolInfo,stageSymbolData,GameSystem.CurrentStageData.Party.Currency);
                symbolInfos.Add(record);
            }
            return symbolInfos;
        }

        private static List<GetItemInfo> MakeSelectRelicGetItemInfos(RankType rankType)
        {
            var getItemInfos = new List<GetItemInfo>();
            var alcanaRank = rankType;
            var alcanaIds = PartyInfo.CurrentAlcanaIdList(CurrentStage.Id,CurrentStage.CurrentSeek,CurrentStage.WorldNo);
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
                    if (getItemInfos.Find(a => a.Param1 == alcanaSkills[rand].Value.Id) == null)
                    {
                        // 報酬設定
                        var alcanaData = new GetItemData
                        {
                            Type = GetItemType.SelectRelic,
                            Param1 = alcanaSkills[rand].Value.Id
                        };
                        var getItemInfo = new GetItemInfo(alcanaData);
                        if (getItemInfos.Find(a => a.Param1 == alcanaData.Param1) == null)
                        {
                            getItemInfos.Add(getItemInfo);
                        }
                    }
                }
            }
            return getItemInfos;
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
            var stageSymbolData = new StageSymbolData
            {
                SymbolType = SymbolType.None
            };
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
                        stageSymbolData.CopyParamData(symbol);
                        stageSymbolData.Param1 = -1;
                        stageSymbolData.PrizeSetId = 0;
                        break;
                    case SymbolType.Actor:
                        // 今まで遭遇していないアクターを選定
                        var list = new List<int>();
                        foreach (var actorInfo in PartyInfo.ActorInfos)
                        {
                            if (!PartyInfo.PastActorIdList(CurrentStage.Id,CurrentStage.CurrentSeek,CurrentStage.WorldNo).Contains(actorInfo.ActorId))
                            {
                                list.Add(actorInfo.ActorId);
                            }
                        }
                        targetRand = Random.Range(0,list.Count);
                        stageSymbolData.CopyParamData(symbol);
                        stageSymbolData.Param1 = list[targetRand];
                        stageSymbolData.Param2 = 0;
                        break;
                    case SymbolType.Alcana:
                        if (!PartyInfo.CurrentAlchemyIdList(CurrentStage.Id,CurrentStage.CurrentSeek,CurrentStage.WorldNo).Contains(symbol.Param1))
                        {
                            stageSymbolData.CopyParamData(symbol);
                        }
                        break;
                    default:
                        if (symbol.IsGroupSymbol())
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
                            stageSymbolData.CopyParamData(symbol);
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
            var troopInfo = new TroopInfo(troopId,troopId == -1);
            // ランダム生成
            if (troopId == -1)
            {
                var lv = stageSymbolData.Seek + CurrentStage.Master.StageLv + plusLevel;
                troopInfo.MakeEnemyRandomTroopDates(lv);
                // 確定報酬でNuminos
                var numinosGetItem = new GetItemData
                {
                    Param1 = (lv / 2) + 9,
                    Type = GetItemType.Numinous
                };
                troopInfo.AddGetItemInfo(new GetItemInfo(numinosGetItem));
                // ランダム報酬データ設定
                // 70 = Rank1 Passive,
                // 7 = Rank2 Passive,
                // 15 = Rank1 Active,
                // 3 = Rank2 Active
                // 3 = Rank1 Enhance
                // 2 = Rank2 Enhance
                var getItemData = MakeSkillGetItemInfo();
                if (getItemData != null)
                {
                    troopInfo.AddGetItemInfo(new GetItemInfo(getItemData));
                }
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

        private static GetItemData MakeSkillGetItemInfo()
        {
            GetItemData getItemData = null;
            int rand = Random.Range(0,100);
            if (rand < 70)
            {
                getItemData = AddSkillGetItemData(RankType.PassiveRank1);
            } else
            if (rand >= 70 && rand < 77)
            {
                getItemData = AddSkillGetItemData(RankType.PassiveRank2);
            } else
            if (rand >= 77 && rand < 92)
            {
                getItemData = AddSkillGetItemData(RankType.ActiveRank1);
            } else
            if (rand >= 92 && rand < 95)
            {
                getItemData = AddSkillGetItemData(RankType.ActiveRank2);
            } else
            if (rand >= 95 && rand < 98)
            {
                getItemData = AddSkillGetItemData(RankType.EnhanceRank1);
            } else
            {
                getItemData = AddSkillGetItemData(RankType.EnhanceRank2);
            }
            // 有効な強化でなければ再抽選
            if (getItemData != null)
            {
                var skillId = getItemData.Param1;
                var skillData = DataSystem.FindSkill(skillId);
                if (skillData.Rank == RankType.EnhanceRank1 || skillData.Rank == RankType.EnhanceRank2)
                {
                    var beforeSkillId = skillData.Id - 400000;
                    if (!PartyInfo.CurrentAllSkillIds(CurrentStage.Id,CurrentStage.CurrentSeek,CurrentStage.WorldNo).Contains(beforeSkillId))
                    {
                        return MakeSkillGetItemInfo();
                    }
                }
            }
            return getItemData;
        }

        private static GetItemData AddSkillGetItemData(RankType rankType)
        {
            var hasSkills = CurrentSaveData.Party.CurrentAlchemyIdList(CurrentStage.Id,CurrentStage.CurrentSeek,CurrentStage.WorldNo);
            var skillList = new List<SkillData>(DataSystem.Skills.Values);
            var skills = skillList.FindAll(a => a.Rank == rankType && !hasSkills.Contains(a.Id));
            if (skills.Count > 0)
            {
                var skillRand = Random.Range(0,skills.Count);
                var getItemData = new GetItemData
                {
                    Param1 = skills[skillRand].Id,
                    Type = GetItemType.Skill
                };
                return getItemData;
            }
            return null;
        }
    }
}
