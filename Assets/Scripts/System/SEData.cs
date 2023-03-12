using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SEData
{
    public int Id;
    public string Key;
    public string FileName;
}

public enum SEType {
    None,
    Decide,
    Cancel,
    Cursor
}
