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
        public int Rank;
        public int Mt;
        public int Hit;
        public int Cri;
        public int MinRange;
        public int MaxRange;
        public int UseCount;
        public int Worth;
        public int Exp;

        

        public string GetName()
        {
            string name = Name;
            return name;
        }

    }

}

