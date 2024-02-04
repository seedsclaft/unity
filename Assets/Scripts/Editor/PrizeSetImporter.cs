
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

public class PrizeSetInfoImporter : AssetPostprocessor {
    enum BaseColumn
    {
		Id = 0,
		Type,
        Param1,
        Param2
    }
	static readonly string ExcelName = "PrizeSets.xlsx";

	// アセット更新があると呼ばれる
	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
		foreach (string asset in importedAssets) {

			if (AssetPostImporter.CheckOnPostprocessAllAssets(asset,ExcelName))
			{
				CreatePrizeSetData(asset);
				AssetDatabase.SaveAssets();
				return;
			}
		}
	}

	static void CreatePrizeSetData(string asset)
	{
		Debug.Log("CreatePrizeSetData");
		// 拡張子なしのファイル名を取得
		string FileName = Path.GetFileNameWithoutExtension(asset);

		// ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
		string ExportPath = $"{Path.Combine(AssetPostImporter.ExportExcelPath, FileName)}.asset";

		PrizeSetDates Data = AssetDatabase.LoadAssetAtPath<PrizeSetDates>(ExportPath);
		if (!Data)
		{
			// データがなければ作成
			Data = ScriptableObject.CreateInstance<PrizeSetDates>();
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
				Data.Data.Clear();

				// エクセルシートからセル単位で読み込み
				ISheet BaseSheet = Book.GetSheetAt(0);
				for (int i = 1; i <= BaseSheet.LastRowNum; i++)
				{
					IRow Baserow = BaseSheet.GetRow(i);

					var PrizeSet = new PrizeSetData();
					PrizeSet.Id = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.Id);
					var getItemData = new GetItemData();
                    getItemData.Type = (GetItemType)AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.Type);
                    getItemData.Param1 = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.Param1);
                    getItemData.Param2 = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.Param2);
					PrizeSet.GetItem = getItemData;
                    Data.Data.Add(PrizeSet);
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