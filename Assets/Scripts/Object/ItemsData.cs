using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemsData : ScriptableObject {
    [SerializeField] public List<ItemData> _data = new List<ItemData>();


    [Serializable]
    public class ItemData
    {   
        public int Id;
        public string Name;
        public int Worth;
        public int Feature;
        public int Target;
        public int Value;

        

        public string GetName()
        {
            string name = Name;
            return name;
        }

    }

}

