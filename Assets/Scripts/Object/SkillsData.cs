using System;
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
        public SkillType SkillType;
        public TargetType TargetType;
        public string Help;
        public RangeType Range;
        public bool AliveOnly;

        public List<FeatureData> FeatureDatas;
        public List<TriggerData> TriggerDatas;
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
    
    [Serializable]
    public class TriggerData
    {   
        public int SkillId;
        public TriggerType TriggerType;
        public TriggerTiming TriggerTiming;
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

public enum SkillType{
    None = 0,
    Magic = 1, // 通常スキル
    Passive = 2, // パッシブ
    Demigod = 3, // 神化
    Awaken = 4, // 覚醒
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
    Self = 4,
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
    MpHeal = 4,
    State = 5,

}

public enum TriggerType
{
    None = 0,
    AfterMp = 101, // Mpが〇になる
    ChainCount = 102, // 拘束成功回数
    ActionResultDeath = 103, // 攻撃を受けると戦闘不能になる
    DeadWithoutSelf = 104, // 自身以外が戦闘不能
    SelfDead = 105
}

public enum TriggerTiming
{
    None = 0,
    Use = 1,
    After = 2,
    Interrupt = 3
}

public enum FeatureType
{
    None = 0,
    HpDamage = 1,
    HpHeal = 2,
    HpDrain = 3,
    NoEffectHpDamage = 11,
    AddState = 21,
    RemoveState = 22,
    PlusSkill = 101,
    KindHeal = 201,
}