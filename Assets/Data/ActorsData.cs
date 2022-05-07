using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorsData : ScriptableObject {
    [SerializeField] public List<ActorData> _data = new List<ActorData>();
    [SerializeField] public List<TextData> _textdata = new List<TextData>();


    [Serializable]
    public class ActorData
    {   
        public int Id;
        public int NameId;
        public int ClassId;
        public string ImagePath;
        public int InitLv;
        public int MaxLv;
        public StatusInfo InitStatus;
        public StatusInfo MaxStatus;
        
        public int CurrentParam(StatusParamType growType,int level)
        {
            int init = InitStatus.GetParameter(growType);
            int max = MaxStatus.GetParameter(growType);
            float per = MaxLv - InitLv;
            float upParam = ((max - init) / per) * level;
            return init + (int)upParam;
        }

        public StatusInfo LevelUpStatus(int level)
        {
            StatusInfo upStatus = new StatusInfo();
            foreach (StatusParamType growType in Enum.GetValues(typeof(StatusParamType)))
            {
                int currentParam = CurrentParam(growType,level);
                int nextParam = CurrentParam(growType,level + 1);
                if (currentParam < nextParam){
                    int upParam = nextParam - currentParam;
                    upStatus.AddParameter(growType,upParam);
                }
            }
            return upStatus;
        }
    }

}

[Serializable]
public class ActorInfo
{
    private int _actorId;
    public int ActorId {get {return _actorId;}}
    private int _exp;

    public ActorInfo(ActorsData.ActorData actorData)
    {
        _actorId = actorData.Id;
        _exp = (actorData.InitLv - 1) * 100;
    }

    private int GetLevel()
    {
        return _exp / 100;
    }

    private void GainExp(int exp)
    {
        _exp += exp; 
    }

};

[Serializable]
public class StatusInfo
{
    public int _hp = 0;
    public int _mp = 0;
    public int _atk = 0;
    public int _spd = 0;
    public int Hp {get { return _hp;}}
    public int Mp {get { return _mp;}}
    public int Atk {get { return _atk;}}
    public int Spd {get { return _spd;}}
    public void SetParameter(int hp,int mp,int atk,int spd)
    {
        _hp = hp;
        _mp = mp;
        _atk = atk;
        _spd = spd;
    }

    public int GetParameter(StatusParamType paramType)
    {
        switch (paramType)
        {
            case StatusParamType.Hp: return _hp;
            case StatusParamType.Mp: return _mp;
            case StatusParamType.Atk: return _atk;
            case StatusParamType.Spd: return _spd;
        }
        return 0;
    }
    
    public void AddParameter(StatusParamType paramType,int param)
    {
        switch (paramType)
        {
            case StatusParamType.Hp: _hp += param; break;
            case StatusParamType.Mp: _mp += param; break;
            case StatusParamType.Atk: _atk += param; break;
            case StatusParamType.Spd: _spd += param; break;
        }
    }
}

public enum StatusParamType
{
    Hp = 0,
    Mp,
    Atk,
    Spd,
}

[Serializable]
public class TextData
{   
    public int Id;
    public string Text;
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