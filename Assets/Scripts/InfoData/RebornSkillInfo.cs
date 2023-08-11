using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class RebornSkillInfo 
{
    public SkillsData.SkillData Master {get {return DataSystem.Skills.Find(a => a.Id == _id);}}
    private int _id;
    public int Id => _id;
    
    private int _param1 = 0;
    public int Param1 => _param1;
    private int _param2 = 0;
    public int Param2 => _param2;
    private int _param3 = 0;
    public int Param3 => _param3;

    public RebornSkillInfo(int id,int param1,int param2,int param3)
    {
        _id = id;
        _param1 = param1;
        _param2 = param2;
        _param3 = param3;
    }

    public string Name()
    {
        return Master.Name.Replace("\\d",_param1.ToString());
    }

    public string Help()
    {
        return Master.Help.Replace("\\d",_param1.ToString());
    }
}
