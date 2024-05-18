using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public partial class BattleModel : BaseModel
    {
        public List<BattlerInfo> GetBattlerInfos(bool isActor,bool isFriend)
        {
            if (isActor && isFriend || !isActor && !isFriend)
            {
                return _party.BattlerInfos;
            }
            return _troop.BattlerInfos;
        }

        private RangeType CalcRangeType(SkillData skillData,BattlerInfo battlerInfo)
        {
            var rangeType = skillData.Range;
            if (battlerInfo.IsState(StateType.Extension))
            {
                rangeType = RangeType.L;
            }
            return rangeType;
        }

        // 選択範囲が敵味方全員の場合
        private List<int> TargetIndexAll()
        {
            var targetIndexList = new List<int>();
            foreach (var battlerInfo in _party.BattlerInfos)
            {
                targetIndexList.Add(battlerInfo.Index);
            }
            foreach (var battlerInfo in _troop.BattlerInfos)
            {
                targetIndexList.Add(battlerInfo.Index);
            }
            return targetIndexList;
        }

        // 選択範囲が相手
        private List<int> TargetIndexOpponent(bool isActor)
        {   
            var targetIndexList = new List<int>();
            foreach (var battlerInfo in GetBattlerInfos(isActor,false))
            {
                targetIndexList.Add(battlerInfo.Index);
            }
            return targetIndexList;
        }

        private List<int> TargetIndexFriend(bool isActor)
        {
            var targetIndexList = new List<int>();
            foreach (var battlerInfo in GetBattlerInfos(isActor,true))
            {
                targetIndexList.Add(battlerInfo.Index);
            }
            return targetIndexList;
        }

        private List<int> WithinRangeTargetList(BattlerInfo battlerInfo,RangeType skillRangeType)
        {
            var targetIndexList = new List<int>();
            var isActor = battlerInfo.IsActor;
            var targetUnit = isActor ? _troop : _party;
            var friendUnit = isActor ? _party : _troop;
            foreach (var friend in friendUnit.BattlerInfos)
            {
                targetIndexList.Add(friend.Index);
            }

            if (skillRangeType == RangeType.L)
            {
                foreach (var opponent in targetUnit.BattlerInfos)
                {
                    targetIndexList.Add(opponent.Index);
                }
                return targetIndexList;
            }
        
            var selfIsFront = false;
            if (battlerInfo.LineIndex == LineType.Front)
            {
                selfIsFront = true;
            } else
            {
                // 前面の味方が一人もいない場合は前面
                if (friendUnit.AliveBattlerInfos.Find(a => a.LineIndex == LineType.Front) == null)
                {
                    selfIsFront = true;
                }
            }

            if (selfIsFront)
            {
                var targetIsFrontAlive = targetUnit.AliveBattlerInfos.Find(a => a.LineIndex == LineType.Front);
                foreach (var opponent in targetUnit.BattlerInfos)
                {
                    var opponentIsFront = false;
                    if (opponent.LineIndex == LineType.Front)
                    {
                        opponentIsFront = true;
                    } else
                    {
                        // 前面の味方が一人もいない場合は前面
                        if (targetIsFrontAlive == null)
                        {
                            opponentIsFront = true;
                        }
                    }
                    if (opponentIsFront)
                    {
                        targetIndexList.Add(opponent.Index);
                    }
                }
            }
            return targetIndexList;
        }

        // 選択可能な対象のインデックスを取得
        public List<int> GetSkillTargetIndexList(int skillId,int subjectIndex,bool checkCondition,int counterSubjectIndex = -1,ActionInfo actionInfo = null,List<ActionResultInfo> actionResultInfos = null)
        {
            var skillData = DataSystem.FindSkill(skillId);
            var subject = GetBattlerInfo(subjectIndex);
            
            var rangeType = CalcRangeType(skillData,subject);

            var targetIndexList = new List<int>();
            switch (skillData.TargetType)
            {
                case TargetType.All:
                    targetIndexList.AddRange(TargetIndexAll());
                    break;
                case TargetType.Opponent:
                    targetIndexList.AddRange(TargetIndexOpponent(subject.IsActor));
                    break;
                case TargetType.Friend:
                    targetIndexList.AddRange(TargetIndexFriend(subject.IsActor));
                    if (skillData.Scope == ScopeType.WithoutSelfOne || skillData.Scope == ScopeType.WithoutSelfAll)
                    {
                        targetIndexList.Remove(subject.Index);
                    }
                    break;
                case TargetType.Self:
                    targetIndexList.Add(subject.Index);
                    break;
                case TargetType.Counter:
                    if (counterSubjectIndex > -1)
                    {
                        targetIndexList.Add(counterSubjectIndex);
                    }
                    break;
                case TargetType.AttackTarget:
                    if (actionInfo != null && actionResultInfos.Count > 0)
                    {
                        foreach (var actionResultInfo in actionResultInfos)
                        {
                            if (!targetIndexList.Contains(actionResultInfo.TargetIndex))
                            {
                                targetIndexList.Add(actionResultInfo.TargetIndex);     
                            }               
                        }
                    }
                    break;
            }
            switch (skillData.AliveType)
            {
                case AliveType.DeathOnly:
                    targetIndexList = targetIndexList.FindAll(a => !GetBattlerInfo(a).IsAlive());
                    break;
                case AliveType.AliveOnly:
                    targetIndexList = targetIndexList.FindAll(a => GetBattlerInfo(a).IsAlive());
                    break;
                case AliveType.All:
                    break;
            }
            if (skillData.ScopeTriggers.Count > 0)
            {
                targetIndexList = CheckScopeTriggers(targetIndexList,skillData.ScopeTriggers);
            }
            
            var withinRangeTargetList = WithinRangeTargetList(subject,rangeType);
            // 範囲外にいる対象を候補から外す
            targetIndexList = targetIndexList.FindAll(a => withinRangeTargetList.Contains(a));
            if (checkCondition == true)
            {
                targetIndexList = targetIndexList.FindAll(a => CanUseCondition(skillId,subject,a));
            }
            return targetIndexList;
        }
    }
}
