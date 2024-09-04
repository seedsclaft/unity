using System;
using System.Collections.Generic;

namespace Ryneus
{
	[Serializable]
	public class SaveStageInfo
	{
		private StageInfo _currentStage = null;
		public StageInfo CurrentStage => _currentStage;
		private PartyInfo _party = null;
		public PartyInfo Party => _party;

		public void Initialize()
		{
			InitParty();
			_currentStage = null;
		}

		public void InitializeStageData(int stageId)
		{
			InitParty();
			MakeStageData(stageId);
			SetInitMembers();
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
				if (lvUpNum > 0)
				{
					for (int i = 0;i < lvUpNum-1;i++)
					{
						var levelUpInfo = actorInfo.LevelUp(0,0,0,0,0);
            			_party.SetLevelUpInfo(levelUpInfo);
					}
				}
			}
		}

		public void InitParty()
		{
			_party = new PartyInfo();
		}		
	}
}