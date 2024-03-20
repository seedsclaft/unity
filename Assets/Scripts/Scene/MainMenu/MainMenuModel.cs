using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class MainMenuModel : BaseModel
    {
        private TacticsCommandType _TacticsCommandType = TacticsCommandType.Train;
        public TacticsCommandType TacticsCommandType => _TacticsCommandType;
        public void SetTacticsCommandType(TacticsCommandType tacticsCommandType)
        {
            _TacticsCommandType = tacticsCommandType;
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
        
        public List<ListData> TacticsCommand()
        {
            var commandListDates = new List<SystemData.CommandData>();
            foreach (var commandListData in DataSystem.TacticsCommand)
            {
                if ((TacticsCommandType)commandListData.Id != TacticsCommandType.Paradigm)
                {
                    commandListDates.Add(commandListData);
                }
            }
            return MakeListData(commandListDates);
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
        

        public List<ListData> Stages(){
            var list = new List<StageInfo>();
            var stages = DataSystem.Stages.FindAll(a => a.Selectable);
            foreach (var stage in stages)
            {
                var score = 0;
                var stageInfo = new StageInfo(stage);
                stageInfo.SetClearCount(CurrentData.PlayerInfo.ClearCount(stage.Id));
                var scoreMax = 0;
                var records = PartyInfo.SymbolRecordList.FindAll(a => a.StageId == stageInfo.Id);
                foreach (var record in records)
                {
                    if (record.SymbolInfo != null)
                    {
                        scoreMax += record.SymbolInfo.ScoreMax();
                        score += record.BattleScore;
                    }
                }
                stageInfo.SetScore(score);
                stageInfo.SetScoreMax(scoreMax);
                list.Add(stageInfo);
            }
            return MakeListData(list);
        }

        public void StartSelectStage(int stageId)
        {
            CurrentSaveData.MakeStageData(stageId);
            if (SelectedStage(stageId))
            {
                PartyInfo.SetStageSymbolInfos(SelectedSymbolInfos(stageId));
                CurrentStage.SetCurrentTurn(SelectedStageCurrentTurn(stageId));
            } else
            {
                PartyInfo.SetStageSymbolInfos(StageSymbolInfos());
                MakeSymbolResultInfos();
            }
            SavePlayerStageData(true);
        }

        public List<SymbolInfo> SelectedSymbolInfos(int stageId)
        {
            var list = new List<SymbolInfo>();
            var records = PartyInfo.SymbolRecordList.FindAll(a => a.SymbolInfo.StageSymbolData.StageId == stageId);
            foreach (var record in records)
            {
                list.Add(record.SymbolInfo);
            }
            return list;
        }
        
        private bool SelectedStage(int stageId)
        {
            var records = PartyInfo.SymbolRecordList.FindAll(a => a.SymbolInfo.StageSymbolData.StageId == stageId);
            return records.Count > 0;
        }

        public int SelectedStageCurrentTurn(int stageId)
        {
            var turn = 0;
            var records = PartyInfo.SymbolRecordList.FindAll(a => a.SymbolInfo.StageSymbolData.StageId == stageId && a.Selected);
            foreach (var record in records)
            {
                if (record.SymbolInfo.StageSymbolData.Seek >= turn)
                {
                    turn = record.SymbolInfo.StageSymbolData.Seek;
                }
            }
            return turn + 1;
        }

        public bool NeedSlotData(int stageId)
        {
            return DataSystem.FindStage(stageId).UseSlot;
        }

        public List<ListData> SideMenu()
        {
            var list = new List<SystemData.CommandData>();
            var menuCommand = new SystemData.CommandData();
            menuCommand.Id = 2;
            menuCommand.Name = DataSystem.GetTextData(703).Text;
            menuCommand.Key = "Help";
            list.Add(menuCommand);
            return MakeListData(list);
        }
    }
}