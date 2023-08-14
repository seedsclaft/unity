using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NCMB;

public class RebornResultModel : BaseModel
{
    public List<SystemData.MenuCommandData> RebornResultCommand()
    {
        List<SystemData.MenuCommandData> menuCommandDatas = new List<SystemData.MenuCommandData>();
        SystemData.MenuCommandData yesCommand = new SystemData.MenuCommandData();
        yesCommand.Key = "Yes";
        yesCommand.Name = DataSystem.System.GetTextData(6).Text;
        yesCommand.Id = 0;
        menuCommandDatas.Add(yesCommand);
        SystemData.MenuCommandData noCommand = new SystemData.MenuCommandData();
        noCommand.Key = "No";
        noCommand.Name = DataSystem.System.GetTextData(3040).Text;
        noCommand.Id = 1;
        menuCommandDatas.Add(noCommand);
        return menuCommandDatas;
    }

    public List<ActorInfo> RebornMembers()
    {
        return new List<ActorInfo>{CurrentData.PlayerInfo.SaveActorList[CurrentStage.RebornActorIndex]};
    }

    public List<GetItemInfo> ResultGetItemInfos()
    {
        var getItemInfos = new List<GetItemInfo>();
        var actorInfo = CurrentData.PlayerInfo.SaveActorList[CurrentStage.RebornActorIndex];
        
        var commandRebornSkill = actorInfo.RebornSkillInfos.Find(a => a.Master.FeatureDatas.Find(b => b.FeatureType == FeatureType.RebornCommandLvUp) != null);
        if (commandRebornSkill != null)
        {
            var upLvCount = commandRebornSkill.Param2;
            GetItemInfo getItemInfo = new GetItemInfo(null);
            getItemInfo.MakeCommandRebornResult((TacticsComandType)commandRebornSkill.Param1,upLvCount);
            getItemInfos.Add(getItemInfo);
        }

        var statusRebornSkill = actorInfo.RebornSkillInfos.Find(a => a.Master.FeatureDatas.Find(b => b.FeatureType == FeatureType.RebornStatusUp) != null);
        if (statusRebornSkill != null)
        {
            var upStatusCount = statusRebornSkill.Param2;
            GetItemInfo getItemInfo = new GetItemInfo(null);
            var actorName = DataSystem.Actors.Find(a => a.Id == CurrentStage.SelectActorIds[0]).Name;
            getItemInfo.MakeStatusRebornResult(actorName,(StatusParamType)statusRebornSkill.Param1,upStatusCount);
            getItemInfos.Add(getItemInfo);
        }

        var addSkillRebornSkills = actorInfo.RebornSkillInfos.FindAll(a => a.Master.FeatureDatas.Find(b => b.FeatureType == FeatureType.RebornAddSkill) != null);
        foreach (var addSkill in addSkillRebornSkills)
        {
            GetItemInfo getItemInfo = new GetItemInfo(null);
            getItemInfo.MakeAlchemyBonusResult(DataSystem.Skills.Find(a => a.Id == addSkill.Id));
            getItemInfos.Add(getItemInfo);
        }

        
        var questRebornSkill = actorInfo.RebornSkillInfos.Find(a => a.Master.FeatureDatas.Find(b => b.FeatureType == FeatureType.RebornQuest) != null);
        if (questRebornSkill != null)
        {
            var upStatusCount = questRebornSkill.Param2;
            GetItemInfo getItemInfo = new GetItemInfo(null);
            var actorName = DataSystem.Actors.Find(a => a.Id == CurrentStage.SelectActorIds[0]).Name;
            getItemInfo.MakeQuestRebornResult(actorName,0,upStatusCount);
            getItemInfos.Add(getItemInfo);
        }
        return getItemInfos;
    }
}