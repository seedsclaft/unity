using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ActionResultInfo 
{
    private int _subjectIndex = 0;
    public int SubjectIndex => _subjectIndex;
    private int _targetIndex = 0;
    public int TargetIndex => _targetIndex;

    private int _skillId = -1;
    public ActionResultInfo(BattlerInfo subject,BattlerInfo target,List<SkillData.FeatureData> featureDates,int skillId,bool isOneTarget = false)
    {
        if (subject != null && target != null)
        {
            _subjectIndex = subject.Index;
            _targetIndex = target.Index;
            _execStateInfos[subject.Index] = new ();
            _execStateInfos[_targetIndex] = new ();
            _skillId = skillId;
        }
        for (int i = 0; i < featureDates.Count; i++)
        {
            MakeFeature(subject,target,featureDates[i],skillId,isOneTarget);
        }
        if (subject != null && target != null)
        {
            if (_hpDamage >= (target.Hp + _hpHeal) && target.IsAlive())
            {
                if (target.IsState(StateType.Undead) && featureDates.Find(a => a.FeatureType == FeatureType.BreakUndead) == null)
                {
                    var undeadFeature = new SkillData.FeatureData();
                    undeadFeature.FeatureType = FeatureType.RemoveState;
                    undeadFeature.Param1 = (int)StateType.Undead;
                    MakeRemoveState(target,target,undeadFeature);
                    _overkillHpDamage = _hpDamage;
                    _hpDamage = target.Hp - 1;
                } else
                {
                    _deadIndexList.Add(target.Index);
                }
            }
            int reduceHp = subject.MaxHp - subject.Hp;
            int recoveryHp = Mathf.Min(_reHeal,reduceHp);
            if ((_reDamage - recoveryHp) >= subject.Hp && subject.IsAlive())
            {
                if (subject.IsState(StateType.Undead) && featureDates.Find(a => a.FeatureType == FeatureType.BreakUndead) == null)
                {
                    var undeadFeature = new SkillData.FeatureData();
                    undeadFeature.FeatureType = FeatureType.RemoveState;
                    undeadFeature.Param1 = (int)StateType.Undead;
                    MakeRemoveState(subject,subject,undeadFeature);
                    _reDamage = subject.Hp - 1;
                } else
                {
                    _deadIndexList.Add(subject.Index);
                }
            }
            foreach (var removeState in _removedStates)
            {
                if (removeState.StateType == StateType.Death)
                {
                    _aliveIndexList.Add(removeState.TargetIndex);
                }
            }
        }
    }

    private int _hpDamage = 0;
    public int HpDamage => _hpDamage;
    public void SetHpDamage(int hpDamage)
    {
        _hpDamage = hpDamage;
    }
    private int _overkillHpDamage = 0;
    public int OverkillHpDamage => _overkillHpDamage;
    private int _hpHeal = 0;
    public int HpHeal {
        get { return _hpHeal;} set{_hpHeal = value;}
    }
    private int _mpDamage = 0;
    public int MpDamage => _mpDamage;
    private int _mpHeal = 0;
    public int MpHeal {
        get {return _mpHeal;} set{_mpHeal = value;}
    }
    private int _apDamage = 0;
    public int ApDamage => _apDamage;

    private int _apHeal = 0;
    public int ApHeal {
        get {return _apHeal;} set{_apHeal = value;}
    }
    private int _reDamage = 0;
    public int ReDamage => _reDamage;
    public void SetReDamage(int reDamage)
    {
        _reDamage = reDamage;
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
                if (CheckIsHit(subject,target,isOneTarget,range))
                {
                    MakeHpDamage(subject,target,featureData,false,isOneTarget,range);
                }
                return;
            case FeatureType.HpHeal:
                MakeHpHeal(subject,target,featureData);
                return;
            case FeatureType.HpDrain:
                if (CheckIsHit(subject,target,isOneTarget,range))
                {
                    MakeHpDrain(subject,target,featureData,isOneTarget,range);
                }
                return;
            case FeatureType.HpDefineDamage:
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
            case FeatureType.MpDamage:
                if (CheckIsHit(subject,target,isOneTarget,range))
                {
                    MakeMpDamage(subject,target,featureData);
                }
                return;
            case FeatureType.MpHeal:
                MakeMpHeal(subject,target,featureData);
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
            case FeatureType.RemoveStatePassive:
                MakeRemoveStatePassive(subject,target,featureData);
                return;
            case FeatureType.RemainHpOne:
                MakeRemainHpOne(subject);
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
        }
    }

    private bool CheckIsHit(BattlerInfo subject,BattlerInfo target,bool isOneTarget,int range)
    {
        var skillData = DataSystem.FindSkill(_skillId);
        if (skillData != null && (skillData.SkillType == SkillType.Demigod || skillData.SkillType == SkillType.Awaken))
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
        if (target.IsState(StateType.Chain))
        {
            return true;
        }
        if (subject.Index == target.Index)
        {
            return true;
        }
        if (isOneTarget && target.IsState(StateType.RevengeAct))
        {
            return true;
        }
        int hit = 100;
        if (range > 0)
        {
            hit -= range * 25;
        }
        // S⇒L Range.Sスキル = range=1で25%カット
        // L⇒L Range.Sスキル = range=2で50%カット
        // S⇒L Range.Lスキル = range=0
        // L⇒L Range.Lスキル = range=1で25%カット
        hit += subject.CurrentHit();
        if (subject.IsState(StateType.Blind))
        {
            hit -= subject.StateEffectAll(StateType.Blind);
        }
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
        float UpperDamageRate = 1;
        if (isNoEffect == false)
        {
            if (battlerInfo.IsState(StateType.DamageUp))
            {
                UpperDamageRate += battlerInfo.StateEffectAll(StateType.DamageUp) * 0.01f;
            }
            if (battlerInfo.IsState(StateType.Rebellious) && isOneTarget)
            {
                UpperDamageRate += (1f - ((float)battlerInfo.Hp / (float)battlerInfo.MaxHp));
            }
        }
        return UpperDamageRate;
    }

    private float CalcDamageCut(BattlerInfo target,bool isNoEffect)
    {
        float damageCutRate = 0;
        if (isNoEffect == false)
        {
            if (target.IsState(StateType.DamageCut))
            {
                damageCutRate += target.StateEffectAll(StateType.DamageCut) * 0.01f;
            }
        }
        return damageCutRate;
    }

    private int CalcRange(BattlerInfo subject,BattlerInfo target,int skillId)
    {
        var range = 0;
        var skillData = DataSystem.FindSkill(skillId);
        if (skillData.Range == RangeType.S)
        {
            // S⇒L Range.Sスキル = range=1で25%カット
            // L⇒L Range.Sスキル = range=2で50%カット
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
            // L⇒L Range.Lスキル = range=1で25%カット
            if (subject.LineIndex == LineType.Back && target.LineIndex == LineType.Back)
            {
                range = 1;
            }
        }
        return range;
    }

    private void MakeHpDamage(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData,bool isNoEffect,bool isOneTarget,int range)
    {
        var hpDamage = 0;
        int AtkValue = CurrentAttack(subject,isNoEffect);
        int DefValue = CurrentDefense(subject,target,isNoEffect);
        float UpperDamageRate = CurrentDamageRate(subject,isNoEffect,isOneTarget);
        float DamageRate = featureData.Param1 * UpperDamageRate;
        float SkillDamage = (DamageRate * 0.01f * (AtkValue * 0.5f));
        if (target.CanMove() && !isNoEffect)
        {
            CalcCounterDamage(subject,target,SkillDamage);
        }
        CalcFreezeDamage(subject,SkillDamage);

        SkillDamage *= GetDefenseRateValue((AtkValue * 0.5f),DefValue);
        float DamageValue = Mathf.Max(1,SkillDamage);
        if (target.IsState(StateType.HolyCoffin))
        {
            DamageValue *= (1 + target.StateEffectAll(StateType.HolyCoffin) * 0.01f);
        }
        DamageValue *= (1f - CalcDamageCut(target,isNoEffect));
        hpDamage = (int)Mathf.Round(DamageValue);
        // 属性補正
        // クリティカル
        if (IsCritical(subject,target))
        {
            hpDamage = ApplyCritical(hpDamage);
        }
        hpDamage = ApplyVariance(hpDamage);
        if (target.IsState(StateType.CounterAura) && target.IsState(StateType.CounterAuraShell))
        {
            hpDamage -= target.StateEffectAll(StateType.CounterAuraShell);
        }
        hpDamage = Mathf.Max(1,hpDamage);
        if (target.IsState(StateType.NoDamage) && !isNoEffect)
        {
            hpDamage = 0;
            SeekStateCount(target,StateType.NoDamage);
        }
        if (subject.IsState(StateType.Deadly) && !isNoEffect)
        {
            if (!target.IsState(StateType.Barrier))
            {
                int rand = new System.Random().Next(0, 100);
                if (subject.StateEffectAll(StateType.Deadly) >= rand){
                    hpDamage = target.Hp;
                }
            }
        }
        if (subject.IsState(StateType.Drain))
        {
            _reHeal += (int)Mathf.Floor(hpDamage * subject.StateEffectAll(StateType.Drain) * 0.01f);
        }
        if (range > 0)
        {
            // S⇒L Range.Sスキル = range=1で25%カット
            // L⇒L Range.Sスキル = range=2で50%カット
            // S⇒L Range.Lスキル = range=0
            // L⇒L Range.Lスキル = range=1で25%カット
            hpDamage = (int)MathF.Round((float)hpDamage * (1 - (0.25f * range)));
        }
        _hpDamage += hpDamage; 
    }

    private void MakeHpPerDamage(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData,bool isNoEffect,bool isOneTarget,int range)
    {
        var hpDamage = (int)Math.Round(target.MaxHp * 0.01f * featureData.Param1);
        _hpDamage += hpDamage; 
    }

    private void MakeHpAddDamage(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData,bool isNoEffect,bool isOneTarget,int range)
    {
        var hpDamage = (int)Math.Round(_hpDamage * 0.01f * featureData.Param1);
        _hpDamage += hpDamage;
        // 追加ダメージで戦闘不能にならない
        if (_hpDamage > target.Hp)
        {
            _hpDamage = target.Hp - 1;
        }
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
        _hpHeal += healValue;
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
        var hpDamage = 0;
        int AtkValue = featureData.Param1;
        float DamageRate = 100;
        float UpperDamageRate = CurrentDamageRate(subject,isNoEffect,false);
        DamageRate *= UpperDamageRate;
        float SkillDamage = (DamageRate * 0.01f * AtkValue);
        float DamageValue = Mathf.Max(1,SkillDamage);
        if (target.IsState(StateType.HolyCoffin))
        {
            DamageValue *= (1 + target.StateEffectAll(StateType.HolyCoffin) * 0.01f);
        }
        DamageValue *= (1f - CalcDamageCut(target,isNoEffect));
        hpDamage = (int)Mathf.Round(DamageValue);
        hpDamage = Mathf.Max(1,hpDamage);
        if (subject.IsState(StateType.Drain))
        {
            _reHeal = (int)Mathf.Floor(hpDamage * subject.StateEffectAll(StateType.Drain) * 0.01f);
        }
        _hpDamage += hpDamage;
    }

    private void MakeHpStateDamage(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData,bool isNoEffect,bool isOneTarget)
    {
        var hpDamage = 0;
        int AtkValue = CurrentAttack(subject,isNoEffect);
        int DefValue = CurrentDefense(subject,target,isNoEffect);
        float DamageRate = featureData.Param2;
        if (target.IsState((StateType)featureData.Param3))
        {
            DamageRate = featureData.Param1;
        }
        float UpperDamageRate = CurrentDamageRate(subject,isNoEffect,isOneTarget);
        DamageRate *= UpperDamageRate;
        float SkillDamage = (DamageRate * 0.01f * (AtkValue * 0.5f));
        if (target.CanMove() && !isNoEffect)
        {
            CalcCounterDamage(subject,target,SkillDamage);
        }
        CalcFreezeDamage(subject,SkillDamage);

        SkillDamage *= GetDefenseRateValue((AtkValue * 0.5f),DefValue);
        //SkillDamage -= (DefValue * 0.5f);
        float DamageValue = Mathf.Max(1,SkillDamage);
        if (target.IsState(StateType.HolyCoffin))
        {
            DamageValue *= (1 + target.StateEffectAll(StateType.HolyCoffin) * 0.01f);
        }
        DamageValue *= (1f - CalcDamageCut(target,isNoEffect));
        hpDamage = (int)Mathf.Round(DamageValue);
        // 属性補正
        // クリティカル
        if (IsCritical(subject,target))
        {
            hpDamage = ApplyCritical(hpDamage);
        }
        hpDamage = ApplyVariance(hpDamage);
        hpDamage = Mathf.Max(1,hpDamage);
        if (target.IsState(StateType.NoDamage) && !isNoEffect)
        {
            hpDamage = 0;
            SeekStateCount(target,StateType.NoDamage);
        }
        if (subject.IsState(StateType.Deadly) && !isNoEffect)
        {
            if (!target.IsState(StateType.Barrier))
            {
                int rand = new System.Random().Next(0, 100);
                if (subject.StateEffectAll(StateType.Deadly) >= rand){
                    hpDamage = target.Hp;
                }
            }
        }
        if (subject.IsState(StateType.Drain))
        {
            _reHeal += (int)Mathf.Floor(hpDamage * subject.StateEffectAll(StateType.Drain) * 0.01f);
        }
        _hpDamage += hpDamage; 
    }

    // 呪いダメージ計算
    private void MakeHpCursedDamage(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData,bool isNoEffect,bool isOneTarget)
    {
        var hpDamage = 0;
        int AtkValue = featureData.Param1;
        float DamageRate = 100;
        float UpperDamageRate = CurrentDamageRate(subject,isNoEffect,isOneTarget);
        DamageRate *= UpperDamageRate;
        float SkillDamage = (DamageRate * 0.01f * AtkValue);
        float DamageValue = Mathf.Max(1,SkillDamage);
        if (target.IsState(StateType.HolyCoffin))
        {
            DamageValue *= (1 + target.StateEffectAll(StateType.HolyCoffin) * 0.01f);
        }
        DamageValue *= (1f - CalcDamageCut(target,isNoEffect));
        hpDamage = (int)Mathf.Round(DamageValue);
        hpDamage = Mathf.Max(1,hpDamage);
        if (target.IsState(StateType.NoDamage) && !isNoEffect)
        {
            hpDamage = 0;
            SeekStateCount(target,StateType.NoDamage);
        }
        if (target.IsState(StateType.DamageCut) && !isNoEffect)
        {
            
        }
        /*
        if (subject.IsState(StateType.Drain))
        {
            _reHeal = (int)Mathf.Floor(hpDamage * subject.StateEffectAll(StateType.Drain) * 0.01f);
        }
        */
        _hpDamage += hpDamage;
    }

    private void MakeMpDamage(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData)
    {
        _mpDamage = featureData.Param1;
    }

    private void MakeMpHeal(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData)
    {
        float HealValue = featureData.Param1;
        _mpHeal = (int)Mathf.Round(HealValue);
        //_hpHeal = ApplyVariance(_hpHeal);
    }

    private void MakeAddState(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData,bool checkCounter = false,bool isOneTarget = false,bool removeTimingIsNextTurn = false,int range = 0)
    {
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
        if (stateInfo.Master.StateType == StateType.CounterAura || stateInfo.Master.StateType == StateType.Benediction)
        {
            stateInfo.Turns = 200 - subject.Status.Spd * 2;
        } else
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
                    var removeFeature = new SkillData.FeatureData();
                    removeFeature.FeatureType = FeatureType.RemoveState;
                    removeFeature.Param1 = (int)removeState.Master.StateType;
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
            var counterAddState = new SkillData.FeatureData();
            counterAddState.FeatureType = FeatureType.AddState;
            counterAddState.Param1 = featureData.Param1;
            counterAddState.Param2 = featureData.Param2;
            counterAddState.Param3 = featureData.Param3;
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

    private void MakeRemainHpOne(BattlerInfo subject)
    {
        _reDamage = subject.Hp - 1;
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
        if (target.IsState(StateType.Undead))
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
    public void AddRemoveState(StateInfo stateInfo)
    {
        if (_removedStates.IndexOf(stateInfo) == -1){
            _removedStates.Add(stateInfo);
        }
    }
    
    private float GetDefenseRateValue(float atk,float def){
        // 防御率 ＝ 1 - 防御 / (攻撃 + 防御)　※攻撃 + 防御 < 1の時、1
        float _defenseRateValue;
        if ((atk + def) < 1){
            _defenseRateValue = 1;
        } else{
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

    private void CalcCounterDamage(BattlerInfo subject,BattlerInfo target,float skillDamage)
    {
        if (target.IsState(StateType.CounterAura))
        {
            _execStateInfos[target.Index].Add(target.GetStateInfo(StateType.CounterAura));
            if (subject.IsState(StateType.NoDamage))
            {
                SeekStateCount(subject,StateType.NoDamage);
            } else
            {                
                _reDamage += CounterDamageValue(target);
            }
        }
    }

    private int CounterDamageValue(BattlerInfo target)
    {
        int ReDamage = (int)Mathf.Floor((target.CurrentDef(false) * 0.5f) * target.StateEffectAll(StateType.CounterAura) * 0.01f);
        ReDamage += target.StateEffectAll(StateType.CounterAuraDamage);
        return ReDamage;
    }

    private bool IsCritical(BattlerInfo subject,BattlerInfo target)
    {
        int HitOver = subject.CurrentHit() - target.CurrentEva();
        if (HitOver < 0){
            HitOver = 0;
        }
        int CriticalRate = subject.StateEffectAll(StateType.CriticalRateUp) + HitOver;
        int rand = new System.Random().Next(0, 100);
        return (CriticalRate >= rand);
    }

    private int AntiDoteDamageValue(BattlerInfo target)
    {
        int ReDamage = (int)Mathf.Floor((target.CurrentDef(false) * 0.5f));
        return ReDamage;
    }

    private int ApplyCritical(int value)
    {
        return Mathf.FloorToInt( value * 1.5f );
    }

    private int ApplyVariance(int value)
    {
        int rand = new System.Random().Next(-10, 10);
        return (int)Mathf.Floor(value * (1 + rand * 0.01f));
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
                || AddedStates.Find(a => a.Master.StateType == StateType.Chain) != null
                || AddedStates.Find(a => a.Master.StateType == StateType.Death) != null
                || DeadIndexList.Contains(TargetIndex);
    }
}
