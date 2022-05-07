
using System;
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
		BaseHp,
		BaseStr,
		BaseMag,
		BaseTec,
		BaseSpd,
		BaseLuk,
		BaseDef,
		BaseRes,
		BaseMov,
		MaxHp,
		MaxStr,
		MaxMag,
		MaxTec,
		MaxSpd,
		MaxLuk,
		MaxDef,
		MaxRes,
		MaxMov,
		BaseSword,
		BaseLance,
		BaseAxe,
		BaseBow,
		BaseKnive,
		BaseStrike,
		BaseFire,
		BaseThunder,
		BaseWind,
		BaseLight,
		BaseDark,
		BaseStave,
		MaxSword,
		MaxLance,
		MaxAxe,
		MaxBow,
		MaxKnive,
		MaxStrike,
		MaxFire,
		MaxThunder,
		MaxWind,
		MaxLight,
		MaxDark,
		MaxStave
    }
	static readonly string ExcelPath = "Assets/Data";
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
				CreateBook(asset, Mainstream, out IWorkbook Book);

				// 情報の初期化
				Data._data.Clear();

				// エクセルシートからセル単位で読み込み
				ISheet BaseSheet = Book.GetSheetAt(0);

				for (int i = 1; i <= BaseSheet.LastRowNum; i++)
				{
					IRow Baserow = BaseSheet.GetRow(i);

					var ClassData = new ClassesData.ClassData();
					ClassData.Id = (int)Baserow.GetCell((int)BaseColumn.Id)?.SafeNumericCellValue();
					ClassData.Name = Baserow.GetCell((int)BaseColumn.Name)?.SafeStringCellValue();
					
					int BaseHp = (int)Baserow.GetCell((int)BaseColumn.BaseHp)?.SafeNumericCellValue();
					int BaseStr = (int)Baserow.GetCell((int)BaseColumn.BaseStr)?.SafeNumericCellValue();
					int BaseMag = (int)Baserow.GetCell((int)BaseColumn.BaseMag)?.SafeNumericCellValue();
					int BaseTec = (int)Baserow.GetCell((int)BaseColumn.BaseTec)?.SafeNumericCellValue();
					int BaseSpd = (int)Baserow.GetCell((int)BaseColumn.BaseSpd)?.SafeNumericCellValue();
					int BaseLuk = (int)Baserow.GetCell((int)BaseColumn.BaseLuk)?.SafeNumericCellValue();
					int BaseDef = (int)Baserow.GetCell((int)BaseColumn.BaseDef)?.SafeNumericCellValue();
					int BaseRes = (int)Baserow.GetCell((int)BaseColumn.BaseRes)?.SafeNumericCellValue();
					int BaseMov = (int)Baserow.GetCell((int)BaseColumn.BaseMov)?.SafeNumericCellValue();
					ClassData.BaseStatus = new StatusInfo();
					ClassData.BaseStatus.SetParameter(BaseHp,BaseStr,BaseMag,BaseTec,BaseSpd,BaseLuk,BaseDef,BaseRes,BaseMov);

					int MaxHp = (int)Baserow.GetCell((int)BaseColumn.MaxHp)?.SafeNumericCellValue();
					int MaxStr = (int)Baserow.GetCell((int)BaseColumn.MaxStr)?.SafeNumericCellValue();
					int MaxMag = (int)Baserow.GetCell((int)BaseColumn.MaxMag)?.SafeNumericCellValue();
					int MaxTec = (int)Baserow.GetCell((int)BaseColumn.MaxTec)?.SafeNumericCellValue();
					int MaxSpd = (int)Baserow.GetCell((int)BaseColumn.MaxSpd)?.SafeNumericCellValue();
					int MaxLuk = (int)Baserow.GetCell((int)BaseColumn.MaxLuk)?.SafeNumericCellValue();
					int MaxDef = (int)Baserow.GetCell((int)BaseColumn.MaxDef)?.SafeNumericCellValue();
					int MaxRes = (int)Baserow.GetCell((int)BaseColumn.MaxRes)?.SafeNumericCellValue();
					int MaxMov = (int)Baserow.GetCell((int)BaseColumn.MaxMov)?.SafeNumericCellValue();

					ClassData.MaxStatus = new StatusInfo();
					ClassData.MaxStatus.SetParameter(MaxHp,MaxStr,MaxMag,MaxTec,MaxSpd,MaxLuk,MaxDef,MaxRes,MaxMov);

					int BaseSword = (int)Baserow.GetCell((int)BaseColumn.BaseSword)?.SafeNumericCellValue();
					int BaseLance = (int)Baserow.GetCell((int)BaseColumn.BaseLance)?.SafeNumericCellValue();
					int BaseAxe = (int)Baserow.GetCell((int)BaseColumn.BaseAxe)?.SafeNumericCellValue();
					int BaseBow = (int)Baserow.GetCell((int)BaseColumn.BaseBow)?.SafeNumericCellValue();
					int BaseKnive = (int)Baserow.GetCell((int)BaseColumn.BaseKnive)?.SafeNumericCellValue();
					int BaseStrike = (int)Baserow.GetCell((int)BaseColumn.BaseStrike)?.SafeNumericCellValue();
					int BaseFire = (int)Baserow.GetCell((int)BaseColumn.BaseFire)?.SafeNumericCellValue();
					int BaseThunder = (int)Baserow.GetCell((int)BaseColumn.BaseThunder)?.SafeNumericCellValue();
					int BaseWind = (int)Baserow.GetCell((int)BaseColumn.BaseWind)?.SafeNumericCellValue();
					int BaseLight = (int)Baserow.GetCell((int)BaseColumn.BaseLight)?.SafeNumericCellValue();
					int BaseDark = (int)Baserow.GetCell((int)BaseColumn.BaseDark)?.SafeNumericCellValue();
					int BaseStave = (int)Baserow.GetCell((int)BaseColumn.BaseStave)?.SafeNumericCellValue();
					ClassData.BaseWeaponRank = new WeaponRankInfo();
					ClassData.BaseWeaponRank.SetParameter(BaseSword,BaseLance,BaseAxe,BaseBow,BaseKnive,BaseStrike,BaseFire,BaseThunder,BaseWind,BaseLight,BaseDark,BaseStave);

					int MaxSword = (int)Baserow.GetCell((int)BaseColumn.MaxSword)?.SafeNumericCellValue();
					int MaxLance = (int)Baserow.GetCell((int)BaseColumn.MaxLance)?.SafeNumericCellValue();
					int MaxAxe = (int)Baserow.GetCell((int)BaseColumn.MaxAxe)?.SafeNumericCellValue();
					int MaxBow = (int)Baserow.GetCell((int)BaseColumn.MaxBow)?.SafeNumericCellValue();
					int MaxKnive = (int)Baserow.GetCell((int)BaseColumn.MaxKnive)?.SafeNumericCellValue();
					int MaxStrike = (int)Baserow.GetCell((int)BaseColumn.MaxStrike)?.SafeNumericCellValue();
					int MaxFire = (int)Baserow.GetCell((int)BaseColumn.MaxFire)?.SafeNumericCellValue();
					int MaxThunder = (int)Baserow.GetCell((int)BaseColumn.MaxThunder)?.SafeNumericCellValue();
					int MaxWind = (int)Baserow.GetCell((int)BaseColumn.MaxWind)?.SafeNumericCellValue();
					int MaxLight = (int)Baserow.GetCell((int)BaseColumn.MaxLight)?.SafeNumericCellValue();
					int MaxDark = (int)Baserow.GetCell((int)BaseColumn.MaxDark)?.SafeNumericCellValue();
					int MaxStave = (int)Baserow.GetCell((int)BaseColumn.MaxStave)?.SafeNumericCellValue();
					ClassData.MaxWeaponRank = new WeaponRankInfo();
					ClassData.MaxWeaponRank.SetParameter(MaxSword,MaxLance,MaxAxe,MaxBow,MaxKnive,MaxStrike,MaxFire,MaxThunder,MaxWind,MaxLight,MaxDark,MaxStave);

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
