
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

public class RulesImporter : AssetPostprocessor {
    enum BaseColumn
    {
		Id = 0,
		NameId,
        Category
    }
	static readonly string ExcelName = "Rules.xlsx";

	// アセット更新があると呼ばれる
	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
		foreach (string asset in importedAssets) {

			if (AssetPostImporter.CheckOnPostprocessAllAssets(asset,ExcelName))
			{
				CreateRuleInfo(asset);
				AssetDatabase.SaveAssets();
				return;
			}
		}
	}

	static void CreateRuleInfo(string asset)
	{
		// 拡張子なしのファイル名を取得
		string FileName = Path.GetFileNameWithoutExtension(asset);

		// ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
		string ExportPath = $"{Path.Combine(AssetPostImporter.ExportExcelPath, FileName)}.asset";

		RuleDatas Data = AssetDatabase.LoadAssetAtPath<RuleDatas>(ExportPath);
		if (!Data)
		{
			// データがなければ作成
			Data = ScriptableObject.CreateInstance<RuleDatas>();
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

				for (int i = 1; i <= BaseSheet.LastRowNum-1; i++)
				{
					IRow Baserow = BaseSheet.GetRow(i);

					var RuleData = new RuleData();
					RuleData.Id = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.Id);
					RuleData.Name = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.NameId)).Text;
					RuleData.Help = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.NameId)).Help;
					RuleData.Category = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.Category);
					Data.Data.Add(RuleData);
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