using System.Collections;
using System.Collections.Generic;

public class ActionInfo 
{
    private int _index;
    private int _skillId = 0;
    
    private int _subjectIndex = 0;
    public int SubjectIndex => _subjectIndex;

    private int _lastTargetIndex = 0;
    public int LastTargetIndex => _lastTargetIndex;
    public SkillData Master {get {return DataSystem.Skills.Find(a => a.Id == _skillId);}}
    
    private RangeType _rangeType = RangeType.None;
    public RangeType RangeType => _rangeType;
    private TargetType _targetType = TargetType.None;
    public TargetType TargetType => _targetType;
    private ScopeType _scopeType = ScopeType.None;
    public ScopeType ScopeType => _scopeType;

    private List<ActionResultInfo> _actionResult = new ();
    public List<ActionResultInfo> ActionResults => _actionResult;

    private int _mpCost;
    public int MpCost => _mpCost;
    private List<int> _targetIndexList;
    public List<int> TargetIndexList => _targetIndexList;

    private bool _triggeredSkill = false;
    public bool TriggeredSkill => _triggeredSkill;

    public ActionInfo(int index,int skillId,int subjectIndex,int lastTargetIndex,List<int> targetIndexList)
    {
        _index = index;
        _skillId = skillId;
        _scopeType = Master.Scope;
        _rangeType = Master.Range;
        _targetType = Master.TargetType;
        _subjectIndex = subjectIndex;
        _lastTargetIndex = lastTargetIndex;
        _targetIndexList = targetIndexList;
    }

    public void SetRangeType(RangeType rangeType)
    {
        _rangeType = rangeType;
    }

    public void SetMpCost(int mpCost)
    {
        _mpCost = mpCost;
    }

    public void SetActionResult(List<ActionResultInfo> actionResult)
    {
        _actionResult = actionResult;
    }

    public List<ActionInfo> CheckPlusSkill()
    {
        var featureDates = Master.FeatureDates;
        var PlusSkill = featureDates.FindAll(a => a.FeatureType == FeatureType.PlusSkill);
        
        var actionInfos = new List<ActionInfo>();
        for (var i = 0;i < PlusSkill.Count;i++){
            var skillInfo = new SkillInfo(PlusSkill[i].Param1);
            var actionInfo = new ActionInfo(_index,skillInfo.Id,SubjectIndex,-1,null);
            actionInfo.SetTriggerSkill(true);
            actionInfos.Add(actionInfo);
        }
        return actionInfos;
    }

    public void SetTriggerSkill(bool triggeredSkill)
    {
        _triggeredSkill = triggeredSkill;
    }
}
