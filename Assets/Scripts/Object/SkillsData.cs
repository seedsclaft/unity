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
        public string IconPath;
        public int Mp;
        public AttributeType Attribute;
        public ScopeType Scope;
        public EffectType EffectType;
        public float EffectValue;
        
    }
}


public enum ScopeType{
    None = 0,
    OneEnemy = 1,
    AllEnemy = 2,
    OneParty = 3,
    AllParty = 4,
    Self = 5
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
