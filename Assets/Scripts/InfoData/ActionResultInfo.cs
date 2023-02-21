using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionResultInfo 
{
    private int _subjectIndex = 0;
    public int SubjectIndex{
        get { return _subjectIndex;}
    }
    private int _targetIndex = 0;
    public int TargetIndex{
        get { return _targetIndex;}
    }

    private ActionInfo _actionInfo = null;


    public ActionResultInfo(int subjectIndex,int targetIndex,ActionInfo actionInfo)
    {
        _subjectIndex = subjectIndex;
        _targetIndex = targetIndex;
        _actionInfo = actionInfo;
    }

    private int _hpDamage = 0;
    public int HpDamage {
        get {return _hpDamage;} set{_hpDamage = value;}
    }
    private int _hpHeal = 0;
    public int HpHeal {
        get {return _hpHeal;} set{_hpHeal = value;}
    }
    private int _mpDamage = 0;
    public int MpDamage {
        get {return _mpDamage;}
    }
    private int _reDamage = 0;
    public int ReDamage {
        get {return _reDamage;} set{_reDamage = value;}
    }
    private int _reHeal = 0;
    public int ReHeal {
        get {return _reHeal;} set{_reHeal = value;}
    }
    private bool _isDead = false;
    public bool IsDead {
        get {return _isDead;}
    }

    
    private List<StateInfo> _addedStates = new List<StateInfo>();
    public List<StateInfo> AddedStates {
        get {return _addedStates;}
    }
    private List<StateInfo> _removedStates = new List<StateInfo>();
    public List<StateInfo> RemovedStates {
        get {return _removedStates;}
    }
    private Dictionary<int,List<StateType>> _execStateInfos = new Dictionary<int, List<StateType>>();
    public  Dictionary<int,List<StateType>> ExecStateInfos{
        get {return _execStateInfos;}
    }

    public void MakeResultData(BattlerInfo subject,BattlerInfo target)
    {
        _targetIndex = target.Index;
        _execStateInfos[subject.Index] = new List<StateType>();
        _execStateInfos[_targetIndex] = new List<StateType>();
        List<SkillsData.FeatureData> featureDatas = _actionInfo.Master.FeatureDatas;
        for (int i = 0; i < featureDatas.Count; i++)
        {
            MakeFeature(subject,target,featureDatas[i]);
        }
    }

    private void MakeFeature(BattlerInfo subject,BattlerInfo target,SkillsData.FeatureData featureData)
    {
        switch (featureData.FeatureType)
        {
            case FeatureType.HpDamage:
                MakeHpDamage(subject,target,featureData);
                return;
            case FeatureType.HpHeal:
                MakeHpHeal(subject,target,featureData);
                return;
            case FeatureType.HpDrain:
                MakeHpDrain(subject,target,featureData);
                return;
            case FeatureType.AddState:
                MakeAddState(subject,target,featureData);
                return;
            case FeatureType.RemoveState:
                MakeRemoveState(subject,target,featureData);
                return;
            case FeatureType.PlusSkill:
                return;

        }
    }

    private void MakeHpDamage(BattlerInfo subject,BattlerInfo target,SkillsData.FeatureData featureData)
    {
        if (target.IsState(StateType.NoDamage))
        {
            _execStateInfos[target.Index].Add(StateType.NoDamage);
            return;
        }
        int AtkValue = subject.CurrentAtk();
        int DefValue = target.CurrentDef();
        float SkillDamage = (featureData.Param1 * 0.01f * (AtkValue * 0.5f));
        if (target.IsState(StateType.CounterOura))
        {
            _execStateInfos[target.Index].Add(StateType.CounterOura);
            _reDamage = (int)SkillDamage;
        }
        SkillDamage -= (DefValue * 0.5f);
        float DamageValue = Mathf.Max(0,SkillDamage);
        _hpDamage = (int)Mathf.Round(DamageValue);
        // 属性補正
        // クリティカル
        _hpDamage = ApplyVariance(_hpDamage);
        if (_hpDamage >= target.Hp)
        {
            _isDead = true;
        }
    }

    private void MakeHpHeal(BattlerInfo subject,BattlerInfo target,SkillsData.FeatureData featureData)
    {
        float HealValue = target.Status.Hp * featureData.Param1 * 0.01f;
        _hpHeal = (int)Mathf.Round(HealValue);
        _hpHeal = ApplyVariance(_hpHeal);
    }

    private void MakeHpDrain(BattlerInfo subject,BattlerInfo target,SkillsData.FeatureData featureData)
    {
        MakeHpDamage(subject,target,featureData);
        _reHeal = (int)Mathf.Floor(HpDamage * featureData.Param3 * 0.01f);
    }

    private void MakeAddState(BattlerInfo subject,BattlerInfo target,SkillsData.FeatureData featureData)
    {
        StateInfo stateInfo = new StateInfo(featureData.Param1,featureData.Param2,featureData.Param3,subject.Index,target.Index);
        bool IsAdded = target.AddState(stateInfo);
        if (IsAdded)
        {
            _addedStates.Add(stateInfo);
        }
    }
    
    private void MakeRemoveState(BattlerInfo subject,BattlerInfo target,SkillsData.FeatureData featureData)
    {
        StateInfo stateInfo = new StateInfo(featureData.Param1,featureData.Param2,featureData.Param3,subject.Index,target.Index);
        bool IsRemoved = target.RemoveState(stateInfo);
        if (IsRemoved)
        {
            _removedStates.Add(stateInfo);
        }
    }

    public void AddRemoveState(StateInfo stateInfo)
    {
        _removedStates.Add(stateInfo);
    }


    private int ApplyVariance(int value)
    {
        int rand = new System.Random().Next(-10, 10);
        return (int) Mathf.Floor(value * (1 + rand * 0.01f));
    }
}
