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
        }
        
        public ActorInfo TacticsActor()
        {
            return StageMembers()[0];
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
            var selectRecords = PartyInfo.SymbolRecordList.FindAll(a => a.IsSameStageSeek(symbolResultInfo.StageId,symbolResultInfo.Seek,symbolResultInfo.WorldNo));
            selectRecords.Sort((a,b) => a.StageSymbolData.SeekIndex > b.StageSymbolData.SeekIndex ? 1 : -1);
            Func<SymbolResultInfo,bool> enable = (a) => 
            {
                var enable = false;
                if (PartyInfo.RemakeHistory())
                {
                    if (a.StageSymbolData.StageId == CurrentStage.Id && a.StageSymbolData.Seek <= CurrentStage.CurrentSeek)
                    {
                        enable = true;
                    }
                    if (a.StageSymbolData.StageId < CurrentStage.Id)
                    {
                        enable = true;
                    }
                }
                return a.StageSymbolData.Seek == CurrentStage.CurrentSeek || enable;
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
                var skillInfo = new SkillInfo(getItemInfo.Param1);
                var cost = 0;
                skillInfo.SetEnable(cost <= PartyInfo.Currency);
                skillInfos.Add(skillInfo);
            }
            return skillInfos;
        }

        public void MakeSelectRelic(int skillId)
        {
            var getItemInfos = CurrentSelectRecord().SymbolInfo.GetItemInfos;
            var selectRelicInfos = getItemInfos.FindAll(a => a.GetItemType == GetItemType.SelectRelic);
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
            var index = 0;
            foreach (var getItemInfo in getItemInfos)
            {
                var skillInfo = new SkillInfo(getItemInfo.Param1);
                var cost = ShopLearningCost(skillInfo);
                skillInfo.SetEnable(cost <= PartyInfo.Currency && !_shopSelectIndexes.Contains(index));
                skillInfo.SetLearningCost(cost);
                skillInfos.Add(skillInfo);
                index++;
            }
            return skillInfos;
        }

        public List<ActorInfo> AddActorInfos(int actorId)
        {
            return PartyInfo.ActorInfos.FindAll(a => a.ActorId == actorId);
        }

        public List<ActorInfo> AddSelectActorInfos()
        {
            var pastActorIdList = PartyInfo.CurrentActorIdList(CurrentStage.Id,CurrentStage.CurrentSeek,CurrentStage.WorldNo);
            // 違うworldNoのActorIdも含まない
            var worldNo = CurrentStage.WorldNo == 0 ? 1 : 0;
            var anotherActorIdList = PartyInfo.CurrentActorIdList(99,99,worldNo);

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
            /*
            if (CurrentStage.SurvivalMode)
            {
                var isSurvivalGameOver = StageMembers().Count > 0 && !Actors().Exists(a => a.Lost == false);
                if (isSurvivalGameOver)
                {
                    CurrentStage.SetEndingType(EndingType.C);
                    return DataSystem.Adventures.Find(a => a.Id == 21);
                }
                var isSurvivalClear = RemainTurns <= 0;
                if (isSurvivalClear)
                {
                    CurrentStage.SetEndingType(EndingType.A);
                    StageClear();
                    return DataSystem.Adventures.Find(a => a.Id == 151);
                }
                return null;
            }
            */
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
                StageClear();
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
                Name = DataSystem.GetText(701),
                Key = "Option"
            };
            list.Add(retire);
            var menuCommand = new SystemData.CommandData
            {
                Id = 2,
                Name = DataSystem.GetText(703),
                Key = "Help"
            };
            list.Add(menuCommand);
            if (CurrentStage.SurvivalMode == false)
            {
                var saveCommand = new SystemData.CommandData
                {
                    Id = 3,
                    Name = DataSystem.GetText(707),
                    Key = "Save"
                };
                list.Add(saveCommand);
            }
            var titleCommand = new SystemData.CommandData
            {
                Id = 4,
                Name = DataSystem.GetText(709),
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

        public TacticsCommandData TacticsCommandData()
        {
            var tacticsCommandData = new TacticsCommandData
            {
                Title = CommandTitle(),
            };
            return tacticsCommandData;
        }

        private string CommandTitle()
        {
            return DataSystem.GetText((int)_TacticsCommandType);
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

        public List<ListData> SymbolRecords()
        {
            var symbolInfos = new List<SymbolInfo>();
            var recordList = new Dictionary<int,List<SymbolResultInfo>>();
            
            var stageSeekList = new List<int>();
            var selectRecords = PartyInfo.SymbolRecordList.FindAll(a => a.EndFlag == false && a.Seek > 0 || a.EndFlag == false && PartyInfo.EnableMultiverse() && a.Seek == 0);
            selectRecords = selectRecords.FindAll(a => a.WorldNo == CurrentStage.WorldNo);
            // 現在を挿入
            var currentSeek = CurrentStage.CurrentSeek;
            // ストック数
            var stockCount = PartyInfo.StageStockCount;
            foreach (var selectRecord in selectRecords)
            {
                if (stageSeekList.Count >= stockCount)
                {
                    //continue;
                }
                var stageKey = (selectRecord.StageSymbolData.StageId-1)*100 + selectRecord.StageSymbolData.Seek;
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
                var stageKey = (selectRecord.StageSymbolData.StageId-1)*100 + selectRecord.StageSymbolData.Seek;
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
            var currentInfo = new SymbolInfo(SymbolType.None);
            var currentResult = new SymbolResultInfo(currentInfo,currentSymbol,0);
            currentResult.SetWorldNo(CurrentStage.WorldNo);
            currentInfo.SetLastSelected(true);
            var currentList = new List<SymbolResultInfo>(){currentResult};
    
            var resultList = new List<List<SymbolResultInfo>>();
            var result = recordList.OrderBy(a => a.Key).ToList();
            foreach (var resultData in result)
            {
                resultList.Add(resultData.Value);
            }
            var currentIndex = resultList.FindIndex(a => a[0].IsSameStageSeek(CurrentStage.Id,currentSeek,CurrentStage.WorldNo));
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
                if (record.Find(a => a.IsSameStageSeek(CurrentStage.Id,currentSeek,CurrentStage.WorldNo)) != null)
                {
                    list.SetSelected(true);
                }
                listData.Add(list);
            }
            _firstRecordIndex = listData.FindIndex(a => a.Selected);
            return listData;
        }

        public List<ListData> ParallelCommand()
        {
            var menuCommandDates = new List<SystemData.CommandData>();
            var yesCommand = new SystemData.CommandData
            {
                Key = "Yes",
                Name = DataSystem.GetText(23050),
                Id = 0
            };
            var noCommand = new SystemData.CommandData
            {
                Key = "No",
                Name = DataSystem.GetText(23040),
                Id = 1
            };
            menuCommandDates.Add(noCommand);
            menuCommandDates.Add(yesCommand);            
            Func<SystemData.CommandData,bool> enable = (a) => 
            {
                if (a.Key == "Yes")
                {
                    return PartyInfo.ParallelHistory();
                } else
                if (a.Key == "No")
                {
                    return PartyInfo.RemakeHistory();
                }
                return false;
            };
            return MakeListData(menuCommandDates,enable,-1);
        }
        
        public bool RemakeHistory()
        {
            return PartyInfo.RemakeHistory();
        }

        public bool ParallelHistory()
        {
            return PartyInfo.ParallelHistory();
        }

        public void ResetBattlerIndex()
        {
            foreach (var stageMember in StageMembers())
            {
                stageMember.SetBattleIndex(-1);
            }
        }

        public bool EnableShopMagic(SkillInfo skillInfo)
        {
            var cost = ShopLearningCost(skillInfo);
            return cost <= PartyInfo.Currency;
        }

        public void PayShopCurrency(SkillInfo skillInfo,int index)
        {
            if (EnableShopMagic(skillInfo))
            {
                PartyInfo.ChangeCurrency(Currency - ShopLearningCost(skillInfo));
                _shopSelectIndexes.Add(index);
            }
        }

        public int ShopLearningCost(SkillInfo skillInfo)
        {
            if (skillInfo.Master.Rank >= 20)
            {
                return 20;
            }
            return 10;
        }

        public List<GetItemInfo> LearningShopMagics()
        {
            var list = new List<GetItemInfo>();
            int index = 0;
            foreach (var getItemInfo in CurrentSelectRecord().SymbolInfo.GetItemInfos)
            {
                if (_shopSelectIndexes.Contains(index))
                {
                    list.Add(getItemInfo);
                }
                index++;
            }
            return list;
        }

        public void CommandNormalWorld()
        {
            SetWorldCurrentStage(0);
        }

        public void CommandAnotherWorld()
        {
            SetWorldCurrentStage(1);
        }

        private void SetWorldCurrentStage(int worldNo)
        {
            CurrentStage.SetWorldNo(worldNo);
            // 進捗度を調整
            var selectedRecords = PartyInfo.SymbolRecordList.FindAll(a => a.Selected && a.WorldNo == worldNo);
            var selectRecords = PartyInfo.SymbolRecordList.FindAll(a => a.WorldNo == worldNo);
            
            if (selectedRecords.Count > 0)
            {
                var lastRecord = selectRecords[selectRecords.Count-1];
                foreach (var selectRecord in selectRecords)
                {
                    if (selectedRecords.Find(a => a.StageId == selectRecord.StageId && a.Seek == selectRecord.Seek) == null)
                    {
                        if (selectRecord.StageId < lastRecord.StageId)
                        {
                            lastRecord = selectRecord;
                            continue;
                        }
                        
                        if (selectRecord.Seek < lastRecord.Seek)
                        {
                            lastRecord = selectRecord;
                            continue;
                        }
                    }
                }
                CurrentStage.SetStageId(lastRecord.StageId);
                CurrentStage.SetCurrentTurn(lastRecord.Seek);
            } else
            {
                CurrentStage.SetStageId(0);
                CurrentStage.SetCurrentTurn(0);  
            }
            SetStageSeek();
        }

        public void SetSurvivalMode()
        {
            CurrentStage.SetSurvivalMode();
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