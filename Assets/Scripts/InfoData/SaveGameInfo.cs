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

        private List<string> _readEventKeys = new ();
        public List<string> ReadEventKeys => _readEventKeys;
        public void AddEventReadFlag(string key)
        {
            _readEventKeys.Add(key);
        }
		
		public void Initialize()
		{
			_partyInfo = new PartyInfo();
		}
	}
}