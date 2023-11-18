
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

public class MakerEffectImporter : AssetPostprocessor{
	static readonly string ImportPath = "Assets/MakerEffect/Resources";
	static readonly string ExportPath = "Assets/MakerEffect/Resources_bak/MakerEffectDatas";
	static readonly string FileName = "Animations.json";

	// Fileがあったら呼ばれる
	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
		
        foreach (string asset in importedAssets) {
			if (CheckOnPostprocessAsset(asset,FileName))
			{
				CreateEffectData(asset);
				AssetDatabase.SaveAssets();
				return;
			}
		}
	}

	static public bool CheckOnPostprocessAsset(string asset,string ExcelName)
	{
		string ext = Path.GetExtension(asset);
		if (ext != ".json") return false;

		// 同じパスのみ
		string filePath = Path.GetDirectoryName(asset);
		filePath = filePath.Replace("\\", "/");
		if (filePath != ImportPath) return false;

		return true;
	}

	static void CreateEffectData(string asset)
	{
		Debug.Log("CreateEffectData");
		// 拡張子なしのファイル名を取得
		string FileName = Path.GetFileNameWithoutExtension(asset);

        // jsonを開く
        var data = Resources.Load(FileName, typeof(TextAsset)) as TextAsset;
        var stringData = data.ToString();
        // JSONの始まりが配列なので最初の[と最後の]を補正
        var convert = "{\"data\":[";
        convert += stringData.Substring(1,stringData.Length-1);
        convert += "}";
            
        var MakerEffectDatas = JsonUtility.FromJson<MakerEffectDatas>(convert);
        foreach (var MakerEffectData in MakerEffectDatas.data)
        {
            if (MakerEffectData.effectName != null)
            {
                // ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
		        string ExportFilePath = $"{Path.Combine(ExportPath, MakerEffectData.effectName)}.asset";

                // エフェクト名で保存
                MakerEffectAssetData Data = AssetDatabase.LoadAssetAtPath<MakerEffectAssetData>(ExportFilePath);
                if (Data == null){
                    // データがなければ作成
                    Data = ScriptableObject.CreateInstance<MakerEffectAssetData>();
                    AssetDatabase.CreateAsset(Data, ExportFilePath);
                    Data.hideFlags = HideFlags.NotEditable;
                }
                Data.AssetData = MakerEffectData;
                EditorUtility.SetDirty(Data);
            }
        }
	}
}

