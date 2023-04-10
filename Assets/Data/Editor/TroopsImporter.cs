
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

public class TroopsImporter : AssetPostprocessor {
    enum BaseColumn
    {
		Id = 0,
		TroopId,
		EnemyId,
		Lv,
		BossFlag,
		Line,
    }
	enum BaseGetItemColumn
    {		
		Id,
		TroopId,
		Type,
		Param1,
		Param2,
    }
	static readonly string ExcelPath = "Assets/Data";
	static readonly string ExcelName = "Troops.xlsx";

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

			CreateTroopData(asset);

			AssetDatabase.SaveAssets();
			return;
		}
	}

	static void CreateTroopData(string asset)
	{
		// 拡張子なしのファイル名を取得
		string FileName = Path.GetFileNameWithoutExtension(asset);

		// ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
		string ExportPath = $"{Path.Combine(Path.GetDirectoryName(asset), FileName)}.asset";

		TroopsData Data = AssetDatabase.LoadAssetAtPath<TroopsData>(ExportPath);
		if (!Data)
		{
			// データがなければ作成
			Data = ScriptableObject.CreateInstance<TroopsData>();
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

				// 情報の初期化
				Data._data.Clear();

				// エクセルシートからセル単位で読み込み
				ISheet BaseSheet = Book.GetSheetAt(0);

				for (int i = 1; i <= BaseSheet.LastRowNum; i++)
				{
					IRow Baserow = BaseSheet.GetRow(i);

					var TroopData = new TroopsData.TroopData();
					TroopData.Id = (int)Baserow.GetCell((int)BaseColumn.Id)?.SafeNumericCellValue();
					TroopData.TroopId = (int)Baserow.GetCell((int)BaseColumn.TroopId)?.SafeNumericCellValue();
					TroopData.EnemyId = (int)Baserow.GetCell((int)BaseColumn.EnemyId)?.SafeNumericCellValue();
					TroopData.Lv = (int)Baserow.GetCell((int)BaseColumn.Lv)?.SafeNumericCellValue();
					TroopData.BossFlag = Baserow.GetCell((int)BaseColumn.BossFlag)?.SafeNumericCellValue() == 1;
					TroopData.Line = (LineType)Baserow.GetCell((int)BaseColumn.Line)?.SafeNumericCellValue();
					TroopData.GetItemDatas = new List<GetItemData>();
					Data._data.Add(TroopData);
				}

				BaseSheet = Book.GetSheetAt(1);
				for (int i = 1; i <= BaseSheet.LastRowNum; i++)
				{
					IRow Baserow = BaseSheet.GetRow(i);
					int Id = (int)Baserow.GetCell((int)BaseGetItemColumn.Id)?.SafeNumericCellValue();
					int TroopId = (int)Baserow.GetCell((int)BaseGetItemColumn.TroopId)?.SafeNumericCellValue();
					var troopData = Data._data.Find(a => a.TroopId == TroopId && a.Line == LineType.Back);
					if (troopData != null)
					{
						var getItemData = new GetItemData();
						getItemData.Type = (GetItemType)Baserow.GetCell((int)BaseGetItemColumn.Type)?.SafeNumericCellValue();
						getItemData.Param1 = (int)Baserow.GetCell((int)BaseGetItemColumn.Param1)?.SafeNumericCellValue();
						getItemData.Param2 = (int)Baserow.GetCell((int)BaseGetItemColumn.Param2)?.SafeNumericCellValue();
						troopData.GetItemDatas.Add(getItemData);
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

}
