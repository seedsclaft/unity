using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SEData
{
    public int Id;
    public string Key;
    public string FileName;
    public float Volume;
    public float Pitch;
}

public enum SEType {
    None,
    Decide,
    Cancel,
    Cursor,
    Damage,
    Defeat,
    Demigod,
    Deny,
    LevelUp,
    Critical,
    Skill,
}
