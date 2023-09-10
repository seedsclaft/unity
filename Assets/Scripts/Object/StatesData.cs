using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatesData : ScriptableObject
{
    [SerializeField] public List<StateData> _data = new();
    
    [Serializable]
    public class StateData
    {   
        public int Id;
        public string Name;
        public string Help;
        public string IconPath;
        public RemovalTiming RemovalTiming;
        public bool OverWrite;
        public string EffectPath;
        public EffectPositionType EffectPosition;
    }
}

public enum StateType
{
    None = 0,
    Death = 1,
    Demigod = 11,
    Slow = 21,
    Stun = 22,
    SlipDamage = 23,
    Blind = 24,
    Freeze = 26,
    Barrier = 31,
    Extension = 32,
    MaxHpUp = 41,
    MaxMpUp = 42,
    AtkUp = 43,
    DefUp = 44,
    DamageUp = 46,
    CriticalRateUp = 47,
    EvaUp = 52,
    Accel = 53,
    TargetRateUp = 61,
    TargetRateDown = 62,
    RemoveBuff = 71,
    DefDown = 84,
    Chain = 101,
    ChainDamageUp = 102,
    CounterOura = 103,
    NoDamage = 104,
    Regene = 105,
    SetAfterAp = 106,
    Substitute = 107,
    Benediction  = 109,
    Curse  = 110,
    Drain  = 111,
    AfterHeal = 112,
    CounterOuraDamage = 113,
    Deadly = 114,
    CounterOuraHeal = 115,
    HealActionSelfHeal = 116,
    Undead = 117,
    AbsoluteHit = 118,
    AntiDote = 119,
    Prizm = 120,
    NoPassive = 121,
}

public enum RemovalTiming
{
    None = 0,
    UpdateTurn = 1,
    UpdateAp = 2,
    UpdateChain = 3,
    UpdateCount = 4,
}

public enum EffectPositionType
{
    Center = 0,
    Down = 1
}