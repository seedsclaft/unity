using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class CheckTriggerAttackAction : ICheckTrigger
    {
        public bool CheckTrigger(SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo)
        {
            var isTrigger = false;
            switch (triggerData.TriggerType)
            {
                case TriggerType.SelfAttackActionInfo:
                    if (battlerInfo.IsAlive() && checkTriggerInfo.ActionInfo != null && checkTriggerInfo.ActionInfo.Master.IsHpDamageFeature())
                    {
                        if (battlerInfo.Index == checkTriggerInfo.ActionInfo.SubjectIndex)
                        {
                            isTrigger = true;
                        }
                    }
                    break;
                case TriggerType.FriendAttackActionInfo:
                    isTrigger = CheckFriendAttackActionInfo(triggerData,battlerInfo,checkTriggerInfo).Count > 0;
                    break;
                case TriggerType.OpponentBuffActionInfo:
                    isTrigger = CheckOpponentBuffActionInfo(triggerData,battlerInfo,checkTriggerInfo).Count > 0;
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
                case TriggerType.FriendAttackActionInfo:
                    targetIndexList.AddRange(CheckFriendAttackActionInfo(triggerData,battlerInfo,checkTriggerInfo));
                    break;
                case TriggerType.OpponentBuffActionInfo:
                    targetIndexList.AddRange(CheckOpponentBuffActionInfo(triggerData,battlerInfo,checkTriggerInfo));
                    break;
            }
        }

        private List<int> CheckFriendAttackActionInfo(SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo)
        {
            var list = new List<int>();
            var actionInfo = checkTriggerInfo.ActionInfo;
            if (!battlerInfo.IsAlive())
            {
                return list;
            }
            if (actionInfo == null)
            {
                return list;
            }
            if (!actionInfo.Master.IsHpDamageFeature())
            {
                return list;
            }
            var subject = checkTriggerInfo.GetBattlerInfo(actionInfo.SubjectIndex);
            if (subject != null && battlerInfo.IsActor == subject.IsActor)
            {
                if (battlerInfo.Index != actionInfo.SubjectIndex)
                {
                    list.Add(actionInfo.SubjectIndex);
                }
            }
            return list;
        }

        private List<int> CheckOpponentBuffActionInfo(SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo)
        {
            var list = new List<int>();
            var actionInfo = checkTriggerInfo.ActionInfo;
            if (!battlerInfo.IsAlive())
            {
                return list;
            }
            if (actionInfo == null)
            {
                return list;
            }
            if (!actionInfo.Master.IsAddBuffFeature())
            {
                return list;
            }
            var subject = checkTriggerInfo.GetBattlerInfo(actionInfo.SubjectIndex);
            if (subject != null && battlerInfo.IsActor != subject.IsActor)
            {
                if (battlerInfo.Index != actionInfo.SubjectIndex)
                {
                    list.Add(actionInfo.SubjectIndex);
                }
            }
            return list;
        }
    }
}
