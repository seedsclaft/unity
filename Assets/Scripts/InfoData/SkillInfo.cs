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
    private bool _forget;
    public bool Forget {get {return _forget;}}
    public AttributeType Attribute {get {return Master.Attribute;}}
    
    private int _learingCost;
    public int LearingCost {get {return _learingCost;}}

    private List<SkillsData.TriggerData> _triggerDatas = new List<SkillsData.TriggerData>();
    public List<SkillsData.TriggerData> TriggerDatas {get {return _triggerDatas;}}

    private int _weight = 100;
    public int Weight {get {return _weight;}}

    public SkillInfo(int id)
    {
        _id = id;
        _interrupt = false;
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
    
    public void SetForget(bool forget)
    {
        _forget = forget;
    }

    public void SetLearingCost(int learningCost)
    {
        _learingCost = learningCost;
    }

    public bool CanUseTrigger(BattlerInfo battlerInfo,List<BattlerInfo> party,List<BattlerInfo> troops)
    {
        if (_triggerDatas.Count == 0) return true;
        bool CanUse = true;
        foreach (var triggerData in _triggerDatas)
        {
            switch (triggerData.TriggerType)
            {
                case TriggerType.IsExistDeathMember:
                if (troops.FindAll(a => a.IsState(StateType.Death)).Count < triggerData.Param1)
                {
                    CanUse = false;
                }
                break;
                case TriggerType.IsExistAliveMember:
                if (troops.FindAll(a => !a.IsState(StateType.Death)).Count < triggerData.Param1)
                {
                    CanUse = false;
                }
                break;
                case TriggerType.TurnNumPer:
                if ((battlerInfo.TurnCount % triggerData.Param1) - triggerData.Param2 != 0)
                {
                    CanUse = false;
                }
                break;
                case TriggerType.HpRateUnder:
                if (((float)battlerInfo.Hp / (float)battlerInfo.MaxHp) > triggerData.Param1 * 0.01f)
                {
                    CanUse = false;
                }
                break;
                case TriggerType.HpRateUpper:
                if (((float)battlerInfo.Hp / (float)battlerInfo.MaxHp) < triggerData.Param1 * 0.01f)
                {
                    CanUse = false;
                }
                break;
                case TriggerType.PartyHpRateUnder:
                var filter = troops.FindAll(a => ((float)battlerInfo.Hp / (float)battlerInfo.MaxHp) <= triggerData.Param1 * 0.01f);
                if (filter.Count == 0)
                {
                    CanUse = false;
                }
                break;
            }
        }


        return CanUse;
    }
}
