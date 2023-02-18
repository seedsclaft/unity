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
    Demigod = 11
}

public enum RemovalTiming
{
    None = 0,
    UpdateTurn = 1,
    UpdateAp = 2,

}