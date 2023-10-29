using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ActorDatas : ScriptableObject
{
    [SerializeField] public List<ActorData> Data = new();
}

[Serializable]
public class ActorData 
{   
    public int Id;
    public string Name;
    public string SubName;
    public int ClassId;
    public string ImagePath;
    public int InitLv;
    public int MaxLv;
    public StatusInfo InitStatus;
    public StatusInfo PlusStatus;
    public StatusInfo NeedStatus;
    public List<int> Attribute;
    public int X;
    public int Y;
    public float Scale;
    public int AwakenX;
    public int AwakenY;
    public float AwakenScale;
    public List<LearningData> LearningSkills = new();
}

[Serializable]
public class LearningData
{   
    public int SkillId;
    public int Level;
    public int Weight;
    public List<SkillData.TriggerData> TriggerDatas;
}
