using System;
using System.Collections.Generic;

namespace Ryneus
{
	[Serializable]
	public class SaveGameInfo
	{
		private PartyInfo _partyInfo = null;
		public PartyInfo PartyInfo => _partyInfo;

		private StageInfo _stageInfo = null;
		public StageInfo StageInfo => _stageInfo;
		public void SetStageInfo(StageInfo stageInfo) => _stageInfo = stageInfo;

		public void Initialize()
		{
			_partyInfo = new PartyInfo();
		}
	}
}