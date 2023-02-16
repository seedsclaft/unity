using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionResultInfo 
{
    private int _subjectIndex = 0;
    public BattlerInfo Subject{
        get {return _battlerInfos.Find(a => a.Index == _subjectIndex);}
    }
    private int _targetIndex = 0;
    public BattlerInfo Target{
        get {return _battlerInfos.Find(a => a.Index == _targetIndex);}
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

    private List<BattlerInfo> _battlerInfos = new List<BattlerInfo>();
    public void MakeResultData(List<BattlerInfo> battlerInfos)
    {
        _battlerInfos = battlerInfos;
        List<SkillsData.FeatureData> featureDatas = _actionInfo.Master.FeatureDatas;
        for (int i = 0; i < featureDatas.Count; i++)
        {
            MakeFeature(featureDatas[i]);
        }
    }

    private void MakeFeature(SkillsData.FeatureData featureData)
    {
        switch (featureData.FeatureType)
        {
            case FeatureType.HpDamage:
                MakeHpDamage(featureData);
                return;

        }
    }

    private void MakeHpDamage(SkillsData.FeatureData featureData)
    {
        int AtkValue = Subject.Status.Atk;
        int DefValue = Target.Status.Def;
        float DamageValue = Mathf.Max(0,(featureData.Param1 * 0.01f * (AtkValue * 0.5f)) - (DefValue * 0.5f));
        _hpDamage = (int)Mathf.Round(DamageValue);
        // 属性補正
        // クリティカル
        _hpDamage = ApplyVariance(_hpDamage);
        if (_hpDamage >= Target.Hp)
        {
            _isDead = true;
        }
    }

    private int ApplyVariance(int value)
    {
        int rand = new System.Random().Next(-10, 10);
        return (int) Mathf.Floor(value * (1 + rand * 0.01f));
    }
}
