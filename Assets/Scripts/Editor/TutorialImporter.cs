using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.SS.UserModel;

namespace Ryneus
{
	public class TutorialImporter : AssetPostprocessor 
	{
		static readonly string ExcelName = "Tutorial.xlsx";

		// アセット更新があると呼ばれる
		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) 
		{
			foreach (string asset in importedAssets) 
			{
				if (AssetPostImporter.CheckOnPostprocessAllAssets(asset,ExcelName))
				{
					CreateTutorialInfo(asset);
					AssetDatabase.SaveAssets();
					return;
				}
			}
		}

		static void CreateTutorialInfo(string asset)
		{
			Debug.Log("CreateTutorialInfo");
			// 拡張子なしのファイル名を取得
			string FileName = Path.GetFileNameWithoutExtension(asset);

			// ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
			string ExportPath = $"{Path.Combine(AssetPostImporter.ExportExcelPath, FileName)}.asset";

			TutorialDates Data = AssetDatabase.LoadAssetAtPath<TutorialDates>(ExportPath);
			if (!Data)
			{
				// データがなければ作成
				Data = ScriptableObject.CreateInstance<TutorialDates>();
				AssetDatabase.CreateAsset(Data, ExportPath);
				//Data.hideFlags = HideFlags.NotEditable;
			}
			Data.hideFlags = HideFlags.None;

			try
			{
				// ファイルを開く
				using (var Mainstream = File.Open(asset, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					// エクセルブックを作成
					AssetPostImporter.CreateBook(asset, Mainstream, out IWorkbook Book);
					
					// 情報の初期化
					Data.Data.Clear();
                    List<TextData> textData = AssetPostImporter.CreateText(Book.GetSheetAt(1));

					// エクセルシートからセル単位で読み込み
					ISheet BaseSheet = Book.GetSheetAt(0);
					var KeyRow = BaseSheet.GetRow(0);
					AssetPostImporter.SetKeyNames(KeyRow.Cells);

					for (int i = 1; i <= BaseSheet.LastRowNum; i++)
					{
						IRow BaseRow = BaseSheet.GetRow(i);

                        var TutorialData = new TutorialData
                        {
                            Id = AssetPostImporter.ImportNumeric(BaseRow, "Id"),
                            Name = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow, "Id")).Text,
                            Help = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow, "Id")).Help,
							
                            SceneType = (Scene)AssetPostImporter.ImportNumeric(BaseRow, "SceneType"),
                            Type = AssetPostImporter.ImportNumeric(BaseRow, "Type"),
                            Param1 = AssetPostImporter.ImportNumeric(BaseRow, "Param1"),
                            Param2 = AssetPostImporter.ImportNumeric(BaseRow, "Param2"),
                            Param3 = AssetPostImporter.ImportNumeric(BaseRow, "Param3"),
                            X = AssetPostImporter.ImportNumeric(BaseRow, "X"),
                            Y = AssetPostImporter.ImportNumeric(BaseRow, "Y"),
                            Width = AssetPostImporter.ImportNumeric(BaseRow, "Width"),
                            Height = AssetPostImporter.ImportNumeric(BaseRow, "Height"),
                        };

						
						Data.Data.Add(TutorialData);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogError(ex);
			}

			EditorUtility.SetDirty(Data);
		}
	}
}