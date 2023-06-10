using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RulesData : ScriptableObject
{
    [SerializeField] public List<RuleData> _data = new List<RuleData>();
    
    [Serializable]
    public class RuleData
    {   
        public int Id;
        public string Name;
        public string Help;
        public int Category;
    }
}