
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.SS.UserModel;
namespace Ryneus
{
    public class StageSymbolImporter : AssetPostprocessor 
	{
		/*
		static readonly string ExcelName = "StageSymbol.xlsx";
		static readonly string BaseName = "Stages.xlsx";

		// アセット更新があると呼ばれる
		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) 
		{
			foreach (string asset in importedAssets) 
			{
				if (AssetPostImporter.CheckOnPostprocessAllAssetsDivide(asset,ExcelName))
				{
					CreateStageSymbolData(asset);
					AssetDatabase.SaveAssets();
					return;
				}
			}
		}

		static void CreateStageSymbolData(string asset)
		{
			// 拡張子なしのファイル名を取得
			string FileName = Path.GetFileNameWithoutExtension(BaseName);

			// ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
			string ExportPath = $"{Path.Combine(AssetPostImporter.ExportExcelPath, FileName)}.asset";

			var Data = AssetDatabase.LoadAssetAtPath<StageDates>(ExportPath);
			if (!Data)
			{
                return;
			}
			//Data.hideFlags = HideFlags.None;

			try
			{
				// ファイルを開く
				using (var Mainstream = File.Open(asset, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					// エクセルブックを作成
					AssetPostImporter.CreateBook(asset, Mainstream, out IWorkbook Book);
					
					// エクセルシートからセル単位で読み込み
                    List<StageSymbolData> _symbolDates = new ();
					ISheet SymbolSheet = Book.GetSheetAt(0);
					var KeyRow = SymbolSheet.GetRow(0);
					for (int i = 1; i <= SymbolSheet.LastRowNum; i++)
					{
						IRow BaseRow = SymbolSheet.GetRow(i);
						AssetPostImporter.SetKeyNames(KeyRow.Cells);
                        var StageId = AssetPostImporter.ImportNumeric(BaseRow,"Id");
                        var StageData = Data.Data.Find(a => a.Id == StageId);
                        if (StageData == null)
                        {
                            continue;
                        }
                        var SymbolData = new StageSymbolData();
                        
                        if (StageId == StageData.Id)
                        {
                            SymbolData.StageId = AssetPostImporter.ImportNumeric(BaseRow, "Id");
                            SymbolData.Seek = AssetPostImporter.ImportNumeric(BaseRow, "Seek");
                            SymbolData.SeekIndex = AssetPostImporter.ImportNumeric(BaseRow, "SeekIndex");
                            SymbolData.SymbolType = (SymbolType)AssetPostImporter.ImportNumeric(BaseRow, "SymbolType");
                            SymbolData.Rate = AssetPostImporter.ImportNumeric(BaseRow, "Rate");
                            SymbolData.Param1 = AssetPostImporter.ImportNumeric(BaseRow, "Param1");
                            SymbolData.Param2 = AssetPostImporter.ImportNumeric(BaseRow, "Param2");
                            SymbolData.PrizeSetId = AssetPostImporter.ImportNumeric(BaseRow, "PrizeSetId");
                            SymbolData.ClearCount = AssetPostImporter.ImportNumeric(BaseRow, "ClearCount");
                            
                            _symbolDates.Add(SymbolData);
                        }
						

						StageData.StageSymbols = _symbolDates;
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogError(ex);
			}

			EditorUtility.SetDirty(Data);
		}
		*/
	}
}