using System.Collections;
using System.Collections.Generic;

public class MainMenuModel : BaseModel
{
    public List<StageInfo> Stages(){
        return GameSystem.CurrentData.Stages;
    }
    
    public void SetStageId(int stageId)
    {
        PartyInfo.SetStageId(stageId);
    }
}
