using System;
using System.Collections.Generic;

[Serializable]
public class SaveInfo
{
	private PlayerInfo _playerInfo = null;
    public PlayerInfo PlayerInfo => _playerInfo;
    private List<ActorInfo> _actors = new ();
    public List<ActorInfo> Actors => _actors;
    private PartyInfo _party = null;
	public PartyInfo Party => _party;
    private StageInfo _currentStage = null;
	public StageInfo CurrentStage => _currentStage;
    private AlcanaInfo _currentAlcana = null;
	public AlcanaInfo CurrentAlcana => _currentAlcana;

	private bool _resumeStage = true;
	public bool ResumeStage => _resumeStage;
	public void SetResumeStage(bool resumeStage)
	{
		_resumeStage = resumeStage;
	}
    public SaveInfo()
    {
		this.InitActors();
		this.InitParty();
		_playerInfo = new PlayerInfo();
		_playerInfo.InitStages();
	}

    public void InitActors()
    {
        _actors.Clear();
    }

	public void MakeStageData(int actorId,int debugStageId = 0)
	{
		//InitActors();
		_party.InitActors();
		//AddActor(actorId);
		int stageId = 0;
		if (debugStageId != 0)
		{
			stageId = debugStageId;
		} else
		{
			stageId = _party.StageId;
		}
		var stageData = DataSystem.Stages.Find(a => a.Id == stageId);
		_currentStage = new StageInfo(stageData);
		_currentStage.AddSelectActorId(actorId);
		_currentAlcana = new AlcanaInfo();
		_currentAlcana.InitData();
	}

	public void AddActor(ActorInfo actorInfo)
	{
		_actors.Add(actorInfo);
		_party.AddActor(actorInfo.ActorId);
	}
	
	public void UpdateActorInfo(ActorInfo actorInfo)
	{
		var findIndex = _actors.FindIndex(a => a.ActorId == actorInfo.ActorId);
		if (findIndex > -1)
		{
			_actors[findIndex] = actorInfo;
		}
	}

	public void AddTestActor(ActorData actorData)
	{
		if (actorData != null)
		{
			var actorInfo = new ActorInfo(actorData);
			actorInfo.InitSkillInfo(actorData.LearningSkills);
			_actors.Add(actorInfo);
			_party.AddActor(actorInfo.ActorId);
		}
	}

    public void InitParty()
    {
        _party = new PartyInfo();
		_party.ChangeCurrency(DataSystem.System.InitCurrency);
    }

	public void InitSaveData()
	{
		this.InitPlayer();
		_playerInfo.InitStageInfo();
	}

	public void InitPlayer()
	{
		for (int i = 0;i < DataSystem.InitActors.Count;i++)
		{
			var actorData = DataSystem.Actors.Find(actor => actor.Id == DataSystem.InitActors[i]);
			if (actorData != null)
			{
				var actorInfo = new ActorInfo(actorData);
				actorInfo.InitSkillInfo(actorData.LearningSkills);
				_actors.Add(actorInfo);
				_party.AddActor(actorInfo.ActorId);
			}
		}
	}

	public void SetPlayerName(string name)
	{
		_playerInfo.SetPlayerName(name);
		_playerInfo.SetUserId();
	}

	public void ChangeRouteSelectStage(int stageId)
	{
		var stageData = DataSystem.Stages.Find(a => a.Id == stageId);
		var current = _currentStage;
		var currentStage = new StageInfo(stageData);
		currentStage.SetMoveStageData(current);
		_currentStage = currentStage;
	}

	public void MoveStage(int stageId)
	{
		var stageData = DataSystem.Stages.Find(a => a.Id == stageId);
		var current = _currentStage;
		var moveStage = new StageInfo(stageData);
		moveStage.SetMoveStageData(current);
		_currentStage = moveStage;
	}

	public void ClearStageInfo()
	{
		_currentStage = null;
	}
}