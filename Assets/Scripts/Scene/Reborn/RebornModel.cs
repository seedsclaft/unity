using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NCMB;

public class RebornModel : BaseModel
{
    private int _rebornActorIndex = 0;
    public List<ActorInfo> ActorInfos(){
        var actorInfos = CurrentData.PlayerInfo.SaveActorList;
        if (actorInfos == null || actorInfos.Count == 0)
        {
            for (int i = 0;i < 5;i++)
            {
                var tempActor = new ActorInfo(DataSystem.Actors[i]);
                tempActor.InitSkillInfo(DataSystem.Actors[i].LearningSkills);
                    
                var rebornSkill = new RebornSkillInfo(4001+i,i+1,1,0);
                tempActor.AddRebornSkill(rebornSkill);
                CurrentData.PlayerInfo.AddActorInfo(tempActor);

            }
        }
        return CurrentData.PlayerInfo.SaveActorList;
    }

    public ActorInfo RebornActorInfo()
    {
        List<ActorInfo> actorInfos = ActorInfos();
        if (actorInfos.Count > _rebornActorIndex)
        {
            return actorInfos[_rebornActorIndex];
        }
        return null;
    }

    public void SetRebornActorIndex(int index)
    {
        _rebornActorIndex = index;
    }

    public void OnRebornSkill()
    {
        var actorInfo = RebornActorInfo();
        if (actorInfo == null) return;
        var commandRebornSkill = actorInfo.RebornSkillInfos.Find(a => a.Master.FeatureDatas.Find(b => b.FeatureType == FeatureType.RebornCommandLvUp) != null);
        if (commandRebornSkill != null)
        {
            var upLvCount = commandRebornSkill.Param2;
            for (int i = 0;i < upLvCount;i++)
            {
                PartyInfo.AddCommandRank((TacticsComandType)commandRebornSkill.Param1);
            }
        }

        var statusRebornSkill = actorInfo.RebornSkillInfos.Find(a => a.Master.FeatureDatas.Find(b => b.FeatureType == FeatureType.RebornStatusUp) != null);
        if (statusRebornSkill != null)
        {
            var upStatusCount = statusRebornSkill.Param2;
            CurrentData.Actors[0].TempStatus.AddParameter((StatusParamType)statusRebornSkill.Param1,upStatusCount);
            CurrentData.Actors[0].DecideStrength(0);
        }

        var addSkillRebornSkills = actorInfo.RebornSkillInfos.FindAll(a => a.Master.FeatureDatas.Find(b => b.FeatureType == FeatureType.RebornAddSkill) != null);
        foreach (var addSkill in addSkillRebornSkills)
        {
            PartyInfo.AddAlchemy(addSkill.Param1);
        }
        
    }
}
