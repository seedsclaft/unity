using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

public class AssetPostImporter
{
	static public string ExcelPath = "Assets/Data";
	static public string ExportExcelPath = "Assets/Resources/Data";

	static public bool CheckOnPostprocessAllAssets(string asset,string ExcelName)
	{
		string ext = Path.GetExtension(asset);
		if (ext != ".xls" && ext != ".xlsx" && ext != ".xlsm") return false;

		// エクセルを開いているデータはスキップ
		string fileName = Path.GetFileName(asset);
		if (fileName.StartsWith("~$")) return false;

		// 同じパスのみ
		string filePath = Path.GetDirectoryName(asset);
		filePath = filePath.Replace("\\", "/");
		if (filePath != AssetPostImporter.ExcelPath) return false;

		// 同じファイルのみ
		if (fileName != ExcelName) return false;
		return true;
	}
    public static int ImportNumeric(IRow Baserow,int Column)
    {
        //Debug.Log(Column);
        return (int)Baserow.GetCell(Column).SafeNumericCellValue();
    }
    public static float ImportFloat(IRow Baserow,int Column)
    {
        //Debug.Log(Column);
        return (float)Baserow.GetCell(Column).SafeNumericCellValue();
    }
    public static string ImportString(IRow Baserow,int Column)
    {
        //Debug.Log(Column);
        return (string)Baserow.GetCell(Column).SafeStringCellValue();
    }
	// エクセルワークブックを作成
	public static void CreateBook(string path, Stream stream, out IWorkbook Workbook)
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
	// テキストデータを作成
	public static List<TextData> CreateText(ISheet BaseSheet)
	{
		var textData = new List<TextData>();

		for (int i = 1; i <= BaseSheet.LastRowNum; i++)
		{
			IRow Baserow = BaseSheet.GetRow(i);
			var TextData = new TextData();

			TextData.Id = AssetPostImporter.ImportNumeric(Baserow,(int)BaseTextColumn.Id);
			TextData.Text = AssetPostImporter.ImportString(Baserow,(int)BaseTextColumn.Text);
			TextData.Help = AssetPostImporter.ImportString(Baserow,(int)BaseTextColumn.Help);
			
			textData.Add(TextData);
		}

		return textData;
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

public enum BaseTextColumn
{

	Id = 0,
	Text,
	Help,
}