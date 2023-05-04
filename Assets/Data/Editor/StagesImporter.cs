
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

public class StagesInfoImporter : AssetPostprocessor {
    enum BaseColumn
    {
		Id = 0,
		NameId,
        Turns,
		InitMembers
    }
    enum BaseEventColumn
    {
		Id = 0,
        Turns,
		Timing,
		_TIming,
		Type,
		_Type,
		Param,
		ReadFlag
    }
	static readonly string ExcelPath = "Assets/Resources/Data";
	static readonly string ExcelName = "Stages.xlsx";

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

			CreateStagesData(asset);

			AssetDatabase.SaveAssets();
			return;
		}
	}

	static void CreateStagesData(string asset)
	{
		// 拡張子なしのファイル名を取得
		string FileName = Path.GetFileNameWithoutExtension(asset);

		// ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
		string ExportPath = $"{Path.Combine(Path.GetDirectoryName(asset), FileName)}.asset";

		StagesData Data = AssetDatabase.LoadAssetAtPath<StagesData>(ExportPath);
		if (!Data)
		{
			// データがなければ作成
			Data = ScriptableObject.CreateInstance<StagesData>();
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
				ISheet EventSheet = Book.GetSheetAt(1);
				for (int i = 1; i <= BaseSheet.LastRowNum; i++)
				{
					IRow Baserow = BaseSheet.GetRow(i);

					var StageData = new StagesData.StageData();
					StageData.Id = (int)Baserow.GetCell((int)BaseColumn.Id)?.SafeNumericCellValue();
					StageData.Name = textData.Find(a => a.Id == (int)Baserow.GetCell((int)BaseColumn.NameId).NumericCellValue).Text;
					StageData.Help = textData.Find(a => a.Id == (int)Baserow.GetCell((int)BaseColumn.NameId).NumericCellValue).Help;
					StageData.Turns = (int)Baserow.GetCell((int)BaseColumn.Turns)?.SafeNumericCellValue();
					StageData.InitMembers = new List<int>();
					string[] list = Baserow.GetCell((int)BaseColumn.InitMembers)?.SafeStringCellValue().Split(',');
					foreach (string item in list)
					{
						StageData.InitMembers.Add(int.Parse(item));
					}
					StageData.StageEvents = new List<StagesData.StageEventData>();
					for (int j = 1; j <= EventSheet.LastRowNum; j++)
					{
						IRow Eventrow = EventSheet.GetRow(j);
						var EventData = new StagesData.StageEventData();
						var StageId = (int)Eventrow.GetCell((int)BaseEventColumn.Id)?.SafeNumericCellValue();
						
						if (StageId == StageData.Id)
						{
							EventData.Turns = (int)Eventrow.GetCell((int)BaseEventColumn.Turns)?.SafeNumericCellValue();
							EventData.Timing = (EventTiming)Eventrow.GetCell((int)BaseEventColumn.Timing)?.SafeNumericCellValue();
							EventData.Type = (StageEventType)Eventrow.GetCell((int)BaseEventColumn.Type)?.SafeNumericCellValue();
							EventData.Param = (int)Eventrow.GetCell((int)BaseEventColumn.Param)?.SafeNumericCellValue();
							EventData.ReadFlag = (bool)(Eventrow.GetCell((int)BaseEventColumn.ReadFlag)?.SafeNumericCellValue() == 1);
							
							EventData.EventKey = EventData.Turns.ToString() + EventData.Timing.ToString() + EventData.Type.ToString() + EventData.Param.ToString();

							StageData.StageEvents.Add(EventData);
						}
					}
					Data._data.Add(StageData);
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

	// 文字列を分解
	static string[] StringSplit(string str, int count)
	{
		List<string> List = new List<string>();

		int Length = (int)Math.Ceiling((double)str.Length / count);

		for (int i = 0; i < Length; i++)
		{
			int Start = count * i;

			// 始まりが文字列の長さより多かったら
			if (str.Length <= Start)
			{
				break;
			}
			// 読み取る大きさが文字列の長さより多かったら終わりを指定しない
			if (str.Length < Start + count)
			{
				List.Add(str.Substring(Start));
			}
			// 始まりの位置と終わりの位置を指定（始まりの値は含むが終わりの値は含まない）
			else
			{
				List.Add(str.Substring(Start, count));
			}
		}

		return List.ToArray();
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
