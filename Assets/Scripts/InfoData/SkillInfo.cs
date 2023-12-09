using System;
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
    public void SetEnable(bool IsEnable)
    {
        _enable = IsEnable;
    }
    public AttributeType Attribute {get {return Master.Attribute;}}

    private LearningState _learningState;
    public LearningState LearningState => _learningState;
    public void SetLearningState(LearningState learningState)
    {
        _learningState = learningState;
    }

    private int _learningCost = 0;
    public int LearningCost => _learningCost;
    public void SetLearningCost(int learningCost)
    {
        _learningCost = learningCost;
    }

    private List<SkillData.TriggerData> _triggerDates = new();
    public List<SkillData.TriggerData> TriggerDates => _triggerDates;

    private int _weight = 100;
    public int Weight => _weight;
    public void SetWeight(int weight)
    {
        _weight = weight;
    }
    private int _useCount = 0;
    public int UseCount => _useCount;
    public void SetUseCount(int useCount)
    {
        _useCount = useCount;
    }
    public void GainUseCount()
    {
        _useCount++;
    }
    public SkillInfo(int id)
    {
        _id = id;
        _learningState = LearningState.None;
    }
    
    public void SetTriggerDates(List<SkillData.TriggerData> triggerDates)
    {
        _triggerDates = triggerDates;
    }
    
    public void SetParam(string param1,int param2,int param3)
    {
        _param1 = param1;
        _param2 = param2;
        _param3 = param3;
    }
}
