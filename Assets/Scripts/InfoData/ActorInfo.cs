using System;

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




    public ActorInfo(ActorsData.ActorData actorData)
    {
        _actorId = actorData.Id;
        _exp = (actorData.InitLv - 1) * 100;
        _status = actorData.InitStatus;
    }

    public int Level { get{return _exp / 100;}}

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