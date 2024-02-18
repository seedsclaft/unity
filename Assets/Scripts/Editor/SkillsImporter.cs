
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

public class SkillsImporter : AssetPostprocessor {
    enum BaseColumn
    {
		Id = 0,
		NameId,
		_Name,
        IconIndex,
        _IconIndex,
		AnimationName,
		AnimationType,
		_AnimationType,
		DamageTiming,
		MpCost,
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
    }

    enum BaseFeatureColumn
	{
		SkillId,
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
	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
		foreach (string asset in importedAssets) {

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

					var SkillData = new SkillData();
					SkillData.Id = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Id);
					SkillData.Name = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.NameId)).Text;
					SkillData.IconIndex = (MagicIconType)AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.IconIndex);
					SkillData.AnimationId = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.AnimationName);
                    SkillData.AnimationType = (AnimationType)AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.AnimationType);
                    //SkillData.DamageTiming = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.DamageTiming);
                    SkillData.MpCost = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.MpCost);
					SkillData.Rank = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Rank);
					SkillData.Attribute = (AttributeType)AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Attribute);
					SkillData.SkillType = (SkillType)AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.SkillType);
                    SkillData.TargetType = (TargetType)AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.TargetType);
                    SkillData.Scope = (ScopeType)AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Scope);
                    SkillData.Range = (RangeType)AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Range);
                    SkillData.RepeatTime = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.RepeatTime);
					SkillData.AliveOnly = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.AliveOnly) == 1;
					SkillData.Help = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.NameId)).Help;
					Data.Data.Add(SkillData);
				}


				BaseSheet = Book.GetSheetAt(1);
				
				for (int i = 1; i <= BaseSheet.LastRowNum; i++)
				{
					IRow BaseRow = BaseSheet.GetRow(i);

					var FeatureData = new SkillData.FeatureData();
					FeatureData.SkillId = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseFeatureColumn.SkillId);
					FeatureData.FeatureType = (FeatureType)AssetPostImporter.ImportNumeric(BaseRow,(int)BaseFeatureColumn.FeatureType);
					FeatureData.Param1 = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseFeatureColumn.Param1);
					FeatureData.Param2 = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseFeatureColumn.Param2);
					FeatureData.Param3 = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseFeatureColumn.Param3);
					FeatureData.Rate = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseFeatureColumn.Rate);
					
					var SkillData = Data.Data.Find(a => a.Id == FeatureData.SkillId);
					if (SkillData != null){
						if (SkillData.FeatureDates == null){
							SkillData.FeatureDates = new List<SkillData.FeatureData>();
						}
						SkillData.FeatureDates.Add(FeatureData);
					}
				}
				
				BaseSheet = Book.GetSheetAt(2);
				for (int i = 1; i <= BaseSheet.LastRowNum; i++)
				{
					IRow BaseRow = BaseSheet.GetRow(i);

					var TriggerData = new SkillData.TriggerData();
					TriggerData.SkillId = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseTriggerColumn.SkillId);
					TriggerData.TriggerType = (TriggerType)AssetPostImporter.ImportNumeric(BaseRow,(int)BaseTriggerColumn.TriggerType);
					TriggerData.TriggerTiming = (TriggerTiming)AssetPostImporter.ImportNumeric(BaseRow,(int)BaseTriggerColumn.TriggerTiming);
					TriggerData.Param1 = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseTriggerColumn.Param1);
					TriggerData.Param2 = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseTriggerColumn.Param2);
					TriggerData.Param3 = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseTriggerColumn.Param3);
					
					var SkillData = Data.Data.Find(a => a.Id == TriggerData.SkillId);
					if (SkillData != null){
						if (SkillData.TriggerDates == null){
							SkillData.TriggerDates = new List<SkillData.TriggerData>();
						}
						SkillData.TriggerDates.Add(TriggerData);
					}
				}
				BaseSheet = Book.GetSheetAt(3);
				for (int i = 1; i <= BaseSheet.LastRowNum; i++)
				{
					IRow BaseRow = BaseSheet.GetRow(i);
					
					var ScopeTriggerData = new SkillData.TriggerData();
					ScopeTriggerData.SkillId = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseTriggerColumn.SkillId);
					ScopeTriggerData.TriggerType = (TriggerType)AssetPostImporter.ImportNumeric(BaseRow,(int)BaseTriggerColumn.TriggerType);
					ScopeTriggerData.TriggerTiming = (TriggerTiming)AssetPostImporter.ImportNumeric(BaseRow,(int)BaseTriggerColumn.TriggerTiming);
					ScopeTriggerData.Param1 = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseTriggerColumn.Param1);
					ScopeTriggerData.Param2 = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseTriggerColumn.Param2);
					ScopeTriggerData.Param3 = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseTriggerColumn.Param3);
					
					var SkillData = Data.Data.Find(a => a.Id == ScopeTriggerData.SkillId);
					if (SkillData != null){
						if (SkillData.ScopeTriggers == null){
							SkillData.ScopeTriggers = new List<SkillData.TriggerData>();
						}
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
