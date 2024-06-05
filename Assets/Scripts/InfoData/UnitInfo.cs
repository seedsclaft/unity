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

        public UnitInfo CopyData()
        {
            var copyData = new UnitInfo();
            var battlerInfos = new List<BattlerInfo>();
            var idx = 0;
            foreach (var battlerInfo in _battlerInfos)
            {
                if (battlerInfo.IsActor)
                {
                    var skillTriggerInfos = GameSystem.CurrentStageData.Party.SkillTriggerInfos(battlerInfo.ActorInfo.ActorId);
                    var copyBattlerInfo = new BattlerInfo(battlerInfo.ActorInfo,idx+1,skillTriggerInfos);
                    battlerInfos.Add(copyBattlerInfo);
                } else
                {
                    var copyBattlerInfo = new BattlerInfo(battlerInfo.EnemyData,battlerInfo.Level,idx,battlerInfo.LineIndex,battlerInfo.BossFlag);
                    battlerInfos.Add(copyBattlerInfo);
                }
                idx++;
            }
            copyData.SetBattlers(battlerInfos);
            return copyData;
        }
    }
}