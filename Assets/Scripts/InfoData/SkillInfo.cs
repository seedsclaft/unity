using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class SkillInfo 
{
    public SkillData Master {get {return DataSystem.Skills.Find(a => a.Id == _id);}}
    private int _id;
    public int Id => _id;
    private int _deckIndex = -1;
    public int DeckIndex => _deckIndex;
    public void SetDeckIndex(int index)
    {
        _deckIndex = index;
    }

    private string _param1 = "";
    public string Param1 => _param1;
    private int _param2 = 0;
    public int Param2 => _param2;
    private int _param3 = 0;
    public int Param3 => _param3;

    private bool _enable;
    public bool Enable => _enable;
    public AttributeType Attribute {get {return Master.Attribute;}}

    private LearningState _learningState;
    public LearningState LearningState => _learningState;

    private AttributeType _learnAttributeType;
    public AttributeType LearnAttributeType => _learnAttributeType;

    private List<SkillData.TriggerData> _triggerDatas = new();
    public List<SkillData.TriggerData> TriggerDatas => _triggerDatas;

    private int _weight = 100;
    public int Weight => _weight;
    private bool _isUsed = false;
    public bool IsUsed => _isUsed;
    public SkillInfo(int id)
    {
        _id = id;
        _learningState = LearningState.None;
    }
    
    public void SetTriggerDatas(List<SkillData.TriggerData> triggerDatas)
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
    
    public void SetParam(string param1,int param2,int param3)
    {
        _param1 = param1;
        _param2 = param2;
        _param3 = param3;
    }
}
