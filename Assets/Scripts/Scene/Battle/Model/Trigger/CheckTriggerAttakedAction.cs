using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class CheckTriggerAttackedAction : ICheckTrigger
    {
        public bool CheckTrigger(SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo)
        {
            var isTrigger = false;
            switch (triggerData.TriggerType)
            {
                case TriggerType.AttackedActionIsScope:
                    isTrigger = CheckAttackedActionIsScope(triggerData,battlerInfo,checkTriggerInfo).Count > 0;
                    break;
                case TriggerType.AttackedActionIsKind:
                    isTrigger = CheckAttackedActionIsKind(triggerData,battlerInfo,checkTriggerInfo).Count > 0;
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

        public void AddTriggerTargetList(List<int> targetIndexList,SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo)
        {
            switch (triggerData.TriggerType)
            {
                case TriggerType.AttackedActionIsScope:
                    targetIndexList.AddRange(CheckAttackedActionIsScope(triggerData,battlerInfo,checkTriggerInfo));
                    break;
                case TriggerType.AttackedActionIsKind:
                    targetIndexList.AddRange(CheckAttackedActionIsKind(triggerData,battlerInfo,checkTriggerInfo));
                    break;
            }
        }

        private List<int> CheckAttackedActionIsScope(SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo)
        {
            var list = new List<int>();
            var actionInfo = checkTriggerInfo.ActionInfo;
            var actionResultInfos = checkTriggerInfo.ActionResultInfos;
            if (!battlerInfo.IsAlive())
            {
                return list;
            }
            if (actionInfo == null)
            {
                return list;
            }
            if (actionResultInfos == null)
            {
                return list;
            }
            if (!actionInfo.Master.IsHpDamageFeature())
            {
                return list;
            }
            var subject = checkTriggerInfo.GetBattlerInfo(actionInfo.SubjectIndex);
            if (subject != null && battlerInfo.IsActor != subject.IsActor && battlerInfo.Index != actionInfo.SubjectIndex)
            {
                var targetActionResultInfos = actionResultInfos.FindAll(a => a.TargetIndex == battlerInfo.Index);
                if (targetActionResultInfos.Count > 0 && actionInfo.ScopeType == (ScopeType)triggerData.Param1)
                {
                    list.Add(battlerInfo.Index);
                }
            }
            return list;
        }        
        
        private List<int> CheckAttackedActionIsKind(SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo)
        {
            var list = new List<int>();
            var actionInfo = checkTriggerInfo.ActionInfo;
            var actionResultInfos = checkTriggerInfo.ActionResultInfos;
            if (!battlerInfo.IsAlive())
            {
                return list;
            }
            if (actionInfo == null)
            {
                return list;
            }
            if (actionResultInfos == null)
            {
                return list;
            }
            if (!actionInfo.Master.IsHpDamageFeature())
            {
                return list;
            }
            var subject = checkTriggerInfo.GetBattlerInfo(actionInfo.SubjectIndex);
            if (subject != null && battlerInfo.IsActor != subject.IsActor && battlerInfo.Index != actionInfo.SubjectIndex)
            {
                var targetActionResultInfos = actionResultInfos.FindAll(a => a.TargetIndex == battlerInfo.Index);
                if (targetActionResultInfos.Count > 0 && subject.Kinds.Contains((KindType)triggerData.Param1))
                {
                    list.Add(battlerInfo.Index);
                }
            }
            return list;
        }
    }
}
