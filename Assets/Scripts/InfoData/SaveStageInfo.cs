using System;
using System.Collections.Generic;

[Serializable]
public class SaveStageInfo
{
    private StageInfo _currentStage = null;
	public StageInfo CurrentStage => _currentStage;
    private PartyInfo _party = null;
	public PartyInfo Party => _party;
    private AlcanaInfo stageAlcana = null;
	public AlcanaInfo StageAlcana => stageAlcana;

	private bool _resumeStage = true;
	public bool ResumeStage => _resumeStage;
	public void SetResumeStage(bool resumeStage)
	{
		_resumeStage = resumeStage;
	}

    public void Initialize()
    {
        InitParty();
        stageAlcana = new AlcanaInfo();
        _resumeStage = false;
		_currentStage = null;
    }

	public void InitializeStageData(int stageId)
	{
        InitParty();
		var stageData = DataSystem.FindStage(stageId);
		_currentStage = new StageInfo(stageData);
		SetInitMembers();
		_party.AddActorId(1);
		stageAlcana = new AlcanaInfo();
		stageAlcana.InitData();
	}

	public void MakeStageData(int stageId)
	{
		var stageData = DataSystem.FindStage(stageId);
		_currentStage = new StageInfo(stageData);
	}

	private void SetInitMembers()
	{
        // Party初期化
		_party.InitActorInfos();
		var actorInfos = new List<ActorInfo>();
        foreach (var initMember in _currentStage.Master.InitMembers)
        {
			var actorData = DataSystem.FindActor(initMember);
			if (actorData != null)
			{
				var actorInfo = new ActorInfo(actorData);
				actorInfos.Add(actorInfo);
			}
        }
		_party.SetActorInfos(actorInfos);
	}

	public void AddTestActor(ActorData actorData,int lvUpNum)
	{
		if (actorData != null)
		{
			var actorInfo = new ActorInfo(DataSystem.FindActor(actorData.Id));
			_party.UpdateActorInfo(actorInfo);
			_party.AddActorId(actorInfo.ActorId);
			for (int i = 0;i < actorInfo.Master.LearningSkills.Count;i++)
			{
				var _learningData = actorData.LearningSkills[i];
				if (actorInfo.Skills.Find(a =>a.Id == _learningData.SkillId) != null) continue;
				var skillInfo = new SkillInfo(_learningData.SkillId);
				skillInfo.SetLearningState(LearningState.Learned);
				actorInfo.Skills.Add(skillInfo);
			}
			var statusInfo = actorInfo.LevelUp(lvUpNum-1);
            actorInfo.TempStatus.SetParameter(
                statusInfo.Hp,
                statusInfo.Mp,
                statusInfo.Atk,
                statusInfo.Def,
                statusInfo.Spd
            );
			actorInfo.DecideStrength(0);
			actorInfo.ChangeHp(actorInfo.MaxHp);
		}
	}

    public void InitParty()
    {
        _party = new PartyInfo();
    }

	public void InitAllActorMembers()
	{
		/*
        foreach (var actorData in DataSystem.Actors)
        {
            if (actorData != null)
			{
				var actorInfo = new ActorInfo(actorData);
				_party.AddActor(actorInfo.ActorId);
			}
        }
		*/
    }
    
    public void ChangeRouteSelectStage(int stageId)
	{
		var stageData = DataSystem.FindStage(stageId);
		var current = _currentStage;
		var currentStage = new StageInfo(stageData);
		currentStage.SetMoveStageData(current);
		_currentStage = currentStage;
	}
}
