using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

public class SeImporter : AssetPostprocessor {
    enum BaseColumn
    {
		Id = 0,
		Key,
		FileName,
		Volume,
		Pitch,
		Loop,
    }
	//static readonly string ExcelPath = "Assets/Resources/Data";
	static readonly string ExcelName = "SE.xlsx";

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
			//if (filePath != ExcelPath) { continue; }

			// 同じファイルのみ
			if (fileName != ExcelName) { continue; }

			CreateInfo(asset);

			AssetDatabase.SaveAssets();
			return;
		}
	}

	static void CreateInfo(string asset)
	{
		// 拡張子なしのファイル名を取得
		string FileName = Path.GetFileNameWithoutExtension(asset);

		// ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
		string ExportPath = $"Assets\\Resources\\Data\\MainData.asset";

		DataManager Data = AssetDatabase.LoadAssetAtPath<DataManager>(ExportPath);
		if (!Data)
		{
			// データがなければ作成
			Data = ScriptableObject.CreateInstance<DataManager>();
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

				// 情報の初期化
				Data.SE.Clear();

				// エクセルシートからセル単位で読み込み
				ISheet BaseSheet = Book.GetSheetAt(0);

				for (int i = 1; i <= BaseSheet.LastRowNum; i++)
				{
					IRow Baserow = BaseSheet.GetRow(i);

					var SE = new SEData();
					SE.Id = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.Id);
					SE.Key = AssetPostImporter.ImportString(Baserow,(int)BaseColumn.Key);
					SE.FileName = AssetPostImporter.ImportString(Baserow,(int)BaseColumn.FileName);
					SE.Volume = (float)AssetPostImporter.ImportFloat(Baserow,(int)BaseColumn.Volume);
					SE.Pitch = (float)AssetPostImporter.ImportFloat(Baserow,(int)BaseColumn.Pitch);
					Data.SE.Add(SE);
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