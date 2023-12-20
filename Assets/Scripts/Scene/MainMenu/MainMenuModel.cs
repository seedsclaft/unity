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

    public void InitPartyInfo()
    {
        CurrentData.InitActors();
        CurrentData.InitPlayer();
        PartyInfo.InitActors();
        CurrentData.ClearStageInfo();
    }
    
    public void InitializeStageData(int stageId)
    {
        PartyInfo.SetStageId(stageId);
        
        // Party初期化
        PartyInfo.InitActors();
        var stageMembers = DataSystem.Stages.Find(a => a.Id == stageId).InitMembers;
        foreach (var stageMember in stageMembers)
        {
            PartyInfo.AddActor(stageMember);
        }
    }

    public List<SystemData.CommandData> SideMenu()
    {
        var list = new List<SystemData.CommandData>();
        var menuCommand = new SystemData.CommandData();
        menuCommand.Id = 2;
        menuCommand.Name = DataSystem.System.GetTextData(703).Text;
        menuCommand.Key = "Help";
        list.Add(menuCommand);
        var ranking = new SystemData.CommandData();
        ranking.Id = 1;
        ranking.Name = DataSystem.System.GetTextData(702).Text;
        ranking.Key = "Ranking";
        list.Add(ranking);
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
