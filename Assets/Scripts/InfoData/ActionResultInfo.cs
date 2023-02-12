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
    }

    public void MakeResultData()
    {
        if (_actionInfo.Master.EffectType == EffectType.Attack)
        {
            
        }
    }
}
