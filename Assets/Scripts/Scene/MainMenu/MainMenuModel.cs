using System.Collections;
using System.Collections.Generic;

public class MainMenuModel : BaseModel
{
    public List<ListData> Stages(){
        var list = new List<StageInfo>();
        var stages = DataSystem.Stages.FindAll(a => a.Selectable);
        foreach (var stage in stages)
        {
            var stageInfo = new StageInfo(stage);
            stageInfo.SetClearCount(CurrentData.PlayerInfo.ClearCount(stage.Id));
            list.Add(stageInfo);
        }
        return MakeListData(list);
    }

    public void InitStageData()
    {
        InitSaveStageInfo();
    }
    
    public void InitializeStageData(int stageId)
    {
        CurrentStageData.MakeStageData(stageId);
    }

    public List<SystemData.CommandData> SideMenu()
    {
        var list = new List<SystemData.CommandData>();
        var menuCommand = new SystemData.CommandData();
        menuCommand.Id = 2;
        menuCommand.Name = DataSystem.System.GetTextData(703).Text;
        menuCommand.Key = "Help";
        list.Add(menuCommand);
        /*
        var slot = new SystemData.CommandData();
        slot.Id = 0;
        slot.Name = DataSystem.System.GetTextData(705).Text;
        slot.Key = "Slot";
        list.Add(slot);
        */
        return list;
    }
}
