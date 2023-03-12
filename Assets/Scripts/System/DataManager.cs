using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : ScriptableObject
{
    [SerializeField] public List<BGMData> BGM = new List<BGMData>();
    [SerializeField] public List<SEData> SE = new List<SEData>();
    
    public BGMData GetBGM(string key){
        BGMData bGMData = BGM.Find(a => a.Key == key);
        if (bGMData != null)
        {
            return bGMData;
        }
        return null;
    }
    
    public SEData GetSE(string key){
        SEData seData = SE.Find(a => a.Key == key);
        if (seData != null)
        {
            return seData;
        }
        return null;
    }
}
