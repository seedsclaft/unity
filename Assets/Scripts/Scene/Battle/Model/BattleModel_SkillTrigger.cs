using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Ryneus
{
    public partial class BattleModel : BaseModel
    {
        private List<SkillData.TriggerData> ConvertTriggerDates(List<SkillTriggerData> skillTriggerDates)
        {
            var triggerDates = new List<SkillData.TriggerData>();
            foreach (var skillTriggerData in skillTriggerDates)
            {
                if (skillTriggerData != null && skillTriggerData.Id > 0)
                {
                    var triggerData = new SkillData.TriggerData
                    {
                        TriggerType = skillTriggerData.TriggerType,
                        Param1 = skillTriggerData.Param1,
                        Param2 = skillTriggerData.Param2,
                        Param3 = skillTriggerData.Param3,
                    };
                    triggerDates.Add(triggerData);
                }
            }
            return triggerDates;
        }

        /// <summary>
        /// 作戦結果のスキルIDと対象を取得
        /// </summary>
        /// <param name="battlerInfo"></param>
        /// <param name="skillTriggerInfos"></param>
        /// <param name="actionInfo"></param>
        /// <param name="actionResultInfos"></param>
        /// <returns></returns>
        private (int,int) SelectSkillTargetBySkillTriggerDates(BattlerInfo battlerInfo,List<SkillTriggerInfo> skillTriggerInfos,ActionInfo actionInfo = null,List<ActionResultInfo> actionResultInfos = null)
        {
            var selectSkillId = -1;
            var selectTargetIndex = -1;
            var counterSubjectIndex = actionInfo != null ? actionInfo.SubjectIndex : -1;
            foreach (var skillTriggerInfo in skillTriggerInfos)
            {
                // 条件
                var triggerDates = ConvertTriggerDates(skillTriggerInfo.SkillTriggerDates);
                if (selectSkillId == -1 && selectTargetIndex == -1 && skillTriggerInfo.SkillId > 0)
                {
                    // 条件なし
                    if (triggerDates.Count == 0)
                    {
                        selectSkillId = skillTriggerInfo.SkillId;
                    } else
                    {
                        if (triggerDates.Count == 2)
                        {
                            triggerDates[0].Param3 = 1; // and判定
                        }
                        if (CanUseSkillTrigger(triggerDates,battlerInfo))
                        {           
                            selectSkillId = skillTriggerInfo.SkillId;
                        }
                    }
                }
                // 優先指定の判定
                if (selectSkillId > -1 && selectTargetIndex == -1)
                {
                    var targetIndexList = GetSkillTargetIndexList(selectSkillId,battlerInfo.Index,true,counterSubjectIndex,actionInfo,actionResultInfos);
                    if (targetIndexList.Count == 0)
                    {
                        var triggeredSkill = DataSystem.FindSkill(selectSkillId);
                        if (actionResultInfos != null && triggeredSkill != null && triggeredSkill.TargetType == TargetType.IsTriggerTarget)
                        {
                            targetIndexList = TriggerTargetList(battlerInfo,triggeredSkill.TriggerDates[0],actionInfo,actionResultInfos);
                        }
                        if (targetIndexList.Count == 0)
                        {
                            selectSkillId = -1;
                        }
                    }
                    var target = CanUseSkillTriggerTarget(selectSkillId,triggerDates,battlerInfo,targetIndexList);
                    if (target > -1)
                    {
                        // 対象が有効か
                        var condition = CanUseCondition(selectSkillId,battlerInfo,target);
                        if (condition)
                        {
                            selectTargetIndex = target;
                        } else
                        {
                            selectSkillId = -1;
                        }
                    } else
                    {
                        selectSkillId = -1;
                    }
                }
                if (selectSkillId > -1 && selectTargetIndex > -1)
                {
                    return (selectSkillId,selectTargetIndex);
                }
            }
            return (selectSkillId,selectTargetIndex);
        }

        private bool CanUseSkillTrigger(List<SkillData.TriggerData> triggerDates,BattlerInfo battlerInfo)
        {
            bool CanUse = true;
            if (triggerDates.Count > 0)
            {
                CanUse = IsTriggeredSkillInfo(battlerInfo,triggerDates,null,new List<ActionResultInfo>());
            }
            return CanUse;
        }
        
        private int CanUseSkillTriggerTarget(int skillId,List<SkillData.TriggerData> triggerDates,BattlerInfo battlerInfo,List<int> targetIndexes)
        {
            // 条件なし
            if (triggerDates.Count == 0)
            {
                return BattleUtility.NearTargetIndex(battlerInfo,targetIndexes);
            }
            var skillData = DataSystem.FindSkill(skillId);
            var targetIndexList1 = new List<int>();
            var targetIndexList2 = new List<int>();
            // ～を優先判定用
            var targetIndexWithInList = new List<int>();
            if (triggerDates.Count == 1)
            {
                targetIndexList2 = targetIndexes;
            }
            for (int i = 0;i < triggerDates.Count;i++)
            {
                var targetIndexList = i == 0 ? targetIndexList1 : targetIndexList2;
                foreach (var targetIndex in targetIndexes)
                {
                    var triggerDate = triggerDates[i];
                    var targetBattler = GetBattlerInfo(targetIndex);
                    var friends = battlerInfo.IsActor ? _party : _troop;
                    var opponents = battlerInfo.IsActor ? _troop : _party;
                    var IsFriend = battlerInfo.IsActor == targetBattler.IsActor;
                    switch (triggerDate.TriggerType)
                    {
                        // ターゲットに含めるか判定
                        case TriggerType.FriendHpRateUnder:
                        // Param2==-1はその対象を起点に平均Hpで比較する
                        if (triggerDate.Param2 == -1)
                        {
                            // この時点で有効なtargetIndexesが判定されているので人数で判定
                            var lineTargets = LineTargetBattlers(skillData.Scope,targetBattler,friends.AliveBattlerInfos);
                            if (lineTargets.All(a => targetIndexes.Contains(a.Index)))
                            {
                                targetIndexList.Add(targetIndex);
                            }
                        } else
                        {
                            // Param1==100の場合は未満
                            if (triggerDate.Param1 == 100)
                            {
                                if (IsFriend && targetBattler.HpRate < 0.01f * triggerDate.Param1)
                                {
                                    targetIndexList.Add(targetIndex);
                                }
                            } else
                            {
                                if (IsFriend && targetBattler.HpRate <= 0.01f * triggerDate.Param1)
                                {
                                    targetIndexList.Add(targetIndex);
                                }
                            }
                        }
                        break;
                        case TriggerType.FriendHpRateUpper:
                        // Param2==-1はその対象を起点に平均Hpで比較する
                        if (triggerDate.Param2 == -1)
                        {
                            // この時点で有効なtargetIndexesが判定されているので人数で判定
                            var lineTargets = LineTargetBattlers(skillData.Scope,targetBattler,friends.AliveBattlerInfos);
                            if (lineTargets.All(a => targetIndexes.Contains(a.Index)))
                            {
                                targetIndexList.Add(targetIndex);
                            }
                        } else
                        {
                            if (IsFriend && targetBattler.HpRate >= 0.01f * triggerDate.Param1)
                            {
                                targetIndexList.Add(targetIndex);
                            }
                        }
                        break;
                        case TriggerType.OpponentHpRateUnder:
                        // Param2==-1はその対象を起点に平均Hpで比較する
                        if (triggerDate.Param2 == -1)
                        {
                            // この時点で有効なtargetIndexesが判定されているので人数で判定
                            var lineTargets = LineTargetBattlers(skillData.Scope,targetBattler,opponents.AliveBattlerInfos);
                            if (lineTargets.All(a => targetIndexes.Contains(a.Index)))
                            {
                                targetIndexList.Add(targetIndex);
                            }
                        } else
                        {
                            if (triggerDate.Param1 == 100)
                            {
                                if (!IsFriend && targetBattler.HpRate < 0.01f * triggerDate.Param1)
                                {
                                    targetIndexList.Add(targetIndex);
                                }
                            } else
                            {
                                if (!IsFriend && targetBattler.HpRate <= 0.01f * triggerDate.Param1)
                                {
                                    targetIndexList.Add(targetIndex);
                                }
                            }
                        }
                        break;
                        case TriggerType.OpponentHpRateUpper:
                        // Param2==-1はその対象を起点に平均Hpで比較する
                        if (triggerDate.Param2 == -1)
                        {
                            // この時点で有効なtargetIndexesが判定されているので人数で判定
                            var lineTargets = LineTargetBattlers(skillData.Scope,targetBattler,opponents.AliveBattlerInfos);
                            if (lineTargets.All(a => targetIndexes.Contains(a.Index)))
                            {
                                targetIndexList.Add(targetIndex);
                            }
                        } else
                        {
                            if (!IsFriend && targetBattler.HpRate >= 0.01f * triggerDate.Param1)
                            {
                                targetIndexList.Add(targetIndex);
                            }
                        }
                        break;
                        case TriggerType.FriendMpUnder:
                        if (triggerDate.Param1 == 100)
                        {
                            if (IsFriend && targetBattler.MpRate < 0.01f * triggerDate.Param1)
                            {
                                targetIndexList.Add(targetIndex);
                            }
                        } else
                        {
                            if (IsFriend && targetBattler.MpRate <= 0.01f * triggerDate.Param1)
                            {
                                targetIndexList.Add(targetIndex);
                            }
                        }
                        break;
                        case TriggerType.FriendMpUpper:
                        if (IsFriend && targetBattler.MpRate >= 0.01f * triggerDate.Param1)
                        {
                            targetIndexList.Add(targetIndex);
                        }
                        break;
                        case TriggerType.OpponentMpUnder:
                        if (triggerDate.Param1 == 100)
                        {
                            if (!IsFriend && targetBattler.MpRate < 0.01f * triggerDate.Param1)
                            {
                                targetIndexList.Add(targetIndex);
                            }
                        } else
                        {
                            if (!IsFriend && targetBattler.MpRate <= 0.01f * triggerDate.Param1)
                            {
                                targetIndexList.Add(targetIndex);
                            }
                        }
                        break;
                        case TriggerType.OpponentMpUpper:
                        if (!IsFriend && targetBattler.MpRate >= 0.01f * triggerDate.Param1)
                        {
                            targetIndexList.Add(targetIndex);
                        }
                        break;
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
                        if (IsFriend && friends.AliveBattlerInfos.FindAll(a => a.LineIndex == targetBattler.LineIndex).Count >= triggerDate.Param1)
                        {
                            targetIndexList.Add(targetIndex);
                        }
                        break;
                        case TriggerType.OpponentMoreTargetCount:
                        if (!IsFriend && opponents.AliveBattlerInfos.FindAll(a => a.LineIndex == targetBattler.LineIndex).Count >= triggerDate.Param1)
                        {
                            targetIndexList.Add(targetIndex);
                        }
                        break;
                        case TriggerType.FriendHasKind:
                        if (IsFriend && targetBattler.Kinds.Contains((KindType)triggerDate.Param1))
                        {
                            targetIndexList.Add(targetIndex);
                        }
                        break;
                        case TriggerType.OpponentHasKind:
                        if (!IsFriend && targetBattler.Kinds.Contains((KindType)triggerDate.Param1))
                        {
                            targetIndexList.Add(targetIndex);
                        }
                        break;
                        case TriggerType.FriendIsState:
                        if (IsFriend && targetBattler.IsState((StateType)triggerDate.Param1))
                        {
                            targetIndexList.Add(targetIndex);
                        }
                        break;
                        case TriggerType.OpponentIsState:
                        if (!IsFriend && targetBattler.IsState((StateType)triggerDate.Param1))
                        {
                            targetIndexList.Add(targetIndex);
                        }
                        break;
                        case TriggerType.FriendIsNotState:
                        if (IsFriend && !targetBattler.IsState((StateType)triggerDate.Param1))
                        {
                            targetIndexList.Add(targetIndex);
                        }
                        break;
                        case TriggerType.OpponentIsNotState:
                        if (!IsFriend && !targetBattler.IsState((StateType)triggerDate.Param1))
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
                        if (battlerInfo.Index == targetIndex && targetBattler.StateInfos.Find(a => a.Master.Buff == true) != null)
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
                        if (battlerInfo.Index == targetIndex && targetBattler.StateInfos.Find(a => a.Master.DeBuff == true) != null)
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
                        case TriggerType.IsNotAwaken:
                        if (battlerInfo.Index == targetIndex && !targetBattler.IsAwaken)
                        {
                            targetIndexList.Add(targetIndex);
                        }
                        break;
                        case TriggerType.IsAwaken:
                        if (battlerInfo.Index == targetIndex && targetBattler.IsAwaken)
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
                        case TriggerType.FriendMembersMoreCount:
                        if (friends.AliveBattlerInfos.Count >= triggerDate.Param1)
                        {
                            targetIndexList.Add(targetIndex);
                        }
                        break;
                        case TriggerType.FriendMembersLessCount:
                        if (friends.AliveBattlerInfos.Count <= triggerDate.Param1)
                        {
                            targetIndexList.Add(targetIndex);
                        }
                        break;
                        case TriggerType.OpponentMembersMoreCount:
                        if (opponents.AliveBattlerInfos.Count >= triggerDate.Param1)
                        {
                            targetIndexList.Add(targetIndex);
                        }
                        break;
                        case TriggerType.OpponentMembersLessCount:
                        if (opponents.AliveBattlerInfos.Count <= triggerDate.Param1)
                        {
                            targetIndexList.Add(targetIndex);
                        }
                        break;
                        case TriggerType.TurnNum:
                        if (battlerInfo.TurnCount == triggerDate.Param1)
                        {
                            targetIndexList.Add(targetIndex);
                        }
                        break;
                        case TriggerType.TurnNumPer:
                        if ((battlerInfo.TurnCount % triggerDate.Param1) - triggerDate.Param2 == 0)
                        {
                            targetIndexList.Add(targetIndex);
                        }
                        break;
                        case TriggerType.SelfTargetOnly:
                        if (battlerInfo.Index == targetIndex)
                        {
                            targetIndexList.Add(targetIndex);
                        }
                        break;
                        case TriggerType.SelfTargetNotOnly:
                        if (battlerInfo.Index != targetIndex)
                        {
                            targetIndexList.Add(targetIndex);
                        }
                        break;
                        default:
                            targetIndexList.Add(targetIndex);
                        break;
                    }
                    if (triggerDate.Param2 == 1)
                    {
                        targetIndexWithInList.Add(targetIndex);
                    }
                }
            }
            var bindTargetIndexList = new List<int>();
            foreach (var targetIndex1 in targetIndexList1)
            {
                if (targetIndexList2.Contains(targetIndex1))
                {
                    bindTargetIndexList.Add(targetIndex1);
                }
            }

            // ～範囲優先の第二候補があれば変更
            if (bindTargetIndexList.Count == 0)
            {   
                bindTargetIndexList = targetIndexWithInList;
            }

            // ～が高い・低い順位で選択
            var friendTargets = new List<BattlerInfo>();
            var opponentTargets = new List<BattlerInfo>();
            foreach (var bindTargetIndex in bindTargetIndexList)
            {
                var bindBattlerInfo = GetBattlerInfo(bindTargetIndex);
                if (bindBattlerInfo.IsActor && battlerInfo.IsActor)
                {
                    friendTargets.Add(bindBattlerInfo);
                } else
                {
                    opponentTargets.Add(bindBattlerInfo);
                }
            }
            for (int i = 0;i < triggerDates.Count;i++)
            {
                var triggerDate = triggerDates[i];
                switch (triggerDate.TriggerType)
                {
                    case TriggerType.LessHpFriend:
                    if (friendTargets.Count > 0)
                    {
                        // Param1==1の場合は割合
                        if (triggerDate.Param1 == 1)
                        {
                            friendTargets.Sort((a,b) => a.HpRate < b.HpRate ? -1: 1);
                            var hpRate = friendTargets[0].HpRate;
                            friendTargets = friendTargets.FindAll(a => a.HpRate == hpRate);
                        } else
                        {
                            friendTargets.Sort((a,b) => a.Hp < b.Hp ? -1: 1);
                            var hp = friendTargets[0].Hp;
                            friendTargets = friendTargets.FindAll(a => a.Hp == hp);
                        }
                        return BattleUtility.NearTargetIndex(battlerInfo,friendTargets);
                    }
                    break;
                    case TriggerType.MostHpFriend:
                    if (friendTargets.Count > 0)
                    {
                        if (triggerDate.Param1 == 1)
                        {
                            friendTargets.Sort((a,b) => a.HpRate < b.HpRate ? 1: -1);
                            var hpRate = friendTargets[0].HpRate;
                            friendTargets = friendTargets.FindAll(a => a.HpRate == hpRate);
                        } else
                        {
                            friendTargets.Sort((a,b) => a.Hp < b.Hp ? 1: -1);
                            var hp = friendTargets[0].Hp;
                            friendTargets = friendTargets.FindAll(a => a.Hp == hp);
                        }
                        return BattleUtility.NearTargetIndex(battlerInfo,friendTargets);
                    }
                    break;
                    case TriggerType.LessHpTarget:
                    if (opponentTargets.Count > 0)
                    {
                        if (triggerDate.Param1 == 1)
                        {
                            opponentTargets.Sort((a,b) => a.HpRate < b.HpRate ? -1: 1);
                            var hpRate = opponentTargets[0].HpRate;
                            opponentTargets = opponentTargets.FindAll(a => a.HpRate == hpRate);
                        } else
                        {
                            opponentTargets.Sort((a,b) => a.Hp < b.Hp ? -1: 1);
                            var hp = opponentTargets[0].Hp;
                            opponentTargets = opponentTargets.FindAll(a => a.Hp == hp);
                        }
                        return BattleUtility.NearTargetIndex(battlerInfo,opponentTargets);
                    }
                    break;
                    case TriggerType.MostHpTarget:
                    if (opponentTargets.Count > 0)
                    {
                        if (triggerDate.Param1 == 1)
                        {
                            opponentTargets.Sort((a,b) => a.HpRate < b.HpRate ? 1: -1);
                            var hpRate = opponentTargets[0].HpRate;
                            opponentTargets = opponentTargets.FindAll(a => a.HpRate == hpRate);
                        } else
                        {
                            opponentTargets.Sort((a,b) => a.Hp < b.Hp ? 1: -1);
                            var hp = opponentTargets[0].Hp;
                            opponentTargets = opponentTargets.FindAll(a => a.Hp == hp);
                        }
                        return BattleUtility.NearTargetIndex(battlerInfo,opponentTargets);
                    }
                    break;
                    case TriggerType.FriendLineMoreTarget:
                    if (friendTargets.Count > 0)
                    {
                        var front = friendTargets.FindAll(a => a.LineIndex == LineType.Front);
                        var back = friendTargets.FindAll(a => a.LineIndex == LineType.Back);
                        if (back.Count > front.Count)
                        {
                            return BattleUtility.NearTargetIndex(battlerInfo,back);
                        } else
                        {
                            return BattleUtility.NearTargetIndex(battlerInfo,front);
                        }
                    }
                    break;
                    case TriggerType.FriendLineLessTarget:
                    if (friendTargets.Count > 0)
                    {
                        var front = friendTargets.FindAll(a => a.LineIndex == LineType.Front);
                        var back = friendTargets.FindAll(a => a.LineIndex == LineType.Back);
                        if (back.Count < front.Count)
                        {
                            return BattleUtility.NearTargetIndex(battlerInfo,back);
                        } else
                        {
                            return BattleUtility.NearTargetIndex(battlerInfo,front);
                        }
                    }
                    break;
                    case TriggerType.OpponentLineMoreTarget:
                    if (opponentTargets.Count > 0)
                    {
                        var front = opponentTargets.FindAll(a => a.LineIndex == LineType.Front);
                        var back = opponentTargets.FindAll(a => a.LineIndex == LineType.Back);
                        if (back.Count > front.Count)
                        {
                            return BattleUtility.NearTargetIndex(battlerInfo,back);
                        } else
                        {
                            return BattleUtility.NearTargetIndex(battlerInfo,front);
                        }
                    }
                    break;
                    case TriggerType.OpponentLineLessTarget:
                    if (opponentTargets.Count > 0)
                    {
                        var front = opponentTargets.FindAll(a => a.LineIndex == LineType.Front);
                        var back = opponentTargets.FindAll(a => a.LineIndex == LineType.Back);
                        if (back.Count < front.Count)
                        {
                            return BattleUtility.NearTargetIndex(battlerInfo,back);
                        } else
                        {
                            return BattleUtility.NearTargetIndex(battlerInfo,front);
                        }
                    }
                    break;
                    case TriggerType.FriendStatusUpper:
                        var friendStatusUpperIndex = SortStatusUpperTargetIndex(battlerInfo,friendTargets,(StatusParamType)triggerDate.Param1);
                        return friendStatusUpperIndex;
                    case TriggerType.FriendStatusUnder:
                        var friendStatusUnderIndex = SortStatusUnderTargetIndex(battlerInfo,friendTargets,(StatusParamType)triggerDate.Param1);
                        return friendStatusUnderIndex;
                    case TriggerType.OpponentStatusUpper:
                        var opponentStatusUpperIndex = SortStatusUpperTargetIndex(battlerInfo,opponentTargets,(StatusParamType)triggerDate.Param1);
                        return opponentStatusUpperIndex;
                    case TriggerType.OpponentStatusUnder:
                        var opponentStatusUnderIndex = SortStatusUnderTargetIndex(battlerInfo,opponentTargets,(StatusParamType)triggerDate.Param1);
                        return opponentStatusUnderIndex;
                    default:
                    break;
                }
            }
            if (bindTargetIndexList.Count > 0)
            {
                // 複数候補は列に近い方を選ぶ
                return BattleUtility.NearTargetIndex(battlerInfo,bindTargetIndexList);
            }
            return -1;
        }


        private int SortStatusUpperTargetIndex(BattlerInfo battlerInfo,List<BattlerInfo> targetInfos,StatusParamType statusParamType)
        {
            if (targetInfos.Count > 0)
            {
                if (statusParamType == (int)StatusParamType.Hp)
                {
                    targetInfos.Sort((a,b) => a.MaxHp > b.MaxHp ? -1: 1);
                    var hp = targetInfos[0].MaxHp;
                    targetInfos = targetInfos.FindAll(a => a.MaxHp == hp);
                } else
                if (statusParamType == StatusParamType.Mp)
                {
                    targetInfos.Sort((a,b) => a.MaxMp > b.MaxMp ? -1: 1);
                    var mp = targetInfos[0].MaxMp;
                    targetInfos = targetInfos.FindAll(a => a.MaxMp == mp);
                } else
                if (statusParamType == StatusParamType.Atk)
                {
                    targetInfos.Sort((a,b) => a.CurrentAtk() > b.CurrentAtk() ? -1: 1);
                    var atk = targetInfos[0].CurrentAtk();
                    targetInfos = targetInfos.FindAll(a => a.CurrentAtk() == atk);
                } else
                if (statusParamType == StatusParamType.Def)
                {
                    targetInfos.Sort((a,b) => a.CurrentDef() > b.CurrentDef() ? -1: 1);
                    var def = targetInfos[0].CurrentDef();
                    targetInfos = targetInfos.FindAll(a => a.CurrentDef() == def);
                } else
                if (statusParamType == StatusParamType.Spd)
                {
                    targetInfos.Sort((a,b) => a.CurrentSpd() > b.CurrentSpd() ? -1: 1);
                    var spd = targetInfos[0].CurrentSpd();
                    targetInfos = targetInfos.FindAll(a => a.CurrentSpd() == spd);
                }
                return BattleUtility.NearTargetIndex(battlerInfo,targetInfos);
            }
            return -1;
        }

        private int SortStatusUnderTargetIndex(BattlerInfo battlerInfo,List<BattlerInfo> targetInfos,StatusParamType statusParamType)
        {
            if (targetInfos.Count > 0)
            {
                if (statusParamType == (int)StatusParamType.Hp)
                {
                    targetInfos.Sort((a,b) => a.MaxHp > b.MaxHp ? 1: -1);
                    var hp = targetInfos[0].MaxHp;
                    targetInfos = targetInfos.FindAll(a => a.MaxHp == hp);
                } else
                if (statusParamType == StatusParamType.Mp)
                {
                    targetInfos.Sort((a,b) => a.MaxMp > b.MaxMp ? 1: -1);
                    var mp = targetInfos[0].MaxMp;
                    targetInfos = targetInfos.FindAll(a => a.MaxMp == mp);
                } else
                if (statusParamType == StatusParamType.Atk)
                {
                    targetInfos.Sort((a,b) => a.CurrentAtk() > b.CurrentAtk() ? 1: -1);
                    var atk = targetInfos[0].CurrentAtk();
                    targetInfos = targetInfos.FindAll(a => a.CurrentAtk() == atk);
                } else
                if (statusParamType == StatusParamType.Def)
                {
                    targetInfos.Sort((a,b) => a.CurrentDef() > b.CurrentDef() ? 1: -1);
                    var def = targetInfos[0].CurrentDef();
                    targetInfos = targetInfos.FindAll(a => a.CurrentDef() == def);
                } else
                if (statusParamType == StatusParamType.Spd)
                {
                    targetInfos.Sort((a,b) => a.CurrentSpd() > b.CurrentSpd() ? 1: -1);
                    var spd = targetInfos[0].CurrentSpd();
                    targetInfos = targetInfos.FindAll(a => a.CurrentSpd() == spd);
                }
                return BattleUtility.NearTargetIndex(battlerInfo,targetInfos);
            }
            return -1;
        }
    }
}
