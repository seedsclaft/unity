using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class CheckTriggerMp : ICheckTrigger
    {
        public bool CheckTrigger(SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo)
        {
            var isTrigger = false;
            switch (triggerData.TriggerType)
            {
                case TriggerType.SelfMpUnder:
                    return CheckMpRateUnderMore(battlerInfo,triggerData.Param1);
                case TriggerType.SelfMpUpper:
                    return CheckMpRateUpperMore(battlerInfo,triggerData.Param1);
                case TriggerType.FriendMpUnder:
                if (triggerData.Param1 == 100)
                {
                    return checkTriggerInfo.Friends.Find(a => CheckMpRateUnder(a,triggerData.Param1)) != null;
                } else
                {
                    return checkTriggerInfo.Friends.Find(a => CheckMpRateUnderMore(a,triggerData.Param1)) != null;
                }
                case TriggerType.FriendMpUpper:
                    return checkTriggerInfo.Friends.Find(a => CheckMpRateUpperMore(a,triggerData.Param1)) != null;
                case TriggerType.OpponentMpUnder:
                if (triggerData.Param1 == 100)
                {
                    return checkTriggerInfo.Opponents.Find(a => CheckMpRateUnder(a,triggerData.Param1)) != null;
                } else
                {
                    return checkTriggerInfo.Opponents.Find(a => CheckMpRateUnderMore(a,triggerData.Param1)) != null;
                }
                case TriggerType.OpponentMpUpper:
                    return checkTriggerInfo.Opponents.Find(a => CheckMpRateUnderMore(a,triggerData.Param1)) != null;
            }
            return isTrigger;
        }
        
        private bool CheckMpRateUpperMore(BattlerInfo battlerInfo,int param1)
        {
            return battlerInfo.MpRate >= param1 * 0.01f;
        }

        private bool CheckMpRateUnderMore(BattlerInfo battlerInfo,int param1)
        {
            return battlerInfo.MpRate <= param1 * 0.01f;
        }

        private bool CheckMpRateUnder(BattlerInfo battlerInfo,int param1)
        {
            return battlerInfo.MpRate < param1 * 0.01f;
        }
        public int CheckTargetIndex(SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo,int targetBattlerIndex)
        {
            return -1;
        }
        public void AddTargetIndexList(List<int> targetIndexList,List<int> targetIndexes,BattlerInfo targetBattler,SkillData.TriggerData triggerData,SkillData skillData,CheckTriggerInfo checkTriggerInfo)
        {
            var IsFriend = checkTriggerInfo.IsFriend(targetBattler);
            var targetIndex = targetBattler.Index;
            
            switch (triggerData.TriggerType)
            {
                case TriggerType.FriendMpUnder:
                if (IsFriend)
                {
                    if (triggerData.Param1 == 100)
                    {
                        if (CheckMpRateUnder(targetBattler,triggerData.Param1))
                        {
                            targetIndexList.Add(targetIndex);
                        }
                    } else
                    {
                        if (CheckMpRateUnderMore(targetBattler,triggerData.Param1))
                        {
                            targetIndexList.Add(targetIndex);
                        }
                    }
                }
                break;
                case TriggerType.FriendMpUpper:
                if (IsFriend && CheckMpRateUpperMore(targetBattler,triggerData.Param1))
                {
                    targetIndexList.Add(targetIndex);
                }
                break;
                case TriggerType.OpponentMpUnder:
                if (!IsFriend)
                {
                    if (triggerData.Param1 == 100)
                    {
                        if (CheckMpRateUnder(targetBattler,triggerData.Param1))
                        {
                            targetIndexList.Add(targetIndex);
                        }
                    } else
                    {
                        if (CheckMpRateUnderMore(targetBattler,triggerData.Param1))
                        {
                            targetIndexList.Add(targetIndex);
                        }
                    }
                }
                break;
                case TriggerType.OpponentMpUpper:
                if (!IsFriend && CheckMpRateUpperMore(targetBattler,triggerData.Param1))
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
