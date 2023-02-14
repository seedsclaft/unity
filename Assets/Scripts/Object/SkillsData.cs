﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillsData : ScriptableObject {
    [SerializeField] public List<SkillData> _data = new List<SkillData>();
    [Serializable]
    public class SkillData
    {   
        public int Id;
        public string Name;
        public int IconIndex;
        public string AnimationName;
        public AnimationType AnimationType;
        public int DamageTiming;
        public int MpCost;
        public AttributeType Attribute;
        public ScopeType Scope;
        public EffectType EffectType;
        public float EffectValue;
        public TargetType TargetType;
        public string Help;
        public RangeType Range;

        public int TriggerType;
        public int TriggerValue;
        public List<FeatureData> FeatureDatas;
    }
    [Serializable]
    public class FeatureData
    {   
        public int SkillId;
        public FeatureType FeatureType;
        public int Param1;
        public int Param2;
        public int Param3;
    }
}

public enum AnimationType
{
    None = 0,
    One = 1,
    Line = 2,
    All = 3
}

public enum ScopeType{
    None = 0,
    One = 1,
    Line = 2,
    All = 3,
    Self = 4,
}


public enum EffectType{
    None = 0,
    Attack = 1, // 攻撃
}

public enum AttributeType{
    None = 0,
    Fire = 1,
    Thunder = 2,
    Ice = 3,
    White = 4,
    Black = 5
}

public enum TargetType{
    None = 0,
    Opponent = 1,
    Friend = 2,
    All = 3,
    Self = 4
}

public enum RangeType{
    None = 0,
    S = 1,
    L = 2
}
public enum DamageType
{
    None = 0,
    HpDamage = 1,
    HpCritical = 2,
    HpHeal = 3,    
    MpHeal = 4
}