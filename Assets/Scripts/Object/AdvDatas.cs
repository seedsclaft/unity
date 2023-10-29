using System;
using System.Collections.Generic;
using UnityEngine;

public class AdvDatas : ScriptableObject
{
    [SerializeField] public List<AdvData> Data = new();
}

[Serializable]
public class AdvData
{   
    public int Id;
    public string AdvName;
    public Scene EndJump;
}