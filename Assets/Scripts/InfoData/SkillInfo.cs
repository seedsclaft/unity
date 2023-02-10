using System;

[Serializable]
public class SkillInfo 
{
    
    public SkillsData.SkillData Master {get {return DataSystem.Skills.Find(a => a.Id == _id);}}
    private int _id;
    public int Id {get {return _id;}}
    

    public AttributeType Attribute {get {return Master.Attribute;}}
    public SkillInfo(int id)
    {
        _id = id;
    }
}
