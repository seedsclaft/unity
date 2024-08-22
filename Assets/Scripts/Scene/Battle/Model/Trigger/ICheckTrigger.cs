using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public interface ICheckTrigger
    {
        bool CheckTrigger(SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo);
    
        int CheckTargetIndex(SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo);

        public void AddTargetIndexList(List<int> targetIndexList,List<int> targetIndexes,BattlerInfo targetBattler,SkillData.TriggerData triggerData,SkillData skillData,CheckTriggerInfo checkTriggerInfo);
    
        public void AddTriggerTargetList(List<int> targetIndexList,SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo);
    }
}
