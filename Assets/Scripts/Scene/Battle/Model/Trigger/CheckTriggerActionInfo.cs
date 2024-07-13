using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class CheckTriggerActionInfo : ICheckTrigger
    {
        public bool CheckTrigger(SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo)
        {
            var actionInfo = checkTriggerInfo.ActionInfo;
            var actionResultInfos = checkTriggerInfo.ActionResultInfos;
            var isTrigger = false;
            switch (triggerData.TriggerType)
            {
                /*
                case TriggerType.ActionMpCost:
                if (battlerInfo.IsAlive() && actionInfo != null && actionInfo.MpCost == triggerData.Param1)
                {
                    isTrigger = true;
                }
                break;
                */
                case TriggerType.TargetHpRateUnder:
                if (actionResultInfos != null && actionResultInfos.Count > 0)
                {
                    if (!battlerInfo.IsAlive() && triggerData.Param2 == 0)
                    {
                        break;
                    }
                    foreach (var actionResultInfo in actionResultInfos)
                    {
                        if (actionResultInfo.HpDamage > 0 && actionResultInfo.TargetIndex != actionResultInfo.SubjectIndex)
                        {
                            var targetBattlerInfo = checkTriggerInfo.GetBattlerInfo(actionResultInfo.TargetIndex);
                            if (battlerInfo.IsActor == targetBattlerInfo.IsActor)
                            {
                                var targetHp = 0f;
                                if (targetBattlerInfo.Hp != 0)
                                {
                                    targetHp = (float)targetBattlerInfo.Hp / (float)targetBattlerInfo.MaxHp;
                                }
                                if (targetHp <= triggerData.Param1 * 0.01f)
                                {
                                    isTrigger = true;
                                }
                            }
                        }
                    }
                }
                break;
                case TriggerType.TargetDeath:
                if (battlerInfo.IsAlive() && actionResultInfos != null && actionResultInfos.Count > 0)
                {
                    foreach (var actionResultInfo in actionResultInfos)
                    {
                        if (actionResultInfo.TargetIndex != actionResultInfo.SubjectIndex)
                        {
                            var targetBattlerInfo = checkTriggerInfo.GetBattlerInfo(actionResultInfo.TargetIndex);
                            if (actionResultInfo.DeadIndexList.Contains(targetBattlerInfo.Index))
                            {
                                isTrigger = true;
                            }               
                        }
                    }
                }
                break;
                case TriggerType.OneAttackOverDamage:
                if (battlerInfo.IsAlive() && battlerInfo.MaxDamage >= triggerData.Param1)
                {
                    isTrigger = true;
                }
                break;
                case TriggerType.FriendAttackedAction:
                if (battlerInfo.IsAlive())
                {
                    if (actionInfo != null && actionInfo.Master.IsHpDamageFeature())
                    {
                        if (battlerInfo.IsActor != checkTriggerInfo.GetBattlerInfo(actionInfo.SubjectIndex).IsActor)
                        {
                            isTrigger = true;
                        }
                    }
                }
                break;
                case TriggerType.SelfAttackedAction:
                if (battlerInfo.IsAlive())
                {
                    if (actionInfo != null && actionInfo.ActionResults != null && actionInfo.Master.IsHpDamageFeature())
                    {
                        var results = actionInfo.ActionResults.FindAll(a => a.HpDamage > 0 && a.TargetIndex == battlerInfo.Index);
                        if (results.Count > 0)
                        {
                            isTrigger = true;
                        }
                    }
                }
                break;
                case TriggerType.FriendAction:
                if (battlerInfo.IsAlive())
                {
                    if (actionInfo != null && actionInfo.ActionResults != null && actionInfo.Master.IsHpDamageFeature())
                    {
                        if (battlerInfo.IsActor == checkTriggerInfo.GetBattlerInfo(actionInfo.SubjectIndex).IsActor && battlerInfo.Index != actionInfo.SubjectIndex)
                        {
                            var success = actionInfo.ActionResults.Count > 0;
                            if (success)
                            {
                                isTrigger = true;
                            }
                        }
                    }
                }
                break;
                case TriggerType.FriendAttackAction:
                if (battlerInfo.IsAlive())
                {
                    if (actionInfo != null && actionInfo.ActionResults != null && actionInfo.Master.IsHpDamageFeature())
                    {
                        if (battlerInfo.IsActor == checkTriggerInfo.GetBattlerInfo(actionInfo.SubjectIndex).IsActor && battlerInfo.Index != actionInfo.SubjectIndex)
                        {
                            var results = actionInfo.ActionResults.FindAll(a => a.HpDamage > 0);
                            if (results.Count > 0)
                            {
                                isTrigger = true;
                            }
                        }
                    }
                }
                break;
                case TriggerType.OpponentHealAction:
                if (battlerInfo.IsAlive())
                {
                    if (actionInfo != null && actionInfo.ActionResults != null && actionInfo.Master.IsHpHealFeature())
                    {
                        if (battlerInfo.IsActor != checkTriggerInfo.GetBattlerInfo(actionInfo.SubjectIndex).IsActor && battlerInfo.Index != actionInfo.SubjectIndex)
                        {
                            var results = actionInfo.ActionResults.FindAll(a => a.HpHeal > 0);
                            if (results.Count > 0)
                            {
                                isTrigger = true;
                            }
                        }
                    }
                }
                break;
                case TriggerType.OpponentDamageShieldAction:
                if (battlerInfo.IsAlive())
                {
                    if (actionInfo != null && actionInfo.ActionResults != null)
                    {
                        if (battlerInfo.IsActor == checkTriggerInfo.GetBattlerInfo(actionInfo.SubjectIndex).IsActor && battlerInfo.Index != actionInfo.SubjectIndex)
                        {
                            foreach (var actionResultInfo in actionInfo.ActionResults)
                            {
                                foreach (var execStateInfo in actionResultInfo.ExecStateInfos)
                                {
                                    if (execStateInfo.Key == actionResultInfo.TargetIndex)
                                    {
                                        if (execStateInfo.Value.Find(a => a.StateType == StateType.NoDamage) != null)
                                        {
                                            isTrigger = true;
                                        }
                                    }
                                }
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
