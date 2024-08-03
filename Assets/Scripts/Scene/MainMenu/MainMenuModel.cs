using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class MainMenuModel : BaseModel
    {
        public List<ListData> Stages()
        {
            var list = new List<StageInfo>();
            var stages = DataSystem.Stages.FindAll(a => a.Selectable);
            foreach (var stage in stages)
            {
                var score = 0;
                var stageInfo = new StageInfo(stage);
                //stageInfo.SetClearCount(CurrentData.PlayerInfo.ClearCount(stage.Id));
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
            SetStageSeek();
            if (SelectedStage(stageId))
            {
                CurrentStage.SetCurrentTurn(SelectedStageCurrentTurn(stageId));
            } else
            {
                // 新規レコード作成
                foreach (var record in StageResultInfos(stageId))
                {
                    PartyInfo.SetSymbolResultInfo(record);
                }
                foreach (var record in StageResultInfos(stageId))
                {
                    record.SetWorldNo(1);
                    PartyInfo.SetSymbolResultInfo(record);
                }
            }
            SavePlayerStageData(true);
        }
        
        private bool SelectedStage(int stageId)
        {
            var records = PartyInfo.SymbolRecordList.FindAll(a => a.StageSymbolData.StageId == stageId);
            return records.Count > 0;
        }

        public int SelectedStageCurrentTurn(int stageId)
        {
            var turn = 0;
            var records = PartyInfo.SymbolRecordList.FindAll(a => a.StageSymbolData.StageId == stageId && a.Selected);
            foreach (var record in records)
            {
                if (record.StageSymbolData.Seek >= turn)
                {
                    turn = record.StageSymbolData.Seek;
                }
            }
            return turn + 1;
        }

        public bool NeedSlotData(int stageId)
        {
            return DataSystem.FindStage(stageId).UseSlot;
        }

        public StageInfo NextStage()
        {
            var list = new List<StageInfo>();
            var find = DataSystem.Stages.Find(a => CurrentSaveData.Party.ClearCount(a.Id) == 0 && a.Id > 0);
            if (find != null)
            {
                return new StageInfo(find);
            }
            return null;
        }

        public List<ListData> SideMenu()
        {
            var list = new List<SystemData.CommandData>();
            var menuCommand = new SystemData.CommandData
            {
                Id = 2,
                Name = DataSystem.GetText(703),
                Key = "Help"
            };
            list.Add(menuCommand);
            var saveCommand = new SystemData.CommandData
            {
                Id = 3,
                Name = DataSystem.GetText(707),
                Key = "Save"
            };
            list.Add(saveCommand);
            return MakeListData(list);
        }
    }
}