using System.Collections;
using System.Collections.Generic;

public class RebornModel : BaseModel
{
    private List<ListData> _actorInfos = new ();
    private int _currentIndex = 0; 

    public ActorInfo CurrentActor
    {
        get {return (ActorInfo)ActorInfos()[_currentIndex].Data;}
    }

    public void ChangeActorIndex(int value){
        _currentIndex += value;
        if (_currentIndex > ActorInfos().Count-1){
            _currentIndex = 0;
        } else
        if (_currentIndex < 0){
            _currentIndex = ActorInfos().Count-1;
        }
    }

    public List<ListData> ActorInfos(){
        if (_actorInfos.Count > 0)
        {
            return _actorInfos;
        }
        var list = new List<ListData>();
        var actorInfos = CurrentData.PlayerInfo.SaveActorList;
        if (actorInfos.Count == 0)
        {
            CurrentData.PlayerInfo.InitSaveActorList();
            SavePlayerData();
        }
        var idx = 0;
        foreach (var actorInfo in actorInfos)
        {
            var listData = new ListData(actorInfo,idx);
            if (actorInfo.Master.ClassId == DataSystem.FindActor(PartyInfo.ActorIdList[0]).ClassId)
            {
                listData.SetEnable(false);
            }
            list.Add(listData);
            idx++;
        }
        _actorInfos = list;
        return _actorInfos;
    }


    public ActorInfo RebornActorInfo()
    {
        var actorInfos = ActorInfos();
        if (actorInfos.Count > _currentIndex)
        {
            return (ActorInfo)actorInfos[_currentIndex].Data;
        }
        return null;
    }

    public void SetRebornActorIndex(int index)
    {
        _currentIndex = index;
    }

    public void OnRebornSkill()
    {
        CurrentStage.SetRebornActorIndex(_currentIndex);
        var rebornActorInfo = RebornActorInfo();
        if (rebornActorInfo == null) return;
        var actorInfo = StageMembers()[0];
        var commandRebornSkill = rebornActorInfo.RebornSkillInfos.Find(a => a.Master.FeatureDates.Find(b => b.FeatureType == FeatureType.RebornCommandLvUp) != null);
        if (commandRebornSkill != null)
        {
            var upLvCount = commandRebornSkill.Param2;
            for (int i = 0;i < upLvCount;i++)
            {
            }
        }

        var statusRebornSkill = rebornActorInfo.RebornSkillInfos.Find(a => a.Master.FeatureDates.Find(b => b.FeatureType == FeatureType.RebornStatusUp) != null);
        if (statusRebornSkill != null)
        {
            var upStatusCount = statusRebornSkill.Param2;
            actorInfo.TempStatus.AddParameter((StatusParamType)statusRebornSkill.Param3,upStatusCount);
            actorInfo.DecideStrength(0);
        }

        var addSkillRebornSkills = rebornActorInfo.RebornSkillInfos.FindAll(a => a.Master.FeatureDates.Find(b => b.FeatureType == FeatureType.RebornAddSkill) != null);
        foreach (var addSkill in addSkillRebornSkills)
        {
            PartyInfo.AddAlchemy(addSkill.Param3);
        }
        
        var questRebornSkill = rebornActorInfo.RebornSkillInfos.Find(a => a.Master.FeatureDates.Find(b => b.FeatureType == FeatureType.RebornQuest) != null);
        if (questRebornSkill != null)
        {
            var upStatusCount = questRebornSkill.Param2;
            actorInfo.TempStatus.AddParameter(StatusParamType.Hp,upStatusCount);
            actorInfo.TempStatus.AddParameter(StatusParamType.Mp,upStatusCount);
            actorInfo.DecideStrength(0);
        }
    }
}
