using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class CheckTriggerBattleCount : ICheckTrigger
    {
        public bool CheckTrigger(SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo)
        {
            var isTrigger = false;
            switch (triggerData.TriggerType)
            {
                case TriggerType.SkillUsedCount:
                if (battlerInfo.IsAlive())
                {
                    var skill = battlerInfo.Skills.Find(a => a.Id == triggerData.Param3);
                    if (skill != null)
                    {
                        if (skill.UseCount >= triggerData.Param1)
                        {
                            isTrigger = true;
                        }
                    }
                }
                break;
                case TriggerType.AttackedCount:
                if (battlerInfo.IsAlive() && battlerInfo.AttackedCount >= triggerData.Param1)
                {
                    isTrigger = true;
                }
                break;
                case TriggerType.BeCriticalCount:
                if (battlerInfo.IsAlive() && battlerInfo.BeCriticalCount >= triggerData.Param1)
                {
                    isTrigger = true;
                }
                break;
                case TriggerType.DodgeCountOver:
                if (battlerInfo.IsAlive() && battlerInfo.DodgeCount >= triggerData.Param1)
                {
                    isTrigger = true;
                }
                break;
                case TriggerType.HpHealCountOver:
                if (battlerInfo.IsAlive() && battlerInfo.HealCount >= triggerData.Param1)
                {
                    isTrigger = true;
                }
                break;
                case TriggerType.ChainCount:
                if (battlerInfo.ChainSuccessCount >= triggerData.Param1)
                {
                    isTrigger = true;
                }
                break;
            }
            return isTrigger;
        }

        public int CheckTargetIndex(SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo,int targetBattlerIndex)
        {
            return -1;
        }

        public void AddTargetIndexList(List<int> targetIndexList,List<int> targetIndexes,BattlerInfo targetBattler,SkillData.TriggerData triggerData,SkillData skillData,CheckTriggerInfo checkTriggerInfo)
        {
            
        }

        public void AddTriggerTargetList(List<int> targetIndexList,SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo)
        {

        }
    }
}
