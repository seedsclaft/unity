using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class CheckTriggerKind : ICheckTrigger
    {
        public bool CheckTrigger(SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo)
        {
            var isTrigger = false;
            switch (triggerData.TriggerType)
            {
                case TriggerType.FriendHasKind:
                if (checkTriggerInfo.Friends.Find(a => a.Kinds.Contains((KindType)triggerData.Param1)) != null)
                {
                    isTrigger = true;
                }
                if (triggerData.Param2 == 1)
                {
                    isTrigger = checkTriggerInfo.Friends.Count > 0;
                }
                break;
                case TriggerType.OpponentHasKind:
                if (checkTriggerInfo.Opponents.Find(a => a.Kinds.Contains((KindType)triggerData.Param1)) != null)
                {
                    isTrigger = true;
                }
                if (triggerData.Param2 == 1)
                {
                    isTrigger = checkTriggerInfo.Opponents.Count > 0;
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
                case TriggerType.FriendHasKind:
                if (IsFriend && targetBattler.Kinds.Contains((KindType)triggerData.Param1))
                {
                    targetIndexList.Add(targetIndex);
                }
                break;
                case TriggerType.OpponentHasKind:
                if (!IsFriend && targetBattler.Kinds.Contains((KindType)triggerData.Param1))
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
