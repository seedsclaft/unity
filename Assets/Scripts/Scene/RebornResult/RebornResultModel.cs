using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class RebornResultModel : BaseModel
{
    public List<ListData> RebornResultCommand()
    {
        var commandListDates = MakeListData(BaseConfirmCommand(3040,6));
        foreach (var commandListData in commandListDates)
        {
            var listData = (SystemData.CommandData)commandListData.Data;
            if (listData.Id == 1)
            {
                commandListData.SetEnable(false);
            }
        }
        return commandListDates;
    }

    public List<ActorInfo> RebornMembers()
    {
        return new List<ActorInfo>{CurrentData.PlayerInfo.SaveActorList[CurrentStage.RebornActorIndex]};
    }

    public List<ListData> ResultGetItemInfos()
    {
        var getItemInfos = new List<GetItemInfo>();
        var actorInfo = CurrentData.PlayerInfo.SaveActorList[CurrentStage.RebornActorIndex];
        
        var commandRebornSkill = actorInfo.RebornSkillInfos.Find(a => a.Master.FeatureDates.Find(b => b.FeatureType == FeatureType.RebornCommandLvUp) != null);
        if (commandRebornSkill != null)
        {
            var upLvCount = commandRebornSkill.Param2;
            var getItemInfo = new GetItemInfo(null);
            getItemInfo.MakeCommandCountResult(0,upLvCount,(TacticsCommandType)commandRebornSkill.Param3);
            getItemInfos.Add(getItemInfo);
        }

        var statusRebornSkill = actorInfo.RebornSkillInfos.Find(a => a.Master.FeatureDates.Find(b => b.FeatureType == FeatureType.RebornStatusUp) != null);
        if (statusRebornSkill != null)
        {
            var upStatusCount = statusRebornSkill.Param2;
            var getItemInfo = new GetItemInfo(null);
            var actorName = DataSystem.FindActor(PartyInfo.ActorIdList[0]).Name;
            getItemInfo.MakeStatusRebornResult(actorName,(StatusParamType)statusRebornSkill.Param3,upStatusCount);
            getItemInfos.Add(getItemInfo);
        }

        var addSkillRebornSkills = actorInfo.RebornSkillInfos.FindAll(a => a.Master.FeatureDates.Find(b => b.FeatureType == FeatureType.RebornAddSkill) != null);
        foreach (var addSkill in addSkillRebornSkills)
        {
            var getItemInfo = new GetItemInfo(null);
            getItemInfo.MakeAlchemyBonusResult(DataSystem.FindSkill(addSkill.Param3));
            getItemInfos.Add(getItemInfo);
        }

        
        var questRebornSkill = actorInfo.RebornSkillInfos.Find(a => a.Master.FeatureDates.Find(b => b.FeatureType == FeatureType.RebornQuest) != null);
        if (questRebornSkill != null)
        {
            var upStatusCount = questRebornSkill.Param2;
            var getItemInfo = new GetItemInfo(null);
            var actorName = DataSystem.FindActor(PartyInfo.ActorIdList[0]).Name;
            getItemInfo.MakeQuestRebornResult(actorName,0,upStatusCount);
            getItemInfos.Add(getItemInfo);
        }
        return ListData.MakeListData(getItemInfos);
    }
}
