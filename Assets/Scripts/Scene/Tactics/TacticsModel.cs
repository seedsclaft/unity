using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Ryneus
{
    public class TacticsModel : BaseModel
    {
        private TacticsSceneInfo _sceneParam;
        public TacticsSceneInfo SceneParam => _sceneParam;
        public TacticsModel()
        {
            _sceneParam = (TacticsSceneInfo)GameSystem.SceneStackManager.LastSceneParam;
            SetFirstBattleActorId();
        }

        public void SetFirstBattleActorId()
        {
            var stageMembers = StageMembers();
            if (stageMembers.Count > 0)
            {
                var firstBattler = stageMembers.Find(a => a.BattleIndex == 1);
                if (firstBattler != null)
                {
                    SetSelectActorId(firstBattler.ActorId);
                } else
                {
                    SetSelectActorId(stageMembers[0].ActorId);
                }
            }
        }
        
        private int _selectActorId = 0;
        public void SetSelectActorId(int actorId)
        {
            _selectActorId = actorId;
        }    
        public ActorInfo TacticsActor()
        {
            return StageMembers().Find(a => a.ActorId == _selectActorId);
        }

        private TacticsCommandType _TacticsCommandType = TacticsCommandType.Train;
        public TacticsCommandType TacticsCommandType => _TacticsCommandType;
        public void SetTacticsCommandType(TacticsCommandType tacticsCommandType)
        {
            _TacticsCommandType = tacticsCommandType;
        }
        private int _firstRecordIndex = -1;
        public int FirstRecordIndex => _firstRecordIndex;

        private List<int> _shopSelectIndexes = new ();


        private Dictionary<TacticsCommandType,bool> _tacticsCommandEnables = new ();
        public void SetTacticsCommandEnables(TacticsCommandType tacticsCommand,bool isEnable)
        {
            _tacticsCommandEnables[tacticsCommand] = isEnable;
        }

        public List<ListData> TacticsCommand()
        {
            if (StageMembers().Count == 0)
            {
                SetTacticsCommandEnables(TacticsCommandType.Train,false);
                SetTacticsCommandEnables(TacticsCommandType.Alchemy,false);
                SetTacticsCommandEnables(TacticsCommandType.Status,false);
                //SetTacticsCommandEnables(TacticsCommandType.Recovery,false);
            }
            var commandListDates = MakeListData(DataSystem.TacticsCommand);
            foreach (var commandListData in commandListDates)
            {
                var commandData = (SystemData.CommandData)commandListData.Data;
                if (_tacticsCommandEnables.ContainsKey((TacticsCommandType)commandData.Id))
                {
                    commandListData.SetEnable(_tacticsCommandEnables[(TacticsCommandType)commandData.Id]);
                }
            }
            return commandListDates;
        }

        public ListData ChangeEnableCommandData(int index,bool enable)
        {
            return new ListData(DataSystem.TacticsCommand[index],index,enable);
        }

        public List<ListData> StageResultInfos(SymbolResultInfo symbolResultInfo)
        {
            var selectRecords = PartyInfo.SymbolRecordList.FindAll(a => a.IsSameStageSeek(symbolResultInfo.StageId,symbolResultInfo.Seek,symbolResultInfo.WorldType));
            selectRecords.Sort((a,b) => a.SeekIndex > b.SeekIndex ? 1 : -1);
            Func<SymbolResultInfo,bool> enable = (a) => 
            {
                var enable = false;
                if (a.StageId == CurrentStage.Id && a.Seek <= CurrentStage.Seek)
                {
                    enable = true;
                }
                if (a.StageId < CurrentStage.Id)
                {
                    enable = true;
                }
                return a.Seek == CurrentStage.Seek || enable;
            };
            var seekIndex = 0;
            if (CurrentSelectRecord() != null)
            {
                seekIndex = CurrentSelectRecord().SeekIndex;
            }
            return MakeListData(selectRecords,enable,seekIndex);
        }

        public void SetStageSeekIndex(int seekIndex)
        {
            CurrentStage.SetSeekIndex(seekIndex);
        }

        public List<SkillInfo> AlcanaMagicSkillInfos(List<GetItemInfo> getItemInfos)
        {
            var skillInfos = new List<SkillInfo>();
            foreach (var getItemInfo in getItemInfos)
            {
                if (getItemInfo.GetItemType == GetItemType.Skill)
                {
                    var skillInfo = new SkillInfo(getItemInfo.Param1);
                    var cost = 0;
                    skillInfo.SetEnable(cost <= Currency);
                    skillInfos.Add(skillInfo);
                }
            }
            return skillInfos;
        }

        public void MakeSelectRelic(int skillId)
        {
            var getItemInfos = CurrentSelectRecord().SymbolInfo.GetItemInfos;
            var selectRelicInfos = getItemInfos.FindAll(a => a.GetItemType == GetItemType.Skill);
            // 魔法取得
            var selectRelic = selectRelicInfos.Find(a => a.Param1 == skillId);
            foreach (var selectRelicInfo in selectRelicInfos)
            {
                selectRelicInfo.SetGetFlag(false);
            }
            selectRelic.SetGetFlag(true);
        }

        public List<SkillInfo> ShopMagicSkillInfos(List<GetItemInfo> getItemInfos)
        {
            var skillInfos = new List<SkillInfo>();
            foreach (var getItemInfo in getItemInfos)
            {
                var skillInfo = new SkillInfo(getItemInfo.Param1);
                var cost = ShopLearningCost(skillInfo);
                skillInfo.SetEnable(cost <= (Currency - LearningShopMagicCost()) && !_shopSelectIndexes.Contains(skillInfo.Id));
                skillInfo.SetLearningCost(cost);
                skillInfos.Add(skillInfo);
            }
            return skillInfos;
        }

        public List<ActorInfo> AddActorInfos(int actorId)
        {
            return PartyInfo.ActorInfos.FindAll(a => a.ActorId == actorId);
        }

        public List<ActorInfo> AddSelectActorInfos()
        {
            var pastActorIdList = PartyInfo.CurrentActorIdList(CurrentStage.Id,CurrentStage.Seek,CurrentStage.WorldType);
            // 違うworldTypeのActorIdも含まない
            var worldType = CurrentStage.WorldType == WorldType.Main ? WorldType.Brunch : WorldType.Main;
            var anotherActorIdList = PartyInfo.CurrentActorIdList(99,99,worldType);

            pastActorIdList.AddRange(anotherActorIdList);
            return PartyInfo.ActorInfos.FindAll(a => !pastActorIdList.Contains(a.ActorId));
        }

        public List<ActorInfo> AddSelectActorGetItemInfos(List<GetItemInfo> getItemInfos)
        {
            var actorInfos = new List<ActorInfo>();
            foreach (var getItemInfo in getItemInfos)
            {
                if (getItemInfo.GetItemType == GetItemType.AddActor)
                {
                    var actorInfo = PartyInfo.ActorInfos.Find(a => a.ActorId == getItemInfo.Param1);
                    actorInfos.Add(actorInfo);
                }
            }
            return actorInfos;
        }

        public AdvData StartTacticsAdvData()
        {
            var isGameOver = false;//PartyInfo.ActorIdList.Count > 0 && (Actors().Find(a => a.ActorId == PartyInfo.ActorIdList[0])).Lost;
            if (isGameOver)
            {
                CurrentStage.SetEndingType(EndingType.C);
                return DataSystem.Adventures.Find(a => a.Id == 21);
            }
            /*
            var isAEndGameClear = CurrentStage.StageClear;
            if (isAEndGameClear)
            {
                CurrentStage.SetEndingType(EndingType.A);
                StageClear();
                return DataSystem.Adventures.Find(a => a.Id == 151);
            }
            */
            var isBEndGameClear = CurrentStage.ClearTroopIds.Contains(4010);
            if (isBEndGameClear)
            {
                CurrentStage.SetEndingType(EndingType.B);
                return DataSystem.Adventures.Find(a => a.Id == 152);
            }
            return null;
        }

        public List<ListData> SideMenu()
        {
            var list = new List<SystemData.CommandData>();
            var retire = new SystemData.CommandData
            {
                Id = 1,
                Name = DataSystem.GetText(13410),
                Key = "Option"
            };
            list.Add(retire);
            var menuCommand = new SystemData.CommandData
            {
                Id = 2,
                Name = DataSystem.GetText(19700),
                Key = "Help"
            };
            list.Add(menuCommand);
            var dictionaryCommand = new SystemData.CommandData
            {
                Id = 11,
                Name = DataSystem.GetText(19730),
                Key = "Dictionary"
            };
            list.Add(dictionaryCommand);
            var saveCommand = new SystemData.CommandData
            {
                Id = 3,
                Name = DataSystem.GetText(19710),
                Key = "Save"
            };
            list.Add(saveCommand);
            var titleCommand = new SystemData.CommandData
            {
                Id = 4,
                Name = DataSystem.GetText(19720),
                Key = "Title"
            };
            list.Add(titleCommand);
            Func<SystemData.CommandData,bool> enable = (a) => 
            {
                if (a.Key == "Save" || a.Key == "Retire")
                {
                    return PartyInfo.ReturnSymbol == null;
                }
                return true;
            };
            return MakeListData(list,enable);
        }

        public string TacticsCommandInputInfo()
        {
            switch (_TacticsCommandType)
            {
                case TacticsCommandType.Train:
                    return "TRAIN";
                case TacticsCommandType.Alchemy:
                    return "ALCHEMY";
                    /*
                case TacticsCommandType.Recovery:
                    return "RECOVERY";
                    */
            }
            return "";
        }

        public void AssignBattlerIndex()
        {
            var idList = PartyInfo.LastBattlerIdList;
            var idx = 1;
            foreach (var id in idList)
            {
                var actor = StageMembers().Find(a => a.ActorId == id);
                if (actor != null)
                {
                    actor.SetBattleIndex(idx);
                    idx++;
                }
            }
        }

        /// <summary>
        /// 表示するステージデータ
        /// </summary>
        /// <returns></returns>
        public List<ListData> SymbolRecords()
        {
            var symbolInfos = new List<SymbolInfo>();
            var recordList = new Dictionary<int,List<SymbolResultInfo>>();
            
            var stageSeekList = new List<int>();
            var selectRecords = PartyInfo.SymbolRecordList.FindAll(a => a.StageId > 0);
            selectRecords = selectRecords.FindAll(a => a.WorldType == CurrentStage.WorldType);
            // ブランチは始点と終点を作る
            if (CurrentStage.WorldType == WorldType.Brunch)
            {
                var brunchSymbol = PartyInfo.BrunchBaseSymbol;
                var returnSymbol = PartyInfo.ReturnSymbol;
                selectRecords = selectRecords.FindAll(a => a.IsBeforeStageSeek(returnSymbol.StageId,returnSymbol.Seek,WorldType.Brunch) && a.IsAfterStageSeek(brunchSymbol.StageId,brunchSymbol.Seek,WorldType.Brunch));
            }
            // 現在を挿入
            var currentSeek = CurrentStage.Seek;
            // ストック数
            var stockCount = PartyInfo.StageStockCount;
            foreach (var selectRecord in selectRecords)
            {
                if (stageSeekList.Count >= stockCount)
                {
                    //continue;
                }
                var stageKey = (selectRecord.StageId-1)*100 + selectRecord.Seek;
                if (!stageSeekList.Contains(stageKey))
                {
                    stageSeekList.Add(stageKey);
                }
            }    
            stageSeekList.Sort((a,b) => a - b > 0 ? 1 : -1);
            
            foreach (var stageSeek in stageSeekList)
            {
                var list = new List<SymbolResultInfo>();
                recordList[stageSeek] = new List<SymbolResultInfo>();
            }
            var lastSelectSeek = selectRecords.Count > 0 ? selectRecords.Select(a => a.Seek).Max() : -1;
            foreach (var selectRecord in selectRecords)
            {
                var stageKey = (selectRecord.StageId-1)*100 + selectRecord.Seek;
                if (recordList.ContainsKey(stageKey))
                {
                    recordList[stageKey].Add(selectRecord);
                }
            }
            var currentSymbol = new StageSymbolData
            {
                StageId = CurrentStage.Id,
                Seek = currentSeek,
                SeekIndex = 0,
                SymbolType = SymbolType.None
            };
            var currentInfo = new SymbolInfo(currentSymbol);
            var currentResult = new SymbolResultInfo(currentInfo);
            currentResult.SetWorldType(CurrentStage.WorldType);
            currentInfo.SetLastSelected(true);
            var currentList = new List<SymbolResultInfo>(){currentResult};
    
            var resultList = new List<List<SymbolResultInfo>>();
            var result = recordList.OrderBy(a => a.Key).ToList();
            foreach (var resultData in result)
            {
                resultList.Add(resultData.Value);
            }
            var currentIndex = resultList.FindIndex(a => a[0].IsSameStageSeek(CurrentStage.Id,currentSeek,CurrentStage.WorldType));
            if (currentIndex > -1)
            {
                resultList.Insert(currentIndex, currentList);
            } else
            {
                resultList.Add(currentList);
            }
            var listData = new List<ListData>();
            foreach (var record in resultList)
            {
                var list = new ListData(record);
                list.SetSelected(false);
                list.SetEnable(false);
                if (record.Find(a => a.IsSameStageSeek(CurrentStage.Id,currentSeek,CurrentStage.WorldType)) != null)
                {
                    list.SetSelected(true);
                }
                listData.Add(list);
            }
            _firstRecordIndex = listData.FindIndex(a => a.Selected);
            return listData;
        }

        public void ResetBattlerIndex()
        {
            foreach (var stageMember in StageMembers())
            {
                //stageMember.SetBattleIndex(-1);
            }
        }

        public bool EnableShopMagic(SkillInfo skillInfo)
        {
            var cost = ShopLearningCost(skillInfo);
            return cost <= (Currency - LearningShopMagicCost());
        }

        public void PayShopCurrency(SkillInfo skillInfo)
        {
            if (EnableShopMagic(skillInfo))
            {
                var cost = ShopLearningCost(skillInfo);
                var getItemInfo = CurrentSelectRecord().SymbolInfo.GetItemInfos.Find(a => a.Param1 == skillInfo.Id);
                getItemInfo?.SetResultParam(cost);
                //getItemInfo.SetGetFlag(true);
                _shopSelectIndexes.Add(skillInfo.Id);
            }
        }

        public bool IsSelectedShopMagic(SkillInfo skillInfo)
        {
            return _shopSelectIndexes.Contains(skillInfo.Id);
        }

        public void CancelShopCurrency(SkillInfo skillInfo)
        {
            if (EnableShopMagic(skillInfo))
            {
                var getItemInfo = CurrentSelectRecord().SymbolInfo.GetItemInfos.Find(a => a.Param1 == skillInfo.Id);
                getItemInfo?.SetResultParam(0);
                //getItemInfo.SetGetFlag(false);
                _shopSelectIndexes.Remove(skillInfo.Id);
            }
        }

        public int ShopLearningCost(SkillInfo skillInfo)
        {
            if (skillInfo.Master.Rank == RankType.ActiveRank2 || skillInfo.Master.Rank == RankType.PassiveRank2 || skillInfo.Master.Rank == RankType.EnhanceRank2)
            {
                return 20;
            }
            return 10;
        }

        public List<GetItemInfo> LearningShopMagics()
        {
            var list = new List<GetItemInfo>();
            foreach (var getItemInfo in CurrentSelectRecord().SymbolInfo.GetItemInfos)
            {
                if (_shopSelectIndexes.Contains(getItemInfo.Param1))
                {
                    list.Add(getItemInfo);
                }
            }
            return list;
        }

        public int LearningShopMagicCost()
        {
            int cost = 0;
            foreach (var getItemInfo in LearningShopMagics())
            {
                cost += getItemInfo.ResultParam;
            }
            return cost;
        }

        public void CommandNormalWorld()
        {
            SetWorldCurrentStage(WorldType.Main);
        }

        public void CommandAnotherWorld()
        {
            SetWorldCurrentStage(WorldType.Brunch);
        }

        private void SetWorldCurrentStage(WorldType worldType)
        {
            CurrentStage.SetWorldType(worldType);
            var symbolData = worldType == WorldType.Main ? PartyInfo.ReturnSymbol : PartyInfo.BrunchSymbol;
            CurrentStage.SetStageId(symbolData.StageId);
            CurrentStage.SetCurrentTurn(symbolData.Seek);
            SetStageSeek();
        }

        public bool CheckTutorial(TacticsViewEvent viewEvent)
        {
            if (CurrentStageTutorialDates.Count == 0)
            {
                return false;
            }
            var tutorial = CurrentStageTutorialDates[0];
            switch (tutorial.Type)
            {
                case TutorialType.TacticsCommandTrain:
                    return (viewEvent.commandType == Tactics.CommandType.SelectTacticsCommand && (TacticsCommandType)viewEvent.template == TacticsCommandType.Train);
                case TutorialType.TacticsCommandAlchemy:
                    return (viewEvent.commandType == Tactics.CommandType.SelectTacticsCommand && (TacticsCommandType)viewEvent.template == TacticsCommandType.Alchemy);
                /*
                case TutorialType.TacticsCommandRecover:
                    return (viewEvent.commandType == Tactics.CommandType.TacticsCommand && (TacticsCommandType)viewEvent.template == TacticsCommandType.Recovery);
                */
                //case TutorialType.TacticsSelectTacticsActor:
                //    return (viewEvent.commandType == Tactics.CommandType.SelectTacticsActor);
                case TutorialType.TacticsSelectEnemy:
                    return (viewEvent.commandType == Tactics.CommandType.SelectSymbol);
                //case TutorialType.TacticsSelectAlchemyMagic:
                //    return (viewEvent.commandType == Tactics.CommandType.SkillAlchemy && (SkillInfo)viewEvent.template != null);
                
            }
            return false;
        }
    }

    public class TacticsSceneInfo
    {
        // バトル直前に戻る
        public bool ReturnBeforeBattle;
        public bool ReturnNextBattle;
        public int SeekIndex = 0;
    }

    public class TacticsActorInfo
    {
        public ActorInfo ActorInfo;
        public List<ActorInfo> ActorInfos;
        public TacticsCommandType TacticsCommandType;
        public string DisableText;
    }

    public class TacticsCommandData
    {
        public string Title;
    }
}