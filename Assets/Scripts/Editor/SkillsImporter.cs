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
		enum BaseColumn
		{
			Id = 0,
			NameId,
			_Name,
			IconIndex,
			_IconIndex,
			AnimationId,
			AnimationType,
			_AnimationType,
			CountTurn,
			Rank,
			Attribute,
			_Attribute,
			SkillType,
			_SkillType,
			TargetType,
			_TargetType,
			Scope,
			_Scope,
			Range,
			RepeatTime,
			AliveOnly,
			TimingOnlyCount,
		}

		enum BaseFeatureColumn
		{
			SkillId,
			_SkillId,
			FeatureType,
			_FeatureType,
			Param1,
			Param2,
			Param3,
			Rate,
		}
		enum BaseTriggerColumn
		{
			SkillId,
			TriggerType = 2,
			TriggerTiming = 4,
			Param1,
			Param2,
			Param3 
		}
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
				Data.hideFlags = HideFlags.NotEditable;
			}

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

					for (int i = 1; i <= BaseSheet.LastRowNum; i++)
					{
						IRow BaseRow = BaseSheet.GetRow(i);
                        var SkillData = new SkillData
                        {
                            Id = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.Id),
                            Name = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.NameId)).Text,
                            IconIndex = (MagicIconType)AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.IconIndex),
                            AnimationId = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.AnimationId),
                            AnimationType = (AnimationType)AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.AnimationType),
                            //SkillData.DamageTiming = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.DamageTiming);
                            CountTurn = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.CountTurn),
                            Rank = (RankType)AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.Rank),
                            Attribute = (AttributeType)AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.Attribute),
                            SkillType = (SkillType)AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.SkillType),
                            TargetType = (TargetType)AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.TargetType),
                            Scope = (ScopeType)AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.Scope),
                            Range = (RangeType)AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.Range),
                            RepeatTime = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.RepeatTime),
                            AliveType = (AliveType)AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.AliveOnly),
                            TimingOnlyCount = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.TimingOnlyCount),
                            Help = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.NameId)).Help,
                        	//TurnCount = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.TurnCount),
                        };
                        Data.Data.Add(SkillData);
					}


					BaseSheet = Book.GetSheetAt(1);
					
					for (int i = 1; i <= BaseSheet.LastRowNum; i++)
					{
						IRow BaseRow = BaseSheet.GetRow(i);

                        var FeatureData = new SkillData.FeatureData
                        {
                            SkillId = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseFeatureColumn.SkillId),
                            FeatureType = (FeatureType)AssetPostImporter.ImportNumeric(BaseRow, (int)BaseFeatureColumn.FeatureType),
                            Param1 = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseFeatureColumn.Param1),
                            Param2 = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseFeatureColumn.Param2),
                            Param3 = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseFeatureColumn.Param3),
                            Rate = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseFeatureColumn.Rate)
                        };

                        var SkillData = Data.Data.Find(a => a.Id == FeatureData.SkillId);
						if (SkillData != null)
						{
							SkillData.FeatureDates ??= new List<SkillData.FeatureData>();
							SkillData.FeatureDates.Add(FeatureData);
						}
					}
					
					BaseSheet = Book.GetSheetAt(2);
					for (int i = 1; i <= BaseSheet.LastRowNum; i++)
					{
						IRow BaseRow = BaseSheet.GetRow(i);

                        var TriggerData = new SkillData.TriggerData
                        {
                            SkillId = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseTriggerColumn.SkillId),
                            TriggerType = (TriggerType)AssetPostImporter.ImportNumeric(BaseRow, (int)BaseTriggerColumn.TriggerType),
                            TriggerTiming = (TriggerTiming)AssetPostImporter.ImportNumeric(BaseRow, (int)BaseTriggerColumn.TriggerTiming),
                            Param1 = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseTriggerColumn.Param1),
                            Param2 = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseTriggerColumn.Param2),
                            Param3 = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseTriggerColumn.Param3)
                        };

                        var SkillData = Data.Data.Find(a => a.Id == TriggerData.SkillId);
						if (SkillData != null){
							SkillData.TriggerDates ??= new List<SkillData.TriggerData>();
							SkillData.TriggerDates.Add(TriggerData);
						}
					}
					BaseSheet = Book.GetSheetAt(3);
					for (int i = 1; i <= BaseSheet.LastRowNum; i++)
					{
						IRow BaseRow = BaseSheet.GetRow(i);

                        var ScopeTriggerData = new SkillData.TriggerData
                        {
                            SkillId = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseTriggerColumn.SkillId),
                            TriggerType = (TriggerType)AssetPostImporter.ImportNumeric(BaseRow, (int)BaseTriggerColumn.TriggerType),
                            TriggerTiming = (TriggerTiming)AssetPostImporter.ImportNumeric(BaseRow, (int)BaseTriggerColumn.TriggerTiming),
                            Param1 = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseTriggerColumn.Param1),
                            Param2 = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseTriggerColumn.Param2),
                            Param3 = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseTriggerColumn.Param3)
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