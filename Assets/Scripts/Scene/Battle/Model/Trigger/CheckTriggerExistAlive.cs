using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class CheckTriggerExistAlive : ICheckTrigger
    {
        public bool CheckTrigger(SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo)
        {
            var isTrigger = false;
            switch (triggerData.TriggerType)
            {
                case TriggerType.IsExistDeathMember:
                var IsExistDeathMember = battlerInfo.IsActor ? checkTriggerInfo.Friends : checkTriggerInfo.Opponents;
                if (IsExistDeathMember.FindAll(a => !a.IsAlive()).Count >= triggerData.Param1)
                {
                    isTrigger = true;
                }
                break;
                case TriggerType.IsExistAliveMember:
                var IsExistAliveMember = battlerInfo.IsActor ? checkTriggerInfo.Friends : checkTriggerInfo.Opponents;
                if (IsExistAliveMember.FindAll(a => a.IsAlive()).Count >= triggerData.Param1)
                {
                    isTrigger = true;
                }
                break;
            }
            return isTrigger;
        }

        public int CheckTargetIndex(SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo)
        {
            return -1;
        }

        public void AddTargetIndexList(List<int> targetIndexList,List<int> targetIndexes,BattlerInfo targetBattler,SkillData.TriggerData triggerData,SkillData skillData,CheckTriggerInfo checkTriggerInfo)
        {
            
        }
    }
}
