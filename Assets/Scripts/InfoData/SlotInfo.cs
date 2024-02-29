using System;
using System.Collections.Generic;

namespace Ryneus
{
    [Serializable]
    public class SlotInfo
    {
        public SlotInfo(List<ActorInfo> actorInfos)
        {
            _actorInfos = actorInfos;
        }
        private List<ActorInfo> _actorInfos = new ();
        public List<ActorInfo> ActorInfos => _actorInfos; 
        private string _timeRecord;
        public string TimeRecord => _timeRecord; 

        public void SetTimeRecord()
        {
            DateTime dt1 = DateTime.Now;
            _timeRecord = dt1.ToString("yyyy/MM/dd");
        }
    }
}