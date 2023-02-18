using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionResultInfo 
{
    private int _subjectIndex = 0;
    private int _targetIndex = 0;

    private ActionInfo _actionInfo = null;


    public ActionResultInfo(int subjectIndex,int targetIndex,ActionInfo actionInfo)
    {
        _subjectIndex = subjectIndex;
        _targetIndex = targetIndex;
        _actionInfo = actionInfo;
    }

    private int _hpDamage = 0;
    public int HpDamage {
        get {return _hpDamage;}
    }
    private int _mpDamage = 0;
    public int MpDamage {
        get {return _mpDamage;}
    }
    private bool _isDead = false;
    public bool IsDead {
        get {return _isDead;}
    }
    public int TargetIndex{
        get { return _targetIndex;}
    }

    private List<StateInfo> _addedStates = new List<StateInfo>();
    public List<StateInfo> AddedStates {
        get {return _addedStates;}
    }
    private List<StateInfo> _removedStates = new List<StateInfo>();
    public List<StateInfo> RemovedStates {
        get {return _removedStates;}
    }

    public void MakeResultData(BattlerInfo subject,BattlerInfo target)
    {
        _targetIndex = target.Index;
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
            case FeatureType.AddState:
                MakeAddState(subject,target,featureData);
                return;
            case FeatureType.RemoveState:
                MakeRemoveState(subject,target,featureData);
                return;
            case FeatureType.PlusSkill:
                //MakePlusSkill(featureData);
                return;

        }
    }

    private void MakeHpDamage(BattlerInfo subject,BattlerInfo target,SkillsData.FeatureData featureData)
    {
        int AtkValue = subject.CurrentAtk();
        int DefValue = target.CurrentDef();
        float DamageValue = Mathf.Max(0,(featureData.Param1 * 0.01f * (AtkValue * 0.5f)) - (DefValue * 0.5f));
        _hpDamage = (int)Mathf.Round(DamageValue);
        // 属性補正
        // クリティカル
        _hpDamage = ApplyVariance(_hpDamage);
        if (_hpDamage >= target.Hp)
        {
            _isDead = true;
        }
    }

    private void MakeAddState(BattlerInfo subject,BattlerInfo target,SkillsData.FeatureData featureData)
    {
        StateInfo stateInfo = new StateInfo(featureData.Param1,featureData.Param2,featureData.Param3,subject.Index);
        bool IsAdded = target.AddState(stateInfo);
        if (IsAdded)
        {
            _addedStates.Add(stateInfo);
        }
    }
    
    private void MakeRemoveState(BattlerInfo subject,BattlerInfo target,SkillsData.FeatureData featureData)
    {
        StateInfo stateInfo = new StateInfo(featureData.Param1,featureData.Param2,featureData.Param3,subject.Index);
        bool IsRemoved = target.RemoveState(stateInfo);
        if (IsRemoved)
        {
            _removedStates.Add(stateInfo);
        }
    }


    private int ApplyVariance(int value)
    {
        int rand = new System.Random().Next(-10, 10);
        return (int) Mathf.Floor(value * (1 + rand * 0.01f));
    }
}
