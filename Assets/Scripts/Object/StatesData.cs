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
        public RemovalTiming RemovalTiming;
    }
}

public enum StateType
{
    None = 0,
    Death = 1,
    Demigod = 11,
    Slow = 21,
    EvaUp = 52,
    Chain = 101,
    ChainDamageUp = 102,
    CounterOura = 103,
    NoDamage = 104,
    Regene = 105,
    SetAfterAp = 106,
}

public enum RemovalTiming
{
    None = 0,
    UpdateTurn = 1,
    UpdateAp = 2,
    UpdateChain = 3,
    UpdateCount = 4,

}