using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class SkillInfo 
{
    public SkillsData.SkillData Master {get {return DataSystem.Skills.Find(a => a.Id == _id);}}
    private int _id;
    public int Id => _id;
    
    private bool _enable;
    public bool Enable => _enable;
    public AttributeType Attribute {get {return Master.Attribute;}}

    private LearningState _learningState;
    public LearningState LearningState => _learningState;

    private AttributeType _learnAttributeType;
    public AttributeType LearnAttributeType => _learnAttributeType;

    private List<SkillsData.TriggerData> _triggerDatas = new();
    public List<SkillsData.TriggerData> TriggerDatas => _triggerDatas;

    private int _weight = 100;
    public int Weight => _weight;
    private bool _isUsed = false;
    public bool IsUsed => _isUsed;
    public SkillInfo(int id)
    {
        _id = id;
        _learningState = LearningState.None;
    }
    
    public void SetTriggerDatas(List<SkillsData.TriggerData> triggerDatas)
    {
        _triggerDatas = triggerDatas;
    }
    
    public void SetWeight(int weight)
    {
        _weight = weight;
    }

    public void SetEnable(bool IsEnable)
    {
        _enable = IsEnable;
    }

    public void SetLearningState(LearningState learningState)
    {
        _learningState = learningState;
    }

    public void SetLearnAttribute(AttributeType attributeType)
    {
        _learnAttributeType = attributeType;
    }

    public void SetIsUsed(bool isUsed)
    {
        _isUsed = isUsed;
    }
}
