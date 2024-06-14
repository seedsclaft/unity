using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class CheckTriggerTurnCount : ICheckTrigger
    {
        public bool CheckTrigger(SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo)
        {
            var isTrigger = false;
            switch (triggerData.TriggerType)
            {
                case TriggerType.TurnNumUnder:
                    return battlerInfo.TurnCount < triggerData.Param1;
                case TriggerType.TurnNum:
                    return battlerInfo.TurnCount == triggerData.Param1;
                case TriggerType.TurnNumPer:
                if (triggerData.Param1 == 0)
                {
                    return battlerInfo.TurnCount - triggerData.Param2 == 0;
                } else
                {
                    return (battlerInfo.TurnCount % triggerData.Param1) - triggerData.Param2 == 0;
                }
                case TriggerType.ActionCountPer:
                var actionCount = 0;
                foreach (var member in checkTriggerInfo.Friends)
                {
                    actionCount += member.TurnCount;
                }
                foreach (var member in checkTriggerInfo.Opponents)
                {
                    actionCount += member.TurnCount;
                }
                if (triggerData.Param1 > 0)
                {
                    return (actionCount % triggerData.Param1) - triggerData.Param2 == 0;
                } else
                {
                    return actionCount - triggerData.Param2 == 0;
                }
                case TriggerType.SelfTargetOnly:
                    return battlerInfo.IsAlive();
                case TriggerType.SelfTargetNotOnly:
                    return checkTriggerInfo.Friends.Count > 1;
                case TriggerType.ActionInfoTurnNumPer:
                if (checkTriggerInfo.ActionInfo != null)
                {
                    var actionBattlerInfo = checkTriggerInfo.GetBattlerInfo(checkTriggerInfo.ActionInfo.SubjectIndex);
                    if (triggerData.Param1 == 0)
                    {
                        if (actionBattlerInfo.TurnCount - triggerData.Param2 == 0)
                        {
                            isTrigger = true;
                        }
                    } else
                    {
                        if ((actionBattlerInfo.TurnCount % triggerData.Param1) - triggerData.Param2 == 0)
                        {
                            isTrigger = true;
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
            var IsFriend = checkTriggerInfo.IsFriend(targetBattler);
            var targetIndex = targetBattler.Index;
            
            switch (triggerData.TriggerType)
            {
                case TriggerType.TurnNum:
                if (checkTriggerInfo.BattlerInfo.TurnCount == triggerData.Param1)
                {
                    targetIndexList.Add(targetIndex);
                }
                break;
                case TriggerType.TurnNumPer:
                if ((checkTriggerInfo.BattlerInfo.TurnCount % triggerData.Param1) - triggerData.Param2 == 0)
                {
                    targetIndexList.Add(targetIndex);
                }
                break;
                case TriggerType.SelfTargetOnly:
                if (checkTriggerInfo.BattlerInfo.Index == targetIndex)
                {
                    targetIndexList.Add(targetIndex);
                }
                break;
                case TriggerType.SelfTargetNotOnly:
                if (checkTriggerInfo.BattlerInfo.Index != targetIndex)
                {
                    targetIndexList.Add(targetIndex);
                }
                break;
            }
        }
    }
}