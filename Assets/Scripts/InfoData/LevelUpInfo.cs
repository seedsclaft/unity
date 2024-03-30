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
        private int _currency = 0;
        public int Currency => _currency;
        private int _stageId = -1;
        public int StageId => _stageId;
        private int _seek = -1;
        public int Seek => _seek;
        private int _seekIndex = -1;
        public int SeekIndex => _seekIndex;

        public LevelUpInfo(int currency,int stageId,int seek,int seekIndex)
        {
            _currency = currency;
            _stageId = stageId;
            _seek = seek;
            _seekIndex = seekIndex;
        }
    }
}
