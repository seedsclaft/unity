using System;
using System.Collections.Generic;
using UnityEngine;

public class HelpDates : ScriptableObject
{
    [SerializeField] public List<HelpData> Data = new();
}

[Serializable]
public class HelpData
{   
    public int Id;
    public string Key;
    public string Help;
}