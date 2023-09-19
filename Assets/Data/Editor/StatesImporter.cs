
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

public class StatesImporter : AssetPostprocessor {
    enum BaseColumn
    {
		Id = 0,
		NameId,
		IconIndex,
        RemovalTiming,
		OverRight,
		EffectPath,
		EffectPosition,
		OverLap
    }
	static readonly string ExcelPath = "Assets/Resources/Data";
	static readonly string ExcelName = "States.xlsx";

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

			CreateStateInfo(asset);

			AssetDatabase.SaveAssets();
			return;
		}
	}

	static void CreateStateInfo(string asset)
	{
		// 拡張子なしのファイル名を取得
		string FileName = Path.GetFileNameWithoutExtension(asset);

		// ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
		string ExportPath = $"{Path.Combine(Path.GetDirectoryName(asset), FileName)}.asset";

		StatesData Data = AssetDatabase.LoadAssetAtPath<StatesData>(ExportPath);
		if (!Data)
		{
			// データがなければ作成
			Data = ScriptableObject.CreateInstance<StatesData>();
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
				List<TextData> textData = CreateText(Book.GetSheetAt(1));

				// 情報の初期化
				Data._data.Clear();

				// エクセルシートからセル単位で読み込み
				ISheet BaseSheet = Book.GetSheetAt(0);

				for (int i = 1; i <= BaseSheet.LastRowNum; i++)
				{
					IRow Baserow = BaseSheet.GetRow(i);

					var StateData = new StatesData.StateData();
					StateData.Id = (int)Baserow.GetCell((int)BaseColumn.Id).NumericCellValue;
					StateData.Name = textData.Find(a => a.Id == (int)Baserow.GetCell((int)BaseColumn.NameId).NumericCellValue).Text;
					StateData.Help = textData.Find(a => a.Id == (int)Baserow.GetCell((int)BaseColumn.NameId).NumericCellValue).Help;
					StateData.IconPath = Baserow.GetCell((int)BaseColumn.IconIndex)?.SafeStringCellValue();
					StateData.RemovalTiming = (RemovalTiming)Baserow.GetCell((int)BaseColumn.RemovalTiming).NumericCellValue;
					StateData.OverWrite = (bool)(Baserow.GetCell((int)BaseColumn.OverRight)?.SafeNumericCellValue() == 1);
					StateData.EffectPath = Baserow.GetCell((int)BaseColumn.EffectPath)?.SafeStringCellValue();
					StateData.EffectPosition = (EffectPositionType)Baserow.GetCell((int)BaseColumn.EffectPosition)?.SafeNumericCellValue();
					StateData.OverLap = (bool)(Baserow.GetCell((int)BaseColumn.OverLap)?.SafeNumericCellValue() == 1);
					
					
					Data._data.Add(StateData);
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