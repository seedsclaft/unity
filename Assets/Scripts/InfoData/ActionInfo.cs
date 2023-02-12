using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionInfo 
{
    private int _skillId = 0;
    
    private int _subjectIndex = 0;
    public int SubjectIndex {get {return _subjectIndex;}}
    
    private int _lastTargetIndex = 0;
    public int LastTargetIndex {get {return _lastTargetIndex;}}
    public SkillsData.SkillData Master {get {return DataSystem.Skills.Find(a => a.Id == _skillId);}}
    
    private RangeType _rangeType = RangeType.None;
    public RangeType RangeType {get {return _rangeType;}}
    private TargetType _targetType = TargetType.None;
    public TargetType TargetType {get {return _targetType;}}
    private ScopeType _scopeType = ScopeType.None;
    public ScopeType ScopeType {get {return _scopeType;}}

    private List<ActionResultInfo> _actionResult = new List<ActionResultInfo>();
    public List<ActionResultInfo> actionResults {get {return _actionResult;}}
    public ActionInfo(int skillId,int subjectIndex,int lastTargetIndex)
    {
        _skillId = skillId;
        _scopeType = Master.Scope;
        _rangeType = Master.Range;
        _targetType = Master.TargetType;
        _subjectIndex = subjectIndex;
        _lastTargetIndex = lastTargetIndex;
    }

    public void SetActionResult(List<ActionResultInfo> actionResult)
    {
        _actionResult = actionResult;
    }
}
