using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdvsData : ScriptableObject {
    [SerializeField] public List<AdvData> _data = new List<AdvData>();

    [Serializable]
    public class AdvData
    {   
        public int Id;
        public string AdvName;

    }

}