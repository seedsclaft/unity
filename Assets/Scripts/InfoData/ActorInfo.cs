using System;

[Serializable]
public class ActorInfo
{
    private int _actorId;
    public int ActorId {get {return _actorId;}}
    private int _exp;
    private int _hp;
    public int Hp {get {return _hp;}}

    public ActorInfo(ActorsData.ActorData actorData)
    {
        _actorId = actorData.Id;
        _exp = (actorData.InitLv - 1) * 100;
        _hp = actorData.InitStatus.Hp;
    }

    public int Level { get{return _exp / 100;}}

    private void GainExp(int exp)
    {
        _exp += exp; 
    }

};
