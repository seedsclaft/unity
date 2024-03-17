using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
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
                        var chainTargets = _batterInfo.IsActor ? targetMember : partyMember;
                        CalcChainSkillWeight(skillInfo,attackTargets,skillTargetAI,chainTargets);
                    }
                    // CA
                    if (skillInfo.Master.IsStateFeature(StateType.CounterAura))
                    {
                        var caTargets = _batterInfo.IsActor ? targetMember : partyMember;
                        CalcCounterOuraSkillWeight(skillInfo,caTargets,skillTargetAI);
                    }
                    // 回復
                    if (skillInfo.Master.IsHpHealFeature())
                    {
                        var friends = _batterInfo.IsActor ? partyMember : targetMember;
                        friends.Sort((a,b) => a.Hp < b.Hp ? 1:-1);
                        var opponents = _batterInfo.IsActor ? targetMember : partyMember;
                        CalcHealSkillWeight(skillInfo,attackTargets,skillTargetAI,friends,opponents);
                    }
                    // 回避バフ
                    if (skillInfo.Master.IsStateFeature(StateType.EvaUp))
                    {
                        CalcBuffSkillWeight(skillInfo,attackTargets,skillTargetAI,StateType.EvaUp);
                    }
                    // 状態異常
                    if (skillInfo.Master.IsStateFeature(StateType.Stun) 
                    || skillInfo.Master.IsStateFeature(StateType.Slow)
                    || skillInfo.Master.IsStateFeature(StateType.Curse)
                    || skillInfo.Master.IsStateFeature(StateType.BurnDamage)
                    || skillInfo.Master.IsStateFeature(StateType.Blind)
                    || skillInfo.Master.IsStateFeature(StateType.Freeze))
                    {
                        CalcAbnormalSkillWeight(skillInfo,attackTargets,skillTargetAI,StateType.Slow);
                    }
                    // プリズム
                    if (skillInfo.Master.IsStateFeature(StateType.Prism))
                    {
                        CalcPrismSkillWeight(skillInfo,skillTargetAI);
                    }
                    // 攻撃無効
                    if (skillInfo.Master.IsStateFeature(StateType.NoDamage))
                    {
                        CalcNoDamageSkillWeight(skillInfo,skillTarget,skillTargetAI);
                    }
                    // 挑発
                    if (skillInfo.Master.IsStateFeature(StateType.Substitute))
                    {
                        // CAを使える
                        if (skillInfos.Find(a => a.Master.IsStateFeature(StateType.CounterAura)) != null)
                        {
                            CalcSubstituteSkillWeight(skillInfo,attackTargets,skillTargetAI,StateType.EvaUp);
                        } else
                        {
                            skillTargetAI.Weigth = 1;
                        }
                    }
                    // Awake前でMpが支払える
                    if (skillInfos.Find(a => a.TriggerDates.Find(b => b.TriggerType == TriggerType.PayBattleMp) != null) != null)
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
                Debug.Log("スキル = " + DataSystem.Skills.Find(a => a.Id == skillTargetAI.SkillId).Name + " W =" + skillTargetAI.Weigth);
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
            targets = targets.FindAll(a => a.IsAlive() && (skillInfo.Master.AliveType != AliveType.DeathOnly));
            if (skillInfo.Master.Range == RangeType.S && !_batterInfo.IsState(StateType.Extension))
            {
                var isActor = _batterInfo.IsActor;
                var findFront = targets.Find(a => a.IsActor != isActor && a.IsAlive() && a.LineIndex == LineType.Front);
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
            if (attackTargets.Find(a => a.IsState(StateType.CounterAura)) != null)
            {
                skillTargetAI.Weigth = 1;
                return;
            }

            // 攻撃値計算
            var attackFeatures = skillInfo.Master.FeatureDates.FindAll(a => a.FeatureType == FeatureType.HpDamage || a.FeatureType == FeatureType.HpConsumeDamage);
            
            // 攻撃回数
            var repeatTime = skillInfo.Master.RepeatTime;
            repeatTime += _batterInfo.GetStateInfoAll(StateType.Prism).Count;
            foreach (var attackFeature in attackFeatures)
            {
                foreach (var attackTarget in attackTargets)
                {
                    var result = new ActionResultInfo(_batterInfo,attackTarget,skillInfo.Master.FeatureDates,-1);
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
                if (repeatTime > 1)
                {
                    skillTargetAI.Weigth += (repeatTime-1) * 15;
                }
            }
        }

        private static void CalcChainSkillWeight(SkillInfo skillInfo,List<BattlerInfo> attackTargets, SkillTargetAI skillTargetAI,List<BattlerInfo> opponents)
        {
            // 攻撃値計算
            var chainFeatures = skillInfo.Master.FeatureDates.FindAll(a => (a.FeatureType == FeatureType.AddState || a.FeatureType == FeatureType.AddStateNextTurn)&& a.Param1 == (int)StateType.Chain);
            foreach (var chainFeature in chainFeatures)
            {
                foreach (var attackTarget in attackTargets)
                {
                    // CAを狙う
                    if (attackTarget.IsState(StateType.CounterAura))
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
                var result = new ActionResultInfo(_batterInfo,attackTarget,skillInfo.Master.FeatureDates,-1);
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
                if (skillInfo.Master.Scope == ScopeType.One && attackTarget.Skills.Find(a => a.Master.IsStateFeature(StateType.CounterAura)) != null)
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
            skillTargetAI.Weigth += 10;
            foreach (var attackTarget in attackTargets)
            {
                var stateInfo = attackTarget.GetStateInfoAll(stateType);
                if (stateInfo.Find(a => a.SkillId == skillInfo.Id) != null)
                {
                    return;
                }
                skillTargetAI.Weigth += 25;
            }
        }

        private static void CalcPrismSkillWeight(SkillInfo skillInfo,SkillTargetAI skillTargetAI)
        {
            // プリズム段階数
            int prismNum = _batterInfo.GetStateInfoAll(StateType.Prism).Count;
            if (prismNum < 3)
            {
                skillTargetAI.Weigth = 75 - prismNum * 25;
            } else
            // アタックヒールの時
            if (prismNum < 5 && _batterInfo.Skills.Find(a => a.Master.IsStateFeature(StateType.AssistHeal)) != null)
            {
                skillTargetAI.Weigth = 75 - (prismNum-2) * 25;
            }
        }

        private static void CalcNoDamageSkillWeight(SkillInfo skillInfo,BattlerInfo skillTarget,SkillTargetAI skillTargetAI)
        {
            if (skillTarget.IsState(StateType.NoDamage))
            {
                return;
            }
            // 残り使用回数
            var useCount = _batterInfo.Mp / skillInfo.Master.MpCost;
            // Hp割合
            var hpRate = ((float)skillTarget.Hp / (float)skillTarget.MaxHp) * 50;
            skillTargetAI.Weigth = useCount * 5 + (50 - (int)hpRate);
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
                        var result = new ActionResultInfo(_batterInfo,attackTarget,skillInfo.Master.FeatureDates,-1);
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
}