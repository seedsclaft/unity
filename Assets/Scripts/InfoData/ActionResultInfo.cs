using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionResultInfo 
{
    private BattlerInfo _subject = null;
    private BattlerInfo _target = null;
    public BattlerInfo Target{
        get {return _target;}
    }
    private ActionInfo _actionInfo = null;


    public ActionResultInfo(BattlerInfo subject,BattlerInfo target,ActionInfo actionInfo)
    {
        _subject = subject;
        _target = target;
        _actionInfo = actionInfo;
        MakeResultData();
    }

    private int _hpDamage = 0;
    public int HpDamage {
        get {return _hpDamage;}
    }
    private bool _isDead = false;
    public bool IsDead {
        get {return _isDead;}
    }
    public int TargetIndex{
        get { return _target.Index;}
    }

    public void MakeResultData()
    {
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
        int AtkValue = _subject.Status.Atk;
        int DefValue = _target.Status.Def;
        float DamageValue = Mathf.Max(0,(featureData.Param1 * 0.01f * (AtkValue * 0.5f)) - (DefValue * 0.5f));
        _hpDamage = (int)Mathf.Round(DamageValue);
        // 属性補正
        // クリティカル
        _hpDamage = ApplyVariance(_hpDamage);
        if (_hpDamage >= _target.Hp)
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
