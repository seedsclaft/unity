using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TipsData : ScriptableObject
{
    [SerializeField] public List<TipData> _data = new List<TipData>();
    
    [Serializable]
    public class TipData
    {   
        public int Id;
        public string Name;
        public string ImagePath;
    }
}