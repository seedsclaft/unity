using System;
using System.Collections.Generic;
using UnityEngine;
using Effekseer;
using UnityEditor;

[Serializable]
public class AnimationDates : ScriptableObject
{
    [SerializeField] public List<AnimationData> Data = new();
}

[Serializable]
public class AnimationData 
{   
    public int Id;
    public string AnimationPath;

    private static string _resourcesPath = "Assets/Resources/Animations/";
    private static string _backupPath = "Assets/Resources_bak/Animations/";
    public void CheckResourceFile()
    {
        var resource = (EffekseerEffectAsset)AssetDatabase.LoadAssetAtPath(_resourcesPath + AnimationPath + ".asset", typeof(EffekseerEffectAsset));
        if (resource != null)
        {
            foreach (var item in resource.textureResources)
            {
                var texture = AssetDatabase.LoadAssetAtPath(_resourcesPath + AnimationPath + "/" + item.path, typeof(Texture));
                if (texture == null)
                {
                    Debug.Log(_resourcesPath + AnimationPath + "/" + item.path);
                }
            }
        } else
        {
            //Debug.Log("not" + AnimationPath);
        }
    }
}
