﻿using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

public class ClassesInfoImporter : AssetPostprocessor {
    enum BaseColumn
    {
		Id = 0,
		Name,
    }
	static readonly string ExcelPath = "Assets/Resources/Data";
	static readonly string ExcelName = "Classes.xlsx";

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

			CreateClassesData(asset);

			AssetDatabase.SaveAssets();
			return;
		}
	}

	static void CreateClassesData(string asset)
	{
		// 拡張子なしのファイル名を取得
		string FileName = Path.GetFileNameWithoutExtension(asset);

		// ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
		string ExportPath = $"{Path.Combine(Path.GetDirectoryName(asset), FileName)}.asset";

		ClassesData Data = AssetDatabase.LoadAssetAtPath<ClassesData>(ExportPath);
		if (!Data)
		{
			// データがなければ作成
			Data = ScriptableObject.CreateInstance<ClassesData>();
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
				Data._data.Clear();

				// エクセルシートからセル単位で読み込み
				ISheet BaseSheet = Book.GetSheetAt(0);

				for (int i = 1; i <= BaseSheet.LastRowNum; i++)
				{
					IRow Baserow = BaseSheet.GetRow(i);

					var ClassData = new ClassesData.ClassData();
					ClassData.Id = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.Id);
					ClassData.Name = AssetPostImporter.ImportString(Baserow,(int)BaseColumn.Name);
					
					Data._data.Add(ClassData);
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
