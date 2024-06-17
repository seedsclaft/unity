using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Ryneus
{
    public class CheckTriggerState : ICheckTrigger
    {
        public bool CheckTrigger(SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo)
        {
            var isTrigger = false;
            switch (triggerData.TriggerType)
            {
                case TriggerType.IsState:
                    return battlerInfo.IsState((StateType)triggerData.Param1);
                case TriggerType.IsNotState:
                    return !battlerInfo.IsState((StateType)triggerData.Param1);
                case TriggerType.FriendIsState:
                if (checkTriggerInfo.Friends.Find(a => a.IsState((StateType)triggerData.Param1)) != null)
                {
                    isTrigger = true;
                }
                if (triggerData.Param2 == 1)
                {
                    isTrigger = checkTriggerInfo.Friends.Count > 0;
                }
                break;
                case TriggerType.OpponentIsState:
                if (checkTriggerInfo.Opponents.Find(a => a.IsState((StateType)triggerData.Param1)) != null)
                {
                    isTrigger = true;
                }
                if (triggerData.Param2 == 1)
                {
                    isTrigger = checkTriggerInfo.Opponents.Count > 0;
                }
                break;
                case TriggerType.FriendIsNotState:
                if (checkTriggerInfo.Friends.Find(a => !a.IsState((StateType)triggerData.Param1)) != null)
                {
                    isTrigger = true;
                }
                if (triggerData.Param2 == 1)
                {
                    isTrigger = checkTriggerInfo.Friends.Count > 0;
                }
                break;
                case TriggerType.OpponentIsNotState:
                if (checkTriggerInfo.Opponents.Find(a => !a.IsState((StateType)triggerData.Param1)) != null)
                {
                    isTrigger = true;
                }
                if (triggerData.Param2 == 1)
                {
                    isTrigger = checkTriggerInfo.Opponents.Count > 0;
                }
                break;
                case TriggerType.IsAbnormalState:
                    return battlerInfo.StateInfos.Find(a => a.Master.Abnormal == true) != null;
                case TriggerType.FriendIsAbnormalState:
                if (checkTriggerInfo.Friends.Find(a => a.StateInfos.Find(a => a.Master.Abnormal == true) != null) != null)
                {
                    isTrigger = true;
                }
                if (triggerData.Param2 == 1)
                {
                    isTrigger = checkTriggerInfo.Friends.Count > 0;
                }
                break;
                case TriggerType.OpponentIsAbnormalState:
                if (checkTriggerInfo.Opponents.Find(a => a.StateInfos.Find(a => a.Master.Abnormal == true) != null) != null)
                {
                    isTrigger = true;
                }
                if (triggerData.Param2 == 1)
                {
                    isTrigger = checkTriggerInfo.Opponents.Count > 0;
                }
                break;
                case TriggerType.FriendIsNotAbnormalState:
                if (checkTriggerInfo.Friends.Find(a => a.StateInfos.Find(a => a.Master.Abnormal == true) == null) != null)
                {
                    isTrigger = true;
                }
                if (triggerData.Param2 == 1)
                {
                    isTrigger = checkTriggerInfo.Friends.Count > 0;
                }
                break;
                case TriggerType.OpponentIsNotAbnormalState:
                if (checkTriggerInfo.Opponents.Find(a => a.StateInfos.Find(a => a.Master.Abnormal == true) == null) != null)
                {
                    isTrigger = true;
                }
                if (triggerData.Param2 == 1)
                {
                    isTrigger = checkTriggerInfo.Opponents.Count > 0;
                }
                break;
                case TriggerType.IsBuffState:
                    return battlerInfo.StateInfos.Find(a => a.Master.Buff == true) != null;
                case TriggerType.FriendIsBuffState:
                if (checkTriggerInfo.Friends.Find(a => a.StateInfos.Find(a => a.Master.Buff == true) != null) != null)
                {
                    isTrigger = true;
                }
                if (triggerData.Param2 == 1)
                {
                    isTrigger = checkTriggerInfo.Friends.Count > 0;
                }
                break;
                case TriggerType.OpponentIsBuffState:
                if (checkTriggerInfo.Opponents.Find(a => a.StateInfos.Find(a => a.Master.Buff == true) != null) != null)
                {
                    isTrigger = true;
                }
                if (triggerData.Param2 == 1)
                {
                    isTrigger = checkTriggerInfo.Opponents.Count > 0;
                }
                break;
                case TriggerType.FriendIsNotBuffState:
                if (checkTriggerInfo.Friends.Find(a => a.StateInfos.Find(a => a.Master.Buff == true) == null) != null)
                {
                    isTrigger = true;
                }
                if (triggerData.Param2 == 1)
                {
                    isTrigger = checkTriggerInfo.Friends.Count > 0;
                }
                break;
                case TriggerType.OpponentIsNotBuffState:
                if (checkTriggerInfo.Opponents.Find(a => a.StateInfos.Find(a => a.Master.Buff == true) == null) != null)
                {
                    isTrigger = true;
                }
                if (triggerData.Param2 == 1)
                {
                    isTrigger = checkTriggerInfo.Opponents.Count > 0;
                }
                break;
                case TriggerType.IsDeBuffState:
                    return battlerInfo.StateInfos.Find(a => a.Master.DeBuff == true) != null;
                case TriggerType.FriendIsDeBuffState:
                if (checkTriggerInfo.Friends.Find(a => a.StateInfos.Find(a => a.Master.DeBuff == true) != null) != null)
                {
                    isTrigger = true;
                }
                if (triggerData.Param2 == 1)
                {
                    isTrigger = checkTriggerInfo.Friends.Count > 0;
                }
                break;
                case TriggerType.OpponentIsDeBuffState:
                if (checkTriggerInfo.Opponents.Find(a => a.StateInfos.Find(a => a.Master.DeBuff == true) != null) != null)
                {
                    isTrigger = true;
                }
                if (triggerData.Param2 == 1)
                {
                    isTrigger = checkTriggerInfo.Opponents.Count > 0;
                }
                break;
                case TriggerType.FriendIsNotDeBuffState:
                if (checkTriggerInfo.Friends.Find(a => a.StateInfos.Find(a => a.Master.DeBuff == true) == null) != null)
                {
                    isTrigger = true;
                }
                if (triggerData.Param2 == 1)
                {
                    isTrigger = checkTriggerInfo.Friends.Count > 0;
                }
                break;
                case TriggerType.OpponentIsNotDeBuffState:
                if (checkTriggerInfo.Opponents.Find(a => a.StateInfos.Find(a => a.Master.DeBuff == true) == null) != null)
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
                case TriggerType.FriendIsState:
                if (IsFriend && targetBattler.IsState((StateType)triggerData.Param1))
                {
                    targetIndexList.Add(targetIndex);
                }
                break;
                case TriggerType.OpponentIsState:
                if (!IsFriend && targetBattler.IsState((StateType)triggerData.Param1))
                {
                    targetIndexList.Add(targetIndex);
                }
                break;
                case TriggerType.FriendIsNotState:
                if (IsFriend && !targetBattler.IsState((StateType)triggerData.Param1))
                {
                    targetIndexList.Add(targetIndex);
                }
                break;
                case TriggerType.OpponentIsNotState:
                if (!IsFriend && !targetBattler.IsState((StateType)triggerData.Param1))
                {
                    targetIndexList.Add(targetIndex);
                }
                break;
                case TriggerType.FriendIsAbnormalState:
                if (IsFriend && targetBattler.StateInfos.Find(a => a.Master.Abnormal == true) != null)
                {
                    targetIndexList.Add(targetIndex);
                }
                break;
                case TriggerType.OpponentIsAbnormalState:
                if (!IsFriend && targetBattler.StateInfos.Find(a => a.Master.Abnormal == true) != null)
                {
                    targetIndexList.Add(targetIndex);
                }
                break;
                case TriggerType.FriendIsNotAbnormalState:
                if (IsFriend && targetBattler.StateInfos.Find(a => a.Master.Abnormal == true) == null)
                {
                    targetIndexList.Add(targetIndex);
                }
                break;
                case TriggerType.OpponentIsNotAbnormalState:
                if (!IsFriend && targetBattler.StateInfos.Find(a => a.Master.Abnormal == true) == null)
                {
                    targetIndexList.Add(targetIndex);
                }
                break;
                case TriggerType.IsBuffState:
                if (checkTriggerInfo.BattlerInfo.Index == targetIndex && targetBattler.StateInfos.Find(a => a.Master.Buff == true) != null)
                {
                    targetIndexList.Add(targetIndex);
                }
                break;
                case TriggerType.FriendIsBuffState:
                if (IsFriend && targetBattler.StateInfos.Find(a => a.Master.Buff == true) != null)
                {
                    targetIndexList.Add(targetIndex);
                }
                break;
                case TriggerType.OpponentIsBuffState:
                if (!IsFriend && targetBattler.StateInfos.Find(a => a.Master.Buff == true) != null)
                {
                    targetIndexList.Add(targetIndex);
                }
                break;
                case TriggerType.FriendIsNotBuffState:
                if (IsFriend && targetBattler.StateInfos.Find(a => a.Master.Buff == true) == null)
                {
                    targetIndexList.Add(targetIndex);
                }
                break;
                case TriggerType.OpponentIsNotBuffState:
                if (!IsFriend && targetBattler.StateInfos.Find(a => a.Master.Buff == true) == null)
                {
                    targetIndexList.Add(targetIndex);
                }
                break;
                case TriggerType.IsDeBuffState:
                if (checkTriggerInfo.BattlerInfo.Index == targetIndex && targetBattler.StateInfos.Find(a => a.Master.DeBuff == true) != null)
                {
                    targetIndexList.Add(targetIndex);
                }
                break;
                case TriggerType.FriendIsDeBuffState:
                if (IsFriend && targetBattler.StateInfos.Find(a => a.Master.DeBuff == true) != null)
                {
                    targetIndexList.Add(targetIndex);
                }
                break;
                case TriggerType.OpponentIsDeBuffState:
                if (!IsFriend && targetBattler.StateInfos.Find(a => a.Master.DeBuff == true) != null)
                {
                    targetIndexList.Add(targetIndex);
                }
                break;
                case TriggerType.FriendIsNotDeBuffState:
                if (IsFriend && targetBattler.StateInfos.Find(a => a.Master.DeBuff == true) == null)
                {
                    targetIndexList.Add(targetIndex);
                }
                break;
                case TriggerType.OpponentIsNotDeBuffState:
                if (!IsFriend && targetBattler.StateInfos.Find(a => a.Master.DeBuff == true) == null)
                {
                    targetIndexList.Add(targetIndex);
                }
                break;
            }
        }
    }
}

