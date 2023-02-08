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
        

        public string GetName()
        {
            string name = Name;
            return name;
        }

    }


}