using System.Collections;
using System.Collections.Generic;
using System;

namespace Ryneus
{
    [Serializable]
    public class LevelUpInfo
    {
        private bool _enable = true;
        public bool Enable => _enable;
        public void SetEnable(bool enable)
        {
            _enable = enable;
        }
        private int _actorId = -1;
        public int ActorId => _actorId;
        private int _level = -1;
        public int Level => _level;
        public void SetLevel(int level)
        {
            _level = level;
        }
        private int _skillId = -1;
        public int SkillId => _skillId;
        public void SetSkillId(int skillId)
        {
            _skillId = skillId;
        }
        private int _currency = 0;
        public int Currency => _currency;
        private int _stageId = -1;
        public int StageId => _stageId;
        private int _seek = -1;
        public int Seek => _seek;
        private int _seekIndex = -1;
        public int SeekIndex => _seekIndex;

        public LevelUpInfo(int actorId,int currency,int stageId,int seek,int seekIndex)
        {
            _actorId = actorId;
            _currency = currency;
            _stageId = stageId;
            _seek = seek;
            _seekIndex = seekIndex;
        }

        public bool IsSameLevelUpInfo(LevelUpInfo levelUpInfo)
        {
            return levelUpInfo.ActorId == _actorId && levelUpInfo.SkillId == _skillId && levelUpInfo.Level == _level && levelUpInfo.StageId == _stageId && levelUpInfo.Seek == _seek && levelUpInfo.SeekIndex == _seekIndex;
        }

        public bool IsLevelUpData()
        {
            return _enable && _skillId == -1;
        }

        public bool IsLearnSkillData()
        {
            return _enable && _skillId > -1;
        }

        public bool IsBattleResultData()
        {
            return IsLevelUpData() && _stageId > -1 && _currency == 0;
        }

        public bool IsTrainData()
        {
            return IsLevelUpData() && _currency > 0;
        }
    }
}
