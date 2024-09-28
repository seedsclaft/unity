using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.SS.UserModel;

namespace Ryneus
{
	public class SkillsImporter : AssetPostprocessor 
	{
		static readonly string ExcelName = "Skills.xlsx";

		// アセット更新があると呼ばれる
		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) 
		{
			foreach (string asset in importedAssets) 
			{
				if (AssetPostImporter.CheckOnPostprocessAllAssets(asset,ExcelName))
				{
					CreateSkillData(asset);
					AssetDatabase.SaveAssets();
					return;
				}
			}
		}

		static void CreateSkillData(string asset)
		{
			// 拡張子なしのファイル名を取得
			string FileName = Path.GetFileNameWithoutExtension(asset);

			// ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
			string ExportPath = $"{Path.Combine(AssetPostImporter.ExportExcelPath, FileName)}.asset";

			SkillDates Data = AssetDatabase.LoadAssetAtPath<SkillDates>(ExportPath);
			if (!Data)
			{
				// データがなければ作成
				Data = ScriptableObject.CreateInstance<SkillDates>();
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
					// テキストデータ作成
					List<TextData> textData = AssetPostImporter.CreateText(Book.GetSheetAt(4));
					// 情報の初期化
					Data.Data.Clear();
					// エクセルシートからセル単位で読み込み
					ISheet BaseSheet = Book.GetSheetAt(0);

					// KeyData生成
					IRow KeyRow = BaseSheet.GetRow(0);
					AssetPostImporter.SetKeyNames(KeyRow.Cells);
					for (int i = 1; i <= BaseSheet.LastRowNum; i++)
					{
						IRow BaseRow = BaseSheet.GetRow(i);
                        var SkillData = new SkillData
                        {
                            Id = AssetPostImporter.ImportNumeric(BaseRow, "Id"),
                            Name = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow, "NameId")).Text,
                            IconIndex = (MagicIconType)AssetPostImporter.ImportNumeric(BaseRow, "IconIndex"),
                            AnimationId = AssetPostImporter.ImportNumeric(BaseRow, "AnimationId"),
                            AnimationType = (AnimationType)AssetPostImporter.ImportNumeric(BaseRow, "AnimationType"),
                            CountTurn = AssetPostImporter.ImportNumeric(BaseRow, "CountTurn"),
                            Rank = (RankType)AssetPostImporter.ImportNumeric(BaseRow, "Rank"),
                            Attribute = (AttributeType)AssetPostImporter.ImportNumeric(BaseRow, "Attribute"),
                            SkillType = (SkillType)AssetPostImporter.ImportNumeric(BaseRow, "SkillType"),
                            TargetType = (TargetType)AssetPostImporter.ImportNumeric(BaseRow, "TargetType"),
                            Range = (RangeType)AssetPostImporter.ImportNumeric(BaseRow, "Range"),
                            Scope = (ScopeType)AssetPostImporter.ImportNumeric(BaseRow, "ScopeType"),
                            RepeatTime = AssetPostImporter.ImportNumeric(BaseRow, "RepeatTime"),
                            AliveType = (AliveType)AssetPostImporter.ImportNumeric(BaseRow, "AliveOnly"),
                            TimingOnlyCount = AssetPostImporter.ImportNumeric(BaseRow, "TimingOnlyCount"),
                            Help = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow, "NameId")).Help,
                        	//TurnCount = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.TurnCount),
                        };
                        Data.Data.Add(SkillData);
					}


					BaseSheet = Book.GetSheetAt(1);
					// KeyData生成
					KeyRow = BaseSheet.GetRow(0);
					AssetPostImporter.SetKeyNames(KeyRow.Cells);
					
					for (int i = 1; i <= BaseSheet.LastRowNum; i++)
					{
						IRow BaseRow = BaseSheet.GetRow(i);

                        var FeatureData = new SkillData.FeatureData
                        {
                            SkillId = AssetPostImporter.ImportNumeric(BaseRow, "SkillId"),
                            FeatureType = (FeatureType)AssetPostImporter.ImportNumeric(BaseRow, "FeatureType"),
                            Param1 = AssetPostImporter.ImportNumeric(BaseRow, "Param1"),
                            Param2 = AssetPostImporter.ImportNumeric(BaseRow, "Param2"),
                            Param3 = AssetPostImporter.ImportNumeric(BaseRow, "Param3"),
                            Rate = AssetPostImporter.ImportNumeric(BaseRow, "Rate")
                        };

                        var SkillData = Data.Data.Find(a => a.Id == FeatureData.SkillId);
						if (SkillData != null)
						{
							SkillData.FeatureDates ??= new List<SkillData.FeatureData>();
							SkillData.FeatureDates.Add(FeatureData);
						}
					}
					
					BaseSheet = Book.GetSheetAt(2);
					// KeyData生成
					KeyRow = BaseSheet.GetRow(0);
					AssetPostImporter.SetKeyNames(KeyRow.Cells);
					for (int i = 1; i <= BaseSheet.LastRowNum; i++)
					{
						IRow BaseRow = BaseSheet.GetRow(i);

                        var TriggerData = new SkillData.TriggerData
                        {
                            SkillId = AssetPostImporter.ImportNumeric(BaseRow, "SkillId"),
                            TriggerType = (TriggerType)AssetPostImporter.ImportNumeric(BaseRow, "TriggerType"),
                            TriggerTiming = (TriggerTiming)AssetPostImporter.ImportNumeric(BaseRow, "TriggerTiming"),
                            Param1 = AssetPostImporter.ImportNumeric(BaseRow, "Param1"),
                            Param2 = AssetPostImporter.ImportNumeric(BaseRow, "Param2"),
                            Param3 = AssetPostImporter.ImportNumeric(BaseRow, "Param3")
                        };

                        var SkillData = Data.Data.Find(a => a.Id == TriggerData.SkillId);
						if (SkillData != null){
							SkillData.TriggerDates ??= new List<SkillData.TriggerData>();
							SkillData.TriggerDates.Add(TriggerData);
						}
					}
					BaseSheet = Book.GetSheetAt(3);
					// KeyData生成
					KeyRow = BaseSheet.GetRow(0);
					AssetPostImporter.SetKeyNames(KeyRow.Cells);
					for (int i = 1; i <= BaseSheet.LastRowNum; i++)
					{
						IRow BaseRow = BaseSheet.GetRow(i);

                        var ScopeTriggerData = new SkillData.TriggerData
                        {
                            SkillId = AssetPostImporter.ImportNumeric(BaseRow, "SkillId"),
                            TriggerType = (TriggerType)AssetPostImporter.ImportNumeric(BaseRow, "TriggerType"),
                            TriggerTiming = (TriggerTiming)AssetPostImporter.ImportNumeric(BaseRow, "TriggerTiming"),
                            Param1 = AssetPostImporter.ImportNumeric(BaseRow, "Param1"),
                            Param2 = AssetPostImporter.ImportNumeric(BaseRow, "Param2"),
                            Param3 = AssetPostImporter.ImportNumeric(BaseRow, "Param3")
                        };

                        var SkillData = Data.Data.Find(a => a.Id == ScopeTriggerData.SkillId);
						if (SkillData != null)
						{
							SkillData.ScopeTriggers ??= new List<SkillData.TriggerData>();
							SkillData.ScopeTriggers.Add(ScopeTriggerData);
						}
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