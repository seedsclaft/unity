using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    [Serializable]
    public class ActionResultInfo 
    {
        private int _subjectIndex = 0;
        public int SubjectIndex => _subjectIndex;
        private int _targetIndex = 0;
        public int TargetIndex => _targetIndex;

        private int _skillId = -1;
        public int SkillId => _skillId;
        public ActionResultInfo(BattlerInfo subject,BattlerInfo target,List<SkillData.FeatureData> featureDates,int skillId,bool isOneTarget = false,SkillInfo skillInfo = null)
        {
            if (subject != null && target != null)
            {
                _subjectIndex = subject.Index;
                _targetIndex = target.Index;
                _execStateInfos[subject.Index] = new ();
                _execStateInfos[_targetIndex] = new ();
                _skillId = skillId;
            }
            foreach (var featureData in featureDates)
            {
                MakeFeature(subject,target,featureData,skillId,isOneTarget);
            }
            if (subject != null && target != null)
            {
                // 攻撃を受けたら外れるステートを解除
                var allStateInfos = target.StateInfos;
                foreach (var stateInfo in allStateInfos)
                {
                    if (stateInfo.Master.RemoveByAttack)
                    {                
                        _removedStates.Add(stateInfo);
                    }
                }

                if (_hpDamage >= (target.Hp + _hpHeal) && target.IsAlive())
                {
                    if (target.IsState(StateType.Undead) && featureDates.Find(a => a.FeatureType == FeatureType.BreakUndead) == null)
                    {
                        var undeadFeature = new SkillData.FeatureData
                        {
                            FeatureType = FeatureType.RemoveState,
                            Param1 = (int)StateType.Undead
                        };
                        MakeRemoveState(target,target,undeadFeature);
                        _overkillHpDamage = _hpDamage;
                        _hpDamage = target.Hp - 1;
                    } else
                    {
                        if (target.IsState(StateType.Reraise))
                        {
                            SeekStateCount(target,StateType.Reraise);
                            _overkillHpDamage = _hpDamage;
                            _hpDamage = target.Hp - 1;
                        } else
                        {
                            _deadIndexList.Add(target.Index);
                        }
                    }
                }
                int reduceHp = subject.MaxHp - subject.Hp;
                int recoveryHp = Mathf.Min(_reHeal,reduceHp);
                if ((_reDamage - recoveryHp) >= subject.Hp && subject.IsAlive())
                {
                    if (subject.IsState(StateType.Undead) && featureDates.Find(a => a.FeatureType == FeatureType.BreakUndead) == null)
                    {
                        var undeadFeature = new SkillData.FeatureData
                        {
                            FeatureType = FeatureType.RemoveState,
                            Param1 = (int)StateType.Undead
                        };
                        MakeRemoveState(subject,subject,undeadFeature);
                        _reDamage = subject.Hp - 1;
                    } else
                    {
                        if (target.IsState(StateType.Reraise))
                        {
                            SeekStateCount(target,StateType.Reraise);
                            _overkillHpDamage = _hpDamage;
                            _hpDamage = target.Hp - 1;
                        } else
                        {
                            _deadIndexList.Add(target.Index);
                        }
                    }
                }
                foreach (var removeState in _removedStates)
                {
                    if (removeState.StateType == StateType.Death)
                    {
                        _aliveIndexList.Add(removeState.TargetIndex);
                    }
                }
                // 呪詛判定
                for (int i = _deadIndexList.Count-1;i >= 0;i--)
                {
                    var deadIndex = _deadIndexList[i];
                    if (!_aliveIndexList.Contains(deadIndex))
                    {
                        if (target.Index == deadIndex && target.IsState(StateType.Curse))
                        {
                            _hpDamage = target.Hp - 1;
                            float curseDamage = target.DamagedValue + _hpDamage;
                            curseDamage *= target.GetStateEffectAll(StateType.Curse) * 0.01f;
                            _curseDamage += (int)curseDamage;
                            target.SetDamagedValue(0);
                            _deadIndexList.RemoveAt(i);
                        }
                    }
                }
            }
        }

        private int _hpDamage = 0;
        public int HpDamage => _hpDamage;
        private int _overkillHpDamage = 0;
        public int OverkillHpDamage => _overkillHpDamage;
        private int _hpHeal = 0;
        public int HpHeal => _hpHeal;
        public void SetHpHeal(int hpHeal)
        {
            _hpHeal = hpHeal;
        }
        private int _ctDamage = 0;
        public int CtDamage => _ctDamage;
        private int _ctHealSkillId = -1;
        public int CtHealSkillId => _ctHealSkillId;
        private int _ctHeal = 0;
        public int CtHeal => _ctHeal;
        private int _apDamage = 0;
        public int ApDamage => _apDamage;

        private int _apHeal = 0;
        public int ApHeal => _apHeal;
        private int _reDamage = 0;
        public int ReDamage => _reDamage;
        public void SetReDamage(int reDamage)
        {
            _reDamage = reDamage;
        }
        private int _curseDamage = 0;
        public int CurseDamage => _curseDamage;
        public void SetCurseDamage(int curseDamage)
        {
            _curseDamage = curseDamage;
        }
        private int _reHeal = 0;
        public int ReHeal => _reHeal;
        public void SetReHeal(int reHeal)
        {
            _reHeal = reHeal;
        }
        private List<int> _deadIndexList = new ();
        public List<int> DeadIndexList => _deadIndexList;
        private List<int> _aliveIndexList = new ();
        public List<int> AliveIndexList => _aliveIndexList;

        private bool _missed = false;
        public bool Missed => _missed;
        private bool _critical = false;
        public bool Critical => _critical;
        
        private List<StateInfo> _addedStates = new ();
        public List<StateInfo> AddedStates => _addedStates;
        private List<StateInfo> _removedStates = new ();
        public List<StateInfo> RemovedStates => _removedStates;
        private List<StateInfo> _displayStates = new ();
        public List<StateInfo> DisplayStates => _displayStates;
        private Dictionary<int,List<StateInfo>> _execStateInfos = new ();
        public  Dictionary<int,List<StateInfo>> ExecStateInfos => _execStateInfos;
        private bool _cursedDamage = false;
        public bool CursedDamage => _cursedDamage;
        public void SetCursedDamage(bool cursedDamage) {_cursedDamage = cursedDamage;}

        private int _turnCount;
        public int TurnCount => _turnCount;
        public void SetTurnCount(int turnCount) {_turnCount = turnCount;}

        private bool _startDash;
        public bool StartDash => _startDash;

        private void MakeFeature(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData,int skillId,bool isOneTarget = false)
        {
            var range = CalcRange(subject,target,skillId);
            switch (featureData.FeatureType)
            {
                case FeatureType.HpDamage:
                case FeatureType.HpConsumeDamage:
                    if (CheckIsHit(subject,target,isOneTarget,range))
                    {
                        MakeHpDamage(subject,target,featureData,false,isOneTarget,range);
                    }
                    return;
                case FeatureType.HpHeal:
                    MakeHpHeal(subject,target,featureData);
                    return;
                case FeatureType.HpDivide:
                    MakeHpDivide(subject,target,featureData);
                    return;
                case FeatureType.HpDrain:
                    if (CheckIsHit(subject,target,isOneTarget,range))
                    {
                        MakeHpDrain(subject,target,featureData,isOneTarget,range);
                    }
                    return;
                case FeatureType.HpSlipDamage:
                    MakeHpDefineDamage(subject,target,featureData,false);
                    return;
                case FeatureType.HpStateDamage:
                    if (CheckIsHit(subject,target,isOneTarget,range))
                    {
                        MakeHpStateDamage(subject,target,featureData,false,isOneTarget);
                    }
                    return;
                case FeatureType.HpCursedDamage:
                    MakeHpCursedDamage(subject,target,featureData,false,isOneTarget);
                    return;
                case FeatureType.NoEffectHpDamage:
                    MakeHpDamage(subject,target,featureData,true,isOneTarget,range);
                    return;
                case FeatureType.NoEffectHpPerDamage:
                    MakeHpPerDamage(subject,target,featureData,true,isOneTarget,range);
                    return;
                case FeatureType.NoEffectHpAddDamage:
                    MakeHpAddDamage(subject,target,featureData,true,isOneTarget,range);
                    return;
                case FeatureType.RevengeHpDamage:
                    if (CheckIsHit(subject,target,isOneTarget,range))
                    {
                        MakeRevengeHpDamage(subject,target,featureData,false,isOneTarget,range);
                    }
                    return;
                case FeatureType.PenetrateHpDamage:
                    if (CheckIsHit(subject,target,isOneTarget,range))
                    {
                        MakePenetrateHpDamage(subject,target,featureData,false,isOneTarget,range);
                    }
                    return;
                case FeatureType.HpParamHpDamage:
                    if (CheckIsHit(subject,target,isOneTarget,range))
                    {
                        MakeHpParamHpDamage(subject,target,featureData,false,isOneTarget,range);
                    }
                    return;
                case FeatureType.CtDamage:
                    if (CheckIsHit(subject,target,isOneTarget,range))
                    {
                        MakeCtDamage(subject,target,featureData);
                    }
                    return;
                case FeatureType.CtHeal:
                    MakeCtHeal(subject,target,featureData);
                    return;
                case FeatureType.CtDrain:
                    if (CheckIsHit(subject,target,isOneTarget,range))
                    {
                        MakeCtDrain(subject,target,featureData);
                    }
                    return;
                case FeatureType.ActiveCtHeal:
                    MakeActiveCtHeal(subject,target,featureData);
                    return;
                case FeatureType.AddState:
                    MakeAddState(subject,target,featureData,true);
                    return;
                case FeatureType.AddStateNextTurn:
                    MakeAddState(subject,target,featureData,true,false,true);
                    return;
                case FeatureType.RemoveState:
                    MakeRemoveState(subject,target,featureData);
                    return;
                case FeatureType.RemoveAbnormalState:
                    MakeRemoveAbnormalState(subject,target,featureData);
                    return;
                case FeatureType.RemoveBuffState:
                    MakeRemoveBuffState(subject,target);
                    return;
                case FeatureType.RemoveDeBuffState:
                    MakeRemoveDeBuffState(subject,target,featureData);
                    return;
                case FeatureType.RemoveStatePassive:
                    MakeRemoveStatePassive(subject,target,featureData);
                    return;
                case FeatureType.ChangeStateParam:
                    MakeChangeStateParam(subject,target,featureData);
                    return;
                case FeatureType.RemainHpOne:
                    MakeRemainHpOne(subject);
                    return;
                case FeatureType.RemainHpOneTarget:
                    MakeHpOne(target);
                    return;
                case FeatureType.ActionResultSetAp:
                    MakeActionResultSetAp(target,featureData);
                    return;
                case FeatureType.ApHeal:
                    MakeApHeal(subject,target,featureData);
                    return;
                case FeatureType.StartDash:
                    MakeStartDash(target);
                    return;
                case FeatureType.ApDamage:
                    MakeApDamage(subject,target,featureData);
                    return;
                case FeatureType.KindHeal:
                    MakeKindHeal(subject,target,featureData);
                    return;
                case FeatureType.BreakUndead:
                    MakeBreakUndead(subject,target,featureData);
                    return;
                case FeatureType.ChangeFeatureParam1:
                    MakeChangeFeatureParam(subject,target,featureData,1);
                    return;
                case FeatureType.ChangeFeatureParam2:
                    MakeChangeFeatureParam(subject,target,featureData,2);
                    return;
                case FeatureType.ChangeFeatureParam3:
                    MakeChangeFeatureParam(subject,target,featureData,3);
                    return;
                case FeatureType.ChangeFeatureParam1StageWinCount:
                    MakeAddFeatureParamStageWinCount(subject,target,featureData,1);
                    return;
                case FeatureType.ChangeFeatureParam2StageWinCount:
                    MakeAddFeatureParamStageWinCount(subject,target,featureData,2);
                    return;
                case FeatureType.ChangeFeatureParam3StageWinCount:
                    MakeAddFeatureParamStageWinCount(subject,target,featureData,3);
                    return;
                case FeatureType.ChangeMagicCountTurn:
                    MakeChangeMagicCountTurn(subject,target,featureData);
                    return;
                case FeatureType.ChangeFeatureRate:
                    MakeChangeFeatureRate(subject,target,featureData,1);
                    return;
                case FeatureType.AddSkillPlusSkill:
                    MakeAddSkillPlusSkill(subject,featureData);
                    return;
                case FeatureType.ReflectLastAbnormal:
                    MakeReflectLastAbnormal(subject,target,featureData);
                    return;
                case FeatureType.RobBuffState:
                    MakeRobBuffState(subject,target,featureData);
                    return;
            }
        }

        private bool CheckIsHit(BattlerInfo subject,BattlerInfo target,bool isOneTarget,int range)
        {
            var skillData = DataSystem.FindSkill(_skillId);
            if (skillData != null && (skillData.SkillType == SkillType.Messiah || skillData.SkillType == SkillType.Awaken))
            {
                return true;
            }
            if (skillData != null && skillData.IsAbsoluteHit())
            {
                return true;
            }
            if (!IsHit(subject,target,isOneTarget,range))
            {
                if (subject.IsState(StateType.AbsoluteHit))
                {
                    SeekStateCount(subject,StateType.AbsoluteHit);
                    return true;
                }
                _missed = true;
                return false;
            }
            return true;
        }

        private bool IsHit(BattlerInfo subject,BattlerInfo target,bool isOneTarget,int range)
        {
            /*
            if (target.IsState(StateType.Chain))
            {
                return true;
            }
            */
            if (subject.Index == target.Index)
            {
                return true;
            }
            /*
            if (isOneTarget && target.IsState(StateType.RevengeAct))
            {
                return true;
            }
            */
            int hit = 100;
            if (range > 0)
            {
                //hit -= range * 15;
            }
            // S⇒L Range.Sスキル = range=1で15%カット
            // L⇒L Range.Sスキル = range=2で30%カット
            // S⇒L Range.Lスキル = range=0
            // L⇒L Range.Lスキル = range=1で15%カット
            hit += subject.CurrentHit();
            hit -= target.CurrentEva();
            if (hit < 10)
            {
                hit = 10;
            }
            int rand = new System.Random().Next(0, 100);
            return hit >= rand;
        }

        private int CurrentAttack(BattlerInfo battlerInfo,bool isNoEffect)
        {
            int AtkValue = battlerInfo.CurrentAtk(isNoEffect);
            return AtkValue;
        }

        private int CurrentDefense(BattlerInfo subject, BattlerInfo target,bool isNoEffect)
        {
            int DefValue = target.CurrentDef(isNoEffect);
            if (isNoEffect == false)
            {
                if (subject.IsState(StateType.Penetrate))
                {
                    var Penetrate = 100 - subject.StateEffectAll(StateType.Penetrate);
                    DefValue = (int)(DefValue * Penetrate * 0.01f);
                }
            }
            return DefValue;
        }

        private float CurrentDamageRate(BattlerInfo battlerInfo,bool isNoEffect,bool isOneTarget)
        {
            return battlerInfo.CurrentDamageRate(isNoEffect);
        }

        private float CalcDamageValue(BattlerInfo subject,BattlerInfo target,float SkillDamage,bool isNoEffect)
        {
            float DamageValue = Mathf.Max(1,SkillDamage);
            DamageValue = CalcHolyCoffin(subject,target,DamageValue);
            DamageValue *= 1f - CalcDamageCutRate(subject,target,isNoEffect);
            DamageValue -= CalcDamageCut(subject,target,isNoEffect);
            return DamageValue;
        }

        private float CalcDamageCutRate(BattlerInfo subject,BattlerInfo target,bool isNoEffect)
        {
            float damageCutRate = 0;
            if (isNoEffect == false)
            {
                if (target.IsState(StateType.DamageCutRate))
                {
                    damageCutRate += target.StateEffectAll(StateType.DamageCutRate) * 0.01f;
                    SeekStateCount(target,StateType.DamageCutRate);
                }
                var substituteStateInfos = subject.GetStateInfoAll(StateType.Substitute);
                if (substituteStateInfos.Count > 0)
                {
                    if (substituteStateInfos.Find(a => a.BattlerId == target.Index) != null)
                    {
                        // 挑発でダメージカット50%
                        damageCutRate += subject.GetStateEffectAll(StateType.Substitute) * 0.01f;
                    }
                }
            }
            return damageCutRate;
        }

        private int CalcDamageCut(BattlerInfo subject,BattlerInfo target,bool isNoEffect)
        {
            int damageCut = 0;
            if (isNoEffect == false)
            {
                if (target.IsState(StateType.DamageCut))
                {
                    damageCut += target.StateEffectAll(StateType.DamageCut);
                    SeekStateCount(target,StateType.DamageCut);
                }
            }
            return damageCut;
        }

        private int CalcRange(BattlerInfo subject,BattlerInfo target,int skillId)
        {
            var range = 0;
            var skillData = DataSystem.FindSkill(skillId);
            if (skillData == null)
            {
                return range;
            }
            if (skillData.Range == RangeType.S)
            {
                // S⇒L Range.Sスキル = range=1で15%カット
                // L⇒L Range.Sスキル = range=2で30%カット
                if (subject.LineIndex == LineType.Front && target.LineIndex == LineType.Back)
                {
                    range = 1;
                } else
                if (subject.LineIndex == LineType.Back && target.LineIndex == LineType.Back)
                {
                    range = 2;
                } else
                if (subject.LineIndex == LineType.Back && target.LineIndex == LineType.Front)
                {
                    range = 1;
                }
            } else
            if (skillData.Range == RangeType.L)
            {
                // S⇒L Range.Lスキル = range=0
                // L⇒L Range.Lスキル = range=1で15%カット
                if (subject.LineIndex == LineType.Back && target.LineIndex == LineType.Back)
                {
                    range = 1;
                }
            }
            return range;
        }

        private float CalcHpDamage(float atkValue,BattlerInfo subject,BattlerInfo target,float featureValue,bool isNoEffect,bool isOneTarget)
        {
            float DamageValue = CalcAttackDamageValue(atkValue,subject,featureValue,isNoEffect,isOneTarget);
            CalcFreezeDamage(subject,DamageValue);

            // 攻撃ダメージ - 防御値
            int DefValue = CurrentDefense(subject,target,isNoEffect);
            DamageValue *= GetDefenseRateValue(atkValue,DefValue);
            float hpDamage = CalcDamageValue(subject,target,DamageValue,isNoEffect);

            // 効果補正
            return CalcDamageEffect(hpDamage,subject,target,isNoEffect);
        }

        private float CalcAttackDamageValue(float atkValue,BattlerInfo subject,float featureValue,bool isNoEffect,bool isOneTarget)
        {
            float DamageRate = featureValue * CurrentDamageRate(subject,isNoEffect,isOneTarget);
            return DamageRate * atkValue * 0.01f;
        }
        

        private void MakeHpDamage(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData,bool isNoEffect,bool isOneTarget,int range)
        {
            // 攻撃ダメージ
            float AtkValue = CurrentAttack(subject,isNoEffect) * 0.5f;
            var hpDamage = CalcHpDamage(AtkValue,subject,target,featureData.Param1,isNoEffect,isOneTarget);
            _hpDamage += (int)hpDamage;
        }

        private void MakeHpPerDamage(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData,bool isNoEffect,bool isOneTarget,int range)
        {
            var hpDamage = target.MaxHp * 0.01f * featureData.Param1;
            hpDamage = CalcDamageShield(subject,target,hpDamage);
            _hpDamage += (int)hpDamage;
        }

        private void MakeHpAddDamage(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData,bool isNoEffect,bool isOneTarget,int range)
        {
            var hpDamage = _hpDamage * 0.01f * featureData.Param1;
            hpDamage = CalcDamageShield(subject,target,hpDamage);
            _hpDamage += (int)hpDamage;
            // 追加ダメージで戦闘不能にならない
            if (_hpDamage > target.Hp)
            {
                _hpDamage = target.Hp - 1;
            }
        }

        private void MakeRevengeHpDamage(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData,bool isNoEffect,bool isOneTarget,int range)
        {
            // 攻撃ダメージ
            float AtkValue = subject.DamagedValue;
            var hpDamage = CalcHpDamage(AtkValue,subject,target,featureData.Param1,isNoEffect,isOneTarget);
            _hpDamage += (int)hpDamage; 
        }

        private void MakePenetrateHpDamage(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData,bool isNoEffect,bool isOneTarget,int range)
        {
            float atkValue = CurrentAttack(subject,isNoEffect);
            float DamageValue = CalcAttackDamageValue(atkValue,subject,featureData.Param1,isNoEffect,isOneTarget);
            CalcFreezeDamage(subject,DamageValue);

            // 攻撃ダメージ - 防御値
            int DefValue = CurrentDefense(subject,target,isNoEffect);
            // 無視分を反映
            DefValue = (int)(DefValue * (1f - featureData.Param3 * 0.01f));
            DamageValue *= GetDefenseRateValue(atkValue,DefValue);
            float hpDamage = CalcDamageValue(subject,target,DamageValue,isNoEffect);

            // 効果補正
            hpDamage = CalcDamageEffect(hpDamage,subject,target,isNoEffect);
            _hpDamage += (int)hpDamage; 
        }

        private void MakeHpParamHpDamage(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData,bool isNoEffect,bool isOneTarget,int range)
        {
            int atkValue = CurrentAttack(subject,isNoEffect);
    
            float DamageRate = featureData.Param1 * CurrentDamageRate(subject,isNoEffect,isOneTarget);
            float HpRate = 1 - subject.HpRate * featureData.Param3;
            DamageRate += HpRate;
            float DamageValue = DamageRate * atkValue * 0.01f;
            CalcFreezeDamage(subject,DamageValue);

            // 攻撃ダメージ - 防御値
            int DefValue = CurrentDefense(subject,target,isNoEffect);
            DamageValue *= GetDefenseRateValue(atkValue,DefValue);
            float hpDamage = CalcDamageValue(subject,target,DamageValue,isNoEffect);

            // 効果補正
            hpDamage = CalcDamageEffect(hpDamage,subject,target,isNoEffect);
            _hpDamage += (int)hpDamage;
        }

        private float CalcDamageEffect(float hpDamage,BattlerInfo subject,BattlerInfo target,bool isNoEffect)
        {
            // クリティカル
            if (IsCritical(subject,target))
            {
                hpDamage = ApplyCritical(hpDamage,subject);
            }
            hpDamage = ApplyVariance(hpDamage);
            hpDamage -= CalcDamageShell(target);
            hpDamage = CalcAddDamage(subject,target,hpDamage);
            CalcAddState(subject,target);
            hpDamage = Mathf.Max(1,hpDamage);
            if (IsNoDamage(target,isNoEffect))
            {
                hpDamage = 0;
            }
            if (IsDeadlyDamage(subject,target,isNoEffect))
            {
                // 対象がボスの場合は残りHpの50%ダメージ
                if (target.Kinds.Contains(KindType.Boss))
                {
                    hpDamage = Math.Max(target.Hp / 2,hpDamage);
                } else
                {
                    hpDamage = target.Hp;
                }
            }
            if (!isNoEffect)
            {
                CalcCounterDamage(subject,target,hpDamage);
            }
            _reHeal += CalcDrainValue(subject,hpDamage);
            return CalcDamageShield(subject,target,hpDamage);
        }

        private void MakeHpHeal(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData)
        {
            float HealValue = featureData.Param1;
            // param3が1の時は割合
            var healValue = 0;
            if ((HpHealType)featureData.Param3 == HpHealType.RateValue)
            {
                healValue += (int)Mathf.Round(target.MaxHp * HealValue * 0.01f);
            } else
            {
                healValue += (int)Mathf.Round(HealValue);
            }
            if (target.IsState(StateType.HealValueUp))
            {
                healValue += target.GetStateEffectAll(StateType.HealValueUp);
            }
            if (target.IsState(StateType.NotHeal))
            {
                _displayStates.Add(target.GetStateInfo(StateType.NotHeal));
                healValue = 0;
            }
            _hpHeal += healValue;
            if (subject != target)
            {
                if (subject.IsState(StateType.HealActionSelfHeal))
                {
                    _reHeal += healValue;
                }
            }
        }

        private void MakeHpDivide(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData)
        {
            float HealValue = featureData.Param1 * 0.01f * subject.MaxHp;
            HealValue = Math.Min(subject.MaxHp,HealValue);
            // param3が1の時は割合
            var healValue = (int)Mathf.Round(HealValue);
            if (target.IsState(StateType.HealValueUp))
            {
                healValue += target.GetStateEffectAll(StateType.HealValueUp);
            }
            if (target.IsState(StateType.NotHeal))
            {
                _displayStates.Add(target.GetStateInfo(StateType.NotHeal));
                healValue = 0;
            }
            _hpHeal += healValue;
            _reDamage += healValue;
            if (subject != target)
            {
                if (subject.IsState(StateType.HealActionSelfHeal))
                {
                    _reHeal += healValue;
                }
            }
        }

        private void MakeHpDrain(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData,bool isOneTarget,int range)
        {
            MakeHpDamage(subject,target,featureData,false,isOneTarget,range);
            _reHeal = (int)Mathf.Floor(HpDamage * featureData.Param3 * 0.01f);
        }

        // スリップダメージ計算
        private void MakeHpDefineDamage(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData,bool isNoEffect)
        {
            var hpDamage = featureData.Param1;
            hpDamage = Mathf.Max(1,hpDamage);
            _hpDamage += hpDamage;
        }

        private void MakeHpStateDamage(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData,bool isNoEffect,bool isOneTarget)
        {
            float atkValue = CurrentAttack(subject,isNoEffect) * 0.5f;
            float DamageRate = featureData.Param2;
            if (target.IsState((StateType)featureData.Param3))
            {
                DamageRate = featureData.Param1;
            }
            var hpDamage = CalcHpDamage(atkValue,subject,target,DamageRate,isNoEffect,isOneTarget);
            _hpDamage += (int)hpDamage;
        }

        // 呪いダメージ計算
        private void MakeHpCursedDamage(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData,bool isNoEffect,bool isOneTarget)
        {
            float atkValue = featureData.Param1 * 0.5f;
            var hpDamage = CalcHpDamage(atkValue,subject,target,100,isNoEffect,isOneTarget);
            _hpDamage += (int)hpDamage;
        }

        private void MakeCtDamage(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData)
        {
            _ctDamage = featureData.Param1;
        }

        private void MakeCtDrain(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData)
        {
            var mpDamage = Math.Min(featureData.Param1,target.Mp);
            _ctDamage = mpDamage;
            _ctHeal = mpDamage;
        }

        private void MakeCtHeal(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData)
        {
            float HealValue = featureData.Param1;
            _ctHeal = (int)Mathf.Round(HealValue);
        }

        private void MakeActiveCtHeal(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData)
        {
            _ctHealSkillId = subject.LastSelectSkillId;
            float HealValue = featureData.Param1;
            _ctHeal = (int)Mathf.Round(HealValue);
        }

        private void MakeAddState(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData,bool checkCounter = false,bool isOneTarget = false,bool removeTimingIsNextTurn = false,int range = 0)
        {
            if (featureData.Rate < UnityEngine.Random.Range(0,100))
            {
                //_missed = true;
                return;
            }
            var stateInfo = new StateInfo((StateType)featureData.Param1,featureData.Param2,featureData.Param3,subject.Index,target.Index,_skillId);
            if (removeTimingIsNextTurn)
            {
                stateInfo.SetRemoveTiming(RemovalTiming.NextSelfTurn);
            }
            if (stateInfo.Master.CheckHit)
            {
                if (!CheckIsHit(subject,target,isOneTarget,range))
                {
                    return;
                }
            }
            if (stateInfo.Master.StateType == StateType.Death)
            {
                _hpDamage = target.Hp;
            }
            bool IsAdded = target.AddState(stateInfo,false);
            if (IsAdded)
            {
                if (stateInfo.Master.StateType == StateType.RemoveBuff)
                {
                    var removeStates = target.GetRemovalBuffStates();
                    foreach (var removeState in removeStates)
                    {
                        var removeFeature = new SkillData.FeatureData
                        {
                            FeatureType = FeatureType.RemoveState,
                            Param1 = (int)removeState.Master.StateType
                        };
                        MakeRemoveState(subject,target,removeFeature);
                    }
                } else
                {
                    _addedStates.Add(stateInfo);
                }
            } else
            {
                if (target.IsState(StateType.Barrier))
                {
                    if (stateInfo.Master.Abnormal)
                    {
                        SeekStateCount(target,StateType.Barrier);
                    }
                }
            }
            if (checkCounter == true && stateInfo.Master.Abnormal && target.IsState(StateType.AntiDote))
            {
                _execStateInfos[target.Index].Add(target.GetStateInfo(StateType.AntiDote));
                if (subject.IsState(StateType.NoDamage))
                {
                    SeekStateCount(subject,StateType.NoDamage);
                } else
                {
                    _reDamage += AntiDoteDamageValue(target);
                }
                var counterAddState = new SkillData.FeatureData
                {
                    FeatureType = FeatureType.AddState,
                    Param1 = featureData.Param1,
                    Param2 = featureData.Param2,
                    Param3 = featureData.Param3,
                    Rate = 100
                };
                MakeAddState(target,subject,counterAddState,false);
            }
        }
        
        private void MakeRemoveState(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData)
        {
            // skillId -1のRemoveは強制で解除する
            var stateInfo = new StateInfo((StateType)featureData.Param1,featureData.Param2,featureData.Param3,subject.Index,target.Index,-1);
            bool IsRemoved = target.RemoveState(stateInfo,false);
            if (IsRemoved)
            {
                _removedStates.Add(stateInfo);
            }
        }

        private void MakeRemoveAbnormalState(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData)
        {
            // skillId -1のRemoveは強制で解除する
            var abnormalStates = target.StateInfos.FindAll(a => a.Master.Abnormal == true && a.BattlerId != target.Index);
            foreach (var abnormalState in abnormalStates)
            {
                bool IsRemoved = target.RemoveState(abnormalState,false);
                if (IsRemoved)
                {
                    _removedStates.Add(abnormalState);
                }
            }
        }

        private void MakeRemoveBuffState(BattlerInfo subject,BattlerInfo target)
        {
            // skillId -1のRemoveは強制で解除する
            var abnormalStates = target.StateInfos.FindAll(a => a.Master.Buff == true && a.BattlerId != target.Index);
            foreach (var abnormalState in abnormalStates)
            {
                bool IsRemoved = target.RemoveState(abnormalState,false);
                if (IsRemoved)
                {
                    _removedStates.Add(abnormalState);
                }
            }
        }

        private void MakeRemoveDeBuffState(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData)
        {
            // skillId -1のRemoveは強制で解除する
            var abnormalStates = target.StateInfos.FindAll(a => a.Master.DeBuff == true && a.BattlerId != target.Index);
            foreach (var abnormalState in abnormalStates)
            {
                bool IsRemoved = target.RemoveState(abnormalState,false);
                if (IsRemoved)
                {
                    _removedStates.Add(abnormalState);
                }
            }
        }

        private void MakeRemoveStatePassive(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData)
        {
            // パッシブはそのパッシブスキルのみ解除する
            var stateInfo = new StateInfo((StateType)featureData.Param1,featureData.Param2,featureData.Param3,subject.Index,target.Index,_skillId);
            bool IsRemoved = target.RemoveState(stateInfo,false);
            if (IsRemoved)
            {
                _removedStates.Add(stateInfo);
            }
        }

        private void MakeChangeStateParam(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData)
        {
            // ステートのparam2とparam3を上書き
            var stateInfos = target.GetStateInfoAll((StateType)featureData.Param1);
            foreach (var stateInfo in stateInfos)
            {
                if (featureData.Param2 > stateInfo.Turns)
                {
                    stateInfo.SetTurn(featureData.Param2);
                }
                if (featureData.Param3 > stateInfo.Effect)
                {
                    stateInfo.SetEffect(featureData.Param3);
                }
            }
            if (stateInfos.Count > 0)
            {
                _displayStates.Add(stateInfos[0]);
            }
        }

        private void MakeRemainHpOne(BattlerInfo subject)
        {
            _reDamage = subject.Hp - 1;
        }

        private void MakeHpOne(BattlerInfo battlerInfo)
        {
            battlerInfo.SetHp(1);
        }

        private void MakeActionResultSetAp(BattlerInfo target,SkillData.FeatureData featureData)
        {
            int apValue = featureData.Param1;
            target.SetAp(apValue);
        }

        private void MakeApHeal(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData)
        {
            float HealValue = featureData.Param1;
            _apHeal = (int)Mathf.Round(HealValue);
        }

        private void MakeApDamage(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData)
        {
            float HealValue = featureData.Param1;
            _apDamage = (int)Mathf.Round(HealValue);
        }

        private void MakeStartDash(BattlerInfo target)
        {
            target.SetAp(0);
            _startDash = true;
        }

        public void MakeKindHeal(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData)
        {
            if (target.IsState(StateType.Undead) || (target.EnemyData != null && target.EnemyData.Kinds.Contains(KindType.Undead)))
            {
                _hpDamage = (int)Mathf.Floor( _hpHeal * featureData.Param3 * 0.01f);
                _hpHeal = 0;
            }
        }

        public void MakeBreakUndead(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData)
        {
            if (target.Kinds.IndexOf((KindType)featureData.Param1) != -1)
            {
                _hpDamage = (int)Mathf.Floor( _hpDamage * featureData.Param3 * 0.01f);
                _hpHeal = 0;
            }
        }

        public void MakeChangeFeatureParam(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData,int featureParamIndex)
        {
            // 即代入
            var skillInfo = subject.Skills.Find(a => a.Id == featureData.Param1);
            if (skillInfo != null)
            {
                // featureのIndex
                var feature = skillInfo.FeatureDates.Count >= featureData.Param2 ? skillInfo.FeatureDates[featureData.Param2] : null;
                if (feature != null)
                {
                    switch (featureParamIndex)
                    {
                        case 1:
                            feature.Param1 = featureData.Param3;
                            break;
                        case 2:
                            feature.Param2 = featureData.Param3;
                            break;
                        case 3:
                            feature.Param3 = featureData.Param3;
                            break;
                    }
                }
            }
        }

        public void MakeAddFeatureParamStageWinCount(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData,int featureParamIndex)
        {
            // 即代入
            var skillInfo = subject.Skills.Find(a => a.Id == featureData.Param1);
            if (skillInfo != null)
            {
                // featureのIndex
                var feature = skillInfo.FeatureDates.Count >= featureData.Param2 ? skillInfo.FeatureDates[featureData.Param2] : null;
                var winCount = GameSystem.CurrentStageData.CurrentStage.ClearTroopIds.Count;
                if (feature != null)
                {
                    switch (featureParamIndex)
                    {
                        case 1:
                            feature.Param1 += winCount * featureData.Param3;
                            break;
                        case 2:
                            feature.Param2 += winCount * featureData.Param3;
                            break;
                        case 3:
                            feature.Param3 += winCount * featureData.Param3;
                            break;
                    }
                }
            }
        }

        public void MakeChangeMagicCountTurn(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData)
        {
            // 即代入
            var skillInfo = subject.Skills.Find(a => a.Id == featureData.Param1);
            if (skillInfo != null)
            {
                skillInfo.SetCountTurn(featureData.Param3);
            }
        }

        public void MakeChangeFeatureRate(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData,int featureParamIndex)
        {
            // 即代入
            var skillInfo = subject.Skills.Find(a => a.Id == featureData.Param1);
            if (skillInfo != null)
            {
                // featureのIndex
                var feature = skillInfo.FeatureDates.Count >= featureData.Param2 ? skillInfo.FeatureDates[featureData.Param2] : null;
                if (feature != null)
                {
                    feature.Rate = featureData.Param3;
                }
            }
        }

        private void MakeAddSkillPlusSkill(BattlerInfo subject,SkillData.FeatureData featureData)
        {
            // 即代入
            var skillInfo = subject.Skills.Find(a => a.Id == featureData.Param1);
            if (skillInfo != null)
            {
                var plusSkill = DataSystem.FindSkill(featureData.Param3);
                // plusSkillのfeatureを追加する
                foreach (var feature in plusSkill.FeatureDates)
                {
                    if (skillInfo.FeatureDates.Find(a => a.SkillId == feature.SkillId) == null)
                    {
                        skillInfo.FeatureDates.Add(feature);
                    }
                }
            }
        }

        private void MakeReflectLastAbnormal(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData)
        {
            var lastAbnormalInfos = subject.StateInfos.FindAll(a => a.Master.Abnormal);
            if (lastAbnormalInfos.Count > 0)
            {
                var lastAbnormal = lastAbnormalInfos[lastAbnormalInfos.Count - 1];
                var abnormalFeature = new SkillData.FeatureData
                {
                    FeatureType = FeatureType.AddState,
                    Param1 = (int)lastAbnormal.Master.StateType,
                    Param2 = lastAbnormal.Turns,
                    Param3 = lastAbnormal.Effect,
                    Rate = 100
                };
                MakeAddState(subject,target,abnormalFeature);
            }
        }

        private void MakeRobBuffState(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData)
        {
            var buffStates = target.StateInfos.FindAll(a => a.Master.Buff);
            if (buffStates.Count > 0)
            {
                foreach (var buffState in buffStates)
                {
                    var buffFeature = new SkillData.FeatureData
                    {
                        FeatureType = FeatureType.AddState,
                        Param1 = (int)buffState.Master.StateType,
                        Param2 = buffState.Turns,
                        Param3 = buffState.Effect,
                        Rate = 100
                    };
                    MakeAddState(subject,subject,buffFeature);
                }
                MakeRemoveBuffState(subject,target);
            }
        }

        public void AddRemoveState(StateInfo stateInfo)
        {
            if (_removedStates.IndexOf(stateInfo) == -1)
            {
                _removedStates.Add(stateInfo);
            }
        }
        
        private float GetDefenseRateValue(float atk,float def){
            // 防御率 ＝ 1 - 防御 / (攻撃 + 防御)　※攻撃 + 防御 < 1の時、1
            float _defenseRateValue;
            if ((atk + def) < 1)
            {
                _defenseRateValue = 1;
            } else
            {
                _defenseRateValue = 1 - (def / (atk + def));
            }
            return _defenseRateValue;
        }

        private void CalcFreezeDamage(BattlerInfo subject,float skillDamage)
        {
            if (subject.IsState(StateType.Freeze))
            {
                _execStateInfos[subject.Index].Add(subject.GetStateInfo(StateType.Freeze));
                if (subject.IsState(StateType.NoDamage))
                {
                    SeekStateCount(subject,StateType.NoDamage);
                } else
                {
                    _reDamage += FreezeDamageValue(subject,skillDamage);
                }
            }
        }

        private int FreezeDamageValue(BattlerInfo subject,float skillDamage)
        {
            int ReDamage = (int)Mathf.Floor(skillDamage * subject.StateEffectAll(StateType.Freeze) * 0.01f);
            return ReDamage;
        }

        private int CalcDrainValue(BattlerInfo subject,float hpDamage)
        {
            if (subject.IsState(StateType.Drain) && !subject.IsState(StateType.NotHeal))
            {
                return (int)Mathf.Floor(hpDamage * subject.StateEffectAll(StateType.Drain) * 0.01f);
            }
            return 0;
        }

        private bool IsDeadlyDamage(BattlerInfo subject,BattlerInfo target,bool isNoEffect)
        {
            if (subject.IsState(StateType.Deadly) && !isNoEffect)
            {
                if (!target.IsState(StateType.Barrier))
                {
                    int rand = new System.Random().Next(0, 100);
                    if (subject.StateEffectAll(StateType.Deadly) >= rand)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsNoDamage(BattlerInfo target,bool isNoEffect)
        {
            if (target.IsState(StateType.NoDamage) && !isNoEffect)
            {
                SeekStateCount(target,StateType.NoDamage);
                return true;
            }
            return false;
        }

        private int CalcDamageShell(BattlerInfo target)
        {
            if (target.IsState(StateType.CounterAura) && target.IsState(StateType.CounterAuraShell))
            {
                return target.StateEffectAll(StateType.CounterAuraShell);
            }
            return 0;
        }

        private void CalcCounterDamage(BattlerInfo subject,BattlerInfo target,float hpDamage)
        {
            if (target.IsState(StateType.CounterAura))
            {
                _execStateInfos[target.Index].Add(target.GetStateInfo(StateType.CounterAura));
                if (subject.IsState(StateType.NoDamage))
                {
                    SeekStateCount(subject,StateType.NoDamage);
                } else
                {                
                    _reDamage += CounterDamageValue(target,hpDamage);
                }
            }
        }

        private int CounterDamageValue(BattlerInfo target,float hpDamage)
        {
            int ReDamage = (int)Mathf.Floor(hpDamage * target.StateEffectAll(StateType.CounterAura) * 0.01f);
            ReDamage += target.StateEffectAll(StateType.CounterAuraDamage);
            return Math.Max(1,ReDamage);
        }

        private bool IsCritical(BattlerInfo subject,BattlerInfo target)
        {
            int HitOver = subject.CurrentHit() - target.CurrentEva();
            if (HitOver < 0){
                HitOver = 0;
            }
            int CriticalRate = subject.StateEffectAll(StateType.CriticalRateUp) + HitOver;
            int rand = new System.Random().Next(0, 100);
            _critical = CriticalRate > rand;
            return _critical;
        }

        private float CriticalDamageRate(BattlerInfo subject)
        {
            return subject.StateEffectAll(StateType.CriticalDamageRateUp) * 0.01f;
        }

        private int AntiDoteDamageValue(BattlerInfo target)
        {
            int ReDamage = (int)Mathf.Floor(target.CurrentDef(false) * 0.5f);
            return ReDamage;
        }

        private float CalcHolyCoffin(BattlerInfo subject,BattlerInfo target,float hpDamage)
        {
            if (target.IsState(StateType.HolyCoffin))
            {
                hpDamage *= 1 + target.StateEffectAll(StateType.HolyCoffin) * 0.01f;
            }
            return hpDamage;
        }

        private float CalcAddDamage(BattlerInfo subject,BattlerInfo target,float hpDamage)
        {
            var addDamage = 0f;
            if (subject.IsState(StateType.MpCostZeroAddDamage))
            {
                if (DataSystem.FindSkill(_skillId).CountTurn == 0)
                {
                    addDamage = hpDamage * 0.01f * subject.StateEffectAll(StateType.MpCostZeroAddDamage);
                }
                if (hpDamage < target.Hp)
                {
                    if ((addDamage + hpDamage) >= target.Hp)
                    {
                        return target.Hp - 1;
                    }
                }
            } else
            {
                return hpDamage;
            }
            return addDamage + hpDamage;
        }

        private void CalcAddState(BattlerInfo subject,BattlerInfo target)
        {
            if (subject.IsState(StateType.MpCostZeroAddState) && _skillId > 0)
            {
                if (DataSystem.FindSkill(_skillId).CountTurn == 0)
                {
                    var stateInfos = subject.GetStateInfoAll(StateType.MpCostZeroAddState);
                    foreach (var stateInfo in stateInfos)
                    {
                        var skillData = DataSystem.FindSkill(stateInfo.Effect);
                        if (skillData != null)
                        {
                            foreach (var featureData in skillData.FeatureDates)
                            {
                                MakeAddState(subject,target,featureData);
                            }
                        }
                    }
                }
            }
        }

        private float CalcDamageShield(BattlerInfo subject,BattlerInfo target,float hpDamage)
        {
            var shield = target.StateEffectAll(StateType.DamageShield);
            if (shield > hpDamage)
            {
                hpDamage = 0;
                _displayStates.Add(target.GetStateInfo(StateType.DamageShield));
            }
            return hpDamage;
        }

        private int ApplyCritical(float value,BattlerInfo subject)
        {
            var criticalDamageRate = 1.5f + CriticalDamageRate(subject);
            return Mathf.FloorToInt( value * criticalDamageRate );
        }

        private float ApplyVariance(float value)
        {
            int rand = new System.Random().Next(-5, 5);
            return (value * (1 + rand * 0.01f));
        }

        private void SeekStateCount(BattlerInfo battlerInfo,StateType stateType)
        {
            var seekState = battlerInfo.GetStateInfo(stateType);
            if (seekState.RemovalTiming == RemovalTiming.UpdateCount)
            {
                if (!_execStateInfos[battlerInfo.Index].Contains(seekState))
                {
                    _execStateInfos[battlerInfo.Index].Add(seekState);
                    int count = seekState.Turns;
                    if ((count-1) <= 0)
                    {
                        _removedStates.Add(battlerInfo.GetStateInfo(stateType));
                    } else{
                        _displayStates.Add(battlerInfo.GetStateInfo(stateType));
                    }
                }
            }
        }

        public int SeekCount(BattlerInfo target,StateType stateType)
        {
            int removeCount = RemovedStates.FindAll(a => a.Master.StateType == stateType && a.TargetIndex == target.Index).Count;
            int displayCount = DisplayStates.FindAll(a => a.Master.StateType == stateType && a.TargetIndex == target.Index).Count;
            return removeCount + displayCount;
        }

        // 拘束と祝福を解除できるか
        public bool RemoveAttackStateDamage()
        {
            return HpDamage > 0 
                    || AddedStates.Find(a => a.Master.StateType == StateType.Stun) != null
                    //|| AddedStates.Find(a => a.Master.StateType == StateType.Chain) != null
                    || AddedStates.Find(a => a.Master.StateType == StateType.Death) != null
                    || DeadIndexList.Contains(TargetIndex);
        }

        public static List<int> ConvertIndexes(List<ActionResultInfo> actionResultInfos)
        {
            var targetIndexes = new List<int>();
            foreach (var actionResultInfo in actionResultInfos)
            {
                var targetIndex = actionResultInfo.TargetIndex;
                targetIndexes.Add(targetIndex);
            }
            return targetIndexes;
        }
    }
}