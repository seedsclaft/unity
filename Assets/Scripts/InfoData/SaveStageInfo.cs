using System;
using System.Collections.Generic;

[Serializable]
public class SaveStageInfo
{
    private StageInfo _currentStage = null;
	public StageInfo CurrentStage => _currentStage;
    private List<ActorInfo> _actors = new ();
    public List<ActorInfo> Actors => _actors;
    private PartyInfo _party = null;
	public PartyInfo Party => _party;
    private AlcanaInfo _currentAlcana = null;
	public AlcanaInfo CurrentAlcana => _currentAlcana;

	private bool _resumeStage = true;
	public bool ResumeStage => _resumeStage;
	public void SetResumeStage(bool resumeStage)
	{
		_resumeStage = resumeStage;
	}

    public void Initialize()
    {
        ClearActors();
        InitParty();
        _currentAlcana = new AlcanaInfo();
        _resumeStage = false;
		_currentStage = null;
    }


	public void MakeStageData(int stageId)
	{
        InitParty();
		_party.InitActors();
        _party.ClearData();
		var stageData = DataSystem.Stages.Find(a => a.Id == stageId);
		_currentStage = new StageInfo(stageData);
		SetStageMembers();
		_currentAlcana = new AlcanaInfo();
		_currentAlcana.InitData();
	}

	private void SetStageMembers()
	{
        // Party初期化
        _party.InitActors();
		ClearActors();
        var stageMembers = _currentStage.Master.InitMembers;
        foreach (var stageMember in stageMembers)
        {
			var actorData = DataSystem.Actors.Find(a => a.Id == stageMember);
			if (actorData != null)
			{
				var actorInfo = new ActorInfo(actorData);
				AddActor(actorInfo);
			}
        }

	}

    public void ClearActors()
    {
        _actors.Clear();
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
			_actors.Add(actorInfo);
			_party.AddActor(actorInfo.ActorId);
		}
	}

    public void InitParty()
    {
        _party = new PartyInfo();
		_party.ChangeCurrency(DataSystem.System.InitCurrency);
    }

	public void InitAllActorMembers()
	{
        foreach (var actorData in DataSystem.Actors)
        {
            if (actorData != null)
			{
				var actorInfo = new ActorInfo(actorData);
				_actors.Add(actorInfo);
				_party.AddActor(actorInfo.ActorId);
			}
        }
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
}
