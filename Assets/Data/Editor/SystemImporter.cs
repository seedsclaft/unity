using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text.RegularExpressions;

public class SystemImporter : AssetPostprocessor
{
    enum BaseColumn
    {
		Id = 0,
		Key,
		NameTextId,
    }

    enum BaseDefineColumn
    {
		Key = 0,
		Param,
    }


	static readonly string ExcelPath = "Assets/Data";
	static readonly string ExcelName = "System.xlsx";
    
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
			Debug.Log(ExcelName);

			CreateMenuCommandInfo(asset);

			AssetDatabase.SaveAssets();
			return;
		}
    }

	static void CreateMenuCommandInfo(string asset)
	{
		// 拡張子なしのファイル名を取得
		string FileName = Path.GetFileNameWithoutExtension(asset);

		// ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
		string ExportPath = $"{Path.Combine(Path.GetDirectoryName(asset), FileName)}.asset";

		// ExportPath内のMenuCommandInfoListを検索
		SystemData Data = AssetDatabase.LoadAssetAtPath<SystemData>(ExportPath);
		if (!Data)
		{
			// データがなければ作成
			Data = ScriptableObject.CreateInstance<SystemData>();
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
				Data.MenuCommandDataList = new List<SystemData.MenuCommandData>();
				Data.TitleCommandData = new List<SystemData.MenuCommandData>();
				Data.InitActors = new List<int>();

				// エクセルシートからセル単位で読み込み
				ISheet BaseSheet = Book.GetSheetAt(0);

				for (int i = 1; i <= BaseSheet.LastRowNum; i++)
				{
					IRow Baserow = BaseSheet.GetRow(i);

					var MenuCommandInfo = new SystemData.MenuCommandData();
					MenuCommandInfo.Id = (int)Baserow.GetCell((int)BaseColumn.Id)?.SafeNumericCellValue();
					MenuCommandInfo.Key = Baserow.GetCell((int)BaseColumn.Key)?.SafeStringCellValue();
					MenuCommandInfo.NameTextId = (int)Baserow.GetCell((int)BaseColumn.NameTextId)?.SafeNumericCellValue();
					Data.MenuCommandDataList.Add(MenuCommandInfo);
				}
				
				BaseSheet = Book.GetSheetAt(1);

				for (int i = 1; i <= BaseSheet.LastRowNum; i++)
				{
					IRow Baserow = BaseSheet.GetRow(i);

					var TitleCommandInfo = new SystemData.MenuCommandData();
					TitleCommandInfo.Id = (int)Baserow.GetCell((int)BaseColumn.Id)?.SafeNumericCellValue();
					TitleCommandInfo.Key = Baserow.GetCell((int)BaseColumn.Key)?.SafeStringCellValue();
					TitleCommandInfo.NameTextId = (int)Baserow.GetCell((int)BaseColumn.NameTextId)?.SafeNumericCellValue();
					Data.TitleCommandData.Add(TitleCommandInfo);
				}

				BaseSheet = Book.GetSheetAt(2);

				for (int i = 1; i <= BaseSheet.LastRowNum; i++)
				{
					IRow Baserow = BaseSheet.GetRow(i);

					var KeyName = Baserow.GetCell((int)BaseDefineColumn.Key)?.SafeStringCellValue();
					if (KeyName == "initActors")
					{
						string[] list = Baserow.GetCell((int)BaseDefineColumn.Param)?.SafeStringCellValue().Split(',');
						foreach (string item in list)
						{
							Data.InitActors.Add(int.Parse(item));
						}
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
