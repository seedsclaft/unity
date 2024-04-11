using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Ryneus
{
    public class TacticsModel : BaseModel
    {
        public TacticsModel()
        {
            _selectActorId = StageMembers()[0].ActorId;
        }

        private SymbolInfo _symbolInfo;
        public SymbolInfo SymbolInfo => _symbolInfo;
        public void SetSymbolInfo(SymbolInfo symbolInfo)
        {
            _symbolInfo = symbolInfo;
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

        public List<ListData> StageSymbolInfos(int seek)
        {
            var list = new List<SymbolInfo>();
            var symbolInfos = PartyInfo.StageSymbolInfos.FindAll(a => a.StageSymbolData.Seek == seek);
            var selectRecords = PartyInfo.SymbolRecordList.FindAll(a => a.StageId == CurrentStage.Id && a.Selected == true);
            for (int i = 0;i < symbolInfos.Count;i++)
            {
                var symbolInfo = new SymbolInfo();
                symbolInfo.CopyData(symbolInfos[i]);
                var saveRecord = selectRecords.Find(a => a.IsSameSymbol(CurrentStage.Id,symbolInfos[i].StageSymbolData.Seek,symbolInfos[i].StageSymbolData.SeekIndex));
                symbolInfo.SetSelected(saveRecord != null);
                symbolInfo.SetCleared(symbolInfos[i].Cleared);
                MakePrizeData(saveRecord,symbolInfo.GetItemInfos);
                list.Add(symbolInfo);
            }
            list.Sort((a,b) => a.StageSymbolData.SeekIndex > b.StageSymbolData.SeekIndex ? 1 : -1);
            return MakeListData(list);
        }

        public void SetStageSeekIndex(int symbolIndex)
        {
            CurrentStage.SetSeekIndex(symbolIndex);
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
            var isGameOver = false;//PartyInfo.ActorIdList.Count > 0 && (Actors().Find(a => a.ActorId == PartyInfo.ActorIdList[0])).Lost;
            if (isGameOver)
            {
                CurrentStage.SetEndingType(EndingType.C);
                return DataSystem.Adventures.Find(a => a.Id == 21);
            }
            var isAEndGameClear = CurrentStage.StageClear;
            if (isAEndGameClear)
            {
                CurrentStage.SetEndingType(EndingType.A);
                StageClear();
                return DataSystem.Adventures.Find(a => a.Id == 151);
            }
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
                Name = DataSystem.GetText(704),
                Key = "Retire"
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
            return MakeListData(list);
        }

        public List<ListData> TacticsCharacterData()
        {
            var list = new List<TacticsActorInfo>();
            foreach (var member in StageMembers())
            {
                var tacticsActorInfo = new TacticsActorInfo
                {
                    TacticsCommandType = _TacticsCommandType,
                    ActorInfo = member,
                    ActorInfos = StageMembers()
                };
                list.Add(tacticsActorInfo);
            }
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
                Description = CommandDescription(),
                Rank = CommandRank()
            };
            return tacticsCommandData;
        }

        private string CommandTitle()
        {
            return DataSystem.GetText((int)_TacticsCommandType);
        }

        private int CommandRank()
        {
            return 0;
        }

        private string CommandDescription()
        {
            int rank = CommandRank();
            if (rank > 0)
            {
                // %の確率で
                var bonusParam = rank * 10;
                return DataSystem.GetReplaceText(10 + (int)_TacticsCommandType,bonusParam.ToString());
            }
            int count = 0;
            switch (_TacticsCommandType)
            {
                case TacticsCommandType.Train:
                    count = DataSystem.System.TrainCount;
                    break;
                case TacticsCommandType.Alchemy:
                    count = DataSystem.System.AlchemyCount;
                    break;
                    /*
                case TacticsCommandType.Recovery:
                    count = DataSystem.System.RecoveryCount;
                    break;
                    */
            }
            return DataSystem.GetReplaceText(10,count.ToString());
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
            var symbolInfoList = new List<List<SymbolInfo>>();
            
            var stageSeekList = new List<int>();
            foreach (var symbolInfo in PartyInfo.StageSymbolInfos)
            {
                if (!stageSeekList.Contains(symbolInfo.StageSymbolData.Seek))
                {
                    stageSeekList.Add(symbolInfo.StageSymbolData.Seek);
                }
            }
            foreach (var stageSeek in stageSeekList)
            {
                var list = new List<SymbolInfo>();
                symbolInfoList.Add(list);
            }
            var selectRecords = PartyInfo.SymbolRecordList.FindAll(a => a.StageId == CurrentStage.Id && a.Selected == true);
            var lastSelectSeek = selectRecords.Count > 0 ? selectRecords.Select(a => a.Seek).Max() : -1;
            foreach (var stageSymbolInfo in PartyInfo.StageSymbolInfos)
            {
                var symbolInfo = new SymbolInfo();
                symbolInfo.CopyData(stageSymbolInfo);
                var saveRecord = selectRecords.Find(a => a.IsSameSymbol(stageSymbolInfo.StageSymbolData.StageId,stageSymbolInfo.StageSymbolData.Seek,stageSymbolInfo.StageSymbolData.SeekIndex));
                symbolInfo.SetSelected(saveRecord != null);
                //symbolInfo.SetLastSelected(saveRecord != null && lastSelectSeek == symbolInfo.StageSymbolData.Seek);
                symbolInfo.SetPast(saveRecord == null && stageSymbolInfo.StageSymbolData.Seek <= lastSelectSeek);
                if (saveRecord != null)
                {
                    MakePrizeData(saveRecord,symbolInfo.GetItemInfos);
                }
                symbolInfoList[symbolInfo.StageSymbolData.Seek-1].Add(symbolInfo);
            }
            // 現在を挿入
            var seekIndex = CurrentStage.CurrentTurn;
            var currentSymbol = new StageSymbolData
            {
                Seek = seekIndex,
                SeekIndex = 0,
                SymbolType = SymbolType.None
            };
            var currentInfo = new SymbolInfo(currentSymbol);
            currentInfo.SetLastSelected(true);
            var currentList = new List<SymbolInfo>(){currentInfo};
            symbolInfoList.Insert(seekIndex-1,currentList);

            var listData = new List<ListData>();
            foreach (var symbolInfos1 in symbolInfoList)
            {
                var list = new ListData(symbolInfos1);
                list.SetSelected(false);
                list.SetEnable(false);
                if (symbolInfos1.Find(a => a.StageSymbolData.Seek == seekIndex) != null)
                {
                    list.SetSelected(true);
                }
                listData.Add(list);
            }
            return listData;
        }

        public List<ListData> ParallelCommand()
        {
            return MakeListData(BaseConfirmCommand(23050,23040));
        }
        
        public bool CanParallel()
        {
            return PartyInfo.Currency >= PartyInfo.ParallelCost();
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
                    return (viewEvent.commandType == Tactics.CommandType.TacticsCommand && (TacticsCommandType)viewEvent.template == TacticsCommandType.Train);
                case TutorialType.TacticsCommandAlchemy:
                    return (viewEvent.commandType == Tactics.CommandType.TacticsCommand && (TacticsCommandType)viewEvent.template == TacticsCommandType.Alchemy);
                /*
                case TutorialType.TacticsCommandRecover:
                    return (viewEvent.commandType == Tactics.CommandType.TacticsCommand && (TacticsCommandType)viewEvent.template == TacticsCommandType.Recovery);
                */
                case TutorialType.TacticsSelectTacticsActor:
                    return (viewEvent.commandType == Tactics.CommandType.SelectTacticsActor);
                case TutorialType.TacticsSelectTacticsDecide:
                    return (viewEvent.commandType == Tactics.CommandType.TacticsCommandClose);
                case TutorialType.TacticsSelectEnemy:
                    return (viewEvent.commandType == Tactics.CommandType.SelectSymbol);
                case TutorialType.TacticsSelectAlchemyMagic:
                    return (viewEvent.commandType == Tactics.CommandType.SkillAlchemy && (SkillInfo)viewEvent.template != null);
                
            }
            return false;
        }
    }

    public class TacticsActorInfo{
        public ActorInfo ActorInfo;
        public List<ActorInfo> ActorInfos;
        public TacticsCommandType TacticsCommandType;
    }

    public class TacticsCommandData{
        public string Title;
        public int Rank;
        public string Description;
    }
}