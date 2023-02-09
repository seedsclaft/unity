
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
        IconIndex,
		MpCost,
		Attribute,
		EffectType,
		EffectValue,
		TargetType,
		Scope,
		Range,
		AnimationId,
    }

	static readonly string ExcelPath = "Assets/Data";
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
				CreateBook(asset, Mainstream, out IWorkbook Book);
				// テキストデータ作成
				List<TextData> textData = CreateText(Book.GetSheetAt(1));
				// 情報の初期化
				Data._data.Clear();
				// エクセルシートからセル単位で読み込み
				ISheet BaseSheet = Book.GetSheetAt(0);

				for (int i = 1; i <= BaseSheet.LastRowNum; i++)
				{
					IRow Baserow = BaseSheet.GetRow(i);

					var SkillData = new SkillsData.SkillData();
					SkillData.Id = (int)Baserow.GetCell((int)BaseColumn.Id)?.NumericCellValue;
					SkillData.Name = textData.Find(a => a.Id == (int)Baserow.GetCell((int)BaseColumn.NameId)?.NumericCellValue).Text;
					SkillData.IconIndex = (int)Baserow.GetCell((int)BaseColumn.IconIndex)?.NumericCellValue;
					SkillData.MpCost = (int)Baserow.GetCell((int)BaseColumn.MpCost)?.NumericCellValue;
					SkillData.Attribute = (AttributeType)Baserow.GetCell((int)BaseColumn.Attribute)?.NumericCellValue;
					SkillData.EffectType = (EffectType)Baserow.GetCell((int)BaseColumn.EffectType)?.NumericCellValue;
                    SkillData.EffectValue = (float)Baserow.GetCell((int)BaseColumn.EffectValue)?.NumericCellValue;
					SkillData.TargetType = (TargetType)Baserow.GetCell((int)BaseColumn.TargetType)?.NumericCellValue;
                    SkillData.Scope = (ScopeType)Baserow.GetCell((int)BaseColumn.Scope)?.NumericCellValue;
                    SkillData.Range = (RangeType)Baserow.GetCell((int)BaseColumn.Range)?.NumericCellValue;
                    SkillData.AnimationId = (int)Baserow.GetCell((int)BaseColumn.AnimationId)?.NumericCellValue;
                    SkillData.Help = textData.Find(a => a.Id == (int)Baserow.GetCell((int)BaseColumn.NameId)?.NumericCellValue).Help;
					Data._data.Add(SkillData);
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
			TextData.Help = Baserow.GetCell((int)BaseTextColumn.Help).ToString();
			
			textData.Add(TextData);
		}

		return textData;
	}
}
