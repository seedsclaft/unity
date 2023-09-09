using System.Collections;
using System.Collections.Generic;

public class MainMenuModel : BaseModel
{
    public List<StageInfo> Stages(){
        return GameSystem.CurrentData.Stages;
    }

    public void InitPartyInfo()
    {
        CurrentData.InitActors();
        CurrentData.InitPlayer();
        PartyInfo.InitActors();
        GameSystem.CurrentData.ClearStageInfo();
    }
    
    public void SetStageId(int stageId)
    {
        PartyInfo.SetStageId(stageId);
        
        // Party初期化
        PartyInfo.InitActors();
        List<int> stageMembers = DataSystem.Stages.Find(a => a.Id == stageId).InitMembers;
        for (int i = 0;i < stageMembers.Count;i++)
        {
            PartyInfo.AddActor(stageMembers[i]);
        }
    }

    public List<SystemData.CommandData> SideMenu()
    {
        var list = new List<SystemData.CommandData>();
        var menucommand = new SystemData.CommandData();
        menucommand.Id = 2;
        menucommand.Name = DataSystem.System.GetTextData(703).Text;
        menucommand.Key = "Help";
        list.Add(menucommand);
        var ranking = new SystemData.CommandData();
        ranking.Id = 1;
        ranking.Name = DataSystem.System.GetTextData(702).Text;
        ranking.Key = "Ranking";
        list.Add(ranking);
        var slot = new SystemData.CommandData();
        slot.Id = 0;
        slot.Name = DataSystem.System.GetTextData(705).Text;
        slot.Key = "Slot";
        list.Add(slot);
        return list;
    }
}
