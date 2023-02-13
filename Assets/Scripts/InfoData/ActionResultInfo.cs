using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionResultInfo 
{
    private BattlerInfo _subject = null;
    private BattlerInfo _target = null;
    private ActionInfo _actionInfo = null;
    public ActionResultInfo(BattlerInfo subject,BattlerInfo target,ActionInfo actionInfo)
    {
        _subject = subject;
        _target = target;
        _actionInfo = actionInfo;
        MakeResultData();
    }

    private int _hpDamage = 0;

    public void MakeResultData()
    {
        List<SkillsData.FeatureData> featureDatas = _actionInfo.Master.FeatureDatas;
        for (int i = 0; i < featureDatas.Count; i++)
        {
            ExecFeature(featureDatas[i]);
        }
    }

    private void ExecFeature(SkillsData.FeatureData featureData)
    {
        switch (featureData.FeatureType)
        {
            case FeatureType.HpDamage:
                ExecHpDamage(featureData);
                return;

        }
    }

    private void ExecHpDamage(SkillsData.FeatureData featureData)
    {
        int AtkValue = _subject.Status.Atk;
        int DefValue = _target.Status.Def;
        float DamageValue = Mathf.Max(0,(featureData.Param1 * 0.01f * (AtkValue * 0.5f)) - (DefValue * 0.5f));
        _hpDamage = (int)Mathf.Round(DamageValue);
    }
}
