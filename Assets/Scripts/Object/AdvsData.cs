using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdvsData : ScriptableObject {
    [SerializeField] public List<AdvData> _data = new();

    [Serializable]
    public class AdvData
    {   
        public int Id;
        public string AdvName;
        public Scene EndJump;
    }
}