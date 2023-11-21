
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
		Selectable,
        Turns,
		InitMembers,
		RandomTroopCount,
		BGMId,
		Reborn
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
	static readonly string ExcelName = "Stages.xlsx";

	// アセット更新があると呼ばれる
	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
		foreach (string asset in importedAssets) {

			if (AssetPostImporter.CheckOnPostprocessAllAssets(asset,ExcelName))
			{
				CreateStagesData(asset);
				AssetDatabase.SaveAssets();
				return;
			}
		}
	}

	static void CreateStagesData(string asset)
	{
		// 拡張子なしのファイル名を取得
		string FileName = Path.GetFileNameWithoutExtension(asset);

		// ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
		string ExportPath = $"{Path.Combine(AssetPostImporter.ExportExcelPath, FileName)}.asset";

		StageDates Data = AssetDatabase.LoadAssetAtPath<StageDates>(ExportPath);
		if (!Data)
		{
			// データがなければ作成
			Data = ScriptableObject.CreateInstance<StageDates>();
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
				List<TextData> textData = AssetPostImporter.CreateText(Book.GetSheetAt(2));

				// 情報の初期化
				Data.Data.Clear();

				// エクセルシートからセル単位で読み込み
				ISheet BaseSheet = Book.GetSheetAt(0);
				ISheet EventSheet = Book.GetSheetAt(1);
				for (int i = 1; i <= BaseSheet.LastRowNum; i++)
				{
					IRow BaseRow = BaseSheet.GetRow(i);

					var StageData = new StageData();
					StageData.Id = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Id);
					StageData.Name = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.NameId)).Text;
					StageData.Selectable = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Selectable) == 1;
					StageData.Help = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.NameId)).Help;
					StageData.Turns = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Turns);
					StageData.InitMembers = new List<int>();
					string[] list = AssetPostImporter.ImportString(BaseRow,(int)BaseColumn.InitMembers).Split(',');
					foreach (string item in list)
					{
						StageData.InitMembers.Add(int.Parse(item));
					}
					StageData.RandomTroopCount = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.RandomTroopCount);
					StageData.BGMId = new List<int>();
					string[] bgmList = AssetPostImporter.ImportString(BaseRow,(int)BaseColumn.BGMId).Split(',');
					foreach (string item in bgmList)
					{
						StageData.BGMId.Add(int.Parse(item));
					}
					StageData.Reborn = (AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Reborn) == 1);
					
					StageData.StageEvents = new List<StageEventData>();
					for (int j = 1; j <= EventSheet.LastRowNum; j++)
					{
						IRow EventRow = EventSheet.GetRow(j);
						var EventData = new StageEventData();
						var StageId = (int)EventRow.GetCell((int)BaseEventColumn.Id)?.SafeNumericCellValue();
						
						if (StageId == StageData.Id)
						{
							EventData.Turns = (int)EventRow.GetCell((int)BaseEventColumn.Turns)?.SafeNumericCellValue();
							EventData.Timing = (EventTiming)EventRow.GetCell((int)BaseEventColumn.Timing)?.SafeNumericCellValue();
							EventData.Type = (StageEventType)EventRow.GetCell((int)BaseEventColumn.Type)?.SafeNumericCellValue();
							EventData.Param = (int)EventRow.GetCell((int)BaseEventColumn.Param)?.SafeNumericCellValue();
							EventData.ReadFlag = (bool)(EventRow.GetCell((int)BaseEventColumn.ReadFlag)?.SafeNumericCellValue() == 1);
							
							EventData.EventKey = EventData.Turns.ToString() + EventData.Timing.ToString() + EventData.Type.ToString() + EventData.Param.ToString();

							StageData.StageEvents.Add(EventData);
						}
					}
					Data.Data.Add(StageData);
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
