
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
	public class SkillTriggerImporter : AssetPostprocessor {
		enum BaseColumn
		{
			Id = 0,
			NameId,
			_NameId,
			Category,
			Priority,
			TargetType,
			TriggerType,
			Param1,
			Param2,
			Param3
		}
		static readonly string ExcelName = "SkillTrigger.xlsx";

		// アセット更新があると呼ばれる
		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
			foreach (string asset in importedAssets) {

				if (AssetPostImporter.CheckOnPostprocessAllAssets(asset,ExcelName))
				{
					CreateSkillTriggerInfo(asset);
					AssetDatabase.SaveAssets();
					return;
				}
			}
		}

		static void CreateSkillTriggerInfo(string asset)
		{
			// 拡張子なしのファイル名を取得
			string FileName = Path.GetFileNameWithoutExtension(asset);

			// ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
			string ExportPath = $"{Path.Combine(AssetPostImporter.ExportExcelPath, FileName)}.asset";

			SkillTriggerDates Data = AssetDatabase.LoadAssetAtPath<SkillTriggerDates>(ExportPath);
			if (!Data)
			{
				// データがなければ作成
				Data = ScriptableObject.CreateInstance<SkillTriggerDates>();
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

					for (int i = 1; i <= BaseSheet.LastRowNum-1; i++)
					{
						IRow BaseRow = BaseSheet.GetRow(i);

                        var SkillTriggerData = new SkillTriggerData
                        {
                            Id = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.Id),
                            Name = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.NameId)).Text,
                            Help = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.NameId)).Help,
                            Category = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.Category),
                            Priority = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.Priority),
                            TargetType = (TargetType)AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.TargetType),
                            TriggerType = (TriggerType)AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.TriggerType),
                            Param1 = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.Param1),
                            Param2 = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.Param2),
                            Param3 = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.Param3)
                        };
                        Data.Data.Add(SkillTriggerData);
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