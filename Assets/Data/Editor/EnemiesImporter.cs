
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

public class EnemiesImporter : AssetPostprocessor {
    enum BaseColumn
    {
		Id = 0,
		NameId,
		ImageName,
		BaseHp,
		BaseMp,
		BaseAtk,
		BaseDef,
		BaseSpd,
    }
    enum BaseTextColumn
    {

		Id = 0,
		Text,
		Help
	}
	static readonly string ExcelPath = "Assets/Data";
	static readonly string ExcelName = "Enemies.xlsx";

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

			CreateEnemyData(asset);

			AssetDatabase.SaveAssets();
			return;
		}
	}

	static void CreateEnemyData(string asset)
	{
		// 拡張子なしのファイル名を取得
		string FileName = Path.GetFileNameWithoutExtension(asset);

		// ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
		string ExportPath = $"{Path.Combine(Path.GetDirectoryName(asset), FileName)}.asset";

		EnemiesData Data = AssetDatabase.LoadAssetAtPath<EnemiesData>(ExportPath);
		if (!Data)
		{
			// データがなければ作成
			Data = ScriptableObject.CreateInstance<EnemiesData>();
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
				Data._data.Clear();

				// エクセルシートからセル単位で読み込み
				ISheet BaseSheet = Book.GetSheetAt(0);

				for (int i = 1; i <= BaseSheet.LastRowNum; i++)
				{
					IRow Baserow = BaseSheet.GetRow(i);

					var EnemyData = new EnemiesData.EnemyData();
					EnemyData.Id = (int)Baserow.GetCell((int)BaseColumn.Id)?.SafeNumericCellValue();
					EnemyData.NameId = (int)Baserow.GetCell((int)BaseColumn.NameId)?.SafeNumericCellValue();
					EnemyData.ImageName = Baserow.GetCell((int)BaseColumn.ImageName)?.SafeStringCellValue();
					
					int BaseHp = (int)Baserow.GetCell((int)BaseColumn.BaseHp)?.SafeNumericCellValue();
					int BaseMp = (int)Baserow.GetCell((int)BaseColumn.BaseMp)?.SafeNumericCellValue();
					int BaseAtk = (int)Baserow.GetCell((int)BaseColumn.BaseAtk)?.SafeNumericCellValue();
					int BaseDef = (int)Baserow.GetCell((int)BaseColumn.BaseDef)?.SafeNumericCellValue();
					int BaseSpd = (int)Baserow.GetCell((int)BaseColumn.BaseSpd)?.SafeNumericCellValue();
					EnemyData.BaseStatus = new StatusInfo();
					EnemyData.BaseStatus.SetParameter(BaseHp,BaseMp,BaseAtk,BaseDef,BaseSpd);

					Data._data.Add(EnemyData);
				}

				// 情報の初期化
				Data._textdata.Clear();

				BaseSheet = Book.GetSheetAt(1);

				for (int i = 1; i <= BaseSheet.LastRowNum; i++)
				{
					IRow Baserow = BaseSheet.GetRow(i);
					var TextData = new TextData();

					TextData.Id = (int)Baserow.GetCell((int)BaseTextColumn.Id)?.SafeNumericCellValue();
					TextData.Text = Baserow.GetCell((int)BaseTextColumn.Text)?.SafeStringCellValue();
					
					Data._textdata.Add(TextData);
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

	// 文字列を分解
	static string[] StringSplit(string str, int count)
	{
		List<string> List = new List<string>();

		int Length = (int)Math.Ceiling((double)str.Length / count);

		for (int i = 0; i < Length; i++)
		{
			int Start = count * i;

			// 始まりが文字列の長さより多かったら
			if (str.Length <= Start)
			{
				break;
			}
			// 読み取る大きさが文字列の長さより多かったら終わりを指定しない
			if (str.Length < Start + count)
			{
				List.Add(str.Substring(Start));
			}
			// 始まりの位置と終わりの位置を指定（始まりの値は含むが終わりの値は含まない）
			else
			{
				List.Add(str.Substring(Start, count));
			}
		}

		return List.ToArray();
	}
}
