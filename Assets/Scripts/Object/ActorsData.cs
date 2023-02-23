﻿using System;
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
        public int X;
        public int Y;
        public float Scale;
        public int AwakenX;
        public int AwakenY;
        public float AwakenScale;
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
}

[Serializable]
public class TextData
{   
    public int Id;
    public string Text;
    public string Help;
}