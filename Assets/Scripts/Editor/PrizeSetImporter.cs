
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.SS.UserModel;

namespace Ryneus
{
	public class PrizeSetInfoImporter : AssetPostprocessor 
	{
		enum BaseColumn
		{
			Id = 0,
			Type,
			Param1,
			Param2
		}
		static readonly string ExcelName = "PrizeSets.xlsx";

		// アセット更新があると呼ばれる
		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
			foreach (string asset in importedAssets) 
			{
				if (AssetPostImporter.CheckOnPostprocessAllAssets(asset,ExcelName))
				{
					CreatePrizeSetData(asset);
					AssetDatabase.SaveAssets();
					return;
				}
			}
		}

		static void CreatePrizeSetData(string asset)
		{
			Debug.Log("CreatePrizeSetData");
			// 拡張子なしのファイル名を取得
			string FileName = Path.GetFileNameWithoutExtension(asset);

			// ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
			string ExportPath = $"{Path.Combine(AssetPostImporter.ExportExcelPath, FileName)}.asset";

			PrizeSetDates Data = AssetDatabase.LoadAssetAtPath<PrizeSetDates>(ExportPath);
			if (!Data)
			{
				// データがなければ作成
				Data = ScriptableObject.CreateInstance<PrizeSetDates>();
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

						var PrizeSet = new PrizeSetData();
						PrizeSet.Id = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Id);
                        var getItemData = new GetItemData
                        {
                            Type = (GetItemType)AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.Type),
                            Param1 = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.Param1),
                            Param2 = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.Param2)
                        };
                        PrizeSet.GetItem = getItemData;
						Data.Data.Add(PrizeSet);
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