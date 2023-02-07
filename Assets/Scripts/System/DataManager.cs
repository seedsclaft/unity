using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : ScriptableObject
{
    [SerializeField] public List<BGMData> BGM = new List<BGMData>();
    
    public BGMData GetBGM(string key){
        BGMData bGMData = BGM.Find(a => a.Key == key);
        if (bGMData != null)
        {
            return bGMData;
        }
        return null;
    }
}
