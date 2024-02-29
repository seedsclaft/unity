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
	public class TestBattleImporter : AssetPostprocessor {
		enum BaseColumn
		{
			BattleId = 0,
			IsActor,
			Level,
			IsFront,
		}
		enum BaseActionColumn
		{
			BattlerIndex = 0,
			SkillId = 2,

		}
		static readonly string ExcelName = "TestBattle.xlsx";

		// アセット更新があると呼ばれる
		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
			foreach (string asset in importedAssets) {

				if (AssetPostImporter.CheckOnPostprocessAllAssets(asset,ExcelName))
				{
					CreateTestBattle(asset);
					AssetDatabase.SaveAssets();
					return;
				}
			}
		}

		static void CreateTestBattle(string asset)
		{
			Debug.Log("CreateTestBattle");
			// 拡張子なしのファイル名を取得
			string FileName = Path.GetFileNameWithoutExtension(asset);

			// ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
			string ExportPath = $"{Path.Combine(AssetPostImporter.ExportExcelPath, FileName)}.asset";

			TestBattleData Data = AssetDatabase.LoadAssetAtPath<TestBattleData>(ExportPath);
			if (!Data)
			{
				// データがなければ作成
				Data = ScriptableObject.CreateInstance<TestBattleData>();
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
					Data.TestBattleDates.Clear();
					Data.TestActionDates.Clear();
					DataSystem.LoadData();

					// エクセルシートからセル単位で読み込み
					ISheet BaseSheet = Book.GetSheetAt(0);

					for (int i = 1; i <= BaseSheet.LastRowNum; i++)
					{
						IRow BaseRow = BaseSheet.GetRow(i);

						var TestBattler = new TestBattlerData();

						TestBattler.IsActor = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.IsActor) == 1;
						TestBattler.BattlerId = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.BattleId);
						TestBattler.Level = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Level);
						TestBattler.IsFront = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.IsFront) == 1;
						

						Data.TestBattleDates.Add(TestBattler);
					}

					// 行動情報設定
					BaseSheet = Book.GetSheetAt(1);
					for (int i = 1; i <= BaseSheet.LastRowNum; i++)
					{
						IRow BaseRow = BaseSheet.GetRow(i);

						var ActionData = new TestActionData();
						ActionData.BattlerIndex = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseActionColumn.BattlerIndex);
						ActionData.SkillId = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseActionColumn.SkillId);
						
						Data.TestActionDates.Add(ActionData);
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