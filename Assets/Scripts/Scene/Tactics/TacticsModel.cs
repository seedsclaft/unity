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
        private Dictionary<TacticsCommandType,bool> _tacticsCommandEnables = new ();
        public void SetTacticsCommandEnables(TacticsCommandType tacticsCommand,bool isEnable)
        {
            _tacticsCommandEnables[tacticsCommand] = isEnable;
        }

        public List<ListData> TacticsCommand()
        {
            if (CurrentStage.SurvivalMode)
            {
                SetTacticsCommandEnables(TacticsCommandType.Train,false);
                SetTacticsCommandEnables(TacticsCommandType.Alchemy,false);
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

        public List<ListData> StageRecords(SymbolResultInfo symbolResultInfo)
        {
            var selectRecords = PartyInfo.SymbolRecordList.FindAll(a => a.StageId == symbolResultInfo.StageId && a.StageSymbolData.Seek == symbolResultInfo.Seek);
            selectRecords.Sort((a,b) => a.StageSymbolData.SeekIndex > b.StageSymbolData.SeekIndex ? 1 : -1);
            Func<SymbolResultInfo,bool> enable = (a) => 
            {
                return a.StageSymbolData.Seek == CurrentStage.CurrentTurn;
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

        public void SaveTempBattleMembers()
        {
            TempInfo.CashBattleActors(BattleMembers());
        }


        public List<SkillInfo> AlcanaMagicSkillInfos(List<GetItemInfo> getItemInfos)
        {
            var skillInfos = new List<SkillInfo>();
            foreach (var getItemInfo in getItemInfos)
            {
                var skillInfo = new SkillInfo(getItemInfo.Param1);
                var cost = getItemInfo.Param2;
                skillInfo.SetEnable(cost <= PartyInfo.Currency);
                skillInfos.Add(skillInfo);
            }
            return skillInfos;
        }

        public void SetTempAddActorStatusInfos(int actorId)
        {
            var actorInfos = PartyInfo.ActorInfos.FindAll(a => a.ActorId == actorId);
            TempInfo.SetTempStatusActorInfos(actorInfos);
        }

        public void SetTempAddSelectActorStatusInfos()
        {
            var pastActorIdList = PartyInfo.PastActorIdList(CurrentStage.Id,CurrentStage.CurrentTurn);
            var actorInfos = PartyInfo.ActorInfos.FindAll(a => !pastActorIdList.Contains(a.ActorId));
            TempInfo.SetTempStatusActorInfos(actorInfos);
        }

        public void SetTempAddSelectActorGetItemInfoStatusInfos(List<GetItemInfo> getItemInfos)
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
            TempInfo.SetTempStatusActorInfos(actorInfos);
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
                var stageCommand = new SystemData.CommandData
                {
                    Id = 3,
                    Name = "ステージ選択へ",
                    Key = "Retire"
                };
                list.Add(stageCommand);
            var titleCommand = new SystemData.CommandData
            {
                Id = 4,
                Name = DataSystem.GetText(709),
                Key = "Title"
            };
            list.Add(titleCommand);
            return MakeListData(list);
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
            var selectRecords = PartyInfo.SymbolRecordList.FindAll(a => a.EndFlag == false && a.Seek > 0);
            
            // 現在を挿入
            var currentSeek = CurrentStage.CurrentTurn;
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
            currentInfo.SetLastSelected(true);
            var currentList = new List<SymbolResultInfo>(){currentResult};
    
            var resultList = new List<List<SymbolResultInfo>>();
            var result = recordList.OrderBy(a => a.Key).ToList();
            foreach (var resultData in result)
            {
                resultList.Add(resultData.Value);
            }
            var currentIndex = resultList.FindIndex(a => a[0].StageId == CurrentStage.Id && a[0].Seek == currentSeek);
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
                if (record.Find(a => a.StageId == CurrentStage.Id && a.StageSymbolData.Seek == currentSeek) != null)
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
                Name = DataSystem.GetText(23040),
                Id = 0
            };
            var noCommand = new SystemData.CommandData
            {
                Key = "No",
                Name = DataSystem.GetText(23050),
                Id = 1
            };
            menuCommandDates.Add(noCommand);
            menuCommandDates.Add(yesCommand);            
            Func<SystemData.CommandData,bool> enable = (a) => 
            {
                if (a.Key == "Yes")
                {
                    return PartyInfo.RemakeHistory();
                } else
                if (a.Key == "No")
                {
                    return PartyInfo.ParallelHistory();
                }
                return false;
            };
            return MakeListData(menuCommandDates,enable,-1);
        }
        
        public bool CanParallel()
        {
            return PartyInfo.ParallelCount > 0;
        }

        public void ResetBattlerIndex()
        {
            foreach (var stageMember in StageMembers())
            {
                stageMember.SetBattleIndex(-1);
            }
        }

        public void SetPartyBattlerIdList()
        {
            var idList = new List<int>();
            foreach (var battleMember in BattleMembers())
            {
                idList.Add(battleMember.ActorId);
            }
            PartyInfo.SetLastBattlerIdList(idList);
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
        public int SeekIndex = 0;
    }

    public class TacticsActorInfo
    {
        public ActorInfo ActorInfo;
        public List<ActorInfo> ActorInfos;
        public TacticsCommandType TacticsCommandType;
    }

    public class TacticsCommandData
    {
        public string Title;
    }
}