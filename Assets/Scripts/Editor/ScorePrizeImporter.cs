
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.SS.UserModel;

namespace Ryneus
{
	public class ScorePrizesImporter : AssetPostprocessor 
	{
		enum BaseColumn
		{
			Id = 0,
			PrizeSetId,
			Score,
            ConditionName
		}
		static readonly string ExcelName = "ScorePrizes.xlsx";

		// アセット更新があると呼ばれる
		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) 
		{
			foreach (string asset in importedAssets) 
			{
				if (AssetPostImporter.CheckOnPostprocessAllAssets(asset,ExcelName))
				{
					CreateScorePrizeInfo(asset);
					AssetDatabase.SaveAssets();
					return;
				}
			}
		}

		static void CreateScorePrizeInfo(string asset)
		{
			// 拡張子なしのファイル名を取得
			string FileName = Path.GetFileNameWithoutExtension(asset);

			// ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
			string ExportPath = $"{Path.Combine(AssetPostImporter.ExportExcelPath, FileName)}.asset";

			ScorePrizeDates Data = AssetDatabase.LoadAssetAtPath<ScorePrizeDates>(ExportPath);
			if (!Data)
			{
				// データがなければ作成
				Data = ScriptableObject.CreateInstance<ScorePrizeDates>();
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
					List<TextData> textData = AssetPostImporter.CreateText(Book.GetSheetAt(1));

					// 情報の初期化
					Data.Data.Clear();

					// エクセルシートからセル単位で読み込み
					ISheet BaseSheet = Book.GetSheetAt(0);

					for (int i = 1; i <= BaseSheet.LastRowNum; i++)
					{
						IRow BaseRow = BaseSheet.GetRow(i);

                        var ScorePrizeData = new ScorePrizeData
                        {
                            Id = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.Id),
                            PriseSetId = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.PrizeSetId),
                            Score = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.Score),
                            Title = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.ConditionName)).Text,
                            Help = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.ConditionName)).Help
                        };
                        Data.Data.Add(ScorePrizeData);
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