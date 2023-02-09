using System;

[Serializable]
public class SkillInfo 
{
    private int _id;
    public int Id {get {return _id;}}
    

    public SkillInfo(int id)
    {
        _id = id;
    }
}
