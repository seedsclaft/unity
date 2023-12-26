using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class AlcanaInfo{
    private bool _IsAlcana;
    public bool IsAlcana {get {return _IsAlcana;}}
    private List<SkillInfo> _ownAlcanaList = new ();
    public List<SkillInfo> OwnAlcanaList {get {return _ownAlcanaList;}}
    private StateInfo _alcanaStateInfo = null;
    public StateInfo AlcanaState {get {return _alcanaStateInfo;}}

    public AlcanaInfo(){
        InitData();
    }

    public void InitData()
    {
        _IsAlcana = false;
        _ownAlcanaList.Clear();
        _ownAlcanaList.Add(new SkillInfo(500001));
    }

    public List<SkillInfo> CheckAlcanaSkillInfo(TriggerTiming triggerTiming)
    {
        return _ownAlcanaList.FindAll(a => a.Master.TriggerDates.Find(b => b.TriggerTiming == triggerTiming) != null);
    }

    public void AddAlcana(SkillInfo skillInfo)
    {
        _ownAlcanaList.Add(skillInfo);
    }

    public void ReleaseAlcana(SkillInfo skillInfo)
    {
        var findIndex = _ownAlcanaList.FindIndex(a => a == skillInfo);
        if (findIndex > -1)
        {
            _ownAlcanaList.RemoveAt(findIndex);
        }
    }

    public void SetIsAlcana(bool isAlcana)
    {
        _IsAlcana = isAlcana;
    }

    public void SetAlcanaStateInfo(StateInfo stateInfo)
    {
        _alcanaStateInfo = stateInfo;
    }



}
