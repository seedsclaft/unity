using System.Collections;
using System.Collections.Generic;

public class MainMenuModel : BaseModel
{
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

    public bool ClearedStage(int stageId)
    {
        return CurrentData.PlayerInfo.ClearCount(stageId) > 0;
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
        /*
        var slot = new SystemData.CommandData();
        slot.Id = 0;
        slot.Name = DataSystem.GetTextData(705).Text;
        slot.Key = "Slot";
        list.Add(slot);
        */
        return MakeListData(list);
    }
}
