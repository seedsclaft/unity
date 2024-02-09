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
		Toggle,
		ToggleText1,
		ToggleText2,
		ExistAndroid,
		ExistWebGL,
    }

    enum BaseDefineColumn
    {
		Key = 0,
		Param,
    }

	static readonly string ExcelName = "System.xlsx";
    
	// アセット更新があると呼ばれる
	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
		foreach (string asset in importedAssets) {

			if (AssetPostImporter.CheckOnPostprocessAllAssets(asset,ExcelName))
			{
				CreateMenuCommandInfo(asset);
				AssetDatabase.SaveAssets();
				return;
			}
		}
	}

	static void CreateMenuCommandInfo(string asset)
	{
		// 拡張子なしのファイル名を取得
		string FileName = Path.GetFileNameWithoutExtension(asset);

		// ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
		string ExportPath = $"{Path.Combine(AssetPostImporter.ExportExcelPath, FileName)}.asset";

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
				AssetPostImporter.CreateBook(asset, Mainstream, out IWorkbook Book);
				List<TextData> textData = AssetPostImporter.CreateText(Book.GetSheetAt(6));

				// 情報の初期化
				Data.TacticsCommandData = new ();
				Data.StatusCommandData = new ();
				Data.OptionCommandData = new ();
				Data.TitleCommandData = new ();
				Data.SystemTextData = new ();
				Data.SystemTextData = textData;
				Data.InputDataList = new ();

				// エクセルシートからセル単位で読み込み
				ISheet BaseSheet = Book.GetSheetAt(0);

				for (int i = 1; i <= BaseSheet.LastRowNum; i++)
				{
					IRow BaseRow = BaseSheet.GetRow(i);

					var TitleCommandInfo = new SystemData.CommandData();
					TitleCommandInfo.Id = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Id);
					TitleCommandInfo.Key = AssetPostImporter.ImportString(BaseRow,(int)BaseColumn.Key);
					TitleCommandInfo.Name = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.NameTextId)).Text;
					TitleCommandInfo.Help = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.NameTextId)).Help;
					Data.TacticsCommandData.Add(TitleCommandInfo);
				}
				
				BaseSheet = Book.GetSheetAt(1);

				for (int i = 1; i <= BaseSheet.LastRowNum; i++)
				{
					IRow BaseRow = BaseSheet.GetRow(i);

					var TitleCommandInfo = new SystemData.CommandData();
					TitleCommandInfo.Id = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Id);
					TitleCommandInfo.Key = AssetPostImporter.ImportString(BaseRow,(int)BaseColumn.Key);
					TitleCommandInfo.Name = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.NameTextId)).Text;
					TitleCommandInfo.Help = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.NameTextId)).Help;
					Data.TitleCommandData.Add(TitleCommandInfo);
				}

				BaseSheet = Book.GetSheetAt(2);

				for (int i = 1; i <= BaseSheet.LastRowNum; i++)
				{
					IRow BaseRow = BaseSheet.GetRow(i);

					var StatusCommandInfo = new SystemData.CommandData();
					StatusCommandInfo.Id = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Id);
					StatusCommandInfo.Key = AssetPostImporter.ImportString(BaseRow,(int)BaseColumn.Key);
					StatusCommandInfo.Name = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.NameTextId)).Text;
					StatusCommandInfo.Help = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.NameTextId)).Help;
					Data.StatusCommandData.Add(StatusCommandInfo);
				}

				BaseSheet = Book.GetSheetAt(3);

				for (int i = 1; i <= BaseSheet.LastRowNum; i++)
				{
					IRow BaseRow = BaseSheet.GetRow(i);

					var OptionCommand = new SystemData.OptionCommand();
					OptionCommand.Id = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Id);
					OptionCommand.Key = AssetPostImporter.ImportString(BaseRow,(int)BaseColumn.Key);
					OptionCommand.Name = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.NameTextId)).Text;
					OptionCommand.Help = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.NameTextId)).Help;
					OptionCommand.Toggles = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Toggle) == 1;
					OptionCommand.ToggleText1 = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.ToggleText1);
					OptionCommand.ToggleText2 = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.ToggleText2);
					OptionCommand.ExistAndroid = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.ExistAndroid) == 1;
					OptionCommand.ExistWebGL = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.ExistWebGL) == 1;
					Data.OptionCommandData.Add(OptionCommand);
				}
				
				BaseSheet = Book.GetSheetAt(4);

				for (int i = 1; i <= BaseSheet.LastRowNum; i++)
				{
					IRow BaseRow = BaseSheet.GetRow(i);
					var inputData = new SystemData.InputData();
					inputData.Key = AssetPostImporter.ImportString(BaseRow,(int)BaseColumn.Id);
					inputData.KeyId = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Key);
					inputData.Name = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.NameTextId)).Text;
					
					Data.InputDataList.Add(inputData);
				}

				BaseSheet = Book.GetSheetAt(5);

				for (int i = 1; i <= BaseSheet.LastRowNum; i++)
				{
					IRow BaseRow = BaseSheet.GetRow(i);

					var KeyName = AssetPostImporter.ImportString(BaseRow,(int)BaseDefineColumn.Key);
					
					if (KeyName == "initCurrency")
					{
						Data.InitCurrency = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseDefineColumn.Param);
					}
					if (KeyName == "trainCount")
					{
						Data.TrainCount = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseDefineColumn.Param);
					}
					if (KeyName == "alchemyCount")
					{
						Data.AlchemyCount = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseDefineColumn.Param);
					}
					if (KeyName == "recoveryCount")
					{
						Data.RecoveryCount = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseDefineColumn.Param);
					}
					if (KeyName == "battleCount")
					{
						Data.BattleCount = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseDefineColumn.Param);
					}
					if (KeyName == "resourceCount")
					{
						Data.ResourceCount = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseDefineColumn.Param);
					}
					if (KeyName == "alcanaSelectCount")
					{
						Data.AlcanaSelectCount = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseDefineColumn.Param);
					}
					if (KeyName == "battleBonusValue")
					{
						Data.BattleBonusValue = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseDefineColumn.Param);
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



	// テキストデータを作成
	static List<TextData> CreateText(ISheet BaseSheet)
	{
		var textData = new List<TextData>();

		for (int i = 1; i <= BaseSheet.LastRowNum; i++)
		{
			IRow BaseRow = BaseSheet.GetRow(i);
			var TextData = new TextData();

			TextData.Id = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseTextColumn.Id);
			TextData.Text = AssetPostImporter.ImportString(BaseRow,(int)BaseTextColumn.Text).ToString();
			TextData.Help = AssetPostImporter.ImportString(BaseRow,(int)BaseTextColumn.Help).ToString();
			
			textData.Add(TextData);
		}

		return textData;
	}
}
