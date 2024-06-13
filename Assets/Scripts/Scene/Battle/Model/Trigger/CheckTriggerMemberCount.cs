using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class CheckTriggerMemberCount : ICheckTrigger
    {
        public bool CheckTrigger(SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo)
        {
            var isTrigger = false;
            switch (triggerData.TriggerType)
            {
                case TriggerType.LessTroopMembers:
                    return checkTriggerInfo.Opponents.Count >= checkTriggerInfo.Friends.Count;
                case TriggerType.MoreTroopMembers:
                    return checkTriggerInfo.Opponents.Count <= checkTriggerInfo.Friends.Count;
                case TriggerType.FriendMembersMoreCount:
                    return checkTriggerInfo.Friends.Count >= triggerData.Param1;
                case TriggerType.FriendMembersLessCount:
                    return checkTriggerInfo.Friends.Count <= triggerData.Param1;
                case TriggerType.OpponentMembersMoreCount:
                    return checkTriggerInfo.Opponents.Count >= triggerData.Param1;
                case TriggerType.OpponentMembersLessCount:
                    return checkTriggerInfo.Opponents.Count <= triggerData.Param1;
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
                case TriggerType.FriendMembersMoreCount:
                if (checkTriggerInfo.AliveBattlerInfos(IsFriend).Count >= triggerData.Param1)
                {
                    targetIndexList.Add(targetIndex);
                }
                break;
                case TriggerType.FriendMembersLessCount:
                if (checkTriggerInfo.AliveBattlerInfos(IsFriend).Count <= triggerData.Param1)
                {
                    targetIndexList.Add(targetIndex);
                }
                break;
                case TriggerType.OpponentMembersMoreCount:
                if (checkTriggerInfo.AliveBattlerInfos(!IsFriend).Count >= triggerData.Param1)
                {
                    targetIndexList.Add(targetIndex);
                }
                break;
                case TriggerType.OpponentMembersLessCount:
                if (checkTriggerInfo.AliveBattlerInfos(!IsFriend).Count <= triggerData.Param1)
                {
                    targetIndexList.Add(targetIndex);
                }
                break;
            }
        }
    }
}
