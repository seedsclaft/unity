using System;
using System.Collections.Generic;

[Serializable]
public class ActorInfo
{
    private int _actorId;
    public int ActorId {get {return _actorId;}}
    private int _exp;
    public int MaxHp { get
        {
            return DataSystem.GetActor(_actorId).CurrentParam(StatusParamType.Hp,Level);
        }
    }
    
    private StatusInfo _status;
    public StatusInfo Status {get {return _status;}}
    public int Hp {get {return _status.Hp;}}
    private List<SkillInfo> _skills;
    public List<SkillInfo> Skills {get {return _skills;}}


    public ActorInfo(ActorsData.ActorData actorData)
    {
        _actorId = actorData.Id;
        _exp = (actorData.InitLv - 1) * 100;
        _status = actorData.InitStatus;
    }

    public void InitSkillInfo(List<LearningData> learningData)
    {
        _skills = new List<SkillInfo>();
        for (int i = 0;i < learningData.Count;i++)
        {
            LearningData _learningData = learningData[i];
            if (_skills.Find(a =>a.SkillId == _learningData.SkillId) != null) continue;
            if (_learningData.LearningType == LearningType.Level)
            {
                if (Level >= _learningData.Value)
                {
                    SkillInfo skillInfo = new SkillInfo(_learningData.SkillId);
                    _skills.Add(skillInfo);
                }
            }
        }
    }

    public int Level { get{return 1 + (_exp / 100);}}

    private void GainExp(int exp)
    {
        _exp += exp; 
    }

    public int GainHp(int value)
    {
        int tempHp = _status._hp + value;
        int gainHp = tempHp < 0 ? _status._hp * -1 : value;
        gainHp = tempHp > MaxHp ? MaxHp - value : gainHp;
        _status._hp += gainHp;
        CheckParameter();
        return gainHp;
    }

    public void CheckParameter()
    {
        _status._hp = Math.Max(0,MaxHp);
    }

}