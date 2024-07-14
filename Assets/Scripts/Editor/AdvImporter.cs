
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.SS.UserModel;

namespace Ryneus
{
	public class AdvImporter : AssetPostprocessor 
	{
		enum BaseColumn
		{
			Id = 0,
			AdvName,
			EndJump,
		}	
		static readonly string ExcelName = "Adventures.xlsx";

		// アセット更新があると呼ばれる
		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
			foreach (string asset in importedAssets) 
			{
				if (AssetPostImporter.CheckOnPostprocessAllAssets(asset,ExcelName))
				{
					CreateAdvInfo(asset);
					AssetDatabase.SaveAssets();
					return;
				}
			}
		}

		static void CreateAdvInfo(string asset)
		{
			Debug.Log("CreateAdvInfo");
			// 拡張子なしのファイル名を取得
			string FileName = Path.GetFileNameWithoutExtension(asset);

			// ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
			string ExportPath = $"{Path.Combine(AssetPostImporter.ExportExcelPath, FileName)}.asset";

			AdvDates Data = AssetDatabase.LoadAssetAtPath<AdvDates>(ExportPath);
			if (!Data)
			{
				// データがなければ作成
				Data = ScriptableObject.CreateInstance<AdvDates>();
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
					Data.Data.Clear();

					// エクセルシートからセル単位で読み込み
					ISheet BaseSheet = Book.GetSheetAt(0);

					for (int i = 1; i <= BaseSheet.LastRowNum; i++)
					{
						IRow BaseRow = BaseSheet.GetRow(i);

                        var AdvData = new AdvData
                        {
                            Id = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.Id),
                            AdvName = AssetPostImporter.ImportString(BaseRow, (int)BaseColumn.AdvName),
                            EndJump = (Scene)AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.EndJump)
                        };

                        Data.Data.Add(AdvData);
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