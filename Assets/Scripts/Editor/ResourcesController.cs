using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Effekseer;

namespace Ryneus
{
    public class ResourcesController 
    {
        private static string _resourcesPath = "Assets/Resources/Animations/";
        private static string _backupPath = "Assets/Resources_bak/Animations/";

        private static string _makerBackupPath = "Assets/MakerEffect/Resources_bak/Animations/";
        private static string _makerResourcePath = "Assets/MakerEffect/Resources/Animations/";
        private static string _makerAnimationDataPath = "Assets/MakerEffect/Resources_bak/MakerEffectDates/";
        private static string _makerDataPath = "Assets/MakerEffect/Resources/Animations/AnimationData/";
        private static string _makerFolder = "MakerEffect";
        
        private static List<string> _animationFolder = new List<string>(){
            "tktk01","tktk02","MAGICALxSPIRAL","NA_Effekseer","NA_Effekseer_Vol_2","EffekseerVol_3"
        };

        [MenuItem ("Resources/MoveResourceBackup")]
        static void MoveResourceBackup() {
            foreach (var folderPath in _animationFolder)
            {
                AssetDatabase.MoveAsset(_resourcesPath + folderPath, _backupPath + folderPath);
            }
        }

        [MenuItem ("Resources/CreateAnimationFolder")]
        static void CreateAnimationFolder() {
            foreach (var folderPath in _animationFolder)
            {
                var folder = AssetDatabase.IsValidFolder("Assets/Resources/Animations/" + folderPath);
                
                if (folder == false)
                {
                    AssetDatabase.CreateFolder("Assets/Resources/Animations", folderPath);
                }
            }
        }

        [MenuItem ("Resources/CreateAnimationChildFolder")]
        static void CreateAnimationChildFolder() {
            foreach (var folderPath in _animationFolder)
            {
                var folder = AssetDatabase.IsValidFolder("Assets/Resources/Animations/" + folderPath);
                if (folder == true)
                {
                    var tex = AssetDatabase.IsValidFolder("Assets/Resources/Animations/" + folderPath + "/Texture");
                    if (tex == false)
                    {
                        AssetDatabase.CreateFolder("Assets/Resources/Animations/" + folderPath, "Texture");
                    }
                    var model = AssetDatabase.IsValidFolder("Assets/Resources/Animations/" + folderPath + "/Model");
                    if (model == false)
                    {
                        AssetDatabase.CreateFolder("Assets/Resources/Animations/" + folderPath, "Model");
                    }            
                    var parts = AssetDatabase.IsValidFolder("Assets/Resources/Animations/" + folderPath + "/Parts");
                    if (parts == false)
                    {
                        AssetDatabase.CreateFolder("Assets/Resources/Animations/" + folderPath, "Parts");
                    }      
                }
            }
        }

