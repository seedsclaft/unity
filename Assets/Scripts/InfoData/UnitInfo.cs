using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    [System.Serializable]
    public class UnitInfo 
    {
        private List<BattlerInfo> _battlerInfos;
        public List<BattlerInfo> BattlerInfos => _battlerInfos;
        public List<BattlerInfo> AliveBattlerInfos => _battlerInfos.FindAll(a => a.IsAlive());
        
        public void SetBattlers(List<BattlerInfo> battlerInfos)
        {
            _battlerInfos = battlerInfos;
        }
        
        public List<BattlerInfo> FrontBattlers()
        {
            // 最前列は
            if (IsFrontAlive())
            {
                return _battlerInfos.FindAll(a => a.LineIndex == LineType.Front);
            }
            return _battlerInfos;
        }
        
        private bool IsFrontAlive()
        {
            // 最前列は
            return AliveBattlerInfos.Find(a => a.LineIndex == LineType.Front) != null;
        }

        public int TotalEvaluate()
        {
            var evaluate = 0;
            foreach (var battlerInfo in _battlerInfos)
            {
                evaluate += battlerInfo.Evaluate();
            }
            return evaluate;
        }

    }
}