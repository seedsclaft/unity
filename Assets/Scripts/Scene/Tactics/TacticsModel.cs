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


        private int _selectSkillId = 0;
        public int SelectAlcanaSkillId => _selectSkillId;

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


        private int _stageSeekIndex = -1; 
        
        private bool _needAllTacticsCommand = false;
        public bool NeedAllTacticsCommand => _needAllTacticsCommand;
        public void SetNeedAllTacticsCommand(bool isNeed)
        {
            _needAllTacticsCommand = isNeed;
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
                var saveRecord = selectRecords.Find(a => a.IsSameSymbol(CurrentStage.Id,seek+1,i));
                symbolInfo.SetSelected(saveRecord != null);
                symbolInfo.SetCleared(symbolInfos[i].Cleared);
                MakePrizeData(saveRecord,symbolInfo.GetItemInfos);
                list.Add(symbolInfo);
            }
            return MakeListData(list);
        }



        public void RefreshTacticsEnable()
        {
            for (int i = 0;i < StageMembers().Count;i++)
            {
                foreach(var tacticsCommandType in Enum.GetValues(typeof(TacticsCommandType)))
                {
                    if ((int)tacticsCommandType != 0)
                    {
                        StageMembers()[i].RefreshTacticsEnable((TacticsCommandType)tacticsCommandType,CanTacticsCommand((TacticsCommandType)tacticsCommandType,StageMembers()[i]));
                    }       
                }
            }
        }

        private bool CanTacticsCommand(TacticsCommandType tacticsCommandType,ActorInfo actorInfo)
        {
            if (tacticsCommandType == TacticsCommandType.Paradigm)
            {
                return true;
            }
            if (tacticsCommandType == TacticsCommandType.Train)
            {
                return Currency >= TacticsUtility.TrainCost(actorInfo);
            }
            if (tacticsCommandType == TacticsCommandType.Alchemy)
            {
                return true;
            }
            /*
            if (tacticsCommandType == TacticsCommandType.Recovery)
            {
                return Currency > 0;
            }
            */
            return false;
        }

        public bool CheckActorTrain()
        {
            var actorInfo = TacticsActor();
            var cost = TacticsUtility.TrainCost(actorInfo);
            return Currency >= cost;
        }

        public int SelectActorEvaluate()
        {
            return TacticsActor().Evaluate();
        }

        public void SelectActorTrain()
        {
            var actorInfo = TacticsActor();
            var cost = TacticsUtility.TrainCost(actorInfo);
            actorInfo.LevelUp(0);
            actorInfo.GainNuminousCost(cost);
            actorInfo.GainLevelUpCost(cost);
            PartyInfo.ChangeCurrency(Currency - cost);
        }

        public void LearnMagic(int skillId)
        {
            var actorInfo = TacticsActor();
            var skillInfo = new SkillInfo(skillId);
            var learningCost = TacticsUtility.LearningMagicCost(actorInfo,skillInfo.Attribute,StageMembers());
            PartyInfo.ChangeCurrency(PartyInfo.Currency - learningCost);
            actorInfo.LearnSkill(skillInfo.Id);
            actorInfo.GainNuminousCost(learningCost);
        }

        public void SelectActorParadigm()
        {
            var actorInfo = TacticsActor();
            if (actorInfo != null){
                if (actorInfo.TacticsCommandType == TacticsCommandType.Paradigm)
                {   
                    actorInfo.ClearTacticsCommand();
                } else
                {
                    if (CanTacticsCommand(TacticsCommandType.Paradigm,actorInfo))
                    {   
                        actorInfo.SetTacticsCommand(TacticsCommandType.Paradigm,TacticsUtility.TrainCost(actorInfo));
                    }
                }
            }
        }

        public void SetStageSeekIndex(int symbolIndex)
        {
            _stageSeekIndex = symbolIndex;
            CurrentStage.SetSeekIndex(_stageSeekIndex);
        }

        public void SaveTempBattleMembers()
        {
            TempInfo.CashBattleActors(BattleMembers());
        }

        public void SetInBattle()
        {
            var actorInfo = TacticsActor();
            var battleIndex = StageMembers().FindAll(a => a.BattleIndex >= 0).Count + 1;
            if (battleIndex >= 5) return;
            if (actorInfo.BattleIndex >= 0)
            {
                actorInfo.SetBattleIndex(-1);
            } else
            {
                actorInfo.SetBattleIndex(battleIndex);
            }
        }

        public List<ListData> SelectActorLearningMagicList()
        {
            var skillInfos = new List<SkillInfo>();
            var actorInfo = TacticsActor();
            
            foreach (var alchemyId in PartyInfo.CurrentAlchemyIdList(CurrentStage.Id,CurrentStage.CurrentTurn))
            {
                //if (actorInfo.IsLearnedSkill(alchemyId)) continue;
                var skillInfo = new SkillInfo(alchemyId);
                var cost = TacticsUtility.LearningMagicCost(actorInfo,skillInfo.Attribute,StageMembers());
                skillInfo.SetEnable(Currency >= cost && !actorInfo.IsLearnedSkill(alchemyId));
                skillInfo.SetLearningCost(cost);
                skillInfos.Add(skillInfo);
            }
            return MakeListData(skillInfos);
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

        public void SelectRecoveryPlus()
        {
            var actorInfo = TacticsActor();
            var remain = TacticsUtility.RemainRecoveryCost(actorInfo);
            if (actorInfo != null && remain > 0){
                var cost = TacticsUtility.RecoveryCost(actorInfo);
                if (Currency >= cost){
                    actorInfo.ChangeHp(actorInfo.CurrentHp + 10);
                    actorInfo.ChangeMp(actorInfo.CurrentMp + 10);
                    PartyInfo.ChangeCurrency(Currency - cost);
                }
            }
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
            var retire = new SystemData.CommandData();
            retire.Id = 1;
            retire.Name = DataSystem.GetTextData(704).Text;
            retire.Key = "Retire";
            list.Add(retire);
            var menuCommand = new SystemData.CommandData();
            menuCommand.Id = 2;
            menuCommand.Name = DataSystem.GetTextData(703).Text;
            menuCommand.Key = "Help";
            list.Add(menuCommand);
            if (CurrentStage.SurvivalMode == false)
            {
                var saveCommand = new SystemData.CommandData();
                saveCommand.Id = 3;
                saveCommand.Name = DataSystem.GetTextData(707).Text;
                saveCommand.Key = "Save";
                list.Add(saveCommand);
            }
            return MakeListData(list);
        }

        public List<ListData> TacticsCharacterData()
        {
            var list = new List<TacticsActorInfo>();
            foreach (var member in StageMembers())
            {
                var tacticsActorInfo = new TacticsActorInfo();
                tacticsActorInfo.TacticsCommandType = _TacticsCommandType;
                tacticsActorInfo.ActorInfo = member;
                tacticsActorInfo.ActorInfos = StageMembers();
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
            var tacticsCommandData = new TacticsCommandData();
            tacticsCommandData.Title = CommandTitle();
            tacticsCommandData.Description = CommandDescription();
            tacticsCommandData.Rank = CommandRank();
            return tacticsCommandData;
        }

        private string CommandTitle()
        {
            return DataSystem.GetTextData((int)_TacticsCommandType).Text;
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

        public void ChangeBattleLineIndex(int actorId,bool isFront)
        {
            var actorInfo = TacticsActor();
            if (actorInfo.LineIndex == LineType.Front && isFront == false)
            {
                actorInfo.SetLineIndex(LineType.Back);
            } else
            if (actorInfo.LineIndex == LineType.Back && isFront == true)
            {
                actorInfo.SetLineIndex(LineType.Front);
            }
        }

        public void AssignBattlerIndex()
        {
            var idList = PartyInfo.LastBattlerIdList;
            foreach (var id in idList)
            {
                var actor = StageMembers().Find(a => a.ActorId == id);
                if (actor != null)
                {
                    actor.SetBattleIndex(id);
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
            var currentSymbol = new StageSymbolData();
            currentSymbol.Seek = seekIndex;
            currentSymbol.SeekIndex = 0;
            currentSymbol.SymbolType = SymbolType.None;
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

        public int PartyEvaluate()
        {
            var evaluate = 0;
            foreach (var actorInfo in BattleMembers())
            {
                evaluate += actorInfo.Evaluate();
            }
            return evaluate;
        }

        public int TroopEvaluate()
        {
            if (_stageSeekIndex >= 0)
            {
                var symbol = CurrentSelectSymbol();
                if (symbol != null && symbol.SymbolType == SymbolType.Battle || symbol.SymbolType == SymbolType.Boss)
                {
                    return symbol.BattleEvaluate();
                }
            }
            return 0;
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