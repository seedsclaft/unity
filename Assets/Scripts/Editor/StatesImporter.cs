
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
		Abnormal,
		RemoveByAttack,
		RemoveByDeath,
    }
	static readonly string ExcelName = "States.xlsx";

	// アセット更新があると呼ばれる
	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
		foreach (string asset in importedAssets) {

			if (AssetPostImporter.CheckOnPostprocessAllAssets(asset,ExcelName))
			{
				CreateStateInfo(asset);
				AssetDatabase.SaveAssets();
				return;
			}
		}
	}


	static void CreateStateInfo(string asset)
	{
		// 拡張子なしのファイル名を取得
		string FileName = Path.GetFileNameWithoutExtension(asset);

		// ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
		string ExportPath = $"{Path.Combine(AssetPostImporter.ExportExcelPath, FileName)}.asset";

		StateDates Data = AssetDatabase.LoadAssetAtPath<StateDates>(ExportPath);
		if (!Data)
		{
			// データがなければ作成
			Data = ScriptableObject.CreateInstance<StateDates>();
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

					var StateData = new StateData();
					StateData.StateType = (StateType)AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Id);
					StateData.Name = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.NameId)).Text;
					StateData.Help = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.NameId)).Help;
					StateData.IconPath = AssetPostImporter.ImportString(BaseRow,(int)BaseColumn.IconIndex);
					StateData.RemovalTiming = (RemovalTiming)AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.RemovalTiming);
					StateData.OverWrite = (bool)(AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.OverRight) == 1);
					StateData.EffectPath = AssetPostImporter.ImportString(BaseRow,(int)BaseColumn.EffectPath);
					StateData.EffectPosition = (EffectPositionType)AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.EffectPosition);
					StateData.OverLap = (bool)(AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.OverLap) == 1);
					StateData.Removal = (bool)(AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Removal) == 1);
					StateData.Abnormal = (bool)(AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Abnormal) == 1);
					StateData.RemoveByAttack = (bool)(AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.RemoveByAttack) == 1);
					StateData.RemoveByDeath = (bool)(AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.RemoveByDeath) == 1);
					
					Data.Data.Add(StateData);
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