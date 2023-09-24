using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleActorAI 
{
    private static BattlerInfo _batterInfo;
    public static (int,int) MakeAutoActorSkillId(List<SkillInfo> skillInfos,BattlerInfo battlerInfo,List<BattlerInfo> partyMember,List<BattlerInfo> targetMember)
    {
        _batterInfo = battlerInfo;
        skillInfos = skillInfos.FindAll(a => a.Id > 100);
        if (skillInfos.Count == 0)
        {
            return (0,0);
        }
        return ActorSkillId(skillInfos,partyMember,targetMember);
    }

    private static (int,int) ActorSkillId(List<SkillInfo> skillInfos,List<BattlerInfo> partyMember,List<BattlerInfo> targetMember)
    {
        var skillTargetAIs = new List<SkillTargetAI>();
        foreach (var skillInfo in skillInfos)
        {
            var skillTargets = MakeSkillTargets(skillInfo,partyMember,targetMember);
            foreach (var skillTarget in skillTargets)
            {
                var skillTargetAI = new SkillTargetAI(skillInfo.Id,skillTarget.Index);
                // 有効範囲
                var attackTargets = MakeAttackTargets(skillInfo,skillTarget,partyMember,targetMember);
                // 攻撃
                if (skillInfo.Master.IsHpDamageFeature())
                {
                    CalcAttackSkillWeight(skillInfo,attackTargets,skillTargetAI);
                }
                // 拘束
                if (skillInfo.Master.IsStateFeature(StateType.Chain))
                {
                    var chainTargets = _batterInfo.isActor ? targetMember : partyMember;
                    CalcChainSkillWeight(skillInfo,attackTargets,skillTargetAI,chainTargets);
                }
                // CA
                if (skillInfo.Master.IsStateFeature(StateType.CounterOura))
                {
                    var caTargets = _batterInfo.isActor ? targetMember : partyMember;
                    CalcCounterOuraSkillWeight(skillInfo,caTargets,skillTargetAI);
                }
                // 回復
                if (skillInfo.Master.IsHpHealFeature())
                {
                    var friends = _batterInfo.isActor ? partyMember : targetMember;
                    friends.Sort((a,b) => a.Hp < b.Hp ? 1:-1);
                    var opponents = _batterInfo.isActor ? targetMember : partyMember;
                    CalcHealSkillWeight(skillInfo,attackTargets,skillTargetAI,friends,opponents);
                }
                // 回避バフ
                if (skillInfo.Master.IsStateFeature(StateType.EvaUp))
                {
                    CalcBuffSkillWeight(skillInfo,attackTargets,skillTargetAI,StateType.EvaUp);
                }
                // 鈍足
                if (skillInfo.Master.IsStateFeature(StateType.Slow))
                {
                    CalcAbnormalSkillWeight(skillInfo,attackTargets,skillTargetAI,StateType.Slow);
                }
                // 挑発
                if (skillInfo.Master.IsStateFeature(StateType.Substitute))
                {
                    // CAを使える
                    if (skillInfos.Find(a => a.Master.IsStateFeature(StateType.CounterOura)) != null)
                    {
                        CalcSubstituteSkillWeight(skillInfo,attackTargets,skillTargetAI,StateType.EvaUp);
                    } else
                    {
                        skillTargetAI.Weigth = 1;
                    }
                }
                // Awake前でMpが支払える
                if (skillInfos.Find(a => a.TriggerDatas.Find(b => b.TriggerType == TriggerType.PayBattleMp) != null) != null)
                {
                    if (skillTargetAI.Weigth > 1 && _batterInfo.IsAwaken == false && skillInfo.Master.MpCost > 0)
                    {
                        skillTargetAI.Weigth += 150;
                    }
                }
                // アフターバーナーを有効活用
                if (skillInfo.Master.MpCost > 0 && _batterInfo.Mp <= 4)
                {
                    skillTargetAI.Weigth += 50;
                }
                /* else
                {
                    // 状態異常
                    if (caTargets.Count > 0)
                    {
                        weight += 100;
                    }
                }
                */
                skillTargetAIs.Add(skillTargetAI);
            }
        }
#if UNITY_EDITOR
        foreach (var skillTargetAI in skillTargetAIs)
        {
            Debug.Log("スキルID = " + skillTargetAI.SkillId + " W =" + skillTargetAI.Weigth);
        }
#endif
        var (skillId,targetId) = GetSkillTargetAI(skillTargetAIs);
        
        return (skillId,targetId);
    }

    private static (int,int) GetSkillTargetAI(List<SkillTargetAI> skillTargetAIs)
    {
        int rate = 0;
        foreach (var skillTargetAI in skillTargetAIs)
        {
            rate += skillTargetAI.Weigth;
        }
        
        rate = UnityEngine.Random.Range (0,rate);
        int skillId = -1;
        int targetId = -1;
        foreach (var skillTargetAI in skillTargetAIs)
        {
            rate -= skillTargetAI.Weigth;
            if (rate <= 0 && skillId == -1)
            {
                skillId = skillTargetAI.SkillId;
                targetId = skillTargetAI.TargetIndex;
            }
        }
        return (skillId,targetId);
    }
    private static List<BattlerInfo> MakeSkillTargets(SkillInfo skillInfo,List<BattlerInfo> partyMember,List<BattlerInfo> targetMember)
    {
        List<BattlerInfo> targets = new List<BattlerInfo>();
        targets.AddRange(partyMember);
        targets.AddRange(targetMember);
        if (skillInfo.Master.TargetType == TargetType.Opponent)
        {
            targets = targetMember;
        } else
        if (skillInfo.Master.TargetType == TargetType.Friend)
        {
            targets = partyMember;
        } else
        if (skillInfo.Master.TargetType == TargetType.Self)
        {
            targets = partyMember.FindAll(a => a.Index == _batterInfo.Index);
        }
        targets = targets.FindAll(a => a.IsAlive() == skillInfo.Master.AliveOnly);
        if (skillInfo.Master.Range == RangeType.S && !_batterInfo.IsState(StateType.Extension))
        {
            var isActor = _batterInfo.isActor;
            var findFront = targets.Find(a => a.isActor != isActor && a.IsAlive() && a.LineIndex == LineType.Front);
            if (findFront != null)
            {
                targets = targets.FindAll(a => a.LineIndex == LineType.Front);
            }
        }
        return targets;
    }

    private static List<BattlerInfo> MakeAttackTargets(SkillInfo skillInfo,BattlerInfo skillTarget,List<BattlerInfo> partyMember,List<BattlerInfo> targetMember)
    {
        List<BattlerInfo> targets = MakeSkillTargets(skillInfo,partyMember,targetMember);
        if (skillInfo.Master.Scope == ScopeType.All)
        {

        } else
        if (skillInfo.Master.Scope == ScopeType.Line)
        {
            targets = targets.FindAll(a => a.LineIndex == skillTarget.LineIndex);
        } else
        if (skillInfo.Master.Scope == ScopeType.One)
        {
            targets = targets.FindAll(a => a.Index == skillTarget.Index);
        }
        return targets;
    }


    private static void CalcAttackSkillWeight(SkillInfo skillInfo,List<BattlerInfo> attackTargets, SkillTargetAI skillTargetAI)
    {
        // CAなら狙わない
        if (attackTargets.Find(a => a.IsState(StateType.CounterOura)) != null)
        {
            skillTargetAI.Weigth = 1;
            return;
        }

        // 攻撃値計算
        var attackFeatures = skillInfo.Master.FeatureDatas.FindAll(a => a.FeatureType == FeatureType.HpDamage);
        foreach (var attackFeature in attackFeatures)
        {
            foreach (var attackTarget in attackTargets)
            {
                var result = new ActionResultInfo(_batterInfo,attackTarget,skillInfo.Master.FeatureDatas,-1);
                skillTargetAI.Weigth += result.HpDamage;
                if (result.HpDamage >= attackTarget.Hp)
                {
                    skillTargetAI.Weigth += 50;
                }
            }
            // 攻撃対象が複数
            if (attackTargets.Count > 1)
            {
                skillTargetAI.Weigth += attackTargets.Count * 50;
            }
        }
    }

    private static void CalcChainSkillWeight(SkillInfo skillInfo,List<BattlerInfo> attackTargets, SkillTargetAI skillTargetAI,List<BattlerInfo> opponents)
    {
        // 攻撃値計算
        var chainFeatures = skillInfo.Master.FeatureDatas.FindAll(a => a.FeatureType == FeatureType.AddState && a.Param1 == (int)StateType.Chain);
        foreach (var chainFeature in chainFeatures)
        {
            foreach (var attackTarget in attackTargets)
            {
                // CAを狙う
                if (attackTarget.IsState(StateType.CounterOura))
                {
                    skillTargetAI.Weigth += 100;
                }
                // ボスクラスを狙う
                if (attackTarget.MaxHp > 200)
                {
                    skillTargetAI.Weigth += 100;
                }
                // Frontを狙う
                if (attackTarget.LineIndex == LineType.Front)
                {
                    skillTargetAI.Weigth += 100;
                } else
                {
                    // Frontが存在しない
                    if (opponents.Find(a => a.IsAlive() && a.LineIndex == LineType.Front) == null)
                    {
                        skillTargetAI.Weigth += 100;
                    }
                }
            }
            // 攻撃対象が複数
            if (attackTargets.Count > 1)
            {
                skillTargetAI.Weigth += attackTargets.Count * 50;
            }
        }
    }

    private static void CalcCounterOuraSkillWeight(SkillInfo skillInfo,List<BattlerInfo> attackTargets, SkillTargetAI skillTargetAI)
    {
        foreach (var attackTarget in attackTargets)
        {
            var result = new ActionResultInfo(_batterInfo,attackTarget,skillInfo.Master.FeatureDatas,-1);
            skillTargetAI.Weigth += result.ReDamage;
            if (result.ReDamage >= attackTarget.Hp)
            {
                skillTargetAI.Weigth += 100;
            }
            var states = attackTarget.GetStateInfoAll(StateType.Substitute);
            // 挑発している
            if (states.Find(a => a.BattlerId == _batterInfo.Index) != null)
            {
                skillTargetAI.Weigth += 100;
            }
        }
        skillTargetAI.Weigth += 50;
    }
    
    private static void CalcBuffSkillWeight(SkillInfo skillInfo,List<BattlerInfo> attackTargets, SkillTargetAI skillTargetAI,StateType stateType)
    {
        if (_batterInfo.TurnCount > 2)
        {
            skillTargetAI.Weigth = 1;
            return;
        }
        foreach (var attackTarget in attackTargets)
        {
            // CA持ちには回避バフを入れない
            if (skillInfo.Master.Scope == ScopeType.One && attackTarget.Skills.Find(a => a.Master.IsStateFeature(StateType.CounterOura)) != null)
            {
                skillTargetAI.Weigth = 1;
                return;
            }
            var stateInfo = attackTarget.GetStateInfoAll(stateType);
            if (stateInfo.Find(a => a.SkillId == skillInfo.Id) != null)
            {
                return;
            }
            skillTargetAI.Weigth += 50;
        }
    }

    private static void CalcAbnormalSkillWeight(SkillInfo skillInfo,List<BattlerInfo> attackTargets, SkillTargetAI skillTargetAI,StateType stateType)
    {
        foreach (var attackTarget in attackTargets)
        {
            var stateInfo = attackTarget.GetStateInfoAll(stateType);
            if (stateInfo.Find(a => a.SkillId == skillInfo.Id) != null)
            {
                return;
            }
            skillTargetAI.Weigth += 50;
        }
        // 攻撃対象が複数
        if (attackTargets.Count > 1)
        {
            skillTargetAI.Weigth += attackTargets.Count * 50;
        }
    }

    private static void CalcHealSkillWeight(SkillInfo skillInfo,List<BattlerInfo> attackTargets, SkillTargetAI skillTargetAI,List<BattlerInfo> friends,List<BattlerInfo> opponents)
    {
        foreach (var attackTarget in attackTargets)
        {
            
            if (friends.Find(a => a.Index == attackTarget.Index) != null)
            {
                if (attackTarget.Kinds.Contains(KindType.Undead))
                {
                    skillTargetAI.Weigth = 1;
                } else
                {
                    // attackTargetsの中で一番Hpが少ない
                    if (((float)attackTarget.Hp / (float)attackTarget.MaxHp) <= 0.33f || attackTarget.Hp < 25)
                    {
                        if (friends[0] == attackTarget)
                        {
                            skillTargetAI.Weigth += 100;
                        }
                        if (!attackTarget.IsAlive())
                        {
                            skillTargetAI.Weigth += 50;
                        }
                    }
                    if (((float)attackTarget.Hp / (float)attackTarget.MaxHp) >= 0.75f)
                    {
                        skillTargetAI.Weigth = 1;
                    }
                }
            }
            if (opponents.Find(a => a.Index == attackTarget.Index) != null)
            {
                if (attackTarget.Kinds.Contains(KindType.Undead))
                {
                    var result = new ActionResultInfo(_batterInfo,attackTarget,skillInfo.Master.FeatureDatas,-1);
                    skillTargetAI.Weigth += result.HpDamage;
                    if (result.HpDamage >= attackTarget.Hp)
                    {
                        skillTargetAI.Weigth += 50;
                    }
                } else
                {
                    skillTargetAI.Weigth = 1;
                }
            }
        }
    }

    private static void CalcSubstituteSkillWeight(SkillInfo skillInfo,List<BattlerInfo> attackTargets, SkillTargetAI skillTargetAI,StateType stateType)
    {
        // すでに挑発している
        if (attackTargets.Find(a => a.IsState(StateType.Substitute)) != null)
        {
            skillTargetAI.Weigth = 1;
            return;
        }
        var count = attackTargets.FindAll(a => !a.IsState(StateType.Substitute)).Count;
        skillTargetAI.Weigth += count * 50;
        skillTargetAI.Weigth *= (int)(((float)_batterInfo.Hp / (float)_batterInfo.MaxHp) * 100);
    }

    public class SkillTargetAI
    {
        public int SkillId;
        public int TargetIndex;
        public int Weigth;

        public SkillTargetAI(int skillId,int targetIndex)
        {
            SkillId = skillId;
            TargetIndex = targetIndex;
            Weigth = 10;
        }
    }
}