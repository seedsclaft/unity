using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class CheckTriggerLineIndex : ICheckTrigger
    {
        public bool CheckTrigger(SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo)
        {
            var isTrigger = false;
            switch (triggerData.TriggerType)
            {
                case TriggerType.FriendLineMoreTarget:
                case TriggerType.FriendLineLessTarget:
                case TriggerType.OpponentLineMoreTarget:
                case TriggerType.OpponentLineLessTarget:
                    return true;
                case TriggerType.SelfLineFront:
                    return battlerInfo.LineIndex == LineType.Front;
                case TriggerType.SelfLineBack:
                    return battlerInfo.LineIndex == LineType.Back;
                case TriggerType.FriendLineFront:
                    if (checkTriggerInfo.Friends.Find(a => a.LineIndex == LineType.Front) != null)
                    {
                        isTrigger = true;
                    }
                    if (triggerData.Param2 == 1)
                    {
                        isTrigger = checkTriggerInfo.Friends.Count > 0;
                    }
                    break;
                case TriggerType.FriendLineBack:
                    if (checkTriggerInfo.Friends.Find(a => a.LineIndex == LineType.Back) != null)
                    {
                        isTrigger = true;
                    }
                    if (triggerData.Param2 == 1)
                    {
                        isTrigger = checkTriggerInfo.Friends.Count > 0;
                    }
                break;
                case TriggerType.OpponentLineFront:
                    if (checkTriggerInfo.Opponents.Find(a => a.LineIndex == LineType.Front) != null)
                    {
                        isTrigger = true;
                    }
                    if (triggerData.Param2 == 1)
                    {
                        isTrigger = checkTriggerInfo.Opponents.Count > 0;
                    }
                    break;
                case TriggerType.OpponentLineBack:
                    if (checkTriggerInfo.Opponents.Find(a => a.LineIndex == LineType.Back) != null)
                    {
                        isTrigger = true;
                    }
                    if (triggerData.Param2 == 1)
                    {
                        isTrigger = checkTriggerInfo.Opponents.Count > 0;
                    }
                    break;
                case TriggerType.FriendMoreTargetCount:
                    return checkTriggerInfo.FriendFrontBattlerInfos.Count >= triggerData.Param1 || checkTriggerInfo.FriendBackBattlerInfos.Count >= triggerData.Param1;
                case TriggerType.OpponentMoreTargetCount:
                    return checkTriggerInfo.OpponentsFrontBattlerInfos.Count >= triggerData.Param1 || checkTriggerInfo.OpponentsBackBattlerInfos.Count >= triggerData.Param1;
                case TriggerType.FriendBattleIndex:
                    foreach (var friend in checkTriggerInfo.Friends)
                    {
                        if (friend.IsActor && friend.Index == triggerData.Param1)
                        {
                            isTrigger = true;
                        } else
                        if (!friend.IsActor && friend.Index-100 == triggerData.Param1)
                        {
                            isTrigger = true;
                        }
                    }
                    break;
                case TriggerType.OpponentBattleIndex:
                    foreach (var opponent in checkTriggerInfo.Opponents)
                    {
                        if (opponent.IsActor && opponent.Index == triggerData.Param1)
                        {
                            isTrigger = true;
                        } else
                        if (!opponent.IsActor && opponent.Index-99 == triggerData.Param1)
                        {
                            isTrigger = true;
                        }
                    }
                    break;
            }
            return isTrigger;
        }

        public int CheckTargetIndex(SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo)
        {
            switch (triggerData.TriggerType)
            {
                case TriggerType.FriendLineMoreTarget:
                    return LineMoreTargetIndex(checkTriggerInfo.Friends,battlerInfo,triggerData.Param1);
                case TriggerType.FriendLineLessTarget:
                    return LineLessTargetIndex(checkTriggerInfo.Friends,battlerInfo,triggerData.Param1);
                case TriggerType.OpponentLineMoreTarget:
                    return LineMoreTargetIndex(checkTriggerInfo.Opponents,battlerInfo,triggerData.Param1);
                case TriggerType.OpponentLineLessTarget:
                    return LineLessTargetIndex(checkTriggerInfo.Opponents,battlerInfo,triggerData.Param1);
            }
            return -1;
        }

        private int LineMoreTargetIndex(List<BattlerInfo> targetBattlers,BattlerInfo battlerInfo,int param1)
        {
            if (targetBattlers.Count > 0)
            {
                var front = targetBattlers.FindAll(a => a.LineIndex == LineType.Front);
                var back = targetBattlers.FindAll(a => a.LineIndex == LineType.Back);
                if (back.Count > front.Count)
                {
                    return BattleUtility.NearTargetIndex(battlerInfo,back);
                } else
                {
                    return BattleUtility.NearTargetIndex(battlerInfo,front);
                }
            }
            return -1;
        }

        private int LineLessTargetIndex(List<BattlerInfo> targetBattlers,BattlerInfo battlerInfo,int param1)
        {
            if (targetBattlers.Count > 0)
            {
                var front = targetBattlers.FindAll(a => a.LineIndex == LineType.Front);
                var back = targetBattlers.FindAll(a => a.LineIndex == LineType.Back);
                if (back.Count < front.Count)
                {
                    return BattleUtility.NearTargetIndex(battlerInfo,back);
                } else
                {
                    return BattleUtility.NearTargetIndex(battlerInfo,front);
                }
            }
            return -1;
        }

        public void AddTargetIndexList(List<int> targetIndexList,List<int> targetIndexes,BattlerInfo targetBattler,SkillData.TriggerData triggerData,SkillData skillData,CheckTriggerInfo checkTriggerInfo)
        {
            var IsFriend = checkTriggerInfo.IsFriend(targetBattler);
            var targetIndex = targetBattler.Index;
            
            switch (triggerData.TriggerType)
            {
                case TriggerType.FriendLineFront:
                    if (IsFriend && targetBattler.LineIndex == LineType.Front)
                    {
                        targetIndexList.Add(targetIndex);
                    }
                    break;
                case TriggerType.FriendLineBack:
                    if (IsFriend && targetBattler.LineIndex == LineType.Back)
                    {
                        targetIndexList.Add(targetIndex);
                    }
                    break;
                case TriggerType.OpponentLineFront:
                    if (!IsFriend && targetBattler.LineIndex == LineType.Front)
                    {
                        targetIndexList.Add(targetIndex);
                    }
                    break;
                case TriggerType.OpponentLineBack:
                    if (!IsFriend && targetBattler.LineIndex == LineType.Back)
                    {
                        targetIndexList.Add(targetIndex);
                    }
                    break;
                case TriggerType.FriendMoreTargetCount:
                    if (IsFriend && checkTriggerInfo.AliveBattlerInfos(IsFriend).FindAll(a => a.LineIndex == targetBattler.LineIndex).Count >= triggerData.Param1)
                    {
                        targetIndexList.Add(targetIndex);
                    }
                    break;
                case TriggerType.OpponentMoreTargetCount:
                    if (!IsFriend && checkTriggerInfo.AliveBattlerInfos(!IsFriend).FindAll(a => a.LineIndex == targetBattler.LineIndex).Count >= triggerData.Param1)
                    {
                        targetIndexList.Add(targetIndex);
                    }
                    break;
                case TriggerType.FriendBattleIndex:
                    if (targetBattler.IsActor && targetBattler.Index == triggerData.Param1)
                    {
                        targetIndexList.Add(targetIndex);
                    } else
                    if (!targetBattler.IsActor && targetBattler.Index-100 == triggerData.Param1)
                    {
                        targetIndexList.Add(targetIndex);
                    }
                    break;
                case TriggerType.OpponentBattleIndex:
                    if (targetBattler.IsActor && targetBattler.Index == triggerData.Param1)
                    {
                        targetIndexList.Add(targetIndex);
                    } else
                    if (!targetBattler.IsActor && targetBattler.Index-99 == triggerData.Param1)
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
