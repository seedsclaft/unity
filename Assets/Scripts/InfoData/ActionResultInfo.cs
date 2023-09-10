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

    private int _skillIndex = -1;
    public ActionResultInfo(BattlerInfo subject,BattlerInfo target,List<SkillsData.FeatureData> featureDatas,int skillIndex)
    {
        if (subject != null && target != null)
        {
            _subjectIndex = subject.Index;
            _targetIndex = target.Index;
            _execStateInfos[subject.Index] = new List<StateType>();
            _execStateInfos[_targetIndex] = new List<StateType>();
            _skillIndex = skillIndex;
        }
        for (int i = 0; i < featureDatas.Count; i++)
        {
            MakeFeature(subject,target,featureDatas[i]);
        }
        if (subject != null && target != null)
        {
            if (_hpDamage >= target.Hp && target.IsAlive())
            {
                if (target.IsState(StateType.Undead) && featureDatas.Find(a => a.FeatureType == FeatureType.BreakUndead) == null)
                {
                    SkillsData.FeatureData undeadFeature = new SkillsData.FeatureData();
                    undeadFeature.FeatureType = FeatureType.RemoveState;
                    undeadFeature.Param1 = (int)StateType.Undead;
                    MakeRemoveState(target,target,undeadFeature);
                    _hpDamage = target.Hp - 1;
                } else
                {
                    _deadIndexList.Add(target.Index);
                }
            }
            int resuceHp = subject.MaxHp - subject.Hp;
            int recoveryHp = Mathf.Min(_reHeal,resuceHp);
            if ((_reDamage - recoveryHp) >= subject.Hp && subject.IsAlive())
            {
                if (subject.IsState(StateType.Undead) && featureDatas.Find(a => a.FeatureType == FeatureType.BreakUndead) == null)
                {
                    SkillsData.FeatureData undeadFeature = new SkillsData.FeatureData();
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
                if (removeState.StateId == (int)StateType.Death)
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
    private Dictionary<int,List<StateType>> _execStateInfos = new ();
    public  Dictionary<int,List<StateType>> ExecStateInfos => _execStateInfos;

    private void MakeFeature(BattlerInfo subject,BattlerInfo target,SkillsData.FeatureData featureData)
    {
        switch (featureData.FeatureType)
        {
            case FeatureType.HpDamage:
                MakeHpDamage(subject,target,featureData,false);
                return;
            case FeatureType.HpHeal:
                MakeHpHeal(subject,target,featureData);
                return;
            case FeatureType.HpDrain:
                MakeHpDrain(subject,target,featureData);
                return;
            case FeatureType.HpDefineDamage:
                MakeHpDefineDamage(subject,target,featureData,false);
                return;
            case FeatureType.NoEffectHpDamage:
                MakeHpDamage(subject,target,featureData,true);
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
            case FeatureType.RemoveStatePassive:
                MakeRemoveStatePassive(subject,target,featureData);
                return;
            case FeatureType.ApHeal:
                MakeApHeal(subject,target,featureData);
                return;
            case FeatureType.KindHeal:
                MakeKindHeal(subject,target,featureData);
                return;
            case FeatureType.BreakUndead:
                MakeBreakUndead(subject,target,featureData);
                return;
        }
    }

    private bool IsHit(BattlerInfo subject,BattlerInfo target)
    {
        if (subject.IsState(StateType.AbsoluteHit))
        {
            return true;
        }
        if (target.IsState(StateType.Chain))
        {
            return true;
        }
        int hit = 100;
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

    private void MakeHpDamage(BattlerInfo subject,BattlerInfo target,SkillsData.FeatureData featureData,bool isNoEffect)
    {
        if (!IsHit(subject,target) && !isNoEffect)
        {
            _missed = true;
            return;
        }
        int AtkValue = subject.CurrentAtk();
        if (subject.IsState(StateType.AtkUp) && !isNoEffect)
        {
            AtkValue += subject.StateEffectAll(StateType.AtkUp);
        }
        int DefValue = target.CurrentDef();
        if (target.IsState(StateType.DefUp) && !isNoEffect)
        {
            DefValue += target.StateEffectAll(StateType.DefUp);
        }
        if (target.IsState(StateType.DefDown) && !isNoEffect)
        {
            DefValue -= target.StateEffectAll(StateType.DefDown);
        }
        float DamageRate = featureData.Param1;
        if (subject.IsState(StateType.DamageUp))
        {
            DamageRate *= subject.StateEffectAll(StateType.DamageUp);
        }
        float SkillDamage = (DamageRate * 0.01f * (AtkValue * 0.5f));
        if (target.CanMove() && !isNoEffect)
        {
            if (target.IsState(StateType.CounterOura))
            {
                _execStateInfos[target.Index].Add(StateType.CounterOura);
                if (subject.IsState(StateType.NoDamage))
                {
                    _execStateInfos[subject.Index].Add(StateType.NoDamage);
                    SeekNoDamage(subject);
                } else
                {
                    _reDamage += CounterDamageValue(target);
                }
                if (target.IsState(StateType.CounterOuraHeal))
                {
                    SkillsData.FeatureData counterOuraHeal = new SkillsData.FeatureData();
                    counterOuraHeal.FeatureType = FeatureType.HpHeal;
                    counterOuraHeal.Param1 = target.StateEffectAll(StateType.CounterOuraHeal);
                    MakeHpHeal(target,target,counterOuraHeal);
                }
            }
        }
        if (subject.IsState(StateType.Freeze))
        {
            _execStateInfos[subject.Index].Add(StateType.Freeze);
            if (subject.IsState(StateType.NoDamage))
            {
                _execStateInfos[subject.Index].Add(StateType.NoDamage);
                SeekNoDamage(subject);
            } else
            {
                _reDamage += FreezeDamageValue(subject,SkillDamage);
            }
        }

        SkillDamage *= GetDeffenseRateValue((AtkValue * 0.5f),DefValue);
        //SkillDamage -= (DefValue * 0.5f);
        float DamageValue = Mathf.Max(1,SkillDamage);
        _hpDamage = (int)Mathf.Round(DamageValue);
        // 属性補正
        // クリティカル
        if (subject.IsState(StateType.CriticalRateUp))
        {
            int CriticalRate = subject.StateEffectAll(StateType.CriticalRateUp);
            int rand = new System.Random().Next(0, 100);
            if (CriticalRate >= rand)
            {        
                _hpDamage = ApplyCritical(_hpDamage);
            }
        }
        _hpDamage = ApplyVariance(_hpDamage);
        _hpDamage = Mathf.Max(1,_hpDamage);
        if (target.IsState(StateType.NoDamage) && !isNoEffect)
        {
            _execStateInfos[target.Index].Add(StateType.NoDamage);
            _hpDamage = 0;
            SeekNoDamage(target);
        }
        if (subject.IsState(StateType.Deadly))
        {
            int rand = new System.Random().Next(0, 100);
            if (subject.StateEffectAll(StateType.Deadly) >= rand){
                _hpDamage = target.Hp;
            }
        }
        if (subject.IsState(StateType.Drain))
        {
            _reHeal = (int)Mathf.Floor(_hpDamage * subject.StateEffectAll(StateType.Drain) * 0.01f);
        }
    }

    private void MakeHpHeal(BattlerInfo subject,BattlerInfo target,SkillsData.FeatureData featureData)
    {
        float HealValue = featureData.Param1;
        _hpHeal = (int)Mathf.Round(HealValue);
        if (subject != target)
        {
            if (subject.IsState(StateType.HealActionSelfHeal))
            {
                _reHeal = (int)Mathf.Round(HealValue);
            }
        }
    }

    private void MakeHpDrain(BattlerInfo subject,BattlerInfo target,SkillsData.FeatureData featureData)
    {
        MakeHpDamage(subject,target,featureData,false);
        _reHeal = (int)Mathf.Floor(HpDamage * featureData.Param3 * 0.01f);
    }

    // スリップダメージ計算
    private void MakeHpDefineDamage(BattlerInfo subject,BattlerInfo target,SkillsData.FeatureData featureData,bool isNoEffect)
    {
        /*
        if (!IsHit(subject,target) && !isNoEffect)
        {
            _missed = true;
            return;
        }
        */
        int AtkValue = featureData.Param1;
        float DamageRate = 100;
        if (subject.IsState(StateType.DamageUp))
        {
            DamageRate *= subject.StateEffectAll(StateType.DamageUp);
        }
        float SkillDamage = (DamageRate * 0.01f * AtkValue);
        /*
        if (target.CanMove() && !isNoEffect)
        {
            if (target.IsState(StateType.CounterOura))
            {
                _execStateInfos[target.Index].Add(StateType.CounterOura);
                _reDamage += CounterDamageValue(target);
                if (target.IsState(StateType.CounterOuraHeal))
                {
                    SkillsData.FeatureData counterOuraHeal = new SkillsData.FeatureData();
                    counterOuraHeal.FeatureType = FeatureType.HpHeal;
                    counterOuraHeal.Param1 = target.StateEffectAll(StateType.CounterOuraHeal);
                    MakeHpHeal(target,target,counterOuraHeal);
                }
            }
        }
        */
        /*
        if (subject.IsState(StateType.Freeze))
        {
            _execStateInfos[subject.Index].Add(StateType.Freeze);
            _reDamage += FreezeDamageValue(subject,SkillDamage);
        }
        */
        float DamageValue = Mathf.Max(1,SkillDamage);
        _hpDamage = (int)Mathf.Round(DamageValue);
        _hpDamage = Mathf.Max(1,_hpDamage);
        /*
        if (target.IsState(StateType.NoDamage) && !isNoEffect)
        {
            _execStateInfos[target.Index].Add(StateType.NoDamage);
            _hpDamage = 0;
            int count = target.StateTurn(StateType.NoDamage);
            if (count <= 1)
            {
                _removedStates.Add(target.GetStateInfo(StateType.NoDamage));
            } else{
                _displayStates.Add(target.GetStateInfo(StateType.NoDamage));
            }
        }
        */
        if (subject.IsState(StateType.Drain))
        {
            _reHeal = (int)Mathf.Floor(_hpDamage * subject.StateEffectAll(StateType.Drain) * 0.01f);
        }
    }

    private void MakeMpHeal(BattlerInfo subject,BattlerInfo target,SkillsData.FeatureData featureData)
    {
        float HealValue = featureData.Param1;
        _mpHeal = (int)Mathf.Round(HealValue);
        //_hpHeal = ApplyVariance(_hpHeal);
    }

    private void MakeAddState(BattlerInfo subject,BattlerInfo target,SkillsData.FeatureData featureData,bool checkCounter = false)
    {
        StateInfo stateInfo = new StateInfo(featureData.Param1,featureData.Param2,featureData.Param3,subject.Index,target.Index,_skillIndex);
        if (stateInfo.Master.Id == (int)StateType.CounterOura || stateInfo.Master.Id == (int)StateType.Benediction)
        {
            stateInfo.Turns = 200 - subject.Status.Spd * 2;
        } else
        if (stateInfo.Master.Id == (int)StateType.Demigod)
        {
            stateInfo.Effect = subject.DemigodParam;
        } else
        if (stateInfo.Master.Id == (int)StateType.Death)
        {
            _hpDamage = target.Hp;
        }
        bool IsAdded = target.AddState(stateInfo,false);
        if (IsAdded)
        {
            if (stateInfo.Master.Id == (int)StateType.RemoveBuff)
            {
                var removeStates = target.GetRemovalBuffStates();
                foreach (var removeState in removeStates)
                {
                    SkillsData.FeatureData removeFeature = new SkillsData.FeatureData();
                    removeFeature.FeatureType = FeatureType.RemoveState;
                    removeFeature.Param1 = removeState.Master.Id;
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
                if (stateInfo.IsBarrierStateType())
                {
                    StateInfo barrierState = new StateInfo((int)StateType.Barrier,0,0,0,target.Index,-1);
                    _displayStates.Add(barrierState);
                }
            }
        }
        if (stateInfo.IsBarrierStateType() && target.IsState(StateType.AntiDote))
        {
            _execStateInfos[target.Index].Add(StateType.AntiDote);
            if (subject.IsState(StateType.NoDamage))
            {
                _execStateInfos[subject.Index].Add(StateType.NoDamage);
                SeekNoDamage(subject);
            } else
            {
                _reDamage += AntiDoteDamageValue(target);
            }
            SkillsData.FeatureData counterAddState = new SkillsData.FeatureData();
            counterAddState.FeatureType = FeatureType.AddState;
            counterAddState.Param1 = featureData.Param1;
            counterAddState.Param2 = featureData.Param2;
            counterAddState.Param3 = featureData.Param3;
            MakeAddState(target,subject,counterAddState,false);
        }
    }
    
    private void MakeRemoveState(BattlerInfo subject,BattlerInfo target,SkillsData.FeatureData featureData)
    {
        // skillId -1のRemoveは強制で解除する
        StateInfo stateInfo = new StateInfo(featureData.Param1,featureData.Param2,featureData.Param3,subject.Index,target.Index,-1);
        bool IsRemoved = target.RemoveState(stateInfo,false);
        if (IsRemoved)
        {
            _removedStates.Add(stateInfo);
        }
    }

    private void MakeRemoveStatePassive(BattlerInfo subject,BattlerInfo target,SkillsData.FeatureData featureData)
    {
        // パッシブはそのパッシブスキルのみ解除する
        StateInfo stateInfo = new StateInfo(featureData.Param1,featureData.Param2,featureData.Param3,subject.Index,target.Index,_skillIndex);
        bool IsRemoved = target.RemoveState(stateInfo,false);
        if (IsRemoved)
        {
            _removedStates.Add(stateInfo);
        }
    }

    private void MakeApHeal(BattlerInfo subject,BattlerInfo target,SkillsData.FeatureData featureData)
    {
        float HealValue = featureData.Param1;
        _apHeal = (int)Mathf.Round(HealValue);
    }

    public void MakeKindHeal(BattlerInfo subject,BattlerInfo target,SkillsData.FeatureData featureData)
    {
        if (target.Kinds.IndexOf((KindType)featureData.Param1) != -1)
        {
            _hpDamage = (int)Mathf.Floor( _hpHeal * featureData.Param3 * 0.01f);
            _hpHeal = 0;
        }
    }

    public void MakeBreakUndead(BattlerInfo subject,BattlerInfo target,SkillsData.FeatureData featureData)
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
    
    private float GetDeffenseRateValue(float atk,float def){
        // 防御率 ＝ 1 - 防御 / (攻撃 + 防御)　※攻撃 + 防御 < 1の時、1
        float _deffenceRateValue;
        if ((atk + def) < 1){
            _deffenceRateValue = 1;
        } else{
            _deffenceRateValue = 1 - (def / (atk + def));
        }
        return _deffenceRateValue;
    }

    private int FreezeDamageValue(BattlerInfo subject,float skillDamage)
    {
        int ReDamage = (int)Mathf.Floor(skillDamage * subject.StateEffectAll(StateType.Freeze) * 0.01f);
        return ReDamage;
    }

    private int CounterDamageValue(BattlerInfo target)
    {
        int ReDamage = (int)Mathf.Floor((target.CurrentDef() * 0.5f) * target.StateEffectAll(StateType.CounterOura) * 0.01f);
        ReDamage += target.StateEffectAll(StateType.CounterOuraDamage);
        return ReDamage;
    }

    private int AntiDoteDamageValue(BattlerInfo target)
    {
        int ReDamage = (int)Mathf.Floor((target.CurrentDef() * 0.5f));
        return ReDamage;
    }

    private int ApplyCritical(int value)
    {
        return Mathf.FloorToInt( value * 1.5f );
    }

    private int ApplyVariance(int value)
    {
        int rand = new System.Random().Next(-10, 10);
        return (int) Mathf.Floor(value * (1 + rand * 0.01f));
    }

    private void SeekNoDamage(BattlerInfo battlerInfo)
    {
        int count = battlerInfo.StateTurn(StateType.NoDamage);
        if (count <= 1)
        {
            _removedStates.Add(battlerInfo.GetStateInfo(StateType.NoDamage));
        } else{
            _displayStates.Add(battlerInfo.GetStateInfo(StateType.NoDamage));
        }
    }

}
