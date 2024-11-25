using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using Effekseer;

namespace Ryneus
{
    public class MakerEffectConverter : AssetPostprocessor
    {
        static readonly string ImportPath = "Assets/MakerEffect/JsonFile";
        static readonly string EffectPath = "Assets/MakerEffect/Resources/Effects/";
        static readonly string ExportPath = "Assets/MakerEffect/Resources/Animations/";
        
        static readonly string SoundPath = "Assets/MakerEffect/Resources/Se/";
        static readonly string FileName = "Animations.json";

        // Fileがあったら呼ばれる
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) 
        {	
            foreach (string asset in importedAssets) 
            {
                if (CheckOnPostprocessAsset(asset,FileName))
                {
                    ConvertEffectAsset(asset);
                    AssetDatabase.SaveAssets();
                    return;
                }
            }
        }

        public static bool CheckOnPostprocessAsset(string asset,string FileName)
        {
            string ext = Path.GetExtension(asset);
            if (ext != ".json") return false;

            string fileName = Path.GetFileName(asset);
            // 同じパスのみ
            string filePath = Path.GetDirectoryName(asset);
            filePath = filePath.Replace("\\", "/");
            if (filePath != ImportPath) return false;
            // 同じファイルのみ
            if (fileName != FileName) return false;

            return true;
        }	
        
        private static void ConvertEffectAsset(string asset)
        {
            Debug.Log("ConvertEffectAsset");
            string FileName = Path.GetFileNameWithoutExtension(asset);

            // jsonを開く
            var data = AssetDatabase.LoadAssetAtPath<TextAsset>(ImportPath + "/" + FileName + ".json");
            var stringData = data.ToString();
            // JSONの始まりが配列なので最初の[と最後の]を補正
            var convert = "{\"data\":[";
            convert += stringData.Substring(1,stringData.Length-1);
            convert += "}";
            var MakerEffectDates = JsonUtility.FromJson<MakerEffectDates>(convert);
            foreach (var effectData in MakerEffectDates.data)
            {
    			var effectAsset = AssetDatabase.LoadAssetAtPath<EffekseerEffectAsset>(EffectPath + effectData.effectName + ".asset");
                if (effectAsset != null)
                {
                    effectAsset.soundResources = new Effekseer.Internal.EffekseerSoundResource[effectData.soundTimings.Count];
                    for (int i = 0; i < effectData.soundTimings.Count; i++)
                    {
                        effectAsset.soundResources[i] = Effekseer.Internal.EffekseerSoundResource.LoadAsset(SoundPath, effectData.soundTimings[i].se.name + ".ogg");

                        if (effectAsset.soundResources[i].clip == null)
                        {
                            Debug.LogWarning(string.Format("Failed to load {0}", effectData.soundTimings[i].se.name + ".ogg"));
                        }
                    }
                    // データを作成
                    EffekseerEffectAsset d = AssetDatabase.LoadAssetAtPath<EffekseerEffectAsset>(ExportPath + effectData.effectName + ".asset");
                    if (d == null)
                    {
                        d = ScriptableObject.CreateInstance<MakerEffectAsset>();
                        AssetDatabase.CreateAsset(d, ExportPath + effectData.effectName + ".asset");
                    }
                    d.hideFlags = HideFlags.NotEditable;    
                    d.efkBytes = effectAsset.efkBytes;
                    d.textureResources = new Effekseer.Internal.EffekseerTextureResource[effectAsset.textureResources.Length];
                    d.textureResources = effectAsset.textureResources;
                    d.modelResources = new Effekseer.Internal.EffekseerModelResource[effectAsset.modelResources.Length];
                    d.modelResources = effectAsset.modelResources;
                    d.materialResources = new Effekseer.Internal.EffekseerMaterialResource[effectAsset.materialResources.Length];
                    d.materialResources = effectAsset.materialResources;
                    d.curveResources = new Effekseer.Internal.EffekseerCurveResource[effectAsset.curveResources.Length];
                    d.curveResources = effectAsset.curveResources;
                    d.Scale = effectAsset.Scale;
                    var makerD = (MakerEffectAsset)d;
                    makerD.soundTimings = new List<MakerEffectData.SoundTimings>();
                    foreach (var soundTimings in effectData.soundTimings)
                    {
                        makerD.soundTimings.Add(soundTimings);
                    }
			        EditorUtility.SetDirty(d);
                }
            }
        }
    }
}