        [MenuItem ("Resources/Deploy")]
        static void Deploy() {
            var animationDates = Resources.Load<AnimationDates>("Data/Animations");
            var paths = new List<string>();
            foreach (var item in animationDates.Data)
            {
                if (item.AnimationPath != "")
                paths.Add(item.AnimationPath);
            }

            foreach (var str in paths)
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
            var mt = AssetDatabase.LoadAssetAtPath(_makerBackupPath + _makerFolder + str, typeof(Object));
            if (mt != null)
            {
                AssetDatabase.MoveAsset(_makerBackupPath + _makerFolder + str, _makerResourcePath + _makerFolder + str);
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
            var mt = AssetDatabase.LoadAssetAtPath(_makerBackupPath + _makerFolder + str, typeof(Object));
            if (mt != null)
            {
                AssetDatabase.MoveAsset(_makerBackupPath + _makerFolder + str, _makerResourcePath + _makerFolder + str);
            }
        }

        private static void SoundOutput(string str)
        {
            var mt = AssetDatabase.LoadAssetAtPath(_makerBackupPath + "Sound" + str + ".ogg", typeof(Object));
                    
            if (mt != null)
            {
                AssetDatabase.MoveAsset(_makerBackupPath + "Sound/" + str + ".ogg", _makerResourcePath + "Sound/" + str + ".ogg");
            }
        }

        [MenuItem ("Resources/SoundAttachment")]
        private static void SoundAttachment()
        {
            var animationDates = Resources.Load<AnimationDates>("Data/Animations");
            var paths = new List<string>();
            foreach (var item in animationDates.Data)
            {
                if (item.AnimationPath != "")
                paths.Add(item.AnimationPath);
            }
            foreach (var str in paths)
            {    
                var t = (EffekseerEffectAsset)AssetDatabase.LoadAssetAtPath(_resourcesPath + str + ".asset", typeof(EffekseerEffectAsset));
                if (t != null)
                {
                    foreach (var item in t.soundResources)
                    {
                        if (item.clip == null)
                        {
                            var path = item.path.Replace("../","/");
                            var sound = AssetDatabase.LoadAssetAtPath(_resourcesPath + path, typeof(AudioClip));
                            
                            if (sound != null)
                            {
                                item.clip = (AudioClip)sound;
                            }
                        }
                    }
                    EditorUtility.SetDirty(t);
                    AssetDatabase.SaveAssetIfDirty(t);
                }
            }
        }

        [MenuItem ("Resources/DeployMakerEffect")]
        static void DeployMakerEffect() {
            var animationDates = Resources.Load<AnimationDates>("Data/Animations");
            var paths = new List<string>();
            foreach (var item in animationDates.Data)
            {
                if (item.AnimationPath != "" && item.MakerEffect == true)
                {
                    paths.Add(item.AnimationPath);
                    var mt = AssetDatabase.LoadAssetAtPath(_makerAnimationDataPath + item.AnimationPath.Replace("MakerEffect/","") + ".asset", typeof(Object));
                    if (mt != null)
                    {
                        
                        AssetDatabase.MoveAsset(_makerAnimationDataPath + item.AnimationPath.Replace("MakerEffect/","") + ".asset", _makerDataPath + item.AnimationPath.Replace("MakerEffect/","") + ".asset");
                    }
                }
            }
            foreach (var str in paths)
            {    
                var t = (EffekseerEffectAsset)AssetDatabase.LoadAssetAtPath(_makerBackupPath + str + ".asset", typeof(EffekseerEffectAsset));
                if (t != null)
                {
                    AssetDatabase.MoveAsset(_makerBackupPath + str + ".asset", _makerResourcePath + str + ".asset");
                    foreach (var tr in t.textureResources)
                    {
                        TextureOutput("/" + tr.path);
                    }
                    
                    foreach (var tr in t.modelResources)
                    {
                        ModelOutput("/" + tr.path);
                    }

                }
                var t2 = (EffekseerEffectAsset)AssetDatabase.LoadAssetAtPath(_makerResourcePath + str + ".asset", typeof(EffekseerEffectAsset));
                if (t2 != null)
                {
                    AssetDatabase.MoveAsset(_makerBackupPath + str + ".asset", _makerResourcePath + str + ".asset");
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

        [MenuItem ("Resources/DeployMakerSound")]
        static void DeployMakerSound() {
            var animationDates = Resources.Load<AnimationDates>("Data/Animations");
            var paths = new List<string>();
            foreach (var item in animationDates.Data)
            {
                if (item.AnimationPath != "" && item.MakerEffect == true)
                {
                    
                    var mt = Resources.Load<MakerEffectAssetData>("Animations/AnimationData/" + item.AnimationPath.Replace("MakerEffect/",""));
                    
                    if (mt != null)
                    {
                        foreach (var soundTimings in mt.AssetData.soundTimings)
                        {
                            if (soundTimings.se != null)
                            {
                                SoundOutput("/" + soundTimings.se.name);
                            }
                        }
                    }
                    
                }
            }
        }
    }
}