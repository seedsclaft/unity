using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatesData : ScriptableObject
{
    [SerializeField] public List<StateData> _data = new List<StateData>();
    
    [Serializable]
    public class StateData
    {   
        public int Id;
        public string Name;
        public string Help;
        public int IconIndex;
        public RemovalTiming RemovalTiming;
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
    Chain = 101,
    ChainDamageUp = 102,
    CounterOura = 103,
    NoDamage = 104,
    Regene = 105,
    SetAfterAp = 106,
    Substitute = 107,
    Banish = 108,
    Benediction  = 109,
    Curse  = 110,
    Drain  = 111,
    AfterHeal = 112,
    CounterOuraDamage = 113,
    DamageAddState = 114,
}

public enum RemovalTiming
{
    None = 0,
    UpdateTurn = 1,
    UpdateAp = 2,
    UpdateChain = 3,
    UpdateCount = 4,

}