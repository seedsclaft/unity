using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Text;
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
        
        [MenuItem ("Resources/CommonEvent")]
        static void CommonEvent() 
        {
            var csvStrings = new List<string>
            {
                // パラメータ設定
                "Command,Arg1,Arg2,Arg3,Arg4,Arg5,Arg6,WaitType,Text,PageCtrl,Voice,WindowType" + "\n"
            };
            var CommonEventDates = Resources.Load<CommonEventDates>("Data/CommonEvents").data;
            if (CommonEventDates != null)
            {
                foreach (var d in CommonEventDates)
                {
                    if (d.list == null)
                    {
                        continue;
                    }
                    if (d.id < 100)
                    {
                        continue;
                    }
                    if (d.id != 101)
                    {
                        continue;
                    }
                    var lastName = "";
                    foreach (var l in d.list)
                    {
                        var csvCol = new List<string>();
                        switch (l.code)
                        {
                            case 101: // メッセージ準備
                                lastName = l.parameters[0];
                            break;
                            case 401: // メッセージ表示
                                csvCol.Add("");
                                csvCol.Add(lastName); // アクター名
                                csvCol.Add("");
                                csvCol.Add("");
                                csvCol.Add("");
                                csvCol.Add("");
                                csvCol.Add("");
                                csvCol.Add("");
                                csvCol.Add(l.parameters[0]);
                                break;
                            case 108: // 注釈
                                continue;
                            case 213: // 吹き出し表示
                            break;
                            case 221: // フェードアウト
                                csvCol.Add("FadeOut");
                                csvCol.Add("black");
                                break;
                            case 222: // フェードイン
                                csvCol.Add("FadeIn");
                                csvCol.Add("black");
                                break;
                            case 223: // 色調変更（カラーは変換不可）
                                csvCol.Add("FadeOut");
                                csvCol.Add("#00000068");
                                break;
                            case 224: // 画面のフラッシュ
                            break;
                            case 225: // 画面のシェイク
                                csvCol.Add("Shake");
                                csvCol.Add("Camera");
                                break;
                            case 230: // ウェイト
                                csvCol.Add("Wait");
                                csvCol.Add("");
                                csvCol.Add("");
                                csvCol.Add("");
                                csvCol.Add("");
                                csvCol.Add("");
                                csvCol.Add("1");
                                break;
                            case 231: // ピクチャの表示
                                csvCol.Add("Bg");
                                csvCol.Add(l.parameters[1]);
                                break;
                            case 241: // BGM再生 (ファイル指定・音量不可)
                                csvCol.Add("Bgm");
                                break;
                            case 242: // BGMフェードアウト
                                csvCol.Add("StopBgm");
                                csvCol.Add("");
                                csvCol.Add("");
                                csvCol.Add("");
                                csvCol.Add("");
                                csvCol.Add("");
                                csvCol.Add("2");
                                break;
                            case 245: // BGS再生(ファイル指定・音量不可)
                                csvCol.Add("Ambience");
                                break;
                            case 246: // BGSフェードアウト
                                csvCol.Add("StopAmbience");
                                csvCol.Add("");
                                csvCol.Add("");
                                csvCol.Add("");
                                csvCol.Add("");
                                csvCol.Add("");
                                csvCol.Add("2");
                                break;
                            case 250: // Se再生(ファイル指定・音量不可)
                                csvCol.Add("PlaySe");
                                break;
                            case 320: // アクター名変更（変換不可）
                            break;
                            
                            
                        }
                        if (csvCol.Count == 0)
                        {
                            continue;
                        }
                        // ","を挿入
                        if (csvCol.Count < 11)
                        {
                            for (int i = csvCol.Count;i < 11;i++)
                            {   
                                csvCol.Add("");
                            }
                        }
                        var csvText = "";
                        foreach (var csvC in csvCol)
                        {
                            csvText += csvC + ",";
                        }
                        if (csvText != "")
                        {
                            csvStrings.Add(csvText);  
                        }  
                    }
                }
            }
            var sw = new StreamWriter(@"output.csv", false);
            foreach (var csvString in csvStrings)
            {
                sw.WriteLine(csvString);
            }
            sw.Flush();
            sw.Close();
        }
	}
}