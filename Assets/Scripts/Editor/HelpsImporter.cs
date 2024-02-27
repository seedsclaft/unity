
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

public class HelpsImporter : AssetPostprocessor {
    enum BaseColumn
    {
		Id = 0,
		Key,
		Help,
    }
	static readonly string ExcelName = "Helps.xlsx";

	// アセット更新があると呼ばれる
	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
		foreach (string asset in importedAssets) {

			if (AssetPostImporter.CheckOnPostprocessAllAssets(asset,ExcelName))
			{
				CreateHelpInfo(asset);
				AssetDatabase.SaveAssets();
				return;
			}
		}
	}

	static void CreateHelpInfo(string asset)
	{
		// 拡張子なしのファイル名を取得
		string FileName = Path.GetFileNameWithoutExtension(asset);

		// ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
		string ExportPath = $"{Path.Combine(AssetPostImporter.ExportExcelPath, FileName)}.asset";

		HelpDates Data = AssetDatabase.LoadAssetAtPath<HelpDates>(ExportPath);
		if (!Data)
		{
			// データがなければ作成
			Data = ScriptableObject.CreateInstance<HelpDates>();
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
				Data.Data.Clear();

				// エクセルシートからセル単位で読み込み
				ISheet BaseSheet = Book.GetSheetAt(0);

				for (int i = 1; i <= BaseSheet.LastRowNum; i++)
				{
					IRow BaseRow = BaseSheet.GetRow(i);

					var HelpData = new HelpData();
					HelpData.Id = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Id);
					HelpData.Key = AssetPostImporter.ImportString(BaseRow,(int)BaseColumn.Key);
                    HelpData.Help = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Help)).Help;
		    		Data.Data.Add(HelpData);
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