using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NCMB;

public class RebornModel : BaseModel
{
    private int _rebornActorIndex = 0;

    public void ResetStage()
    {
        GameSystem.CurrentData.ClearStageInfo();
        // Party初期化
        PartyInfo.InitActors();
        List<int> stageMembers = DataSystem.Stages.Find(a => a.Id == PartyInfo.StageId).InitMembers;
        for (int i = 0;i < stageMembers.Count;i++)
        {
            PartyInfo.AddActor(stageMembers[i]);
        }
    }
    public List<ActorInfo> ActorInfos(){
        var actorInfos = CurrentData.PlayerInfo.SaveActorList;
        if (actorInfos == null || actorInfos.Count == 0)
        {
            for (int i = 0;i < 5;i++)
            {
                var tempActor = new ActorInfo(DataSystem.Actors[i]);
                tempActor.InitSkillInfo(DataSystem.Actors[i].LearningSkills);
                    
                var rebornSkill = new SkillInfo(4001+i); 
                rebornSkill.SetParam(i+1,1,0);
                tempActor.AddRebornSkill(rebornSkill);
                CurrentData.PlayerInfo.AddActorInfo(tempActor);

            }
        }
        return CurrentData.PlayerInfo.SaveActorList;
    }

    public List<int> DisableActorIndexs()
    {
        var list = new List<int>();
        var idx = 0;
        foreach (var actorInfo in ActorInfos())
        {
            if (actorInfo.ActorId == CurrentStage.SelectActorIds[0])
            {
                list.Add(idx);
            }
            idx++;
        }
        return list;
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
        CurrentStage.SetRebornActorIndex(_rebornActorIndex);
        var rebornActorInfo = RebornActorInfo();
        if (rebornActorInfo == null) return;
        var actorInfo = StageMembers()[0];
        var commandRebornSkill = rebornActorInfo.RebornSkillInfos.Find(a => a.Master.FeatureDatas.Find(b => b.FeatureType == FeatureType.RebornCommandLvUp) != null);
        if (commandRebornSkill != null)
        {
            var upLvCount = commandRebornSkill.Param2;
            for (int i = 0;i < upLvCount;i++)
            {
                PartyInfo.AddCommandRank((TacticsComandType)commandRebornSkill.Param1);
            }
        }

        var statusRebornSkill = rebornActorInfo.RebornSkillInfos.Find(a => a.Master.FeatureDatas.Find(b => b.FeatureType == FeatureType.RebornStatusUp) != null);
        if (statusRebornSkill != null)
        {
            var upStatusCount = statusRebornSkill.Param2;
            actorInfo.TempStatus.AddParameter((StatusParamType)statusRebornSkill.Param1,upStatusCount);
            actorInfo.DecideStrength(0);
        }

        var addSkillRebornSkills = rebornActorInfo.RebornSkillInfos.FindAll(a => a.Master.FeatureDatas.Find(b => b.FeatureType == FeatureType.RebornAddSkill) != null);
        foreach (var addSkill in addSkillRebornSkills)
        {
            PartyInfo.AddAlchemy(addSkill.Param1);
        }
        
        var questRebornSkill = rebornActorInfo.RebornSkillInfos.Find(a => a.Master.FeatureDatas.Find(b => b.FeatureType == FeatureType.RebornQuest) != null);
        if (questRebornSkill != null)
        {
            var upStatusCount = questRebornSkill.Param2;
            actorInfo.TempStatus.AddParameter(StatusParamType.Hp,upStatusCount);
            actorInfo.TempStatus.AddParameter(StatusParamType.Mp,upStatusCount);
            actorInfo.DecideStrength(0);
        }
    }
}
