using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.SS.UserModel;

namespace Ryneus
{
	public class BgmImporter : AssetPostprocessor 
	{
		//static readonly string ExcelPath = "Assets/Resources/Data";
		static readonly string ExcelName = "BGM.xlsx";

		// アセット更新があると呼ばれる
		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) 
		{
			foreach (string asset in importedAssets) {

				string ext = Path.GetExtension(asset);
				if (ext != ".xls" && ext != ".xlsx" && ext != ".xlsm") continue;

				// エクセルを開いているデータはスキップ
				string fileName = Path.GetFileName(asset);
				if (fileName.StartsWith("~$")) continue;

				// 同じパスのみ
				string filePath = Path.GetDirectoryName(asset);
				filePath = filePath.Replace("\\", "/");
				//if (filePath != ExcelPath) { continue; }

				// 同じファイルのみ
				if (fileName != ExcelName) { continue; }

				CreateInfo(asset);

				AssetDatabase.SaveAssets();
				return;
			}
		}

		static void CreateInfo(string asset)
		{
			// 拡張子なしのファイル名を取得
			string FileName = Path.GetFileNameWithoutExtension(asset);

			// ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
			string ExportPath = $"Assets\\Resources\\Data\\MainData.asset";

			DataManager Data = AssetDatabase.LoadAssetAtPath<DataManager>(ExportPath);
			if (!Data)
			{
				// データがなければ作成
				Data = ScriptableObject.CreateInstance<DataManager>();
				AssetDatabase.CreateAsset(Data, ExportPath);
				Data.hideFlags = HideFlags.NotEditable;
			}

			try
			{
				// ファイルを開く
				using (var Mainstream = File.Open(asset, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					// エクセルブックを作成
					AssetPostImporter.CreateBook(asset, Mainstream, out IWorkbook Book);

					// 情報の初期化
					Data.BGM.Clear();

					// エクセルシートからセル単位で読み込み
					ISheet BaseSheet = Book.GetSheetAt(0);
					var KeyRow = BaseSheet.GetRow(0);
					AssetPostImporter.SetKeyNames(KeyRow.Cells);

					for (int i = 1; i <= BaseSheet.LastRowNum; i++)
					{
						IRow BaseRow = BaseSheet.GetRow(i);

                        var BGM = new BGMData
                        {
                            Id = AssetPostImporter.ImportNumeric(BaseRow, "Id"),
                            Key = AssetPostImporter.ImportString(BaseRow, "Key"),
                            FileName = AssetPostImporter.ImportString(BaseRow, "FileName"),
                            Volume = AssetPostImporter.ImportFloat(BaseRow, "Volume"),
                            Loop = AssetPostImporter.ImportBool(BaseRow, "Loop"),
                            CrossFade = AssetPostImporter.ImportString(BaseRow, "CrossFade")
                        };
                        Data.BGM.Add(BGM);
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