using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class SkillInfo 
{
    public SkillsData.SkillData Master {get {return DataSystem.Skills.Find(a => a.Id == _id);}}
    private int _id;
    public int Id {get {return _id;}}
    
    private bool _enable;
    public bool Enable {get {return _enable;}}
    private bool _interrupt;
    public bool Interrupt {get {return _interrupt;}}
    public AttributeType Attribute {get {return Master.Attribute;}}
    
    private int _learingCost;
    public int LearingCost {get {return _learingCost;}}
    private int _hintLv = -1;
    public int HintLv {get {return _hintLv;}}

    private LearningState _learningState;
    public LearningState LearningState {get {return _learningState;}}

    private List<SkillsData.TriggerData> _triggerDatas = new List<SkillsData.TriggerData>();
    public List<SkillsData.TriggerData> TriggerDatas {get {return _triggerDatas;}}

    private int _weight = 100;
    public int Weight {get {return _weight;}}

    public SkillInfo(int id)
    {
        _id = id;
        _interrupt = false;
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

    public void SetInterrupt(bool IsInterrupt)
    {
        _interrupt = IsInterrupt;
    }

    public void SetLearingCost(int learningCost)
    {
        _learingCost = learningCost;
    }
    
    public void SetHintLv(int hintLv)
    {
        _hintLv = hintLv;
    }

    public void SetLearningState(LearningState learningState)
    {
        _learningState = learningState;
    }

    public bool CanUseTrigger(BattlerInfo battlerInfo,List<BattlerInfo> party,List<BattlerInfo> troops)
    {
        if (_triggerDatas.Count == 0) return true;
        bool CanUse = true;
        foreach (var triggerData in _triggerDatas)
        {
            if (triggerData.CanUseTrigger(battlerInfo,party,troops) == false)
            {
                CanUse = false;
            }
        }
        return CanUse;
    }
}
