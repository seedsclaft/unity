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
        public int MaxLv;
        public StatusInfo InitStatus;
        public StatusInfo PlusStatus;
        public StatusInfo NeedStatus;
        public List<int> Attribute;
        public List<LearningData> LearningSkills = new List<LearningData>();
        
        public int CurrentParam(StatusParamType growType,int level)
        {
            int init = InitStatus.GetParameter(growType);
            return init;
        }

    }

}

[Serializable]
public class LearningData
{   
    public int SkillId;
    public LearningType LearningType;
    public int Value;
}
public enum LearningType{
    None = 0,
    Level = 1
}

[Serializable]
public class TextData
{   
    public int Id;
    public string Text;
    public string Help;
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