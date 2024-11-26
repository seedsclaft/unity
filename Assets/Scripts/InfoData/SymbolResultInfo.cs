using System;
using System.Collections.Generic;

namespace Ryneus
{
    [Serializable]
    public class SymbolResultInfo
    {
        public int StageId = -1;
        public int Seek = -1;
        public int SeekIndex = -1;
        public List<int> ResultParams = new ();

        public SymbolResultInfo(int stageId,int seek,int seekIndex)
        {
            StageId = stageId;
            Seek = seek;
            SeekIndex = seekIndex;
        }
    }
}