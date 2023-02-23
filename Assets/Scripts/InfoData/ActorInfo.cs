using System;
using System.Collections.Generic;

[Serializable]
public class ActorInfo
{
    private int _actorId;
    public int ActorId {get {return _actorId;}}
    public int MaxHp { get
        {
            return DataSystem.GetActor(_actorId).CurrentParam(StatusParamType.Hp,Level);
        }
    }

    private int _level;
    public int Level {get {return _level;}}
    
    private StatusInfo _status;
    public StatusInfo Status {get {return _status;}}
    private StatusInfo _usePoint;
    public StatusInfo UsePoint {get {return _usePoint;}}
    private List<int> _attribute;
    public List<int> Attribute {get {return _attribute;}}
    private List<SkillInfo> _skills;
    public List<SkillInfo> Skills {get {return _skills;}}


    private int _hp;
    public int Hp {get {return _hp;}}
    private int _mp;
    public int Mp {get {return _mp;}}
    private int _ap;
    public int Ap {get {return _ap;}}

    private TacticsComandType _tacticsComandType = TacticsComandType.None;
    public TacticsComandType TacticsComandType {get {return _tacticsComandType;}}


    public ActorInfo(ActorsData.ActorData actorData)
    {
        _actorId = actorData.Id;
        _status = actorData.InitStatus;
        _usePoint = actorData.NeedStatus;
        _attribute = actorData.Attribute;
        _hp = _status.Hp;
        _mp = _status.Mp;
    }

    public void InitSkillInfo(List<LearningData> learningData)
    {
        _skills = new List<SkillInfo>();
        for (int i = 0;i < learningData.Count;i++)
        {
            LearningData _learningData = learningData[i];
            if (_skills.Find(a =>a.Id == _learningData.SkillId) != null) continue;
            SkillInfo skillInfo = new SkillInfo(_learningData.SkillId);
            _skills.Add(skillInfo);
        }
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

    public void UpdateStatus()
    {
    }

    public void SetTacticsCommand(TacticsComandType tacticsComandType)
    {
        _tacticsComandType = tacticsComandType;
    }
    public void ClearTacticsCommand()
    {
        _tacticsComandType = TacticsComandType.None;
    }
}
