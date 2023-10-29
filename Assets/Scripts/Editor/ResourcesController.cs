using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Effekseer;

public class ResourcesController 
{
    private static string _resourcesPath = "Assets/Resources/Animations/";
    private static string _backupPath = "Assets/Resources_bak/Animations/";

    private static List<string> _animationFolder = new List<string>(){
        "tktk01","tktk02","MAGICALxSPIRAL","NA_Effekseer","NA_Effekseer_Vol_2"
    };

    private static List<string> _defineAnimation = new List<string>(){
        "NA_Effekseer/NA_cut-in_002_1",
        "NA_Effekseer/NA_cut-in_002_2",
        "NA_Effekseer/NA_cut-in_002_3",
        "NA_Effekseer/NA_cut-in_002_4",
        "NA_Effekseer/NA_cut-in_002_5",
        "tktk01/Cure1",
        "NA_Effekseer/NA_Fire_001",
        "NA_Effekseer_Vol_2/NA_v2_3d_Magic Circle_normal",
        "NA_Effekseer_Vol_2/NA_v2_3d_Magic Circle_Fire",
        "NA_Effekseer_Vol_2/NA_v2_3d_Magic Circle_electric",
        "NA_Effekseer_Vol_2/NA_v2_3d_Magic Circle_ice",
        "NA_Effekseer_Vol_2/NA_v2_3d_Magic Circle_light",
        "NA_Effekseer_Vol_2/NA_v2_3d_Magic Circle_dark",
    };

    [MenuItem ("Resources/MoveResourceBackup")]
    static void MoveResourceBackup() {
        foreach (var folderPath in _animationFolder)
        {
            AssetDatabase.MoveAsset(_resourcesPath + folderPath, _backupPath + folderPath);
        }
    }

    [MenuItem ("Resources/CreateFolder")]
    static void CreateFolder() {
        foreach (var folderPath in _animationFolder)
        {
            var folder = AssetDatabase.FindAssets("Assets/Resources/Animations" + folderPath);
            if (folder != null)
            {
                AssetDatabase.CreateFolder("Assets/Resources/Animations", folderPath);
            }
            var tex = AssetDatabase.FindAssets("Assets/Resources/Animations/" + folderPath + "/Texture");
            if (tex != null)
            {
                AssetDatabase.CreateFolder("Assets/Resources/Animations/" + folderPath, "Texture");
            }
            var model = AssetDatabase.FindAssets("Assets/Resources/Animations/" + folderPath + "/Model");
            if (model != null)
            {
                AssetDatabase.CreateFolder("Assets/Resources/Animations/" + folderPath, "Model");
            }            
        }
    }

    [MenuItem ("Resources/Deploy")]
    static void Deploy() {
        var skills = Resources.Load<SkillDatas>("Data/Skills");
        var states = Resources.Load<StateDatas>("Data/States");
        var strs = new List<string>();
        foreach (var item in skills.Data)
        {
            if (item.AnimationName != "")
            strs.Add(item.AnimationName);
        }
        foreach (var item in states.Data)
        {
            if (item.EffectPath != "")
            strs.Add(item.EffectPath);
        }

        foreach (var str in strs)
        {    
            var t = (EffekseerEffectAsset)AssetDatabase.LoadAssetAtPath(_backupPath + str + ".asset", typeof(EffekseerEffectAsset));
            if (t != null)
            {
                AssetDatabase.MoveAsset(_backupPath + str + ".asset", _resourcesPath + str + ".asset");
                foreach (var tr in t.textureResources)
                {
                    TextureOutput("/" + tr.path);
                }
                
                foreach (var tr in t.modelResources)
                {
                    ModelOutput("/" + tr.path);
                }
            }
            var t2 = (EffekseerEffectAsset)AssetDatabase.LoadAssetAtPath(_resourcesPath + str + ".asset", typeof(EffekseerEffectAsset));
            if (t2 != null)
            {
                AssetDatabase.MoveAsset(_backupPath + str + ".asset", _resourcesPath + str + ".asset");
                foreach (var tr in t2.textureResources)
                {
                    TextureOutput("/" + tr.path);
                }
                
                foreach (var tr in t2.modelResources)
                {
                    ModelOutput("/" + tr.path);
                }
            }
        }
        foreach (var str in _defineAnimation)
        {    
            var t = (EffekseerEffectAsset)AssetDatabase.LoadAssetAtPath(_backupPath + str + ".asset", typeof(EffekseerEffectAsset));
            if (t != null)
            {
                AssetDatabase.MoveAsset(_backupPath + str + ".asset", _resourcesPath + str + ".asset");
                foreach (var tr in t.textureResources)
                {
                    TextureOutput("/" + tr.path);
                }
                
                foreach (var tr in t.modelResources)
                {
                    ModelOutput("/" + tr.path);
                }
            }
            var t2 = (EffekseerEffectAsset)AssetDatabase.LoadAssetAtPath(_resourcesPath + str + ".asset", typeof(EffekseerEffectAsset));
            if (t2 != null)
            {
                AssetDatabase.MoveAsset(_backupPath + str + ".asset", _resourcesPath + str + ".asset");
                foreach (var tr in t2.textureResources)
                {
                    TextureOutput("/" + tr.path);
                }
                
                foreach (var tr in t2.modelResources)
                {
                    ModelOutput("/" + tr.path);
                }
            }
        }
    }

    private static void TextureOutput(string str)
    {
        foreach (var folderPath in _animationFolder)
        {
            var tt = AssetDatabase.LoadAssetAtPath(_backupPath + folderPath + str, typeof(Texture));
            if (tt != null)
            {
                AssetDatabase.MoveAsset(_backupPath + folderPath + str, _resourcesPath + folderPath + str);
            }
        }
    }
    private static void ModelOutput(string str)
    {
        foreach (var folderPath in _animationFolder)
        {
            var tt = AssetDatabase.LoadAssetAtPath(_backupPath + folderPath + str, typeof(Object));
            if (tt != null)
            {
                AssetDatabase.MoveAsset(_backupPath + folderPath + str, _resourcesPath + folderPath + str);
            }
        }
    }
}
