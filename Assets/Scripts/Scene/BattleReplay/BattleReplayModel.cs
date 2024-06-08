using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class BattleReplayModel : BattleModel
    {
        public string ReplayFilePath()
        {
            return CurrentStageKey();
        }
        private int _replayIndex = -1;
        public void SeekReplayCounter()
        {
            _replayIndex++;
        }
        private SaveBattleInfo _saveBattleInfo;
        public SaveBattleInfo SaveBattleInfo => _saveBattleInfo;
        public void SetSaveBattleInfo(SaveBattleInfo saveBattleInfo)
        {
            _saveBattleInfo = saveBattleInfo;
        }

        public ActionInfo GetSaveActionData()
        {
            if (_saveBattleInfo.actionInfos.ContainsKey(_replayIndex))
            {
                var data = _saveBattleInfo.actionInfos[_replayIndex];
                return data;
            }
            return null;
        }
        public List<ActionResultInfo> GetSaveResultData()
        {
            if (_saveBattleInfo.actionResultInfos.ContainsKey(_replayIndex))
            {
                var data = _saveBattleInfo.actionResultInfos[_replayIndex];
                return data;
            }
            return null;
        }

        public new ActionInfo CurrentActionInfo()
        {
            return GetSaveActionData();
        }
    }
}
