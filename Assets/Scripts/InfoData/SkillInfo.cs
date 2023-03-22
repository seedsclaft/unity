using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class SkillInfo 
{
    public SkillsData.SkillData Master {get {return DataSystem.Skills.Find(a => a.Id == _id);}}
    private int _id;
    public int Id {get {return _id;}}
    
    private bool _enabel;
    public bool Enabel {get {return _enabel;}}
    private bool _interrupt;
    public bool Interrupt {get {return _interrupt;}}
    public AttributeType Attribute {get {return Master.Attribute;}}
    
    private int _learingCost;
    public int LearingCost {get {return _learingCost;} set {_learingCost = value;}}

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

    public void SetEnable()
    {
        _enabel = true;
    }

    public void SetDisable()
    {
        _enabel = false;
    }

    public void SetInterrupt(bool IsInterrupt)
    {
        _interrupt = IsInterrupt;
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
                if (troops.FindAll(a => !a.IsState(StateType.Death)).Count > triggerData.Param1)
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
