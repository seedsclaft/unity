using System;

[Serializable]
public class SkillInfo 
{
    public SkillsData.SkillData Master {get {return DataSystem.Skills.Find(a => a.Id == _id);}}
    private int _id;
    public int Id {get {return _id;}}
    
    private bool _enabel;
    public bool Enabel {get {return _enabel;}}
    public AttributeType Attribute {get {return Master.Attribute;}}
    public SkillInfo(int id)
    {
        _id = id;
    }

    public void SetEnable()
    {
        _enabel = true;
    }
    public void SetDisable()
    {
        _enabel = false;
    }
}
