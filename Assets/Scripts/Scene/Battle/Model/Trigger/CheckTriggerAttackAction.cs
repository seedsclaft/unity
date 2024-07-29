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
                if (battlerInfo.IsAlive() && checkTriggerInfo.ActionInfo != null && checkTriggerInfo.ActionInfo.Master.IsHpDamageFeature())
                {
                    if (battlerInfo.IsActor == checkTriggerInfo.GetBattlerInfo(checkTriggerInfo.ActionInfo.SubjectIndex).IsActor)
                    {
                        if (battlerInfo.Index != checkTriggerInfo.ActionInfo.SubjectIndex)
                        {
                            isTrigger = true;
                        }
                    }
                }
                break;
                case TriggerType.OpponentAttackActionInfo:
                if (battlerInfo.IsAlive() && checkTriggerInfo.ActionInfo != null && checkTriggerInfo.ActionInfo.Master.IsHpDamageFeature())
                {
                    if (battlerInfo.IsActor != checkTriggerInfo.GetBattlerInfo(checkTriggerInfo.ActionInfo.SubjectIndex).IsActor)
                    {
                        if (battlerInfo.Index != checkTriggerInfo.ActionInfo.SubjectIndex)
                        {
                            // Param2が1の時は結果の攻撃対象に自身を指定
                            if (triggerData.Param2 == 1)
                            {
                                if (checkTriggerInfo.ActionResultInfos.Count > 0)
                                {
                                    isTrigger = checkTriggerInfo.ActionResultInfos.Find(a => a.TargetIndex == battlerInfo.Index) != null;
                                }
                            } else
                            {
                                isTrigger = true;
                            }
                        }
                    }
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
