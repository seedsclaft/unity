using System;

[Serializable]
public class SkillInfo 
{
    private int _skillId;
    public int SkillId {get {return _skillId;}}
    
    public SkillInfo(int skillId)
    {
        _skillId = skillId;
    }
}
