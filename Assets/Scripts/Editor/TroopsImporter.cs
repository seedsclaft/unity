
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.SS.UserModel;

namespace Ryneus
{
	public class TroopsImporter : AssetPostprocessor 
	{
		static readonly string ExcelName = "Troops.xlsx";

		// アセット更新があると呼ばれる
		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
			foreach (string asset in importedAssets) 
			{
				if (AssetPostImporter.CheckOnPostprocessAllAssets(asset,ExcelName))
				{
					CreateTroopData(asset);
					AssetDatabase.SaveAssets();
					return;
				}
			}
		}

		static void CreateTroopData(string asset)
		{
			Debug.Log("CreateTroopData");
			// 拡張子なしのファイル名を取得
			string FileName = Path.GetFileNameWithoutExtension(asset);

			// ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
			string ExportPath = $"{Path.Combine(AssetPostImporter.ExportExcelPath, FileName)}.asset";

			TroopDates Data = AssetDatabase.LoadAssetAtPath<TroopDates>(ExportPath);
			if (!Data)
			{
				// データがなければ作成
				Data = ScriptableObject.CreateInstance<TroopDates>();
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

					// エクセルシートからセル単位で読み込み
					ISheet BaseSheet = Book.GetSheetAt(0);
					var KeyRow = BaseSheet.GetRow(0);
					AssetPostImporter.SetKeyNames(KeyRow.Cells);

					var TroopEnemyDates = new List<TroopEnemyData>();
					for (int i = 1; i <= BaseSheet.LastRowNum; i++)
					{
						IRow BaseRow = BaseSheet.GetRow(i);

                        var TroopEnemyData = new TroopEnemyData
                        {
                            Id = AssetPostImporter.ImportNumeric(BaseRow, "Id"),
                            TroopId = AssetPostImporter.ImportNumeric(BaseRow, "TroopId"),
                            EnemyId = AssetPostImporter.ImportNumeric(BaseRow, "EnemyId"),
                            Lv = AssetPostImporter.ImportNumeric(BaseRow, "Lv"),
                            BossFlag = AssetPostImporter.ImportNumeric(BaseRow, "BossFlag") == 1,
                            Line = (LineType)AssetPostImporter.ImportNumeric(BaseRow, "Line"),
                            StageLv = AssetPostImporter.ImportNumeric(BaseRow, "StageTurn"),
                            //PrizeSetId = AssetPostImporter.ImportNumeric(BaseRow, "PrizeSetId)
                        };
                        //TroopData.GetItemDates = new List<GetItemData>();
                        TroopEnemyDates.Add(TroopEnemyData);
					}

					foreach (var TroopEnemyData in TroopEnemyDates)
					{
						var FindTroop = Data.Data.Find(a => a.TroopId == TroopEnemyData.TroopId);
						if (FindTroop == null)
						{
                            var TroopData = new TroopData
                            {
                                TroopId = TroopEnemyData.TroopId,
                                //PrizeSetId = TroopEnemyData.PrizeSetId,
                                TroopEnemies = new List<TroopEnemyData>()
                            };
                            Data.Data.Add(TroopData);
						}
						FindTroop = Data.Data.Find(a => a.TroopId == TroopEnemyData.TroopId);
						FindTroop.TroopEnemies.Add(TroopEnemyData);
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