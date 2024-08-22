using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class CheckTriggerAwaken : ICheckTrigger
    {
        public bool CheckTrigger(SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo)
        {
            var isTrigger = false;
            if (!battlerInfo.IsAlive())
            {
                return isTrigger;
            }
            switch (triggerData.TriggerType)
            {
                case TriggerType.IsNotAwaken:
                    return !battlerInfo.IsAwaken;
                case TriggerType.IsAwaken:
                    return battlerInfo.IsAwaken;
                case TriggerType.FriendIsNotAwaken:
                if (checkTriggerInfo.Friends.Find(a => !a.IsAwaken) != null)
                {
                    isTrigger = true;
                }
                if (triggerData.Param2 == 1)
                {
                    isTrigger = checkTriggerInfo.Friends.Count > 0;
                }
                break;
                case TriggerType.FriendIsAwaken:
                if (checkTriggerInfo.Friends.Find(a => a.IsAwaken) != null)
                {
                    isTrigger = true;
                }
                if (triggerData.Param2 == 1)
                {
                    isTrigger = checkTriggerInfo.Friends.Count > 0;
                }
                break;
                case TriggerType.OpponentIsNotAwaken:
                if (checkTriggerInfo.Opponents.Find(a => !a.IsAwaken) != null)
                {
                    isTrigger = true;
                }
                if (triggerData.Param2 == 1)
                {
                    isTrigger = checkTriggerInfo.Friends.Count > 0;
                }
                break;
                case TriggerType.OpponentIsAwaken:
                if (checkTriggerInfo.Opponents.Find(a => a.IsAwaken) != null)
                {
                    isTrigger = true;
                }
                if (triggerData.Param2 == 1)
                {
                    isTrigger = checkTriggerInfo.Friends.Count > 0;
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
                case TriggerType.IsNotAwaken:
                if (checkTriggerInfo.BattlerInfo.Index == targetIndex && !targetBattler.IsAwaken)
                {
                    targetIndexList.Add(targetIndex);
                }
                break;
                case TriggerType.IsAwaken:
                if (checkTriggerInfo.BattlerInfo.Index == targetIndex && targetBattler.IsAwaken)
                {
                    targetIndexList.Add(targetIndex);
                }
                break;
                case TriggerType.FriendIsNotAwaken:
                if (IsFriend && !targetBattler.IsAwaken)
                {
                    targetIndexList.Add(targetIndex);
                }
                break;
                case TriggerType.FriendIsAwaken:
                if (IsFriend && targetBattler.IsAwaken)
                {
                    targetIndexList.Add(targetIndex);
                }
                break;
                case TriggerType.OpponentIsNotAwaken:
                if (!IsFriend && !targetBattler.IsAwaken)
                {
                    targetIndexList.Add(targetIndex);
                }
                break;
                case TriggerType.OpponentIsAwaken:
                if (!IsFriend && targetBattler.IsAwaken)
                {
                    targetIndexList.Add(targetIndex);
                }
                break;
            }
        }

        public void AddTriggerTargetList(List<int> targetIndexList,SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo)
        {

        }
    }
}
