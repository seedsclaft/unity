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
    public StateType StateType;
    public string Name;
    public string Help;
    public string IconPath;
    public RemovalTiming RemovalTiming;
    public bool OverWrite;
    public string EffectPath;
    public EffectPositionType EffectPosition;
    public float EffectScale;
    public bool OverLap;
    public bool Removal;
    public bool Abnormal;
    public bool CheckHit; // 命中回避判定をするか
    public bool RemoveByAttack;
    // 付与者が戦闘不能になった時に効果が切れるか
    public bool RemoveByDeath;
}


public enum StateType
{
    None = 0,
    Death = 1,
    Wait = 10,
    Demigod = 1010,
    StatusUp = 1011,
    MaxHpUp = 1020,
    MaxMpUp = 1030,
    AtkUp = 1040,
    AtkDown = 1041,
    DefUp = 1050,
    DefDown = 1051,
    DefPerDown = 1052,
    SpdUp = 1060,
    CriticalRateUp = 1070,
    HitUp = 1080,
    EvaUp = 1090,
    BurnDamage = 2010, // 火傷
    Chain = 2020, // 拘束
    ChainDamageUp = 2021, // 拘束ダメージアップ
    CounterAura = 2030, // CA
    CounterAuraDamage = 2031,
    CounterAuraShell = 2032,
    Regenerate = 2040,
    NoDamage = 2050,
    Drain  = 2060,
    AntiDote = 2070,
    Prism = 2080,
    NoPassive = 2090,
    RevengeAct = 2100,
    Heist = 2110,
    Curse  = 2120,
    Substitute = 2130,
    Freeze = 2140,
    Stun = 2150,
    Slow = 2160,
    Blind = 2170,
    Barrier = 2180,
    Extension = 2190,
    Benediction = 2200,
    AfterHeal = 2210,
    Deadly = 2220,
    HealActionSelfHeal = 2230,
    AbsoluteHit = 2240,
    AssistHeal = 2250,
    Rebellious  = 2260,
    Undead = 2270,
    Accel = 2280,
    DamageUp = 2290,
    TargetRateUp = 2300,
    TargetRateDown = 2310,
    RemoveBuff = 2320,
    Penetrate = 2330,
}

public enum RemovalTiming
{
    None = 0,
    UpdateTurn = 1,
    UpdateAp = 2,
    UpdateChain = 3,
    UpdateCount = 4,
    NextSelfTurn = 5
}

public enum EffectPositionType
{
    Center = 0,
    Down = 1
}