using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

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
    
    public BGMData GetBGM(int bgmId){
        BGMData bGMData = BGM.Find(a => a.Id == bgmId);
        if (bGMData != null)
        {
            return bGMData;
        }
        return null;
    }

    public async UniTask<AudioClip> GetSE(string fileName){
        string sePath = "Assets/Audios/SE/" + fileName + ".ogg";
        var result = await ResourceSystem.LoadAsset<AudioClip>(sePath);
        return result;
    }
}
