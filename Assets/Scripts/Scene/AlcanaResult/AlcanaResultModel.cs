using System.Collections;
using System.Collections.Generic;

public class AlcanaResultModel : BaseModel
{
    public AlcanaResultModel()
    {
        SetStageActor();
    }

    public List<ListData> AlcanaResultCommand()
    {
        var commandListDates = MakeListData(BaseConfirmCommand(3040,6));
        foreach (var commandListData in commandListDates)
        {
            var listData = (SystemData.CommandData)commandListData.Data;
            if (listData.Id == 1)
            {
                //commandListData.SetEnable(false);
            }
        }
        return commandListDates;
    }

    public List<ActorInfo> AlcanaMembers()
    {
        return StageMembers();
    }

    public List<ListData> ResultGetItemInfos()
    {
        var getItemInfos = new List<GetItemInfo>();
        var skillInfos = TempData.TempAlcanaSkillInfos;
        foreach (var skillInfo in skillInfos)
        {
            getItemInfos.AddRange(GetAlcanaResults(skillInfo));
        }
        return MakeListData(getItemInfos);
    }

    public void ReleaseAlcana()
    {
        var skillInfos = TempData.TempAlcanaSkillInfos;
        foreach (var skillInfo in skillInfos)
        {
            CurrentAlcana.DisableAlcana(skillInfo);
        }
        TempData.ClearAlcana();
    }
}
