
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Ryneus
{
	public class TroopsImporter : AssetPostprocessor {
		enum BaseColumn
		{
			Id = 0,
			TroopId,
			EnemyId,
			_EnemyId,
			Lv,
			BossFlag,
			Line,
			StageTurn,
			PrizeSetId
		}
		enum BaseGetItemColumn
		{		
			Id,
			TroopId,
			Type,
			Param1,
			Param2,
		}
		static readonly string ExcelName = "Troops.xlsx";

		// アセット更新があると呼ばれる
		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
			foreach (string asset in importedAssets) {

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

					var TroopEnemyDates = new List<TroopEnemyData>();
					for (int i = 1; i <= BaseSheet.LastRowNum; i++)
					{
						IRow BaseRow = BaseSheet.GetRow(i);

						var TroopEnemyData = new TroopEnemyData();
						TroopEnemyData.Id = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Id);
						TroopEnemyData.TroopId = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.TroopId);
						TroopEnemyData.EnemyId = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.EnemyId);
						TroopEnemyData.Lv = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Lv);
						TroopEnemyData.BossFlag = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.BossFlag) == 1;
						TroopEnemyData.Line = (LineType)AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Line);
						TroopEnemyData.PrizeSetId = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.PrizeSetId);
						//TroopData.GetItemDates = new List<GetItemData>();
						TroopEnemyDates.Add(TroopEnemyData);
					}

					foreach (var TroopEnemyData in TroopEnemyDates)
					{
						var FindTroop = Data.Data.Find(a => a.TroopId == TroopEnemyData.TroopId);
						if (FindTroop == null)
						{
							var TroopData = new TroopData();
							TroopData.TroopId = TroopEnemyData.TroopId;
							TroopData.PrizeSetId = TroopEnemyData.PrizeSetId;
							TroopData.TroopEnemies = new List<TroopEnemyData>();
							Data.Data.Add(TroopData);
						}
						FindTroop = Data.Data.Find(a => a.TroopId == TroopEnemyData.TroopId);
						FindTroop.TroopEnemies.Add(TroopEnemyData);
					}

					/*
					BaseSheet = Book.GetSheetAt(1);
					for (int i = 1; i <= BaseSheet.LastRowNum; i++)
					{
						IRow BaseRow = BaseSheet.GetRow(i);
						int Id = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseGetItemColumn.Id);
						int TroopId = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseGetItemColumn.TroopId);
						var troopData = Data.Data.Find(a => a.TroopId == TroopId && a.Line == LineType.Back);
						if (troopData != null)
						{
							var getItemData = new GetItemData();
							getItemData.Type = (GetItemType)AssetPostImporter.ImportNumeric(BaseRow,(int)BaseGetItemColumn.Type);
							getItemData.Param1 = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseGetItemColumn.Param1);
							getItemData.Param2 = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseGetItemColumn.Param2);
							troopData.GetItemDates.Add(getItemData);
						}
					}
					*/
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