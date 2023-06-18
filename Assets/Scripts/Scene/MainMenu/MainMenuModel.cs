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
}
