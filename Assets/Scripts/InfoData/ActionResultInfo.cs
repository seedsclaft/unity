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
            MakeFeature(subject,target,featureDates[i],isOneTarget);
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

    private void MakeFeature(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData,bool isOneTarget = false)
    {
        switch (featureData.FeatureType)
        {
            case FeatureType.HpDamage:
                if (CheckIsHit(subject,target,isOneTarget))
                {
                    MakeHpDamage(subject,target,featureData,false,isOneTarget);
                }
                return;
            case FeatureType.HpHeal:
                MakeHpHeal(subject,target,featureData);
                return;
            case FeatureType.HpDrain:
                if (CheckIsHit(subject,target,isOneTarget))
                {
                    MakeHpDrain(subject,target,featureData,isOneTarget);
                }
                return;
            case FeatureType.HpDefineDamage:
                MakeHpDefineDamage(subject,target,featureData,false);
                return;
            case FeatureType.HpStateDamage:
                if (CheckIsHit(subject,target,isOneTarget))
                {
                    MakeHpStateDamage(subject,target,featureData,false,isOneTarget);
                }
                return;
            case FeatureType.HpCursedDamage:
                MakeHpCursedDamage(subject,target,featureData,false,isOneTarget);
                return;
            case FeatureType.NoEffectHpDamage:
                MakeHpDamage(subject,target,featureData,true,isOneTarget);
                return;
            case FeatureType.MpDamage:
                if (CheckIsHit(subject,target,isOneTarget))
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
            case FeatureType.KindHeal:
                MakeKindHeal(subject,target,featureData);
                return;
            case FeatureType.BreakUndead:
                MakeBreakUndead(subject,target,featureData);
                return;
        }
    }

    private bool CheckIsHit(BattlerInfo subject,BattlerInfo target,bool isOneTarget)
    {
        var skillData = DataSystem.Skills.Find(a => a.Id == _skillId);
        if (skillData != null && (skillData.SkillType == SkillType.Demigod || skillData.SkillType == SkillType.Awaken))
        {
            return true;
        }
        if (!IsHit(subject,target,isOneTarget))
        {
            _missed = true;
            return false;
        }
        return true;
    }

    private bool IsHit(BattlerInfo subject,BattlerInfo target,bool isOneTarget)
    {
        if (subject.IsState(StateType.AbsoluteHit))
        {
            return true;
        }
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
        if (subject.IsState(StateType.HitUp))
        {
            hit += subject.StateEffectAll(StateType.HitUp);
        }
        if (subject.IsState(StateType.Blind))
        {
            hit -= subject.StateEffectAll(StateType.Blind);
        }
        if (target.IsState(StateType.EvaUp))
        {
            hit -= target.StateEffectAll(StateType.EvaUp);
        }
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

    private int CurrentDefense(BattlerInfo battlerInfo,bool isNoEffect)
    {
        int DefValue = battlerInfo.CurrentDef(isNoEffect);
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

    private void MakeHpDamage(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData,bool isNoEffect,bool isOneTarget)
    {
        var hpDamage = 0;
        int AtkValue = CurrentAttack(subject,isNoEffect);
        int DefValue = CurrentDefense(target,isNoEffect);
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
        hpDamage = (int)Mathf.Round(DamageValue);
        // 属性補正
        // クリティカル
        if (subject.IsState(StateType.CriticalRateUp))
        {
            int CriticalRate = subject.StateEffectAll(StateType.CriticalRateUp);
            int rand = new System.Random().Next(0, 100);
            if (CriticalRate >= rand)
            {        
                hpDamage = ApplyCritical(hpDamage);
            }
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
        _hpDamage += hpDamage; 
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

    private void MakeHpDrain(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData,bool isOneTarget)
    {
        MakeHpDamage(subject,target,featureData,false,isOneTarget);
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
        int DefValue = CurrentDefense(target,isNoEffect);
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
        hpDamage = (int)Mathf.Round(DamageValue);
        // 属性補正
        // クリティカル
        if (subject.IsState(StateType.CriticalRateUp))
        {
            int CriticalRate = subject.StateEffectAll(StateType.CriticalRateUp);
            int rand = new System.Random().Next(0, 100);
            if (CriticalRate >= rand)
            {        
                hpDamage = ApplyCritical(hpDamage);
            }
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
        hpDamage = (int)Mathf.Round(DamageValue);
        hpDamage = Mathf.Max(1,hpDamage);
        if (target.IsState(StateType.NoDamage) && !isNoEffect)
        {
            hpDamage = 0;
            SeekStateCount(target,StateType.NoDamage);
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

    private void MakeAddState(BattlerInfo subject,BattlerInfo target,SkillData.FeatureData featureData,bool checkCounter = false,bool isOneTarget = false)
    {
        var stateInfo = new StateInfo((StateType)featureData.Param1,featureData.Param2,featureData.Param3,subject.Index,target.Index,_skillId);
        if (stateInfo.Master.CheckHit)
        {
            if (!CheckIsHit(subject,target,isOneTarget))
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
        if (seekState.Master.RemovalTiming == RemovalTiming.UpdateCount)
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
}
