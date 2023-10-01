
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
		AnimationPosition,
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
		Param3
	}
    enum BaseTriggerColumn
	{

		SkillId,
		TriggerType,
		TriggerTiming,
		Param1,
		Param2,
		Param3 
	}
	static readonly string ExcelPath = "Assets/Resources/Data";
	static readonly string ExcelName = "Skills.xlsx";

	// アセット更新があると呼ばれる
	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
		foreach (string asset in importedAssets) {

			string ext = Path.GetExtension(asset);
			if (ext != ".xls" && ext != ".xlsx" && ext != ".xlsm") continue;

			// エクセルを開いているデータはスキップ
			string fileName = Path.GetFileName(asset);
			if (fileName.StartsWith("~$")) continue;

			// 同じパスのみ
			string filePath = Path.GetDirectoryName(asset);
			filePath = filePath.Replace("\\", "/");
			if (filePath != ExcelPath) { continue; }

			// 同じファイルのみ
			if (fileName != ExcelName) { continue; }

			CreateSkillData(asset);

			AssetDatabase.SaveAssets();
			return;
		}
	}

	static void CreateSkillData(string asset)
	{
		// 拡張子なしのファイル名を取得
		string FileName = Path.GetFileNameWithoutExtension(asset);

		// ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
		string ExportPath = $"{Path.Combine(Path.GetDirectoryName(asset), FileName)}.asset";

		SkillsData Data = AssetDatabase.LoadAssetAtPath<SkillsData>(ExportPath);
		if (!Data)
		{
			// データがなければ作成
			Data = ScriptableObject.CreateInstance<SkillsData>();
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
				List<TextData> textData = AssetPostImporter.CreateText(Book.GetSheetAt(3));
				// 情報の初期化
				Data._data.Clear();
				// エクセルシートからセル単位で読み込み
				ISheet BaseSheet = Book.GetSheetAt(0);

				for (int i = 1; i <= BaseSheet.LastRowNum; i++)
				{
					IRow Baserow = BaseSheet.GetRow(i);

					var SkillData = new SkillsData.SkillData();
					SkillData.Id = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.Id);
					SkillData.Name = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.NameId)).Text;
					SkillData.IconIndex = (MagicIconType)AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.IconIndex);
					SkillData.AnimationName = AssetPostImporter.ImportString(Baserow,(int)BaseColumn.AnimationName);
                    SkillData.AnimationPosition = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.AnimationPosition);
                    SkillData.AnimationType = (AnimationType)AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.AnimationType);
                    SkillData.DamageTiming = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.DamageTiming);
                    SkillData.MpCost = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.MpCost);
					SkillData.Rank = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.Rank);
					SkillData.Attribute = (AttributeType)AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.Attribute);
					SkillData.SkillType = (SkillType)AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.SkillType);
                    SkillData.TargetType = (TargetType)AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.TargetType);
                    SkillData.Scope = (ScopeType)AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.Scope);
                    SkillData.Range = (RangeType)AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.Range);
                    SkillData.RepeatTime = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.RepeatTime);
					SkillData.AliveOnly = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.AliveOnly) == 1;
					SkillData.Help = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.NameId)).Help;
					Data._data.Add(SkillData);
				}


				BaseSheet = Book.GetSheetAt(1);
				
				for (int i = 1; i <= BaseSheet.LastRowNum; i++)
				{
					IRow Baserow = BaseSheet.GetRow(i);

					var FeatureData = new SkillsData.FeatureData();
					FeatureData.SkillId = AssetPostImporter.ImportNumeric(Baserow,(int)BaseFeatureColumn.SkillId);
					FeatureData.FeatureType = (FeatureType)AssetPostImporter.ImportNumeric(Baserow,(int)BaseFeatureColumn.FeatureType);
					FeatureData.Param1 = AssetPostImporter.ImportNumeric(Baserow,(int)BaseFeatureColumn.Param1);
					FeatureData.Param2 = AssetPostImporter.ImportNumeric(Baserow,(int)BaseFeatureColumn.Param2);
					FeatureData.Param3 = AssetPostImporter.ImportNumeric(Baserow,(int)BaseFeatureColumn.Param3);
					
					var SkillData = Data._data.Find(a => a.Id == FeatureData.SkillId);
					if (SkillData != null){
						if (SkillData.FeatureDatas == null){
							SkillData.FeatureDatas = new List<SkillsData.FeatureData>();
						}
						SkillData.FeatureDatas.Add(FeatureData);
					}
				}
				
				BaseSheet = Book.GetSheetAt(2);
				for (int i = 1; i <= BaseSheet.LastRowNum; i++)
				{
					IRow Baserow = BaseSheet.GetRow(i);

					var TriggerData = new SkillsData.TriggerData();
					TriggerData.SkillId = AssetPostImporter.ImportNumeric(Baserow,(int)BaseTriggerColumn.SkillId);
					TriggerData.TriggerType = (TriggerType)AssetPostImporter.ImportNumeric(Baserow,(int)BaseTriggerColumn.TriggerType);
					TriggerData.TriggerTiming = (TriggerTiming)AssetPostImporter.ImportNumeric(Baserow,(int)BaseTriggerColumn.TriggerTiming);
					TriggerData.Param1 = AssetPostImporter.ImportNumeric(Baserow,(int)BaseTriggerColumn.Param1);
					TriggerData.Param2 = AssetPostImporter.ImportNumeric(Baserow,(int)BaseTriggerColumn.Param2);
					TriggerData.Param3 = AssetPostImporter.ImportNumeric(Baserow,(int)BaseTriggerColumn.Param3);
					
					var SkillData = Data._data.Find(a => a.Id == TriggerData.SkillId);
					if (SkillData != null){
						if (SkillData.TriggerDatas == null){
							SkillData.TriggerDatas = new List<SkillsData.TriggerData>();
						}
						SkillData.TriggerDatas.Add(TriggerData);
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
