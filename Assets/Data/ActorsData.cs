using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorsData : ScriptableObject {
    [SerializeField] public List<ActorData> _data = new List<ActorData>();


    [Serializable]
    public class ActorData
    {   
        public int Id;
        public string Name;
        public int ClassId;
        public string ImagePath;
        public int InitLv;
        public StatusInfo InitStatus;
        public StatusInfo GrowthRateStatus;
        

        public string GetName()
        {
            string name = Name;
            return name;
        }

    }

}

[Serializable]
public class ActorInfo
{
    private int _actorId;
    public int ActorId {get {return _actorId;}}
    private int _level;
    private int _exp;
    private StatusInfo _plusStatus;

    public ActorInfo(ActorsData.ActorData actorInfo)
    {
        _actorId = actorInfo.Id;
        _level = actorInfo.InitLv;
        _exp = (actorInfo.InitLv - 1) * 100;
        _plusStatus = new StatusInfo();
    }

    public StatusInfo LevelUp(StatusInfo growStatus,StatusInfo baseStatus,StatusInfo maxStatus)
    {
        _level++;
        return LevelUpStatus(growStatus,baseStatus,maxStatus);
    }

    private StatusInfo LevelUpStatus(StatusInfo growStatus,StatusInfo baseStatus,StatusInfo maxStatus)
    {
        StatusInfo upStatus = new StatusInfo();
        foreach (StatusParamType growType in Enum.GetValues(typeof(StatusParamType)))
        {
            int currentParam = baseStatus.GetParameter(growType) + _plusStatus.GetParameter(growType);
            int maxParam = maxStatus.GetParameter(growType);
            if (currentParam < maxParam){
                int growParam = growStatus.GetParameter(growType);
                if (IsStatusUp(growParam)){
                    _plusStatus.AddParameter(growType,1);
                    upStatus.AddParameter(growType,1);
                }
            }
        }
        return upStatus;
    }

    private bool IsStatusUp(int growParam)
    {
        int rate = new System.Random().Next(0,100);
        return (growParam >= rate);
    }

};

[Serializable]
public class StatusInfo
{
    public int _hp = 0;
    public int _str = 0;
    public int _mag = 0;
    public int _tec = 0;
    public int _spd = 0;
    public int _luc = 0;
    public int _def = 0;
    public int _res = 0;
    public int _mov = 0;
    public int Hp {get { return _hp;}}
    public int Atk {get { return _str;}}
    public int Tec {get { return _tec;}}
    public int Spd {get { return _spd;}}
    public int Luc {get { return _luc;}}
    public int Def {get { return _def;}}
    public int Res {get { return _res;}}
    public int Mov {get { return _mov;}}

    public void SetParameter(int hp,int str,int mag,int tec,int spd,int luc,int def,int res,int mov)
    {
        _hp = hp;
        _str = str;
        _mag = mag;
        _tec = tec;
        _spd = spd;
        _luc = luc;
        _def = def;
        _res = res;
        _mov = mov;
    }

    public int GetParameter(StatusParamType paramType)
    {
        switch (paramType)
        {
            case StatusParamType.Hp: return _hp;
            case StatusParamType.Str: return _str;
            case StatusParamType.Mag: return _mag;
            case StatusParamType.Tec: return _tec;
            case StatusParamType.Spd: return _spd;
            case StatusParamType.Luc: return _luc;
            case StatusParamType.Def: return _def;
            case StatusParamType.Res: return _res;
            case StatusParamType.Mov: return _mov;
        }
        return 0;
    }
    
    public void AddParameter(StatusParamType paramType,int param)
    {
        switch (paramType)
        {
            case StatusParamType.Hp: _hp += param; break;
            case StatusParamType.Str: _str += param; break;
            case StatusParamType.Mag: _mag += param; break;
            case StatusParamType.Tec: _tec += param; break;
            case StatusParamType.Spd: _spd += param; break;
            case StatusParamType.Luc: _luc += param; break;
            case StatusParamType.Def: _def += param; break; 
            case StatusParamType.Res: _res += param; break; 
            case StatusParamType.Mov: _mov += param; break;
        }
    }
}

public enum StatusParamType
{
    Hp = 0,
    Str,
    Mag,
    Tec,
    Spd,
    Luc,
    Def,
    Res,
    Mov
}


[Serializable]
public class WeaponRankInfo
{
    public int _sword = 0;
    public int _lance = 0;
    public int _axe = 0;
    public int _bow = 0;
    public int _knive = 0;
    public int _strike = 0;
    public int _fire = 0;
    public int _thunder = 0;
    public int _wind = 0;
    public int _light = 0;
    public int _dark = 0;
    public int _strve = 0;
    public int Sword {get { return _sword;}}
    public int Lance {get { return _lance;}}
    public int Axe {get { return _axe;}}
    public int Bow {get { return _bow;}}
    public int Knive {get { return _knive;}}
    public int Strike {get { return _strike;}}
    public int Fire {get { return _fire;}}
    public int Thunder {get { return _thunder;}}
    public int Wind {get { return _wind;}}
    public int Light {get { return _light;}}
    public int Dark {get { return _dark;}}
    public int Strve {get { return _strve;}}

    public void SetParameter(int sword,int lance,int axe,int bow,int knive,int strike,int fire,int thunder,int wind,int light,int dark,int strve)
    {
        _sword = sword;
        _lance = lance;
        _axe = axe;
        _bow = bow;
        _knive = knive;
        _strike = strike;
        _fire = fire;
        _thunder = thunder;
        _wind = wind;
        _light = light;
        _dark = dark;
        _strve = strve;
    }
}