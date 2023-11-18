
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

public class AnimationImporter : AssetPostprocessor {
    enum BaseColumn
    {
		Id = 0,
		AnimationPath,
		MakerEffect,
		Enable,
    }
	static readonly string ExcelName = "Animations.xlsx";

	// アセット更新があると呼ばれる
	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
		foreach (string asset in importedAssets) {

			if (AssetPostImporter.CheckOnPostprocessAllAssets(asset,ExcelName))
			{
				CreateAnimationInfo(asset);
				AssetDatabase.SaveAssets();
				return;
			}
		}
	}

	static void CreateAnimationInfo(string asset)
	{
		Debug.Log("CreateAnimationInfo");
		// 拡張子なしのファイル名を取得
		string FileName = Path.GetFileNameWithoutExtension(asset);

		// ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
		string ExportPath = $"{Path.Combine(AssetPostImporter.ExportExcelPath, FileName)}.asset";

		AnimationDates Data = AssetDatabase.LoadAssetAtPath<AnimationDates>(ExportPath);
		if (!Data)
		{
			// データがなければ作成
			Data = ScriptableObject.CreateInstance<AnimationDates>();
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

					var AnimationData = new AnimationData();
					AnimationData.Id = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.Id);
					AnimationData.AnimationPath = AssetPostImporter.ImportString(Baserow,(int)BaseColumn.AnimationPath);
					AnimationData.MakerEffect = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.MakerEffect) == 1;
					
					Data.Data.Add(AnimationData);
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

