
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

public class ActorsImporter : AssetPostprocessor {
    enum BaseColumn
    {
		Id = 0,
		NameId,
		ClassId,
		ImagePath,
		InitLv,
		MaxLv,
		InitHp,
		InitMp,
		InitAtk,
		InitSpd,
		MaxHp,
		MaxMp,
		MaxAtk,
		MaxSpd,
    }
    enum BaseLearningColumn
    {

		ActorId = 0,
		SkillId,
		LearningType,
		Value,
	}
	static readonly string ExcelPath = "Assets/Data";
	static readonly string ExcelName = "Actors.xlsx";

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

			CreateActorInfo(asset);

			AssetDatabase.SaveAssets();
			return;
		}
	}

	static void CreateActorInfo(string asset)
	{
		// 拡張子なしのファイル名を取得
		string FileName = Path.GetFileNameWithoutExtension(asset);

		// ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
		string ExportPath = $"{Path.Combine(Path.GetDirectoryName(asset), FileName)}.asset";

		ActorsData Data = AssetDatabase.LoadAssetAtPath<ActorsData>(ExportPath);
		if (!Data)
		{
			// データがなければ作成
			Data = ScriptableObject.CreateInstance<ActorsData>();
			AssetDatabase.CreateAsset(Data, ExportPath);
			Data.hideFlags = HideFlags.NotEditable;
		}

		try
		{
			// ファイルを開く
			using (var Mainstream = File.Open(asset, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				// エクセルブックを作成
				CreateBook(asset, Mainstream, out IWorkbook Book);
				List<TextData> textData = CreateText(Book.GetSheetAt(2));

				// 情報の初期化
				Data._data.Clear();

				// エクセルシートからセル単位で読み込み
				ISheet BaseSheet = Book.GetSheetAt(0);

				for (int i = 1; i <= BaseSheet.LastRowNum; i++)
				{
					IRow Baserow = BaseSheet.GetRow(i);

					var ActorData = new ActorsData.ActorData();
					ActorData.Id = (int)Baserow.GetCell((int)BaseColumn.Id).NumericCellValue;
					ActorData.Name = textData.Find(a => a.Id == (int)Baserow.GetCell((int)BaseColumn.NameId).NumericCellValue).Text;
					
					ActorData.ClassId = (int)Baserow.GetCell((int)BaseColumn.ClassId)?.NumericCellValue;
					ActorData.ImagePath = Baserow.GetCell((int)BaseColumn.ImagePath).ToString();
					ActorData.InitLv = (int)Baserow.GetCell((int)BaseColumn.InitLv)?.NumericCellValue;
					ActorData.MaxLv = (int)Baserow.GetCell((int)BaseColumn.MaxLv)?.NumericCellValue;

					int InitHp = (int)Baserow.GetCell((int)BaseColumn.InitHp)?.NumericCellValue;
					int InitMp = (int)Baserow.GetCell((int)BaseColumn.InitMp)?.NumericCellValue;
					int InitAtk = (int)Baserow.GetCell((int)BaseColumn.InitAtk)?.NumericCellValue;
					int InitSpd = (int)Baserow.GetCell((int)BaseColumn.InitSpd)?.NumericCellValue;
					ActorData.InitStatus = new StatusInfo();
					ActorData.InitStatus.SetParameter(InitHp,InitMp,InitAtk,InitSpd);

					int MaxHp = (int)Baserow.GetCell((int)BaseColumn.MaxHp)?.NumericCellValue;
					int MaxMp = (int)Baserow.GetCell((int)BaseColumn.MaxMp)?.NumericCellValue;
					int MaxAtk = (int)Baserow.GetCell((int)BaseColumn.MaxAtk)?.NumericCellValue;
					int MaxSpd = (int)Baserow.GetCell((int)BaseColumn.MaxSpd)?.NumericCellValue;
					ActorData.MaxStatus = new StatusInfo();
					ActorData.MaxStatus.SetParameter(MaxHp,MaxMp,MaxAtk,MaxSpd);

					Data._data.Add(ActorData);
				}
				// 習得スキル情報設定
				BaseSheet = Book.GetSheetAt(1);

				for (int i = 1; i <= BaseSheet.LastRowNum; i++)
				{
					IRow Baserow = BaseSheet.GetRow(i);
					var LearningData = new LearningData();

					int ActorId = (int)Baserow.GetCell((int)BaseLearningColumn.ActorId)?.NumericCellValue;
					ActorsData.ActorData Actor = Data._data.Find(a => a.Id == ActorId);
					
					LearningData.SkillId = (int)Baserow.GetCell((int)BaseLearningColumn.SkillId)?.NumericCellValue;
					LearningData.LearningType = (LearningType)Baserow.GetCell((int)BaseLearningColumn.LearningType)?.NumericCellValue;
					LearningData.Value = (int)Baserow.GetCell((int)BaseLearningColumn.Value)?.NumericCellValue;
					
					Actor.LearningSkills.Add(LearningData);
				}

			}
		}
		catch (Exception ex)
		{
			Debug.LogError(ex);
		}

		EditorUtility.SetDirty(Data);
	}


	// エクセルワークブックを作成
	static void CreateBook(string path, Stream stream, out IWorkbook Workbook)
	{
		// 拡張子が".xls"の場合
		if (Path.GetExtension(path) == ".xls")
		{
			Workbook = new HSSFWorkbook(stream);
		}
		// 拡張子がそれ以外の場合
		else
		{
			Workbook = new XSSFWorkbook(stream);
		}
	}

	// テキストデータを作成
	static List<TextData> CreateText(ISheet BaseSheet)
	{
		var textData = new List<TextData>();

		for (int i = 1; i <= BaseSheet.LastRowNum; i++)
		{
			IRow Baserow = BaseSheet.GetRow(i);
			var TextData = new TextData();

			TextData.Id = (int)Baserow.GetCell((int)BaseTextColumn.Id)?.NumericCellValue;
			TextData.Text = Baserow.GetCell((int)BaseTextColumn.Text).ToString();
			
			textData.Add(TextData);
		}

		return textData;
	}
}


public enum BaseTextColumn
{

	Id = 0,
	Text,
}