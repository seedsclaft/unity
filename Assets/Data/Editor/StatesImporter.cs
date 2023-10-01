
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
		OverLap,
		Removal,
		Abnormal
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
				AssetPostImporter.CreateBook(asset, Mainstream, out IWorkbook Book);
				List<TextData> textData = AssetPostImporter.CreateText(Book.GetSheetAt(1));

				// 情報の初期化
				Data._data.Clear();

				// エクセルシートからセル単位で読み込み
				ISheet BaseSheet = Book.GetSheetAt(0);

				for (int i = 1; i <= BaseSheet.LastRowNum; i++)
				{
					IRow Baserow = BaseSheet.GetRow(i);

					var StateData = new StatesData.StateData();
					StateData.Id = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.Id);
					StateData.Name = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.NameId)).Text;
					StateData.Help = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.NameId)).Help;
					StateData.IconPath = AssetPostImporter.ImportString(Baserow,(int)BaseColumn.IconIndex);
					StateData.RemovalTiming = (RemovalTiming)AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.RemovalTiming);
					StateData.OverWrite = (bool)(AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.OverRight) == 1);
					StateData.EffectPath = AssetPostImporter.ImportString(Baserow,(int)BaseColumn.EffectPath);
					StateData.EffectPosition = (EffectPositionType)AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.EffectPosition);
					StateData.OverLap = (bool)(AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.OverLap) == 1);
					StateData.Removal = (bool)(AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.Removal) == 1);
					StateData.Abnormal = (bool)(AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.Abnormal) == 1);
					
					
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
}