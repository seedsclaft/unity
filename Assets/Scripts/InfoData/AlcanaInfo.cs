using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class AlcanaInfo{
    private List<SkillInfo> _currentTurnAlcanaList = new ();
    public List<SkillInfo> CurrentTurnAlcanaList => _currentTurnAlcanaList;
    public void SetCurrentTurnAlcanaList(List<SkillInfo> currentTurnAlcanaList)
    {
        foreach (var currentTurnAlcana in currentTurnAlcanaList)
        {
            if (!_currentTurnAlcanaList.Contains(currentTurnAlcana))
            {
                _currentTurnAlcanaList.Add(currentTurnAlcana);
            }
        }
    }
    public void ClearCurrentTurnAlcanaList()
    {
        _currentTurnAlcanaList.Clear();
    }
    private bool _IsAlcana;
    public bool IsAlcana {get {return _IsAlcana;}}
    private List<SkillInfo> _ownAlcanaList = new ();
    public List<SkillInfo> OwnAlcanaList {get {return _ownAlcanaList;}}
    public List<SkillInfo> EnableOwnAlcanaList {get {return _ownAlcanaList.FindAll(a => a.Enable);}}
    private StateInfo _alcanaStateInfo = null;
    public StateInfo AlcanaState {get {return _alcanaStateInfo;}}

    public AlcanaInfo(){
        InitData();
    }

    public void InitData()
    {
        _IsAlcana = false;
        _ownAlcanaList.Clear();
        for (var i = 500001;i <= 500002;i++)
        {
            //_ownAlcanaList.Add(new SkillInfo(i));
        }
    }

    public void ClearOwnAlcanaList()
    {
        _ownAlcanaList.Clear();
    }

    public List<SkillInfo> CheckAlcanaSkillInfo(TriggerTiming triggerTiming)
    {
        return _ownAlcanaList.FindAll(a => a.Enable && a.Master.TriggerDates.Find(b => b.TriggerTiming == triggerTiming) != null);
    }

    public void AddAlcana(SkillInfo skillInfo)
    {
        _ownAlcanaList.Add(skillInfo);
    }

    public void DisableAlcana(SkillInfo skillInfo)
    {
        skillInfo.SetEnable(false);
    }

    public void DeleteAlcana(SkillInfo skillInfo)
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

    public bool CheckAlchemyCostZero(AttributeType attributeType)
    {
        return _currentTurnAlcanaList.Find(a => a.Master.FeatureDates.Find(b => b.FeatureType == FeatureType.AlchemyCostZero && b.Param1 == (int)attributeType) != null) != null;
    }

    public bool CheckNoBattleLost()
    {
        return _currentTurnAlcanaList.Find(a => a.Master.FeatureDates.Find(b => b.FeatureType == FeatureType.NoBattleLost) != null) != null;
    }

    public bool CheckResourceBonus()
    {
        return _currentTurnAlcanaList.Find(a => a.Master.FeatureDates.Find(b => b.FeatureType == FeatureType.ResourceBonus) != null) != null;
    }
    
    public bool CheckCommandCostZero(TacticsCommandType tacticsCommandType)
    {
        return _currentTurnAlcanaList.Find(a => a.Master.FeatureDates.Find(b => b.FeatureType == FeatureType.CommandCostZero && b.Param1 == (int)tacticsCommandType) != null) != null;
    }

    public bool CheckAlchemyValue()
    {
        return _currentTurnAlcanaList.Find(a => a.Master.FeatureDates.Find(b => b.FeatureType == FeatureType.AlchemyCostBonus) != null) != null;
    }
}
