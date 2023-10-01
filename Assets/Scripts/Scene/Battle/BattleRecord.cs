using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleRecord 
{
    private ActionResultInfo _resultInfo;
    private ActionInfo _actionInfo;
    public BattleRecord(ActionInfo actionInfo)
    {
        _actionInfo = actionInfo;
        //Debug.Log(_actionInfo.Master.Name);
    }

    public BattleRecord(ActionResultInfo resultInfo)
    {
        _resultInfo = resultInfo;
        //Debug.Log("subject " + resultInfo.SubjectIndex);
        //Debug.Log("target " + resultInfo.TargetIndex);
    }
}
