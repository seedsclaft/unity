using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StagesData : ScriptableObject {
    [SerializeField] public List<StageData> _data = new List<StageData>();


    [Serializable]
    public class StageData
    {   
        public int Id;
        public string Name;
        public string Help;
        public int Turns;
        public List<StageEventData> StageEvents;
        

        public string GetName()
        {
            string name = Name;
            return name;
        }

    }



    [Serializable]
    public class StageEventData
    {
        public int Turns;
        public EventTiming Timing;
        public StageEventType Type;
        public int Param;
    }
}

public enum EventTiming{
    None = 0,
    StartTactics = 1
}

public enum StageEventType{
    None = 0,
    CommandDisable = 1
}