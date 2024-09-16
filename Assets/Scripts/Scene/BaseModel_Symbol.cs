using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ryneus
{
    public partial class BaseModel
    {
        public List<SymbolResultInfo> StageResultInfos(List<StageSymbolData> stageSymbolDates)
        {
            var resultInfos = new List<SymbolResultInfo>();
            var symbolDates = stageSymbolDates.FindAll(a => a.Seek > 0);
            foreach (var symbolMaster in symbolDates)
            {
                var randFlag = false;
                var symbolInfo = new SymbolInfo(symbolMaster);
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
                    symbolInfo = new SymbolInfo(stageSymbolData);
                    if (stageSymbolData.SymbolType == SymbolType.Battle)
                    {
                        symbolInfo.SetTroopInfo(BattleTroop(stageSymbolData));
                        randFlag = true;
                    }
                }
                if (stageSymbolData.SymbolType == SymbolType.Random)
                {
                    stageSymbolData = RandomSymbolData(stageSymbolDates);
                    stageSymbolData.StageId = symbolMaster.StageId;
                    stageSymbolData.Seek = symbolMaster.Seek;
                    stageSymbolData.SeekIndex = symbolMaster.SeekIndex;
                    symbolInfo = new SymbolInfo(stageSymbolData);
                    if (stageSymbolData.SymbolType == SymbolType.Battle && stageSymbolData.Param1 == -1)
                    {
                        symbolInfo.SetTroopInfo(BattleTroop(stageSymbolData));
                    }
                }
                // 報酬リスト
                var getItemInfos = new List<GetItemInfo>();
                switch (stageSymbolData.SymbolType)
                {
                    case SymbolType.Battle:
                    case SymbolType.Boss:
                        if (stageSymbolData.Param1 > 0)
                        {
                            symbolInfo.SetTroopInfo(BattleTroop(stageSymbolData));
                            if (randFlag)
                            {
                                var numinosGetItem = MakeEnemyRandomNuminos(stageSymbolData.StageId,stageSymbolData.Seek);
                                symbolInfo.TroopInfo.AddGetItemInfo(numinosGetItem);
                            }                    
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
                            getItemInfos.AddRange(relicInfos);
                        }
                        break;
                    case SymbolType.Resource:
                        // 報酬設定
                        getItemInfos.Add(MakeGetItemInfo(GetItemType.Numinous,stageSymbolData.Param1));
                        break;
                    case SymbolType.Actor:
                        // 表示用に報酬設定
                        getItemInfos.Add(MakeGetItemInfo(GetItemType.AddActor,stageSymbolData.Param1));
                        break;
                    case SymbolType.SelectActor:
                        getItemInfos.AddRange(MakeSelectActorGetItemInfos(stageSymbolData.Param2 == 0));
                        break;
                    case SymbolType.Shop:
                        // Rank1,2からランダム設定
                        MakeShopGetItemInfos(getItemInfos,symbolInfo,RankType.ActiveRank2,1);
                        MakeShopGetItemInfos(getItemInfos,symbolInfo,RankType.PassiveRank1,2);
                        MakeShopGetItemInfos(getItemInfos,symbolInfo,RankType.PassiveRank2,3);
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
                            getItemInfos.AddRange(MakeSelectRelicGetItemInfos((RankType)getItemInfo.Param2));
                        } else
                        if (prizeSet.GetItem.Type == GetItemType.SelectAddActor)
                        {
                            getItemInfos.AddRange(MakeSelectActorGetItemInfos(getItemInfo.Param2 == 0));
                        } else
                        if (prizeSet.GetItem.Type == GetItemType.Numinous)
                        {
                            var numinosBonus = PartyInfo.BattleNuminosBonus(stageSymbolData.StageId,stageSymbolData.Seek,WorldType.Main);
                            var data = new GetItemData
                            {
                                Param1 = getItemInfo.Param1 + numinosBonus,
                                Param2 = getItemInfo.Param2,
                                Type = GetItemType.Numinous
                            };
                            getItemInfos.Add(new GetItemInfo(data));
                        } else
                        {
                            getItemInfos.Add(getItemInfo);
                        }
                    }
                }
                symbolInfo.SetGetItemInfos(getItemInfos);
                var record = new SymbolResultInfo(symbolInfo);
                resultInfos.Add(record);
                // ブランチ用データ作成
                var brunchSymbol = new SymbolInfo(stageSymbolData);
                brunchSymbol.CopyData(symbolInfo);
                var brunchRecord = new SymbolResultInfo(brunchSymbol);
                brunchRecord.SetWorldType(WorldType.Brunch);
                resultInfos.Add(brunchRecord);
            }
            return resultInfos;
        }

        private GetItemInfo MakeGetItemInfo(GetItemType getItemType,int param1)
        {
            var getItemData = new GetItemData
            {
                Type = getItemType,
                Param1 = param1
            };
            return new GetItemInfo(getItemData);
        }

        private void MakeShopGetItemInfos(List<GetItemInfo> getItemInfos,SymbolInfo symbolInfo,RankType rankType,int count)
        {
            var alcanaRank = rankType;
            var alcanaIds = PartyInfo.CurrentAlcanaIdList(CurrentStage.Id,CurrentStage.Seek,CurrentStage.WorldType);
            var alcanaSkills = DataSystem.Skills.Where(a => a.Value.Rank == alcanaRank && !alcanaIds.Contains(a.Value.Id)).ToList();
            while (getItemInfos.Count <= count)
            {
                var rand = Random.Range(0,alcanaSkills.Count);
                // 報酬設定
                if (getItemInfos.Find(a => a.Param1 == alcanaSkills[rand].Value.Id) == null)
                {
                    var getItemInfo = MakeGetItemInfo(GetItemType.Skill,alcanaSkills[rand].Value.Id);
                    if (getItemInfos.Find(a => a.Param1 == alcanaSkills[rand].Value.Id) == null)
                    {
                        symbolInfo.SetGetItemInfos(new List<GetItemInfo>(){getItemInfo});
                        getItemInfos.Add(getItemInfo);
                    }
                }
            }
        }

        private List<GetItemInfo> MakeSelectActorGetItemInfos(bool freeSelect)
        {
            var getItemInfos = new List<GetItemInfo>
            {
                // タイトル表示用
                MakeGetItemInfo(GetItemType.SelectAddActor, -1)
            };
            // 表示用に報酬設定
            if (freeSelect)
            {
                // 自由選択
            } else
            {
                // 選択できるアクターが3人まで
                var pastActorIdList = PartyInfo.PastActorIdList(CurrentStage.Id,CurrentStage.Seek,CurrentStage.WorldType);
                var actorInfos = PartyInfo.ActorInfos.FindAll(a => !pastActorIdList.Contains(a.ActorId));
                var count = 3;
                if (actorInfos.Count < count)
                {
                    count = actorInfos.Count;
                }
                if (count == 0)
                {
                    // 報酬設定
                    getItemInfos.Add(MakeGetItemInfo(GetItemType.Numinous,20));
                } else
                {
                    while (getItemInfos.Count <= count)
                    {
                        var rand = Random.Range(0,actorInfos.Count);
                        if (getItemInfos.Find(a => a.Param1 == actorInfos[rand].ActorId) == null)
                        {
                            getItemInfos.Add(MakeGetItemInfo(GetItemType.AddActor,actorInfos[rand].ActorId));
                        }
                    }
                }
            }
            return getItemInfos;
        }

        private List<GetItemInfo> MakeSelectRelicGetItemInfos(RankType rankType)
        {
            var getItemInfos = new List<GetItemInfo>
            {
                // タイトル表示用
                MakeGetItemInfo(GetItemType.SelectRelic, -1)
            };
            var alcanaRank = rankType;
            var alcanaIds = PartyInfo.CurrentAlcanaIdList(CurrentStage.Id,CurrentStage.Seek,CurrentStage.WorldType);
            var alcanaSkills = DataSystem.Skills.Where(a => a.Value.Rank == alcanaRank && !alcanaIds.Contains(a.Value.Id)).ToList();
            var count = 3;
            if (alcanaSkills.Count < count)
            {
                count = alcanaSkills.Count;
            }
            if (count == 0)
            {
                // 報酬設定
                getItemInfos.Add(MakeGetItemInfo(GetItemType.Numinous,20));
            } else
            {
                while (getItemInfos.Count <= count)
                {
                    var rand = Random.Range(0,alcanaSkills.Count);
                    if (getItemInfos.Find(a => a.Param1 == alcanaSkills[rand].Value.Id) == null)
                    {
                        // 報酬設定
                        var getItemInfo = MakeGetItemInfo(GetItemType.Skill,alcanaSkills[rand].Value.Id);
                        if (getItemInfos.Find(a => a.Param1 == alcanaSkills[rand].Value.Id) == null)
                        {
                            getItemInfos.Add(getItemInfo);
                        }
                    }
                }
            }
            return getItemInfos;
        }
        
        private StageSymbolData PickUpSymbolData(List<SymbolGroupData> groupDates)
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

        private StageSymbolData RandomSymbolData(List<StageSymbolData> stageSymbolDates)
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
                            if (!PartyInfo.PastActorIdList(CurrentStage.Id,CurrentStage.Seek,CurrentStage.WorldType).Contains(actorInfo.ActorId))
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
                        if (!PartyInfo.CurrentAlchemyIdList(CurrentStage.Id,CurrentStage.Seek,CurrentStage.WorldType).Contains(symbol.Param1))
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
        
        private TroopInfo BattleTroop(StageSymbolData stageSymbolData)
        {
            var troopId = stageSymbolData.Param1;
            var plusLv = PartyInfo.BattleEnemyLv(stageSymbolData.StageId,stageSymbolData.Seek,WorldType.Main);
            var plusNuminos = PartyInfo.BattleEnemyLv(stageSymbolData.StageId,stageSymbolData.Seek,WorldType.Main);
            var troopInfo = new TroopInfo(troopId,troopId == -1);
            var lv = DataSystem.Stages.Find(a => a.Id == stageSymbolData.StageId).StageLv + plusLv;
            // ランダム生成
            if (troopId == -1)
            {
                troopInfo.MakeEnemyRandomTroopDates(stageSymbolData.Seek + lv);
                var numinosGetItem = MakeEnemyRandomNuminos(stageSymbolData.StageId,stageSymbolData.Seek);
                troopInfo.AddGetItemInfo(numinosGetItem);
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
                troopInfo.MakeEnemyTroopDates(lv);
            }
            return troopInfo;
        }

        private GetItemInfo MakeEnemyRandomNuminos(int stageId,int seek)
        {
            var numinosBonus = PartyInfo.BattleNuminosBonus(stageId,seek,WorldType.Main);
            var totalScore = (int)PartyInfo.TotalScore(WorldType.Main);
            // 確定報酬でNuminos
            var numinosGetItem = new GetItemData
            {
                Param1 = totalScore + seek + numinosBonus,
                Type = GetItemType.Numinous
            };
            return new GetItemInfo(numinosGetItem);
        }

        private GetItemData MakeSkillGetItemInfo()
        {
            int rand = Random.Range(0, 100);
            GetItemData getItemData;
            if (rand < 70)
            {
                getItemData = AddSkillGetItemData(RankType.PassiveRank1);
            }
            else
            if (rand >= 70 && rand < 80)
            {
                getItemData = AddSkillGetItemData(RankType.PassiveRank2);
            }
            else
            if (rand >= 80 && rand < 85)
            {
                getItemData = AddSkillGetItemData(RankType.ActiveRank2);
            }
            else
            if (rand >= 85 && rand < 95)
            {
                getItemData = AddEnhanceSkillGetItemData(RankType.EnhanceRank1);
            }
            else
            {
                getItemData = AddEnhanceSkillGetItemData(RankType.EnhanceRank2);
            }
            // 候補なければ再抽選
            if (getItemData == null)
            {
                return MakeSkillGetItemInfo();
            }
            return getItemData;
        }

        private GetItemData AddSkillGetItemData(RankType rankType)
        {
            var hasSkills = PartyInfo.CurrentAlchemyIdList(CurrentStage.Id,CurrentStage.Seek,CurrentStage.WorldType);
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

        private GetItemData AddEnhanceSkillGetItemData(RankType rankType)
        {
            var hasSkills = PartyInfo.CurrentAlchemyIdList(CurrentStage.Id,CurrentStage.Seek,CurrentStage.WorldType);
            var skillList = new List<SkillData>(DataSystem.Skills.Values);
            var skills = skillList.FindAll(a => a.Rank == rankType && !hasSkills.Contains(a.Id));
            var allSkillIds = PartyInfo.CurrentAllSkillIds(CurrentStage.Id,CurrentStage.Seek,CurrentStage.WorldType);
            // 強化可能な所持魔法に絞る
            skills = skills.FindAll(a => allSkillIds.Contains(a.Id - 400000));
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