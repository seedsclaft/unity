using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TroopDates : ScriptableObject
{
    [SerializeField] public List<TroopData> Data = new();
}

[Serializable]
public class TroopData
{   
    public int Id;

    public int TroopId;
    public int EnemyId;
    public int Lv;
    public bool BossFlag;
    public LineType Line;
    public int StageTurn;
    public List<GetItemData> GetItemDates;
}
