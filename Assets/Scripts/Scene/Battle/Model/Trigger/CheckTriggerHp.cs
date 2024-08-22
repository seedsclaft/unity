using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ryneus
{
    public class CheckTriggerHp : ICheckTrigger
    {
        public bool CheckTrigger(SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo)
        {
            switch (triggerData.TriggerType)
            {
                case TriggerType.LessHpFriend: // 別処理で判定するためここではパス
                case TriggerType.MostHpFriend: // 別処理で判定するためここではパス
                case TriggerType.LessHpTarget: // 別処理で判定するためここではパス
                case TriggerType.MostHpTarget: // 別処理で判定するためここではパス
                    return true;
                case TriggerType.SelfHpRateUnder:
                    return CheckHpRateUnderMore(battlerInfo,triggerData.Param1);
                case TriggerType.SelfHpRateUpper:
                    return CheckHpRateUpperMore(battlerInfo,triggerData.Param1);
                case TriggerType.FriendHpRateUnder:
                // Param1==100の場合は未満
                if (triggerData.Param1 == 100)
                {
                    return checkTriggerInfo.Friends.Find(a => CheckHpRateUnder(a,triggerData.Param1)) != null;
                } else
                {
                    return checkTriggerInfo.Friends.Find(a => CheckHpRateUnderMore(a,triggerData.Param1)) != null;
                }
                case TriggerType.FriendHpRateUpper:
                    return checkTriggerInfo.Friends.Find(a => CheckHpRateUpperMore(a,triggerData.Param1)) != null;
                case TriggerType.OpponentHpRateUnder:
                // Param1==100の場合は未満
                if (triggerData.Param1 == 100)
                {
                    return checkTriggerInfo.Opponents.Find(a => CheckHpRateUnder(a,triggerData.Param1)) != null;
                } else
                {
                    return checkTriggerInfo.Opponents.Find(a => CheckHpRateUnderMore(a,triggerData.Param1)) != null;
                }
                case TriggerType.OpponentHpRateUpper:
                    return checkTriggerInfo.Opponents.Find(a => CheckHpRateUpperMore(a,triggerData.Param1)) != null;
                case TriggerType.HpUnder:
                    return battlerInfo.Hp <= triggerData.Param1;
            }
            return false;
        }
    
        private bool CheckHpRateUpperMore(BattlerInfo battlerInfo,int param1)
        {
            return battlerInfo.HpRate >= param1 * 0.01f;
        }

        private bool CheckHpRateUnderMore(BattlerInfo battlerInfo,int param1)
        {
            return battlerInfo.HpRate <= param1 * 0.01f;
        }

        private bool CheckHpRateUnder(BattlerInfo battlerInfo,int param1)
        {
            return battlerInfo.HpRate < param1 * 0.01f;
        }

        
        public int CheckTargetIndex(SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo)
        {
            switch (triggerData.TriggerType)
            {
                case TriggerType.LessHpFriend:
                    return LessHpTargetIndex(checkTriggerInfo.Friends,battlerInfo,triggerData.Param1);
                case TriggerType.MostHpFriend:
                    return MostHpTargetIndex(checkTriggerInfo.Friends,battlerInfo,triggerData.Param1);
                case TriggerType.LessHpTarget:
                    return LessHpTargetIndex(checkTriggerInfo.Opponents,battlerInfo,triggerData.Param1);
                case TriggerType.MostHpTarget:
                    return MostHpTargetIndex(checkTriggerInfo.Opponents,battlerInfo,triggerData.Param1);
            }
            return -1;
        }

        private int LessHpTargetIndex(List<BattlerInfo> targetBattlers,BattlerInfo battlerInfo,int param1)
        {
            if (targetBattlers.Count > 0)
            {
                // Param1==1の場合は割合
                if (param1 == 1)
                {
                    targetBattlers.Sort((a,b) => a.HpRate < b.HpRate ? -1: 1);
                    var hpRate = targetBattlers[0].HpRate;
                    targetBattlers = targetBattlers.FindAll(a => a.HpRate == hpRate);
                } else
                {
                    targetBattlers.Sort((a,b) => a.Hp < b.Hp ? -1: 1);
                    var hp = targetBattlers[0].Hp;
                    targetBattlers = targetBattlers.FindAll(a => a.Hp == hp);
                }
                return BattleUtility.NearTargetIndex(battlerInfo,targetBattlers);
            }
            return -1;
        }        
        
        private int MostHpTargetIndex(List<BattlerInfo> targetBattlers,BattlerInfo battlerInfo,int param1)
        {
            if (targetBattlers.Count > 0)
            {
                // Param1==1の場合は割合
                if (param1 == 1)
                {
                    targetBattlers.Sort((a,b) => a.HpRate > b.HpRate ? -1: 1);
                    var hpRate = targetBattlers[0].HpRate;
                    targetBattlers = targetBattlers.FindAll(a => a.HpRate == hpRate);
                } else
                {
                    targetBattlers.Sort((a,b) => a.Hp > b.Hp ? -1: 1);
                    var hp = targetBattlers[0].Hp;
                    targetBattlers = targetBattlers.FindAll(a => a.Hp == hp);
                }
                return BattleUtility.NearTargetIndex(battlerInfo,targetBattlers);
            }
            return -1;
        }

        public void AddTargetIndexList(List<int> targetIndexList,List<int> targetIndexes,BattlerInfo targetBattler,SkillData.TriggerData triggerData,SkillData skillData,CheckTriggerInfo checkTriggerInfo)
        {
            var IsFriend = checkTriggerInfo.IsFriend(targetBattler);
            var targetIndex = targetBattler.Index;
            
            switch (triggerData.TriggerType)
            {
                case TriggerType.SelfHpRateUnder:
                    if (CheckHpRateUnderMore(checkTriggerInfo.BattlerInfo,triggerData.Param1))
                    {
                        targetIndexList.Add(targetIndex);
                    }
                    break;
                case TriggerType.SelfHpRateUpper:
                    if ( CheckHpRateUpperMore(checkTriggerInfo.BattlerInfo,triggerData.Param1))
                    {
                        targetIndexList.Add(targetIndex);
                    }
                    break;
                // ターゲットに含めるか判定
                case TriggerType.FriendHpRateUnder:
                // Param2==-1はその対象を起点に平均Hpで比較する
                if (triggerData.Param2 == -1)
                {
                    // この時点で有効なtargetIndexesが判定されているので人数で判定
                    var lineTargets = LineTargetBattlers(skillData.Scope,targetBattler,checkTriggerInfo.AliveBattlerInfos(IsFriend));
                    if (lineTargets.All(a => targetIndexes.Contains(a.Index)))
                    {
                        targetIndexList.Add(targetIndex);
                    }
                } else
                {
                    // Param1==100の場合は未満
                    if (triggerData.Param1 == 100)
                    {
                        if (IsFriend && CheckHpRateUnder(targetBattler,triggerData.Param1))
                        {
                            targetIndexList.Add(targetIndex);
                        }
                    } else
                    {
                        if (IsFriend && CheckHpRateUnderMore(targetBattler,triggerData.Param1))
                        {
                            targetIndexList.Add(targetIndex);
                        }
                    }
                }
                break;
                case TriggerType.FriendHpRateUpper:
                // Param2==-1はその対象を起点に平均Hpで比較する
                if (triggerData.Param2 == -1)
                {
                    // この時点で有効なtargetIndexesが判定されているので人数で判定
                    var lineTargets = LineTargetBattlers(skillData.Scope,targetBattler,checkTriggerInfo.AliveBattlerInfos(IsFriend));
                    if (lineTargets.All(a => targetIndexes.Contains(a.Index)))
                    {
                        targetIndexList.Add(targetIndex);
                    }
                } else
                {
                    if (IsFriend && CheckHpRateUpperMore(targetBattler,triggerData.Param1))
                    {
                        targetIndexList.Add(targetIndex);
                    }
                }
                break;
                case TriggerType.OpponentHpRateUnder:
                // Param2==-1はその対象を起点に平均Hpで比較する
                if (triggerData.Param2 == -1)
                {
                    // この時点で有効なtargetIndexesが判定されているので人数で判定
                    var lineTargets = LineTargetBattlers(skillData.Scope,targetBattler,checkTriggerInfo.AliveBattlerInfos(!IsFriend));
                    if (lineTargets.All(a => targetIndexes.Contains(a.Index)))
                    {
                        targetIndexList.Add(targetIndex);
                    }
                } else
                {
                    if (triggerData.Param1 == 100)
                    {
                        if (!IsFriend && CheckHpRateUnder(targetBattler,triggerData.Param1))
                        {
                            targetIndexList.Add(targetIndex);
                        }
                    } else
                    {
                        if (!IsFriend && CheckHpRateUnderMore(targetBattler,triggerData.Param1))
                        {
                            targetIndexList.Add(targetIndex);
                        }
                    }
                }
                break;
                case TriggerType.OpponentHpRateUpper:
                // Param2==-1はその対象を起点に平均Hpで比較する
                if (triggerData.Param2 == -1)
                {
                    // この時点で有効なtargetIndexesが判定されているので人数で判定
                    var lineTargets = LineTargetBattlers(skillData.Scope,targetBattler,checkTriggerInfo.AliveBattlerInfos(!IsFriend));
                    if (lineTargets.All(a => targetIndexes.Contains(a.Index)))
                    {
                        targetIndexList.Add(targetIndex);
                    }
                } else
                {
                    if (!IsFriend && CheckHpRateUpperMore(targetBattler,triggerData.Param1))
                    {
                        targetIndexList.Add(targetIndex);
                    }
                }
                break;
            }
        }

        
        private List<BattlerInfo> LineTargetBattlers(ScopeType scopeType,BattlerInfo targetBattler,List<BattlerInfo> targetBatterInfos)
        {
            var fronts = targetBatterInfos.FindAll(a => a.LineIndex == LineType.Front);
            var backs = targetBatterInfos.FindAll(a => a.LineIndex == LineType.Back);
            // この時点で有効なtargetIndexesが判定されているので人数で判定
            var lineTargets = new List<BattlerInfo>(){targetBattler};
            if (scopeType == ScopeType.Line)
            {
                lineTargets = targetBattler.LineIndex == LineType.Front ? fronts : backs;
            } else
            if (scopeType == ScopeType.All)
            {
                lineTargets = targetBatterInfos;
            } else
            if (scopeType == ScopeType.FrontLine)
            {
                lineTargets = fronts;
            } else
            if (scopeType == ScopeType.WithoutSelfAll)
            {
                lineTargets = targetBatterInfos;
                lineTargets.Remove(targetBattler);
            }
            return lineTargets;
        }

        public void AddTriggerTargetList(List<int> targetIndexList,SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo)
        {

        }
    }
}
