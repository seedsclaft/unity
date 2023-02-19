using System;

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
    public SkillInfo(int id)
    {
        _id = id;
        _interrupt = false;
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
}
