using System;
using System.Collections.Generic;
using UnityEngine;

public class StateDates : ScriptableObject
{
    [SerializeField] public List<StateData> Data = new();
}

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
    public bool OverLap;
    public bool Removal;
    public bool Abnormal;
    public bool RemoveByAttack;
    // 付与者が戦闘不能になった時に効果が切れるか
    public bool RemoveByDeath;
}


public enum StateType
{
    None = 0,
    Death = 1,
    Demigod = 11,
    Slow = 21,
    Stun = 22,
    BurnDamage = 2001, // 火傷
    Chain = 2002, // 拘束
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
    AtkDown = 82,
    DefDown = 84,
    DefPerDown = 85,
    ChainDamageUp = 102,
    CounterAura = 103,
    NoDamage = 104,
    Regenerate = 105,
    SetAfterAp = 106,
    Substitute = 107,
    Benediction  = 109,
    Curse  = 110,
    Drain  = 111,
    AfterHeal = 112,
    CounterAuraDamage = 113,
    Deadly = 114,
    CounterAuraHeal = 115,
    HealActionSelfHeal = 116,
    Undead = 117,
    AbsoluteHit = 118,
    AntiDote = 119,
    Prism = 120,
    NoPassive = 121,
    AssistHeal = 122,
    RevengeAct = 123,
    Heist = 124,
    Rebellious  = 125,
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