using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class MainMenuModel : BaseModel
    {
        public bool IsEnding()
        {
            return PartyInfo.HasEndingGetItem(CurrentStage.Id,CurrentStage.Seek,WorldType.Main);
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
            }
            SavePlayerStageData(true);
        }
        
        private bool SelectedStage(int stageId)
        {
            var records = PartyInfo.SymbolRecordList.FindAll(a => a.StageId == stageId);
            return records.Count > 0;
        }

        public int SelectedStageCurrentTurn(int stageId)
        {
            var turn = 0;
            var records = PartyInfo.SymbolRecordList.FindAll(a => a.StageId == stageId && a.Selected);
            foreach (var record in records)
            {
                if (record.Seek >= turn)
                {
                    turn = record.Seek;
                }
            }
            return turn + 1;
        }

        public StageInfo NextStage()
        {
            var list = new List<StageInfo>();
            var find = DataSystem.Stages.Find(a => a.Id > CurrentStage.Id);
            if (find != null)
            {
                return new StageInfo(find);
            }
            return null;
        }
    }
}