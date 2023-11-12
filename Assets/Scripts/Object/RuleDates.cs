using System;
using System.Collections.Generic;
using UnityEngine;

public class RuleDates : ScriptableObject
{
    [SerializeField] public List<RuleData> Data = new();
}

[Serializable]
public class RuleData
{   
    public int Id;
    public string Name;
    public string Help;
    public int Category;
}