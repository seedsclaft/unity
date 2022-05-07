
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

public class ActorsInfoImporter : AssetPostprocessor {
    enum BaseColumn
    {
		Id = 0,
		Name,
		ClassId,
		ImagePath,
		InitLv,
		InitHp,
		InitStr,
		InitMag,
		InitTec,
		InitSpd,
		InitLuk,
		InitDef,
		InitRes,
		InitMov,
		GrowthHp,
		GrowthStr,
		GrowthMag,
		GrowthTec,
		GrowthSpd,
		GrowthLuk,
		GrowthDef,
		GrowthRes,
		GrowthMov,
    }
	static readonly string ExcelPath = "Assets/Data";
	static readonly string ExcelName = "Actors.xlsx";

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

			CreateActorInfo(asset);

			AssetDatabase.SaveAssets();
			return;
		}
	}

	static void CreateActorInfo(string asset)
	{
		// 拡張子なしのファイル名を取得
		string FileName = Path.GetFileNameWithoutExtension(asset);

		// ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
		string ExportPath = $"{Path.Combine(Path.GetDirectoryName(asset), FileName)}.asset";

		// ExportPath内のActorsInfoListを検索
		ActorsData Data = AssetDatabase.LoadAssetAtPath<ActorsData>(ExportPath);
		if (!Data)
		{
			// データがなければ作成
			Data = ScriptableObject.CreateInstance<ActorsData>();
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

					var ActorInfo = new ActorsData.ActorData();
					ActorInfo.Id = (int)Baserow.GetCell((int)BaseColumn.Id)?.SafeNumericCellValue();
					ActorInfo.Name = Baserow.GetCell((int)BaseColumn.Name)?.SafeStringCellValue();
					ActorInfo.ClassId = (int)Baserow.GetCell((int)BaseColumn.ClassId)?.SafeNumericCellValue();
					ActorInfo.ImagePath = Baserow.GetCell((int)BaseColumn.ImagePath)?.SafeStringCellValue();
					ActorInfo.InitLv = (int)Baserow.GetCell((int)BaseColumn.InitLv)?.SafeNumericCellValue();

					int InitHp = (int)Baserow.GetCell((int)BaseColumn.InitHp)?.SafeNumericCellValue();
					int InitStr = (int)Baserow.GetCell((int)BaseColumn.InitStr)?.SafeNumericCellValue();
					int InitMag = (int)Baserow.GetCell((int)BaseColumn.InitMag)?.SafeNumericCellValue();
					int InitTec = (int)Baserow.GetCell((int)BaseColumn.InitTec)?.SafeNumericCellValue();
					int InitSpd = (int)Baserow.GetCell((int)BaseColumn.InitSpd)?.SafeNumericCellValue();
					int InitLuk = (int)Baserow.GetCell((int)BaseColumn.InitLuk)?.SafeNumericCellValue();
					int InitDef = (int)Baserow.GetCell((int)BaseColumn.InitDef)?.SafeNumericCellValue();
					int InitRes = (int)Baserow.GetCell((int)BaseColumn.InitRes)?.SafeNumericCellValue();
					int InitMov = (int)Baserow.GetCell((int)BaseColumn.InitMov)?.SafeNumericCellValue();
					ActorInfo.InitStatus = new StatusInfo();
					ActorInfo.InitStatus.SetParameter(InitHp,InitStr,InitMag,InitTec,InitSpd,InitLuk,InitDef,InitRes,InitMov);

					int GrowthHp = (int)Baserow.GetCell((int)BaseColumn.GrowthHp)?.SafeNumericCellValue();
					int GrowthStr = (int)Baserow.GetCell((int)BaseColumn.GrowthStr)?.SafeNumericCellValue();
					int GrowthMag = (int)Baserow.GetCell((int)BaseColumn.GrowthMag)?.SafeNumericCellValue();
					int GrowthTec = (int)Baserow.GetCell((int)BaseColumn.GrowthTec)?.SafeNumericCellValue();
					int GrowthSpd = (int)Baserow.GetCell((int)BaseColumn.GrowthSpd)?.SafeNumericCellValue();
					int GrowthLuk = (int)Baserow.GetCell((int)BaseColumn.GrowthLuk)?.SafeNumericCellValue();
					int GrowthDef = (int)Baserow.GetCell((int)BaseColumn.GrowthDef)?.SafeNumericCellValue();
					int GrowthRes = (int)Baserow.GetCell((int)BaseColumn.GrowthRes)?.SafeNumericCellValue();
					int GrowthMov = (int)Baserow.GetCell((int)BaseColumn.GrowthMov)?.SafeNumericCellValue();

					ActorInfo.GrowthRateStatus = new StatusInfo();
					ActorInfo.GrowthRateStatus.SetParameter(GrowthHp,GrowthStr,GrowthMag,GrowthTec,GrowthSpd,GrowthLuk,GrowthDef,GrowthRes,GrowthMov);

					Data._data.Add(ActorInfo);
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
