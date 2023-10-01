
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

public class EnemiesImporter : AssetPostprocessor {
    enum BaseColumn
    {
		Id = 0,
		NameId,
		ImagePath,
		Hp,
		Mp,
		Atk,
		Def,
		Spd,
		Kind1,
		Kind2,
		Kind3,
    }
    enum BaseLearningColumn
    {

		ActorId = 0,
		SkillId,
		_SkillName,
		Level,
		Weight,
	}
    enum BaseTriggersColumn
    {

		ActorId = 0,
		SkillId,
		TriggerType,
		TriggerTiming,
		Param1,
		Param2,
		Param3

	}
    enum BaseTextColumn
    {

		Id = 0,
		Text,
		Help
	}
	static readonly string ExcelPath = "Assets/Resources/Data";
	static readonly string ExcelName = "Enemies.xlsx";

	// アセット更新があると呼ばれる
	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
		foreach (string asset in importedAssets) {
			//拡張子を取得
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

			CreateEnemyData(asset);

			AssetDatabase.SaveAssets();
			return;
		}
	}

	static void CreateEnemyData(string asset)
	{
		// 拡張子なしのファイル名を取得
		string FileName = Path.GetFileNameWithoutExtension(asset);

		// ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
		string ExportPath = $"{Path.Combine(Path.GetDirectoryName(asset), FileName)}.asset";

		EnemiesData Data = AssetDatabase.LoadAssetAtPath<EnemiesData>(ExportPath);
		if (!Data)
		{
			// データがなければ作成
			Data = ScriptableObject.CreateInstance<EnemiesData>();
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
				List<TextData> textData = AssetPostImporter.CreateText(Book.GetSheetAt(3));

				// 情報の初期化
				Data._data.Clear();

				// エクセルシートからセル単位で読み込み
				ISheet BaseSheet = Book.GetSheetAt(0);

				for (int i = 1; i <= BaseSheet.LastRowNum; i++)
				{
					IRow Baserow = BaseSheet.GetRow(i);

					var EnemyData = new EnemiesData.EnemyData();
					EnemyData.Id = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.Id);
					EnemyData.Name = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.NameId)).Text;
					EnemyData.ImagePath = AssetPostImporter.ImportString(Baserow,(int)BaseColumn.ImagePath);
					
					int Hp = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.Hp);
					int Mp = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.Mp);
					int Atk = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.Atk);
					int Def = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.Def);
					int Spd = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.Spd);
					EnemyData.Kinds = new List<KindType>();
					KindType Kind1 = (KindType)AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.Kind1);
					if (Kind1 != 0) EnemyData.Kinds.Add(Kind1);
					KindType Kind2 = (KindType)AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.Kind2);
					if (Kind2 != 0) EnemyData.Kinds.Add(Kind2);
					KindType Kind3 = (KindType)AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.Kind3);
					if (Kind3 != 0) EnemyData.Kinds.Add(Kind3);
					EnemyData.BaseStatus = new StatusInfo();
					EnemyData.BaseStatus.SetParameter(Hp,Mp,Atk,Def,Spd);
					Data._data.Add(EnemyData);
				}

				BaseSheet = Book.GetSheetAt(1);

				for (int i = 1; i <= BaseSheet.LastRowNum; i++)
				{
					IRow Baserow = BaseSheet.GetRow(i);
					var LearningData = new LearningData();

					int ActorId = AssetPostImporter.ImportNumeric(Baserow,(int)BaseLearningColumn.ActorId);
					EnemiesData.EnemyData Enemy = Data._data.Find(a => a.Id == ActorId);
					
					LearningData.SkillId = AssetPostImporter.ImportNumeric(Baserow,(int)BaseLearningColumn.SkillId);
					LearningData.Level = AssetPostImporter.ImportNumeric(Baserow,(int)BaseLearningColumn.Level);
					LearningData.Weight = AssetPostImporter.ImportNumeric(Baserow,(int)BaseLearningColumn.Weight);
					LearningData.TriggerDatas = new List<SkillsData.TriggerData>();
					Enemy.LearningSkills.Add(LearningData);
				}

				
				BaseSheet = Book.GetSheetAt(2);

				for (int i = 1; i <= BaseSheet.LastRowNum; i++)
				{
					IRow Baserow = BaseSheet.GetRow(i);

					int ActorId = AssetPostImporter.ImportNumeric(Baserow,(int)BaseTriggersColumn.ActorId);
					EnemiesData.EnemyData Enemy = Data._data.Find(a => a.Id == ActorId);
					int SkillId = AssetPostImporter.ImportNumeric(Baserow,(int)BaseTriggersColumn.SkillId);
					LearningData learningData = Enemy.LearningSkills.Find(a => a.SkillId == SkillId);
					if (learningData != null)
					{
						SkillsData.TriggerData triggerData = new SkillsData.TriggerData();
						triggerData.TriggerType = (TriggerType)AssetPostImporter.ImportNumeric(Baserow,(int)BaseTriggersColumn.TriggerType);
						triggerData.TriggerTiming = (TriggerTiming)AssetPostImporter.ImportNumeric(Baserow,(int)BaseTriggersColumn.TriggerTiming);
						triggerData.Param1 = AssetPostImporter.ImportNumeric(Baserow,(int)BaseTriggersColumn.Param1);
						triggerData.Param2 = AssetPostImporter.ImportNumeric(Baserow,(int)BaseTriggersColumn.Param2);
						triggerData.Param3 = AssetPostImporter.ImportNumeric(Baserow,(int)BaseTriggersColumn.Param3);
						learningData.TriggerDatas.Add(triggerData);
					}
				}
				// 情報の初期化
				Data._textdata.Clear();

				BaseSheet = Book.GetSheetAt(2);

				for (int i = 1; i <= BaseSheet.LastRowNum; i++)
				{
					IRow Baserow = BaseSheet.GetRow(i);
					var TextData = new TextData();

					TextData.Id = AssetPostImporter.ImportNumeric(Baserow,(int)BaseTextColumn.Id);
					TextData.Text = AssetPostImporter.ImportString(Baserow,(int)BaseTextColumn.Text);
					
					Data._textdata.Add(TextData);
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
