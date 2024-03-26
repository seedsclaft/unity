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
            menuCommand.Name = DataSystem.GetText(703);
            menuCommand.Key = "Help";
            list.Add(menuCommand);
            return MakeListData(list);
        }
    }
}