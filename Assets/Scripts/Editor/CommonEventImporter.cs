using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.SS.UserModel;

namespace Ryneus
{
	public class CommonEventImporter : AssetPostprocessor 
	{
        static readonly string ImportPath = "Assets/Data";
		static readonly string FileName = "CommonEvents.json";

        // Fileがあったら呼ばれる
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) 
        {	
            foreach (string asset in importedAssets) 
            {
                if (CheckOnPostprocessAsset(asset,FileName))
                {
                    ConvertJson(asset);
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

		static void ConvertJson(string asset)
		{
            Debug.Log("ConvertJson");
            string FileName = Path.GetFileNameWithoutExtension(asset);

			// ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
			string ExportPath = $"{Path.Combine(AssetPostImporter.ExportExcelPath, FileName)}.asset";

            // jsonを開く
            var data = AssetDatabase.LoadAssetAtPath<TextAsset>(ImportPath + "/" + FileName + ".json");
            var stringData = data.ToString();
            var convert = "{\"data\":[";
            convert += stringData.Substring(1,stringData.Length-1);
            convert += "}";
            var CommonEventDates = JsonUtility.FromJson<CommonEventMasterDates>(convert);
            
            foreach (var CommonEventData in CommonEventDates.data)
            {
                if (CommonEventData.list == null)
                {
                    continue;
                }
                foreach (var item in CommonEventData.list)
                {
                    Debug.Log(item.code);
                    //Debug.Log(item.parameters);
                }
            }

			CommonEventDates Data = AssetDatabase.LoadAssetAtPath<CommonEventDates>(ExportPath);
			if (!Data)
			{
				// データがなければ作成
				Data = ScriptableObject.CreateInstance<CommonEventDates>();
				AssetDatabase.CreateAsset(Data, ExportPath);
			}
            // 情報の初期化
            Data.hideFlags = HideFlags.None;
            Data.data = CommonEventDates.data;
        }
	}
}