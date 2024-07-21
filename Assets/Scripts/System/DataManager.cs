using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class DataManager : ScriptableObject
{
    [SerializeField] public List<BGMData> BGM = new ();
    [SerializeField] public List<SEData> SE =  new ();
    
    public BGMData GetBGM(string key)
    {
        var bGMData = BGM.Find(a => a.Key == key);
        if (bGMData != null)
        {
            return bGMData;
        }
        return null;
    }
    
    public BGMData GetBGM(int bgmId)
    {
        var bGMData = BGM.Find(a => a.Id == bgmId);
        if (bGMData != null)
        {
            return bGMData;
        }
        return null;
    }

    public async UniTask<AudioClip> GetSE(string fileName)
    {
        string sePath = "Assets/Audios/SE/" + fileName + ".ogg";
        var result = await Ryneus.ResourceSystem.LoadAsset<AudioClip>(sePath);
        return result;
    }
}
